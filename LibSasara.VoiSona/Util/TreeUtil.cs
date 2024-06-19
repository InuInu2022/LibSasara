using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CommunityToolkit.Diagnostics;
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
		Guard.IsNull(tree, nameof(tree));

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
		Guard.IsNull(tree, nameof(tree));

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

		var attrs = tree.Children
			.Find(c => string.Equals(c.Name, childName, StringComparison.Ordinal))?
			.Attributes;
		dynamic? value;
#pragma warning disable CA1031 // 一般的な例外の種類はキャッチしません
		try
		{
			value = attrs?
				.Find(a => string.Equals(a.Key, valueName, StringComparison.Ordinal))?
				.Value
				?? default(T);
		}
		catch (System.Exception)
		{
			return default;
		}
#pragma warning restore CA1031 // 一般的な例外の種類はキャッチしません
		if (value is null){
			return default;
		}
		if (typeof(T) == typeof(int))
		{
			return (T)(object)LibSasaraUtil.ConvertInt((string)value);
		}

		if (typeof(T) == typeof(decimal))
		{
			return value switch
			{
				T d => d,
				_ => throw new InvalidCastException($"{value} is error!"),
			};
		}

		if (typeof(T) == typeof(bool))
		{
			return (T)(object)LibSasaraUtil.ConvertBool((string)value);
		}

		if (typeof(T) == typeof(string))
		{
			return (T)(object)value;
		}

		throw new NotSupportedException();
	}

	/// <summary>
	/// 属性がvaluesのみ持ち、Tree中に一つしかない<see cref="Tree.Children"/>の値をセットする
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="tree"></param>
	/// <param name="childName"></param>
	/// <param name="value"></param>
	/// <seealso cref="GetValuesOnlyChildrenValue{T}(Tree, string)"/>
	[RequiresUnreferencedCode($"Calls {nameof(SetValueChildInternal)}")]
	public static void SetValuesOnlyChildrenValue<T>(
		Tree tree,
		string childName,
		T value
	)
		where T: notnull
	{
		Guard.IsNull(tree, nameof(tree));

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
	[RequiresUnreferencedCode($"Calls {nameof(SetValueChildInternal)}")]
	public static void SetValueOnlyChildValue<T>(
		Tree tree,
		string childName,
		T value
	)
		where T: notnull
	{
		Guard.IsNull(tree, nameof(tree));

		if (string.IsNullOrEmpty(childName))
		{
			throw new ArgumentException($"'{nameof(childName)}' を NULL または空にすることはできません。", nameof(childName));
		}

		SetValueChildInternal(tree, childName, value, nameof(value));
	}

	[RequiresUnreferencedCode("Calls LibSasara.VoiSona.Util.HeaderUtil.Analysis(dynamic)")]
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
			var target = tree.Children
				.Find(c => string.Equals(c.Name, childName, StringComparison.Ordinal))?
				.Attributes
				.Find(a => string.Equals(a.Key, valueName, StringComparison.Ordinal));
			if (target is null) return;
			target.Value = value;
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