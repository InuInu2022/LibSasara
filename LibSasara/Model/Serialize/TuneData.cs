using System.ComponentModel;
using System.Xml.Serialization;

namespace LibSasara.Model.Serialize;

/// <summary>
/// 調声データ
/// </summary>
public record TuneData : ITuneData
{
	/// <inheritdoc/>
	[XmlAttribute]
	public int Index { get; set; } = -1;

	/// <inheritdoc/>
	[Browsable(false)]
	[XmlIgnore]
	public bool IndexSpecified => Index >= 0;

	/// <inheritdoc/>
	[XmlAttribute]
	public int Repeat { get; set; } = 1;

	/// <inheritdoc/>
	[Browsable(false)]
	[XmlIgnore]
	public bool RepeatSpecified => Repeat > 0;
}

/// <summary>
/// 無効化された調整データ
/// </summary>
public record NoData: TuneData, ITuneData
{
}
