using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using LibSasara.Model;

namespace LibSasara;

/// <summary>
/// ユーティリティ
/// </summary>
[Obsolete($"Use {nameof(LibSasaraUtil)}")]
public static class SasaraUtil
{
	/// <summary>
	/// 文字列を <see cref="decimal"/> 型に変換。失敗時は第2引数<paramref name="defaultValue"/>を返す
	/// </summary>
	/// <param name="value">数を表す文字列</param>
	/// <param name="defaultValue">失敗時に返す値</param>
	/// <returns></returns>
	[Obsolete($"Use {nameof(LibSasaraUtil.ConvertDecimal)}")]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static decimal ConvertDecimal(
		string? value,
		decimal defaultValue = 0.00m
	)
	{
		return LibSasaraUtil.ConvertDecimal(value, defaultValue);
	}

	/// <summary>
	/// 文字列を <see cref="int"/> 型に変換。失敗時は第2引数<paramref name="defaultValue"/>を返す
	/// </summary>
	/// <param name="value">数を表す文字列</param>
	/// <param name="defaultValue">失敗時に返す値</param>
	[Obsolete($"Use {nameof(LibSasaraUtil.ConvertInt)}")]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int ConvertInt(
		string? value,
		int defaultValue = 0
	)
	{
		return LibSasaraUtil.ConvertInt(value, defaultValue);
	}

	/// <summary>
	/// 文字列を <see cref="bool"/> 型に変換。失敗時は第2引数<paramref name="defaultValue"/>を返す
	/// </summary>
	/// <param name="value">数を表す文字列</param>
	/// <param name="defaultValue">失敗時に返す値</param>
	[Obsolete($"Use {nameof(LibSasaraUtil.ConvertBool)}")]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool ConvertBool
	(
		string? value,
		bool defaultValue = false
	)
	{
		return LibSasaraUtil.ConvertBool(value, defaultValue);
	}

	/// <summary>
	/// Clockを時間（<see cref="TimeSpan"/>）に変換
	/// </summary>
	/// <param name="clockTick">変換したいClock（tick）値</param>
	/// <param name="tempoList">テンポ変更リスト。"clock, tempo"のリスト。</param>
	/// <exception cref="ArgumentOutOfRangeException" />
	/// <seealso cref="SongUnit.Tempos"/>
	/// <returns></returns>
	[Obsolete($"Use {nameof(LibSasaraUtil.ClockToTimeSpan)}")]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static TimeSpan ClockToTimeSpan(
		int clockTick,
		SortedDictionary<int, decimal> tempoList
	)
	{
		return LibSasaraUtil.ClockToTimeSpan(clockTick, tempoList);
	}

	/// <inheritdoc cref="LibSasaraUtil.ClockToTimeSpan(SortedDictionary{int, decimal}, int, int)"/>
	/// <seealso cref="LibSasaraUtil.ClockToTimeSpan(SortedDictionary{int, decimal}, int, int)"/>
	[Obsolete($"Use {nameof(LibSasaraUtil.ClockToTimeSpan)}")]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static TimeSpan ClockToTimeSpan(
		SortedDictionary<int, decimal> tempoList,
		int clockTick,
		int maxClock = 0
	) => LibSasaraUtil.ClockToTimeSpan(
			clockTick,
			tempoList
		);

	/// <summary>
	/// 周波数をMIDIノートナンバーに変換
	/// </summary>
	/// <param name="freq">周波数</param>
	/// <param name="baseFreq">基準ド周波数</param>
	/// <seealso cref="LibSasaraUtil.NoteNumToFreq(int, double)"/>
	/// <returns></returns>
	[Obsolete($"Use {nameof(LibSasaraUtil.FreqToNoteNum)}")]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int FreqToNoteNum(
		double freq,
		double baseFreq = 440.0
	)
		=> LibSasaraUtil.FreqToNoteNum(freq, baseFreq);

	/// <summary>
	/// MIDIノートナンバーをノート中央の周波数に変換
	/// </summary>
	/// <param name="num">MIDIノートナンバー</param>
	/// <param name="baseFreq">基準ド周波数</param>
	/// <returns>ノート中央の周波数</returns>
	/// <seealso cref="LibSasaraUtil.FreqToNoteNum(double, double)"/>
	[Obsolete($"Use {nameof(LibSasaraUtil.NoteNumToFreq)}")]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double NoteNumToFreq(
		int num,
		double baseFreq = 440.0
	)
		=> LibSasaraUtil.NoteNumToFreq(num, baseFreq);

	/// <summary>
	/// MIDIノートナンバーを<see cref="Note"/>の <see cref="Note.PitchOctave"/> と <see cref="Note.PitchStep"/> に変換
	/// </summary>
	/// <param name="num">MIDIノートナンバー</param>
	/// <returns></returns>
	/// <seealso cref="LibSasaraUtil.OctaveStepToNoteNum(int, int)"/>
	[Obsolete($"Use {nameof(LibSasaraUtil.NoteNumToOctaveStep)}")]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static (int octave, int step) NoteNumToOctaveStep(int num)
	{
		return LibSasaraUtil.NoteNumToOctaveStep(num);
	}

	/// <summary>
	/// <see cref="Note"/>の <see cref="Note.PitchOctave"/> と <see cref="Note.PitchStep"/>からMidiノートナンバーに変換
	/// </summary>
	/// <param name="octave"></param>
	/// <param name="step"></param>
	/// <returns></returns>
	/// <seealso cref="LibSasaraUtil.NoteNumToOctaveStep(int)"/>
	[Obsolete($"Use {nameof(LibSasaraUtil.OctaveStepToNoteNum)}")]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int OctaveStepToNoteNum(int octave, int step)
	{
		return LibSasaraUtil.OctaveStepToNoteNum(octave, step);
	}

	/// <summary>
	/// 周波数を<see cref="Note"/>の <see cref="Note.PitchOctave"/> と <see cref="Note.PitchStep"/> に変換
	/// </summary>
	/// <param name="freq">周波数</param>
	/// <returns></returns>
	/// <seealso cref="LibSasaraUtil.OctaveStepToFreq(int, int)"/>
	[Obsolete($"Use {nameof(LibSasaraUtil.FreqToOctaveStep)}")]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static (int octave, int step) FreqToOctaveStep(double freq){
		return LibSasaraUtil.FreqToOctaveStep(freq);
	}

	/// <summary>
	/// <see cref="Note"/>の <see cref="Note.PitchOctave"/> と <see cref="Note.PitchStep"/> をノート中央の周波数に変換
	/// </summary>
	/// <param name="octave"></param>
	/// <param name="step"></param>
	/// <returns></returns>
	/// <seealso cref="LibSasaraUtil.FreqToOctaveStep(double)"/>
	[Obsolete($"Use {nameof(LibSasaraUtil.OctaveStepToFreq)}")]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double OctaveStepToFreq(int octave, int step){
		return LibSasaraUtil.OctaveStepToFreq(octave, step);
	}
}
