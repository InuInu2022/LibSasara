using System.Globalization;
using System.Text;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LibSasara.Model;

/// <summary>
/// Timing file (.lab)
/// </summary>
public class Lab
{
	/// <summary>
	/// 行単位<see cref="LabLine"/>のリスト
	/// </summary>
	public IEnumerable<LabLine>? Lines
	{
		get => lines;
	}

	/// <summary>
    /// CeVIOの場合、labファイルの単位は1000万分の1秒（100 ns）
    /// </summary>
	private const int TenNanoSeconds = 10000000;

	private IEnumerable<LabLine>? lines;
	private static readonly string[] sepReturns = ["\n","\r\n","\r"];
	private static readonly string[] sepSpace = [" "];

	/// <summary>
	/// <inheritdoc cref="Lab"/>
	/// </summary>
	/// <param name="labData"></param>
	/// <param name="fps"></param>
	public Lab(string labData, int fps = 30)
	{
		lines = labData?
			.Split(sepReturns, StringSplitOptions.RemoveEmptyEntries)
			.Where(s => !string.IsNullOrEmpty(s))    //空行無視
			.Select((v) =>
			{
				var a = v.Split(sepSpace,StringSplitOptions.RemoveEmptyEntries);
				return new LabLine(
					Convert.ToDouble(a[0], CultureInfo.InvariantCulture),
					Convert.ToDouble(a[1], CultureInfo.InvariantCulture),
					a[2],
					fps
				);
			});
	}

	/// <inheritdoc cref="Lab"/>
	public Lab(IEnumerable<LabLine> labLines){
		lines = labLines;
	}

	/// <summary>
	/// 文章・小節単位に分割する
	/// </summary>
	/// <param name="threshold">分割基準秒数(sec.)</param>
	/// <returns></returns>
	public List<List<LabLine>> SplitToSentence(double threshold)
	{
		var t = threshold * TenNanoSeconds;

		var l = Lines!
			.Where(v => !string.Equals(v.Phoneme, "pau", StringComparison.Ordinal))	//無音の空白無視
			.Select(v =>
			{
				//判定プロパティ生やす
				return (IsSep: false, v);
			});

		var result = Enumerable
			.Empty<List<LabLine>>()
			.ToList();
		var tmpList = Enumerable
			.Empty<LabLine>()
			.ToList();
		var list = l.ToList();
		var len = list.Count;
		for (int i = 0; i < len; i++)
		{
			var (IsSep, v) = list[i];

			if (i > 0)
			{
				var prev = list[i - 1];
				IsSep = (v.From - prev.v.To) >= t;
			}

			if (IsSep && tmpList.Count != 0)
			{
				var copyed = new List<LabLine>(tmpList);

				result.Add(copyed);
				tmpList.Clear();
			}

			tmpList.Add(v);

			if(i == len - 1){
				result.Add(tmpList);
			}
		}

		return result;
	}

	/// <summary>
	/// 長さを比率に合わせて変更する
	/// </summary>
	/// <param name="percent">0~100</param>
	/// <returns></returns>
	public async ValueTask ChangeLengthByRateAsync(double percent){
		if(lines is null)
		{
			return;
		}

		var rate = 100 / percent;
		var origin = 0.0;

		await Task.Run(() =>
		{
			var newLines = lines.ToList();
			for (int i = 0; lines.Skip(i).Any(); i++)
			{
				var line = lines.ElementAt(i);
				if(i is 0){
					origin = line.From;
				}

				var newFrom = ((line.From - origin) * rate) + origin;
				var len = (line.To - line.From) * rate;
				var newTo = newFrom + len;
				newLines[i] = new LabLine(
					newFrom,
					newTo,
					line.Phoneme,
					LabLine.MovieFPS);
			}

			lines = newLines.AsEnumerable();
		}).ConfigureAwait(false);
	}

