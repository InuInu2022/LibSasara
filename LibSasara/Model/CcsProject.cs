using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace LibSasara.Model;

/// <summary>
/// CCS/CCST共通インターフェイス
/// </summary>
public interface ICeVIOFile
{
	/// <summary>
	/// 指定したIDのトラックデータセット（GroupとUnit）を取得
	/// </summary>
	/// <param name="id">指定ID</param>
	(XElement group, List<XElement> units) GetTrackSet(Guid id);

	/// <inheritdoc cref="GetTrackSet(Guid)"/>
	TrackSet<T> GetTrackSet<T>(Guid id)
		where T : UnitBase;

	/// <summary>
	/// 指定したIDのトラックデータセット（GroupとUnit）を複製 <br/>
	/// see also：<seealso cref="DuplicateAndAddTrackSetAsync(Guid, Guid?)"/>
	/// </summary>
	/// <param name="targetId">指定GroupのGUID</param>
	/// <param name="newId">新規GUID</param>
	ValueTask<(XElement group, List<XElement> units)> DuplicateTrackSetAsync(
		Guid targetId,
		Guid? newId = null
	);

	/// <summary>
	/// 指定したIDのトラックデータセット（GroupとUnit）を複製・追加
	/// </summary>
	/// <param name="targetId">指定GroupのGUID</param>
	/// <param name="newId">新規GUID</param>
	/// <returns></returns>
	ValueTask<(XElement group, List<XElement> units)> DuplicateAndAddTrackSetAsync(
		Guid targetId,
		Guid? newId = null
	);

	/// <summary>
	/// ボイスアイテム一覧を返す
	/// </summary>
	List<XElement> GetUnitsRaw();

	/// <summary>
	/// <inheritdoc cref="GetUnitsRaw()"/>
	/// </summary>
	/// <param name="category">フィルタする分類</param>
	List<XElement> GetUnitsRaw(Category category);

	/// <summary>
	/// トラックデータ（GroupとUnit）のリストを返す
	/// </summary>
	/// <returns></returns>
	List<(XElement group, List<XElement> units)> GetTrackSets();
}

/// <summary>
/// CCS / CCSTファイル管理基底クラス
/// </summary>
public abstract class CeVIOFileBase : ICeVIOFile
{
	//将来的にtsprj,tsslnなども対応したい

	/// <summary>
	/// 元のCCS or CCSTのXDocument
	/// </summary>
	protected XDocument rawXml;

	/// <summary>
	/// ファイルを生成したツールのバージョン
	/// </summary>
	public Version? AutherVersion
	{
		get
		{
			var generator = rawXml
				.Descendants("Generation");
			if (!generator.Any())
			{
				return null;
			}

			var val = generator
				.FirstOrDefault(e => e.HasElements)?
				.Element("Author")?
				.Attribute("Version")?
				.Value;

			if (val is null)
			{
				return null;
			}

			return new(val);
		}
		/*
		//TODO:set version of editor
		set {
			if(value is null)return;

			value.ToString()
		}
		*/
	}

	/// <summary>
	/// 生のGroup要素
	/// </summary>
	public List<XElement> RawGroups
	{
		get => rawXml
			.Descendants("Groups")
			.Elements("Group")
			.ToList();
	}

	/// <summary>
	/// コンストラクタ
	/// </summary>
	/// <param name="xml">CCS or CCSTの <see cref="XDocument"/></param>
	protected CeVIOFileBase(XDocument xml)
	{
		rawXml = xml;
	}

	/// <inheritdoc/>
	/// <seealso cref="GetTrackSet{T}(Guid)"/>
	public (XElement group, List<XElement> units) GetTrackSet(Guid id)
	{
		var g = rawXml
			.Descendants("Group")
			.FirstOrDefault(e
				=> e.Attribute("Id").Value == id.ToString())
			;
		var u = GetUnitsRaw()
			.Where(e
				=> e.Attribute("Group").Value == id.ToString())
			.ToList()
			;
		return (g, u);
	}

