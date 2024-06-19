using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace LibSasara.Model;

//ported from NodoAme source
/// <summary>
/// 音素ユーティリティ
/// </summary>
public static class PhonemeUtil
{
	/// <summary>
	/// 小さい「っ」（促音）の音素。close
	/// </summary>
	public const string CL = "cl";

	/// <summary>
	/// 休符音素。pause
	/// </summary>
	public const string PAU = "pau";

	/// <summary>
	/// 休符音素。silent
	/// </summary>
	public const string SIL = "sil";

	/// <inheritdoc cref="InvalidPhrase"/>
	[SuppressMessage("","CA1707")]
	[Obsolete($"Use ${nameof(InvalidPhrase)}")]
	public const string INVALID_PH = InvalidPhrase;

	/// <summary>
	/// 無効音素
	/// </summary>
	public const string InvalidPhrase = "xx";

	/// <summary>
	/// 日本語の母音音素の正規表現パターン。無声母音含む
	/// </summary>
	public static readonly Regex VowelsJapanese
		= new("[aiueoAIUEO]", RegexOptions.Compiled, TimeSpan.FromSeconds(1));

	/// <inheritdoc cref="VowelsJapanese"/>
	[SuppressMessage("","CA1707")]
	[Obsolete($"Use ${nameof(VowelsJapanese)}")]
	public static readonly Regex VOWELS_JA = VowelsJapanese;

	/// <summary>
	/// 日本語の無声母音音素の正規表現パターン。
	/// </summary>
	public static readonly Regex NoSoundVowels = new("[AIUEO]", RegexOptions.Compiled, TimeSpan.FromSeconds(1));

	/// <inheritdoc cref="NoSoundVowels"/>
	[SuppressMessage("","CA1707")]
	[Obsolete($"Use ${nameof(NoSoundVowels)}")]
	public static readonly Regex NOSOUND_VOWELS = NoSoundVowels;

	/// <summary>
	/// 子音でない音素の正規表現パターン。
	/// </summary>
	public static readonly Regex NoConsonant
		= new($"{InvalidPhrase}|{CL}|{PAU}|{SIL}", RegexOptions.Compiled, TimeSpan.FromSeconds(1));

	/// <inheritdoc cref="NoConsonant"/>
	[SuppressMessage("","CA1707")]
	[Obsolete($"Use ${nameof(NoConsonant)}")]
	public static readonly Regex NO_CONSONANT = NoConsonant;

	/// <summary>
	/// 日本語の鼻音子音音素の正規表現パターン。
	/// </summary>
	public static readonly Regex NasalJapanese = new("[nmN]|ng", RegexOptions.Compiled, TimeSpan.FromSeconds(1));

	/// <inheritdoc cref="NasalJapanese"/>
	[SuppressMessage("","CA1707")]
	[Obsolete($"Use ${nameof(NasalJapanese)}")]
	public static readonly Regex NASAL_JA = NasalJapanese;

	/// <summary>
	/// 音素テキストが母音かどうか
	/// </summary>
	/// <param name="pText"></param>
	/// <returns></returns>
	public static bool IsVowel(string? pText) =>
		!string.IsNullOrEmpty(pText) && VowelsJapanese.IsMatch(pText);

	/// <summary>
	/// ラベルの音素が母音かどうか
	/// </summary>
	/// <param name="label"></param>
	/// <returns></returns>
	public static bool IsVowel(LabLine label) =>
		label is not null && IsVowel(label.Phoneme);

	/// <summary>
	/// 音素テキストが無声母音か？
	/// </summary>
	/// <param name="text"></param>
	/// <returns></returns>
	public static bool IsNoSoundVowel(string text)
		=> NoSoundVowels.IsMatch(text);

	/// <summary>
	/// 音素が子音かどうか
	/// </summary>
	/// <param name="cText"></param>
	/// <returns></returns>
	public static bool IsConsonant(string? cText){
		if(string.IsNullOrEmpty(cText)) {return false;}

		if(NoConsonant.IsMatch(cText)){
			//no:子音
			return false;
		}

		if (IsVowel(cText))
		{
			//no:母音
			return false;
		}

		//yes
		return true;
	}

	/// <inheritdoc cref="IsConsonant(string?)"/>
	/// <param name="label"></param>
	/// <returns></returns>
	public static bool IsConsonant(LabLine label) =>
		label is not null && IsConsonant(label.Phoneme);

	/// <summary>
	/// 鼻音かどうか
	/// </summary>
	/// <param name="nText"></param>
	/// <returns></returns>
	public static bool IsNasal(string? nText) =>
		!string.IsNullOrEmpty(nText) && NasalJapanese.IsMatch(nText);

	/// <summary>
	/// ラベルの音素が鼻音かどうか
	/// </summary>
	/// <param name="label"></param>
	public static bool IsNasal(LabLine label) =>
		label is not null && IsNasal(label.Phoneme);

	/// <summary>
	/// 促音かどうか
	/// </summary>
	/// <param name="text"></param>
	public static bool IsCL(string? text) =>
		!string.IsNullOrEmpty(text) && string.Equals(text, CL, StringComparison.Ordinal);

	/// <summary>
	/// ラベルの音素が促音かどうか
	/// </summary>
	/// <param name="label"></param>
	/// <returns></returns>
	public static bool IsCL(LabLine label) =>
		label is not null && IsCL(label.Phoneme);

	/// <summary>
	/// ラベルの音素が[pau]かどうか
	/// </summary>
	/// <param name="label"></param>
	/// <seealso cref="IsSil(LabLine)"/>
	/// <seealso cref="IsNoSounds(LabLine)"/>
	/// <returns></returns>
	public static bool IsPau(LabLine label) =>
		string.Equals(label?.Phoneme, PAU, StringComparison.Ordinal);

	/// <summary>
	/// ラベルの音素が[sil]かどうか
	/// </summary>
	/// <param name="label"></param>
	/// <seealso cref="IsPau(LabLine)"/>
	/// <seealso cref="IsNoSounds(LabLine)"/>
	/// <returns></returns>
	public static bool IsSil(LabLine label) =>
		string.Equals(label?.Phoneme, SIL, StringComparison.Ordinal);

	/// <summary>
	/// ラベル音素が休符音素かどうか
	/// </summary>
	/// <param name="label"></param>
	/// <seealso cref="IsPau(LabLine)"/>
	/// <seealso cref="IsSil(LabLine)"/>
	/// <returns></returns>
	public static bool IsNoSounds(LabLine label)
		=> label?.Phoneme is PAU ||
			label?.Phoneme is SIL ||
			label?.Phoneme is InvalidPhrase;
}
