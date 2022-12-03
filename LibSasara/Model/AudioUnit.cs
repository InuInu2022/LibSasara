using System.Xml.Linq;

namespace LibSasara.Model;

/// <summary>
/// オーティオUnit管理用クラス
/// </summary>
public class AudioUnit : UnitBase
{
	/// <inheritdoc/>
	public override Category Category { get; } = Category.OuterAudio;

	/// <inheritdoc/>
	public AudioUnit(XElement elem, CeVIOFileBase root)
		: base(elem, root)
	{
	}
}
