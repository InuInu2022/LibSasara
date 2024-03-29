using System;
using System.Collections.Generic;
using System.Linq;
using LibSasara.VoiSona.Model.Talk;

namespace LibSasara.VoiSona.Util;

/// <summary>
/// テキスト表現を変換するユーティリティ
/// </summary>
/// <seealso cref="SasaraUtil.ConvertBool(string?, bool)"/>
/// <seealso cref="SasaraUtil.ConvertDecimal(string?, decimal)"/>
/// <seealso cref="SasaraUtil.ConvertInt(string?, int)"/>
public static class TextUtil
{
	private static readonly char[] delimComma = [','];
	private static readonly char[] delimColon = [':'];

	/// <summary>
	/// [seconds]:[T value] のカンマ区切り文字列を分割変換
	/// </summary>
	/// <typeparam name="T">値の型</typeparam>
	/// <param name="source">[seconds]:[T value] のカンマ区切り文字列</param>
	/// <returns></returns>
	public static IEnumerable<SecondsValue<T>>
	SplitValBySec<T>(string source)
		where T: struct
	{
		if (string.IsNullOrEmpty(source))
		{
			throw new ArgumentException($"'{nameof(source)}' を NULL または空にすることはできません。", nameof(source));
		}

		return source
			.Split(delimComma)
			.Select(v =>
			{
				Span<string> span = v.Split(delimColon);
				return new SecondsValue<T>(
					SasaraUtil
						.ConvertDecimal(span[0]),
					Cast<T>(v)
				);
			});
	}

	/// <summary>
	/// [frames]:[T value] のカンマ区切り文字列を分割変換
	/// </summary>
	/// <typeparam name="T">値の型</typeparam>
	/// <param name="source">[frames]:[T value] のカンマ区切り文字列</param>
	/// <returns></returns>
	public static IEnumerable<FrameValue<T>>
	SplitValByFrame<T>(string source)
		where T: struct
	{
		if (string.IsNullOrEmpty(source))
		{
			throw new ArgumentException($"'{nameof(source)}' を NULL または空にすることはできません。", nameof(source));
		}

		return source
			.Split(delimComma)
			.Select<string, FrameValue<T>>(v =>
			{
				Span<string> span = v.Split(delimColon);
				return new(
					SasaraUtil
						.ConvertInt(span[0]),
					Cast<T>(v)
				);
			});
	}

	/// <summary>
	/// カンマ区切り文字列を分割
	/// </summary>
	/// <param name="source"></param>
	/// <returns></returns>
	public static IEnumerable<T>
	SplitVal<T>(string source)
		where T: struct
	{
		if (string.IsNullOrEmpty(source))
		{
			throw new ArgumentException($"'{nameof(source)}' を NULL または空にすることはできません。", nameof(source));
		}

		return source
			.Split(delimComma)
			.Select(v => Cast<T>(v))
			;
	}

	private static T Cast<T>(string value)
		where T: struct
	{
		var type = typeof(T);
		var code = Type.GetTypeCode(type);

		return code switch
		{
			TypeCode.Int32 =>
				(T)(object)SasaraUtil.ConvertInt(value),
			TypeCode.Boolean =>
				(T)(object)SasaraUtil.ConvertBool(value),
			TypeCode.Decimal =>
				(T)(object)SasaraUtil.ConvertDecimal(value),
			TypeCode.String => (T)(object)value,
			_ => default
		};
	}
}