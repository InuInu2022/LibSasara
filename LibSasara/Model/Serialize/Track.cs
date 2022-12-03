using System.Xml.Serialization;

namespace LibSasara.Model.Serialize;

/// <summary>
/// Track file (.ccst) root
/// </summary>
[XmlRoot("Definition")]
public record Track : SceneBase, IRoot
{
	/// <summary>
	/// <inheritdoc/>
	/// </summary>
	[XmlElement]
	public Generator? Generation { get; set; }
}