	/// <summary>
    /// 指定した秒数ぶん、全体のタイミングをずらします
    /// </summary>
    /// <param name="seconds">秒数。マイナス指定で前にずらします。</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">最初の音素のタイミングがマイナスにずれた場合エラー。</exception>
	public async ValueTask DisplaceSecondsAsync(double seconds){
		if (lines is null) { return; }

		double labSeconds = seconds * TenNanoSeconds;

		var lineFirst = lines
			.First(l => !PhonemeUtil.IsNoSounds(l))
			.From
			+ labSeconds;
		if(lineFirst < 0){
			throw new InvalidOperationException(
				$"input seconds must be bigger than first line time. seconds:{labSeconds}, displaced 1st line time:{lineFirst}");
		}

		await Task.Run(() =>
		{
			var newLines = lines.ToList();
			var count = lines.Count();
			for (int i = 0; i < count; i++)
			{
				var line = lines.ElementAt(i);

				var newFrom = Math.Max(line.From + labSeconds, 0.0);
				var newTo = Math.Max(line.To + labSeconds, 0.0);
				newLines[i] = new LabLine(
					newFrom,
					newTo,
					line.Phoneme,
					LabLine.MovieFPS);
			}

			lines = newLines.AsEnumerable();
		}).ConfigureAwait(false);
	}

	/// <summary>
	/// ラベルファイル(*.lab)フォーマットの文字列を返します。
	/// </summary>
	/// <returns>
	/// ラベルファイル(*.lab)フォーマットの文字列
	/// </returns>
	/// <example>
	/// <code>
	/// 0 1000 sil
	/// 1000 200000 a
	/// 200000 201000 sil
	/// </code>
	/// </example>
	public override string ToString()
	{
		if(lines is null){
			return string.Empty;
		}
		var cap = lines.Count() * 30;
		var sb = new StringBuilder(cap);
		foreach(var i in lines)
		{
			sb.Append(i.From)
				.Append(' ')
				.Append(i.To)
				.Append(' ')
				.AppendLine(i.Phoneme);
		}
		return sb.ToString();
	}
}

/// <summary>
/// 行単位
/// <c>[開始秒] [終了秒] [音素]</c>
/// </summary>
public class LabLine
{
	/// <summary>
	/// 開始秒数
	/// </summary>
	/// <remarks>
	/// CeVIOの場合、labファイルの単位は1000万分の1秒（100 ns）
	/// </remarks>
	public double From { get; }

	/// <summary>
	/// 終了秒数
	/// </summary>
	/// <remarks>
	/// CeVIOの場合、labファイルの単位は1000万分の1秒（100 ns）
	/// </remarks>
	public double To { get; }

	/// <summary>
	/// 長さ秒数
	/// </summary>
	/// <remarks>
	/// CeVIOの場合、labファイルの単位は1000万分の1秒（100 ns）
	/// </remarks>
	public double Length => To - From;

	/// <summary>
	/// 音素
	/// </summary>
	/// https://sinsy.sp.nitech.ac.jp/reference.pdf
	public string Phoneme { get; }

	/// <summary>
	/// フレームレート
	/// </summary>
	/// <remarks>
	/// 動画ファイル等で利用する際に使用する
	/// </remarks>
	public static int MovieFPS { get; set; } = 30;

	/// <summary>
	/// <inheritdoc cref="LabLine"/> コンストラクタ
	/// </summary>
	/// <param name="from"></param>
	/// <param name="to"></param>
	/// <param name="phenome"></param>
	/// <param name="fps"></param>
	public LabLine(
		double from,
		double to,
		string phenome,
		int fps = 30)
	{
		From = from;
		To = to;
		Phoneme = phenome;
		MovieFPS = fps;
	}

	/// <summary>
	/// 開始フレーム
	/// </summary>
	public int FrameFrom => ToFrame(From);

	/// <summary>
	/// 終了フレーム
	/// </summary>
	public int FrameTo => ToFrame(To);

	/// <summary>
	/// 長さフレーム
	/// </summary>
	public int FrameLen => FrameTo - FrameFrom;

	private static int ToFrame(double time)
	{
		var t = (decimal)time;
		const decimal divnum = 10000000m;
		return (int)(decimal.Divide(t, divnum) * MovieFPS);
	}
}