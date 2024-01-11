using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using LibSasara.VoiSona.Util;

namespace LibSasara.VoiSona.Model.Talk;

/// <summary>
/// 発話
/// </summary>
public class Utterance : Tree
{
	private string _text;
	private string? _rawTsml;
	private string? _rawStart;

	/// <inheritdoc cref="Utterance"/>
	public Utterance(
		string? text,
		string? tsml,
		string? start,
		bool? disable = null,
		string exportName = ""
	) : base(nameof(Utterance))
	{
		_text = text ?? "";
		_rawTsml = tsml;
		_rawStart = start;
		if(disable is not null)Disable = disable;
		ExportName = exportName;

		if(text is not null) AddAttribute(nameof(text), text, VoiSonaValueType.String);
		if(tsml is not null) AddAttribute(nameof(tsml), tsml, VoiSonaValueType.String);
		if(start is not null) AddAttribute(nameof(start), start, VoiSonaValueType.String);
		if(disable is not null) AddAttribute(nameof(disable), disable, VoiSonaValueType.Bool);
		if (!string.IsNullOrEmpty(exportName)) AddAttribute(nameof(exportName), exportName, VoiSonaValueType.String);
	}

	/// <summary>
	/// セリフの文字列
	/// </summary>
	public string Text {
		get => _text;
		set
		{
			_text = value;
			AddAttribute("text", value, VoiSonaValueType.String);
		}
	}
	/// <summary>
	/// 発話データTSMLの文字列
	/// </summary>
	/// <seealso cref="Tsml"/>
	public string? RawTsml {
		get => _rawTsml;
		set{
			_rawTsml = value ?? "";
			AddAttribute("tsml", _rawTsml, VoiSonaValueType.String);
		}
	}

