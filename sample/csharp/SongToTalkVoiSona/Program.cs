using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
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

[SuppressMessage("Usage", "CA1050")]
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
		[Option("co", "offset seconds for consonant")]
		decimal consonantOffset = 0.05m,
		string? pathToWav = "",
		string? pathToLab = ""
	){
		if(!File.Exists(pathToSongCcs)){
			await Console.Error.WriteLineAsync($"file:{pathToSongCcs} is not found!")
				.ConfigureAwait(false);
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

		//あればlabファイル
		if(hasLab){
			processed.Label = await SasaraLabel
				.LoadAsync(pathToLab!)
				.ConfigureAwait(false);
		}

		//解析を元にtstprj作成
		await GenerateFileAsync(
			processed,
			pathToPrj,
			castName,
			(splitNoteByThrethold, splitThrethold),
			emotions,
			consonantOffset
		)
			.ConfigureAwait(false);

		_jtalk?.Dispose();
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
	private static async ValueTask<SongData> ProcessCcsAsync(string path)
	{
		var ccs = await SasaraCcs.LoadAsync(path)
			.ConfigureAwait(false);
		var trackset = ccs
			.GetTrackSets<SongUnit>()
			.FirstOrDefault();
		var song = trackset?
			.Units
			.FirstOrDefault();
		if(trackset is null || song is null){
			await Console.Error.WriteLineAsync($"Error!: ソングデータがありません: { path }")
				.ConfigureAwait(false);
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
		List<List<Note>> list = [];

		//念の為ソート
		var notes = song
			.Notes
			.OrderBy(n => n.Clock);

		var phrase = Enumerable.Empty<Note>().ToList();
		foreach (var note in notes)
		{
			if(phrase.Count != 0){
				var last = phrase[^1];
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
		double[]? emotions = null,
		decimal consonantOffset = 0.0m
	)
	{
		var path = Path.Combine(
			System.AppDomain.CurrentDomain.BaseDirectory,
			"file/template.tstprj"
		);
		var TemplateTalk = await LibVoiSona
			.LoadAsync<TstPrj>(path)
			.ConfigureAwait(false);
		await InitOpenJTalkAsync()
			.ConfigureAwait(false);
		var rates = await CulcEmoRatesAsync(castName, emotions)
			.ConfigureAwait(false);

		processed.TempoList ??= new() { { 0, 120 } };
		var sw = new Stopwatch();
		sw.Start();

		var us = processed.PhraseList?
			.AsParallel().AsOrdered()
			.Select(ToUtterance(processed, rates, splitNote, consonantOffset))
			.ToImmutableList()
			;

		if (us is null)
		{
			await Console.Error.WriteLineAsync("解析に失敗しました。。。")
				.ConfigureAwait(false);
			return;
		}

		var tstprj = TemplateTalk
			.ReplaceAllUtterancesAsPrj(us);

		sw.Stop();
		Debug.WriteLine($"★processed: {sw.ElapsedMilliseconds} msec.");

		if (castName is not null)
		{
			var voice = await GetVoiceByCastNameAsync(castName)
				.ConfigureAwait(false);
			tstprj = tstprj
				.ReplaceVoiceAsPrj(voice);
		}

		await LibVoiSona
			.SaveAsync(exportPath, tstprj.Data.ToArray())
			.ConfigureAwait(false);
	}

	private async Task<double[]?> CulcEmoRatesAsync(string? castName, double[]? emotions)
	{
		double[]? rates = null;
		if (castName is not null && emotions is null)
		{
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
		if (emotions is not null)
		{
			//感情比率設定可能に
			if (emotions.Length == 0)
			{
				await Console.Error.WriteLineAsync($"emotion {emotions} is length 0.")
					.ConfigureAwait(false);
			}
			rates = emotions;
		}

		return rates;
	}

	private async ValueTask<Voice> GetVoiceByCastNameAsync(string castName)
	{
		var cast = await GetCastDefAsync(castName)
			.ConfigureAwait(false);

		return new Voice(
			Array.Find(cast.Names, n => n.Lang == Lang.English)?.Display ?? "error",
			cast.Cname,
			cast.Versions[^1]);
	}

	private async ValueTask<Cast> GetCastDefAsync(string castName)
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
		if(defs is null)
		{
			await Console.Error.WriteLineAsync($"invalid cast definitions data: {path}")
				.ConfigureAwait(false);
			ThrowInvalidException(path);
		}
		_defs = Array.Find(defs.Casts,
				c => c.Product == Product.VoiSona
				&& c.Category == CevioCasts.Category.TextVocal
				&& c.Names.Any(n => string.Equals(n.Display, castName, StringComparison.OrdinalIgnoreCase))
			)
			?? throw new ArgumentException(
				$"cast name {castName} is not found in cast data. please check https://github.com/InuInu2022/cevio-casts/ ",
				nameof(castName));
		return _defs;

		// use polyfill for netstandard 2.0
		[DoesNotReturn]
		static void ThrowInvalidException(string path)
		{
			throw new InvalidDataException($"invalid cast definitions data: {path}");
		}
	}

	private async ValueTask InitOpenJTalkAsync()
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
		(bool isSplit, double threthold)? noteSplit = null,
		decimal consonantOffset = 0.0m
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
			//楽譜データだけならnote高さから計算
			//TODO:ccsやwavがあるなら解析して割当
			var pitch = GetPitches(p, data);

			//フレーズ最初が子音の時のオフセット
			var offset = 0.0m;
			var firstPh = phoneme.Split('|')[0].Split(',')[0];
			if(PhonemeUtil.IsConsonant(firstPh)){
				//とりあえず 固定値
				offset = consonantOffset;
			}

			var nu = new Utterance(
				text: text,
				//tsmlを無理やり生成
				tsml: GetTsml(text, pronounce, phoneme, accent),
				//開始時刻
				start: GetStartTimeString(data, p, offset),
				//書き出しファイル名、とりあえずセリフ
				export_name: $"{text}"
			)
			{
				//感情比率
				//感情数に合わせる
				RawFrameStyle = emotionRates is null
					? "0:1:1.000:0.000:0.000:0.000:0.000"
					: $"0:1:{string.Join(':', emotionRates)}",
				//調整前LEN
				PhonemeOriginalDuration = GetSplittedTiming(p, data, consonantOffset),
			};
			//timing
			if (!string.IsNullOrEmpty(timing))
			{
				nu.PhonemeDuration = nu.PhonemeOriginalDuration;
			}
			//pitch
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
		ManageLongVowelSymbols(p);

		return p.ConvertAll(n =>
		{
			var dur = SasaraUtil
				.ClockToTimeSpan(
					n.Duration,
					data.TempoList ?? new() { { 0, 120 } })
				.TotalMilliseconds;
			var th = noteSplit?.threthold ?? 100000;
			//th = th < 100 ? 100 : th;
			th = Math.Max(th, 100);

			if (dur < th) return n;

			var spCount = (int)Math.Floor(dur / th) + 1;

			var ph = GetPhonemeLabel(GetFullContext(new List<Note> { n }), GetPhonemeMode.Note);
			var sph = ph.Split('|');
			var add = spCount - sph.Length;

			if (add <= 0) return n;

			sph = sph[^1].Split(',')[^1] switch
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
				.Join(',', sph);
			n.Lyric = GetPronounce(string.Join('|', sph));
			return n;
		})
		;
	}

	/// <summary>
	/// 歌詞中の「ー」対応
	/// </summary>
	/// <param name="p"></param>
	private void ManageLongVowelSymbols(List<Note> p)
	{
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
			var ph = GetPhonemeLabel(GetFullContext(new List<Note> { prev }), GetPhonemeMode.Note).Split('|');
			var last = ph.Last() ?? "a";
			p[i].Lyric = lyric
				.Replace(
				"ー",
				GetPronounce(last),
				StringComparison.InvariantCulture);
		}
	}

	private static readonly WanaKanaOptions kanaOption = new()
	{
		CustomKanaMapping = new Dictionary<string, string>(StringComparer.Ordinal)
		{
			{"cl","ッ"},
			{"di","ディ"},
		},
	};
	private static string GetPronounce(string phonemes)
	{
		var sb = new StringBuilder(phonemes);
		sb.Replace('|', ' ');
		sb.Replace(",", string.Empty);
		//読みを変えたフレーズ
		var yomi = WanaKana.ToKatakana(sb.ToString(), kanaOption);
		return yomi.Replace(" ", string.Empty, StringComparison.InvariantCulture);
	}

	private string GetPitches(
		List<Note> notes,
		SongData data)
	{
		var tempo = data.TempoList ?? new() { { 0, 120 } };
		List<(TimeSpan start, TimeSpan end, double logF0, int counts)> d = notes
			.ConvertAll(n =>
			(
				start: SasaraUtil
					.ClockToTimeSpan(
						tempo,
						n.Clock
					),
				end: SasaraUtil
					.ClockToTimeSpan(
						tempo,
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
		return $"l{string.Concat(ac)}";
	}

	private static string GetPhonemeLabel(
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
			.Select(s => string.Join(',', s))
			.Where(s => !string.IsNullOrEmpty(s))
			;

		return string.Join('|', splited);
	}

	private FullContextLab GetFullContext(IEnumerable<Note> notes)
	{
		_jtalk ??= new OpenJTalkAPI();

		var lyrics = GetPhraseText(notes);
		if(fcLabelCache
			.TryGetValue(lyrics, out var cachedLabel))
		{
			//キャッシュがあればキャッシュを返す
			return cachedLabel;
		}

		var text = Enumerable.Empty<string>();
		lock(_jtalk){
			text = _jtalk.GetLabels(lyrics);
		}

		if (text is null)
		{
			return new FullContextLab(string.Empty);
		}

		return new FullContextLab(string.Join('\n', text));
	}

	private static IEnumerable<string> ConvertSimpleLabel(IEnumerable<string> fullLabel)
	{
		//pL-pC+pR
		return fullLabel
			.AsParallel().AsSequential()
			.Select(s => s.Split('/')[0])
			.Select(s => FullContextLabelRegex().Match(s).Groups[1].Value)
			;
	}

	/// <summary>
	/// 音素数で等分分割した時間を求める
	/// </summary>
	/// <param name="notes"></param>
	/// <returns></returns>
	private string GetSplittedTiming(
		List<Note> notes,
		SongData song,
		decimal offset = 0.0m
	)
	{
		var tempo = song.TempoList ?? new() { { 0, 120 } };
		var timings = notes
			.AsParallel().AsSequential()
			.Select((n,i) =>
			{
				//オフセット準備
				var is1stNote = i is 0;
				var isConso1stPh = Check1stConsoPhoneme(n);
				var isConsoNextPh = CheckNextConsoPhoneme(notes, i);

				//音素数を数える
				//OpenJTalkで正確に数える
				int count = CountPhonemes(n);

				//開始時間
				var start = SasaraUtil.ClockToTimeSpan(
					tempo,
					n.Clock
				).TotalMilliseconds;
				if (isConso1stPh)
				{
					start -= (double)(offset * 1000);
				}

				//終了時間
				var end = SasaraUtil.ClockToTimeSpan(
					tempo,
					n.Clock + n.Duration
				).TotalMilliseconds;
				if (isConsoNextPh)
				{
					end -= (double)(offset * 1000);
				}

				var repeat = count > 1 && isConso1stPh && offset > 0 ? count - 1 : count;

				//ノートあたりの長さを音素数で等分
				var nLen = (decimal)(end - start);
				nLen = isConso1stPh ? nLen - (offset * 1000) : nLen;
				var sub = nLen / repeat;
				var len = Enumerable
					.Range(0, repeat)
					.Select(_ => sub / 1000m);

				len = isConso1stPh && offset > 0 ? [offset, .. len] : len;

				var str = len.Select(v => v.ToString("N3", CultureInfo.InvariantCulture));
				return string.Join(',', str);
			})
			;
		var s = string
			.Join(',', timings);
		return $"0.005,{s},0.125";
	}

	private bool CheckNextConsoPhoneme(List<Note> p, int i)
	{
		var isConsoNext = false;
		if ((i + 1) < p.Count)
		{
			var next = p[i + 1];
			isConsoNext = Check1stConsoPhoneme(next);
		}
		return isConsoNext;
	}

	private bool Check1stConsoPhoneme(Note n)
	{
		var result = GetPhonemeLabel(
				GetFullContext(new List<Note>() { n }),
				GetPhonemeMode.Note
			).Split('|', StringSplitOptions.None)[0].Split(',')[0];
		return PhonemeUtil.IsConsonant(result) && result != "N";
	}

	/// <summary>
	/// フルコンテクストラベルのキャッシュ
	/// </summary>
	private static Dictionary<string, FullContextLab> fcLabelCache = [];

	private int CountPhonemes(Note n)
	{
		//ノート歌詞が「ー」の時はOpenJTalkでエラーになるので解析しない
		if(string.Equals(n.Lyric, "ー", StringComparison.Ordinal))
		{
			//母音音素一つになるので1
			return 1;
		}
		if(n.Lyric is null){
			return 1;
		}

		var isCached = fcLabelCache
			.TryGetValue(n.Lyric, out var fullContextLab);
		var fcLabel = isCached
			? fullContextLab!
			: GetFullContext(new List<Note> { n });

		return fcLabel
			.Lines
			.Cast<FCLabLineJa>()
			.Select(s => s.Phoneme)
			//前後sil除外
			.Count(s => !string.Equals(s, "sil", StringComparison.Ordinal));
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
	private static string GetPhraseText(IEnumerable<Note> p)
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

	[GeneratedRegex(
		"[’※$＄@＠%％^＾_＿=＝]",
		RegexOptions.None,
		matchTimeoutMilliseconds: 1000)]
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
	private static string GetStartTimeString(
		SongData data,
		List<Note> p,
		decimal offset = 0.0m
	){
		var time = SasaraUtil
			.ClockToTimeSpan(
				data.TempoList!,
				p[0].Clock
			);
		var seconds = ((decimal)time.TotalMilliseconds / 1000.0m) - offset;
		Debug.WriteLine($"+ clock:{p[0].Clock}, time:{time.TotalMilliseconds}, seconds:{seconds}");
		return seconds
			.ToString("N3", CultureInfo.InvariantCulture);
	}

	[GeneratedRegex(
		"-([a-zAIUEON]+)+",
		RegexOptions.ExplicitCapture,
		matchTimeoutMilliseconds: 1000)]
	private static partial Regex FullContextLabelRegex();
	[GeneratedRegex(@"\|,",
		RegexOptions.None,
		matchTimeoutMilliseconds: 1000)]
	private static partial Regex GetPhonemeRegex();
}

/// <summary>
/// トーク変換用中間ソングデータ
/// </summary>
internal sealed record SongData{
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

	public Lab? Label { get; set; }

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