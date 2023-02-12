using System;
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
	public string FilePath
	{
		get => GetUnitAttributeStr(nameof(FilePath));
		set => SetUnitAttribureStr(nameof(FilePath), value);
	}

	/// <inheritdoc/>
	public AudioUnit(XElement elem, CeVIOFileBase root)
		: base(elem, root)
	{
	}

	/// <summary>
	/// AudioのUnit要素生成
	/// </summary>
	/// <remarks>
	/// <para>
	/// AudioのUnit要素の<see cref="XElement"/>を生成します。
	/// </para>
	/// <para>
	/// 生成するだけで<see cref="CeVIOFileBase"/>には紐付けません。
	/// <see cref="Builder.AudioUnitBuilder"/>も活用してください。
	/// </para>
	/// </remarks>
	/// <param name="StartTime"></param>
	/// <param name="Duration"></param>
	/// <param name="FilePath"></param>
	/// <param name="Group"></param>
	/// <returns>AudioのUnit要素の<see cref="XElement"/></returns>
	public static XElement CreateAudioUnitRaw
	(
		TimeSpan StartTime,
		TimeSpan Duration,
		string FilePath,
		Guid? Group = null
	)
	{
		XAttribute[] attrs = {
			new("Version","1.0"),	//
			new("Id",""),			//
			new("Category", nameof(Category.OuterAudio)),
			new(nameof(StartTime),StartTime.ToString("c")),
			new(nameof(Duration),Duration.ToString("c")),
			new(nameof(FilePath), FilePath),
			new(
				nameof(Group),
				Group is null ?
					Guid.NewGuid() :
					Group.ToString()),
		};

		var elem = new XElement("Unit", attrs);
		return elem;
	}
}
