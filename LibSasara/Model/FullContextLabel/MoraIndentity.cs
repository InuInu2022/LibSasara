using System;
using System.Globalization;

namespace LibSasara.Model.FullContextLabel;

/// <summary>
/// <see cref="FullContextLabLine"/>のモーラ（拍）情報
/// </summary>
public record MoraIndentity: IFullContext
{
	/// <inheritdoc cref="MoraIndentity"/>
	public MoraIndentity(
		string a1,
		int a2,
		int a3
	)
	{
		AccentPosDiff = a1;
		ForwardPos = a2;
		BackwardPos = a3;
	}

	/// <inheritdoc cref="MoraIndentity"/>
	public MoraIndentity(
		ReadOnlyMemory<char> curtMoraInfo
	)
	{
		AccentPosDiff = FullContextLabUtil
			.GetCharsFromContext(curtMoraInfo, ':', '+')
			.ToString();
		ForwardPos = FullContextLabUtil
			.GetNumberFromContext(curtMoraInfo, '+', '+');
		BackwardPos = FullContextLabUtil
			.GetNumberFromContext(curtMoraInfo, '+');
	}

	/// <summary>
	/// the difference between accent type and position of the current mora identity
	/// </summary>
	/// <value>"xx" or number string（-49 ∼ 49）</value>
	public string AccentPosDiff { get; set; }

	/// <summary>
	/// position of the current mora identity in the current accent phrase (forward)
	/// </summary>
	/// <value>"xx" or number string(1 ∼ 49)</value>
	public int ForwardPos { get; set; }

	/// <summary>
	/// position of the current mora identity in the current accent phrase (backward)
	/// </summary>
	/// <value>"xx" or number string(1 ∼ 49)</value>
	public int BackwardPos { get; set; }

	/// <summary>
	/// 有効な <see cref="AccentPosDiff"/>を持っているかどうか
	/// </summary>
	public bool HasValidAccentPosDiff
		=> !string.Equals(AccentPosDiff, "xx", StringComparison.Ordinal);

	/// <summary>
	/// a set of positions of the current mora identity in the current accent phrase (forward / backward)
	/// </summary>
	/// <value>invalid: -1, valid: 1 ∼ 49</value>
	public (int Forward, int Backward) Positions
		=> (
			Forward: ForwardPos,
			Backward: BackwardPos
		 );

	/// <summary>
	/// モーラかどうか
	/// </summary>
	/// <remarks>
	/// pauやsilなどの場合はfalse
	/// </remarks>
	public bool IsMora
		=> !string.Equals(AccentPosDiff, "xx", StringComparison.Ordinal) || BackwardPos != -1 || ForwardPos != -1;

	/// <summary>
	/// 有効な <see cref="AccentPosDiff"/>を持っているかどうかを調べて、有効な場合は値を返す
	/// </summary>
	/// <param name="value">有効な場合の差分値</param>
	/// <returns></returns>
	public bool TryGetAccentPosDiffValue(out int value){
		value = HasValidAccentPosDiff
			? FullContextLabUtil
				.GetNumber(AccentPosDiff, int.MinValue)
			: int.MinValue;

		return HasValidAccentPosDiff;
	}
}