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
	public void GetTrack()
	{
		if (SampleTalk is null){
			SampleTalk.Should().NotBeNull();
			return;
		}

		ReadOnlySpan<byte> track = SampleTalk
			.GetTrack(0)
			.Span;
		ReadOnlySpan<byte> key = System.Text.Encoding.UTF8.GetBytes("name");

		var index = track.IndexOf(key);

		var str = System.Text.Encoding.UTF8.GetString(track);

		index.Should().BeGreaterThan(0);

		var head = track.Slice(index + key.Length + 2, 2);
		//データの長さ＋2
		var len = head[0];
		var typeBytes = head[1];
		var type = (VoiSonaValueType)typeBytes;

		type.Should().Be(VoiSonaValueType.String);

		var nameData = MemoryMarshal
			.Cast<byte, StringTreeAttributeData>(track.Slice(index+key.Length,4))[0];

		nameData.Type.Should().Be(type);
		nameData.Null.Should().Be(0x00);
		nameData.Delimiter.Should().Be(0x01);
		nameData.LengthPlusTwo.Should().Be(len);

		var data = track.Slice(index + key.Length + 4, nameData.LengthPlusTwo - 2);

		var tName = System.Text.Encoding.UTF8.GetString(data);

		var header = new Header(
			nameData.LengthPlusTwo - 2,
			(VoiSonaValueType)nameData.Type,
			data.ToImmutableArray()
		);
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
				Debug.WriteLine($"{v.Start}, {v.Disable}");
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
}
