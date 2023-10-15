using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using LibSasara.Model;

namespace LibSasara;

/// <summary>
/// ユーティリティ
/// </summary>
public static class SasaraUtil
{
	private const int TickPerTempo = 960;

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
	/// <param name="tempoList">テンポ変更リスト。"clock, tempo"のリスト。</param>
	/// <param name="clockTick">変換したいClock（tick）値</param>
	/// <param name="maxClock">テンポ変更の最終Clock値。未指定の場合は<paramref name="tempoList"/>の最後のClock＋1小節（<see cref="TickPerTempo"/>*4）とみなす。</param>
	/// <exception cref="ArgumentOutOfRangeException" />
	/// <seealso cref="SongUnit.Tempo"/>
	/// <returns></returns>
	public static TimeSpan ClockToTimeSpan(
		SortedDictionary<int, decimal> tempoList,
		int clockTick,
		int maxClock = 0
	)
	{
		var last = tempoList.Last();
		if(last.Key == 0){
			double msecPerTick = 60 / (double)last.Value * 1000 / (double)TickPerTempo;
			return TimeSpan.FromMilliseconds(msecPerTick*clockTick);
		}

		var reverseList = tempoList
			.Select(v => (v.Key, Tempo: v.Value, Length: -1))
			.Reverse()
			.ToList()
			;
		var clockSum = tempoList
			.Select(v => v.Key)
			.Sum();

		var list = new List<(int Key, decimal Tempo,int Length)>();
		if(maxClock is 0){
			maxClock = last.Key + (TickPerTempo * 4);
		}else if(maxClock < last.Key){
			throw new ArgumentOutOfRangeException(
				nameof(maxClock),
				$"Invalid clock. {nameof(maxClock)} must be greater than or equal to the maximum clock tick in {nameof(tempoList)}.");
		}

		(int Key, decimal, int) next = (maxClock,0,0);
		foreach (var (Key, Tempo, Length) in reverseList)
		{
			list.Add((
				Key,
				Tempo,
				Length: next.Key - Key
			));
			next.Key = Key;
		}
		list.Reverse();
		var l = list
			.Where(v => clockTick > v.Key)
			.Select(item =>
			{
				double msecPerTick = 60 / (double)item.Tempo * 1000 / (double)TickPerTempo;

				double addTick = clockTick switch
				{
					int x when x <= item.Key
						=> 0,
					int x when item.Key < x
						&& x < item.Key + item.Length
						=> clockTick - item.Key,
					int x when item.Key + item.Length <= x
						=> item.Length,
					_ => 0
				};

				return addTick * msecPerTick;
			})
			;
		double sum = l.Sum();
		var rsum = Math.Round(sum, 2, MidpointRounding.ToEven);
		return TimeSpan.FromMilliseconds(rsum);
	}

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
}
