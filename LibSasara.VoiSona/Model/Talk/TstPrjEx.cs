using System;
using System.Runtime.InteropServices;

namespace LibSasara.VoiSona.Model.Talk;

/// <summary>
/// TstPrjに対する拡張メソッド
/// </summary>
public static class TstPrjEx
{
	/// <summary>
	/// tstprj のバイナリ列をコピーする
	/// </summary>
	/// <param name="source"></param>
	/// <returns></returns>
	public static Memory<byte> Copy(
		this TstPrj source
	){
		var len = source.Data.Length;
		var replaced = new Memory<byte>(new byte[len]);
		source.Data.CopyTo(replaced);
		return replaced;
	}

	/// <summary>
	/// トラックの<see cref="Voice"/>（話者）を置き換える
	/// </summary>
	/// <param name="source"><see cref="TstPrj"/>のバイナリ列（<see cref="Memory{T}"/>）</param>
	/// <param name="newVoice">新しい話者</param>
	/// <param name="trackIndex"></param>
	/// <returns></returns>
	/// <exception cref="IndexOutOfRangeException"/>
	public static Memory<byte> ReplaceVoice(
		this TstPrj source,
		Voice newVoice,
		int trackIndex = 0
	){
		var copied = source.Copy();

		var tracksBin = source.GetAllTracksBin();
		if(trackIndex > tracksBin.Count){
			throw new IndexOutOfRangeException($"track[{trackIndex}] is not found!");
		}
		var track = source.GetAllTracks()[trackIndex];
		var trackBin = tracksBin[trackIndex].Span;
		var trackBytesIndex = copied.Span.IndexOf(trackBin);
		var voiceBin = track.Voice!.GetBytes().Span;
		var voiceBytesIndex = trackBin.IndexOf(voiceBin);

		//置き換えの前後をslice
		var before = copied.Slice(0, trackBytesIndex + voiceBytesIndex);
		var after = copied.Slice(
			trackBytesIndex + voiceBytesIndex + voiceBin.Length
		);

		var nvMemory = newVoice.GetBytes();

		var len = before.Length + nvMemory.Length + after.Length;
		var ret = new Memory<byte>(new byte[len]);
		before.CopyTo(ret.Slice(
			0,
			before.Length));
		nvMemory.CopyTo(ret.Slice(
			before.Length,
			nvMemory.Length));
		after.CopyTo(ret.Slice(
			before.Length + nvMemory.Length,
			after.Length
		));

		return ret;
	}
}