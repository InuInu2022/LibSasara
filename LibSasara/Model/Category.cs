using System.Xml.Serialization;

namespace LibSasara.Model;

/// <summary>
/// ユニットの種類
/// </summary>
public enum Category
{
	/// <summary>
	/// トーク
	/// </summary>
	[XmlEnum]
	TextVocal,

	/// <summary>
	/// ソング
	/// </summary>
	[XmlEnum]
	SingerSong,

	/// <summary>
	/// オーディオ
	/// </summary>
	[XmlEnum]
	OuterAudio,
}