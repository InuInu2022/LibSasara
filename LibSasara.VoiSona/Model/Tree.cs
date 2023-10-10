using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using LibSasara.VoiSona.Util;

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
	/// このツリーがコレクション配列かどうか
	/// </summary>
	public bool IsCollection { get; }

	/// <summary>
	/// コンストラクタ
	/// </summary>
	/// <param name="Name">ツリーのプロパティ名 <see cref="Name"/>このツリーがコレクション配列かどうか</param>
	/// <param name="isCollection"></param>
	public Tree(
		string Name,
		bool isCollection = false
	)
	{
		this.Name = Name;
		this.IsCollection = isCollection;
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
		var len = cByte.Length;

		//isCollection
		if (IsCollection){
			var h = withNull
				? stackalloc byte[3+len]
				: stackalloc byte[2+len];
			if (withNull) h[0] = 0x00;
			var offset = withNull ? 1 : 0;
			h[offset] = 0x00;
			var size = BinaryUtil.ParseSizeBytesFromCount(Count);
			h[offset + 1] = size;
			cByte.CopyTo(h.Slice(offset + 2));
			return new(h.ToArray());
		}

		//not collection
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
	/// バイト列化して返す
	/// </summary>
	/// <param name="withNull">子要素のパラメータ名のあとにNULLを付与するか</param>
	/// <param name="endNull">末端にNULLを付与するか</param>
	/// <returns></returns>
	public ReadOnlyMemory<byte> GetBytes(
		bool withNull = false,
		bool endNull = true)
	{
		ReadOnlyMemory<byte> hexName = System.Text
			.Encoding.UTF8.GetBytes($"{Name}\0");
		var atHead = GetAttributeHeader();

		var atValues = AttributeCount > 0
			? Attributes
				.Select(v => v.GetBytes())
				.Aggregate((a, b) => a.Concat(b))
			: Array.Empty<byte>()
			;
		//TODO:必要な処理に置き換え
		if(Name is "Timing" or "LogF0" or "C0")
		{
			//強制的に
			withNull = false;
			endNull = false;
		}
		var cldHead = GetChildHeader(withNull);
		//Console.WriteLine(BitConverter.ToString(cldHead));

		var cldValues = Count > 0
			? Children
				.Select(v => v.GetBytes())
				.Aggregate((a, b) => a.Concat(b))
			: Array.Empty<byte>()
			;

		var ret = hexName;

		if(AttributeCount>0){
			ret = ret
				.Concat(atHead)
				.Concat(atValues)
				;
		}
		if(Count>0){
			ret = ret
				.Concat(cldHead)
				.Concat(cldValues)
				;
		}

		return endNull
			? ret.Concat(new byte[1] { Common.NULL_END })
			: ret;
	}

	/// <summary>
	/// 属性のデータを追加する
	/// </summary>
	/// <remarks>
	/// 同じ <paramref name="key"/>の属性がある時は値を上書き、無ければ新規追加
	/// </remarks>
	/// <param name="key"></param>
	/// <param name="value"></param>
	/// <param name="type"></param>
	public void AddAttribute(
		string key,
		dynamic value,
		VoiSonaValueType? type
	){
		var exists = Attributes
			.Exists(v => string.Equals(v.Key, key, StringComparison.Ordinal));
		if(exists){
			var a = Attributes
				.First(v => string.Equals(v.Key, key, StringComparison.Ordinal));
			a.Value = value;
			a.Type = type;
		}else{
			Attributes.Add(new(key, value, type));
		}
	}

	/// <summary>
	/// 属性のデータを <see cref="KeyValue{T}"/>で取得する
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="key">属性名</param>
	/// <returns></returns>
	public KeyValue<T> GetAttribute<T>(
		string key
	)
		where T: notnull
	{
		var exists = Attributes
			.Exists(v => string.Equals(v.Key, key, StringComparison.Ordinal));
		if(exists){
			var a = Attributes
				.First(v => string.Equals(v.Key, key, StringComparison.Ordinal));
			return new KeyValue<T>(a.Key, (T)a.Value, a.Type);
		}else{
			Debug.WriteLine($"Attribute:{key} is not found!");
			return new(key, default!, VoiSonaValueType.Unknown);
		}
	}

	/// <summary>
	/// 属性の値だけを取得
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="key">属性名</param>
	/// <returns></returns>
	/// <seealso cref="GetAttribute{T}(string)"/>
	public T GetAttributeValue<T>(string key)
		where T: notnull
	{
		return GetAttribute<T>(key).Value;
	}
}
