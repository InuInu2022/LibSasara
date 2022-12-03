using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;

namespace LibSasara.Model.Serialize;

/// <summary>
/// Unit for song track
/// </summary>
public partial record Unit
{
	/*
	/// <inheritdoc cref="Unit.Category"/>
	[XmlAttribute]
	new public Category Category { get; set; }
	*/

	/// <summary>
	/// <inheritdoc cref="Song"/>
	/// </summary>
	[XmlElement]
	public Song? Song { get; set; }

	///<inheritdoc cref="Song"/>
	[XmlIgnore]
	[Browsable(false)]
	public bool SongSpecified => Song is not null;
}

/// <summary>
/// ソング情報
/// </summary>
public record Song: IHasVersion
{
	/// <summary>
	/// テンポ情報リスト
	/// </summary>
	[XmlArray]
	[XmlArrayItem]
	public List<Sound>? Tempo { get; set; }

	/// <summary>
	/// Beat情報リスト
	/// </summary>
	[XmlArray]
	[XmlArrayItem]
	public List<Time>? Beat { get; set; }

	/// <summary>
	/// スコア情報リスト
	/// </summary>
	[XmlArray]
	[XmlArrayItem(typeof(ScoreObject))]
	[XmlArrayItem(typeof(Key))]
	[XmlArrayItem(typeof(Dynamics))]
	[XmlArrayItem(typeof(Note))]
	public List<ScoreObject>? Score { get; set; }

	/// <summary>
	/// 調声データ
	/// </summary>
	[XmlElement]
	public Parameter? Parameter { get; set; }

	///<inheritdoc/>
	[XmlIgnore]
	public Version? Version {
		get=> new(VersionString);
		set { VersionString = value?.ToString(); }
	}
	/// <inheritdoc cref="Version"/>
    [XmlAttribute(nameof(Version))]
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
    public string? VersionString { get; set; }
}

/// <inheritdoc cref="Song.Parameter"/>
public record Parameter
{
	/// <summary>
	/// TMG
	/// </summary>
	//[XmlArray]
	//[XmlArrayItem]
	[XmlElement]
	public Parameters? Timing { get; set; }

	/// <summary>
	/// PIT
	/// </summary>
	//[XmlArray]
	//[XmlArrayItem(typeof(TuneData))]
	//[XmlArrayItem("Data", typeof(Data))]
	//[XmlArrayItem("NoData", typeof(NoData))]
	[XmlElement]
	public Parameters? LogF0 { get; set; }

	/// <summary>
	/// VOL
	/// </summary>
	[XmlElement]
	public Parameters? C0 { get; set; }

	/// <summary>
	/// VIA
	/// </summary>
	//[XmlArray]
	//[XmlArrayItem]
	[XmlElement]
	public Parameters? VibAmp { get; set; }

	/// <summary>
	/// VIF
	/// </summary>
	//[XmlArray]
	//[XmlArrayItem]
	[XmlElement]
	public Parameters? VibFrq { get; set; }

	/// <summary>
	/// ALP
	/// </summary>
	//[XmlArray]
	//[XmlArrayItem]
	[XmlElement]
	public Parameters? Alpha { get; set; }

	/// <summary>
	/// HUS (VoiSona only)
	/// </summary>
	[XmlElement]
	public Parameters? Husky { get; set; }
}

/// <summary>
/// 調声データリスト
/// </summary>
public record Parameters{
	/// <summary>
	/// 調声データの総数
	/// </summary>
	[XmlAttribute("Length")]
	public int Length { get; set; }

	/// <summary>
	/// 調声データリスト
	/// </summary>
	[XmlElement(typeof(TuneData))]
	[XmlElement("Data", typeof(Data))]
	[XmlElement("NoData", typeof(NoData))]
	public List<TuneData>? Data { get; set; }

