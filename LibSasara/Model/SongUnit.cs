using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace LibSasara.Model;

/// <summary>
/// ソングUnit管理用クラス
/// </summary>
public class SongUnit : UnitBase
{
	/// <inheritdoc/>
	public override Category Category { get; } = Category.SingerSong;

	/// <summary>
	/// Songの生要素
	/// </summary>
	public XElement RawSong
	{
		get => rawElem.Element("Song");

		set => rawElem.Element("Song")
			.SetElementValue("Song", value);
	}

	/// <summary>
	/// ソングフォーマットのバージョン
	/// </summary>
	/// <remarks>
	/// ソングエンジンやボイスのバージョンとは異なります。
	/// <list type="bullet">
	///     <item>
	///        <term>VoiSona</term>
	///        <description>VoiSonaの場合は取得できません</description>
	///     </item>
	/// </list>
	/// </remarks>
	public Version SongVersion
	{
		get
		{
			if (RawSong.HasAttributes
				&& RawSong.Attribute("Version") is not null)
			{
				return new(RawSong.Attribute("Version").Value);
			}

			return new();
		}
	}

	/// <summary>
	/// 曲のテンポ変更リスト
	/// </summary>
	/// <remarks>
	/// - Clock: テンポ変更開始時のtick
	/// - Tempo : テンポ
	/// </remarks>
	public SortedDictionary<int, int> Tempo
	{
		get => new(
			RawSong
				.Element("Tempo")
				.Elements("Sound")
				.ToDictionary(
					v => GetAttrInt(v, "Clock"),
					v => GetAttrInt(v, "Tempo")
				)
		);
	}

	/// <summary>
	/// 曲の拍子変更リスト
	/// </summary>
	public SortedDictionary<int, (int Beats, int BeatType)> Beat
	{
		get => new(
			RawSong
				.Element("Beat")
				.Elements("Time")
				.ToDictionary(
					v => GetAttrInt(v, "Clock"),
					v => (
						Beats: GetAttrInt(v, "Beats"),
						BeatType: GetAttrInt(v, "BeatType")
					)
				)
		);
	}

	/// <summary>
	/// 生のScore要素一覧
	/// </summary>
	public List<XElement> RawScores
	{
		get => RawSong
			.Element("Score")
			.Elements()
			.ToList();

		set => RawSong
			.Element("Score")
			.SetElementValue("Score", value);
	}

	/// <summary>
	/// 音符データのリスト
	/// </summary>
	public List<Note> Notes
	{
		get => RawScores
			.Where(v => v.Name == "Note")
			.Select(v =>
			{
				var n = new Note()
				{
					Clock = GetAttrInt(v, "Clock"),
					PitchStep = GetAttrInt(v, "PitchStep"),
					PitchOctave = GetAttrInt(v, "PitchOctave"),
					Duration = GetAttrInt(v, "Duration"),
					Lyric = v.Attribute("Lyric")?.Value,
					Phonetic = v.Attribute("Phonetic")?.Value,
					DoReMi = GetAttrBool(v, "DoReMi"),
					Breath = GetAttrBool(v, "Breath"),
					SlurStart = GetAttrBool(v, "SlurStart"),
					SlurStop = GetAttrBool(v, "SlurStop"),
					SyllabicInt = GetAttrInt(v, "Syllabic"),
				};
				//n.Phonetic =
				return n;
			})
			.ToList();
	}

	/// <inheritdoc/>
	public SongUnit(XElement elem, CeVIOFileBase root)
		: base(elem, root)
	{
	}

	int GetAttrInt(XElement v, string attr)
		=> SasaraUtil.ConvertInt(
			v.Attribute(attr)?.Value
		);

	bool GetAttrBool(XElement v, string attr)
		=> SasaraUtil.ConvertBool(
			v.Attribute(attr)?.Value
		);
}
