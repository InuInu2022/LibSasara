using System;
using System.Diagnostics;

namespace LibSasara.VoiSona.Model.Talk;

/// <summary>
/// 秒ごとの値をあらわすRecord
/// </summary>
/// <typeparam name="TValue">decimal, intなど</typeparam>
public record SecondsValue<TValue> : ISecondsParam
	where TValue: struct
{
	/// <inheritdoc cref="SecondsValue{TValue}"/>
	/// <param name="seconds">秒</param>
	/// <param name="value">値</param>
	public SecondsValue(
		decimal seconds,
		TValue value
	)
	{
		Seconds = seconds;
		Value = value;
	}

	/// <inheritdoc/>
	public decimal Seconds { get; set; }
	/// <summary>
	/// 値
	/// </summary>
	public TValue Value { get; set; }

	/// <summary>
	/// コロン区切り文字列で返します
	/// </summary>
	/// <returns>コロン区切り文字列。`<see cref="Seconds"/>:<see cref="Value"/>` </returns>
	public override string ToString()
		=> $"{Seconds}:{Value}";
}