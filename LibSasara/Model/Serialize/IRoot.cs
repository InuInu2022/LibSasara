using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace LibSasara.Model.Serialize;

/// <summary>
/// Common root
/// </summary>
public interface IRoot
{
    /// <summary>
	/// generator of this file
	/// </summary>
	public Generator? Generation {get;set;}
}

///<inheritdoc cref="IRoot"/>
public abstract record RootBase: IRoot
{
	/// <summary>
	/// <inheritdoc/>
	/// </summary>
	[XmlElement]
	public Generator? Generation { get; set; }
}

/// <summary>
/// 便利関数
/// </summary>
public static class RootExt{
	/// <summary>
	/// ボイスアイテム<see cref="Unit"/>一覧を返す
	/// </summary>
	/// <remarks><see cref="Project"/>と<see cref="Track"/>で<see cref="Unit"/>一覧へのパスが異なるので共通化。</remarks>
	/// <returns>ボイスアイテム一覧</returns>
	public static List<Unit> GetUnits(
		this IRoot root
	){
		var units = new List<Unit>();
		if (root is Project p)
		{
			return p?.Sequence?.Scene?.Units ?? units;
		}
		else if (root is Track t)
		{
			return t?.Units ?? units;
		}

		return units;
	}

	/// <summary>
	/// <inheritdoc cref="RootExt.GetUnits(IRoot)"/>
	/// </summary>
	/// <param name="root"></param>
	/// <param name="category"><see cref="Unit"/>の指定カテゴリ</param>
	/// <returns>指定カテゴリの<see cref="Unit"/>一覧</returns>
	public static List<Unit> GetUnits(
		this IRoot root,
		Category category
	) => root.GetUnits()
		.Where(v => v.Category == category)
		.ToList();
}