using System.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using LibSasara;
using LibSasara.Builder;
using LibSasara.Model;
using LibSasara.Model.Serialize;

using Xunit;
using Xunit.Abstractions;
using FluentAssertions;

namespace test;

public class LibSasaraTest : IAsyncLifetime
{
	private const string CCS_FILEPATH_AI8 = "../../../file/test_ai8_project.ccs";

	private const string CCS_FILEPATH_CS7 = "../../../file/test_cs7_project.ccs";
	private const string CCS_FILEPATH_VOISONA = "../../../file/test_voisona_project.ccs";
	private const string CCS_FILEPATH_AI8_5 = "../../../file/test_ai8_5_project.ccs";
	private const string CCST_FILEPATH_AI8_TALK1 = "../../../file/test_ai8_track_トーク.ccst";
	private const string CCST_FILEPATH_AI8_TALK2 = "../../../file/test_ai8_track_トーク2.ccst";
	private const string CCST_FILEPATH_AI8_SONG = "../../../file/test_ai8_track_ソング.ccst";
	private const string CCST_FILEPATH_AI8_SONG_EN = "../../../file/test_ai8_track_EnglishSong.ccst";
	private const string CCST_FILEPATH_CS7_TALK1 = "../../../file/test_cs7_track_トーク1.ccst";

	private const string CCST_FILEPATH_CS7_SONG1 = "../../../file/test_cs7_track_ソング1.ccst";

	private const string RIKKA_METADATA = """MAEBAXNpbAEqASoBMAEBMAEwADQB5bCP5pil5YWt6IqxAuOBp+OBmQLigKYC44CCAeOCs+ODj+ODq+ODquODg+OCqwLjg4fjgrnigJkCAgFrLG98aCxhfHIsdXxyLGl8Y2x8ayxhAmQsZXxzLFUCAnBhdQEqAioCKgIqASoCKgIqAioBMAIwAjACMAFsaGhobGwCbGwCbAJsATACMAIwAjABMAIwAjECMAA2AeODqOODvOOCsOODq+ODiALjgq3jg6Pjg6kC44GrAeODqOODvOOCsOODq+ODiALjgq3jg6Pjg6kC44OLAXksb3xvfGcsdXxyLHV8dCxvAmt5LGF8cixhAm4saQEqAioCKgEqAioCKgEwAjACMAFsaGhoaAJobAJsATACMAIwATACMAIwADEB44Gq44GjAuOBpgHjg4rjg4MC44OGAW4sYXxjbAJ0LGUBKgIqASoCKgEwAjABaGwCbAEwAjABMAIwADAB44G+44GZAeODnuOCueKAmQFtLGF8cyxVAeOBvuOBmQrjg57jgrnigJkKbSxhfHMsVQpsbAoxCjAKMAowASoBMAFsaAEwATAAMAHigKYBAQEqASoBMAFsATABMQAwAQEBc2lsASoBKgEwAQEwATA=;MAEBAXNpbAEqASoBMAEBMAEwADQB5bCP5pil5YWt6IqxAuOBp+OBmQLigKYC44CCAeOCs+ODj+ODq+ODquODg+OCqwLjg4fjgrnigJkCAgFrLG98aCxhfHIsdXxyLGl8Y2x8ayxhAmQsZXxzLFUCAnBhdQEqAioCKgIqASoCKgIqAioBMAIwAjACMAFsaGhobGwCbGwCbAJsATACMAIwAjABMAIwAjECMAA2AeODqOODvOOCsOODq+ODiALjgq3jg6Pjg6kC44GrAeODqOODvOOCsOODq+ODiALjgq3jg6Pjg6kC44OLAXksb3xvfGcsdXxyLHV8dCxvAmt5LGF8cixhAm4saQEqAioCKgEqAioCKgEwAjACMAFsaGhoaAJobAJsATACMAIwATACMAIwADEB44Gq44GjAuOBpgHjg4rjg4MC44OGAW4sYXxjbAJ0LGUBKgIqASoCKgEwAjABaGwCbAEwAjABMAIwADEB44G+44GZAeODnuOCueKAmQFtLGF8cyxVAeOBvuOBmQrjg57jgrnigJkKbSxhfHMsVQpsbAoxCjAKMAowASoBMAFobAEwATAAMAHigKYBAQEqASoBMAFsATABMQAwAQEBc2lsASoBKgEwAQEwATA""";

	private readonly ITestOutputHelper _output;
	/// <summary>
	/// 読み込み済みのCCSプロジェクト
	/// 確認済みccsファイルであれば良いテストで使用
	/// </summary>
	private static Project? _sampleProj_AI8;

