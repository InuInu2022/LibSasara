using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

using LibSasara.Model;

using FluentAssertions;
using System;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Attributes;
using System.Text.Json;

namespace test;

public class LabFixture: IAsyncLifetime
{
	public static string SampleLab1 = "";
	public static string SampleLab2 = "";

	public Task InitializeAsync()
	{
		SampleLab1 = File.ReadAllText("../../../file/ソング.lab");
		SampleLab2 = File
			.ReadAllText("../../../file/EnglishSong.lab");
		return Task.CompletedTask;
	}

	public Task DisposeAsync()
	{
		return Task.CompletedTask;
	}
}

public class LabTest : IClassFixture<LabFixture>
{
	private readonly ITestOutputHelper _output;
	private readonly LabFixture _fixture;

	public static readonly string SampleLab1 = File
		.ReadAllText("../../../file/ソング.lab");
	public static readonly string SampleLab2 = File
		.ReadAllText("../../../file/EnglishSong.lab");

	public static TheoryData<string, int> Samples { get; set; } = new()
		{
			{SampleLab1, 30},
			{SampleLab2, 30},
		};

	public LabTest(ITestOutputHelper output, LabFixture fixture)
	{
		_output = output;
		_fixture = fixture;
		//SampleLab1 = LabFixture.SampleLab1;
		//SampleLab2 = LabFixture.SampleLab2;
	}

	[Theory]
	[MemberData(nameof(Samples))]
	//[ClassData(typeof(SampleLabs))]
	public void CtorLab(string text, int fps)
	{
		//text = SampleLab1;
		text.Should().NotBeNullOrEmpty();
		var result = new Lab(text, fps);
		result.Should().NotBeNull();
		result.Lines.Should().NotBeNull();
		if (result.Lines is null) return;
		result.Lines.Count().Should().BePositive();
	}

	public static readonly
		TheoryData<Lab, int>
	PhraseData = new()
	{
		{new(SampleLab1), 2},
		{new(SampleLab2), 1}
	};

	[Theory]
	[MemberData(nameof(PhraseData))]
	public void SplitByPhrase(
		Lab lab,
		int count
	)
	{
		var list = lab?
			.SplitToSentence(0.001);
		list.Should().NotBeNullOrEmpty();
		list!.Should().HaveCount(count);
	}

	public static readonly
		TheoryData<Lab, double>
	ChangeLengthData = new()
	{
		{new Lab(SampleLab1!), 50},
		{new Lab(SampleLab1!), 100},
		{new Lab(SampleLab1!), 11.1},
	};

	[Theory]
	[MemberData(nameof(ChangeLengthData))]
	public async void ChangeLengthByRate(
		Lab label,
		double expectPercent
	)
	{
		if (label is null) return;
		var original = new Lab(label.ToString());
		original.Should().NotBeNull();
		await label
			.ChangeLengthByRateAsync(expectPercent);

		var previous = original.Lines?.First().Length;
		var current = label.Lines?.First().Length;

		var rate = expectPercent / 100;
		(previous / current)
			.Should()
			.BeInRange(rate - 0.01, rate + 0.01);

		var prevEnd = original.Lines?.Last().Length;
		var currEnd = label.Lines?.Last().Length;
		(prevEnd / currEnd).Should()
			.BeInRange(rate - 0.01, rate + 0.01);
	}


	// labelfile, expect time, line index, diff time
	public static readonly
		TheoryData<Lab, double, Index, double>
	DisplaceSecondsData = new()
	{
		{new Lab(SampleLab1), 0.0, new(0), -100.0},
		{new Lab(SampleLab1), 0.0, new(1), -100.0},
		{new Lab(SampleLab1), 0.0, new(0, true), -100.0},
		{new Lab(SampleLab1), 59700000, new(0, true), 0.0},
		{new Lab(SampleLab1), 18501016.2601626+1000000, new(0), 1.0},
		{new Lab(SampleLab2), 0.0, new(0), -100.0},
		{new Lab(SampleLab2), 0.0, new(1), -100.0},
		{new Lab(SampleLab2), 0.0, new(0, true), -100.0},
		{new Lab(SampleLab2), 66300000, new(0, true), 0.0},
		{new Lab(SampleLab2), 18700000+1000000, new(0), 1.0},
	};

	[Theory]
	[MemberData(nameof(DisplaceSecondsData))]
	public async Task DisplaceSecondsAsync(
		Lab label,
		double expectFrom,
		Index index,
		double displaced
	)
	{
		if (label is null) return;
		await label.DisplaceSecondsAsync(displaced);

		var resultFrom = label.Lines?.ElementAt(index)?.From;
		var resultTo = label.Lines?.ElementAt(index)?.To;

		//tests
		resultFrom.Should().NotBeNull();
		resultFrom.Should().Be(expectFrom);
		resultTo.Should().NotBeNull();
		var targetTo = expectFrom + (displaced * 10000000);
		resultTo.Should().BeInRange(targetTo-0.01, targetTo+0.01);

		_output.WriteLine($"result:{resultFrom}");
	}

	[Fact]
	public void TestPerformance_DisplaceSeconds()
	{
		var logger = new AccumulationLogger();

		var config = ManualConfig.Create(DefaultConfig.Instance)
			.AddLogger(logger)
			.WithOptions(ConfigOptions.DisableOptimizationsValidator);

		BenchmarkRunner.Run<LabBenchmarks>(config);

		// write benchmark summary
		_output.WriteLine(logger.GetLog());
	}
}

[BenchmarkDotNet.Attributes.MemoryDiagnoser]
[InProcess]
public class LabBenchmarks
{
	[BenchmarkDotNet.Attributes.Benchmark]
	public static void DisplaceSeconds()
	{
		var label = LibSasara.SasaraLabel.LoadAsync("../../../file/ソング.lab").Result;

		var _ = label.DisplaceSecondsAsync(1.1);
	}
}
