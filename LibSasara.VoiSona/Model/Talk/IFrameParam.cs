using System;

namespace LibSasara.VoiSona.Model.Talk;

/// <summary>
/// フレームごとのパラメータ値の共通インターフェイス
/// </summary>
public interface IFrameParam
{
	/// <summary>
	/// パラメータの対象フレーム秒
	/// </summary>
	public decimal Seconds { get; set; }
}