	/// <summary>
	/// cs7
	/// </summary>
	private static Project? _sampleProj_CS7;
	private static CeVIOFileBase? sampleCcsAI8;
	private static CeVIOFileBase? sampleCcsCS7;
	private static CeVIOFileBase? sampleCcsVS;

	//private readonly Track _sampleTrack;

	public static IEnumerable<object[]> TestProjects => new List<object[]>()
		{
			new object[] {_sampleProj_AI8!},
			new object[] {_sampleProj_CS7!}
		};

	/// <summary>
	/// constracter
	/// </summary>
	/// <param name="output"></param>
	public LibSasaraTest(ITestOutputHelper output)
	{
		_output = output;

		//_sampleProj_AI8 = SasaraCcs.LoadDeserializedAsync<Project>(CCS_FILEPATH_AI8).Result;
		//_sampleProj_CS7 = SasaraCcs.LoadDeserializedAsync<Project>(CCS_FILEPATH_CS7).Result;
		//_sampleTrack = SasaraCcs.LoadAsync<Track>(CCST_FILEPATH_CS7_TALK1).Result;
	}

	public async Task InitializeAsync()
	{
		sampleCcsAI8 = await SasaraCcs.LoadAsync<CcsProject>(CCS_FILEPATH_AI8);
		sampleCcsCS7 = await SasaraCcs.LoadAsync(CCS_FILEPATH_CS7);
		sampleCcsVS = await SasaraCcs.LoadAsync(CCS_FILEPATH_VOISONA);
	}

	public Task DisposeAsync()
	{
		return Task.CompletedTask;
	}

	[Fact(DisplayName = "名称チェック")]
	public void CheckLibName()
	{
		Assert.Equal("LibSasara", LibSasara.SasaraCcs.Name);
	}

	[Theory]
	[InlineData(CCS_FILEPATH_AI8)]
	[InlineData(CCS_FILEPATH_CS7)]
	[InlineData(CCST_FILEPATH_AI8_SONG)]
	[InlineData(CCST_FILEPATH_CS7_TALK1)]
	public async Task LoadFileAsync(string path)
	{
		//_output.WriteLine($"path:{Path.GetFullPath(path)}");
		Assert.True(File.Exists(path), $"path:{Path.GetFullPath(path)}");
		IRoot result = Path.GetExtension(path) switch
		{
			"ccst" => await SasaraCcs.LoadDeserializedAsync<Track>(path),
			_ => await SasaraCcs.LoadDeserializedAsync<Project>(path),
		};

		Assert.NotNull(result);
		switch (Path.GetExtension(path))
		{
			case "ccs":
				{
					Assert.True(result.GetType() == typeof(Project));
					break;
				}

			case "ccst":
				{
					Assert.True(result.GetType() == typeof(Track));
					break;
				}

			default:
				break;
		}
	}

	[Theory]
	[InlineData(CCS_FILEPATH_AI8)]
	[InlineData(CCS_FILEPATH_CS7)]
	public async void LoadCcsAsync(string path)
	{
		//_output.WriteLine($"path:{Path.GetFullPath(path)}");
		Assert.True(File.Exists(path), $"path:{Path.GetFullPath(path)}");
		var result = await SasaraCcs.LoadDeserializedAsync<Project>(path);

		Assert.NotNull(result);
		Assert.False(string.IsNullOrEmpty(result.Code));

		var ccs = await SasaraCcs.LoadAsync(path);
		Assert.NotNull(ccs);
		Assert.NotNull(ccs as CcsProject);
	}

	[Theory]
	[InlineData(CCST_FILEPATH_AI8_SONG)]
	[InlineData(CCST_FILEPATH_CS7_TALK1)]
	public async void LoadCcstAsync(string path)
	{
		var result = await SasaraCcs.LoadAsync(path);
		Assert.NotNull(result);
		Assert.NotNull(result as CcstTrack);
	}

	[Trait("Category", "CCS/Version")]
	[Theory]
	[InlineData(CCS_FILEPATH_AI8, "8.3.17.0")]
	[InlineData(CCS_FILEPATH_CS7, "7.0.23.1")]
	public async void VersionAutherAsync(string path, string version)
	{
		var result = await SasaraCcs.LoadDeserializedAsync<Project>(path);

		Assert.Equal(result.Generation?.Author?.Version, new Version(version));
	}

	[Trait("Category", "CCS/Version")]
	[Theory]
	[InlineData(CCS_FILEPATH_AI8, "6.0.20")]
	[InlineData(CCS_FILEPATH_CS7, "5.1.5")]
	public async void VersionTtsAsync(string path, string version)
	{
		var result = await SasaraCcs.LoadDeserializedAsync<Project>(path);

		Assert.Equal(result.Generation?.TTS?.Version, new Version(version));
	}

