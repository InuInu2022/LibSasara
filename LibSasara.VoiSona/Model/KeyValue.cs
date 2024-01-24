using System;
using System.IO;
using System.Linq;
using LibSasara.VoiSona.Util;

namespace LibSasara.VoiSona.Model;

/// <summary>
/// key - value の組を表す
/// </summary>
public sealed record KeyValue<T>
{
	/// <summary>
	/// キー
	/// </summary>
	public string Key { get; set; }
	/// <summary>
	/// 任意の型の値
	/// </summary>
	public T Value { get; set; }
	/// <summary>
	/// 値の型 <see cref="VoiSonaValueType"/>
	/// </summary>
	public VoiSonaValueType? Type { get; set; }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="type"></param>
    public KeyValue(
		string key,
		T value,
		VoiSonaValueType? type = VoiSonaValueType.Unknown
	)
	{
		Key = key;
		Value = value;
		Type = type;
	}

	/// <summary>
	/// ヘッダーバイト列を返す
	/// </summary>
	/// <param name="withNull"></param>
	/// <param name="withData"></param>
	/// <returns></returns>
	public ReadOnlyMemory<byte> GetHeaderBytes(
		bool withNull,
		bool withData = false)
	{
		var ret = HeaderUtil.Analysis(Value!);

		var cByteLen = BinaryUtil.ParseSizeBytesFromCount(ret.Count + 1);
		var len = withNull ? cByteLen + 3 : cByteLen + 2;

		Span<byte> rs = stackalloc byte[len];
		var index = withNull ? 1 : 0;

		if (withNull) rs[0] = 0x00;

		rs[0 + index] = cByteLen;
		Span<byte> cbytes = cByteLen switch
		{
			1 => BitConverter.GetBytes((short)Convert.ToByte(ret.Count + 1)),
			2 => BitConverter.GetBytes(Convert.ToUInt16(ret.Count + 1)),
			4 => BitConverter.GetBytes(Convert.ToInt32(ret.Count + 1)),
			8 => BitConverter.GetBytes(Convert.ToInt64(ret.Count + 1)),
			_ => throw new InvalidDataException(nameof(GetHeaderBytes))
		};
		cbytes.CopyTo(rs.Slice(1 + index, cbytes.Length));
		rs[len - 1] = (byte)ret.Type;

		return withData
			? rs.ToArray().Concat(ret.DataBytes).ToArray().AsMemory()
			: rs.ToArray().AsMemory()
			;
	}

	/// <summary>
	/// バイト列に変換して返す
	/// </summary>
	/// <returns></returns>
	public ReadOnlyMemory<byte> GetBytes(){
		ReadOnlyMemory<byte> hexKey = System.Text
			.Encoding.UTF8.GetBytes($"{Key}\0");
		var head = GetHeaderBytes(false, true);
		return hexKey.Concat(head);
	}
}