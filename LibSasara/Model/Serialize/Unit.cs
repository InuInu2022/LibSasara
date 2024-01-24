using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Xml.Serialization;

namespace LibSasara.Model.Serialize;

/// <summary>
/// a
/// </summary>
public interface IUnit: IHasVersion, IHasId
{
	/// <summary>
	/// a
	/// </summary>
	Category Category { get; set; }
	/// <summary>
	/// b
	/// </summary>
	Guid Group { get; set; }
	/// <summary>
	///
	/// </summary>
	TimeSpan? StartTime { get; set; }
	/// <summary>
	///
	/// </summary>
	string? StartTimeString { get; set; }
	/// <summary>
	///
	/// </summary>
	TimeSpan? Duration { get; set; }
	/// <summary>
	///
	/// </summary>
	string? DurationString { get; set; }
	/// <summary>
	///
	/// </summary>
	string? CastId { get; set; }
	/// <summary>
	///
	/// </summary>
	string? Language { get; set; }
}

/// <summary>
/// ユニット
/// </summary>
public partial record Unit : IUnit
{
	/// <summary>
	/// ユニットのカテゴリ
	/// </summary>
	[XmlAttribute]
	public Category Category { get; set; }

	/// <summary>
	/// 所属トラックを指定するID
	/// </summary>
	[XmlAttribute]
	public Guid Group { get; set; }

	/// <summary>
	/// 開始時間
	/// </summary>
	[XmlIgnore]
	[SuppressMessage("","MA0011")]
	public TimeSpan? StartTime
	{
		get => TimeSpan.Parse(StartTimeString);
		set => StartTimeString = value.ToString();
	}

	/// <summary>
	/// <inheritdoc cref="StartTime"/>
	/// Please use <see cref="StartTime" />.
	/// </summary>
	/// <seealso cref="StartTime"/>
	[XmlAttribute(nameof(StartTime))]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public string? StartTimeString { get; set; }

	/// <summary>
	/// 長さ
	/// </summary>
	[XmlIgnore]
	[SuppressMessage("","MA0011")]
	public TimeSpan? Duration
	{
		get => TimeSpan.Parse(DurationString);
		set { DurationString = value.ToString(); }
	}

	/// <summary>
	/// <inheritdoc cref="Duration"/>
	/// Please use <see cref="Duration" />.
	/// </summary>
	/// <seealso cref="Duration"/>
	[XmlAttribute(nameof(Duration))]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public string? DurationString { get; set; }

	/// <summary>
	/// 指定ボイスライブラリのキャストのID
	/// </summary>
	[XmlAttribute]
	public string? CastId { get; set; }

	/// <summary>
	/// 言語
	/// </summary>
	[XmlAttribute]
	public string? Language { get; set; }

	/// <inheritdoc/>
	[XmlIgnore]
	public Version? Version
	{
		get => new(VersionString);
		set { VersionString = value?.ToString(); }
	}

	/// <inheritdoc/>
	[XmlAttribute("Version")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public string? VersionString { get; set; }

	/// <inheritdoc/>
	[XmlAttribute]
	public string? Id { get; set; }
}

/// <summary>
/// talk unit
/// </summary>
public partial record Unit
{
	/*
	/// <inheritdoc cref="Unit.Category"/>
	[XmlAttribute]
	new public Category Category { get; set; }
	*/

	/// <summary>
	/// セリフ文字列。トークの場合のみ。
	/// </summary>
	[XmlAttribute]
	public string Text { get; set; } = string.Empty;

	/// <summary>
	/// BASE64で記録された調声バイナリメタデータ
	/// </summary>
	[XmlElement(IsNullable = false)]
	public string? Metadata { get; set; }

	/// <summary>
	/// 英語調声データ（英語ボイスのみ）
	/// </summary>
	[XmlElement]
	public Metadata_EN? Metadata_EN { get; set; }

	/// <summary>
	/// キャストのコンディション・感情設定
	/// </summary>
	[XmlArray]
	[XmlArrayItem]
	public Direction<Component>? Direction { get; set; }

