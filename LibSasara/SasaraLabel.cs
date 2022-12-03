using System.IO;
using System.Threading.Tasks;
using LibSasara.Model;
using LibSasara.Model.Serialize;

namespace LibSasara;

/// <summary>
/// An utility library for Timing label file (.lab)
/// </summary>
public static class SasaraLabel
{
	/// <summary>
	/// .lab ファイルを読み込む
	/// </summary>
	/// <param name="path"></param>
	/// <param name="fps">動画等で利用する際のフレームレート指定</param>
	public static async Task<Lab> LoadAsync(
		string path,
		int fps = 30)
	{
        //check file exists
		if(!File.Exists(path)){
			throw new FileNotFoundException(
				$"Not found: {Path.GetFileName(path)}, path:{path}",
				Path.GetFileName(path)
			);
		}

        using var filestream = new FileStream(path, FileMode.Open);
		using var sr = new StreamReader(filestream);
		var t = await sr.ReadToEndAsync();
		return new Lab(t, fps);
	}
}
