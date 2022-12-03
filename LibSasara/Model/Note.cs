using System;
using System.ComponentModel;
using System.Xml.Serialization;
using LibSasara.Model.Serialize;

namespace LibSasara.Model;

/// <summary>
/// 音符
/// </summary>
public record Note : ScoreObject
{
	/// <summary>
	/// 音程の音階。
	/// </summary>
	[XmlAttribute]
	public int PitchStep { get; set; }

	/// <summary>
	/// 音程のオクターブ。
	/// </summary>
	/// <example>
	/// C4のノートなら「4」
	/// </example>
	[XmlAttribute]
	public int PitchOctave { get; set; }

	/// <summary>
	/// 音符の長さ。単位はtick
	/// </summary>
	/// <remarks>
	/// 960 tick = 四分音符
	/// </remarks>
	[XmlAttribute]
	public int Duration { get; set; }

	/// <summary>
	/// 歌詞
	/// </summary>
	[XmlAttribute]
	public string? Lyric { get; set; }

	/// <summary>
	/// 置き換えた音素
	/// </summary>
	[XmlAttribute]
	public string? Phonetic { get; set; }

	/// <summary>
	/// ドレミ発音するかどうか
	/// </summary>
	[XmlAttribute]
	public bool DoReMi { get; set; }

	/// <inheritdoc/>
	[XmlIgnore]
	[Browsable(false)]
	public bool DoReMiSpecified => DoReMi;

	/// <summary>
	/// ブレス指定
	/// </summary>
	[XmlAttribute]
	public bool Breath { get; set; }

	/// <inheritdoc/>
	[XmlIgnore]
	[Browsable(false)]
	public bool BreathSpecified => Breath;

	/// <summary>
	/// アクセント
	/// </summary>
	[XmlAttribute]
	public bool Accent { get; set; }

	/// <inheritdoc/>
	[XmlIgnore]
	[Browsable(false)]
	public bool AccentSpecified => Accent;

	/// <summary>
	/// スタッカート
	/// </summary>
	[XmlAttribute]
	public bool Staccato { get; set; }

	/// <inheritdoc/>
	[XmlIgnore]
	[Browsable(false)]
	public bool StaccatoSpecified => Staccato;

	/// <summary>
	/// スラー開始
	/// </summary>
	[XmlAttribute]
	public bool SlurStart { get; set; }

	/// <inheritdoc/>
	[XmlIgnore]
	[Browsable(false)]
	public bool SlurStartSpecified => SlurStart;

	/// <summary>
	/// スラー終了
	/// </summary>
	[XmlAttribute]
	public bool SlurStop { get; set; }

	/// <inheritdoc/>
	[XmlIgnore]
	[Browsable(false)]
	public bool SlurStopSpecified => SlurStop;

	/// <summary>
	/// 音節
	/// </summary>
	/// <remarks>
	/// <list type="bullet">
	/// <item><see cref="Syllabic.Begin"/> 1 = 音節開始</item>
	/// <item><see cref="Syllabic.Middle"/> 2 = 音節途中</item>
	/// <item><see cref="Syllabic.End"/> 3 = 音節終了</item>
	/// </list>
	/// English only?
	/// </remarks>
	[XmlIgnore]
	public Syllabic Syllabic {
		get => (Syllabic) Enum.ToObject(typeof(Syllabic), SyllabicInt);
		set { SyllabicInt = (int)value; }
	}

	/// <summary>
	/// <inheritdoc cref="Syllabic"/>
	/// </summary>
	[XmlAttribute(nameof(Syllabic))]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public int SyllabicInt { get; set; }

	/// <inheritdoc/>
	[XmlIgnore]
	[Browsable(false)]
	public bool SyllabicSpecified
		=> Syllabic != Syllabic.Begin;
}
