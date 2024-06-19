using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using LibSasara.Model.Serialize;
using System.Diagnostics;
using System.Xml;
using System.Xml.Linq;
using System;
using LibSasara.Model;
using System.Diagnostics.CodeAnalysis;

namespace LibSasara;

/// <summary>
/// An utility library for CeVIO project file (ccs / ccst)
/// </summary>
public static class SasaraCcs
{
	/// <summary>
	/// A name of this library.
	/// </summary>
	public static readonly string Name = "LibSasara";

	/// <summary>
	/// CeVIOプロジェクトファイル(.ccs) または トラックファイル(.ccst)を読み込みます.
	/// ※LINQtoXML版
	/// </summary>
	/// <param name="path"></param>
	/// <returns>管理クラス</returns>
	/// <exception cref="FileNotFoundException"></exception>
	/// <exception cref="InvalidDataException"></exception>
	/// <seealso cref="LoadAsync{T}(string)"/>
	[SuppressMessage("","PH_P009")]
	public static async Task<CeVIOFileBase> LoadAsync(string path)
	{
		//check file exists
		if(!File.Exists(path)){
			throw new FileNotFoundException(
				$"Not found: {Path.GetFileName(path)}, path:{path}",
				Path.GetFileName(path)
			);
		}

		//check file has extension
		if(!Path.HasExtension(path)){
			throw new InvalidDataException("NOT SUPPORTED FILE: file must have .ccs or .ccst extension.");
		}

		using var filestream = new FileStream(path, FileMode.Open);
		var x = await Task.Run(()
			=> XDocument.Load(
				filestream,
				LoadOptions.PreserveWhitespace
			))
			.ConfigureAwait(false);

		switch (x.Root?.Name.LocalName)
		{
			case "Definition":
			{
				return new CcstTrack(x);
			}

			case "Scenario":
			{
				return new CcsProject(x);
			}

			default:
				throw new InvalidDataException();
		}
	}

	/// <inheritdoc cref="LoadAsync(string)"/>
	/// <typeparam name="T"><see cref="CcsProject"/>または<see cref="CcstTrack"/></typeparam>
	/// <seealso cref="LoadAsync(string)"/>
	public static async Task<T?> LoadAsync<T>(string path)
		where T: CeVIOFileBase,ICeVIOFile
	{
		return await LoadAsync(path)
			.ConfigureAwait(false) as T;
	}

	/// <summary>
	/// ファイルがCeVIO関連ファイルか
	/// </summary>
	/// <param name="path"></param>
	/// <returns></returns>
	public static bool IsCevioFile(string path){
		return File.Exists(path)
			&& Path.HasExtension(path)
			&& ((Path.GetExtension(path) is ".ccs" ) ||
				(Path.GetExtension(path) is ".ccst"));
	}

	/// <summary>
	/// 内容に応じた拡張子を付けてファイルを保存
	/// </summary>
	/// <param name="ccs"></param>
	/// <param name="path"></param>
	/// <param name="isAutoExt">内容に応じた拡張子(ccs/ccst)をつけるかどうか</param>
	public static async ValueTask SaveAsync(
		this ICeVIOFile ccs,
		string path,
		bool isAutoExt = true
	){
		//内容に応じたデフォルトの拡張子にする
		if(isAutoExt){
			var ext = ccs switch
			{
				CcsProject => "ccs",
				CcstTrack => "ccst",
				_ => "xml"
			};
			path = Path.ChangeExtension(path, ext);
		}

		await ccs
			.SaveAsync(path)
			.ConfigureAwait(false);
	}
}
