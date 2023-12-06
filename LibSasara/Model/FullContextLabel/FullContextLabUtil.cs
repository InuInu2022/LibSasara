using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace LibSasara.Model.FullContextLabel;

/// <summary>
/// a utility class for full context label
/// </summary>
public static class FullContextLabUtil
{
	/// <summary>
	/// "xx"か1以上の数値の文字列を <see cref="int"/> に変換。
	/// "xx"または変換失敗時は<paramref name="invalidValue"/>を返す
	/// </summary>
	/// <param name="contextValue"></param>
	/// <param name="invalidValue">"xx"または変換失敗時に返す値</param>
	/// <returns>"xx"または変換失敗時は<paramref name="invalidValue"/>を返す</returns>
	public static int GetNumber(
		string contextValue,
		int invalidValue = -1)
	{
		if(string.Equals(contextValue, "xx", StringComparison.Ordinal))
		{
			return invalidValue;
		}
		var isValid = int.TryParse(
			contextValue,
			NumberStyles.Integer,
			CultureInfo.InvariantCulture,
			out var result);
		return isValid ? result : invalidValue;
	}

	/// <summary>
	/// <paramref name="beforChar"/>と <paramref name="afterChar"/>の間が"xx"または数値の時、数値ならintを返す
	/// </summary>
	/// <param name="contexts"></param>
	/// <param name="beforChar"></param>
	/// <param name="afterChar"></param>
	/// <returns></returns>
	public static int GetNumberFromContext(
		ReadOnlyMemory<char> contexts,
		char beforChar,
		char? afterChar
	){
		var c = afterChar is not null
			? GetCharsFromContext(contexts, beforChar, (char)afterChar)
			: GetCharsFromContext(contexts, beforChar);
		return GetNumber(c.ToString());
	}

	/// <summary>
	/// <paramref name="beforChar"/>と文末の間が"xx"または数値の時、数値ならintを返す
	/// </summary>
	/// <param name="contexts"></param>
	/// <param name="beforChar"></param>
	/// <returns></returns>
	public static int GetNumberFromContext(
		ReadOnlyMemory<char> contexts,
		char beforChar
	){
		var c = GetCharsFromContext(contexts, beforChar);
		return GetNumber(c.ToString());
	}

	/// <summary>
	/// <paramref name="beforeChar"/>と <paramref name="afterChar"/>の間の文字を返す
	/// </summary>
	/// <param name="contexts"></param>
	/// <param name="beforeChar"></param>
	/// <param name="afterChar"></param>
	/// <param name="countBefore"><paramref name="beforeChar"/>が何番目に出現する文字列かを指定</param>
	/// <returns></returns>
	public static ReadOnlyMemory<char>
	GetCharsFromContext(
		ReadOnlyMemory<char> contexts,
		char beforeChar,
		char afterChar,
		int countBefore = 1
	)
	{
		if (contexts.Length == 0) { return PhonemeUtil.INVALID_PH.AsMemory(); }

		var labels = contexts.Span;
		var s = countBefore == 1
			? labels.IndexOf(beforeChar)
			: GetNthCharIndex(beforeChar, countBefore, labels);
		if (s == -1)
		{
			return PhonemeUtil.INVALID_PH.AsMemory();
		}

		var e = labels.Slice(s + 1).IndexOf(afterChar);
		if (e == -1)
		{
			return PhonemeUtil.INVALID_PH.AsMemory();
		}

		return contexts.Slice(
			s + 1,
			e);
	}

	private static int GetNthCharIndex(
		char beforeChar,
		int countBefore,
		ReadOnlySpan<char> labels)
	{
		var lastIndex = -1;
		for (var i = 0; i <= countBefore; i++)
		{
			var index = labels
				.Slice(lastIndex + 1)
				.IndexOf(beforeChar);
			lastIndex = lastIndex + 1 + index;
		}

		return lastIndex;
	}

	/// <summary>
	/// <paramref name="beforeChar"/>より以降の文字をすべて返す
	/// </summary>
	/// <param name="contexts"></param>
	/// <param name="beforeChar"></param>
	/// <param name="countBefore"></param>
	/// <seealso cref="GetCharsFromContext(ReadOnlyMemory{char}, char, char, int)"/>
	public static ReadOnlyMemory<char>
	GetCharsFromContext(
		ReadOnlyMemory<char> contexts,
		char beforeChar,
		int countBefore = 1
	)
	{
		if (contexts.Length == 0) { return PhonemeUtil.INVALID_PH.AsMemory(); }

		var labels = contexts.Span;
		var s = countBefore == 1
			? labels.IndexOf(beforeChar)
			: GetNthCharIndex(beforeChar, countBefore, labels);
		if (s == -1)
		{
			return PhonemeUtil.INVALID_PH.AsMemory();
		}

		return contexts.Slice(
			s + 1);
	}

	/// <summary>
	/// <see cref="FCLabLineJa"/> オブジェクトのシーケンスをモーラ（拍）毎のグループに分割します。
	/// </summary>
	/// <param name="list">グループに分割する <see cref="FCLabLineJa"/> オブジェクトのシーケンス。</param>
	/// <returns>各内部リストには、同じモーラに属する <see cref="FCLabLineJa"/> オブジェクトが含まれているリストのリストです。</returns>
	public static IList<List<FCLabLineJa>>
	SplitByMora(
		IEnumerable<FCLabLineJa> list
	)
	{
		if(list is null){
			return Enumerable
				.Empty<List<FCLabLineJa>>().ToList();
		}
		var grouped = new List<List<FCLabLineJa>>();
		var currentGroup = Enumerable
			.Empty<FCLabLineJa>().ToList();

		foreach (var item in list)
		{
			if (currentGroup.Count == 0 ||
				currentGroup[currentGroup.Count - 1].MoraIdentity?.ForwardPos == item.MoraIdentity?.ForwardPos)
			{
				//暫定fix, 同一モーラと判定される場合がある対応
				if (currentGroup.Count >= 1
					&& PhonemeUtil.IsVowel(currentGroup[currentGroup.Count - 1].Phoneme)) {
					//音素数が2以上で母音終わりならモーラの区切りとみなす
					currentGroup = AddNewList(grouped, currentGroup, item);
				} else {
					currentGroup.Add(item);
				}
			}
			else
			{
				currentGroup = AddNewList(grouped, currentGroup, item);
			}
		}

		grouped.Add(currentGroup);
		return grouped;

		static List<FCLabLineJa> AddNewList(
			List<List<FCLabLineJa>> grouped,
			List<FCLabLineJa> currentGroup,
			FCLabLineJa item)
		{
			grouped.Add(currentGroup);
			return new() { item };
		}
	}
}