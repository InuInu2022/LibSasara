using System.Xml.Serialization;

namespace LibSasara.Model.Serialize;

/// <summary>
/// 調整データ
/// </summary>
public record Data: TuneData, ITuneData
{
	/// <summary>
	/// 調声の値
	/// </summary>
	[XmlText]
	public decimal Value { get; set; }
}
