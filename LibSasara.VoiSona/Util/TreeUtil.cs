using System;
using System.Linq;
using LibSasara.VoiSona.Model;

namespace LibSasara.VoiSona.Util;

/// <summary>
/// <see cref="Tree"/>に対するユーティリティクラス
/// </summary>
/// <seealso cref="Tree"/>
public static class TreeUtil
{
	/// <summary>
	/// 属性がvaluesのみ持ち、Tree中に一つしかない<see cref="Tree.Children"/>の値を返す
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
	/// <seealso cref="SetValuesOnlyChildrenValue{T}(Tree, string, T)"/>
	public static T? GetValuesOnlyChildrenValue<T>(
		Tree tree,
		string childName
	)
	{
		var hasChild = tree
			.Children
			.Exists(v => v.Name == childName);
		return hasChild ?
			tree.Children
				.FirstOrDefault(c => c.Name == childName)
				.Attributes
				.FirstOrDefault(a => a.Key == "values")
				.Value
				?? default(T)
			: default(T)
			;
	}

	/// <summary>
	/// 属性がvaluesのみ持ち、Tree中に一つしかない<see cref="Tree.Children"/>の値をセットする
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="tree"></param>
	/// <param name="childName"></param>
	/// <param name="value"></param>
	/// <seealso cref="GetValuesOnlyChildrenValue{T}(Tree, string)"/>
	public static void SetValuesOnlyChildrenValue<T>(
		Tree tree,
		string childName,
		T value
	)
		where T: notnull
	{
		var hasChild = tree
			.Children
			.Exists(v => v.Name == childName);
		if (hasChild)
		{
			tree.Children
				.FirstOrDefault(c => c.Name == childName)
				.Attributes
				.FirstOrDefault(a => a.Key == "values")
				.Value = value;
		}
		else
		{
			var c = new Tree(childName);
			var h = HeaderUtil.Analysis(value);
			var type = h.Type;
			c.AddAttribute("values", value, type);
			tree.Children.Add(c);
		}
	}
}