	[Theory]
	[InlineData(CCST_FILEPATH_AI8_SONG, true)]
	[InlineData(CCST_FILEPATH_AI8_SONG_EN, true)]
	[InlineData(CCST_FILEPATH_CS7_SONG1, true)]
	[InlineData(CCST_FILEPATH_AI8_TALK1, false)]
	public async void IsSongAsync(string path, bool isSong)
	{
		var result = await SasaraCcs.LoadDeserializedAsync<Track>(path);

		Assert.Equal((result?.Units?[0]?.Category is Category.SingerSong), isSong);
	}

	[Theory]
	[InlineData(CCST_FILEPATH_AI8_SONG)]
	//[InlineData(CCS_FILEPATH_VOISONA)]
	public async void CheckSongTrackDataAsync(string path)
	{
		var result = await SasaraCcs.LoadDeserializedAsync<Track>(path);

		var songData = result.GetUnits(Category.SingerSong)[0].Song;

		Assert.NotNull(songData);
	}

	[Theory]
	[InlineData(CCS_FILEPATH_VOISONA)]
	[InlineData(CCS_FILEPATH_AI8)]
	public async void CheckSongProjectDataAsync(string path)
	{
		var result = await SasaraCcs.LoadDeserializedAsync<Project>(path);

		var songData = result.GetUnits(Category.SingerSong)[0]?.Song;

		Assert.NotNull(songData);
	}

	[Theory]
	[InlineData(CCS_FILEPATH_AI8)]
	[InlineData(CCS_FILEPATH_CS7)]
	[InlineData(CCS_FILEPATH_VOISONA)]
	public async void GetUnitDirectAsync(string path)
	{
		var result = await SasaraCcs.LoadDeserializedAsync<Project>(path);

		Assert.True(
			result
				.GetUnits(Category.SingerSong)
				.TrueForAll(v => v.Category == Category.SingerSong)
	  );

		Assert.True(
			result
				.GetUnits(Category.TextVocal)
				.TrueForAll(v => v.Category == Category.TextVocal)
		);

		Assert.True(
			result
				.GetUnits(Category.OuterAudio)
				.TrueForAll(v => v.Category == Category.SingerSong)
		);
	}

	[Theory]
	[InlineData(CCS_FILEPATH_AI8)]
	[InlineData(CCS_FILEPATH_CS7)]
	[InlineData(CCS_FILEPATH_VOISONA)]
	public async void GetFullParams(string path)
	{
		// Given
		var result = await SasaraCcs.LoadDeserializedAsync<IRoot>(path);
		var p =  result.GetUnits(Category.SingerSong)[0]?.Song?.Parameter;
		// When
		var full = p?.LogF0?.GetFullData();
		var logF0len1 = p?.LogF0?.Length;
		var logF0len2 = full?.Count;
		// Then
		Assert.True(
			logF0len1 == logF0len2,
			$"LogF0:{logF0len1},{logF0len2}");

		var noDataLen1 = GetLogF0DataNum<NoData>(p);
		var noDataLen2 = GetFilteredFullDataNum<NoData>(full);
		Assert.True(noDataLen1 == noDataLen2, $"NoData:{noDataLen1}:{noDataLen2}");

		var dataLen1 = GetLogF0DataNum<Data>(p);
		var dataLen2 = GetFilteredFullDataNum<Data>(full);
		Assert.True(dataLen1 == dataLen2, $"{dataLen1}:{dataLen2}");

		var t1 = logF0len1 - (noDataLen2 + dataLen2);
		var t2 = GetFilteredFullDataNum<TuneData>(full);
		Assert.True(t1 == t2, $"{t1}:{t2}");

		int GetLogF0DataNum<T>(Parameter? p)
		=> p?
			.LogF0?
			.Data?
			.Where(v => v.GetType() == typeof(T))
			.Select(v => v.Repeat == 0 ? 1 : v.Repeat)
			.Sum()
			?? 0;
		int GetFilteredFullDataNum<T>(List<ITuneData>? full){
			return full?.Where(v => v.GetType() == typeof(T)).Count() ?? 0;
		}
	}

	[Theory]
	[InlineData(CCS_FILEPATH_AI8, $"{CCS_FILEPATH_AI8}.new.ccs")]
	[InlineData(CCS_FILEPATH_CS7, $"{CCS_FILEPATH_CS7}.new.xml")]
	public async void LoadAndSaveSerializedAsync(string from, string to){
		var result = await SasaraCcs.LoadDeserializedAsync<IRoot>(from);
		if(result is null
			|| result?.Generation is null
			|| result?.Generation?.Author is null){
			return;
		}

		result.Generation.Author.Version = new Version(0, 0);

		//Assert.True(v == new Version(0, 0));

		await result!.SaveSerializedAsync(to);
	}

