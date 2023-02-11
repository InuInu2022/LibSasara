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

	/// <summary>
	/// 無効音素
	/// </summary>
	public const string INVALID_PH = "xx";

	/// <summary>
	/// 日本語の母音音素の正規表現パターン。無声母音含む
	/// </summary>
	public static Regex VOWELS_JA = new("[aiueoAIUEO]", RegexOptions.Compiled);

	/// <summary>
	/// 日本語の無声母音音素の正規表現パターン。
	/// </summary>
	public static Regex NOSOUND_VOWELS = new("[AIUEO]", RegexOptions.Compiled);

	/// <summary>
	/// 子音でない音素の正規表現パターン。
	/// </summary>
	public static Regex NO_CONSONANT = new($"{INVALID_PH}|{CL}|{PAU}|{SIL}", RegexOptions.Compiled);

	/// <summary>
	/// 日本語の鼻音子音音素の正規表現パターン。
	/// </summary>
	public static Regex NASAL_JA = new("[nmN]|ng", RegexOptions.Compiled);

	/// <summary>
	/// 音素テキストが母音かどうか
	/// </summary>
	/// <param name="pText"></param>
	/// <returns></returns>
	public static bool IsVowel(string? pText) =>
		!string.IsNullOrEmpty(pText) && VOWELS_JA.IsMatch(pText);

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
		=> NOSOUND_VOWELS.IsMatch(text);

	/// <summary>
	/// 音素が子音かどうか
	/// </summary>
	/// <param name="cText"></param>
	/// <returns></returns>
	public static bool IsConsonant(string? cText){
		if(string.IsNullOrEmpty(cText)) {return false;}

		if(NO_CONSONANT.IsMatch(cText)){
			//no:子音
			return false;
		}else if(IsVowel(cText)){
			//no:母音
			return false;
		}else{
			//yes
			return true;
		}
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
		!string.IsNullOrEmpty(nText) && NASAL_JA.IsMatch(nText);

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
		!string.IsNullOrEmpty(text) && text == CL;

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
		label?.Phoneme == PAU;

	/// <summary>
	/// ラベルの音素が[sil]かどうか
	/// </summary>
	/// <param name="label"></param>
	/// <seealso cref="IsPau(LabLine)"/>
	/// <seealso cref="IsNoSounds(LabLine)"/>
	/// <returns></returns>
	public static bool IsSil(LabLine label) =>
		label?.Phoneme == SIL;

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
			label?.Phoneme is INVALID_PH;
}
