using System.Xml.Serialization;

namespace LibSasara.Model.Serialize;

/// <summary>
/// Idを持つ
/// </summary>
public interface IHasId
{
	/// <summary>
	/// Identifier
	/// </summary>
	[XmlAttribute]
	public string? Id { get; set; }
}