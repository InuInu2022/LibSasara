using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace LibSasara.Model;

/// <summary>
/// ソングUnit管理用クラス
/// </summary>
/// <inheritdoc/>
public class SongUnit : UnitBase
{
	/// <inheritdoc/>
	public override Category Category { get; } = Category.SingerSong;

	/// <summary>
	/// キャスト（ボイス）の内部ID
	/// </summary>
	public string CastId
	{
		get => GetUnitAttributeStr(nameof(CastId));
		set => SetUnitAttributeStr(nameof(CastId), value);
	}

	/// <summary>
	/// Unitの言語
	/// </summary>
    /// <remarks>
    /// <c>"Japanese"</c>, <c>"English"</c>
    /// </remarks>
	public string Language
	{
		get => GetUnitAttributeStr(nameof(Language));
		set => SetUnitAttributeStr(nameof(Language), value);
	}

	/// <summary>
	/// Songの生要素
	/// </summary>
	public XElement RawSong
	{
		get => rawElem.Element("Song")
			?? new XElement("Song");

		set => rawElem.Element("Song")?
			.SetElementValue("Song", value);
	}

	/// <summary>
	/// ソングフォーマットのバージョン
	/// </summary>
	/// <remarks>
	/// ソングエンジンやボイスのバージョンとは異なります。
	/// <list type="bullet">
	///     <item>
	///        <term>VoiSona</term>
	///        <description>VoiSonaの場合は取得できません</description>
	///     </item>
	/// </list>
	/// </remarks>
	public Version SongVersion
	{
		get
		{
			if (RawSong.HasAttributes
				&& RawSong.Attribute("Version") is not null)
			{
				return new(
					RawSong
						.Attribute("Version")!
						.Value
				);
			}

			return new();
		}
	}

	/// <summary>
    /// 調号を全トラック共通で処理するかどうか
    /// </summary>
    /// <remarks>
    /// CeVIO AI ver. 8.5以降のみ。
    /// <see cref="SongVersion"/>が <c>1.08</c> 以上で存在。
    /// </remarks>
    /// <seealso href="https://cevio.jp/guide/cevio_ai/"/>
	public bool CommonKeys
	{
		get => UnitBase.GetAttrBool(RawScore, nameof(CommonKeys));

		set =>

/* プロジェクト 'LibSasara (net8)' からのマージされていない変更
前:
			SetAttr(RawScore, nameof(CommonKeys), value);
後:
			UnitBase.SetAttr(RawScore, nameof(CommonKeys), value);
*/
            SetAttr(RawScore, nameof(CommonKeys), value);
	}

	/// <summary>
	/// 曲のテンポ変更リスト
	/// </summary>
	/// <remarks>
	/// - Clock: テンポ変更開始時のtick
	/// - Tempo : テンポ
	/// </remarks>
    /// <seealso cref="Tempos"/>
    [Obsolete($"Use {nameof(Tempos)}")]
	public SortedDictionary<int, int> Tempo
	{
		get => new(
			RawSong
				.Element("Tempo")?
				.Elements("Sound")
				.ToDictionary(
					v => UnitBase.GetAttrInt(v, "Clock"),
					v => UnitBase.GetAttrInt(v, "Tempo")
				)
				?? new Dictionary<int, int>()
			);
	}

	/// <summary>
	/// 曲のテンポ変更リスト
	/// </summary>
	/// <remarks>
	/// - Clock : テンポ変更開始時のtick
	/// - Tempo : テンポ
	/// </remarks>
	public SortedDictionary<int, decimal> Tempos
	{
		get => new(
			RawSong
				.Element("Tempo")?
				.Elements("Sound")
				.ToDictionary(
					v => UnitBase.GetAttrInt(v, "Clock"),
					v => UnitBase.GetAttrDecimal(v, "Tempo")
				)
				?? new Dictionary<int, decimal>()
			)
			;
	}

	/// <summary>
	/// 曲の拍子変更リスト
	/// </summary>
	/// <value>tick, (beat, beatType)</value>
	public SortedDictionary<int, (int Beats, int BeatType)> Beat
	{
		get => new(
			RawSong
				.Element("Beat")?
				.Elements("Time")
				.ToDictionary(
					v => UnitBase.GetAttrInt(v, "Clock"),
					v => (
						Beats: UnitBase.GetAttrInt(v, "Beats"),
						BeatType: UnitBase.GetAttrInt(v, "BeatType")
					)
				)
				?? new Dictionary<int, (int,int)>()
		);
	}

