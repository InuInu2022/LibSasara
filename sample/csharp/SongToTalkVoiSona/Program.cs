using System.Net.Http;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CevioCasts;
using LibSasara;
using LibSasara.Model;
using LibSasara.Model.FullContextLabel;
using LibSasara.VoiSona;
using LibSasara.VoiSona.Model.Talk;
using SharpOpenJTalk.Lang;
using WanaKanaNet;

ConsoleApp.Run<SongToTalk>(args);

public partial class SongToTalk: ConsoleAppBase
{
	private OpenJTalkAPI? _jtalk;
	private Cast? _defs;

	[RootCommand]
	public async ValueTask<int> ExportAsync(
		[Option("s", "path to song ccs file")]
		string pathToSongCcs,
		[Option("e", "path to a export tstprj file")]
		string pathToPrj,
		[Option("er", "emotion rate array")]
		double[] emotions,
		[Option("c", "singing cast name")]
		string? castName = null,
		[Option("split", "split note by threthold msec.")]
		bool splitNoteByThrethold = true,
		[Option("th", "threthold time (MessageProcessingHandler.) for note split")]
		double splitThrethold = 250,
		string? pathToWav = "",
		string? pathToLab = ""
	){
		if(!File.Exists(pathToSongCcs)){
			Console.Error.WriteLine($"file:{pathToSongCcs} is not found!");
			return 1;
		}

		//(あれば)wav/labを読み込む
		var hasWav = pathToWav is not null
			&& File.Exists(pathToWav);
		var hasLab = pathToLab is not null
			&& File.Exists(pathToLab);

		//ccsを解析
		var processed = await ProcessCcsAsync(pathToSongCcs)
			.ConfigureAwait(false);

		//解析を元にtstprj作成
		await GenerateFileAsync(
			processed,
			pathToPrj,
			castName,
			(splitNoteByThrethold, splitThrethold),
			emotions
		)
			.ConfigureAwait(false);

		return default;
	}

	/// <summary>
	/// songのccs/ccstデータを解析する
	/// </summary>
	/// <remarks>
	/// 最初のSongトラックのみ
	/// オプションで再現度合いを切り替え
	///  1. 楽譜データのみ
	///  2. 楽譜＋調声データ
	///  3. 楽譜＋フルPITCH(wav)＋フルTMG(lab)
	/// </remarks>
	/// <param name="path"></param>
	/// <returns></returns>
	private async ValueTask<SongData> ProcessCcsAsync(string path){
		var ccs = await SasaraCcs.LoadAsync(path)
			.ConfigureAwait(false);
		var trackset = ccs
			.GetTrackSets<SongUnit>()
			.FirstOrDefault();
		var song = trackset?
			.Units
			.FirstOrDefault();
		if(trackset is null || song is null){
			Console.Error.WriteLine($"Error!: ソングデータがありません: { path }");
			return new SongData();
		}

		//中間データに解析・変換
		var songData = new SongData()
		{
			//トラック名
			SongTrackName = trackset.Name,
			//楽譜のテンポ・ビート
			TempoList = song.Tempos,
			BeatList = song.Beat,
			//1note=1uttranceは重いはず
			//なので休符で区切ったphrase単位に
			PhraseList = SplitByPhrase(song, 0),

			//TODO:tmgやf0を渡す
		};

		return songData;
	}

	/// <summary>
	/// ノートをフレーズ単位で分割
	/// </summary>
	/// <param name="song"></param>
	/// <param name="threthold">同じフレーズとみなすしきい値（tick; 960=1/4）</param>
	/// <returns></returns>
	private static List<List<Note>> SplitByPhrase(
		SongUnit song,
		int threthold = 0
	)
	{
		//返すよう
		List<List<Note>> list = new();

		//念の為ソート
		var notes = song
			.Notes
			.OrderBy(n => n.Clock);

		var phrase = new List<Note>();
		foreach (var note in notes)
		{
			if(phrase.Count != 0){
				var last = phrase.Last();
				if (phrase.Count > 0 &&
					//しきい値以下は同じフレーズとみなす
					Math.Abs(note.Clock - (last.Clock + last.Duration)) > threthold)
				{
					list.Add(phrase);
					phrase = Enumerable
						.Empty<Note>()
						.ToList();
				}
			}
			phrase.Add(note);
		}
		list.Add(phrase);

		return list;
	}

