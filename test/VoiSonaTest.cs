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

namespace test;

public class VoiSonaTest : IAsyncLifetime
{
	private readonly ITestOutputHelper _output;
	private static TstPrj? SampleTalk;
	private static byte[]? SampleSong;

	public VoiSonaTest(ITestOutputHelper output)
	{
		_output = output;
	}

	public async Task InitializeAsync()
	{
		SampleTalk = await LibVoiSona
			.LoadAsync<TstPrj>("../../../file/voisonatalk.tstprj");
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
		SampleTalk?.Data.Should().NotBeNull();

		Console.WriteLine($"len:{SampleTalk?.Data.Length}");
	}

	[Theory]
	[InlineData(Category.TextVocal)]
	public void CheckCategory(
		Category category)
	{
		SampleTalk?.Category
			.Should().Be(category);
	}


	[Fact]
	public void GetAllTracks()
	{
		var tracksBin = SampleTalk?.GetAllTracksBin();

		tracksBin?.Count.Should().BeGreaterThan(0);

		if (tracksBin is null) return;

		var tracksTree = SampleTalk?.GetAllTracks();

		tracksTree?.Count.Should().Be(tracksBin.Count);

		tracksTree?
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

			var us = track.Utterances;

			us.ForEach(v =>
			{
				Debug.WriteLine($"{v.StartRaw}, {v.Disable}");
				Debug.WriteLine($"text: {v.Text}");
				//Debug.WriteLine($"tsml: {v.TsmlString}");
				Debug.WriteLine($"tsml: {v.Tsml}");
				Debug.WriteLine($"POD: {v.PhonemeOriginalDuration}");
				Debug.WriteLine($"PD: {v.PhonemeDuration}");

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

		oLab?.Lines.Should().NotBeNullOrEmpty();
		tLab?.Lines.Should().NotBeNullOrEmpty();

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
		oLab?.Lines?.Select(v=>v.Length)!
			.SequenceEqual(tLab?.Lines!.Select(v=>v.Length)!)
			.Should().Be(isNotTuned);
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
		const decimal x = 1000000m;
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