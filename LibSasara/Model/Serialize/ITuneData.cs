namespace LibSasara.Model.Serialize;

/// <summary>
/// 調声データ
/// </summary>
public interface ITuneData
{
	/// <summary>
	/// インデックス
	/// </summary>
	/// <remarks>
	/// 省略可能。
	/// </remarks>
	int Index { get; set; }

	/// <summary>
	/// 同じ値の繰り返し数
	/// </summary>
	int Repeat { get; set; }
}
