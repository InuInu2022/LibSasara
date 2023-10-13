using System.Net.Mime;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

using LibSasara.Model;
using LibSasara.VoiSona;

using FluentAssertions;
using System;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Attributes;
using LibSasara.VoiSona.Model.Talk;
using LibSasara.VoiSona.Model;
using System.Runtime.InteropServices;
using LibSasara.VoiSona.Util;
using System.Collections.Immutable;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Xml;
using System.Xml.Linq;
using System.Globalization;
using System.Text;
using Microsoft.Extensions.Primitives;
using CommandLine;

namespace test;

public class VoiSonaTest : IAsyncLifetime
{
	private readonly ITestOutputHelper _output;
	private static TstPrj? SampleTalk;
	private static TstPrj? TemplateTalk;
	private static byte[]? SampleSong;

	public VoiSonaTest(ITestOutputHelper output)
	{
		_output = output;
	}

	public async Task InitializeAsync()
	{
		SampleTalk = await LibVoiSona
			.LoadAsync<TstPrj>("../../../file/voisonatalk.tstprj");
		TemplateTalk = await LibVoiSona
			.LoadAsync<TstPrj>("../../../file/template.tstprj");
		//SampleSong = await File
		//	.ReadAllTextAsync("../../../file/voisonasong.tssprj");

	}

	public Task DisposeAsync()
	{
		return Task.CompletedTask;
	}

	[Fact]
	public void LoadTest()
	{
		SampleTalk.Should().NotBeNull();
		if (SampleTalk is null) return;
		SampleTalk.Data.Should().NotBeNull();

		Console.WriteLine($"len:{SampleTalk?.Data.Length}");
	}

	[Theory]
	[InlineData(Category.TextVocal)]
	public void CheckCategory(
		Category category)
	{
		if (SampleTalk is null) return;
		SampleTalk.Category
			.Should().Be(category);
	}


	[Fact]
	public void GetAllTracks()
	{
		if (SampleTalk is null) return;
		var tracksBin = SampleTalk
			.GetAllTracksBin();

		tracksBin.Should().HaveCountGreaterThan(0);

		if (tracksBin is null) return;

		var tracksTree = SampleTalk.GetAllTracks();

		tracksTree.Count.Should().Be(tracksBin.Count);

		tracksTree
			.Select(t => t.AttributeCount)
			.Should().NotBeEmpty()
			.And.HaveCountGreaterThan(0)
			;

		var names = tracksTree?
			.Select(v => v.Attributes.FirstOrDefault(a => a.Key == "name")?.Value?.ToString())
			.Cast<string>()
			;
		names.Should()
			.NotBeEmpty()
			.And.AllBeOfType<string>();
	}

	[Fact]
	public void TrackHasVoice()
	{
		var tracksBin =
			SampleTalk?.GetAllTracksBin()
			;
		if (tracksBin is null) return;

		ReadOnlySpan<byte> key = System.Text.Encoding.UTF8
			.GetBytes($"Voice\0");

		tracksBin
			.Select(t =>
			{
				ReadOnlySpan<byte> key = System.Text.Encoding.UTF8.GetBytes($"Voice\0");
				return t.Span.IndexOf(key);
			})
			.Where(v => v >= 0)
			.Count()
			.Should()
			.BeGreaterThan(0)
			;
	}

	[Fact]
	public void TryFindCollection()
	{
		if (SampleTalk is null) return;

		var tracks = SampleTalk
			.GetAllTracksBin()
			.ToList()
			;

		tracks.Should().NotBeNullOrEmpty();

		tracks[0].Span.Length.Should().BeGreaterThan(0);

		foreach(var track in tracks)
		{
			_output.WriteLine($"{track.Length}");

			var hasContents =
				BinaryUtil.TryFindCollection(
					track.Span,
					"Contents",
					"Utterance",
					out var utterances
				);
			hasContents.Should().BeTrue();
			if (!hasContents) continue;

			foreach(var serif in utterances){
				var hasText =
				BinaryUtil.TryFindAttribute(
					serif.Span,
					"text",
					out var text
				);
				if (!hasText) continue;
				var s = text.Data.ToString();

				var hasTsml =
				BinaryUtil.TryFindAttribute(
					serif.Span,
					"tsml",
					out var tsml
				);

				var hasDuration =
					BinaryUtil.TryFindChild(serif.Span, "PhonemeOriginalDuration", out var tmg);
				if (hasDuration){
					var hasValue =
						BinaryUtil.TryFindAttribute(tmg, "values", out var value);
					if (!hasValue) continue;
					var tmgs = value.Data;
				}
			}



		}
	}

