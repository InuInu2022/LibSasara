using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using LibSasara.VoiSona.Util;

namespace LibSasara.VoiSona.Model.Talk;

/// <summary>
/// TstPrjに対する拡張メソッド
/// </summary>
public static class TstPrjExtension
{
	/// <summary>
	/// tstprj のバイナリ列をコピーする
	/// </summary>
	/// <param name="source"></param>
	/// <returns></returns>
	public static Memory<byte> Copy(
		this TstPrj source
	){
		if (source is null)
		{
			throw new ArgumentNullException(nameof(source));
		}

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
	)
	{
		if (newVoice is null)
		{
			throw new ArgumentNullException(nameof(newVoice));
		}

		var copied = source.Copy();

		TryFindTrack(
			source,
			trackIndex,
			copied,
			out var track,
			out var trackBin,
			out var trackBytesIndex);

		var voiceBin = track.Voice!.GetBytes().Span;
		var voiceBytesIndex = trackBin.IndexOf(voiceBin);

		//置き換えの前後をslice
		(var before, var after) = SplitBeforeAfter(
			copied,
			trackBytesIndex + voiceBytesIndex,
			voiceBin.Length);

		var nvMemory = newVoice.GetBytes();

		return ConcatMemory(before, after, nvMemory);
	}

	/// <inheritdoc cref="ReplaceVoice(TstPrj, Voice, int)"/>
	/// <returns><see cref="TstPrj"/>を返します</returns>
	public static TstPrj ReplaceVoiceAsPrj(
		this TstPrj source,
		Voice newVoice,
		int trackIndex = 0
	){
		var data = ReplaceVoice(source, newVoice, trackIndex);
		return new TstPrj(data.ToArray());
	}

	private static bool TryFindTrack(
		TstPrj source,
		int trackIndex,
		Memory<byte> copied,
		out TalkTrack track,
		out ReadOnlySpan<byte> trackBin,
		out int trackBytesIndex)
	{
		var tracksBin = source.GetAllTracksBin();
		if (trackIndex > tracksBin.Count)
		{
			Debug.WriteLine($"track[{trackIndex}] is not found!");
			track = new TalkTrack();
			trackBin = Array.Empty<byte>();
			trackBytesIndex = 0;
			return false;
		}
		track = source.GetAllTracks()[trackIndex];
		trackBin = tracksBin[trackIndex].Span;
		trackBytesIndex = copied.Span.IndexOf(trackBin);
		return true;
	}

	/// <summary>
	/// トラック内の <see cref="Utterance"/> (セリフ)をすべて置き換える
	/// </summary>
	/// <param name="source"></param>
	/// <param name="newUtterances">新しいセリフのリスト</param>
	/// <param name="trackIndex"></param>
	/// <returns></returns>
	public static Memory<byte> ReplaceAllUtterances(
		this TstPrj source,
		IEnumerable<Utterance> newUtterances,
		int trackIndex = 0
	)
	{
		var copied = source.Copy();

		TryFindTrack(
			source,
			trackIndex,
			copied,
			out var track,
			out var trackBin,
			out var trackBytesIndex);

		if(!track.HasContents){
			throw new InvalidDataException($"Target track {track.TrackName} has no Content child.");
		}

		ReadOnlySpan<byte> hexName =
			System.Text.Encoding.UTF8
			.GetBytes("Contents\0");
		//content要素がtrackの最後の前提
		var contentBin = trackBin
			.Slice(
				trackBin.IndexOf(hexName)
			);
		var contentIndex = trackBin.IndexOf(contentBin);

		//TODO:配列専用のヘッダーbyte生成＆セリフ数反映
		const bool replaceMode = true;
		if(replaceMode){
			//Content collection replace
			(var before, var after) = SplitBeforeAfter(
				copied,
				trackBytesIndex + contentIndex,
				contentBin.Length);

			var content = new Tree("Contents", true);
			content.Children
				.AddRange(newUtterances);

			var uBytes = content.GetBytes(endNull:false);

			return ConcatMemory(before, after, uBytes);
		} else {
			//中身くり抜くパターン
			(var before, var after) = SplitBeforeAfter(
				copied,
				trackBytesIndex + contentIndex + hexName.Length + 3,
				contentBin.Length - hexName.Length - 3);

			var uBytes = newUtterances
				.Select(u => u.GetBytes())
				.Aggregate((x, y) => x.Concat(y));

			return ConcatMemory(before, after, uBytes);
		}
	}

	/// <inheritdoc cref="ReplaceAllUtterances(TstPrj, IEnumerable{Utterance}, int)"/>
	/// <returns><see cref="TstPrj"/>を返します</returns>
	public static TstPrj ReplaceAllUtterancesAsPrj(
		this TstPrj source,
		IEnumerable<Utterance> newUtterances,
		int trackIndex = 0
	){
		var data = ReplaceAllUtterances(source, newUtterances, trackIndex);
		return new TstPrj(data.ToArray());
	}

	private static (Memory<byte> before, Memory<byte> after)
	SplitBeforeAfter(
		Memory<byte> target,
		int insertIndex,
		int insertLength)
	{
		var before = target.Slice(
			0,
			insertIndex);
		var after = target.Slice(
			insertIndex + insertLength);
		return (before, after);
	}

	private static Memory<byte> ConcatMemory(
		Memory<byte> before,
		Memory<byte> after,
		ReadOnlyMemory<byte> insert)
	{
		var len = before.Length + insert.Length + after.Length;
		var ret = new Memory<byte>(new byte[len]);
		before.CopyTo(ret.Slice(
			0,
			before.Length));
		insert.CopyTo(ret.Slice(
			before.Length,
			insert.Length));
		after.CopyTo(ret.Slice(
			before.Length + insert.Length,
			after.Length
		));
		return ret;
	}
}