namespace LibSasara.VoiSona.Model;

/// <summary>
/// common
/// </summary>
public static class VoiSonaCommon{
	/// <summary>
	/// null終端区切り
	/// </summary>
	public const byte NULL_END = 0x00;
}
#pragma warning disable CA1720 // 識別子に型名が含まれます
#pragma warning disable CA1008 // 列挙型は 0 値を含んでいなければなりません
#pragma warning disable CA1028 // 列挙ストレージは Int32 でなければなりません
/// <summary>
/// Type definitions in tssprj/tstprj format
/// </summary>
public enum VoiSonaValueType: byte{
	/// <summary>
	/// int32
	/// </summary>
	Int32 = 01,
	/// <summary>
	/// boolean
	/// </summary>
	Bool = 02,
	/// <summary>
	/// double float
	/// </summary>
	Double = 04,
	/// <summary>
	/// UTF-8 strings
	/// </summary>
	String = 05,
	/// <summary>
	/// unkown type
	/// </summary>
	Unknown = 99,
}
#pragma warning restore CA1720 // 識別子に型名が含まれます
#pragma warning restore CA1008 // 列挙型は 0 値を含んでいなければなりません
#pragma warning restore CA1028 // 列挙ストレージは Int32 でなければなりません