	/// <inheritdoc/>
	/// <seealso cref="GetTrackSets{T}()"/>
	public TrackSet<T> GetTrackSet<T>(Guid id)
		where T: UnitBase
	{
		return new TrackSet<T>(this, id);
	}

	/// <inheritdoc/>
	/// <seealso cref="GetTrackSets{T}()"/>
	public List<(XElement group, List<XElement> units)>
		GetTrackSets()
	{
		return RawGroups
			.Select(v => v.Attribute("Id").Value)
			.Select(v => GetTrackSet(new(v)))
			.ToList();
	}

	/// <inheritdoc/>
	public List<TrackSet<T>> GetTrackSets<T>()
		where T: UnitBase
	{
		return RawGroups
			.Select(v => v.Attribute("Id").Value)
			.Select(v => new TrackSet<T>(this, new(v)))
			.Where(v => {
				Category cat;
				if(typeof(T) == typeof(SongUnit)){
					cat = Category.SingerSong;
				}
				else if(typeof(T)==typeof(TalkUnit)){
					cat = Category.TextVocal;
				}
				else if(typeof(T)==typeof(AudioUnit)){
					cat = Category.OuterAudio;
				}
				else{
					//UnitBase
					return true;
				}

				return v.Category == cat;
			})
			.ToList();
	}

	/// <inheritdoc/>
	public async ValueTask<(XElement group, List<XElement> units)> DuplicateTrackSetAsync(
		Guid targetId,
		Guid? newId = null
	)
	{
		Guid id = newId ?? Guid.NewGuid();
		var (group, units) = GetTrackSet(targetId);

		//duplicate Group
		var ng = new XElement(group);
		ng.Attribute("Id").SetValue(id);

		//duplicate Unit-s
		var nu = await Task.Run(() =>
			units
				.ConvertAll(u =>
				{
					var u2 = new XElement(u);
					u2
						.Attribute("Group")
						.SetValue(id);
					//clear cached rendered data
					if (u2.Attribute("ShapShot") is not null)
					{
						u2.Attribute("SnapShot")
							.Remove();
					}

					return u2;
				})
		);

		return (ng, nu);
	}

	/// <inheritdoc/>
	public async ValueTask<(XElement group, List<XElement> units)> DuplicateAndAddTrackSetAsync(
		Guid targetId,
		Guid? newId = null
	)
	{
		Guid id = newId ?? Guid.NewGuid();

		var (group, units) = GetTrackSet(targetId);
		var (ng, nu) = await DuplicateTrackSetAsync(targetId, id);

		//Add after
		group.AddAfterSelf(ng);
		//var last = units.Last();
		foreach (var item in nu)
		{
			units.Last().AddAfterSelf(item);
			//last = item;
		}

		return (ng, nu);
	}

	/// <inheritdoc/>
	public List<(string id, XElement param)> GetSongTuneRawParamters()
		=> GetUnitsRaw(Category.SingerSong)
			.AsParallel()
			.Where(e => e.HasAttributes)
			.ToList()
			.ConvertAll(e
				=> (
					e.Attribute("Group").Value,
					e.Descendants("Parameter").First()
				)
			)
			;

	/// <inheritdoc/>
	public List<XElement> GetUnitsRaw()
	{
		var x = new List<XElement>();
		try
		{
			x = rawXml
				.Descendants("Units")
				.Elements("Unit")
				.ToList();
		}
		catch (System.Exception e)
		{
			throw new Exception($"ERROR:{e}");
		}

		return x;
	}

	/// <inheritdoc/>
	public List<XElement> GetUnitsRaw(Category category)
		=> GetUnitsRaw()
			.Where(e => e.HasAttributes
				&& e.Attribute("Category").Value == nameof(Category.SingerSong))
			.ToList();

