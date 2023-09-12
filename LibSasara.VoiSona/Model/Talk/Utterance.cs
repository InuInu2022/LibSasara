using System;
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
		Start = start;
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
	public string? Start { get; }
	/// <summary>
	/// セリフ文が有効か無効か
	/// </summary>
	public bool Disable { get; }

	/// <summary>
	/// 元の音素の長さを示すカンマ区切り文字列
	/// </summary>
	/// <seealso cref="PhonemeDuration"/>
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
		const decimal x = 100000m;
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