using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml.Serialization;

namespace LibSasara.Model.Serialize;

/// <summary>
/// 調声データリスト
/// </summary>
public record Parameters{
	/// <inheritdoc cref="Parameters"/>
    /// <param name="name">調声データの名前<seealso cref="Name"/></param>
	public Parameters(string name)
	{
		Name = name;
	}

	/// <summary>
	/// 調声データの名前
	/// </summary>
	public string Name { get; }
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
	/// <param name="length">調声データ数</param>
	/// <remarks>
	/// 元の調声データが無い、または少ない場合に上書き生成する場合はこちらを使用してください。
	/// 省略なし、<paramref name="length"/> 数に展開された<see cref="Data"/>が返ります。データの無い場合で<see cref="NoData"/>ではない場合は<see cref="TuneData.Repeat"/>の無い<see cref="TuneData"/>になります。
	/// </remarks>
	/// <returns>省略なし、<paramref name="length"/> 数に展開された<see cref="Data"/></returns>
	[SuppressMessage("","HLQ012")]
	public List<ITuneData> GetFullData(int length)
	{
		var fullA = Enumerable
			.Range(0, length)
			.Select((_, i) => new TuneData() { Index = i })
			.ToList()
			;

		if (Data is null || Data.Count == 0)
		{
			//調声データが記録されていない場合は埋める
			return fullA.Cast<ITuneData>().ToList();
		}

		var fullB = new List<TuneData>();

		//展開する
		var firstData = Data[0];
		int counter = (firstData.Index > 0) ?
			firstData.Index + 1 : 0;

		foreach (var item in Data)
		{
			int nIndex = item.Index switch
			{
				//Indexがデフォルト値・未指定ならカウンター
				int x when x < 0 => counter,
				//それ以外ならIndexの値を使う
				_ => item.Index
			};
			var append = Enumerable
				.Range(nIndex, item.Repeat == 0 ? 1 : item.Repeat)
				.Select((_, i)
					=> item switch
					{
						NoData d => new NoData() { Index = nIndex + i },
						Data d => new Data() { Index = nIndex + i, Value = (item as Data)?.Value ?? 0 },
						_ => new TuneData() { Index = nIndex + i },
					}
				)
				.ToList()
				;

			fullB = [.. fullB, .. append];
			//counter = nIndex;
			counter = item.Repeat == 0 ?
					nIndex : nIndex + item.Repeat;
		}

		return [.. MergeData(fullA, fullB)];
	}

	/// <summary>
    /// 調声データの合成
    /// </summary>
    /// <param name="fullA"></param>
    /// <param name="fullB"></param>
    /// <returns></returns>
	public static List<ITuneData> MergeData(List<TuneData> fullA, List<TuneData> fullB)
	{
		return fullA
			.GroupJoin(
				fullB,
				a => a.Index,
				b => b.Index,
				(va, vb) =>
				{
					var v = vb?.FirstOrDefault();
					if (v is null)
					{
						return va;
					}

					var t = v.GetType();
					if (t == typeof(Data) || t == typeof(NoData))
					{
						return v;
					}
					else
					{
						return va;
					}
				}
			)
			.Cast<ITuneData>()
			.ToList();
	}

	/// <summary>
	/// 省略無しの調声データリストを取得
	/// </summary>
	/// <remarks>
	/// 省略なし、<see cref="Length"/>数に展開された<see cref="Data"/>が返ります。データの無い場合で<see cref="NoData"/>ではない場合は<see cref="TuneData.Repeat"/>の無い<see cref="TuneData"/>になります。
	/// </remarks>
	/// <returns>省略なし、<see cref="Length"/>数に展開された<see cref="Data"/></returns>
	public List<ITuneData> GetFullData()
	{
		return GetFullData(Length);
	}

	/// <summary>
    /// 圧縮する
    /// </summary>
    /// <param name="data"></param>
	public static List<ITuneData> ShrinkData(
		List<ITuneData> data
	)
	{
		var shrinked = new List<ITuneData>();
		var valid = data
			.Where(v => v is Data || v is NoData);

		ITuneData? prev = null;
		foreach(var i in valid){
			//最初
			if(prev is null){
				shrinked.Add(i);
				prev = i;
				continue;
			}

			//以降
			if(i is Data d){
				if(prev is Data pd){
					if (prev.Index + prev.Repeat >= i.Index && pd.Value == d.Value)
					{
						//同じValueならRepeat++
						pd.Repeat++;
						continue;
					}
				}

				//違うなら新しく追加
				shrinked.Add(i);
				prev = i;
				continue;
			}else{
				if(prev is NoData){
					//同じNoDataならRepeat++
					prev.Repeat++;
					continue;
				}

				//違うなら新しく追加
				shrinked.Add(i);
				prev = i;
				continue;
			}
		}

		return shrinked;
	}
}