	/// <summary>
	/// 省略無しの調声データリストを取得
	/// </summary>
	/// <remarks>
	/// 省略なし、<see cref="Length"/>数に展開された<see cref="Data"/>が返ります。データの無い場合で<see cref="NoData"/>ではない場合は<see cref="TuneData.Repeat"/>の無い<see cref="TuneData"/>になります。
	/// </remarks>
	/// <returns>省略なし、<see cref="Length"/>数に展開された<see cref="Data"/></returns>
	public List<ITuneData> GetFullData()
	{
		var fullA = Enumerable
			.Range(0, Length)
			.Select((_, i) => new TuneData() { Index = i })
			.ToList<TuneData>()
			;

		var fullB = new List<ITuneData>();
		if(Data is null || Data.Count == 0){
			//調声データが記録されていない場合は埋める
			fullB = Enumerable
				.Range(0, Length)
				.Select((_, i) => new TuneData() { Index = i })
				.ToList<ITuneData>();
		}
		else{
			//展開する
			var firstData = Data[0];
			int counter = 0;
			if (firstData.Index > 0)
			{
				counter = firstData.Index +1;
			}

			foreach(var item in Data){
				counter++;
				int nIndex = item.Index switch
				{
					//Indexがデフォルト値・未指定ならカウンター
					int x when x <= 0=> counter,
					//それ以外ならIndexの値を使う
					_ => item.Index
				};
				var append = Enumerable
					.Range(nIndex, item.Repeat==0 ? 1 : item.Repeat)
					.Select((_, i)
						=> item switch
						{
							NoData d => new NoData() { Index = nIndex+i },
							Data d => new Data() { Index = nIndex+i, Value = (item as Data)?.Value ?? 0 },
							_ => new TuneData() { Index = nIndex+i },
						}
					)
					.ToList()
					;

				fullB = fullB
					.Concat(append)
					.ToList();
				counter += item.Repeat;
			}
		}

		//var hasNoData = fullB.Any(v => v is NoData);

		return fullA
			.GroupJoin(
				fullB,
				a => a.Index,
				b => b.Index,
				(va, vb) => {
					var v = vb?.FirstOrDefault();
					if(v is null)
					{
						return va;
					}

					var t = v.GetType();
					if(t == typeof(Data) || t == typeof(NoData)){
						return v;
					}else{
						return va;
					}
				}
			)
			.ToList();
	}
}

/// <summary>
/// 調声データ
/// </summary>
public interface ITuneData
{
	/// <summary>
	/// インデックス
	/// </summary>
	/// <remarks>
	/// 省略可能。
	/// </remarks>
	int Index { get; set; }

	/// <summary>
	/// 同じ値の繰り返し数
	/// </summary>
	int Repeat { get; set; }
}

/// <summary>
/// 調声データ
/// </summary>
public record TuneData : ITuneData
{
	/// <inheritdoc/>
	[XmlAttribute]
	public int Index { get; set; } = -1;

	/// <inheritdoc/>
	[Browsable(false)]
	[XmlIgnore]
	public bool IndexSpecified => Index >= 0;

	/// <inheritdoc/>
	[XmlAttribute]
	public int Repeat { get; set; }

	/// <inheritdoc/>
	[Browsable(false)]
	[XmlIgnore]
	public bool RepeatSpecified => Repeat > 0;
}

/// <summary>
/// 調整データ
/// </summary>
public record Data: TuneData, ITuneData
{
	/// <summary>
	/// 調声の値
	/// </summary>
	[XmlText]
	public decimal Value { get; set; }
}
/// <summary>
/// 無効化された調整データ
/// </summary>
public record NoData: TuneData, ITuneData
{
}

/// <summary>
/// tickの時刻指定を持つ
/// </summary>
public abstract record ClockObject
{
	/// <summary>
	/// 切替時刻(単位：tick)
	/// </summary>
	[XmlAttribute]
	public int Clock { get; set; }
}

/// <summary>
/// テンポ変更情報
/// </summary>
public record Sound : ClockObject
{
	/// <summary>
	/// テンポ指定
	/// </summary>
	[XmlAttribute]
	public double Tempo { get; set; }
}

/// <summary>
/// ビート(拍子)切替指定
/// </summary>
public record Time : ClockObject
{
	/// <summary>
	/// 1小節中の拍数
	/// </summary>
	[XmlAttribute]
	public uint Beats { get; set; }

	/// <summary>
	/// 音価（1拍の長さの基準）
	/// </summary>
	[XmlAttribute]
	public uint BeatType { get; set; }
}

/// <summary>
/// スコア情報の抽象クラス
/// </summary>
public abstract record ScoreObject : ClockObject
{
}

/// <summary>
/// キー(調号)指定・変更
/// </summary>
public record Key : ScoreObject
{
	/// <summary>
	/// Fifths
	/// </summary>
	[XmlAttribute]
	public int Fifths { get; set; }
	/// <summary>
	/// Mode
	/// </summary>
	[XmlAttribute]
	public int Mode { get; set; }
}

/// <summary>
/// ダイナミクス（強弱）指定・変更
/// </summary>
public record Dynamics : ScoreObject
{
	/// <summary>
	/// 強弱指定の値
	/// </summary>
	[XmlIgnore]
	public Model.Dynamics Value {
		get => (Model.Dynamics)Enum.ToObject(typeof(Model.Dynamics), ValueInt);
		set { ValueInt = (int)value; }
	}

	///<inheritdoc cref="Value"/>
	[XmlAttribute(nameof(Value))]
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public int ValueInt { get; set; }
		= (int)Model.Dynamics.N;
}
