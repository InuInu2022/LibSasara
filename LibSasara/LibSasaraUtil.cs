using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using CommunityToolkit.Diagnostics;
using LibSasara.Model;
using LibSasara.Model.Serialize;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

namespace LibSasara;

/// <summary>
/// ユーティリティ
/// </summary>
public static class LibSasaraUtil
{
	private const decimal TickPerBeat = 960;

	/// <summary>
	/// 文字列を <see cref="decimal"/> 型に変換。失敗時は第2引数<paramref name="defaultValue"/>を返す
	/// </summary>
	/// <param name="value">数を表す文字列</param>
	/// <param name="defaultValue">失敗時に返す値</param>
	/// <returns></returns>
	public static decimal ConvertDecimal(
		string? value,
		decimal defaultValue = 0.00m
	)
	{
		var isSafe = decimal.TryParse(
			value,
			NumberStyles.Number,
			CultureInfo.InvariantCulture,
			out var val);

		return isSafe ? val : defaultValue;
	}

	/// <summary>
	/// 文字列を <see cref="int"/> 型に変換。失敗時は第2引数<paramref name="defaultValue"/>を返す
	/// </summary>
	/// <param name="value">数を表す文字列</param>
	/// <param name="defaultValue">失敗時に返す値</param>
	public static int ConvertInt(
		string? value,
		int defaultValue = 0
	)
	{
		var isSafe = int.TryParse(
			value,
			NumberStyles.Number,
			CultureInfo.InvariantCulture,
			out var val);

		return isSafe ? val : defaultValue;
	}

	/// <summary>
	/// 文字列を <see cref="bool"/> 型に変換。失敗時は第2引数<paramref name="defaultValue"/>を返す
	/// </summary>
	/// <param name="value">数を表す文字列</param>
	/// <param name="defaultValue">失敗時に返す値</param>
	public static bool ConvertBool
	(
		string? value,
		bool defaultValue = false
	)
	{
		var isSafe = bool
			.TryParse(value, out var val);
		return isSafe ? val : defaultValue;
	}

	/// <summary>
	/// Clockを時間（<see cref="TimeSpan"/>）に変換
	/// </summary>
	/// <param name="clockTick">変換したいClock（tick）値</param>
	/// <param name="tempoList">テンポ変更リスト。"clock, tempo"のリスト。</param>
	/// <exception cref="ArgumentOutOfRangeException" />
	/// <seealso cref="SongUnit.Tempos"/>
	/// <returns></returns>
	public static TimeSpan ClockToTimeSpan(
		int clockTick,
		SortedDictionary<int, decimal> tempoList
	)
	{
		//null check
		Guard.IsNotNull(tempoList, nameof(ClockToTimeSpan));

		//init
		var sec = 0m;
		var tick = 0m;
		var tempo = 0m;

		foreach (var item in tempoList)
		{
			var cTick = item.Key;
			var cTempo = item.Value;

			var elapsed = Math.Min(clockTick, cTick) - tick;
			if (tempo != 0)
			{
				var spt = 60 / tempo / TickPerBeat;
				sec += elapsed * spt;
			}

			//update
			tick = cTick;
			tempo = cTempo;

			//抜ける
			if (clockTick < cTick) break;
		}
		if(clockTick > tick){
			if (tempo != 0){
				sec += (clockTick - tick) * (60 / tempo /TickPerBeat);
			}
		}

		return TimeSpan.FromSeconds((double)sec);
	}

	/// <inheritdoc cref="ClockToTimeSpan(SortedDictionary{int, decimal}, int, int)"/>
	/// <seealso cref="ClockToTimeSpan(SortedDictionary{int, decimal}, int, int)"/>
	public static TimeSpan ClockToTimeSpan(
		SortedDictionary<int, decimal> tempoList,
		int clockTick,
		int maxClock = 0
	) => ClockToTimeSpan(
			clockTick,
			tempoList
		);

	/// <summary>
	/// 周波数をMIDIノートナンバーに変換
	/// </summary>
	/// <param name="freq">周波数</param>
	/// <param name="baseFreq">基準ド周波数</param>
	/// <seealso cref="NoteNumToFreq(int, double)"/>
	/// <returns></returns>
	public static int FreqToNoteNum(
		double freq,
		double baseFreq = 440.0
	)
		=> (int)Math.Round(69.0 + (12.0 * Math.Log(freq / baseFreq, 2)));