	private async ValueTask GenerateFileAsync(
		SongData processed,
		string exportPath,
		string? castName = null,
		(bool isSplit, double threthold)? splitNote = null,
		double[]? emotions = null
	)
	{
		var path = Path.Combine(
			System.AppDomain.CurrentDomain.BaseDirectory,
			"file/template.tstprj"
		);
		var TemplateTalk = await LibVoiSona
			.LoadAsync<TstPrj>(path)
			.ConfigureAwait(false);
		await InitOpenJTalk()
			.ConfigureAwait(false);

		double[]? rates = null;
		if(castName is not null && emotions is null){
			//感情数を調べる
			var cast = await GetCastDefAsync(castName)
				.ConfigureAwait(false);
			rates = cast
				.Emotions
				.Select(e => 0.00)
				.ToArray();
			//とりあえず最初の感情だけMAX
			rates[0] = 1.00;
		}
		if(emotions is not null){
			//感情比率設定可能に
			if(emotions.Length == 0){
				Console.Error.WriteLine($"emotion {emotions} is length 0.");
			}
			rates = emotions;
		}
		var us = processed
			.PhraseList?
			//.AsParallel()
			.Select(ToUtterance(processed, rates, splitNote))
			.ToImmutableList()
			;
		if (us is null)
		{
			Console.Error.WriteLine("解析に失敗しました。。。");
			return;
		}

		var tstprj = TemplateTalk
			.ReplaceAllUtterancesAsPrj(us);
		if(castName is not null){
			var voice = await GetVoiceByCastNameAsync(castName)
				.ConfigureAwait(false);
			tstprj = tstprj
				.ReplaceVoiceAsPrj(voice);
		}
		await LibVoiSona
			.SaveAsync(exportPath, tstprj.Data.ToArray())
			.ConfigureAwait(false);
	}

	private async ValueTask<Voice> GetVoiceByCastNameAsync(string castName)
	{
		var cast = await GetCastDefAsync(castName)
			.ConfigureAwait(false);

		return new Voice(
			Array.Find(cast.Names, n => n.Lang == Lang.English)?.Display ?? "error",
			cast.Cname,
			cast.Versions.Last()
		);
	}

	private async Task<Cast> GetCastDefAsync(string castName)
	{
		if (_defs is not null) return _defs;

		var path = Path.Combine(
			AppDomain.CurrentDomain.BaseDirectory,
			"lib/data.json"
		);
		var jsonString = await File
			.ReadAllTextAsync(path)
			.ConfigureAwait(false);
		var defs = Definitions.FromJson(jsonString);
		return defs.Casts
			.Where(c => c.Product == Product.VoiSona
			&& c.Category == CevioCasts.Category.TextVocal)
			.FirstOrDefault(c => c.Names.Any(n => n.Display == castName))
			?? throw new ArgumentException($"cast name {castName} is not found in cast data. please check https://github.com/InuInu2022/cevio-casts/ ");
	}

	private async ValueTask InitOpenJTalk()
	{
		var path = Path.Combine(
			System.AppDomain.CurrentDomain.BaseDirectory,
			"lib/open_jtalk_dic_utf_8-1.11/");
		var result = await Task
			.Run(()=> {
				_jtalk = new OpenJTalkAPI();
				return _jtalk.Initialize(path);
			})
			.ConfigureAwait(false);
	}

