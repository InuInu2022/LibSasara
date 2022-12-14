using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace LibSasara.Model;

/// <summary>
/// ソングUnit管理用クラス
/// </summary>
/// <inheritdoc/>
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

	/// <summary>
	/// 生のParameter要素一覧
	/// </summary>
    //TODO: add test
	public List<XElement> RawParameters
	{
		get => RawSong
			.Element("Parameter")
			.Elements()
			.ToList();

		set => RawSong
			.Element("Parameter")
			.SetElementValue("Parameter", value);
	}

	/// <inheritdoc/>
    /// <seealso cref="Builder.SongUnitBuilder"/>
	public SongUnit(XElement elem, CeVIOFileBase root)
		: base(elem, root)
	{
	}

	/// <summary>
	/// SongのUnit要素生成
	/// </summary>
	/// <remarks>
	/// <para>
	/// SongのUnit要素の<see cref="XElement"/>を生成します。
	/// </para>
	/// <para>
	/// 生成するだけで<see cref="CeVIOFileBase"/>には紐付けません。
	/// <see cref="Builder.SongUnitBuilder"/>も活用してください。
	/// </para>
	/// </remarks>
	/// <param name="StartTime"></param>
	/// <param name="Duration"></param>
	/// <param name="CastId"></param>
	/// <param name="Group"></param>
	/// <param name="Language"></param>
	/// <param name="songVersion">Song要素のversion。CS7,AIは <c>1.07</c><br/>see: <seealso cref="SongVersion"/></param>
	/// <param name="tempo"></param>
	/// <param name="beat"></param>
	/// <returns>SongのUnit要素の<see cref="XElement"/></returns>
	/// <seealso cref="Builder.TalkUnitBuilder"/>
	//TODO: Add test
	public static XElement CreateSongUnitRaw
	(
		TimeSpan StartTime,
		TimeSpan Duration,
		string CastId,
		Guid? Group = null,
		string? Language = "Japanese",
		string songVersion = "1.07",
		SortedDictionary<int, int>? tempo = null,
		SortedDictionary<int, (int Beats, int BeatType)>? beat = null
	)
	{
		XAttribute[] attrs = {
			new("Version","1.0"),	//
			new("Id",""),			//
			new("Category", nameof(Category.SingerSong)),
			new(nameof(StartTime),StartTime.ToString("c")),
			new(nameof(Duration),Duration.ToString("c")),
			new(nameof(CastId),CastId),
			new(
				nameof(Group),
				Group is null ?
					Guid.NewGuid() :
					Group.ToString()),
			new(nameof(Language),Language ?? "Japanaese")
		};

		var elem = new XElement("Unit", attrs);
		var songElem = new XElement(
			"Song",
			new XAttribute("Version", songVersion)
		);
		elem.Add(songElem);

		if (tempo is not null)
		{
			var tempos = tempo
				.Select(v =>
				{
					var c = new XElement("Sound");
					c.SetAttributeValue("Clock", v.Key);
					c.SetAttributeValue("Tempo", v.Value);
					return c;
				});
			var tempoElem = new XElement("Tempo");
			tempoElem.Add(tempos);
			elem
				.Add(tempoElem);
		}

		if (beat is not null)
		{
			var beats = beat
				.Select(v =>
				{
					var p = new XElement("Time");
					p.SetAttributeValue("Clock", v.Key);
					p.SetAttributeValue("Beats", v.Value.Beats);
					p.SetAttributeValue("BeatType", v.Value.BeatType);

					return p;
				})
				.ToArray();
			var beatElem = new XElement("Beat");
			beatElem.Add(beats);
			elem
				.Add(beatElem);
		}

		return elem;
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
