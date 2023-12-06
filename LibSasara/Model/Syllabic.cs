using System.Xml.Serialization;

namespace LibSasara.Model;

/// <summary>
/// ノートの音節の位置
/// </summary>
#pragma warning disable CA1008 // 列挙型は 0 値を含んでいなければなりません
public enum Syllabic
#pragma warning restore CA1008 // 列挙型は 0 値を含んでいなければなりません
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
	End = 3,
}
