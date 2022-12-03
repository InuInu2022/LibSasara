using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace LibSasara.Model.Serialize;

/// <summary>
/// Project file (.ccs) root
/// </summary>
[XmlRoot("Scenario")]
public record Project : RootBase
{
	/// <summary>
	/// hashcode?
	/// </summary>
	[XmlAttribute]
	public string? Code { get; set; }

	/// <summary>
	/// シーケンス
	/// </summary>
	[XmlElement]
	public Sequence? Sequence { get; set; }
}

/// <summary>
/// シーケンス
/// </summary>
public record Sequence : IHasId
{
	/// <summary>
	/// シーン
	/// </summary>
	[XmlElement]
	public Scene? Scene { get; set; }

	/// <inheritdoc/>
	[XmlAttribute]
	public string? Id { get; set; }
}

/// <summary>
/// シーン
/// </summary>
public record Scene : SceneBase
{
}