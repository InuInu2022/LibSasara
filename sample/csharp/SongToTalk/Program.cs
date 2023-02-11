using System.Collections.ObjectModel;
using System.Diagnostics;
using FluentCeVIOWrapper.Common;
using LibSasara;
using LibSasara.Builder;
using LibSasara.Model;
using MathNet.Numerics.Statistics;
using Microsoft.Extensions.DependencyInjection;
using SongToTalk;
// sample app
// from song ccs notes to talk ccs
// please download server from https://github.com/InuInu2022/FluentCeVIOWrapper
// and install nupkg FluentCeVIOWrapper

var app = ConsoleApp
	.CreateBuilder(args)
	.ConfigureServices(s
		=> s.AddSingleton<CeVIOService>())
	.Build();
await app.AddCommands<Commands>().RunAsync();

internal class Commands: ConsoleAppBase, IAsyncDisposable
{
	private readonly CeVIOService service;

	public Commands(CeVIOService service)
	{
		this.service = service;
	}

	public void Dispose()
	{
		var _ = DisposeAsync();
	}

	public async ValueTask DisposeAsync()
	{
		await Task.Run(()=> CeVIOService.Finish());
	}

	[RootCommand]
	public async ValueTask RunAsync(
		[Option(
			"s",
			"path to base song ccs/ccst file.")]
			string src,
		[Option(
			"d",
			"path to export talk ccs/ccst file.")]
			string dist,
		[Option(
			"c",
			"speaking cast name")]
			string cast,
		[Option(
			"t",
			"path to template talk ccs/ccst file")]
			string template = "",
		[Option(
			"tts",
			"target cevio product.")]
			string TTS = "CeVIO_AI",
		[Option(
			"stc",
			"stretch speed to note length")]
			bool stretch = false
	){
		try
		{
			await service
				.ExecuteAsync(src, dist, cast, template, TTS, stretch);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"{ex.Message}");
			await DisposeAsync();
		}
		finally{
			await DisposeAsync();
		}
	}
}

internal class CeVIOService{
	private static Process? process;
	private FluentCeVIO? fcw;
	private CeVIOFileBase? srcCcs;
	private CeVIOFileBase? exportCcs;
	private SongUnit? song;
	private string? src;
	private string? dist;