	/// <summary>
	/// 発話データTSMLの<see cref="XElement"/>
	/// </summary>
	/// <remarks>
	/// 有効な<see cref="XElement"/>とするため, root要素として<c>&lt;tsml&gt;</c>でラップされたものが返ります。
	/// </remarks>
	/// <seealso cref="RawTsml"/>
	public XElement Tsml {
		get {
			var s = $"<tsml>{RawTsml?.Replace("\\\"", "\"")}</tsml>";
			return XElement.Parse(s);
		}
		set {
			var hasRoot = value?
					.Element("tsml") is not null;
			var content = hasRoot
				? value!.Element("tsml")?.Elements()
				: value!.Elements("acoustic_phrase");
			if(content is null){
				RawTsml = string.Empty;
				return;
			}
			var sb = new StringBuilder(1000);
			foreach(var i in content)
			{
				sb.Append(i);
			}
			RawTsml = sb.ToString();
		}
	}
	/// <summary>
	/// 時刻を表す文字列 (00.000)
	/// </summary>
	/// <seealso cref="Start"/>
	public string? RawStart {
		get => _rawStart;
		set{
			_rawStart = value ?? "0.00";
			AddAttribute("start", _rawStart, VoiSonaValueType.String);
		}
	}

	/// <summary>
	/// セリフの開始秒
	/// </summary>
	/// <seealso cref="RawStart"/>
	public decimal Start{
		get => SasaraUtil
			.ConvertDecimal(RawStart);
		set {
			RawStart = value.ToString("N2", CultureInfo.InvariantCulture);
		}
	}
	/// <summary>
	/// セリフ文が有効か無効か
	/// </summary>
	public bool? Disable { get; }

	/// <summary>
	/// セリフ行の書き出しファイル名称
	/// </summary>
	public string ExportName { get; }

	/// <summary>
	/// セリフの話速(Speed)
	/// </summary>
	public decimal SpeedRatio {
		get => TreeUtil
			.GetValueOnlyChildValue<decimal>(this, nameof(SpeedRatio));
		set => TreeUtil.SetValueOnlyChildValue(
			this,
			nameof(SpeedRatio),
			value);
	}

	/// <summary>
	/// セリフの音量（Volume)
	/// </summary>
	public decimal C0Shift {
		get => TreeUtil
			.GetValueOnlyChildValue<decimal>(this, nameof(C0Shift));
		set => TreeUtil.SetValueOnlyChildValue(
			this,
			nameof(C0Shift),
			value);
	}

	/// <summary>
	/// セリフの高さ（Pitch） -600 ~ +600
	/// </summary>
	public decimal LogF0Shift {
		get => TreeUtil
			.GetValueOnlyChildValue<decimal>(this, nameof(LogF0Shift));
		set => TreeUtil.SetValueOnlyChildValue(
			this,
			nameof(LogF0Shift),
			value);
	}

	/// <summary>
	/// セリフの声質・声の幼さ（Alpha）
	/// </summary>
	public decimal AlphaShift {
		get => TreeUtil
			.GetValueOnlyChildValue<decimal>(this, nameof(AlphaShift));
		set => TreeUtil.SetValueOnlyChildValue(
			this,
			nameof(AlphaShift),
			value);
	}

	/// <summary>
	/// セリフの抑揚（Intonation）
	/// </summary>
	public decimal LogF0Scale {
		get => TreeUtil
			.GetValueOnlyChildValue<decimal>(this, nameof(LogF0Scale));
		set => TreeUtil.SetValueOnlyChildValue(
			this,
			nameof(LogF0Scale),
			value);
	}

	/// <summary>
	/// 元の音素の長さを示すカンマ区切り文字列
	/// </summary>
	/// <seealso cref="PhonemeDuration"/>
	/// <seealso cref="PhonemeOriginalDurations"/>
	/// <seealso cref="DefaultLabel"/>
	public string PhonemeOriginalDuration
	{
		get
		{
			return TreeUtil
				.GetValuesOnlyChildrenValue<string>(
					this,
					nameof(PhonemeOriginalDuration))
				?? "";
		}

		set => TreeUtil.SetValuesOnlyChildrenValue(
			this,
			nameof(PhonemeOriginalDuration),
			value);
	}

	/// <summary>
	/// 音素別の長さのリスト
	/// </summary>
	/// <seealso cref="PhonemeOriginalDuration"/>
	/// <seealso cref="DefaultLabel"/>
	public IEnumerable<decimal> PhonemeOriginalDurations
	{
		get => TextUtil.SplitVal<decimal>(PhonemeOriginalDuration);
	}

	/// <summary>
	/// 調声後の音素の長さを示すカンマ区切り文字列
	/// </summary>
	/// <seealso cref="PhonemeOriginalDuration"/>
	public string PhonemeDuration{
		get => TreeUtil
			.GetValuesOnlyChildrenValue<string>(
				this,
				nameof(PhonemeDuration))
			?? "";
		set => TreeUtil.SetValuesOnlyChildrenValue(
			this,
			nameof(PhonemeDuration),
			value);
	}

	/// <summary>
	/// 調声前のタイミングの <see cref="LibSasara.Model.Lab"/> を返す
	/// </summary>
	/// <seealso cref="PhonemeOriginalDuration"/>
	/// <seealso cref="Label"/>
	public LibSasara.Model.Lab? DefaultLabel {
		get => BuildLab(PhonemeOriginalDuration);
	}

	/// <summary>
	/// 調整後のタイミングの<see cref="LibSasara.Model.Lab"/> を返す。
	/// 調整されてなければ <see cref="DefaultLabel"/> と同じものが返る。
	/// </summary>
	/// <seealso cref="PhonemeDuration"/>
	/// <seealso cref="DefaultLabel"/>
	public LibSasara.Model.Lab? Label
	{
		get
		{
			var isDefault = string.IsNullOrEmpty(PhonemeDuration);
			if (isDefault) return DefaultLabel;

			var pod = PhonemeOriginalDuration
				.Split(",".ToCharArray());
			var pd = PhonemeDuration
				.Split(",".ToCharArray())
				.Select(v =>
				{
					var a = v.Split(":".ToCharArray());
					return (SasaraUtil.ConvertInt(a[0]), a[1]);
				})
				;
			foreach(var i in pd){
				pod[i.Item1] = i.Item2;
			}
			return BuildLabFromSpan(pod.AsSpan());
		}
	}

	/// <summary>
	/// 生のStyle(感情)変化比率
	/// </summary>
	/// <remarks>
	/// `[seconds]:[?]:[sty1rate]:[sty2rate]:...`
	/// </remarks>
	/// <returns>
	/// カンマ<c>,</c>区切りテキスト。比率は<c>:</c>で区切られます
	/// </returns>
	public string RawFrameStyle{
		get => TreeUtil
			.GetValuesOnlyChildrenValue<string>(
				this,
				nameof(FrameStyle))
			?? "";
		set => TreeUtil.SetValuesOnlyChildrenValue(
			this,
			nameof(FrameStyle),
			value);
	}

	/// <inheritdoc cref="RawFrameStyle"/>
	[Obsolete($"see {nameof(RawFrameStyle)}")]
	public string FrameStyleRaw
	{
		get => RawFrameStyle;
		set => RawFrameStyle = value;
	}

	/// <summary>
	/// 生のVolume(音量)変化比率
	/// </summary>
	/// <returns>
	/// カンマ<c>,</c>区切りテキスト。[seconds]:[value]
	/// </returns>
	public string RawFrameC0{
		get => TreeUtil
			.GetValuesOnlyChildrenValue<string>(
				this,
				nameof(FrameC0))
			?? "";
		set => TreeUtil.SetValuesOnlyChildrenValue(
			this,
			nameof(FrameC0),
			value);
	}

	/// <inheritdoc cref="RawFrameC0"/>
	[Obsolete($"see {nameof(RawFrameC0)}")]
	public string FramwC0Raw
	{
		get => RawFrameC0;
		set => RawFrameC0 = value;
	}

	/// <summary>
	/// 生のPitch(音程)変化比率
	/// </summary>
	/// <returns>
	/// カンマ<c>,</c>区切りテキスト。[phoneme count rate]:[value]
	/// </returns>
	public string RawFrameLogF0{
		get => TreeUtil
			.GetValuesOnlyChildrenValue<string>(
				this,
				nameof(FrameLogF0))
			?? "";
		set => TreeUtil.SetValuesOnlyChildrenValue(
			this,
			nameof(FrameLogF0),
			value);
	}

	/// <inheritdoc cref="RawFrameLogF0"/>
	[Obsolete($"see {nameof(RawFrameLogF0)}")]
	public string FrameLogF0Raw{
		get => RawFrameLogF0;
		set => RawFrameLogF0 = value;
	}

	/// <summary>
	/// 生のAlpha(声質、声の幼さ)変化比率
	/// </summary>
	/// <returns>
	/// カンマ<c>,</c>区切りテキスト。[seconds]:[value]
	/// </returns>
	public string RawFrameAlpha{
		get => TreeUtil
			.GetValuesOnlyChildrenValue<string>(
				this,
				nameof(FrameAlpha))
			?? "";
		set => TreeUtil.SetValuesOnlyChildrenValue(
			this,
			nameof(FrameAlpha),
			value);
	}

	/// <inheritdoc cref="RawFrameAlpha"/>
	[Obsolete($"see {nameof(RawFrameAlpha)}")]
	public string FrameAlphaRaw{
		get => RawFrameAlpha;
		set => RawFrameAlpha = value;
	}

	/// <summary>
	/// Style(感情)の変化比率
	/// </summary>
	public IEnumerable<FrameStyle> FrameStyles
	{
		get {
			return RawFrameStyle
				.Split(",".ToCharArray())
				.Select<string,FrameStyle>(v => {
					Memory<string> a = v.Split(":".ToCharArray());
					return new(
						SasaraUtil.ConvertDecimal(a.Span[0]),
						SasaraUtil.ConvertInt(a.Span[1]),
						a.Slice(3)
							.ToArray()
							.Select(s => SasaraUtil.ConvertDecimal(s))
							.ToList()
					);
				})
				;
		}
	}

	/// <summary>
	/// Volume(音量)変化比率
	/// </summary>
	public IEnumerable<SecondsValue<decimal>> FrameC0
	{
		get => TextUtil
			.SplitValBySec<decimal>(RawFrameC0);
	}

	/// <summary>
	/// Pitch(音程)変化比率
	/// </summary>
	public IEnumerable<SecondsValue<decimal>> FrameLogF0
	{
		get => TextUtil
			.SplitValBySec<decimal>(RawFrameLogF0);
	}

	/// <summary>
	/// Alpha(声質、声の幼さ)変化比率
	/// </summary>
	public IEnumerable<SecondsValue<decimal>> FrameAlpha
	{
		get => TextUtil
			.SplitValBySec<decimal>(RawFrameAlpha);
	}

	private LibSasara.Model.Lab BuildLab(
		string durations
	)
	{
		ReadOnlySpan<string> pd = durations
			.Split([','])
			;
		return BuildLabFromSpan(pd);
	}

	private LibSasara.Model.Lab BuildLabFromSpan(ReadOnlySpan<string> pd)
	{
		var ph = Tsml
			.Descendants("word")
			.Attributes("phoneme")
			.Select(s => s.Value)
			.Select(s => s?.Length == 0 ? "sil" : s)
			;
		char[] separators = [',', '|'];
		ReadOnlySpan<string> phonemes = string
			.Join("|", ph)
			.Split(separators)
			;
		var cap = 30 * pd.Length;
		var sb = new StringBuilder(cap);
		var time = 0m;
		const decimal x = 10000000m;
		for (var i = 0; i < pd.Length; i++)
		{
			var s = time;
			time += decimal.TryParse(
				pd[i],
				NumberStyles.Number,
				CultureInfo.InvariantCulture,
				out var t)
				? t * x : 0m;
			var e = time;
			var p = (i - 1 < 0 || phonemes.Length <= i - 1)
				? "sil"
				: phonemes[i - 1]
				;
			sb.AppendLine($"{s} {e} {p}");
		}
		return new LibSasara.Model.Lab(sb.ToString());
	}
}