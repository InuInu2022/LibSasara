using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using LibSasara.VoiSona.Model;

namespace LibSasara.VoiSona.Util;

/// <summary>
/// binary utility
/// </summary>
public static class BinaryUtil
{
	/// <summary>
	/// Separate to binary collections
	/// </summary>
	/// <param name="source"></param>
	/// <param name="separator"></param>
	/// <returns></returns>
	public static ReadOnlyCollection<ReadOnlyMemory<byte>> Split(
		this ReadOnlySpan<byte> source,
		ReadOnlySpan<byte> separator
	)
	{
		var list = new List<ReadOnlyMemory<byte>>();

		int currentIndex = 0;
		while (currentIndex < source.Length)
		{
			int delimiterIndex = source.Slice(currentIndex).IndexOf(separator);

			if (delimiterIndex == -1)
			{
				list.Add(source.Slice(currentIndex).ToArray());
				break;
			}
			else
			{
				list.Add(source.Slice(currentIndex, delimiterIndex).ToArray());
				currentIndex += delimiterIndex + separator.Length;
			}
		}

		return list.AsReadOnly();
	}

	/// <inheritdoc cref="Split(ReadOnlySpan{byte}, ReadOnlySpan{byte})"/>
	public static ReadOnlyCollection<ReadOnlyMemory<byte>> Split(
		this Span<byte> source,
		ReadOnlySpan<byte> separator
	){
		ReadOnlySpan<byte> ros = source;
		return Split(ros, separator);
	}

	/// <summary>
	/// 指定されたバイト列 (<see cref="Span{T}"/>) から
	/// 指定された名前の属性の検索を試みます。
	/// </summary>
	/// <param name="source">検索対象のバイト列。</param>
	/// <param name="name">検索する属性の名前。</param>
	/// <param name="header">属性が見つかった場合、解析された属性情報が格納される出力パラメータ<see cref="Header"/>。</param>
	/// <returns>属性が見つかった場合は true。それ以外の場合は false。</returns>
	public static bool TryFindAttribute(
		ReadOnlySpan<byte> source,
		string name,
		out Header header
	)
	{
		ReadOnlySpan<byte> key =
			System.Text.Encoding.UTF8.GetBytes($"{name}\0");
		var index = source.IndexOf(key);

		//not found
		if (index == -1)
		{
			Debug.WriteLine($"An attribute is NOT Found. key:{name}");
			header = new Header(-1, Model.VoiSonaValueType.Unknown, Array.Empty<byte>());
			return false;
		}

		var temp = source.Slice(index + key.Length);
		var countType = (int)temp[0];
		var countData = temp.Slice(1, countType);
		int count = countType switch
		{
			0 or 1 => countData[0],
			2 => BitConverter
				.ToInt16(countData.ToArray(), 0),
			3 => BitConverter
				.ToInt32(countData.ToArray(), 0),
			4 => (int)BitConverter
				.ToInt64(countData.ToArray(), 0),
			_ => -1
		};

		var type = (VoiSonaValueType)temp[1 + countType];

		header = new(
			count,
			type,
			temp.Slice(2 + countType, count).ToArray()
		);

		//not supported
		if (count == -1)
		{
			Debug.WriteLine($"Not supported type. key:{name}, type:{countType}");
			return false;
		}

		return true;
	}

	/// <summary>
	///
	/// </summary>
	/// <param name="source"></param>
	/// <param name="name"></param>
	/// <param name="child"></param>
	/// <returns></returns>
	public static bool TryFindChild(
		ReadOnlySpan<byte> source,
		string name,
		out ReadOnlySpan<byte> child
	)
	{
		ReadOnlySpan<byte> key =
			Encoding.UTF8.GetBytes($"{name}\0");
		var index = source.IndexOf(key);

		//not found
		if (index == -1)
		{
			Debug.WriteLine($"An attribute is NOT Found. key:{name}");
			child = ReadOnlySpan<byte>.Empty;
			return false;
		}

		ReadOnlySpan<byte> endKey =
			Encoding.UTF8.GetBytes("\0\0");
		var endCount = source.Slice(index + key.Length).IndexOf(endKey);

		var list = endCount == -1
			//末端まで
			? source.Slice(index)
			//終わり見つかればそこまで
			: source.Slice(index, key.Length + endCount + endKey.Length)
			;
		child = list;
		return true;
	}

    /// <summary>
    ///
    /// </summary>
    /// <param name="source"></param>
    /// <param name="name"></param>
    /// <param name="childName"></param>
    /// <param name="collection"></param>
    /// <returns></returns>
    public static bool TryFindCollection(
		ReadOnlySpan<byte> source,
		string name,
		string childName,
		out ReadOnlyCollection<ReadOnlyMemory<byte>> collection
	)
	{
		ReadOnlySpan<byte> key =
			Encoding.UTF8.GetBytes($"{name}\0");
		var index = source.IndexOf(key);

		//not found
		if (index == -1)
		{
			Debug.WriteLine($"A collection is NOT Found. key:{name}");
			collection = new(Array.Empty<ReadOnlyMemory<byte>>());
			return false;
		}


		var temp = source.Slice(index + key.Length);
		var countType = (int)temp[1];
		var countData = temp.Slice(2, countType);

		int count = countType switch
		{
			1 => countData[0],
			2 => BitConverter.ToInt16(countData.ToArray(), 0),
			_ => 0
		};

		ReadOnlySpan<byte> childKey =
			 Encoding.UTF8.GetBytes($"{childName}\0");
		ReadOnlyMemory<byte> head = childKey.ToArray();

		var separated = temp
			.Split(childKey)
			.Skip(1)
			.Select(v => head.ToArray().Concat(v.ToArray()))
			.ToList()
			;
		/*
		var list = new List<ReadOnlyMemory<byte>>(count);

		for(var i = 0; i < count; i++){
			var a = separated.ElementAt(0).ToArray().AsSpan();

            var hasChild = TryFindChild(a, childName, out var child);

			list.Add(child.ToArray().AsMemory());
		}
		collection = list
			.AsReadOnly();
		*/
		collection = separated
			.Select(v => new ReadOnlyMemory<byte>(v.ToArray()))
			.ToList()
			.AsReadOnly()
			;
		return true;

		/*
		var ret = temp
			.Split(childKey)
			.Skip(1)
			;

		if(ret.Count() != count){
			throw new InvalidDataException($"Can't find child collections collectly! {name}'s {childName}");
		}

		collection = ret
			.Cast<ReadOnlyMemory<byte>>()
			.ToList()
			.AsReadOnly()
			;
		return true;
		*/

	}
}