	[Fact]
	public void CheckUtterance()
	{
		if (SampleTalk is null) return;
		var tracks = SampleTalk
			.GetAllTracks();

		foreach(var track in tracks)
		{
			Debug.WriteLine($"track:{track.TrackName}");
			var v = track.Voice;
			Debug.WriteLine($"{v?.Name}, id:{v?.Id}, {v?.Speaker}, {v?.Version}");

			track.Voice = new Voice("A", "B", "100.0.0");
			track.Voice.Speaker.Should().Be("A");
			track.Voice.Id.Should().Be("B");
			track.Voice.Version.Should().Be("100.0.0");

			Debug.WriteLine($"	volume:{track.Volume}");
			Debug.WriteLine($"	pan:{track.Pan}");

			var us = track.Utterances;

			us.ForEach(v =>
			{
				Debug.WriteLine($"{v.RawStart}, {v.Disable}");
				Debug.WriteLine($"text: {v.Text}");
				//Debug.WriteLine($"tsml: {v.TsmlString}");
				Debug.WriteLine($"tsml: {v.Tsml}");
				Debug.WriteLine($"POD: {v.PhonemeOriginalDuration}");
				Debug.WriteLine($"PD: {v.PhonemeDuration}");

				Debug.WriteLine("--globals--");
				Debug.WriteLine($"{v.SpeedRatio}");
				Debug.WriteLine($"{v.C0Shift}");
				Debug.WriteLine($"{v.LogF0Shift}");
				Debug.WriteLine($"{v.AlphaShift}");
				Debug.WriteLine($"{v.LogF0Scale}");

				var ph = v.Tsml
					.Descendants("word")
					.Attributes("phoneme")
					.Select(s => s.Value)
					.Select(s => s == "" ? "sil" : s)
					;
				var phonemes = string.Join("|", ph);
				Debug.WriteLine($"ph:{phonemes}");

				v.DefaultLabel?
				.Lines?
				.ToList()
				.ForEach(s =>
				{
					Debug.WriteLine($"lab:{s.From}, {s.To}, {s.Phoneme}");
				});

				v.Label?
				.Lines?
				.ToList()
				.ForEach(s =>
				{
					Debug.WriteLine($"lab:{s.From}, {s.To}, {s.Phoneme}");
				});
			});
		}
	}

	[Theory]
	[InlineData("0.1,0.1,0.1", "", true)]
	[InlineData("0.1,0.2,0.3", "1:0.4,2:0.3", false)]
	public void UtteranceDurations(
		string original,
		string tuned,
		bool isNotTuned,
		string tsml = "<word phoneme=\"a|a|a\" />"
	)
	{
		var u = new Utterance("test", tsml, "0.00")
		{
			PhonemeOriginalDuration = original,
			PhonemeDuration = tuned
		};

		u.Should().NotBeNull();
		//set and get test
		u.PhonemeOriginalDuration.Should().Be(original);
		u.PhonemeDuration.Should().Be(tuned);

		//lab
		var oLab = u.DefaultLabel;
		var tLab = u.Label;

		(oLab?.Lines).Should().NotBeNullOrEmpty();
		(tLab?.Lines).Should().NotBeNullOrEmpty();

		//lab phonemes
		var oPh = string.Join("", oLab?.Lines?.Select(l => l.Phoneme) ?? Array.Empty<string>());
		var tPh = string.Join("", tLab?.Lines?.Select(l => l.Phoneme) ?? Array.Empty<string>());
		(oPh == tPh).Should().BeTrue();

		//lab timings
		var oLen = oLab?.Lines?.Count();
		var tLen = tLab?.Lines?.Count();
		oLen.Should().Be(tLen);
		Debug.WriteLine(string.Join(",",oLab?.Lines?.Select(v => v.Length.ToString(CultureInfo.InvariantCulture)!)!));
		Debug.WriteLine(string.Join(",",tLab?.Lines?.Select(v => v.Length.ToString(CultureInfo.InvariantCulture)!)!));

		var l = oLab?.Lines ?? Array.Empty<LabLine>();
		l.Select(v=>v.Length)
			.SequenceEqual(tLab?.Lines!.Select(v=>v.Length)!)
			.Should().Be(isNotTuned);
	}

