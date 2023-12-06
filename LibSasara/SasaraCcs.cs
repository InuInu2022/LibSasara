using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using LibSasara.Model.Serialize;
using System.Diagnostics;
using System.Xml;
using System.Xml.Linq;
using System;
using LibSasara.Model;

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
	/// ※デシリアライズ版
	/// </summary>
	/// <typeparam name="T"><see cref="Project"/>(ccs) or <see cref="Track"/>(ccst)</typeparam>
	/// <param name="path">path to a ccs/ccst file</param>
	/// <returns>デシリアライズされたオブジェクト</returns>
	/// <seealso cref="LoadAsync(string)"/>
	/// <exception cref="FileNotFoundException"></exception>
	/// <exception cref="InvalidDataException"></exception>
	[Obsolete($"use {nameof(LoadAsync)}")]
	public static async Task<T> LoadDeserializedAsync<T>(string path)
		where T : IRoot
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

		var fileExt = Path.GetExtension(path);
		var fileType = fileExt switch
		{
			".ccs" => typeof(Project),
			".ccst" => typeof(Track),
			_ => typeof(Project)
		};
		using var filestream = new FileStream(path, FileMode.Open);
		var serializer = new XmlSerializer(fileType);
		var data = await Task.Run(()=>
			(T) serializer.Deserialize(filestream)).ConfigureAwait(false);
		return data;
	}

	/// <summary>
	/// CeVIOプロジェクトファイル(.ccs) または トラックファイル(.ccst)を読み込みます.
	/// ※LINQtoXML版
	/// </summary>
	/// <param name="path"></param>
	/// <returns>管理クラス</returns>
	/// <exception cref="FileNotFoundException"></exception>
	/// <exception cref="InvalidDataException"></exception>
    /// <seealso cref="LoadAsync{T}(string)"/>
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
			));

		switch (x.Root.Name.LocalName)
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

	/// <summary>
	/// ファイルを保存
	/// ※再現性不正確、使用非推奨
	/// </summary>
	/// <param name="ccs"></param>
	/// <param name="path"></param>
	/// <param name="isAutoExt">内容に応じた拡張子(ccs/ccst)をつけるかどうか</param>
    [Obsolete($"use {nameof(SaveAsync)}")]
	public static async ValueTask SaveSerializedAsync(
		this IRoot ccs,
		string path,
		bool isAutoExt = true
	)
	{
		var serializer = new XmlSerializer(ccs.GetType());

		//内容に応じたデフォルトの拡張子にする
		if(isAutoExt){
			var ext = ccs switch
			{
				Project => "ccs",
				Track => "ccst",
				_ => "xml"
			};
			path = Path.ChangeExtension(path, ext);
		}

		var set = new XmlWriterSettings
		{
			Indent = true,
			IndentChars = "  ",
		};
		var xw = XmlWriter.Create(path, set);
		await Task.Run(() => {
			try
			{
				serializer.Serialize(xw, ccs);
			}
			catch (System.Exception e)
			{
				Debug.WriteLine($"Save error:{e}");
			}
		});
	}
}
