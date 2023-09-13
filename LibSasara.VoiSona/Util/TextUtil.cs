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
	/// <summary>
	/// [seconds]:[decimal value] のカンマ区切り文字列を分割変換
	/// </summary>
	/// <param name="source">[seconds]:[decimal value] のカンマ区切り文字列</param>
	/// <returns></returns>
	public static IEnumerable<FrameValue<decimal>>
	SplitDecValBySec(string source)
	{
		char[] comma = ",".ToCharArray();
		char[] colon = ":".ToCharArray();
		return source
			.Split(comma)
			.Select(v =>
			{
				Span<string> span = v.Split(colon);
				return new FrameValue<decimal>(
					SasaraUtil
						.ConvertDecimal(span[0]),
					SasaraUtil
						.ConvertDecimal(span[1])
				);
			});
	}

	/// <summary>
	/// [seconds]:[int value] のカンマ区切り文字列を分割変換
	/// </summary>
	/// <param name="source">[seconds]:[int value] のカンマ区切り文字列</param>
	/// <returns></returns>
	public static IEnumerable<FrameValue<int>>
	SplitIntValBySec(string source)
	{
		char[] comma = ",".ToCharArray();
		char[] colon = ":".ToCharArray();
		return source
			.Split(comma)
			.Select(v =>
			{
				Span<string> span = v.Split(colon);
				return new FrameValue<int>(
					SasaraUtil
						.ConvertDecimal(span[0]),
					SasaraUtil
						.ConvertInt(span[1])
				);
			});
	}
}