	[Theory]
	[InlineData("こんにちは")]
	[InlineData("0000")]
	[InlineData("日本語カタカナ")]
	[InlineData(
"""
いつは昨日同時にこういう教育めというもののためを云うざるない。
そんなに一番へ満足学はもしその周旋ますでだけを纏っば来あっがは参考威張っでしたから、あまりにはいうないないたた。
一員よりしたものはようやく今で充分なませない。
ついに久原君に講演主義どう解釈で云えん国家この花柳それか担任からという皆妨害ますでしましですて、その昔は彼らか文壇鶴嘴が連れて、嘉納さんのものに事業の私を現にお所有と云って何釣竿とお推測に命じようにひょろひょろご発展がしですんて、どうしてもあに通知にあるでばいるなのが突き破っますだ。
またはしかしお世界へ掴み気も再び高等ときたて、その会にも云ったてという一つをありからいないない。
こういう時女の時その分子は何いっぱいにぶつかっですかと木下さんに打ち壊さましです、ついでの絶対でしょってご逡巡たんまして、主義の中に個性の時間くらいの壇を今向いていて、ある程度の同年に起っでそんな限りをちょうど勤まりうでとするです事まして、汚ませましから始終お理よっませはずべきなかろう。
また顔か有名か運動をなっなと、翌日中一種を考えていたためにお存在の事実を思わだろた。
十月をはむくむく書いばなれたんなけれでて、たといのらくらなろから約束は始終少なくなのなかっ。
"""
	)]
	public void UtteranceText(string text)
	{
		const string defaultText = "aaa";
		var u = new Utterance(defaultText, "<word phoneme=\"a|a|a\" />", "0.00")
		{
			PhonemeOriginalDuration = "0.1,0.1,0.1"
		};
		u.Text.Should().Be(defaultText);

		u.Text = text;
		u.Text.Should().Be(text);
	}

	[Theory]
	[InlineData("0.1,0.1,0.1", "")]
	[InlineData("0.1,0.2,0.3", "1:0.4,2:0.3")]
	public void LabelString(
		string original,
		string tuned,
		string tsml = "<word phoneme=\"a|a|a\" />"
	)
	{
		var u = new Utterance("test", tsml, "0.00")
		{
			PhonemeOriginalDuration = original,
			PhonemeDuration = tuned
		};

		var labStr1 = u.DefaultLabel?.ToString();
		var labStr2 = LabString(u, original);
		labStr1 = labStr1?.Replace(".0", "", StringComparison.InvariantCulture);
		labStr2 = labStr2?.Replace(".0", "", StringComparison.InvariantCulture);

		labStr1.Should().Be(labStr2);
	}

	private string LabString(
		Utterance utterance,
		string durations
	){
		ReadOnlySpan<string> pd = durations
			.Split(",".ToCharArray())
			;
		var ph = utterance
			.Tsml
			.Descendants("word")
			.Attributes("phoneme")
			.Select(s => s.Value)
			.Select(s => s == "" ? "sil" : s)
			;
		char[] separators = { ',', '|' };
		ReadOnlySpan<string> phonemes = string
			.Join("|", ph)
			.Split(separators)
			;
		var cap = 30 * pd.Length;
		var sb = new StringBuilder(cap);
		decimal time = 0m;
		const decimal x = 10000000m;
		for (var i = 0; i < pd.Length; i++)
		{
			var s = time;
			time += decimal.TryParse(pd[i], out var t)
				? t * x : 0m;
			var e = time;
			var p = (i - 1 < 0 || phonemes.Length <= i - 1)
				? "sil"
				: phonemes[i - 1]
				;
			sb.AppendLine(
				CultureInfo.InvariantCulture,
				$"{s} {e} {p}");
		}
		return sb.ToString();
	}
	[Fact]
	public void CopyTest()
	{
		if (TemplateTalk is null) return;
		//データをテンプレから書き換え可能にしてコピー
		var copied = TemplateTalk.Copy();

		copied.Should().NotBeNull();
		var r = new TstPrj(copied.Span.ToArray());
		r.GetAllTracksBin().Count
			.Should()
			.Be(TemplateTalk
				.GetAllTracksBin()
				.Count);
	}

