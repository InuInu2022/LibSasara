using System;

namespace LibSasara.Model;

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