using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace LibSasara.Model;

/// <summary>
/// トークUnit管理用クラス
/// </summary>
public class TalkUnit : UnitBase
{
	/// <inheritdoc/>
	public override Category Category { get; } = Category.TextVocal;

	/// <summary>
	/// キャスト（ボイス）の内部ID
	/// </summary>
	public string CastId
	{
		get => GetUnitAttributeStr(nameof(CastId));
		set => SetUnitAttribureStr(nameof(CastId), value);
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
		set => SetUnitAttribureStr(nameof(Language), value);
	}

	/// <summary>
	/// セリフ文字列
	/// </summary>
    /// <remarks>日本語200文字、英語500文字まで。</remarks>
	public string Text
	{
		get => GetUnitAttributeStr(nameof(Text));
		set => SetUnitAttribureStr(nameof(Text), value);
	}

	/// <summary>
	/// 調声メタデータの生データ文字列
	/// </summary>
	public string? RawMetadata
	{
		get => rawElem.Descendants("Metadata").First().Value;
		set => rawElem.Descendants("Metadata").First().SetValue(value);
	}

	/// <summary>
	/// 調声メタデータ
	/// </summary>
	//TODO: バイナリ解析処理も追加
	// RESEARCH: base64
	public List<byte[]> Metadata
	{
		get
		{
			if (string.IsNullOrEmpty(RawMetadata))
			{
				return new();
			}

			return RawMetadata!
				.Split(';')
				.Select(s => PaddingForBase64(s))
				.Select(s => Convert.FromBase64String(s))
				.ToList()
				;
		}
	}

	/// <summary>
	/// get/set a raw Direction element
	/// </summary>
	public XElement RawDirection
	{
		get => rawElem.Element("Direction");

		set => rawElem.Element("Direction")
			.SetElementValue("Direction", value);
	}

	/// <summary>
	/// セリフのコンディションパラメータ
	/// </summary>
	/// <remarks>
	/// <list type="bullet">
	/// 	<item>
	/// 		<term>Volume</term>
	///		    <description>大きさ。<see cref="Volume"/></description>
	/// 	</item>
	/// 	<item>
	/// 		<term>Speed</term>
	///		    <description>速さ。<see cref="Speed"/></description>
	/// 	</item>
	/// 	<item>
	/// 		<term>Tone</term>
	///		    <description>高さ。<see cref="Tone"/></description>
	/// 	</item>
	/// 	<item>
	/// 		<term>Alpha</term>
	///		    <description>声質。<see cref="Alpha"/></description>
	/// 	</item>
	/// 	<item>
	/// 		<term>LogF0Scale</term>
	///		    <description>抑揚。<see cref="LogF0Scale"/></description>
	/// 	</item>
	/// </list>
	/// </remarks>
	public (decimal Volume, decimal Speed, decimal Tone, decimal Alpha, decimal LogF0Scale) Directions
	{
		get => (
			Volume,
			Speed,
			Tone,
			Alpha,
			LogF0Scale
		);

		set => (Volume, Speed, Tone, Alpha, LogF0Scale) = value;
	}

	/// <summary>
	/// 大きさ
	/// </summary>
	/// <value>max: 8.0, min: -8.0</value>
	public decimal Volume
	{
		get => GetDirectionValue(nameof(Volume));
		set => SetDicrectionValue(nameof(Volume), value);
	}

	/// <summary>
	/// 速さ
	/// </summary>
	/// <value>max: 5.0, min: 0.2</value>
	public decimal Speed
	{
		get => GetDirectionValue(nameof(Speed));
		set => SetDicrectionValue(nameof(Speed), value);
	}

	/// <summary>
	/// 高さ
	/// </summary>
	/// <value>max: 6.0, min: -6.0</value>
	public decimal Tone
	{
		get => GetDirectionValue(nameof(Tone));
		set => SetDicrectionValue(nameof(Tone), value);
	}

	/// <summary>
	/// 声質
	/// </summary>
	/// <value>max: 0.6, min: 0.5</value>
	public decimal Alpha
	{
		get => GetDirectionValue(nameof(Alpha));
		set => SetDicrectionValue(nameof(Alpha), value, "0.00");
	}

	/// <summary>
	/// 抑揚
	/// </summary>
	/// <value>max: 2.0, min: 0.0</value>
	public decimal LogF0Scale
	{
		get => GetDirectionValue(nameof(LogF0Scale));
		set => SetDicrectionValue(nameof(LogF0Scale), value, "0.00");
	}

