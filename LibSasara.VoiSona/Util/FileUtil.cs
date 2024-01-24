using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LibSasara.VoiSona.Util;

/// <summary>
/// file utility
/// </summary>
public static class FileUtil
{
	/// <summary>
	/// load binary file async
	/// </summary>
	/// <param name="path"></param>
	/// <param name="ctx"></param>
	/// <returns></returns>
	/// <exception cref="FileNotFoundException"></exception>
	public static async ValueTask<byte[]>
	LoadAsync(string path, CancellationToken ctx = default)
	{
		if (!File.Exists(path))
		{
			throw new FileNotFoundException(
				$"Not found: {Path.GetFileName(path)}, path:{path}",
				Path.GetFileName(path)
			);
		}
#if NETSTANDARD2_0
		using var fs = new FileStream(path, FileMode.Open);
		var buffer = new byte[fs.Length];
		var _ = await fs
			.ReadAsync(buffer, 0, (int)fs.Length, ctx)
			.ConfigureAwait(false);
#else
		await using var fs = new FileStream(path, FileMode.Open);
		var buffer = new byte[fs.Length];
		var _ = await fs
			.ReadAsync(buffer.AsMemory(), ctx)
			.ConfigureAwait(false);
#endif
		return buffer;
	}

	/// <summary>
	/// save binary file async
	/// </summary>
	/// <param name="path"></param>
	/// <param name="data"></param>
	/// <param name="ctx"></param>
	/// <returns></returns>
	public static async ValueTask
	SaveAsync(
		string path,
		IReadOnlyList<byte> data,
		CancellationToken ctx = default
	){
		data ??= Enumerable.Empty<byte>().ToList();
#if NETSTANDARD2_0
		using var fs = File.Open(path, FileMode.OpenOrCreate);
		fs.Seek(0, SeekOrigin.Begin);
		await fs
			.WriteAsync([.. data], 0, data.Count, ctx)
			.ConfigureAwait(false);
#else
		await using var fs = File.Open(path, FileMode.OpenOrCreate);
		fs.Seek(0, SeekOrigin.Begin);
		await fs
			.WriteAsync(data.ToArray().AsMemory(0, data.Count), ctx)
			.ConfigureAwait(false);
#endif
	}
}