using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace LibSasara.Model.Serialize;

/// <summary>
/// Unit for song track
/// </summary>
public partial record Unit
{
	/*
	/// <inheritdoc cref="Unit.Category"/>
	[XmlAttribute]
	new public Category Category { get; set; }
	*/

	/// <summary>
	/// <inheritdoc cref="Song"/>
	/// </summary>
	[XmlElement]
	public Song? Song { get; set; }

	///<inheritdoc cref="Song"/>
	[XmlIgnore]
	[Browsable(false)]
	public bool SongSpecified => Song is not null;
}

/// <summary>
/// ソング情報
/// </summary>
public record Song: IHasVersion
{
	/// <summary>
	/// テンポ情報リスト
	/// </summary>
	[XmlArray]
	[XmlArrayItem]
	public List<Sound>? Tempo { get; set; }

	/// <summary>
	/// Beat情報リスト
	/// </summary>
	[XmlArray]
	[XmlArrayItem]
	public List<Time>? Beat { get; set; }

	/// <summary>
	/// スコア情報リスト
	/// </summary>
	[XmlArray]
	[XmlArrayItem(typeof(ScoreObject))]
	[XmlArrayItem(typeof(Key))]
	[XmlArrayItem(typeof(Dynamics))]
	[XmlArrayItem(typeof(Note))]
	public List<ScoreObject>? Score { get; set; }

	/// <summary>
	/// 調声データ
	/// </summary>
	[XmlElement]
	public Parameter? Parameter { get; set; }

	///<inheritdoc/>
	[XmlIgnore]
	public Version? Version {
		get=> new(VersionString);
		set { VersionString = value?.ToString(); }
	}
	/// <inheritdoc cref="Version"/>
    [XmlAttribute(nameof(Version))]
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
    public string? VersionString { get; set; }
}

/// <inheritdoc cref="Song.Parameter"/>
public record Parameter
{
	/// <summary>
	/// TMG
	/// </summary>
	//[XmlArray]
	//[XmlArrayItem]
	[XmlElement]
	public Parameters? Timing { get; set; }

	/// <summary>
	/// PIT
	/// </summary>
	//[XmlArray]
	//[XmlArrayItem(typeof(TuneData))]
	//[XmlArrayItem("Data", typeof(Data))]
	//[XmlArrayItem("NoData", typeof(NoData))]
	[XmlElement]
	public Parameters? LogF0 { get; set; }

	/// <summary>
	/// VOL
	/// </summary>
	[XmlElement]
	public Parameters? C0 { get; set; }

	/// <summary>
	/// VIA
	/// </summary>
	//[XmlArray]
	//[XmlArrayItem]
	[XmlElement]
	public Parameters? VibAmp { get; set; }

	/// <summary>
	/// VIF
	/// </summary>
	//[XmlArray]
	//[XmlArrayItem]
	[XmlElement]
	public Parameters? VibFrq { get; set; }

	/// <summary>
	/// ALP
	/// </summary>
	//[XmlArray]
	//[XmlArrayItem]
	[XmlElement]
	public Parameters? Alpha { get; set; }

	/// <summary>
	/// HUS (VoiSona only)
	/// </summary>
	[XmlElement]
	public Parameters? Husky { get; set; }
}

/// <summary>
/// tickの時刻指定を持つ
/// </summary>
public abstract record ClockObject
{
	/// <summary>
	/// 切替時刻(単位：tick)
	/// </summary>
	[XmlAttribute]
	public int Clock { get; set; }
}

/// <summary>
/// テンポ変更情報
/// </summary>
public record Sound : ClockObject
{
	/// <summary>
	/// テンポ指定
	/// </summary>
	[XmlAttribute]
	public double Tempo { get; set; }
}

/// <summary>
/// ビート(拍子)切替指定
/// </summary>
public record Time : ClockObject
{
	/// <summary>
	/// 1小節中の拍数
	/// </summary>
	[XmlAttribute]
	public uint Beats { get; set; }

	/// <summary>
	/// 音価（1拍の長さの基準）
	/// </summary>
	[XmlAttribute]
	public uint BeatType { get; set; }
}

/// <summary>
/// スコア情報の抽象クラス
/// </summary>
public abstract record ScoreObject : ClockObject
{
}

/// <summary>
/// キー(調号)指定・変更
/// </summary>
public record Key : ScoreObject
{
	/// <summary>
	/// Fifths
	/// </summary>
	[XmlAttribute]
	public int Fifths { get; set; }
	/// <summary>
	/// Mode
	/// </summary>
	[XmlAttribute]
	public int Mode { get; set; }
}

/// <summary>
/// ダイナミクス（強弱）指定・変更
/// </summary>
public record Dynamics : ScoreObject
{
	/// <summary>
	/// 強弱指定の値
	/// </summary>
	[XmlIgnore]
	public Model.Dynamics Value {
		get => (Model.Dynamics)Enum.ToObject(typeof(Model.Dynamics), ValueInt);
		set { ValueInt = (int)value; }
	}

	///<inheritdoc cref="Value"/>
	[XmlAttribute(nameof(Value))]
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public int ValueInt { get; set; }
		= (int)Model.Dynamics.N;
}
