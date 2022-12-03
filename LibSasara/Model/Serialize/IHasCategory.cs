using System.Xml.Serialization;

namespace LibSasara.Model.Serialize;

/// <summary>
/// ボイスの種類を指定する
/// </summary>
public interface IHasCategory
{
	/// <summary>
	/// カテゴリ
	/// </summary>
	[XmlElement]
	public Category Category { get; set; }
}