	/// <summary>
	/// 感情パラメータの生要素リスト
	/// </summary>
	public List<XElement> RawComponents
	{
		get => RawDirection.HasElements ?
			RawDirection.Elements("Component").ToList() :
			new();
		//set => RawDirection.ReplaceNodes()
	}

	/// <summary>
	/// 感情パラメータ一覧
	/// </summary>
	/// <remarks>
	/// - CeVIO AIの場合はボイス関連の感情パラメータ一覧
	/// - CeVIO CSの場合は全ボイス共通の感情パラメータ一覧
	/// </remarks>
	/// <returns>感情パラメータの内部IDと値の<see keyword="Tuple"/>リスト</returns>
	public List<(string Id, decimal Value)> Components
	{
		get
		{
			var version = Root.AutherVersion?.Major;
			if (version is not null && version <= 7)
			{
				//cevio cs
				return RawComponents
					.ConvertAll(e =>
						(
							Id: e.Attribute("Name").Value,
							Value: SasaraUtil
								.ConvertDecimal(
									e.Attribute("Value").Value
								)
						))
					;
			}
			else if (version is not null && version >= 8)
			{
				//cevio ai
				return RawComponents
					.Where(e =>
						Regex.IsMatch(
							e.Attribute("Name").Value,
							$"{CastId}_.+"
						)
					)
					.Select(e =>
					{
						return (
							Id: e.Attribute("Name").Value,
							Value: SasaraUtil
								.ConvertDecimal(
									e.Attribute("Value").Value
								)
						);
					})
					.ToList()
					;
			}
			else
			{
				return new();
			}
		}

		set
		{
			value.ForEach(v =>
			{
				var c = RawComponents
					//.Elements("Component")
					//.ToList()
					.FindAll(x => x.Attribute("Name").Value == v.Id)
					;
				c.ForEach(x =>
					x.Attribute("Value").SetValue(v.Value.ToString())
				)
				;
			});
		}
	}

	/// <summary>
	/// 音素データ生データ
	/// </summary>
	public List<XElement> RawPhonemes
	{
		get => rawElem
			.Element("Phonemes")
			.Elements("Phoneme")
			.ToList();
	}

	/// <summary>
	/// 音素データのリスト
	/// </summary>
	/// <remarks>
	/// <seealso cref="SasaraLabel"/>
	/// </remarks>
	public List<TalkPhoneme> Phonemes
	{
		get => RawPhonemes
			.Select<XElement, TalkPhoneme>((v, i) => new()
				{
					Index = i,
					Data = v.Attribute("Data")?.Value,
					Volume = (v.Attribute("Volume") is null) ?
								null :
								SasaraUtil.ConvertDecimal(v.Attribute("Volume").Value),
					Speed = (v.Attribute("Speed") is null) ?
								null :
								SasaraUtil.ConvertDecimal(v.Attribute("Speed").Value),
					Tone = (v.Attribute("Tone") is null) ?
								null :
								SasaraUtil.ConvertDecimal(v.Attribute("Tone").Value),
				})
			.ToList();
	}

	/// <inheritdoc/>
    /// <seealso cref="Builder.TalkUnitBuilder"/>
	public TalkUnit(XElement elem, CeVIOFileBase root)
		: base(elem, root)
	{
	}

