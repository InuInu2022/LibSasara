using System.Text.RegularExpressions;
using System;
using System.Xml.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using LibSasara.Builder;
using System.Threading.Tasks;

namespace LibSasara.Model;

/// <summary>
/// 「トラック」を管理するクラス。
/// </summary>
/// <typeparam name="TUnit">トラック内のUnitの型。</typeparam>
/// <remarks>
/// 「トラック」は一つの<see cref="Group"/>と <see cref="TalkUnit"/>または<see cref="SongUnit"/>,<see cref="AudioUnit"/>の組み合わせ。
/// トークトラックの場合は複数の<see cref="TalkUnit"/>を含みます。
/// </remarks>
public class TrackSet<TUnit> : IEquatable<TrackSet<TUnit>>
	where TUnit : UnitBase
{
	/// <summary>
	/// Group要素のバージョン
	/// </summary>
	/// <remarks>
	/// CeVIO AI ver8.4時点では <c>1.0</c> 固定？
	/// </remarks>
	public Version GroupVersion { get; set; }
		= new Version(1, 0);

	/// <summary>
	/// トラックGroupのGuid
	/// </summary>
	[Category("Group")]
	public Guid GroupId { get; set; }

	/// <summary>
	/// トラックの名前
	/// </summary>
	[Category("Group")]
	public string Name {
		get => GetGroupValueString(nameof(Name));
		set => SetGroupValue(nameof(Name), value);
	}

	/// <summary>
	/// トラックのカテゴリ
	/// </summary>
	public Category Category {
		get {
			var val = GetGroupValueString(nameof(Category));
			Enum.TryParse<Category>(val, out var result);
			return result;
		}

		set { SetGroupValue(nameof(Category), value); }
	}

	/// <summary>
	/// トラックの背景色
	/// </summary>
	[Category("Group")]
	public string Color {
		get => GetGroupValueString(nameof(Color));
	}

	/// <summary>
	/// トラックのボリューム
	/// </summary>
	/// <value>dB</value>
	[Category("Group")]
	public double Volume {
		get => GetGroupValueDouble(nameof(Volume));
		set => SetGroupValue(nameof(Volume), value);
	}

	/// <summary>
	/// トラックのパン
	/// </summary>
	/// <value>左右中央・初期値は<c>0</c></value>
	[Category("Group")]
	public double Pan {
		get => GetGroupValueDouble(nameof(Pan));
		set => SetGroupValue(nameof(Pan), value);
	}

	/// <summary>
	/// トラックのソロ再生状態
	/// </summary>
	[Category("Group")]
	public bool IsSolo {
		get => GetGroupValueBool(nameof(IsSolo));
		set => SetGroupValue(nameof(IsSolo), value);
	}

	/// <summary>
	/// トラックのミュート状態
	/// </summary>
	[Category("Group")]
	public bool IsMuted {
		get => GetGroupValueBool(nameof(IsMuted));
		set => SetGroupValue(nameof(IsMuted), value);
	}

	/// <summary>
	/// トラックのキャストID
	/// </summary>
	/// <remarks>
	/// 複数キャストのTalkトラック、Audioトラックは <c>Mixed</c>
	/// </remarks>
	public string CastId {
		get => GetGroupValueString(nameof(CastId));
		set => SetGroupValue(nameof(CastId), value);
	}

	/// <summary>
	/// トラックの言語を表す文字列
	/// </summary>
	/// TODO: 複数交じる時どうなるか要調査
	public string Language {
		get => GetGroupValueString(nameof(Language));
		set => SetGroupValue(nameof(Language), value);
	}

	/// <summary>
	/// 「トラック」内の全Unit
	/// </summary>
    /// <seealso cref="TalkUnitBuilder"/>
    /// <seealso cref="SongUnitBuilder"/>
    /// <seealso cref="AudioUnitBuilder"/>
    /// <seealso cref="AddUnit(TimeSpan, TimeSpan, string)"/>
    /// <seealso cref="AddUnit(TUnit)"/>
	public List<TUnit> Units
	{
		get => _project
			.GetUnits(this.Category)
			.Where(v => v.Group == this.GroupId)
			.Cast<TUnit>()
			.ToList();
		//set
	}

	/// <summary>
	/// 生のGroup要素
	/// </summary>
	public XElement RawGroup {
		get => _project.RawGroups
			.First(v => v.Attribute("Id").Value == GroupId.ToString());
	}

	/// <summary>
	/// 生のUnits要素
	/// </summary>
	public List<XElement> RawUnits {
		get => _project
			.GetUnitsRaw(this.Category)
			.Where(e
				=> e.Attribute("Group").Value == GroupId.ToString())
			.ToList();
	}

	/// <summary>
	/// 生のトラックデータ（GroupとUnit）のリストを返す
	/// </summary>
	/// <seealso cref="CeVIOFileBase.GetTrackSets"/>
	public List<(XElement group, List<XElement> units)>
	RawTrackSets {
		get => _project
			.GetTrackSets();
	}

	private readonly CeVIOFileBase _project;

	/// <inheritdoc/>
	public TrackSet(
		CeVIOFileBase project,
		Guid groupId
	)
	{
		GroupId = groupId;
		_project = project;
	}

	/// <inheritdoc cref="IEquatable{T}.Equals(T)"/>
	public bool Equals(TrackSet<TUnit> other)
	{
		return this.GroupId == other.GroupId
			&& this.CastId == other.CastId
			&& this.Category == other.Category
			&& this.Name == other.Name
			;
	}

	/// <summary>
    /// トラックに同じ種類のUnitを追加します
    /// </summary>
    /// <seealso cref="Builder.AudioUnitBuilder" />
    /// <seealso cref="Builder.SongUnitBuilder" />
    /// <seealso cref="Builder.TalkUnitBuilder" />
	public TUnit AddUnit(
		TimeSpan start,
		TimeSpan duration,
		string castIdOrFilePath
	){
		if(typeof(TUnit) == typeof(TalkUnit)){
			return (TUnit)(UnitBase)TalkUnitBuilder
				.Create(_project, start, duration, castIdOrFilePath, "")
				.Group(GroupId)
				.Build();
		}
		else if(typeof(TUnit) == typeof(SongUnit)){
			return (TUnit)(UnitBase)SongUnitBuilder
				.Create(_project, start, duration, castIdOrFilePath)
				.Group(GroupId)
				.Build();
		}
		else if(typeof(TUnit) == typeof(AudioUnit)){
			return (TUnit)(UnitBase)AudioUnitBuilder
				.Create(_project, start, duration, castIdOrFilePath)
				.Group(GroupId)
				.Build();
		}else{
			throw new Exception();
		}
	}

	/// <inheritdoc cref="AddUnit(TimeSpan, TimeSpan, string)"/>
    /// <param name="unit"></param>
	public TUnit AddUnit(TUnit unit){
		unit.Group = GroupId;
		return unit;
	}

	/// <summary>
    /// トラック内の全ユニットを削除
    /// </summary>
	public void RemoveAllUnits(){
		RawUnits
			.ForEach(v => v.RemoveAll());
	}

	/// <summary>
	/// トラックのカテゴリに応じた背景色のカラーコードを返す
	/// </summary>
	/// <param name="category">トラックのカテゴリ</param>
	/// <returns></returns>
	public string GetBackgroundColor(Category category){
		return category switch
		{
			Category.SingerSong => "#FFAF1F14",
			Category.TextVocal => "#FF267CCB",
			_ => "#FF008000"
		};
	}

	#region internal Group Element accessor

	//TODO:test
	T GetGroupValue<T>(string name)
		where T: notnull
	{
		var val = RawGroup
			.Attribute(name)
			.Value;
		return Type.GetTypeCode(typeof(T)) switch
		{
			TypeCode.String => (T)(object)val,
			TypeCode.Boolean => (T)(object)Convert.ToBoolean(val),
			TypeCode.Double => (T)(object)Convert.ToDouble(val),
			TypeCode.Int32 => (T)(object)Convert.ToInt32(val),
			TypeCode.Byte => (T)(object)Convert.ToByte(val),
			TypeCode.Char => (T)(object)Convert.ToChar(val),
			TypeCode.DateTime => (T)(object)Convert.ToDateTime(val),
			TypeCode.Decimal => (T)(object)Convert.ToDecimal(val),
			TypeCode.Int16 => (T)(object)Convert.ToInt16(val),
			TypeCode.Int64 => (T)(object)Convert.ToInt64(val),
			TypeCode.SByte => (T)(object)Convert.ToSByte(val),
			TypeCode.Single => (T)(object)Convert.ToSingle(val),
			TypeCode.UInt16 => (T)(object)Convert.ToUInt16(val),
			TypeCode.UInt32 => (T)(object)Convert.ToUInt32(val),
			TypeCode.UInt64 => (T)(object)Convert.ToUInt64(val),
			_ => throw new NotSupportedException()
		};
	}

	bool GetGroupValueBool(string name) =>
		GetGroupValue<bool>(name);

	double GetGroupValueDouble(string name) =>
		GetGroupValue<double>(name);

	string GetGroupValueString(string name) =>
		GetGroupValue<string>(name);

	void SetGroupValue<T>(string name, T value) =>
		RawGroup
			.Attribute(name)
			.SetValue(value);

	#endregion internal Group Element accessor
}