	private Func<List<Note>, Utterance>
	ToUtterance(
		SongData data,
		double[]? emotionRates = null,
		(bool isSplit, double threthold)? noteSplit = null
	)
	{
		return p =>
		{
			//フレーズをセリフ化
			var text = GetPhraseText(p);

			//分割
			if (noteSplit?.isSplit is true){
				p = SplitNoteIfSetOption(data, noteSplit, p);
			}

			var fcLabel = GetFullContext(p);

			//フレーズの音素
			var phoneme = GetPhonemeLabel(fcLabel, GetPhonemeMode.Note);

			//読み
			var pronounce = GetPronounce(phoneme);
			//アクセントの高低
			//TODO: ノートの高低に合わせる
			//とりあえず数だけ合わせる
			var accent = MakeAccText(fcLabel);
			//調整後LEN
			//TODO:ccsやlabから割当
			var timing = "";
			//PIT
			//TODO:楽譜データだけならnote高さから計算
			//TODO:ccsやwavがあるなら解析して割当
			var pitch = GetPitches(p, data);

			var s = string.Concat(data.TempoList!.Select(v => $"{v.Key}, {v.Value}"));

			var nu = new Utterance(
				text: text,
				//tsmlを無理やり生成
				tsml: GetTsml(text, pronounce, phoneme, accent),
				//開始時刻
				start: GetStartTimeString(data, p),
				//書き出しファイル名、とりあえずセリフ
				export_name: $"{text}"
			)
			{
				//感情比率
				//感情数に合わせる
				RawFrameStyle = emotionRates is null
					? "0:1:1.000:0.000:0.000:0.000:0.000"
					: $"0:1:{string.Join(":", emotionRates)}",
				//調整前LEN
				PhonemeOriginalDuration = GetSplittedTiming(p, data),
			};
			if (!string.IsNullOrEmpty(timing))
			{
				nu.PhonemeDuration = nu.PhonemeOriginalDuration; //timing;
			}
			if (!string.IsNullOrEmpty(pitch))
			{
				nu.RawFrameLogF0 = pitch;
			}
			Console.WriteLine($"u[{text}], start:{nu.Start}");
			return nu;
		};
	}

	private List<Note> SplitNoteIfSetOption(
		SongData data,
		(bool isSplit, double threthold)? noteSplit,
		List<Note> p)
	{
		//「ー」対応
		for (var i = 1; i < p.Count; i++)
		{
			var lyric = p[i].Lyric;
			if (lyric is null) continue;
			if (!lyric.Contains('ー', StringComparison.InvariantCulture))
			{
				continue;
			}
			//「ー」の時は前のノートの母音歌詞に置換
			var prev = p[i - 1];
			var ph = GetPhonemeLabel(GetFullContext(new List<Note> { prev }), GetPhonemeMode.Note).Split("|");
			var last = ph.Last() ?? "a";
			p[i].Lyric = lyric
				.Replace(
				"ー",
				GetPronounce(last),
				StringComparison.InvariantCulture);
		}

		return p.ConvertAll(n =>
		{
			var dur = SasaraUtil
				.ClockToTimeSpan(
					n.Duration,
					data.TempoList ?? new() { { 0, 120 } })
				.TotalMilliseconds;
			var th = noteSplit?.threthold ?? 100000;
			th = th < 100 ? 100 : th;

			if (dur < th) return n;

			var spCount = (int)Math.Floor(dur / th) + 1;

			var ph = GetPhonemeLabel(GetFullContext(new List<Note> { n }), GetPhonemeMode.Note);
			var sph = ph.Split('|');
			var add = spCount - sph.Length;

			if (add <= 0) return n;

			sph = sph.Last().Split(',').Last() switch
			{
				//ん
				"N" => sph
					.Concat(
						Enumerable
						.Repeat("u", add > 0 ? add - 1 : 0)
						.Append("N"))
					.ToArray(),
				//無効
				"xx" => sph,
				//それ以外（母音）
				string s => sph
					.Concat(Enumerable.Repeat(s, add))
					.ToArray()
			};
			n.Phonetic = string
				.Join(",", sph);
			n.Lyric = GetPronounce(string.Join("|", sph));
			return n;
		});
	}

	private static WanaKanaOptions kanaOption = new()
	{
		CustomKanaMapping = new Dictionary<string, string>()
			{
				{"cl","ッ"},
				{"di","ディ"}
			}
	};
	private static string GetPronounce(string phonemes)
	{
		var sb = new StringBuilder(phonemes);
		sb.Replace("|", " ");
		sb.Replace(",", "");
		//読みを変えたフレーズ
		var yomi = WanaKana.ToKatakana(sb.ToString(), kanaOption);
		return yomi.Replace(" ", "", StringComparison.InvariantCulture);
	}