	/// <summary>
	/// 生のScore要素
	/// </summary>
    /// <seealso cref="RawScores"/>
	public XElement RawScore
	{
		get => RawSong.Element("Score")
			?? new XElement("Score");
		set => RawSong.SetElementValue("Score", value);
	}

	/// <summary>
	/// トラック全体の声質バー / Global Alpha(ALP)
	/// </summary>
    /// <remarks>
    /// エディタ上で設定する数値と異なる内部的な数値で管理される。
    /// </remarks>
    /// TODO:中央値・MAX/MINの実態調査
	public decimal Alpha
	{
		get => UnitBase.GetAttrDecimal(RawScore, nameof(Alpha));
		set => UnitBase.SetAttr(RawScore, nameof(Alpha), value);
	}

	/// <summary>
	/// トラック全体の基準ピッチ / Tuning pitch
	/// </summary>
	/// <remarks>
    /// トラック全体の基準となるピッチの周波数の値。
    /// 指定がない場合は 440.0 が返ります。
	/// VoiSona v1.0、CeVIO AI v8.5以降
    /// <see cref="SongVersion"/> >= 1.8
	/// </remarks>
    /// <value>Hz. default: 440.0, max: 450.0, min: 430.0</value>
	public decimal PitchShift
	{
		get => UnitBase.GetAttrDecimal(RawScore, nameof(PitchShift), 440.0m);

		set => UnitBase.SetAttr(RawScore, nameof(PitchShift), value);
	}

	/// <summary>
	/// トラック全体のTUNEパラメータ / Global TUNE
	/// </summary>
	/// <remarks>
	/// ピッチの音符の音程への忠実度合いを調整する。
	/// VoiSona v1.2、CeVIO AI v8.5以降
	/// <see cref="SongVersion"/> >= 1.8
	/// </remarks>
	/// <value>default: 0.0, max: 1.0, min: -1.0</value>
	public decimal PitchTune
	{
		get => UnitBase.GetAttrDecimal(RawScore, nameof(PitchTune));
		set => UnitBase.SetAttr(RawScore, nameof(PitchTune), value);
	}

	/// <summary>
	/// トラック全体のハスキーパラメータ / Global Huskiness(HUS)
	/// </summary>
	/// <remarks>
	/// トラック全体のかすれ具合のパラメータ。
    /// VoiSona v1.0、CeVIO AI v8.5以降
    /// <see cref="SongVersion"/> >= 1.8
	/// </remarks>
    /// <value>
    /// - VoiSona: max: 10.0, min: -10.0, default: 0.0<br/>
    /// - CeVIO AI: max: 1.0, min: -1.0, default: 0.0
    /// </value>
	public decimal Husky
	{
		get => UnitBase.GetAttrDecimal(RawScore, nameof(Husky));
		set => UnitBase.SetAttr(RawScore, nameof(Husky), value);
	}

	/// <summary>
	/// 生のScore要素の子要素一覧
	/// </summary>
    /// <remarks>
    /// Score要素の子要素のみのリスト(<see cref="List{XElement}"/>)です。Score要素そのものは <see cref="RawScore"/> を使用して下さい。
    /// </remarks>
    /// <seealso cref="RawScore"/>
	public IList<XElement> RawScores
	{
		get => RawSong?
			.Element("Score")?
			.Elements()?
			.ToList()
			?? Enumerable
				.Empty<XElement>().ToList();

		set => RawSong?
			.Element("Score")?
			.SetElementValue("Score", value);
	}

	/// <summary>
	/// 音符データのリスト
	/// </summary>
	public List<Note> Notes
	{
		get => RawScores
			.Where(v => v.Name == "Note")
			.Select(v =>
			{
				var n = new Note()
				{
					Clock = UnitBase.GetAttrInt(v, "Clock"),
					PitchStep = UnitBase.GetAttrInt(v, "PitchStep"),
					PitchOctave = UnitBase.GetAttrInt(v, "PitchOctave"),
					Duration = UnitBase.GetAttrInt(v, "Duration"),
					Lyric = v.Attribute("Lyric")?.Value,
					Phonetic = v.Attribute("Phonetic")?.Value,
					DoReMi = UnitBase.GetAttrBool(v, "DoReMi"),
					Breath = UnitBase.GetAttrBool(v, "Breath"),
					SlurStart = UnitBase.GetAttrBool(v, "SlurStart"),
					SlurStop = UnitBase.GetAttrBool(v, "SlurStop"),
					SyllabicInt = UnitBase.GetAttrInt(v, "Syllabic"),
				};
				//n.Phonetic =
				return n;
			})
			.ToList();
	}

	private const string ElemNameParameter = "Parameter";

