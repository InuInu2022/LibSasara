using System;
using System.Collections.Generic;
using System.Linq;
using LibSasara.VoiSona.Model;

namespace LibSasara.VoiSona.Util;

/// <summary>
/// header utility
/// </summary>
public static class HeaderUtil
{
	/// <summary>
	/// ヘッダーデータを解析する
	/// </summary>
	/// <param name="value"></param>
	/// <returns></returns>
	public static Header Analysis(dynamic value)
	{
		Type type = value.GetType();

		if (typeof(int) == type)
		{
			var b = BitConverter.GetBytes((int)value);
			return new(b.Length, VoiSonaValueType.Int32, b);
		}
		else if(typeof(bool) == type)
		{
			var b = BitConverter.GetBytes((bool)value);
			return new(b.Length, VoiSonaValueType.Bool, b);
		}
		else if (typeof(double) == type || typeof(decimal) == type)
		{
			var b = BitConverter.GetBytes((double)value);
			return new(b.Length, VoiSonaValueType.Double, b);
		}
		else if (typeof(string) == type)
		{
			byte[] b = System.Text.Encoding.UTF8.GetBytes(value);
			return new(b.Length+1, VoiSonaValueType.String, b.Append<byte>(0).ToArray());
		}
		else
		{
			return new(0, VoiSonaValueType.Unknown, Array.Empty<byte>());
		}
	}
}

/// <summary>
/// <see cref="HeaderUtil.Analysis(dynamic)"/> の返すヘッダー型
/// </summary>
public sealed record Header
{
	/// <summary>
	/// データの長さ
	/// </summary>
	public int Count { get; }
	/// <summary>
	/// データの種類
	/// </summary>
	public VoiSonaValueType Type { get; }
	/// <summary>
	/// データバイナリ列
	/// </summary>
	public IReadOnlyCollection<byte> DataBytes { get; }
	/// <summary>
	/// データ
	/// </summary>
	public dynamic Data {
		get => Type switch
			{
				VoiSonaValueType.Int32
					=> BitConverter.ToInt32(DataBytes.ToArray(),0),
				VoiSonaValueType.Bool
					=> BitConverter.ToBoolean(DataBytes.ToArray(),0),
				VoiSonaValueType.Double
					=> BitConverter.ToDouble(DataBytes.ToArray(),0),
				//文字列はLEN+2なのでカット
				VoiSonaValueType.String
					=> System.Text.Encoding.UTF8
					.GetString(
						DataBytes.ToArray().AsSpan(0, DataBytes.Count-2).ToArray()
					),
				_ => throw new NotSupportedException("Not supported data")
			};

	}

	/// <summary>
	/// コンストラクタ
	/// </summary>
	/// <param name="count"></param>
	/// <param name="type"></param>
	/// <param name="data"></param>
	public Header(
		int count,
		VoiSonaValueType type,
		IReadOnlyCollection<byte> data
	)
	{
		Count = count;
		Type = type;
		DataBytes = data;
	}
}