	public async Task ExecuteAsync(
		string src,
		string dist,
		string castName,
		string template,
		string TTS,
		bool stretch
	)
	{
		if(!File.Exists(src)){
			throw new FileNotFoundException($"src:{src} is not found");
		}

		this.src = src;

		/*
		if(!File.Exists(dist)){
			throw new FileNotFoundException($"{dist} is not found");
		}*/

		this.dist = dist;

		if(!string.IsNullOrEmpty(template) && !File.Exists(template)){
			throw new FileNotFoundException($"template:{template} is not found");
		}

		await AwakeAsync(TTS);

		fcw = await StartTalkAsync(TTS);

		var casts = await fcw.GetAvailableCastsAsync();
		await fcw.CreateParam()
			.Cast(casts[0])
			.SendAsync();

		await fcw.SpeakAsync(
			"ソングトゥトークを、起動しました。"
		);

		if(!casts.Contains(castName)){
			process!.Kill();
			throw new ArgumentException($"{castName}は存在しないキャスト名です。");
		}

		await fcw.SetCastAsync(castName);
		await fcw.SpeakAsync($"キャストを{castName}に切り替えました。");

		srcCcs = await SasaraCcs.LoadAsync(src);
		song = srcCcs
			.GetUnits(Category.SingerSong)
			.OfType<SongUnit>()
			.First();
		var tempo = song.Tempo;
		var notes = song.Notes;
		var groupId = Guid.NewGuid();

		var c = await fcw
			.GetComponentsAsync();

		var castId = await fcw.GetCastIdAsync(castName);
		//var castId = string.Join("-",c[0].Id.Split("-")[0..3]);

		var path = string.IsNullOrEmpty(template) ?
			dist : template;
		exportCcs = await SasaraCcs.LoadAsync(path);

		if(Path.GetExtension(path) == "ccst"){
			exportCcs.RemoveAllGroups();
		}

		exportCcs.AddGroup(
			groupId,
			Category.TextVocal,
			$"ボイパ_{srcCcs.GetTrackSet(song.Group).group.Attribute("Name")?.Value}"
		);

		var cacheTones = new Dictionary<string, double>();
		var cachePhonemes = new Dictionary<string, ReadOnlyCollection<FluentCeVIOWrapper.Common.Models.PhonemeData>>();

		var units = await notes
			.ToAsyncEnumerable()
			.Where(v => !string.IsNullOrEmpty(v.Lyric))
			.SelectAwait(async (v, i) =>
			{
				//"ー"
				return await ReplaceProlongedMarkAsync(v, i, notes);
			})
			.SelectAwait(async (v, i) =>
			{
				//"っ"
				return await ReplaceCloseConsonantAsync(v, i, notes);
			})
			/*.SelectAwait(async v =>
			{
				return await CulcSpeedAsync(v, tempo);
			})*/
			.SelectAwait(async v =>
			{
				//Stretch option
				//APIとccsで速さ指定単位が異なるので要計算
				//API: 0.2 ～ 5.0 (cen:1.0)
				//ccs: 0 ～ 100 (cen: 50)

				if(stretch)
				{
					//TODO: stretch
					await fcw.SetSpeedAsync((uint)50);
				}else{
					await fcw.SetSpeedAsync(50);
				}

				//await fcw.SpeakAsync(v.Lyric!);
				var culc = await fcw
					.GetTextDurationAsync(v.Lyric!);
				//Console.WriteLine($"{v.cast}[{v.Lyric}] note:{v.noteLen}, talklen:{v.len}, rate:{v.rate}, culc:{culc}");

				//pitch

				//target freq.
				var tFreq = SasaraUtil.OctaveStepToFreq(v.PitchOctave, v.PitchStep);
				//Console.WriteLine($"Target Freq. {tFreq}");
				await fcw.SetToneAsync(50);
				var isCachedLyric = cacheTones.ContainsKey(v.Lyric!);

				double avgFreq;
				if(isCachedLyric)
				{
					avgFreq = cacheTones[v.Lyric!];
				}
				else{
					var wp = await EstimateAsync(v.Lyric!);
					avgFreq = wp.F0?.Where(f => f > 0).Mean() ?? 0;
					cacheTones
						.Add(v.Lyric!, avgFreq);
				}

				Console.WriteLine($" avg:{avgFreq}, target:{tFreq}");

				var isCachedPhoneme = cachePhonemes
					.ContainsKey(v.Lyric!);
				var ph = isCachedPhoneme ?
					cachePhonemes[v.Lyric!] :
					await fcw.GetPhonemesAsync(v.Lyric!);
				if (!isCachedPhoneme)
				{
					cachePhonemes[v.Lyric!] = ph;
				}

				var phs = ph
					//.Select(p => p.Phoneme)
					.Select((p, i) =>
					{
						return new TalkPhoneme
						{
							Index = i,
							Data = p.Phoneme,
							//Speed = 0,
							Tone = TalkPhoneme.CulcTone(avgFreq, tFreq)
						};
					})
					;

				Debug.WriteLine($"phs:{phs.Count()}");

				//create units
				return TalkUnitBuilder
					.Create(
						exportCcs,
						SasaraUtil
							.ClockToTimeSpan(tempo, v.Clock),
						TimeSpan.FromSeconds(culc),
						castId,
						v.Lyric ?? ""
					)
					.Group(groupId)
					.Speed(1m)	//TODO:option
					.Phonemes(phs)
					.Build();

				//return (targetRate:r, setFreq);
			})
			.ToListAsync()
			;
		await exportCcs
			.SaveAsync($"{this.dist}");
	}

	/// <summary>
    /// 「っ」を置き換える
    /// </summary>
	/// <remarks>
	/// 「っ」単体の場合は直前の母音を調べて、先頭にくっつける。
    /// 見つからない場合は「あっ」。
	/// </remarks>
    /// <param name="v"></param>
    /// <param name="i"></param>
    /// <param name="notes"></param>
    /// <returns></returns>
	private async ValueTask<Note> ReplaceCloseConsonantAsync(Note v, int i, List<Note> notes)
	{
		//TODO:「っ」で始まる複数文字の歌詞の場合の対応
		if (v.Lyric != "っ")
		{
			return v;
		}

		var lNote = notes
			.GetRange(0, i)
			.Last(n =>
				n.Lyric is not "っ");
		var ph = await fcw!.GetPhonemesAsync(lNote.Lyric!);
		var last = ph
			.Last(v =>
				v.Phoneme is not "sil"
				and not "pau");
		var nLyric = last.Phoneme switch
		{
			"a" => "あ",
			"i" => "い",
			"u" => "う",
			"e" => "え",
			"o" => "お",
			_ => "あ"
		};
		v.Lyric = $"{nLyric}っ";
		return v;
	}