	[Theory]
	[InlineData(CCS_FILEPATH_AI8, $"{CCS_FILEPATH_AI8}.dup.ccs")]
	[InlineData(CCS_FILEPATH_CS7, $"{CCS_FILEPATH_CS7}.dup.xml")]
	[Trait("Category", "FileIO")]
	public async void LoadAndSaveAsync(
		string oldPath,
		string newPath
	){
		var result = await SasaraCcs
			.LoadAsync(oldPath);

		var id = result
			.GetTrackSets()
			.First()
			.GetGroupId();

		var dup = await result.DuplicateAndAddTrackSetAsync(id);

		dup.ReplaceAllCastId("XXXX");

		await result.SaveAsync(newPath);
	}

	[Theory]
	[InlineData(CCS_FILEPATH_AI8)]
	[InlineData(CCS_FILEPATH_CS7)]
	[InlineData(CCST_FILEPATH_AI8_SONG)]
	[InlineData(CCST_FILEPATH_AI8_SONG_EN)]
	[InlineData(CCST_FILEPATH_AI8_TALK1)]
	[InlineData(CCST_FILEPATH_AI8_TALK2)]
	public void IsCeVIOFile(string path)
	{
		Assert.True(SasaraCcs.IsCevioFile(path), $"file:{Path.GetFileName(path)}");
	}

	[Fact]
	public void InitAsyncTest()
	{
		// Given
		var isAI = sampleCcsAI8?
			.AutherVersion?
			.Major == 8;
		Assert.True(isAI);
	}


	[Theory]
	[InlineData(CCS_FILEPATH_AI8)]
	public async Task LoadLinqAsync(string path){
		var result = await SasaraCcs
			.LoadAsync(path);
		result.GetUnitsRaw();

		switch(Path.GetExtension(path))
		{
			case ".ccs":
				{
					Assert.True(result is CcsProject);
					break;
				}

			case ".ccst":
				{
					Assert.True(result is CcstTrack);
					break;
				}
		}
	}

	[Theory]
	[InlineData(CCS_FILEPATH_AI8)]
	public async Task ReplaceCastAsync(string path){
		var result = await SasaraCcs
			.LoadAsync(path);
		var u0 = result.GetUnitsRaw().FirstOrDefault();

		if(u0 is null)
		{
			return;
		}

		var cast = u0!.HasAttributes
			? u0?.Attribute("CastId")?.Value
			: null;
		var guid = u0!.HasAttributes
			? u0?.Attribute("Group")?.Value
			: null;

		Assert.NotNull(guid);
		Assert.NotNull(cast);
		if(guid is null || cast is null)
		{
			return;
		}

		result
			.GetTrackSet(new(guid))
			.ReplaceAllCastId(cast, "NEWCAST")
			;

		var r2 = result.GetUnitsRaw()[0];
		Assert.True(r2?.Attribute("CastId")?.Value == "NEWCAST");
	}

	[Theory]
	//[InlineData(CCS_FILEPATH_AI8)]
	[InlineData(CCS_FILEPATH_CS7)]
	[InlineData(CCST_FILEPATH_AI8_TALK2)]
	public async Task MetadataAsync(string path)
	{
		var result = await SasaraCcs
			.LoadAsync(path);
		var m = result
			.GetUnits(Category.TextVocal)
			.Cast<TalkUnit>()
			//.First()
			//.Metadata
			//.Select(v => v.Metadata)
			;
		//var s = Encoding.UTF8.GetString(m);

		if(!Directory.Exists("./out/")){
			Directory.CreateDirectory("./out/");
		}

		for (var i = 0; i < m.Count();i++)
		{
			var item = m.ElementAt(i);
			using var fs = new FileStream(
				$"./out/metadata.{Path.GetFileNameWithoutExtension(path)}.{item.CastId}.bin",
				FileMode.OpenOrCreate
			);
			byte[] metadata = item.Metadata[0];
			Assert.NotEmpty(metadata);
			Assert.True(metadata.Length > 0,$"byte len: {metadata.Length}");
			await fs.WriteAsync(metadata);
			//fs.Close();
			fs.Flush();
			await Task.Delay(2000);
		}
	}