	/// <summary>
	/// TalkのUnit要素生成
	/// </summary>
    /// <remarks>
    /// <para>
    /// TalkのUnit要素の<see cref="XElement"/>を生成します。
    /// </para>
    /// <para>
    /// 生成するだけで<see cref="CeVIOFileBase"/>には紐付けません。
    /// <see cref="Builder.TalkUnitBuilder"/>も活用してください。
    /// </para>
    /// </remarks>
	/// <param name="StartTime"></param>
	/// <param name="Duration"></param>
	/// <param name="CastId"></param>
	/// <param name="Text"></param>
	/// <param name="Group"></param>
	/// <param name="Language"></param>
	/// <param name="Volume"></param>
	/// <param name="Speed"></param>
	/// <param name="Tone"></param>
	/// <param name="Alpha"></param>
	/// <param name="LogF0Scale"></param>
	/// <param name="components"></param>
	/// <param name="phonemes"><inheritdoc cref="Phonemes"/></param>
	/// <returns>TalkのUnit要素の<see cref="XElement"/></returns>
    /// <seealso cref="Builder.TalkUnitBuilder"/>
	public static XElement CreateTalkUnitRaw
	(
		TimeSpan StartTime,
		TimeSpan Duration,
		string CastId,
		string? Text,
		Guid? Group = null,
		string? Language = "Japanese",
		decimal Volume = 0,
		decimal Speed = 1,
		decimal Tone = 0,
		decimal Alpha = 0.55m,
		decimal LogF0Scale = 1,
		IEnumerable<(string Id, decimal Value)>? components = null,
		IEnumerable<TalkPhoneme>? phonemes = null
	)
	{
		XAttribute[] attrs = {
			new("Version","1.1"),	//
			new("Id",""),			//
			new("Category", nameof(Category.TextVocal)),
			new("Text", Text ?? ""),
			new(nameof(StartTime),StartTime.ToString("c")),
			new(nameof(Duration),Duration.ToString("c")),
			new(nameof(CastId),CastId),
			new(
				nameof(Group),
				Group is null ?
					Guid.NewGuid() :
					Group.ToString()),
			new(nameof(Language),Language ?? "Japanaese"),
		};

		var elem = new XElement("Unit", attrs);
		XAttribute[] dirAttr = {
			new(nameof(Volume), Volume),
			new(nameof(Speed), Speed),
			new(nameof(Tone), Tone),
			new(nameof(Alpha), Alpha),
			new(nameof(LogF0Scale), LogF0Scale),
		};
		elem.Add(
			new XElement("Direction", dirAttr)
		);

		if (components is not null)
		{
			var comps = components
				.Select(v =>
				{
					var c = new XElement("Component");
					c.SetAttributeValue("Name", v.Id);
					c.SetAttributeValue("Value", v.Value);
					return c;
				});
			elem
				.Element("Direction")
				.Add(comps);
		}

		if (phonemes is not null)
		{
			var phs = phonemes
				.Select(v =>
				{
					var p = new XElement("Phoneme");
					p.SetAttributeValue("Data", v.Data);
					p.SetAttributeValue("Index", v.Index);
					if (v.Speed is not null)
					{
						p.SetAttributeValue("Speed", v.Speed);
					}

					if (v.Tone is not null)
					{
						p.SetAttributeValue("Tone", v.Tone);
					}

					if (v.Volume is not null)
					{
						p.SetAttributeValue("Volume", v.Volume);
					}

					return p;
				})
				.ToArray();
			elem.Add(new XElement("Phonemes", phs));
			//elem.SetElementValue("Phonemes", phs);
		}

		return elem;
	}

	decimal GetDirectionValue(string attr)
		=> SasaraUtil.ConvertDecimal(
			RawDirection.Attribute(attr).Value
		);

	void SetDicrectionValue(
		string attr, decimal value, string? format = null)
	{
		var sValue = format is null ?
			value.ToString() :
			value.ToString(format);
		RawDirection.Attribute(attr).SetValue(sValue);
	}

	static string PaddingForBase64(string s)
	{
		var mod = s.Length % 4;
		if (mod == 0)
		{
			return s;
		}

		var add = 4 - mod;
		var len = s.Length + add;
		return s.PadRight(len, '=');
	}
}

/// <summary>
/// トークの音素データ
/// </summary>
public record TalkPhoneme
{
	/// <summary>
	/// index
	/// </summary>
	public int Index { get; set; }
	/// <summary>
	/// 音素文字列
	/// </summary>
	/// <value>余白( <c>[pau][sil]</c> )の場合は文字列無</value>
	public string? Data { get; set; }
	/// <summary>
	/// <inheritdoc cref="TalkUnit.Volume"/>
	/// </summary>
	/// <remarks>
	/// 音素グラフのVOL差分
	/// </remarks>
	/// TODO:単位調査
	public decimal? Volume { get; set; }
	/// <summary>
	/// 音素グラフの長さ差分
	/// </summary>
	/// <remarks>
	/// もとの長さからの差分。単位は秒(sec.)。<br/>
	/// ※もとの長さは不明なので外部連携インターフェイスや.labファイルから取得する。
	/// <seealso cref="SasaraLabel"/>
	/// </remarks>
	public decimal? Speed { get; set; }
	/// <summary>
	/// 音素グラフの高さ(PIT)差分
	/// </summary>
    /// <value>Centの計算式 <c>2^cent/1200</c> の <c>ln()</c></value>
    ///
	public decimal? Tone { get; set; }

	/// <summary>
	/// 元の周波数と目標の周波数から必要な<see cref="Tone"/>の値を求める
	/// </summary>
	/// <param name="baseHz">元の周波数(Hz)</param>
	/// <param name="targetHz">目標の周波数(Hz)</param>
	public static decimal CulcTone(double baseHz, double targetHz)
		=> (decimal)Math.Log(targetHz / baseHz);
}