	/// <summary>
	/// Unit管理オブジェクトで返す
	/// </summary>
	public List<UnitBase> GetUnits()
		=> GetUnitsRaw()
			.ConvertAll<UnitBase>(e =>
				e.Attribute("Category").Value switch
				{
					nameof(Category.SingerSong)
						=> new SongUnit(e, this),
					nameof(Category.TextVocal)
						=> new TalkUnit(e, this),
					_ =>
						new AudioUnit(e, this)
				}
			)
			;

	/// <inheritdoc cref="GetUnits()"/>
	public List<UnitBase> GetUnits(Category category)
		=> GetUnits()
			.Where(v => v.Category == category)
			.ToList();

	/// <summary>
	/// CeVIOのトラックを定義するGroup要素を追加する
	/// </summary>
	/// <param name="group">追加するGroup要素</param>
	/// <seealso cref="AddGroup(Guid, Category, string, string, double, double, bool, bool, string)"/>
	/// <seealso cref="TrackSet{TUnit}"/>
	public void AddGroup(XElement group)
	{
		RawGroups
			.Last()
			.AddAfterSelf(group);
	}

	/// <summary>
	/// <inheritdoc cref="AddGroup(XElement)"/>
	/// </summary>
	/// <param name="groupId">トラックGroupのGuid</param>
	/// <param name="category">トラックの種類</param>
	/// <param name="name">トラック名</param>
	/// <param name="castId">トラックのキャストID。複数キャストやキャストが居ない場合は<value>Mixed</value></param>
	/// <param name="volume">トラックの音量</param>
	/// <param name="pan">トラックのパン</param>
	/// <param name="isSolo">ソロ再生</param>
	/// <param name="isMuted">再生ミュート</param>
	/// <param name="language">トラックの言語</param>
	/// <seealso cref="AddGroup(XElement)"/>
	/// <seealso cref="TrackSet{TUnit}"/>
	public void AddGroup(
		Guid groupId,
		Category category,
		string name,
		string castId = "Mixed",
		double volume = 0,
		double pan = 0,
		bool isSolo = false,
		bool isMuted = false,
		string language = "Japanaese"
	)
	{
		var color = category switch
		{
			Category.SingerSong => "#FFAF1F14",
			Category.TextVocal => "#FF267CCB",
			_ => "#FF008000"
		};

		var attr = new XAttribute[]{
			new("Version", "1.0"),
			new("Id", groupId.ToString()),
			new("Category", category.ToString()),
			new("Name", name),
			new("Color", color),
			new("Volume", volume),
			new("Pan", pan),
			new("IsSolo", isSolo),
			new("IsMuted", isMuted),
			new("CastId", castId),
			new("Language", language)
		};
		var e = new XElement("Group", attr);

		AddGroup(e);
	}

	/// <summary>
	/// Unit要素リストを追加する
	/// </summary>
	/// <param name="units">追加するUnit要素リスト</param>
	public void AddUnits(IEnumerable<XElement> units)
	{
		var x = rawXml
			.Descendants("Units");
		var u = x.Elements("Unit");

		var raw = GetUnitsRaw();
		var parent = raw.Count switch
		{
			0 => rawXml.Descendants("Units").First(),
			_ => GetUnitsRaw().Last().Parent
		};
		parent.Add(units);
	}

	/// <summary>
	/// Group要素を全て削除する
	/// </summary>
	/// <seealso cref="AddGroup(XElement)"/>
	/// <seealso cref="AddGroup(Guid, Category, string, string, double, double, bool, bool, string)"/>
	public void RemoveAllGroups()
	{
		var groups = RawGroups
			.Last()?
			.Parent;
		if (groups?.HasElements != true)
		{
			return;
		}

		groups
			.Elements()
			.Remove();
	}