	/// <summary>
	/// 「ー」を置き換える
	/// </summary>
	/// <remarks>
	/// 直前の母音を探してその音で置き換える。
	/// もしなければ「あ」。
	/// </remarks>
	/// <param name="v"></param>
	/// <param name="i"></param>
	/// <param name="notes"></param>
	/// <returns></returns>
	private async ValueTask<Note> ReplaceProlongedMarkAsync(Note v, int i, List<Note> notes)
	{
		if (v.Lyric != "ー")
		{
			return v;
		}

		var nLyric = "あ";
		var lNote = notes
			.GetRange(0, i)
			.Last(n =>
				n.Lyric is not "ー"
					and not null
					and not "");

		var ph = await fcw!.GetPhonemesAsync(lNote.Lyric!);

		var last = ph
			.Last(v =>
				v.Phoneme is not "sil"
				and not "pau");
		nLyric = last.Phoneme switch
		{
			"a" => "あ",
			"i" => "い",
			"u" => "う",
			"e" => "え",
			"o" => "お",
			_ => "あ"
		};

		v.Lyric = nLyric;
		return v;
	}

	//TODO:remove
	private async ValueTask<
		(string cast, string? Lyric, double noteLen, double len, int rate, int octave, int step, int start)>
	CulcSpeedAsync(
		Note v,
		SortedDictionary<int, int> tempo)
	{
		var cast = await fcw!.GetCastAsync();
		//speed reset
		await fcw.SetSpeedAsync(50);

		var noteLen = SasaraUtil
			.ClockToTimeSpan(tempo, v.Duration)
			.TotalSeconds;

		var len = await fcw
			.GetTextDurationAsync(v.Lyric!);

		var rate = 50;
		if (len < noteLen)
		{
			await fcw.SetSpeedAsync(0);
			var min = await fcw
				.GetTextDurationAsync(v.Lyric!);
			rate = min <= noteLen ?
				0 : //TODO:母音付与して伸ばすオプション
				50 - Convert.ToInt32((noteLen - len) / ((min - len) / 50)); //TODO:要修正
		}
		else if (len > noteLen)
		{
			await fcw.SetSpeedAsync(100);
			var max = await fcw
				.GetTextDurationAsync(v.Lyric!);
			rate = max >= noteLen ?
				100 :
				Convert.ToInt32((noteLen - len) / (max - len) / 50);//TODO:要修正
		}

		var octave = v.PitchOctave;
		var step = v.PitchStep;
		var start = v.Clock;

		return (cast, v.Lyric, noteLen, len, rate, octave, step, start);
	}

	public static void Finish()
	{
		process?.Kill();
	}

	static async ValueTask AwakeAsync(string TTS)
	{
		var ps = new ProcessStartInfo()
		{
			FileName = Path.Combine(
				AppDomain.CurrentDomain.BaseDirectory,
				@".\server\FluentCeVIOWrapper.Server.exe"
			),
			Arguments = $"-cevio {TTS}",
			CreateNoWindow = true,
			ErrorDialog = true,
			UseShellExecute = true,
			//RedirectStandardOutput = true,
		};
		ps.WorkingDirectory = Path.GetDirectoryName(ps.FileName);
		try
		{
			process = Process.Start(ps);
		}
		catch (Exception e)
		{
			Console.WriteLine($"Error {e}");
		}

		System.Console.WriteLine("awaked.");
		await Task.Delay(2000);
	}

	public async Task<FluentCeVIO> StartTalkAsync(string TTS){
		var product = TTS switch
		{
			"CeVIO_AI" => Product.CeVIO_AI,
			_ => Product.CeVIO_CS
		};
		return await FluentCeVIO
			.FactoryAsync(product: product);
	}

	public async ValueTask<WorldParam> EstimateAsync(string serifText)
	{
		var tempName = Path.GetTempFileName();

		var resultOutput = await fcw!.OutputWaveToFileAsync(serifText, tempName);
		if (!resultOutput)
		{
			var msg = $"Faild to save temp file!:{tempName}";
			throw new Exception(msg);
		}

		var (fs, nbit, len, x) = await WorldUtil.ReadWavAsync(tempName);

		//Console.WriteLine((fs, nbit, len, x));
		var parameters = new WorldParam(fs);
		//ピッチ推定
		parameters = await WorldUtil.EstimateF0Async(
			x,
			len,
			parameters
		);
		if (tempName != null && File.Exists(tempName))
		{
			await Task.Run(()=> File.Delete(tempName));  //remove temp file
		}

		return parameters;
	}
}