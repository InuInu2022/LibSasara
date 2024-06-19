/*
# tstprj format memo

[TreeName + \0] (int8) [AttrNum]
	[attr_name + \0] 0x01

[NoAttrTree + \0] 0x00 0x01 0x01

名前 (utf8string)
終端文字列 (0x00)
データ個数の型(byte数)（0x00=なし, 0x01=int8, 0x02=(u?)int16）
データ個数
データの型 (0x00=delimeter?, 0x04=float64 0x05=utf8string)
データbytes

--------------------------------------

PhonemeOriginalDuration 0x00 0x01 0x01
	values 0x00 0x01 (text.len-2) 0x05 (comma separated text) 0x00 0x00
Utterance 0x00 0x01 0x05
	text 0x00 0x01 0x0E (utf8string) 0x00
	tsml

TSTalker 0x00 0x01 (attr_num)
	format 0x00 0x01 0x05 (?)
Tracks 0x00 0x00 0x01 (child_num)
	Track 0x00 0x01 (atrr_num)
		name 0x00 0x01 (len+2) 0x05 (utf8string)
		volume? 0x00 0x01 len 0x04 (double)
		pan? 0x00 0x01 len 0x04 (double)
	Voice 0x00 0x01 (attr_num)
		speaker
		name
		version 0x00 0x01 len 0x05 (utf8string)
	Contents 0x00 0x00 0x01 (child_num)
		Utterance 0x00 0x01 (attr_num)
			text 0x00 0x01 (text_bytes_len) 0x05 (utf8string) 0x00
			tsml 0x00 0x02 (int16_len) 0x05 (utf8string)
			start 0x00 0x01 (text_bytes_len) 0x05 (utf8string)
			export_name 0x00 0x01 (text_bytes_len) 0x05 (utf8string)
			disable 0x00 0x01 0x01 0x02 (bool)
			FrameStyle			//STY
				values
			PhonemeOriginalDuration	//default LEN(TMG)
				values
			FrameC0				//VOL
				values
			FrameLogF0			//PIT
				values
			PhonemeDuration 	//変更したLEN(TMG)
				values
			FrameAlpha			//ALP
				values
			SpeedRatio			//話速
				value 0x00 0x01 (text_bytes_len) 0x05
			C0Shift				//Global VOL
				value
			LogF0Shitt			//Global PIT
				value
			AlphaShift			//Global ALP
				value
			LogF0Scale			//Global Intonation
				value

*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

using LibSasara.Model;
using LibSasara.VoiSona.Util;

namespace LibSasara.VoiSona.Model.Talk;

/// <summary>
/// VoiSona Talk prj
/// </summary>
public record TstPrj: VoiSonaFileBase
{

	/// <inheritdoc/>
	public override Category Category { get; }
		= Category.TextVocal;

	/// <summary>
	/// VoiSona Talk prj コンストラクタ
	/// </summary>
	public TstPrj(IReadOnlyList<byte> data)
		: base(data){
	}

	/// <summary>
	/// トークトラックを取得
	/// </summary>
	/// <param name="index"></param>
	/// <returns></returns>
	[Obsolete($"Use {nameof(GetAllTracksBin)}")]
	public ReadOnlyMemory<byte> GetTrack(int index = 0)
	{
		var tracks = GetAllTracksBin();
		if(tracks.Count-1 < index || index < 0){
			return ReadOnlyMemory<byte>.Empty;
		}
		return tracks[index];
	}

	/// <summary>
	/// get track bin data
	/// </summary>
	/// <returns></returns>
	public ReadOnlyCollection<ReadOnlyMemory<byte>> GetAllTracksBin()
	{
		Span<byte> span = bytes.ToArray();
		const string word = "Track";
		Span<byte> key = Encoding.UTF8
			.GetBytes($"{word}\0");

		return span
			.Split(key)
			//１つ目はtrackデータではないのでスキップ
			.Skip(1)
			.ToList()
			.AsReadOnly();
	}

	/// <summary>
	/// get track tree data
	/// </summary>
	/// <returns></returns>
	[RequiresUnreferencedCode("Call BuildTrack")]
	public ReadOnlyCollection<TalkTrack> GetAllTracks()
	{
		var bins = GetAllTracksBin();
		var list = bins
			.Select(BuildTrack)
			.ToList()
			.AsReadOnly()
			;
		return list;
	}

	[RequiresUnreferencedCode("Calls LibSasara.VoiSona.Model.Talk.TstPrj.SetAttrIfPossible(ReadOnlySpan<Byte>, String, Tree, VoiSonaValueType)")]
	private static TalkTrack BuildTrack(ReadOnlyMemory<byte> v)
	{
		var track = new TalkTrack();

		//attributes
		SetAttrIfPossible(v.Span, "name", track, VoiSonaValueType.String);
		SetAttrIfPossible(v.Span, "volume", track, VoiSonaValueType.Double);
		SetAttrIfPossible(v.Span, "pan", track, VoiSonaValueType.Double);

		//voice
		var hasVoice = BinaryUtil
			.TryFindChild(v.Span, nameof(Voice), out var voice);
		if (hasVoice)
		{
			var s = voice;
			BinaryUtil.TryFindAttribute(s, "speaker", out var speaker);
			BinaryUtil.TryFindAttribute(s, "name", out var name);
			BinaryUtil.TryFindAttribute(s, "version", out var version);
			track.Children.Add(new Voice(speaker.Data, name.Data, version.Data));
		}

		//contents
		var hasContents = BinaryUtil
			.TryFindCollection(
				v.Span,
				"Contents",
				nameof(Utterance),
				out var utterances);
		if (hasContents)
		{
			var contents = new Tree(
				"Contents",
				true);

			BuildContentsTree(utterances, contents);
			track.Children.Add(contents);
		}
		return track;
	}

	private static readonly Dictionary<string, VoiSonaValueType> utteranceChildPairs = new(StringComparer.Ordinal)
	{
		{"PhonemeOriginalDuration",VoiSonaValueType.String},
		{"FrameStyle",VoiSonaValueType.String},
		{"FrameC0",VoiSonaValueType.String},
		{"FrameLogF0",VoiSonaValueType.String},
		{"FrameAlpha",VoiSonaValueType.String},
		{"PhonemeDuration",VoiSonaValueType.String},
		{"SpeedRatio",VoiSonaValueType.String},
		{"C0Shift",VoiSonaValueType.String},
		{"LogF0Shift",VoiSonaValueType.String},
		{"AlphaShift",VoiSonaValueType.String},
		{"LogF0Scale",VoiSonaValueType.String},
	};

	[RequiresUnreferencedCode("Calls LibSasara.VoiSona.Model.Talk.TstPrj.SetValueOnlyChildIfPossible(ReadOnlySpan<Byte>, String, Tree, VoiSonaValueType)")]
	private static void BuildContentsTree(ReadOnlyCollection<ReadOnlyMemory<byte>> utterances, Tree contents)
	{
		for (int i = 0; i < utterances.Count; i++)
		{
			var utterance = utterances[i];
			var hasText = BinaryUtil.TryFindAttribute(utterance.Span, "text", out var text);
			var hasTsml = BinaryUtil.TryFindAttribute(utterance.Span, "tsml", out var tsml);
			var hasStart = BinaryUtil.TryFindAttribute(utterance.Span, "start", out var start);
			var hasDisable = BinaryUtil.TryFindAttribute(utterance.Span, "disable", out var disable);

			var u = new Utterance(
				hasText ? text.Data : null,
				hasTsml ? tsml.Data : null,
				hasStart ? start.Data : null,
				hasDisable ? disable.Data : null
			);

			foreach(var p in utteranceChildPairs)
			{
				SetValueOnlyChildIfPossible(
					utterance.Span,
					p.Key,
					u,
					p.Value);
			}
			contents.Children.Add(u);
		}
	}

	[RequiresUnreferencedCode("Calls LibSasara.VoiSona.Model.Tree.AddAttribute(String, dynamic, VoiSonaValueType?)")]
	static void SetAttrIfPossible(
		ReadOnlySpan<byte> source,
		string name,
		Tree target,
		VoiSonaValueType type
	)
	{
		var hasAttr = BinaryUtil
			.TryFindAttribute(source, name, out var data);
		if (hasAttr)
		{
			target.AddAttribute(name, data.Data, type);
		}
	}

	[RequiresUnreferencedCode("Calls LibSasara.VoiSona.Model.Talk.TstPrj.SetValueChild(ReadOnlySpan<Byte>, String, Tree, VoiSonaValueType, String)")]
	static void SetValueOnlyChildIfPossible(
		ReadOnlySpan<byte> source,
		string name,
		Tree target,
		VoiSonaValueType type
	)
	{
		SetValueChild(source, name, target, type, "values");
	}

	[RequiresUnreferencedCode("Calls LibSasara.VoiSona.Model.Talk.TstPrj.SetValueChild(ReadOnlySpan<Byte>, String, Tree, VoiSonaValueType, String)")]
	static void SetSingleValOnlyChildIfPossible(
		ReadOnlySpan<byte> source,
		string name,
		Tree target,
		VoiSonaValueType type
	)
	{
		SetValueChild(source, name, target, type, "value");
	}

	[RequiresUnreferencedCode("Calls LibSasara.VoiSona.Model.Talk.TstPrj.SetAttrIfPossible(ReadOnlySpan<Byte>, String, Tree, VoiSonaValueType)")]
	static void SetValueChild(
		ReadOnlySpan<byte> source,
		string name,
		Tree target,
		VoiSonaValueType type,
		string attrName
	)
	{
		var hasChild =
			BinaryUtil.TryFindChild(source, name, out var child);
		if (!hasChild) return;

		var tree = new Tree(name);
		SetAttrIfPossible(child, attrName, tree, type);
		target.Children.Add(tree);
	}
}