	[Theory]
	[InlineData(RIKKA_METADATA)]
	public async void CheckMetadataAsync(string metadata)
	{
		var mlist = metadata.Split(";");
		for (int i = 0; i < mlist.Length; i++)
		{
			var s = PaddingForBase64(mlist[i]);
			Assert.True(s.Length % 4 == 0);
			var m = Convert.FromBase64String(s);
			using var fs = new FileStream(
				$"./out/metadata.{s[..12]}.{i}.bin",
				FileMode.OpenOrCreate
			);
			await fs.WriteAsync(m);
			fs.Close();
		}

		static string PaddingForBase64(string s)
		{
			var mod = s.Length % 4;
			if(mod == 0)
			{
				return s;
			}

			var add = 4 - mod;
			var len = s.Length + add;
			return s.PadRight(len, '=');
		}
	}

	[Theory]
	[InlineData(CCS_FILEPATH_CS7)]
	[InlineData(CCS_FILEPATH_AI8)]
	public async void GetDirections(string path)
	{
		var result = await SasaraCcs
			.LoadAsync(path);
		var m = result
			.GetUnits(Category.TextVocal)
			.Cast<TalkUnit>();

		foreach (var item in m)
		{
			Assert.NotNull(item.RawDirection);
			var (Volume, Speed, Tone, Alpha, LogF0Scale) = item.Directions;
			_output.WriteLine($"[{item.CastId}]「{item.Text}」{Volume},{Speed},{Tone},{Alpha},{LogF0Scale}");
			Assert.NotInRange(Volume, 8m, -8m);
			Assert.NotInRange(Speed, 5m, -0.2m);
			Assert.NotInRange(Tone, 6m, -6m);
			Assert.NotInRange(Alpha, 0.6m, -0.5m);
			Assert.NotInRange(LogF0Scale, 2m, 0.0m);

			item.Directions = (1.0m, 1.1m, 1.1m, 0.6m, 1.1m);
			(Volume, Speed, Tone, Alpha, LogF0Scale) = item.Directions;
			//_output.WriteLine($"SET[{item.CastId}]「{item.Text}」{Volume},{Speed},{Tone},{Alpha},{LogF0Scale}");
		}
	}

	[Theory]
	[InlineData("1.1", 1.1)]
	[InlineData("1.23456789", 1.23456789)]
	[InlineData("-1.23456789", -1.23456789)]
	[InlineData("100000", 100000)]
	[InlineData("Not a number", 0)]
	public void ConvertStr2Decimal(string sValue, decimal dValue)
	{
		var result = SasaraUtil.ConvertDecimal(sValue);
		Assert.Equal(result, dValue);
	}

	[Theory]
	[InlineData(CCS_FILEPATH_CS7)]
	[InlineData(CCS_FILEPATH_AI8)]
	public async void Components(string path)
	{
		var result = await SasaraCcs
			.LoadAsync(path);

		result
			.GetUnits(Category.TextVocal)
			.Cast<TalkUnit>()
			.SelectMany(u =>
			{
				_output.WriteLine($"cast:{u.CastId}");
				return u.Components;
			})
			.ToList()
			.ForEach(v =>
			{
				_output.WriteLine($"{v.Id}:{v.Value}");
				Assert.NotNull(v.Id);
				Assert.InRange(v.Value, 0m, 1m);
			})
			;

		result
			.GetUnits(Category.TextVocal)
			.Cast<TalkUnit>()
			.ToList()
			.ForEach(v =>
			{
				var id = result.AutherVersion?.Major switch
				{
					> 7 => $"{v.CastId}_ANGRY",
					< 8 => "fine",
					_ => ""
				};
				v.Components = new(){
					(
						Id:id,
						Value:0.99m
					)};
				var cv = v
					.Components
					.Find(c => c.Id == id)
					.Value;
				Assert.Equal(0.99m, cv);
				//_output.WriteLine($"	comps:{v.Components.Select(c => c.Id+':'+c.Value)}");
			});
	}

	/*
	<Phoneme Data="U" Volume="-0.11524219348052744" Speed="-0.019197994987468596" Tone="-1.7509830357360845" />
	*/
	[Theory]
	[InlineData(-0.11524219348052744)]
	[InlineData(18.02)]
	[InlineData(19.02)]

	public void Decibel(double linear)
	{
		var decibel = 20 * Math.Log10(linear);
		var val = Math.Pow(10, linear / 20);
		_output.WriteLine($"l:{linear}, d:{decibel}, val:{val}");
	}

	[Theory]
	[InlineData(CCS_FILEPATH_CS7)]
	[InlineData(CCS_FILEPATH_AI8)]
	public async void Phonemes(string path){
		var result = await SasaraCcs
			.LoadAsync(path);

		result
			.GetUnits(Category.TextVocal)
			.Cast<TalkUnit>()
			.ToList()
			.ForEach(u =>
			{
				_output.WriteLine($"--{u.CastId}--");
				_output.WriteLine($"「{u.Text}」");

				var ps = u.Phonemes;

				var t = u.Phonemes
					.Select(v => v.Data)
					.Where(v => !string.IsNullOrEmpty(v))
					.Aggregate((a, b) =>
						a + ", " + b
					)
					;

				_output.WriteLine($"ph[{t}]");
			})
			;
	}