	[Theory]
	[InlineData("Takahashi", "techno-sp_ja_JP_m801_tts.tsnvoice","2.0.0 ported from CeVIO AI")]
	[InlineData("Sato Sasara", "techno-sp_ja_JP_f801_tts.tsnvoice", "1.0.0 ported from CeVIO AI")]
	[InlineData("Suzuki Tsudumi", "techno-sp_ja_JP_f802_tts.tsnvoice", "1.0.0 ported from CeVIO AI")]
	public async void ReplaceVoice(
		string speaker,
		string id,
		string version,
		int trackIndex = 0
	)
	{
		if (TemplateTalk is null) return;

		var tstprj = TemplateTalk
			.ReplaceVoice(
				new Voice(
					speaker, id, version),
				trackIndex
			);

		await LibVoiSona
			.SaveAsync($"../../../file/replaced{speaker}.tstprj", tstprj.ToArray());
	}

	[Fact]
	public async void ReplaceAllUtterances()
	{
		if (TemplateTalk is null) return;

		var f0 = string.Empty;
		var sb = new StringBuilder(100000);
		const int limit = 1000;
		for (var i = 0; i< limit; i++){
			string f = Math
				.Round(1.0 + 0.005 * i, 3)
				.ToString("F3", CultureInfo.InvariantCulture);
			string v = Math
				.Round(4.4 + (i * 0.001), 3)
				.ToString("F3", CultureInfo.InvariantCulture);
			sb.Append(
				CultureInfo.InvariantCulture,
				$"{f}:{v}");
			if(i<limit-1)sb.Append(',');
		}
		f0 = sb.ToString();

		var sb2 = new StringBuilder(100000);
		for(var i = 0; i<limit; i++){
			var f = Math.Round(1.0 + 0.005 * i, 3)
			.ToString("F3", CultureInfo.InvariantCulture);
			sb2.Append(CultureInfo.InvariantCulture, $"{f}:4.6");
			if(i<limit-1)sb2.Append(',');
		}

		var us = new List<Utterance>(new Utterance[]{
			new(
				"ドレミ",
				"""<acoustic_phrase><word begin_byte_index="0" chain="0" end_byte_index="15" hl="lhhhh" original="ドレミ" phoneme="d,o|r,e|m,i" pos="感動詞" pronunciation="ドレミ">ドレミ</word></acoustic_phrase>""",
				"1.234",
				export_name: "Talk1_1")
			{
				//must
				RawFrameStyle = "0:1:1.000:0.000:0.000:0.000:0.000",
				PhonemeOriginalDuration = "0.1,0.2,0.3,0.4,0.5,0.6,0.7,0.8,0.9",

				//len
				PhonemeDuration = "0.1,0.2,0.3,0.4,0.5,0.6,0.7,0.8,0.9",

				//pitch write
				RawFrameLogF0 = f0
			},
			new(
				"ファソラ",
				"""<acoustic_phrase><word begin_byte_index="0" chain="0" end_byte_index="15" hl="lhhhh" original="ファソラ" phoneme="f,a|s,o|r,a" pos="感動詞" pronunciation="ファソラ">ファソラ</word></acoustic_phrase>""",
				"6.456",
				export_name: "Talk1_2")
			{
				//must
				RawFrameStyle = "0:1:1.000:0.000:0.000:0.000:0.000",
				PhonemeOriginalDuration = "0.1,0.2,0.3,0.4,0.5,0.6,0.7,0.8,0.9",

				//pitch write
				RawFrameLogF0 = f0
			},
			new(
				"シラソ",
				"""<acoustic_phrase><word begin_byte_index="0" chain="0" end_byte_index="15" hl="lhhhh" original="シラソ" phoneme="sh,i|r,a|s,o" pos="感動詞" pronunciation="シラソ">シラソ</word></acoustic_phrase>""",
				"10.0",
				export_name: "Talk1_3")
			{
				//must
				RawFrameStyle = "0:1:1.000:0.000:0.000:0.000:0.000",
				PhonemeOriginalDuration = "0.1,0.2,0.3,0.4,0.5,0.6,0.7,0.8,0.9",

				//pitch write
				RawFrameLogF0 = sb2.ToString()
			}
		});

		var tstprj = TemplateTalk
			.ReplaceAllUtterances(us);

		await LibVoiSona
			.SaveAsync($"../../../file/replaced_utterances.tstprj", tstprj.ToArray());
	}


