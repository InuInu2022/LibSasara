using System.Collections.Generic;
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
	public List<ITuneData> GetFullData(int length)
	{
		var fullA = Enumerable
			.Range(0, length)
			.Select((_, i) => new TuneData() { Index = i })
			.ToList()
			;

		var fullB = new List<TuneData>();
		if (Data is null || Data.Count == 0)
		{
			//調声データが記録されていない場合は埋める
			return fullA.Cast<ITuneData>().ToList();
		}

		//展開する
		var firstData = Data[0];
		int counter = (firstData.Index > 0) ?
			firstData.Index + 1 : 0;

		foreach (var item in Data)
		{
			counter++;
			int nIndex = item.Index switch
			{
				//Indexがデフォルト値・未指定ならカウンター
				int x when x <= 0 => counter,
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

			fullB = fullB
				.Concat(append)
				.ToList();
			counter += item.Repeat;
		}

		return MergeData(fullA, fullB);
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
}