	/// <summary>
	///<inheritdoc cref="Direction"/>
	/// </summary>
	[XmlIgnore]
	public bool DirectionSpecified => Direction is not null;

	/// <summary>
	/// 音素一覧
	/// </summary>
	[XmlElement]
	public Phonemes? Phonemes { get; set; }

	/// <summary>
	/// 合成結果のキャッシュのキー(CeVIO AIのみ)
	/// </summary>
	[XmlAttribute]
	public string? SnapShot { get; set; }
}

/// <summary>
/// 英語調声データ
/// </summary>
public record Metadata_EN
{
	/// <summary>
	/// 調整前フレーズ
	/// </summary>
	[XmlArray]
    [XmlArrayItem("acoustic_phrase")]
	public List<AcousticPhrase>? Base { get; set; }

	/// <summary>
	/// アクセント調整済みフレーズ
	/// </summary>
	[XmlArray]
    [XmlArrayItem("acoustic_phrase")]
	public List<AcousticPhrase>? Edited { get; set; }
}

/// <summary>
/// フレーズ（文節）単位データ
/// </summary>
public record AcousticPhrase
{
	/// <summary>
	/// 単語リスト
	/// </summary>
	[XmlArray]
    [XmlArrayItem("word")]
	public List<Word>? Words{ get; set; }
}

/// <summary>
/// 単語
/// </summary>
public record Word
{
	/// <summary>
	/// 音素発音表記（アクセント無し）
	/// </summary>
	[XmlAttribute]
	public string? Phoneme { get; set; }
	/// <summary>
	/// アクセント付き発音表記
	/// </summary>
	[XmlAttribute]
	public string? Pronounciation { get; set; }
	/// <summary>
	/// 単語のフレーズ内位置
	/// </summary>
	[XmlAttribute]
	public string? Pos { get; set; }
	/// <summary>
	/// 単語文字列。
	/// </summary>
	[XmlText]
	public string? Text { get; set; }
}

/// <summary>
/// 音素情報一覧
/// </summary>
public record Phonemes
{
	/// <summary>
	/// 音素の一覧
	/// </summary>
	[XmlArray]
    [XmlArrayItem]
	public List<Phoneme>? Phoneme { get; set; }
}

/// <summary>
/// 音素ごとの調声情報
/// </summary>
public record Phoneme
{
	/// <summary>
	/// 音素テキスト
	/// </summary>
	[XmlAttribute]
	public string? Data { get; set; }
	/// <summary>
	/// 大きさ。VOL
	/// </summary>
	[XmlAttribute]
	public double Volume { get; set; }
	/// <summary>
	/// 速さ。TMG
	/// </summary>
	[XmlAttribute]
	public double Speed { get; set; }
	/// <summary>
	/// 高さ。PIT
	/// </summary>
	[XmlAttribute]
	public double Tone { get; set; }
}

/// <summary>
/// コンディション・感情設定
/// </summary>
public class Direction<T>: List<T>
{
	/// <summary>
	/// 大きさ
	/// </summary>
	[XmlAttribute]
	public double Volume { get; set; } = 0;
	/// <summary>
	/// 速さ
	/// </summary>
	[XmlAttribute]
	public double Speed { get; set; } = 1.0;
	/// <summary>
	/// 高さ（6.00 ～ -6.00）
	/// </summary>
	[XmlAttribute]
	public double Tone { get; set; } = 0;
	/// <summary>
	/// 声質
	/// </summary>
	[XmlAttribute]
	public double Alpha { get; set; } = 0.55;
	/// <summary>
	/// 抑揚
	/// </summary>
	[XmlAttribute]
	public double LogF0Scale { get; set; } = 1.0f;

	//public List<Component>? Component { get; set; }
}

/// <summary>
/// 感情オブジェクト
/// </summary>
public record Component
{
	/// <summary>
	/// 感情名。CSは共通、AIはキャスト固有
	/// </summary>
	[XmlAttribute]
	public string? Name { get; set; }

	/// <summary>
	/// 設定値。0.00 ~ 1.00
	/// </summary>
	[XmlAttribute]
	public double Value { get; set; }
}