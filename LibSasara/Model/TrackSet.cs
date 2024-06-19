using System.Text.RegularExpressions;
using System;
using System.Xml.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using LibSasara.Builder;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;
using CommunityToolkit.Diagnostics;

namespace LibSasara.Model;

/// <summary>
/// 「トラック」を管理するクラス。
/// </summary>
/// <typeparam name="TUnit">トラック内のUnitの型。</typeparam>
/// <remarks>
/// 「トラック」は一つの <c>Group</c>要素 と <see cref="TalkUnit"/>または<see cref="SongUnit"/>,<see cref="AudioUnit"/>の組み合わせ。
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
			var isSuccess = Enum.TryParse<Category>(val, out var result);
			return isSuccess ? result : Category.OuterAudio;
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
			.GetUnits(Category)
			.Where(v => v.Group == GroupId)
			.Cast<TUnit>()
			.ToList();
		//set => throw new NotImplementedException(nameof(Units));
	}

	/// <summary>
	/// 生のGroup要素
	/// </summary>
	public XElement RawGroup {
		get => _project.RawGroups
			.First(v => string.Equals(
				v.Attribute("Id")?.Value,
				GroupId.ToString(),
				StringComparison.Ordinal));
	}

	/// <summary>
	/// 生のUnits要素
	/// </summary>
	public List<XElement> RawUnits {
		get => _project
			.GetUnitsRaw(Category)
			.Where(e
				=> string.Equals(e.Attribute("Group")?.Value, GroupId.ToString(), StringComparison.Ordinal))
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
	public bool Equals(TrackSet<TUnit>? other)
	{
		return GroupId == other?.GroupId
			&& string.Equals(CastId,
					other.CastId,
					StringComparison.Ordinal)
			&& Category == other.Category
			&& string.Equals(Name,
					other.Name,
					StringComparison.Ordinal);
	}

	/// <inheritdoc cref="IEquatable{T}.Equals(T)"/>
	public override bool Equals(object? obj)
	{
		if (obj is null) return false;
		if (obj is not TrackSet<TUnit> other) return false;
		return Equals(other);
	}

	/// <inheritdoc/>
	public override int GetHashCode()
	{
		return base.GetHashCode();
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
			throw new InvalidDataException();
		}
	}

	/// <inheritdoc cref="AddUnit(TimeSpan, TimeSpan, string)"/>
    /// <param name="unit"></param>
	public TUnit AddUnit(TUnit unit){
		Guard.IsNull(unit, nameof(AddUnit));
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
		var val = (RawGroup
			.Attribute(name)?
			.Value)
			?? throw new InvalidDataException($"attribute {nameof(name)} is null");
		return Type.GetTypeCode(typeof(T)) switch
		{
			TypeCode.String => (T)(object)val,
			TypeCode.Boolean => (T)(object)Convert.ToBoolean(val, CultureInfo.InvariantCulture),
			TypeCode.Double => (T)(object)Convert.ToDouble(val, CultureInfo.InvariantCulture),
			TypeCode.Int32 => (T)(object)Convert.ToInt32(val, CultureInfo.InvariantCulture),
			TypeCode.Byte => (T)(object)Convert.ToByte(val, CultureInfo.InvariantCulture),
			TypeCode.Char => (T)(object)Convert.ToChar(val, CultureInfo.InvariantCulture),
			TypeCode.DateTime => (T)(object)Convert.ToDateTime(val, CultureInfo.InvariantCulture),
			TypeCode.Decimal => (T)(object)Convert.ToDecimal(val, CultureInfo.InvariantCulture),
			TypeCode.Int16 => (T)(object)Convert.ToInt16(val, CultureInfo.InvariantCulture),
			TypeCode.Int64 => (T)(object)Convert.ToInt64(val, CultureInfo.InvariantCulture),
			TypeCode.SByte => (T)(object)Convert.ToSByte(val, CultureInfo.InvariantCulture),
			TypeCode.Single => (T)(object)Convert.ToSingle(val, CultureInfo.InvariantCulture),
			TypeCode.UInt16 => (T)(object)Convert.ToUInt16(val, CultureInfo.InvariantCulture),
			TypeCode.UInt32 => (T)(object)Convert.ToUInt32(val, CultureInfo.InvariantCulture),
			TypeCode.UInt64 => (T)(object)Convert.ToUInt64(val, CultureInfo.InvariantCulture),
			_ => throw new NotSupportedException()
		};
	}

	bool GetGroupValueBool(string name) =>
		GetGroupValue<bool>(name);

	double GetGroupValueDouble(string name) =>
		GetGroupValue<double>(name);

	string GetGroupValueString(string name) =>
		GetGroupValue<string>(name);

	void SetGroupValue<T>(string name, T value)
		where T: notnull
	=> RawGroup
		.Attribute(name)?
		.SetValue(value)
		;
	#endregion internal Group Element accessor
}