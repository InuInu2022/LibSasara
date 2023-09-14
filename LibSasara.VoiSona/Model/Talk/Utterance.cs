using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using LibSasara.VoiSona.Util;

namespace LibSasara.VoiSona.Model.Talk;

/// <summary>
/// 発話
/// </summary>
public class Utterance : Tree
{
	/// <inheritdoc cref="Utterance"/>
	public Utterance(
		string? text,
		string? tsml,
		string? start,
		bool disable = false
	) : base("Utterance")
	{
		Text = text ?? "";
		TsmlString = tsml;
		RawStart = start;
		Disable = disable;

		if(text is not null) AddAttribute(nameof(text), text, VoiSonaValueType.String);
		if(tsml is not null) AddAttribute(nameof(tsml), tsml, VoiSonaValueType.String);
		if(start is not null) AddAttribute(nameof(start), start, VoiSonaValueType.String);
		AddAttribute(nameof(disable), disable, VoiSonaValueType.Bool);
	}

	/// <summary>
	/// セリフの文字列
	/// </summary>
	public string Text { get; }
	/// <summary>
	/// 発話データTSMLの文字列
	/// </summary>
	/// <seealso cref="Tsml"/>
	public string? TsmlString { get; }

	/// <summary>
	/// 発話データTSMLの<see cref="XElement"/>
	/// </summary>
	/// <seealso cref="TsmlString"/>
	public XElement Tsml {
		get {
			var s = $"<tsml>{TsmlString?.Replace("\\\"", "\"")}</tsml>";
			var xml = XElement.Parse(s);
			return xml;
		}
	}
	/// <summary>
	/// 時刻を表す文字列 (00.000)
	/// </summary>
	/// <seealso cref="Start"/>
	public string? RawStart { get; }

	/// <summary>
	/// セリフの開始秒
	/// </summary>
	/// <seealso cref="RawStart"/>
	public decimal Start{
		get => SasaraUtil
			.ConvertDecimal(RawStart);
	}
	/// <summary>
	/// セリフ文が有効か無効か
	/// </summary>
	public bool Disable { get; }

	/// <summary>
	/// セリフの話速(Speed)
	/// </summary>
	public decimal SpeedRatio {
		get => TreeUtil
			.GetValueOnlyChildValue<decimal>(this, nameof(SpeedRatio));
	}

	/// <summary>
	/// セリフの音量（Volume)
	/// </summary>
	public decimal C0Shift {
		get => TreeUtil
			.GetValueOnlyChildValue<decimal>(this, nameof(C0Shift));
	}

	/// <summary>
	/// セリフの高さ（Pitch） -600 ~ +600
	/// </summary>
	public decimal LogF0Shift {
		get => TreeUtil
			.GetValueOnlyChildValue<decimal>(this, nameof(LogF0Shift));
	}

	/// <summary>
	/// セリフの声質・声の幼さ（Alpha）
	/// </summary>
	public decimal AlphaShift {
		get => TreeUtil
			.GetValueOnlyChildValue<decimal>(this, nameof(AlphaShift));
	}

	/// <summary>
	/// セリフの抑揚（Intonation）
	/// </summary>
	public decimal LogF0Scale {
		get => TreeUtil
			.GetValueOnlyChildValue<decimal>(this, nameof(LogF0Scale));
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
	//TODO
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
	public string FrameStyleRaw{
		get => TreeUtil
			.GetValuesOnlyChildrenValue<string>(
				this,
				nameof(FrameStyleRaw))
			?? "";
		set => TreeUtil.SetValuesOnlyChildrenValue(
			this,
			nameof(FrameStyleRaw),
			value);
	}

	/// <summary>
	/// 生のVolume(音量)変化比率
	/// </summary>
	/// <returns>
	/// カンマ<c>,</c>区切りテキスト。[seconds]:[value]
	/// </returns>
	public string FrameC0Raw{
		get => TreeUtil
			.GetValuesOnlyChildrenValue<string>(
				this,
				nameof(FrameC0Raw))
			?? "";
		set => TreeUtil.SetValuesOnlyChildrenValue(
			this,
			nameof(FrameC0Raw),
			value);
	}

	/// <summary>
	/// 生のPitch(音程)変化比率
	/// </summary>
	/// <returns>
	/// カンマ<c>,</c>区切りテキスト。[seconds]:[value]
	/// </returns>
	public string FrameLogF0Raw{
		get => TreeUtil
			.GetValuesOnlyChildrenValue<string>(
				this,
				nameof(FrameLogF0Raw))
			?? "";
		set => TreeUtil.SetValuesOnlyChildrenValue(
			this,
			nameof(FrameLogF0Raw),
			value);
	}

	/// <summary>
	/// 生のAlpha(声質、声の幼さ)変化比率
	/// </summary>
	/// <returns>
	/// カンマ<c>,</c>区切りテキスト。[seconds]:[value]
	/// </returns>
	public string FrameAlphaRaw{
		get => TreeUtil
			.GetValuesOnlyChildrenValue<string>(
				this,
				nameof(FrameAlphaRaw))
			?? "";
		set => TreeUtil.SetValuesOnlyChildrenValue(
			this,
			nameof(FrameAlphaRaw),
			value);
	}

	/// <summary>
	/// Style(感情)の変化比率
	/// </summary>
	public IEnumerable<FrameStyle> FrameStyles
	{
		get {
			return FrameStyleRaw
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
			.SplitValBySec<decimal>(FrameC0Raw);
	}

	/// <summary>
	/// Pitch(音程)変化比率
	/// </summary>
	public IEnumerable<SecondsValue<decimal>> FrameLogF0
	{
		get => TextUtil
			.SplitValBySec<decimal>(FrameLogF0Raw);
	}

	/// <summary>
	/// Alpha(声質、声の幼さ)変化比率
	/// </summary>
	public IEnumerable<SecondsValue<decimal>> FrameAlpha
	{
		get => TextUtil
			.SplitValBySec<decimal>(FrameAlphaRaw);
	}

	private LibSasara.Model.Lab BuildLab(
		string durations
	)
	{
		ReadOnlySpan<string> pd = durations
			.Split(",".ToCharArray())
			;
		return BuildLabFromSpan(pd);
	}

	private LibSasara.Model.Lab BuildLabFromSpan(ReadOnlySpan<string> pd)
	{
		var ph = Tsml
			.Descendants("word")
			.Attributes("phoneme")
			.Select(s => s.Value)
			.Select(s => s == "" ? "sil" : s)
			;
		char[] separators = { ',', '|' };
		ReadOnlySpan<string> phonemes = string
			.Join("|", ph)
			.Split(separators)
			;
		var cap = 30 * pd.Length;
		var sb = new StringBuilder(cap);
		decimal time = 0m;
		const decimal x = 10000000m;
		for (var i = 0; i < pd.Length; i++)
		{
			var s = time;
			time += decimal.TryParse(pd[i], out var t)
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