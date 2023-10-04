using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LibSasara.VoiSona.Model;
using LibSasara.VoiSona.Model.Talk;
using LibSasara.VoiSona.Util;

namespace LibSasara.VoiSona;

/// <summary>
/// An utility library for VoiSona project file (tstprj / tssprj)
/// </summary>
public static class LibVoiSona
{
	/// <summary>
	/// load binary file async
	/// </summary>
	/// <param name="path"></param>
	/// <param name="ctx"></param>
	/// <returns></returns>
	public static async ValueTask<T>
	LoadAsync<T>(string path, CancellationToken ctx = default)
		where T: VoiSonaFileBase
	{
		var bytes = await FileUtil.LoadAsync(path, ctx);

		if(typeof(T) == typeof(TstPrj)){
			return (T)(object)new TstPrj(bytes);
		}
		//TODO: TssPrj
		else{
			throw new NotSupportedException("not supported yet");
		}
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
	)
		=> await FileUtil
			.SaveAsync(path, data, ctx)
			.ConfigureAwait(false);
}
