using System;
using System.Linq;
using LibSasara.VoiSona.Model;

namespace LibSasara.VoiSona.Util;

/// <summary>
/// Treeに対するユーティリティクラス
/// </summary>
public static class TreeUtil
{
	/// <summary>
	/// 属性がvaluesのみ持ち、Tree中に一つしかない<see cref="Tree.Children"/>の値を返す
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
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
}