	/// <summary>
	/// 生のParameter要素の子要素一覧
	/// </summary>
    /// <seealso cref="RawParameterChildren"/>
	[Obsolete($"Use {nameof(RawParameterChildren)}")]
	public List<XElement> RawParameters
	{
		get => RawParameterChildren;
		set => RawParameterChildren = value;
	}

	/// <summary>
	/// 生のParameter要素の子要素一覧
	/// </summary>
    /// <seealso cref="RawParameter"/>
	public List<XElement> RawParameterChildren
	{
		get => RawParameter?
			.Elements()?
			.ToList() ??
			new List<XElement>();

		set
		{
			var paramElem = RawParameter;

			if(paramElem is null){
				RawSong?
					.Add(new XElement(ElemNameParameter));
				paramElem = RawSong?
					.Element(ElemNameParameter);
			}

			paramElem?
				.SetElementValue(ElemNameParameter, value);
		}
	}

	/// <summary>
    /// 生のParameter要素
    /// </summary>
	public XElement? RawParameter
	{
		get => RawSong?
			.Element(ElemNameParameter);

		set => RawSong?
			.SetElementValue(ElemNameParameter, value);
	}

	/// <summary>
	/// 生のTiming(TMG)要素
	/// </summary>
	public XElement RawTiming
	{
		get => GetRawParameterNodes("Timing");

		set => SetRawParameterNodes("Timing", value);
	}

	/// <summary>
    /// 生のPitch(PIT)要素（LogF0)
    /// </summary>
	public XElement RawPitch
	{
		get => GetRawParameterNodes("LogF0");

		set => SetRawParameterNodes("LogF0", value);
	}

	/// <summary>
    /// 生のVolume(VOL)要素（C0）
    /// </summary>
	public XElement RawVolume
	{
		get => GetRawParameterNodes("C0");

		set => SetRawParameterNodes("C0", value);
	}

	/// <summary>
    /// VOLの調声パラメータ
    /// </summary>
	/// <remarks>
	/// - Length : パラメータ総数（0.005間隔）
	/// </remarks>
    /// <seealso cref="Serialize.Data"/>
    /// <seealso cref="Serialize.NoData"/>
	public Serialize.Parameters Volume
	{
		get => SongUnit.GetParams(RawVolume, "C0");

		set {
			if(value is null){
				throw new XmlException("setted Parameter is null");
			}

			if(RawVolume.HasElements){
				//子要素削除
				RawVolume.RemoveNodes();
			}

			RawVolume
				.SetAttributeValue("Length", value.Length);

			value
				.Data?
				.ConvertAll(d =>
				{
					var isData = d is not Serialize.NoData;
					var n = isData ? "Data" : "NoData";

					var e = new XElement(n);
					if(d is Serialize.Data d2){
						e.SetValue(d2.Value);
					}

					if(d.Index >= 0){
						e.SetAttributeValue("Index", d.Index);
					}

					if(d.Repeat > 0)
					{
						e.SetAttributeValue("Repeat", d.Repeat);
					}

					return e;
				})
				.Where(e => !string.IsNullOrEmpty(e.Value))
				.ToList()
				.ForEach(e => RawVolume.Add(e))
				;
		}
	}

	/// <summary>
	///生のVIA要素（VibAmp）
	/// </summary>
	public XElement RawVibratoAmp
	{
		get => GetRawParameterNodes("VibAmp");

		set => SetRawParameterNodes("VibAmp", value);
	}

	/// <summary>
    /// 生のVIF要素（VibFrq）
    /// </summary>
	public XElement RawVibratoFrq
	{
		get => GetRawParameterNodes("VibFrq");

		set => SetRawParameterNodes("VibFrq", value);
	}

	/// <summary>
    /// 生のAlpha(ALP)要素
    /// </summary>
	public XElement RawAlpha
	{
		get => GetRawParameterNodes("Alpha");

		set => SetRawParameterNodes("Alpha", value);
	}

	/// <summary>
    /// 生のHuskiness(HUS)要素（Husky)
    /// </summary>
	public XElement RawHuskiness
	{
		get => GetRawParameterNodes("Husky");

		set => SetRawParameterNodes("Husky", value);
	}

	/// <inheritdoc/>
    /// <seealso cref="Builder.SongUnitBuilder"/>
	public SongUnit(XElement elem, CeVIOFileBase root)
		: base(elem, root)
	{
	}