	/// <summary>
	/// MIDIノートナンバーをノート中央の周波数に変換
	/// </summary>
	/// <param name="num">MIDIノートナンバー</param>
	/// <param name="baseFreq">基準ド周波数</param>
	/// <returns>ノート中央の周波数</returns>
	/// <seealso cref="FreqToNoteNum(double, double)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static double NoteNumToFreq(
		int num,
		double baseFreq = 440.0
	)
		=> Math.Pow(2.0, (num - 69.0) / 12.0) * baseFreq;

	/// <summary>
	/// MIDIノートナンバーを<see cref="Note"/>の <see cref="Note.PitchOctave"/> と <see cref="Note.PitchStep"/> に変換
	/// </summary>
	/// <param name="num">MIDIノートナンバー</param>
	/// <returns></returns>
	/// <seealso cref="OctaveStepToNoteNum(int, int)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static (int octave, int step) NoteNumToOctaveStep(int num)
	{
		var oc = num == 0 ? -1 : (num / 12) - 1;
		var st = num == 0 ? 0 : num % 12;
		return (oc, st);
	}

	/// <summary>
	/// <see cref="Note"/>の <see cref="Note.PitchOctave"/> と <see cref="Note.PitchStep"/>からMidiノートナンバーに変換
	/// </summary>
	/// <param name="octave"></param>
	/// <param name="step"></param>
	/// <returns></returns>
	/// <seealso cref="NoteNumToOctaveStep(int)"/>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static int OctaveStepToNoteNum(int octave, int step)
	{
		return ((octave + 1) * 12) + step;
	}

	/// <summary>
	/// 周波数を<see cref="Note"/>の <see cref="Note.PitchOctave"/> と <see cref="Note.PitchStep"/> に変換
	/// </summary>
	/// <param name="freq">周波数</param>
	/// <returns></returns>
	/// <seealso cref="OctaveStepToFreq(int, int)"/>
	public static (int octave, int step) FreqToOctaveStep(double freq){
		int num = FreqToNoteNum(freq);
		return NoteNumToOctaveStep(num);
	}

	/// <summary>
	/// <see cref="Note"/>の <see cref="Note.PitchOctave"/> と <see cref="Note.PitchStep"/> をノート中央の周波数に変換
	/// </summary>
	/// <param name="octave"></param>
	/// <param name="step"></param>
	/// <returns></returns>
	/// <seealso cref="FreqToOctaveStep(double)"/>
	public static double OctaveStepToFreq(int octave, int step){
		var num = OctaveStepToNoteNum(octave, step);
		return NoteNumToFreq(num);
	}

	/// <summary>
	/// ノートの存在する小節数・小節内拍数を取得
	/// </summary>
	/// <param name="note">音符（ノート）</param>
	/// <param name="beatList">曲の拍子変更リスト。<seealso cref="SongUnit.Beat"/> を渡してください</param>
	/// <returns>小節数、拍数を返すタプル</returns>
	/// <seealso cref="SongUnit.Beat"/>
	public static (long Measure, long Beats) CalculateMeasureFromNote(
		Model.Note note,
		SortedDictionary<int, (int Beats, int BeatType)> beatList
	)
	{
		//drywetmidiを使って解析する
		using var tmm = new TempoMapManager(
			new TicksPerQuarterNoteTimeDivision(960)
		);
		foreach (var b in beatList)
		{
			tmm.SetTimeSignature(
				(ITimeSpan)new MidiTimeSpan(b.Key),
				new TimeSignature(b.Value.Beats, b.Value.BeatType)
			);
		}

		var noteNum = OctaveStepToNoteNum(note.PitchOctave, note.PitchStep);

		var n = new Melanchall.DryWetMidi.Interaction.Note(
			(Melanchall.DryWetMidi.Common.SevenBitNumber) noteNum
		)
		{
			Time = note.Clock,
			Length = note.Duration,
		}
		;
		var result = n.TimeAs<BarBeatTicksTimeSpan>(tmm.TempoMap);
		var measure = result.Bars;
		var beatPos = result.Beats + 1;

		return (measure, beatPos);
	}
}