	[Theory]
	[InlineData(CCS_FILEPATH_CS7)]
	[InlineData(CCS_FILEPATH_AI8)]
	public async void AddTalkUnitsAsync(string path){
		var result = await SasaraCcs
			.LoadAsync(path);

		var tu = TalkUnitBuilder
			.Create(
				result,
				new TimeSpan(0, 0, 0),
				new TimeSpan(0, 0, 1),
				"A",
				"てすと"
			)
			.Build();
		//checks
		tu.Should().NotBeNull();
	}

	[Fact]
	public void BuildTalkUnitTest(){
		var id = Guid.NewGuid();
		var result = TalkUnitBuilder
			.Create(
				sampleCcsCS7!,
				new TimeSpan(0, 0, 0),
				new TimeSpan(0, 0, 1),
				"CAST_TEST",
				"テストの歌詞"
				)
			.Alpha(0.6m)
			.Group(id)
			.Language("Latin")
			.Volume(1.1)
			.Speed(0.98)
			.Tone(-1.0)
			.LogF0Scale(1.1)
			.Build();
		result.Should().NotBeNull();
		Assert.Equal(0.6m, result.Alpha);
		result.Group.Should().Be(id);
		Assert.Equal("Latin", result.Language);
		Assert.Equal(1.1m, result.Volume);
		Assert.Equal(0.98m, result.Speed);
		Assert.Equal(-1, result.Tone);
		Assert.Equal(1.1m, result.LogF0Scale);
	}

	[Theory]
	[InlineData(CCS_FILEPATH_CS7,"A")]
	[InlineData(CCS_FILEPATH_AI8,"CTNV-JPF-A")]
	public async void SongToTalkAsync(
		string path,
		string castId
	){
		var result = await SasaraCcs
			.LoadAsync(path);

		var song1 = result
			.GetUnits(Category.SingerSong)
			.OfType<SongUnit>()
			.First();

		//Group
		var newId = Guid.NewGuid();

		/*
		var g = new XElement(result
			.RawGroups
			.Last()
		);
		g.Attribute("Id")!.Value = newId.ToString();
		g.Attribute("Name")!.Value = "Generated";
		g.Attribute("Category")!.Value = nameof(Category.TextVocal);
		result.RawGroups.Last().AddAfterSelf(g);
		/*/
		result
			.AddGroup(
				newId,
				Category.TextVocal,
				"Generated",
				castId
			);
		//*/

		var tempo = song1.Tempo;

		var notes = song1.Notes;
		var culcs = notes
			.Select(v => (
				Text: v.Lyric,
				StartTick: v.Clock,
				StartMsec: SasaraUtil
					.ClockToTimeSpan(tempo, v.Clock),
				DurationClock: v.Duration,
				DurationMsec: SasaraUtil
					.ClockToTimeSpan(tempo, v.Duration)
			));

		culcs.ToList().ForEach(v => _output.WriteLine($"culcs:{v}"));

		var units = notes
			.Select(v => (
				Text: v.Lyric,
				StartTime: SasaraUtil
					.ClockToTimeSpan(tempo, v.Clock),
				Duration: SasaraUtil
					.ClockToTimeSpan(tempo, v.Duration),
				CastId: castId,
				Group: newId
			))
			.Select(v => {
				var elem = TalkUnit
					.CreateTalkUnitRaw(
						v.StartTime,
						v.Duration,
						v.CastId,
						v.Text,
						v.Group,
						components: new List<(string, decimal)>(),
						phonemes: new List<TalkPhoneme>()
					);
				return elem;
			})
			;
		result
			.GetUnitsRaw()
			.Last()?
			.Parent?
			.Add(units);

		var newPath = $"{path}.s2t.ccs";

		await result.SaveAsync(newPath);
	}

