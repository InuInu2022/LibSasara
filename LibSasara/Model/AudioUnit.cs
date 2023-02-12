using System.Xml.Linq;

namespace LibSasara.Model;

/// <summary>
/// オーティオUnit管理用クラス
/// </summary>
public class AudioUnit : UnitBase
{
	/// <inheritdoc/>
	public override Category Category { get; } = Category.OuterAudio;

	/// <summary>
    /// オーディオファイルへのpath
    /// </summary>
	public string FilePath {
		get => GetUnitAttributeStr(nameof(FilePath));
		set => SetUnitAttribureStr(nameof(FilePath), value);
	}

	/// <inheritdoc/>
	public AudioUnit(XElement elem, CeVIOFileBase root)
		: base(elem, root)
	{
	}
}
