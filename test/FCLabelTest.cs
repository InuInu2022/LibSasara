using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FluentAssertions;
using LibSasara.Model.FullContextLabel;
using Xunit;
using Xunit.Abstractions;

namespace test;

public class FCLabelFixture : IAsyncLifetime
{
	public string SampleNoTime { get; private set; }
		= string.Empty;

	public FCLabelFixture()
	{
	}

	public Task InitializeAsync()
	{
		SampleNoTime = File.ReadAllText("../../../file/out.full.lab");
		return Task.CompletedTask;
	}

	public Task DisposeAsync()
	{
		return Task.CompletedTask;
	}

}

public partial class FCLabelTest: IClassFixture<FCLabelFixture>
{
	private readonly ITestOutputHelper _output;
	private readonly FCLabelFixture _fixture;

	public FCLabelTest(
		ITestOutputHelper output,
		FCLabelFixture fixture)
	{
		_output = output;
		_fixture = fixture;
	}

	[GeneratedRegex(@"\-([a-zA-Z]+)\+")]
	private static partial Regex CurrentPhonemeRegex();

	[Fact]
	public void Ctor()
	{
		var result = new FullContextLab(_fixture.SampleNoTime);
		result.Should().NotBeNull();

		var lines = _fixture.SampleNoTime
			.Split(
				new[] { "\n", "\r\n", "\r" },
				StringSplitOptions.RemoveEmptyEntries)
			.Where(s => !string.IsNullOrEmpty(s));

		var reg = CurrentPhonemeRegex();
		var linePhonemes = lines
			.Select(s => reg.Match(s).Groups[1].Value)
			.ToArray();

		//line count check
		lines
			.Should().HaveCount(result.Lines.Count);

		//line phonemes check
		result.Lines
			.Select(line => line.Phoneme)
			.Should()
			.BeEquivalentTo(linePhonemes);
	}

	[Fact]
	public void HasTime()
	{
		var result = new FullContextLab(_fixture.SampleNoTime);

		result.Lines
			.Select(line => line.HasTime)
			.Should()
			.AllBeEquivalentTo(false);
	}

	[Fact]
	public void LineMora()
	{
		var result = new FullContextLab(_fixture.SampleNoTime);
		result.Should().NotBeNull();
		if (result is null) return;

		var moras = result.Lines
			.Cast<FCLabLineJa>();
		moras
			.Where(m => m.MoraIdentity?.IsMora ?? false)
			.Select(line => line.MoraIdentity?.HasValidAccentPosDiff)
			.Should()
			.AllBeEquivalentTo(true);

		var splited = FullContextLabUtil
			.SplitByMora(moras);

		var phs = splited
			.Select(s => s.Select(s2 => s2.Phoneme).ToList());
	}

	[Theory]
	[InlineData("abc/def/ghi",'/',3)]
	[InlineData("123.45 3434.12 cl",' ',3)]
	public void SplitSpan(
		string text,
		char separator,
		int expectCount
	)
	{
		var span = text.AsSpan();
		var result = FullContextLab
			.SplitSpan(span, separator);
		result.Should().HaveCount(expectCount);
		var str = text.Split(separator);

		str.Should()
			.HaveElementAt(0, result[0].ToString());
		str.Last().Should()
			.Be(result[result.Count - 1].ToString());
	}

	[Theory]
	[InlineData("xx",-1)]
	[InlineData("1",1)]
	[InlineData("100",100)]
	[InlineData("-12",-12)]
	[InlineData("abc",-1)]
	public void GetNumber(string text, int expect)
	{
		var result = FullContextLabUtil.GetNumber(text);
		result.Should().Be(expect);
	}

	[Theory]
	[InlineData("A:-3+1+7",':','+',"-3")]
	[InlineData("A:-3+1+7",'+','+',"1")]
	[InlineData("E:7_4!0_xx-xx",'_','!',"4")]
	[InlineData("E:7_4!0_xx-xx",'_','-',"xx", 2)]
	[InlineData("F:7_4#0_xx@1_1",'_','#',"4")]
	[InlineData("F:7_4#0_xx@1_1",'_','@',"xx",2)]
	[InlineData("F:7_4#0_xx@1_1",'@','_',"1")]
	public void GetCharFromContext(
		string context,
		char before,
		char after,
		string expect,
		int count = 1)
	{
		ReadOnlyMemory<char> mem = context.ToCharArray();
		var result = FullContextLabUtil
			.GetCharsFromContext(mem, before, after,count);
		result.ToString()
			.Should().Be(expect);
	}
}