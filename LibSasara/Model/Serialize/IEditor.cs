using System.Xml.Serialization;

namespace LibSasara.Model.Serialize;

/// <summary>
/// エディタの表示復元設定用（共通）
/// </summary>
public interface IEditor
{
	/// <summary>
	/// separator position
	/// </summary>
	[XmlAttribute]
	public int Partition { get; set; }
}
/// <summary>
/// トークエディタの表示復元設定
/// TODO: 詳細
/// </summary>
public record TalkEditor : IEditor
{
	///<inheritdoc />
	[XmlAttribute]
	public int Partition { get;set; }

	///<inheritdoc />
	[XmlElement]
	public TalkEditorExtension? Extension { get; set; }
}

/// <summary>
/// トークエディタの表示復元設定拡張
/// </summary>
public class TalkEditorExtension
{
	///<inheritdoc />
	[XmlAttribute]
	public string? VerticalRatio { get; set; }
}

/// <summary>
/// ソングエディタの表示復元設定
/// TODO: 詳細
/// </summary>
public record SongEditor : IEditor
{
	///<inheritdoc />
	[XmlAttribute]
	public int Partition { get; set; }

	///<inheritdoc />
	[XmlAttribute]
	public int Quantize { get; set; }

	///<inheritdoc />
	[XmlAttribute]
	public int Mode { get; set; }

	///<inheritdoc />
	[XmlAttribute]
	public int EditingTool { get; set; }

	///<inheritdoc />
	[XmlElement]
	public ViewScale? ViewScale { get; set; }

	///<inheritdoc />
	[XmlElement]
	public ReferenceState? ReferenceState { get; set; }
}

/// <summary>
/// 前後選択していた調整モード
/// </summary>
public class ReferenceState
{
	///<inheritdoc />
	[XmlAttribute]
	public string Current { get; set; } = "None";

	///<inheritdoc />
	[XmlAttribute]
	public string Previous { get; set; } = "None";
}