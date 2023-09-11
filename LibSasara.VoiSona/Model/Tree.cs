using System.Globalization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LibSasara.VoiSona.Model;

/// <summary>
/// tssprj / ttsprj のバイナリツリー構造
/// </summary>
public class Tree
{
	/// <summary>
	/// ツリーのプロパティ名
	/// </summary>
	public string Name { get; set; }
	/// <summary>
	/// 子ツリーの個数
	/// </summary>
	public int Count {
		get => this.Children?.Count ?? 0;
	}
	/// <summary>
	/// 子ツリー
	/// </summary>
	public List<Tree> Children { get; set; }
		= new();

	/// <summary>
	/// 属性の個数
	/// </summary>
	public int AttributeCount {
		get => this.Attributes?.Count ?? 0;
	}
	/// <summary>
	/// 属性
	/// </summary>
	/// <remarks>
	/// key - value式で返ります。
	/// </remarks>
	public List<KeyValue<dynamic>> Attributes { get; set; }
		= new();

	/// <summary>
	/// コンストラクタ
	/// </summary>
	/// <param name="Name">ツリーのプロパティ名 <see cref="Name"/></param>
	public Tree(
		string Name
	)
	{
		this.Name = Name;
	}

	/// <summary>
	/// 子要素のヘッダー部分を取得
	/// </summary>
	/// <param name="withNull"></param>
	/// <returns></returns>
	public ReadOnlyMemory<byte> GetChildHeader(
		bool withNull = true
	) {
		var cByte = BitConverter
			.GetBytes(Count)
			.AsSpan();
		int len = cByte.Length;

		var n = withNull switch
		{
			true => new byte[2]{
				00, Convert.ToByte(len)
			},
			false => new byte[1]{
				Convert.ToByte(len)
			}
		};
		return n
			.Concat(cByte.ToArray())
			.ToArray();

	}

	/// <summary>
	/// 属性のヘッダーを取得
	/// </summary>
	/// <returns></returns>
	public ReadOnlyMemory<byte> GetAttributeHeader(){
		return AttributeCount switch
		{
			<= 0 => new byte[1] {
				Common.NULL_END
			},
			_ => new byte[2]{
				01,
				Convert.ToByte(AttributeCount)
			}
		};
	}

    /// <summary>
    /// 属性のデータを追加する
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <param name="type"></param>
    public void AddAttribute(
		string key,
		dynamic value,
		VoiSonaValueType? type
	){
		Attributes.Add(new(key, value, type));
	}
}
