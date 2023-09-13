using System;
using System.Collections.Generic;

namespace LibSasara.VoiSona.Model.Talk;

/// <summary>
/// Style(感情)の変化比率パラメータ
/// </summary>
public record FrameStyle : FrameValue<int>, IFrameParam
{
	/// <inheritdoc cref="FrameStyle"/>
	/// <param name="seconds"><inheritdoc cref="IFrameParam.Seconds"/></param>
	/// <param name="value"><inheritdoc cref="Value"/></param>
	/// <param name="rates"><inheritdoc cref="Rates"/></param>
	public FrameStyle(
		decimal seconds,
		int value,
		List<decimal> rates
	): base(seconds, value)
	{
		Rates = rates;
	}

	/// <summary>
	/// 用途不明の値
	/// </summary>
	public new int Value { get; set; }

	/// <summary>
	/// その時点の感情(STY)比率
	/// </summary>
	public List<decimal> Rates { get; set; }

	/// <summary>
	/// コロン区切り文字列で返します
	/// </summary>
	/// <returns>コロン区切り文字列。`<see cref="IFrameParam.Seconds"/>:<see cref="Value"/>:<see cref="Rates"/>[]` </returns>
	public override string ToString()
		=> $"{Seconds}:{Value}:{string.Join(":", Rates)}";
}