	/// <summary>
	/// SongのUnit要素生成
	/// </summary>
	/// <remarks>
	/// <para>
	/// SongのUnit要素の<see cref="XElement"/>を生成します。
	/// </para>
	/// <para>
	/// 生成するだけで<see cref="CeVIOFileBase"/>には紐付けません。
	/// <see cref="Builder.SongUnitBuilder"/>も活用してください。
	/// </para>
	/// </remarks>
	/// <param name="StartTime"></param>
	/// <param name="Duration"></param>
	/// <param name="CastId"></param>
	/// <param name="Group"></param>
	/// <param name="Language"></param>
	/// <param name="songVersion">Song要素のversion。CS7,AI 8.4までは <c>1.07</c>。AI 8.5以降は <c>1.08</c><br/>see: <seealso cref="SongVersion"/></param>
	/// <param name="tempo"></param>
	/// <param name="beat"></param>
	/// <returns>SongのUnit要素の<see cref="XElement"/></returns>
	/// <seealso cref="Builder.TalkUnitBuilder"/>
	//TODO: Add test
	public static XElement CreateSongUnitRaw
	(
		TimeSpan StartTime,
		TimeSpan Duration,
		string CastId,
		Guid? Group = null,
		string? Language = "Japanese",
		string songVersion = "1.07",
		SortedDictionary<int, int>? tempo = null,
		SortedDictionary<int, (int Beats, int BeatType)>? beat = null
	)
	{
		XAttribute[] attrs = {
			new("Version","1.0"),	//
			new("Id",""),			//
			new("Category", nameof(Category.SingerSong)),
			new(nameof(StartTime),StartTime.ToString("c")),
			new(nameof(Duration),Duration.ToString("c")),
			new(nameof(CastId),CastId),
			new(
				nameof(Group),
				Group is null ?
					Guid.NewGuid() :
					Group.ToString()!),
			new(nameof(Language),Language ?? "Japanaese"),
		};

		var elem = new XElement("Unit", attrs);
		var songElem = new XElement(
			"Song",
			new XAttribute("Version", songVersion)
		);
		elem.Add(songElem);

		if (tempo is not null)
		{
			var tempos = tempo
				.Select(v =>
				{
					var c = new XElement("Sound");
					c.SetAttributeValue("Clock", v.Key);
					c.SetAttributeValue("Tempo", v.Value);
					return c;
				});
			var tempoElem = new XElement("Tempo");
			tempoElem.Add(tempos);
			elem
				.Add(tempoElem);
		}

		if (beat is not null)
		{
			var beats = beat
				.Select(v =>
				{
					var p = new XElement("Time");
					p.SetAttributeValue("Clock", v.Key);
					p.SetAttributeValue("Beats", v.Value.Beats);
					p.SetAttributeValue("BeatType", v.Value.BeatType);

					return p;
				})
				.ToArray();
			var beatElem = new XElement("Beat");
			beatElem.Add(beats);
			elem
				.Add(beatElem);
		}

		return elem;
	}

	XElement GetRawParameterNodes(string nodeName){
		if(RawParameter is null){
			RawSong?
				.Add(new XElement(ElemNameParameter));
		}

		var elm = RawParameterChildren?
			.Find(e => string.Equals(
				e.Name.ToString(),
				nodeName,
				StringComparison.Ordinal))
			;
		if(elm is null){
			var hasNode = RawParameter?
				.Element(nodeName) is not null;
			if(!hasNode){
				var x = new XElement(
					nodeName,
					new XAttribute("Length", 0)
				);
				RawParameter?
					.Add(x);
				elm = x;
			}else{
				elm = RawParameterChildren?
					.Elements(nodeName)
					.FirstOrDefault();
			}
			elm ??= new XElement(nodeName);
		}

		return elm;
	}

	static Serialize.Parameters GetParams(XElement raw, string paramName)
	{
		if(!raw.HasElements){
			return new Serialize.Parameters(paramName)
			{
				Length = GetAttrInt(raw, "Length"),
			};
		}

		var len = UnitBase.GetAttrInt(raw, "Length");
		var data = raw.Elements()
			.Select(e =>
			{
				Serialize.TuneData n = (e.Name == "NoData") ?
					new Serialize.NoData() :
					new Serialize.Data();

				if (n is Serialize.Data d)
				{
					d.Value =LibSasaraUtil.ConvertDecimal(e.Value);
				}

				if (e.HasAttributes)
				{
					n.Index = UnitBase.GetAttrInt(e, "Index", -1);
				}

				if (e.Attribute("Repeat") is not null)
				{
					n.Repeat = UnitBase.GetAttrInt(e, "Repeat");
				}

				return n;
			});
		return new Serialize.Parameters(paramName)
		{
			Length = len,
			Data = data.ToList(),
		};
	}

	//TODO:test
	void SetRawParameterNodes(string nodeName, XElement value){
		GetRawParameterNodes(nodeName)
			.SetElementValue("nodeName", value);
	}
}
