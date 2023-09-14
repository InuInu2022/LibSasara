using System;

namespace LibSasara.VoiSona.Model;

/// <summary>
/// common
/// </summary>
public static class Common{
	/// <summary>
	/// null終端区切り
	/// </summary>
	public const byte NULL_END = 0x00;
}

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