	/// <summary>
	/// ccs/ccstファイルを保存
	/// </summary>
	/// <remarks>
	/// <seealso cref="LibSasara.SasaraCcs.SaveAsync(ICeVIOFile, string, bool)"/>
	/// </remarks>
	/// <param name="path"></param>
	public async ValueTask SaveAsync(string path)
	{
		await Task.Run(() =>
		{
			using var writer = new XmlTextWriter(
				path,
				Encoding.UTF8
			);
			writer.Formatting = Formatting.Indented;
			var settings = new XmlWriterSettings
			{
				Indent = true,
				NewLineOnAttributes = true,
				NewLineChars = "\r\n",
			};
			var w = XmlWriter.Create(writer, settings);
			rawXml.Save(w);
		}
		);
	}
}

/// <summary>
/// ext methods
/// </summary>
public static class CeVIOFileExt
{
	/// <summary>
	/// キャスト置き換え
	/// </summary>
	/// <param name="groupAndUnits"></param>
	/// <param name="beforeId"></param>
	/// <param name="afterId"></param>
	/// <param name="lang">言語指定文字列。指定された場合に置き換えます。</param>
	public static void ReplaceCastId(
		this (XElement, List<XElement>) groupAndUnits,
		string beforeId,
		string afterId,
		string? lang = null
	)
	{
		var g = groupAndUnits.Item1.Attribute("CastId");

		if (g.Value == beforeId)
		{
			g.SetValue(afterId);
		}

		groupAndUnits.Item2
			.Where(e
				=> e.Attribute("CastId").Value == beforeId)
			.ToList()
			.ForEach(e =>
			{
				e.SetAttributeValue("CastId", afterId);

				//reset cache
				if (e.Attribute("SnapShot") is not null)
				{
					e.SetAttributeValue("SnapShot", "");
				}

				//replace lang
				if (lang is null)
				{
					return;
				}

				e.SetAttributeValue("Language", lang);
			});
	}

	/// <summary>
	/// キャスト全置き換え（トークの場合全部置き換わります）
	/// </summary>
	/// <param name="groupAndUnits"></param>
	/// <param name="replacedId"></param>
	/// <param name="lang">言語指定文字列。指定された場合に置き換えます。</param>
	public static void ReplaceAllCastId(
		this (XElement, List<XElement>) groupAndUnits,
		string replacedId,
		string? lang = null
	)
	{
		var allUnits = new List<XElement>
		{
			groupAndUnits.Item1
		};
		allUnits = allUnits
			.Concat(groupAndUnits.Item2)
			.ToList();

		allUnits
			.ForEach(v =>
			{
				v
					.Attribute("CastId")
					.SetValue(replacedId);
				v.Attribute("SnapShot")?.Remove();
			});

		if (lang is null)
		{
			return;
		}

		allUnits
			.ForEach(v => v.Attribute("Language").SetValue(lang));
	}

	/// <summary>
	/// GroupのIDを取得
	/// </summary>
	/// <param name="groupAndUnits"></param>
	/// <returns></returns>
	public static Guid GetGroupId(
		this (XElement, List<XElement>) groupAndUnits
	)
	{
		return new(
			groupAndUnits
				.Item1
				.Attribute("Id")
				.Value
		);
	}

	/// <summary>
	/// GroupのIDをまとめて設定
	/// </summary>
	/// <param name="groupAndUnits"></param>
	/// <param name="guid">新しい<see cref="Guid"/></param>
	/// <returns></returns>
	public static (XElement, List<XElement>)
		SetGroupId(
			this (XElement, List<XElement>) groupAndUnits,
			Guid guid
	)
	{
		groupAndUnits.Item1.SetAttributeValue("Id", guid);
		groupAndUnits.Item2
			.Select(v =>
			{
				v.SetAttributeValue("Group", guid);
				return v;
			});
		return groupAndUnits;
	}
}

/// <summary>
/// CCSファイル管理クラス
/// </summary>
public class CcsProject : CeVIOFileBase
{
	/// <inheritdoc/>
	public CcsProject(XDocument xml)
		: base(xml)
	{
	}
}

/// <summary>
/// CCSTファイル管理クラス
/// </summary>
public class CcstTrack : CeVIOFileBase
{
	/// <inheritdoc/>
	public CcstTrack(XDocument xml)
		: base(xml)
	{
	}
}
