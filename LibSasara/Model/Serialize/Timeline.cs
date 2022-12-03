using System;
using System.Xml.Serialization;

namespace LibSasara.Model.Serialize;

/// <summary>
/// エディタのタイムラインの設定
/// </summary>
public record Timeline
{
	/// <summary>
	/// 分割線
	/// </summary>
	[XmlAttribute]
	public string? Partition { get; set; }

	/// <summary>
	/// 位置
	/// </summary>
	[XmlAttribute]

	public string? CurrentPosition { get; set; }

	/// <summary>
	/// <inheritdoc cref="ViewScale"/>
	/// </summary>
	[XmlElement]
	public ViewScale? ViewScale { get; set; }
}

/// <summary>
/// 拡大率
/// </summary>
public record ViewScale
{
	/// <summary>
	/// 水平
	/// </summary>
	[XmlAttribute]
	public decimal Horizontal { get; set; }
	/// <summary>
	/// 縦
	/// </summary>
	[XmlAttribute]
	public decimal Vertical { get; set; }
}