	private string GetPitches(
		List<Note> notes,
		SongData data)
	{
		List<(TimeSpan start, TimeSpan end, double logF0, int counts)> d = notes
			.ConvertAll(n =>
			(
				start: SasaraUtil
					.ClockToTimeSpan(
						data.TempoList ?? new() { { 0, 120 } },
						n.Clock
					),
				end: SasaraUtil
					.ClockToTimeSpan(
						data.TempoList ?? new() { { 0, 120 } },
						n.Clock + n.Duration
					),
				logF0: Math.Log(SasaraUtil
					.OctaveStepToFreq(n.PitchOctave, n.PitchStep)),
				counts: CountPhonemes(n)
			))
			;

		var pitches = Enumerable.Empty<(double ph, double logF0)>().ToList();
		var offset = d[0].start.TotalMilliseconds;
		var total = 1;	//冒頭sil分offset
		foreach (var (start, end, logF0, counts) in d)
		{
			//TODO:時間を見て分割数を決める？
			//一旦固定分割数で
			const int split = 20;
			var length = split * counts;
			const double add = 1.0 / split;
			for (var i = 0; i <= length; i++)
			{
				var t = total + i * add;
				pitches.Add((t, logF0));
			}

			total += counts;
		}

		var sb = new StringBuilder(100000);
		for (int i = 0; i < pitches.Count; i++)
		{
			var sf = pitches[i].ph
				.ToString("F2", CultureInfo.InvariantCulture);
			var logF0 = pitches[i].logF0;
			sb.Append(
				CultureInfo.InvariantCulture,
				$"{sf}:{logF0}");
			if(i<pitches.Count-1)sb.Append(',');
		}
		return sb.ToString();
	}

	private static string MakeAccText(FullContextLab fcLabel)
	{
		var mCount = fcLabel
			.Lines
			.Cast<FCLabLineJa>()
			.First()?
			.UtteranceInfo?
			.Mora
			?? 0
			;
		var ac = Enumerable
			.Range(0, mCount > 1 ? mCount - 1 : 0)
			.Select(s => "h")
			.ToArray();
		var accText = "l" + string.Concat(ac);
		return accText;
	}

	private string GetPhonemeLabel(
		FullContextLab fcLabel,
		GetPhonemeMode mode
	)
	{
		var moras = fcLabel.Lines
			.Cast<FCLabLineJa>();
		var splited = FullContextLabUtil
			.SplitByMora(moras)
			.Select(s => s
				.Select(s2 => s2.Phoneme)
				.Where(s2 => s2 != "sil"))
			.Select(s => string.Join(",", s))
			.Where(s => !string.IsNullOrEmpty(s))
			;

		return string.Join("|", splited);
	}

	private FullContextLab GetFullContext(IEnumerable<Note> notes)
	{
		_jtalk ??= new OpenJTalkAPI();

		var lyrics = GetPhraseText(notes.ToList());
		var text = _jtalk.GetLabels(lyrics);

		if (text is null)
		{
			return new FullContextLab("");
		}

		return new FullContextLab(string.Join("\n", text));
	}

	private IEnumerable<string> ConvertSimpleLabel(IEnumerable<string> fullLabel)
	{
		//pL-pC+pR
		return fullLabel
			.AsParallel().AsSequential()
			.Select(s => s.Split("/")[0])
			.Select(s => FullContextLabelRegex().Match(s).Groups[1].Value)
			;
	}

	/// <summary>
	/// 音素数で等分分割した時間を求める
	/// </summary>
	/// <param name="p"></param>
	/// <returns></returns>
	private string GetSplittedTiming(
		List<Note> p,
		SongData song
	)
	{
		var s = string
			.Join(",", p.Select(n =>
			{
				//音素数を数える
				//OpenJTalkで正確に数える
				int count = CountPhonemes(n);
				/*
				string str = n.Lyric ?? "";
				for (int i = 0; i < str.Length; i++)
				{
					char c = str[i];
					count += IsSinglePhoneme(c) ? 1 : 2;
				}
				count = count == 0 ? 1 : count;
				*/

				//ノートあたりの長さを音素数で等分
				var start = SasaraUtil.ClockToTimeSpan(
					song.TempoList ?? new(){ { 0, 120 } },
					n.Clock
				).TotalMilliseconds;
				var end = SasaraUtil.ClockToTimeSpan(
					song.TempoList ?? new(){ { 0, 120 } },
					n.Clock + n.Duration
				).TotalMilliseconds;
				var sub = (decimal)(end - start) / count;
				//var sub = (decimal)dur.Milliseconds / count;
				var len = Enumerable
					.Range(0, count)
					.Select(_ => sub / 1000m)
					.Select(v => v.ToString("N2", CultureInfo.InvariantCulture));
				return string.Join(",", len);
			})
			)
			;
		return $"0.005,{s},0.125";
	}