	public static object[][] TestTempo
		=> new object[][]
		{
			new object[]{
				0,
				new TimeSpan(0),
				new SortedDictionary<int,int>(){
					{0, 120}
				}
			},
			new object[]{
				960,
				new TimeSpan(0,0,0,0,500),
				new SortedDictionary<int,int>(){
					{0, 120}
				}
			},
			new object[]{
				960,
				new TimeSpan(0,0,0,0,500),
				new SortedDictionary<int,int>(){
					{0, 120},
					{960, 120}
				}
			},
			new object[]{
				960,
				new TimeSpan(0,0,0,0,500),
				new SortedDictionary<int,int>(){
					{0, 120},
					{960, 60}
				}
			},
			new object[]{
				960*4,
				new TimeSpan(0,0,0,1,600),
				new SortedDictionary<int,int>(){
					{0, 150}
				}
			},
			new object[]{
				960*6,
				new TimeSpan(0,0,0,3,190),
				new SortedDictionary<int,int>(){
					{0, 120},
					{960*5, 87},
					{960*6, 100},
					{960*7, 177},
					{960*8, 55},
					{960*9, 200},
					{960*10, 125},
				}
			},
			new object[]{
				960*6.5,
				new TimeSpan(0,0,0,3,490),
				new SortedDictionary<int,int>(){
					{0, 120},
					{960*5, 87},
					{960*6, 100},
					{960*7, 177},
					{960*8, 55},
					{960*9, 200},
					{960*10, 125},
				}
			},
			new object[]{
				4800,
				new TimeSpan(0,0,0,2,439),
				new SortedDictionary<int,int>(){
					{0, 123},
					{7680, 88},
				}
			},
			new object[]{
				8640,
				new TimeSpan(0,0,0,4,584),
				new SortedDictionary<int,int>(){
					{0, 123},
					{7680, 88},
				}
			}
		};

	[Theory]
	[MemberData(nameof(TestTempo))]
	public void ClockToTimeSimple(
		int testClock,
		in TimeSpan should,
		SortedDictionary<int,int> tempos
	)
	{
		_output.WriteLine(
			$"- tick:{testClock}"
		);
		var result = SasaraUtil
			.ClockToTimeSpan(tempos, testClock);
		Assert.InRange(
			result,
			should - new TimeSpan(0, 0, 0, 0, 10),
			should + new TimeSpan(0, 0, 0, 0, 10));
		//Assert.Equal(should, result);
	}

	[Theory]
	[InlineData(440)]
	public void FreqNoteTest(int freq)
	{
		var num = SasaraUtil
			.FreqToNoteNum(freq);
		var rFreq = SasaraUtil
			.NoteNumToFreq(num);

		Assert.Equal(freq, rFreq);
	}

	[Theory]
	[InlineData(CCS_FILEPATH_CS7)]
	[InlineData(CCS_FILEPATH_AI8)]
	public async void AddAudioUnitsAsync(string path){
		var result = await SasaraCcs
			.LoadAsync(path);

		var tu = AudioUnitBuilder
			.Create(
				result,
				new TimeSpan(0, 0, 0),
				new TimeSpan(0, 0, 1),
				Path.GetTempFileName()
			)
			.Build();
		//checks
		Assert.NotNull(tu);
	}

	[Fact]
	public void BuildAudioUnitTest(){
		var id = Guid.NewGuid();
		var result = AudioUnitBuilder
			.Create(
				sampleCcsCS7!,
				new TimeSpan(0, 0, 0),
				new TimeSpan(0, 0, 1),
				Path.GetTempFileName()
			)
			.Group(id)
			.Build();
		Assert.NotNull(result);
		Assert.Equal(id, result.Group);
		Assert.NotNull(result.FilePath);
		Assert.True(File.Exists(result.FilePath));
	}

	[Theory]
	[InlineData(CCS_FILEPATH_CS7, 3, 1, 2, 0)]
	[InlineData(CCS_FILEPATH_AI8, 6, 3, 2, 1)]
	[InlineData(CCS_FILEPATH_VOISONA, 1, 1)]
	public async void GetTrackSetsTestAsync(
		string path,
		int trackCount,
		int songCount = 0,
		int talkCount = 0,
		int audioCount = 0
	){
		var ccs = await SasaraCcs
			.LoadAsync(path);

		var result = ccs
			.GetTrackSets<UnitBase>();

		Assert.NotNull(result);
		Assert.Equal(result.Count, trackCount);

		var songs = ccs.GetTrackSets<SongUnit>();
		Assert.Equal(songs.Count, songCount);
		if(songCount>0){
			var f = songs[0];
			var a = ccs.GetTrackSet<SongUnit>(f.GroupId);
			Assert.Equal(f, a);

			var s = SongUnitBuilder
				.Create(ccs, new TimeSpan(), new TimeSpan(), "XXX")
				.Build();
			f.AddUnit(s);
			Assert.Equal(s.Group, f.GroupId);
		}

		var talks = ccs.GetTrackSets<TalkUnit>();
		Assert.Equal(talks.Count, talkCount);
		if(talkCount>0){
			var f = talks[0];
			var a = ccs.GetTrackSet<TalkUnit>(f.GroupId);
			Assert.Equal(f, a);

			var s = TalkUnitBuilder
				.Create(ccs, new TimeSpan(), new TimeSpan(), "XXX", "test")
				.Build();
			f.AddUnit(s);
			Assert.Equal(s.Group, f.GroupId);

		}

		var audios = ccs.GetTrackSets<AudioUnit>();
		Assert.Equal(audios.Count, audioCount);
		if(audioCount>0){
			var f = audios[0];
			var a = ccs.GetTrackSet<AudioUnit>(f.GroupId);
			Assert.Equal(f, a);

			var s = AudioUnitBuilder
				.Create(ccs, new TimeSpan(), new TimeSpan(), "path/to")
				.Build();
			f.AddUnit(s);
			Assert.Equal(s.Group, f.GroupId);

		}
	}

