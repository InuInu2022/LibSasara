using System.Xml.Serialization;

namespace LibSasara.Model;

/// <summary>
/// ノートの音節の位置
/// </summary>
public enum Syllabic
{
	/// <summary>
	/// 音節開始、後ろのノートに音節続く
	/// </summary>
	[XmlEnum]
	Begin = 1,

	/// <summary>
	/// 音節途中、前後のノートで音節としてつながる
	/// </summary>
	[XmlEnum]
	Middle = 2,

	/// <summary>
	/// 音節終了、前のノートからの音節がここで終わる
	/// </summary>
	[XmlEnum]
	End = 3
}
