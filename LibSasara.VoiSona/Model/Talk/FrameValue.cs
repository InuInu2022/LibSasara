using System;

namespace LibSasara.VoiSona.Model.Talk;

/// <summary>
/// フレームごとの値を表すRecord
/// </summary>
public record FrameValue<T>
	where T : struct
{
	/// <inheritdoc cref="FrameValue{T}"/>
	public FrameValue(
		int frame,
		T value
	)
	{
		Frame = frame;
		Value = value;
	}

	/// <summary>
	/// フレーム数
	/// </summary>
	public int Frame{ get; set; }
	/// <summary>
	/// 値
	/// </summary>
	public T Value { get; set; }

	/// <summary>
	/// コロン区切り文字列で返します
	/// </summary>
	/// <returns>コロン区切り文字列。`<see cref="Frame"/>:<see cref="Value"/>` </returns>
	public override string ToString()
		=> $"{Frame}:{Value}";
}