	private int CountPhonemes(Note n)
	{
		//ノート歌詞が「ー」の時はOpenJTalkでエラーになるので解析しない
		if(n.Lyric == "ー"){
			//母音音素一つになるので1
			return 1;
		}

		var fcLabel = GetFullContext(new List<Note> { n });

		return fcLabel
			.Lines
			.Cast<FCLabLineJa>()
			.Select(s => s.Phoneme)
			//前後sil除外
			.Count(s => s != "sil");
	}

	/// <summary>
	/// ソング用の特殊ラベルを消してフレーズのセリフを得る
	/// </summary>
	/// <param name="p"></param>
	/// <remarks>
	/// - 「’」（全角アポストロフィ）はトークにもあるが意味が異なるので除外
	/// - 「※$＄」（ファルセット）指定はALPで擬似的に再現できるが将来TODO
	/// </remarks>
	/// <returns></returns>
	private static string GetPhraseText(List<Note> p)
	{
		var concated = string
			.Concat(p.Select(n => n.Lyric));

		//TODO: ユーザー辞書対応
		concated = concated
			.Replace(
			"クヮ", "クワ", StringComparison.InvariantCulture);

		if(string.IsNullOrEmpty(concated)){
			concated = "ラ";
		}

		return SpecialLabelRegex()
			.Replace(concated, string.Empty);
	}

	[GeneratedRegex("[’※$＄@＠%％^＾_＿=＝]")]
	private static partial Regex SpecialLabelRegex();

	private static string GetTsml(
		string text,
		string pronounce,
		string phoneme,
		string accent)
	{
		var bytes = Encoding.UTF8.GetByteCount(pronounce);
		return $"""<acoustic_phrase><word begin_byte_index="0" chain="0" end_byte_index="{bytes}" hl="{accent}" original="{text}" phoneme="{phoneme}" pos="感動詞" pronunciation="{pronounce}">{text}</word></acoustic_phrase>""";
	}

	//	とりあえずnoteから算出
	//	本当は最初の子音分、前にはみ出したほうがいい
	private static string GetStartTimeString(SongData data, List<Note> p)
	{
		//TODO: fix ClockToTimeSpan
		var time = SasaraUtil
			.ClockToTimeSpan(
				data.TempoList ?? new(){{0,120}},
				p[0].Clock
			);
		var seconds = (decimal)time.TotalMilliseconds / 1000.0m;
		Console.WriteLine($"+ clock:{p[0].Clock}, time:{time.TotalMilliseconds}, seconds:{seconds}");
		return seconds
			.ToString("N2", CultureInfo.InvariantCulture);
	}

	[GeneratedRegex("-([a-zAIUEON]+)+")]
	private static partial Regex FullContextLabelRegex();
	[GeneratedRegex(@"\|,")]
	private static partial Regex GetPhonemeRegex();
}

/// <summary>
/// トーク変換用中間ソングデータ
/// </summary>
internal record SongData{
	public string? SongTrackName { get; set; }

	//フレーズ単位で分割されたNoteのリスト
	//フレーズ単位でセリフ化する
	public IEnumerable<List<Note>>? PhraseList { get; set; }

	//ピッチ調声データ(LogF0)のリスト
	public IEnumerable<List<decimal>>? PitchList { get; set; }

	//タイミング調声データのリスト
	//ccsに記録されたものであれば細かい中間タイミングデータが取れる
	//なにもない時はでっち上げ
	//labファイルがある時は子音と母音だけなので中間タイミングデータをでっち上げ
	//TODO:public IEnumerable<List<>>? TimingList { get; set; }

	public SortedDictionary<int, decimal>? TempoList { get; set; }
	public SortedDictionary<int, (int Beats, int BeatType)>? BeatList { get; set; }

	//TODO:将来的に対応するパラメータ
	//g Alpha
	//g PitchShift => Pitch?
	//g PitchTune => Into.
	//g Husky
	//Volume => VOL
	//Alpha => ALP
}

internal enum GetPhonemeMode
{
	Phrase = 0,
	Note = 1
}