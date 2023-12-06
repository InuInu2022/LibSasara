using System;
using LibSasara.VoiSona.Model;

namespace LibSasara.VoiSona.Util;

/// <summary>
/// <see cref="Tree"/>に対するユーティリティクラス
/// </summary>
/// <seealso cref="Tree"/>
public static class TreeUtil
{
	/// <summary>
	/// 属性が<c>values</c>のみ持ち、Tree中に一つしかない<see cref="Tree.Children"/>の値を返す
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="tree">対象のTree</param>
	/// <param name="childName">子の名前</param>
	/// <returns></returns>
	/// <seealso cref="SetValuesOnlyChildrenValue{T}(Tree, string, T)"/>
	/// <seealso cref="GetValueOnlyChildValue{T}(Tree, string)"/>
	public static T? GetValuesOnlyChildrenValue<T>(
		Tree tree,
		string childName
	)
	{
		if (tree is null)
		{
			throw new ArgumentNullException(nameof(tree));
		}

		if (string.IsNullOrEmpty(childName))
		{
			throw new ArgumentException($"'{nameof(childName)}' を NULL または空にすることはできません。", nameof(childName));
		}

		return GetValueFromChildInternal<T>(tree, childName, "values");
	}

	/// <summary>
	/// 属性がvalueのみ持ち、Tree中に一つしかない<see cref="Tree.Children"/>の値を返す
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="tree"></param>
	/// <param name="childName"></param>
	/// <returns></returns>
	/// <seealso cref="GetValuesOnlyChildrenValue{T}(Tree, string)"/>
	public static T? GetValueOnlyChildValue<T>(
		Tree tree,
		string childName
	)
	{
		if (tree is null)
		{
			throw new ArgumentNullException(nameof(tree));
		}

		if (string.IsNullOrEmpty(childName))
		{
			throw new ArgumentException($"'{nameof(childName)}' を NULL または空にすることはできません。", nameof(childName));
		}

		return GetValueFromChildInternal<T>(tree, childName, "value");
	}

	private static T? GetValueFromChildInternal<T>(
		Tree tree,
		string childName,
		string valueName)
	{
		var hasChild = tree
			.Children
			.Exists(v => string.Equals(v.Name, childName, StringComparison.Ordinal));

		if (!hasChild) return default;

		var value = tree.Children
			.Find(c => string.Equals(c.Name, childName, StringComparison.Ordinal))
			.Attributes
			.Find(a => string.Equals(a.Key, valueName, StringComparison.Ordinal))
			.Value
			?? default(T);
		if(value is null){
			return default;
		}
		if (typeof(T) == typeof(int))
		{
			return SasaraUtil.ConvertInt(value);
		}
		else if (typeof(T) == typeof(decimal))
		{
			return SasaraUtil.ConvertDecimal(value);
		}else if(typeof(T) == typeof(bool)){
			return SasaraUtil.ConvertBool(value);
		}else if(typeof(T) == typeof(string)){
			return (T)(object)value;
		}else{
			throw new NotSupportedException();
		}
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
		if (tree is null)
		{
			throw new ArgumentNullException(nameof(tree));
		}

		if (string.IsNullOrEmpty(childName))
		{
			throw new ArgumentException($"'{nameof(childName)}' を NULL または空にすることはできません。", nameof(childName));
		}

		SetValueChildInternal(tree, childName, value, "values");
	}

	/// <summary>
	/// 属性がvalueのみ持ち、Tree中に一つしかない<see cref="Tree.Children"/>の値をセットする
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="tree"></param>
	/// <param name="childName"></param>
	/// <param name="value"></param>
	/// <seealso cref="SetValuesOnlyChildrenValue{T}(Tree, string, T)"/>
	public static void SetValueOnlyChildValue<T>(
		Tree tree,
		string childName,
		T value
	)
		where T: notnull
	{
		if (tree is null)
		{
			throw new ArgumentNullException(nameof(tree));
		}

		if (string.IsNullOrEmpty(childName))
		{
			throw new ArgumentException($"'{nameof(childName)}' を NULL または空にすることはできません。", nameof(childName));
		}

		SetValueChildInternal(tree, childName, value, nameof(value));
	}

	private static void SetValueChildInternal<T>(
		Tree tree,
		string childName,
		T value,
		string valueName
	)
		where T : notnull
	{
		var hasChild = tree
			.Children
			.Exists(v => string.Equals(v.Name, childName, StringComparison.Ordinal));
		if (hasChild)
		{
			tree.Children
				.Find(c => string.Equals(c.Name, childName, StringComparison.Ordinal))
				.Attributes
				.Find(a => string.Equals(a.Key, valueName, StringComparison.Ordinal))
				.Value = value;
		}
		else
		{
			var c = new Tree(childName);
			var h = HeaderUtil.Analysis(value);
			var type = h.Type;
			c.AddAttribute(valueName, value, type);
			tree.Children.Add(c);
		}
	}
}