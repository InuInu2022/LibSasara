using System;

namespace LibSasara.VoiSona.Model;

/// <summary>
///
/// </summary>
public static class Common{
    /// <summary>
    /// null終端区切り
    /// </summary>
    public const byte NULL_END = 0x00;
}

/// <summary>
///
/// </summary>
public enum VoiSonaValueType: byte{
    /// <summary>
    ///
    /// </summary>
    Int32 = 01,
    /// <summary>
    ///
    /// </summary>
    Bool = 02,
    /// <summary>
    ///
    /// </summary>
    Double = 04,
    /// <summary>
    ///
    /// </summary>
    String = 05,
    /// <summary>
    ///
    /// </summary>
    Unknown = 99,
}