	[Fact]
	public void BuildLab(){
		var logger = new AccumulationLogger();

        var config = ManualConfig.Create(DefaultConfig.Instance)
            .AddLogger(logger)
            .WithOptions(ConfigOptions.DisableOptimizationsValidator);

        BenchmarkRunner.Run<Benchmarks>(config);

        // write benchmark summary
        _output.WriteLine(logger.GetLog());
		Console.WriteLine(logger.GetLog());
	}

	[Theory]
	[InlineData("test", 1, 100)]
	[InlineData("test2", -10000, 12323)]
	public void TreeAttributes(
		string key,
		int firstValue,
		int secondValue
	)
	{
		var tree = new Tree("Test");

		//ここではまだ0
		tree.AttributeCount.Should().Be(0);

		//同じキーなら上書き
		tree.AddAttribute(key, firstValue, VoiSonaValueType.Int32);
		tree.AddAttribute(key, secondValue, VoiSonaValueType.Int32);
		tree.AttributeCount.Should().Be(1);
		tree.GetAttribute<int>(key)
			.Value
			.Should().Be(secondValue);

		//存在しない筈
		tree.GetAttribute<object>("notfound")
			.Value.Should().BeNull();
	}

	[Theory]
	[InlineData(1, true)]
	[InlineData(100, true)]
	public void TreeChild(
		int childNum,
		bool isCollection
	)
	{
		const string name = "Content";
		var content = new Tree(name,isCollection);
		for (int i = 0; i < childNum; i++)
		{
			content.Children.Add(new Tree($"Child{i}"));
		}

		//null check
		content.Should().NotBeNull();
		if (content is null) return;

		//count check
		content.Children.Should().NotBeEmpty();
		content.Children.Should().HaveCount(childNum);

		//byte check
		var bytes = content.GetBytes();

		//bytes name
		var nb = Encoding.UTF8.GetBytes($"{name}\0");
		var n = bytes.Slice(0, nb.Length);
		Encoding.UTF8.GetString(n.ToArray())
			.Should().Be($"{name}\0");

		//bytes count
		var span = bytes
					.Slice(nb.Length, 4)
					.Span;
		var index = isCollection ? 3 : 2;
		var count = span.Slice(index-1,1)[0];
		count.Should().Be((byte)childNum);
		_output.WriteLine($"count:{count}");


	}

	[Theory]
	[InlineData("test",10, sizeof(byte))]
	[InlineData("test2",3000, sizeof(short))]
	[InlineData("test3",35000, sizeof(int))]
	public void TreeCollection(
		string key,
		int count,
		int size
	)
	{
		var tree = new Tree(key, true);
		tree.Should().NotBeNull();
		if (tree is null) return;

		tree.IsCollection.Should().BeTrue();

		//collection
		for(var i = 0; i < count; i++){
			var c = new Tree($"{key}_{i}");
			tree.Children.Add(c);
		}
		tree.Count.Should().Be(count);

		var header = tree.GetChildHeader();
		header.Should().NotBeNull();

		//size
		var sizeOf = (int)header
			.Slice(2, 1)
			.ToArray()[0];
		sizeOf.Should().Be(size);
		(size + 3).Should().Be(header.Length);
	}
}

[ShortRunJob]
public class Benchmarks
{
	private Utterance utterance;
	public Benchmarks()
	{
		utterance = new Utterance(
			"test",
			"<word phoneme=\"a|a|a\" />",
			"0.00")
		{
			PhonemeOriginalDuration = "0.0,0.1,0.2,0.3"
		};
	}

	[Benchmark]
	public void BuildLab()
	{
		var lab = utterance.Label;
	}
}