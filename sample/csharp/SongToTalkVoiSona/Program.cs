using System.Collections;
using System.Collections.Immutable;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using ConsoleAppFramework;
using LibSasara;
using LibSasara.Model;
using LibSasara.Model.Serialize;
using LibSasara.VoiSona;
using LibSasara.VoiSona.Model.Talk;
using SharpOpenJTalk.Lang;

ConsoleApp.Run<SongToTalk>(args);

public partial class SongToTalk: ConsoleAppBase
{
	private OpenJTalkAPI? _jtalk;

	[RootCommand]
	public async ValueTask<int> ExportAsync(
		[Option("s", "path to a export tstprj file")]
		string pathToSongCcs,
		[Option("e", "path to a export tstprj file")]
		string pathToPrj,
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
		var processed = await ProcessCcsAsync(pathToSongCcs);

		//解析を元にtstprj作成
		await GenerateFileAsync(processed, pathToPrj);

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
		var ccs = await SasaraCcs.LoadAsync(path);
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
		string exportPath
	)
	{
		var path = Path.Combine(
			System.AppDomain.CurrentDomain.BaseDirectory,
			"file/template.tstprj"
		);
		var TemplateTalk = await LibVoiSona
			.LoadAsync<TstPrj>(path);
		await InitOpenJTalk();
		var us = processed
			.PhraseList?
			//.AsParallel()
			.Select(ToUtterance(processed))
			.ToImmutableList()
			;
		if (us is null)
		{
			Console.Error.WriteLine("解析に失敗しました。。。");
			return;
		}
		var tstprj = TemplateTalk
			.ReplaceAllUtterances(us);
		await LibVoiSona
			.SaveAsync(exportPath, tstprj.ToArray());
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
		SongData data
	)
	{
		return p =>
		{
			//フレーズをセリフ化
			var text = GetPhraseText(p);

			//読みを変えたフレーズ
			//TODO:音素toカナ変換
			var pronounce = text;/*string.Concat(
				p.Select(n => n.Phonetic ?? n.Lyric));*/
			//フレーズの音素
			//TODO: OpenJTalk辺りで解析
			//var labels = _jtalk?.GetLabels(text);
			var phoneme = GetPhonemeLabel(p, GetPhonemeMode.Note);//"k,e|r,o|k,e|r,o|k.e|r,o|k,e|r,o|k,u|w,a|k,u|w,a|k,u|w,a";//"d,o|r,e|m,i";
			//アクセントの高低
			//TODO: OpenJTalk辺りで解析
			var accent = "lhhh";
			//調整後LEN
			//TODO:ccsやlabから割当
			var timing = "";
			//PIT
			//TODO:楽譜データだけならnote高さから計算
			//TODO:ccsやwavがあるなら解析して割当
			var pitch = "";

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
				//TODO:感情数に合わせる
				RawFrameStyle = "0:1:1.000:0.000:0.000:0.000:0.000",
				//調整前LEN
				PhonemeOriginalDuration = GetSplittedTiming(p, data),
			};
			if (!string.IsNullOrEmpty(timing))
			{
				nu.PhonemeDuration = nu.PhonemeOriginalDuration; //timing;
			}
			if (!string.IsNullOrEmpty(pitch)){
				nu.RawFrameLogF0 = pitch;
			}
			Console.WriteLine($"u[{text}], start:{nu.Start}");
			return nu;
		};
	}

	private string GetPhonemeLabel(
		IEnumerable<Note> notes,
		GetPhonemeMode mode
	){
		_jtalk ??= new OpenJTalkAPI();

		var text = _jtalk.GetLabels(GetPhraseText(notes.ToList()));
		var list = new List<string>();
		foreach (var note in notes)
		{
			var lyrics = GetPhraseText(new() { note });
			var fLab = _jtalk.GetLabels(lyrics);
			var lab = ConvertSimpleLabel(fLab);
			list.Add(string.Join(",", lab));
		}
		return string.Join("|",list);
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
	private static string GetSplittedTiming(
		List<Note> p, SongData song)
	{
		var s = string
			.Join(",", p.Select(n =>
			{
				//音素数を数える
				//TODO:OpenJTalk辺りで正確に数える
				var count = 0;
				string str = n.Lyric ?? "";
				for (int i = 0; i < str.Length; i++)
				{
					char c = str[i];
					count += IsSinglePhoneme(c) ? 1 : 2;
				}
				count = count == 0 ? 1 : count;

				//ノートあたりの長さを音素数で等分
				var start = SasaraUtil.ClockToTimeSpan(
					song.TempoList ?? new(),
					n.Clock
				).TotalMilliseconds;
				var end = SasaraUtil.ClockToTimeSpan(
					song.TempoList ?? new(),
					n.Clock + n.Duration
				).TotalMilliseconds;
				var dur = SasaraUtil.ClockToTimeSpan(
					song.TempoList ?? new(),
					n.Duration
				);
				var sub = (decimal) dur.Milliseconds / count;
				var len = Enumerable
					.Range(0, count)
					.Select(_ => sub / 1000m)
					.Select(v => v.ToString("N2", CultureInfo.InvariantCulture));
				return string.Join(",", len);
			})
			)
			;
		return $"0.005,{s},0.125";

		static bool IsSinglePhoneme(char c)
		{
			const string singles = "あいうえおんアイウエオンぁぃぅぇぉァィゥェォー";
			return singles.Contains(c, StringComparison.InvariantCulture);
		}
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

		return SpecialLabelRegex()
			.Replace(concated, string.Empty);
	}

	[GeneratedRegex("[’※$＄@＠%％^＾]")]
	private static partial Regex SpecialLabelRegex();

	private static string GetTsml(
		string text,
		string pronounce,
		string phoneme,
		string accent)
	{
		var bytes = Encoding.UTF8.GetByteCount(text);
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
	//TODO:public IEnumerable<List<>>? PitchList { get; set; }

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