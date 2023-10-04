using System.Collections.ObjectModel;
using System;
using System.Collections.Generic;
using LibSasara.Model;
using System.Linq;

namespace LibSasara.VoiSona.Model;

/// <summary>
/// base voisona file
/// </summary>
public abstract record VoiSonaFileBase
{
	private protected readonly IReadOnlyList<byte> bytes;

	/// <summary>
	/// format version
	/// </summary>
	public Version? Format { get; set; }

	/// <summary>
	/// ファイルのカテゴリ（Talk or Song）
	/// </summary>
	/// <remarks>
	/// Talk: <see cref="Category.TextVocal" />, Song: <see cref="Category.SingerSong" />
	/// </remarks>
	public virtual Category Category { get; }

	/// <summary>
	/// raw binary data
	/// </summary>
	public ReadOnlyMemory<byte> Data { get; }

	/// <summary>
	/// コンストラクタ
	/// </summary>
	/// <param name="data"></param>
	protected VoiSonaFileBase(
		IReadOnlyList<byte> data
	)
	{
		bytes = data;
		Data = bytes.ToArray().AsMemory();
	}
}
