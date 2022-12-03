using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace LibSasara.Model.Serialize;

/// <summary>
/// トラック一覧
/// </summary>
public class Groups<T>: List<T>
{
	//public List<Group>? Group { get; set; }

	/// <summary>
	/// 選択中（アクティブ中）のトラックID
	/// </summary>
	[XmlAttribute]
	public Guid ActiveGroup { get; set; }
}

/// <summary>
/// トラック情報
/// </summary>
public record Group : IHasVersion, IHasCategory
{
	/// <summary>
	/// トラックID
	/// </summary>
	[XmlAttribute]
	public Guid Id { get;set; }
	///<inheritdoc/>
	[XmlAttribute]
	public Category Category {get;set; }
	///<inheritdoc/>
	[XmlIgnore]
	public Version? Version {
		get=> new(VersionString);
		set { VersionString = value?.ToString(); }
	}
	/// <inheritdoc cref="Version"/>
    [XmlAttribute("Version")]
    public string? VersionString { get; set; }

	/// <summary>
	/// トラック名
	/// </summary>
	[XmlAttribute]
	public string? Name { get; set; }
	/// <summary>
	/// トラックボリューム
	/// </summary>
	[XmlAttribute]
	public double Volume { get; set; }
	/// <summary>
	/// トラックの背景色
	/// ※指定不可
	/// </summary>
	[XmlAttribute]
	public string? Color { get; set; }
	/// <summary>
	/// トラックの左右パン
	/// </summary>
	[XmlAttribute]
	public double Pan { get; set; }
	/// <summary>
	/// トラックのソロ再生
	/// </summary>
	[XmlAttribute]
	public bool IsSolo { get; set; }
	/// <summary>
	/// トラックのミュート
	/// </summary>
	[XmlAttribute]
	public bool IsMuted { get; set; }
	/// <summary>
	/// トラックキャストのID。
	/// 複数キャストのトークトラック・オーディオトラックは"Mixed"
	/// </summary>
	[XmlAttribute]
	public string? CastId { get; set; }
	/// <summary>
	/// トラックの言語。
	/// ※ボイス複数の場合は最初のキャストの言語？
	/// </summary>
	[XmlAttribute]
	public string? Language { get; set; }
	/// <summary>
	/// トラックの固定時、キャッシュのID
	/// </summary>
	[XmlAttribute]
	public string? SnapShot { get; set; }
}