using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;

namespace LibSasara.Model;

/// <summary>
/// Unit管理用クラス
/// </summary>
/// <remarks>
/// <para>
/// Unit要素（<see cref="SongUnit"/>の場合はソングトラックの楽譜・調整データ全て、<see cref="TalkUnit"/>の場合はセリフ単位）を管理します。
/// シリアライズしたクラスではありません。
/// </para>
/// <para><see cref="RawRoot"/>をはじめとした <c>Raw～</c> で始まるプロパティは元のxmlの<see cref="XElement"/>へのアクセスを提供します。XML要素を直接いじる際に使用します。</para>
/// </remarks>
/// <seealso cref="CeVIOFileBase.AddUnits(System.Collections.Generic.IEnumerable{XElement})"/>
/// <seealso cref="CeVIOFileBase.GetUnits()"/>
public abstract class UnitBase
{
	/// <summary>
	/// ルートのオブジェクト
	/// </summary>
	public XDocument RawRoot => rawElem.Document ?? new();

	/// <summary>
	/// Unitの所属するccs/ccstファイルの管理クラス
	/// </summary>
	public CeVIOFileBase Root { get; }

	/// <summary>
	/// Unitの所属するGroup（エディタ上のトラック）の<see cref="Guid"/>
	/// </summary>
	public Guid Group
	{
		get => new(GetUnitAttributeStr(nameof(Group)));
		set => SetUnitAttribureStr(nameof(Group), value.ToString());
	}

	/// <summary>
	/// Unitの開始時間
	/// </summary>
	/// <seealso cref="Duration"/>
	public TimeSpan StartTime
	{
		get => TimeSpan.Parse(GetUnitAttributeStr(nameof(StartTime)));
		set => SetUnitAttribureStr(nameof(StartTime), value.ToString());
	}

	/// <summary>
	/// Unitの長さ（時間）
	/// </summary>
	/// <seealso cref="StartTime"/>
	public TimeSpan Duration
	{
		get => TimeSpan.Parse(GetUnitAttributeStr(nameof(Duration)));
		set => SetUnitAttribureStr(nameof(Duration), value.ToString());
	}

	/// <summary>
	/// Unitの終了時間
	/// </summary>
    /// <seealso cref="Duration"/>
	public TimeSpan EndTime
		=> StartTime.Add(Duration);

	/// <summary>
	/// 内部管理用 Unit要素
	/// </summary>
    [Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	[SuppressMessage("","CA1051")]
	protected internal XElement rawElem;

	/// <summary>
	/// Unit category
	/// </summary>
    /// <seealso cref="Model.Category"/>
	public virtual Category Category { get; }

	/// <summary>
	/// Unit管理用クラス コンストラクタ
	/// </summary>
	/// <param name="elem"></param>
	/// <param name="root">Unit所属ファイルの管理クラス</param>
    /// <seealso cref="Builder.IUnitBuilder{TUnit, TBuilder}"/>
	protected UnitBase(XElement elem, CeVIOFileBase root)
	{
		if (elem?.Name.LocalName is not "Unit")
		{
			throw new ArgumentException($"parameter {elem} is not a Unit element.", nameof(elem));
		}

		rawElem = elem;
		Root = root;
	}

	internal string GetUnitAttributeStr(string attr)
	{
		return rawElem.Attribute(attr)?.Value ?? string.Empty;
	}

	internal void SetUnitAttribureStr(string attr, string value)
	{
		rawElem.Attribute(attr)?.SetValue(value);
	}

	/// <summary>
    /// <see cref="int"/>のXML属性を取得
    /// </summary>
    /// <param name="v">属性があるXML属性</param>
    /// <param name="attr">属性の名前</param>
    /// <param name="defVal">属性値がないときに受けとる初期値</param>
    /// <returns>属性の値</returns>
	protected internal static int GetAttrInt(XElement v, string attr, int defVal = 0)
		=>LibSasaraUtil.ConvertInt(
			v?.Attribute(attr)?.Value,
			defVal
		);

	/// <summary>
    /// <see cref="bool"/>のXML属性を取得
    /// </summary>
    /// <param name="v">属性があるXML属性</param>
    /// <param name="attr">属性の名前</param>
    /// <returns>属性の値</returns>
	protected internal static bool GetAttrBool(XElement v, string attr)
		=>LibSasaraUtil.ConvertBool(
			v?.Attribute(attr)?.Value
		);

	/// <summary>
	/// <see cref="decimal"/>のXML属性を取得
	/// </summary>
	/// <param name="v">属性があるXML属性</param>
	/// <param name="attr">属性の名前</param>
	/// <param name="defVal">属性値がないときに受けとる初期値</param>
	/// <returns>属性の値</returns>
	protected internal static decimal GetAttrDecimal(
		XElement v, string attr, decimal defVal = 0.00m)
		=>LibSasaraUtil.ConvertDecimal(
			v?.Attribute(attr)?.Value,
			defVal
		);

	/// <summary>
    /// <typeparamref name="T"/> 型のXML属性を設定
    /// </summary>
    /// <typeparam name="T">属性値の型</typeparam>
    /// <param name="v"></param>
    /// <param name="attr"></param>
    /// <param name="value"></param>
	protected internal static void SetAttr<T>(
		XElement v,
		string attr,
		T value
	) => v?.SetAttributeValue(attr, value);
}