	[Theory]
	[InlineData(CCS_FILEPATH_CS7)]
	//[InlineData(CCS_FILEPATH_AI8)]
	public async Task GetVolumeAsync(string path)
	{
		var ccs = await SasaraCcs
			.LoadAsync(path);
		Assert.NotNull(ccs);

		var su = ccs.GetUnits(Category.SingerSong)
			.Cast<SongUnit>()
			.First();
		Assert.NotNull(su);

		var rParams = su.RawParameterChildren;
		Assert.NotNull(rParams);

		var rVol = su.RawVolume;
		//Assert.NotNull(rVol);

		var vol = su.Volume;
		Assert.NotNull(vol);

		var data = vol.Data;
		Assert.NotNull(data);

		var full = vol.GetFullData();
		Assert.Equal(vol.Length, full.Count);

		if(su.Duration > TimeSpan.Zero){
			//Assert.True(full.Count > 0);
		}

		const int count = 5000;
		var full2 = vol.GetFullData(count);
		Assert.Equal(count, full2.Count);

		var full3 = Parameters
			.ShrinkData(full2);

		var sb = new StringBuilder();
		full3
			.ForEach(f2 =>
			{
				if (f2 is Data d)
				{
					sb.Append("Data: ");
				}else if(f2 is NoData n){
					sb.Append("NoData: ");
				}else{
					sb.Append("TuneData: ");
				}

				sb.Append("Index:").Append(f2.Index).Append(", Repeat:").Append(f2.Repeat);
				if (f2 is Data d2){
					sb.Append(", Value:").Append(d2.Value);
				}
				sb.AppendLine();
			});
		await File.WriteAllTextAsync(
			Path.ChangeExtension(path, ".full3.txt"),
			sb.ToString()
		);

		vol.Data = full3.Cast<TuneData>().ToList();

		su.Volume = vol;
		//var vol2 = su.Volume;
		//Assert.NotEqual(count, vol2.Length);

		await ccs
			.SaveAsync(Path.ChangeExtension(path, ".tmp.ccs"));
	}

	[Theory]
	[InlineData(CCS_FILEPATH_CS7)]
	[InlineData(CCST_FILEPATH_AI8_SONG)]
	[InlineData(CCS_FILEPATH_AI8_5)]
	[InlineData(CCS_FILEPATH_VOISONA)]
	public async void SongElementTestAsync(string path)
	{
		var ccs = await SasaraCcs
			.LoadAsync(path);
		Assert.NotNull(ccs);

		var su = ccs.GetUnits(Category.SingerSong)
			.Cast<SongUnit>()
			.First();
		Assert.NotNull(su);

		_output.WriteLine($"- song version: {su.SongVersion}");

		if (su.Alpha is not 0)
		{
			_output.WriteLine($"- Alpha: {su.Alpha}");
		}

		_output.WriteLine($"- CommonKeys: {su.CommonKeys}");
		_output.WriteLine($"- HUS: {su.Husky}");
		_output.WriteLine($"- PitchShift: {su.PitchShift}");
		_output.WriteLine($"- PitchTune: {su.PitchTune}");

		//default expect <= 1.7
		if (
			su.SongVersion > new Version(0, 0)
				&& su.SongVersion <= new Version(1, 7)
		)
		{
			Assert.Equal(0.0m, su.Husky);
			Assert.Equal(440.0m, su.PitchShift);
			Assert.Equal(0.0m, su.PitchTune);
			Assert.False(su.CommonKeys);
		}

		const decimal valAlp = 0.68m;
		const decimal valHus = 0.51m;
		const decimal valPitS = 440.0m;
		const decimal valTune = 0.12m;

		su.Alpha = valAlp;
		Assert.Equal(valAlp, su.Alpha);
		su.CommonKeys = true;
		Assert.True(su.CommonKeys);
		su.Husky = valHus;
		Assert.Equal(valHus, su.Husky);
		su.PitchShift = valPitS;
		Assert.Equal(valPitS, su.PitchShift);
		su.PitchTune = valTune;
		Assert.Equal(valTune, su.PitchTune);
	}
}