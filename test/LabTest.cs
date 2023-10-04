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

namespace test;

public class LabTest : IAsyncLifetime
{
	private readonly ITestOutputHelper _output;
	private static string SampleLab1 = "";
	private static string SampleLab2 = "";

	public LabTest(ITestOutputHelper output)
	{
		_output = output;
	}

	public async Task InitializeAsync()
	{
		SampleLab1 = await File
			.ReadAllTextAsync("../../../file/ソング.lab");
		SampleLab2 = await File
			.ReadAllTextAsync("../../../file/EnglishSong.lab");
	}

	public Task DisposeAsync()
	{
		return Task.CompletedTask;
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
	public void DisplaceSeconds()
	{
		var label = LibSasara.SasaraLabel.LoadAsync("../../../file/ソング.lab").Result;

		var _ = label.DisplaceSecondsAsync(1.1);
	}
}
