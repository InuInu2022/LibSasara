using System;
using System.Linq;
using System.Runtime.InteropServices;
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

		var isInt16 = ret.Count + 1 > 256;
		var cByteLen = isInt16 ? 2 : 1;
		var len = withNull ? cByteLen + 3 : cByteLen + 2;

		Span<byte> rs = stackalloc byte[len];
		var index = withNull ? 1 : 0;

		if (withNull) rs[0] = 0x00;

		rs[0 + index] = isInt16 ? (byte)0x02 : (byte)0x01;
		Span<byte> cbytes = isInt16
			? BitConverter.GetBytes(Convert.ToInt16(ret.Count + 1))
			: BitConverter.GetBytes(Convert.ToByte(ret.Count + 1));
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
		var head = this.GetHeaderBytes(false, true);
		return hexKey.Concat(head);
	}
}