using System.Xml.Serialization;

namespace LibSasara.Model;

/// <summary>
/// 強弱指定
/// </summary>
public enum Dynamics
{
	/// <summary>
	/// ピアニッシモ・ピアニッシモ
	/// </summary>
	[XmlEnum]
	PP_PP = 0,

	/// <summary>
	/// ピアニッシッシモ
	/// </summary>
	[XmlEnum]
	PPP = 1,

	/// <summary>
	/// ピアニッシモ
	/// </summary>
	[XmlEnum]
	PP = 2,

	/// <summary>
	/// ピアノ
	/// </summary>
	[XmlEnum]
	P = 3,

	/// <summary>
	/// メゾピアノ
	/// </summary>
	[XmlEnum]
	MP = 4,

	/// <summary>
	/// ノーマル
	/// </summary>
	[XmlEnum]
	N = 5,

	/// <summary>
	/// メゾフォルテ
	/// </summary>
	[XmlEnum]
	MF = 6,

	/// <summary>
	/// フォルテ
	/// </summary>
	[XmlEnum]
	F = 7,

	/// <summary>
	/// フォルティシモ
	/// </summary>
	[XmlEnum]
	FF = 8,

	/// <summary>
	/// フォルティティシモ
	/// </summary>
	[XmlEnum]
	FFF = 9,

	/// <summary>
	/// フォルティシモ・フォルティシモ
	/// </summary>
	[XmlEnum]
	FF_FF = 10,
}