using System.Collections.ObjectModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using LibSasara.Model.Serialize;

namespace LibSasara.Model.FullContextLabel;

/// <summary>
/// hts/htk full context label manager class
/// </summary>
/// <remarks>
/// <see href="http://hts.sp.nitech.ac.jp/"/> lab_format.pdf (HTS-demo_NIT-ATR503-M001)
/// </remarks>
/// <seealso cref="Lab"/>
public class FullContextLab
{
	private static readonly string[] breaklines
		= new[] { "\n", "\r\n", "\r" };

	/// <summary>
	/// 音素単位各行
	/// </summary>
	public IList<FullContextLabLine> Lines { get; set; }

	/// <summary>
	/// full context labelファイル
	/// </summary>
	/// <remarks>
	/// OpenJTalkの解析結果を元に生成することを想定しています
	/// </remarks>
	public FullContextLab(
		string labData,
		int fps = 30
	)
	{
		Lines = labData
			.Split(
				breaklines,
				StringSplitOptions.RemoveEmptyEntries)
			.AsParallel().AsSequential()
			.Where(s => !string.IsNullOrEmpty(s))
			.Select((s, i) => AnalyzeLine(s, i, fps))
			.ToList()
			;
	}

	/// <summary>
	/// フルコンテキストラベルの文字列を解析
	/// </summary>
	/// <remarks>
	/// 日本語の場合: <br/>
	/// <code>
	/// <![CDATA[ p1^p2-p3+p4=p5 /A:a1+a2+a3 ]]>
	/// <![CDATA[ /B:b1-b2_b3 /C:c1_c2+c3 /D:d1+d2_d3 ]]>
	/// <![CDATA[ /E:e1 e2!e3 e4-e5 /F: f1 f2# f3 f4@ f5 f6| f7 f8 /G:g1 g2%g3 g4 g5 ]]>
	/// <![CDATA[ /H:h1 h2 /I:i1-i2@i3+i4&i5-i6|i7+i8 /J: j1 j2 ]]>
	/// <![CDATA[ /K:k1+k2-k3 ]]>
	/// </code>
	/// </remarks>
	/// <param name="line"></param>
	/// <param name="index"></param>
	/// <param name="fps"></param>
	/// <returns></returns>
	public static FullContextLabLine AnalyzeLine(
		string line,
		int index,
		int fps = 30)
	{
		var span = line.AsSpan();

		var from = 0.0;
		var to = 0.0;
		var fcLabel = ReadOnlySpan<char>.Empty;

		//1文字が数字なら、時間付き表記とみなす
		var hasTime = char.IsDigit(span[0]);
		if (hasTime)
		{
			var sep = SplitSpan(span, ' ');
			from = double.Parse(sep[0].ToString(), CultureInfo.InvariantCulture);
			to = double.Parse(sep[1].ToString(), CultureInfo.InvariantCulture);
			fcLabel = sep[2].Span;
		}
		else
		{
			fcLabel = span;
		}

		//フルコンテクストラベル分析
		var contexts = SplitSpan(fcLabel, '/');
		var phoneme = GetCurrentPhoneme(contexts);

		var curtMoraInfo = contexts[1];
		var prevWordInfo = contexts[2];
		var curtWordInfo = contexts[3];
		var nextWordInfo = contexts[4];
		var prevAccPhrase = contexts[5];
		var curtAccPhrase = contexts[6];
		var nextAccPhrase = contexts[7];
		var prevBreathGroup = contexts[8];
		var curtBreathGroup = contexts[9];
		var nextBreathGroup = contexts[10];
		var utteranceInfo = contexts[11];

		return new FCLabLineJa(
			from,
			to,
			phoneme,
			fps,
			index: index,
			hasTime: hasTime)
		{
			MoraIdentity = new MoraIndentity(curtMoraInfo),
			//TODO:word, accphrase, breathgroup
			UtteranceInfo = new UtteranceInfoJa(utteranceInfo),
		};
	}

	/// <summary>
	/// 現在のLineの音素を返す
	/// </summary>
	/// <remarks>
	/// <list type="bullet">
	///     <listheader>
	///        <term>日本語Talk</term>
	///        <description><c>p1^p2-p3+p4=p5</c></description>
	///     </listheader>
	///     <item>
	///        <term>英語Talk</term>
	///        <description><c>p1^p2-p3+p4=p5@p6_p7</c></description>
	///     </item>
	///     <item>
	///        <term>Song</term>
	///        <description><c>p1@p2^p3-p4+p5=p6_p7%p8^p9_p10∼p11-p12!p13[p14$p15]p16</c></description>
	///     </item>
	/// </list>
	/// </remarks>
	/// <param name="contexts"></param>
	/// <returns></returns>
	private static string GetCurrentPhoneme(IReadOnlyList<ReadOnlyMemory<char>> contexts)
	{
		return FullContextLabUtil
			.GetCharsFromContext(
				contexts[0],
				'-',
				'+')
			.ToString();
	}

	/// <summary>
	/// ROSpan charを分割
	/// </summary>
	/// <param name="chars"></param>
	/// <param name="separator"></param>
	/// <returns></returns>
	public static IReadOnlyList<ReadOnlyMemory<char>>
	SplitSpan(
		ReadOnlySpan<char> chars,
		char separator)
	{
		var ret = new List<ReadOnlyMemory<char>>();
		for (var i = 0; i < chars.Length;)
		{
			var span = chars.Slice(i);
			var next = span.IndexOf(separator);
			if (next >= 0)
			{
				var mem = span
					.Slice(0, next)
					.ToArray()
					.AsMemory();
				ret.Add(mem);
				i += next + 1;
			}
			else
			{
				//見つからない
				ret.Add(span.ToArray().AsMemory());
				break;
			}
		}
		return ret;
	}
}

/// <summary>
/// フルコンテキストラベルの各行
/// </summary>
public abstract class FullContextLabLine : LabLine
{
	/// <summary>
	/// フルコンテキストラベルの各行を生成
	/// </summary>
	/// <param name="from"></param>
	/// <param name="to"></param>
	/// <param name="phenome"></param>
	/// <param name="fps"></param>
	/// <param name="index"></param>
	/// <param name="hasTime"></param>
	protected FullContextLabLine(
		double from,
		double to,
		string phenome,
		int fps = 30,
		int index = -1,
		bool hasTime = false
	) : base(from, to, phenome, fps)
	{
		Index = index;
		HasTime = hasTime;
	}

	/// <summary>
	/// インデックス（いくつ目の音素か）
	/// </summary>
	public int Index { get; }

	/// <summary>
	/// 音素の時刻データを持っているかどうか
	/// </summary>
	public bool HasTime { get; }

	/// <summary>
	/// 言語
	/// </summary>
	public virtual string Language { get; }
		= string.Empty;
}

/// <summary>
/// 日本語Talkのフルコンテキストラベルの各行
/// </summary>
public sealed class FCLabLineJa : FullContextLabLine
{
	/// <inheritdoc/>
	public FCLabLineJa(
		double from,
		double to,
		string phenome,
		int fps = 30,
		int index = -1,
		bool hasTime = false)
		: base(from, to, phenome, fps, index, hasTime)
	{
	}

	/// <inheritdoc/>
	public override string Language => "Japanese";

	/// <inheritdoc cref="MoraIndentity"/>
	public MoraIndentity? MoraIdentity { get; set; }

	/// <summary>
	/// prev./curt./nextの <see cref="WordInfo"/>
	/// </summary>
	public ContinuousContexts<WordInfo>? WordInfos { get; set; }
	/// <summary>
	/// prev./curt./nextの <see cref="IAccentPhrase"/>
	/// </summary>
	public ContinuousContexts<IAccentPhrase>? AccentPhrases { get; set; }
	/// <summary>
	/// prev./curt./nextの <see cref="BreathGroup"/>
	/// </summary>
	public ContinuousContexts<BreathGroup>? BreathGroups { get; set; }
	/// <inheritdoc cref="UtteranceInfoJa"/>
	public UtteranceInfoJa? UtteranceInfo { get; set; }
}

/// <summary>
/// 英語Talkのフルコンテキストラベルの各行
/// </summary>
public sealed class FCLabLineEn : FullContextLabLine
{
	/// <inheritdoc/>
	public FCLabLineEn(
		double from,
		double to,
		string phenome,
		int fps = 30,
		int index = -1,
		bool hasTime = false)
		: base(from, to, phenome, fps, index, hasTime)
	{
		throw new NotSupportedException();
	}

	/// <inheritdoc/>
	public override string Language => "English";

	/// <inheritdoc cref="UtteranceInfoEn"/>
	public UtteranceInfoEn? UtteranceInfo { get; set; }
}

/// <summary>
/// prev., curt., next の一続きの <typeparamref name="T"/> のコンテキスト情報
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed record ContinuousContexts<T>
	where T: IFullContext
{
	/// <inheritdoc cref="ContinuousContexts{T}"/>
	public ContinuousContexts(
		T previous,
		T current,
		T next
	)
	{
		Curt = current;
		Prev = previous;
		Next = next;
	}

	/// <summary>
	/// 一つ前の <typeparamref name="T"/>
	/// </summary>
	/// <seealso cref="PosOfContinous.Prev"/>
	public T Prev { get; private set; }
	/// <summary>
	/// 現在の <typeparamref name="T"/>
	/// </summary>
	/// <seealso cref="PosOfContinous.Curt"/>
	public T Curt { get; private set; }
	/// <summary>
	/// 次の <typeparamref name="T"/>
	/// </summary>
	/// <seealso cref="PosOfContinous.Next"/>
	public T Next { get; private set; }

	/// <summary>
	/// 指定値のコンテクスト<typeparamref name="T"/>を返す
	/// </summary>
	/// <param name="position">指定位置</param>
	/// <returns></returns>
	public T GetContext(PosOfContinous position)
		=> position switch
		{
			PosOfContinous.Prev => Prev,
			PosOfContinous.Curt => Curt,
			PosOfContinous.Next => Next,
			_ => Curt,
		};
}

/// <summary>
/// フルコンテクストラベルのコンテキスト情報
/// </summary>
public interface IFullContext{}

/// <summary>
/// フルコンテクストラベルのコンテキスト情報
/// prev, current, nextのセットの一つであるもの
/// </summary>
public interface IContinousContext: IFullContext {
	/// <summary>
	/// 現在のコンテクスト情報の位置
	/// </summary>
	public PosOfContinous PosOfContinous {get;}
}

/// <summary>
/// position of countinous
/// </summary>
public enum PosOfContinous
{
	/// <summary>
	/// previous
	/// </summary>
	Prev,
	/// <summary>
	/// current
	/// </summary>
	Curt,
	/// <summary>
	/// next
	/// </summary>
	Next,
}

/// <summary>
/// 日本語の単語(word)
/// <c>X:x1-x2_x3</c>
/// </summary>
public sealed record WordInfo: IFullContext
{
	private static readonly List<List<(char Before, char? End)>> Delimiters = new()
	{
		new(){(':','-'), ('-','_'), ('_',null)},
		new(){(':','_'), ('_','+'), ('+',null)},
		new(){(':','+'), ('+','_'), ('_',null)},
	};

	/// <summary>
	/// 単語(word)
	/// </summary>
	/// <param name="partOfSpeech"></param>
	/// <param name="infrectedForms"></param>
	/// <param name="conjugationType"></param>
	public WordInfo(
		string partOfSpeech,
		string infrectedForms,
		string conjugationType)
	{
		PartOfSpeech = partOfSpeech;
		InfrectedForms = infrectedForms;
		ConjugationType = conjugationType;
	}
	/// <summary>
	/// pos (part-of-speech)
	/// </summary>
	public string PartOfSpeech { get; set; }
	/// <summary>
	/// inflected forms
	/// </summary>
	public string InfrectedForms { get; set; }
	/// <summary>
	/// conjugation type
	/// </summary>
	public string ConjugationType { get; set; }
}

/// <summary>
/// 日本語のアクセント句(accent phrase)
/// </summary>
public sealed record AccentPhrase : IAccentPhrase
{
	/// <summary>
	/// アクセント句のコンストラクタ
	/// </summary>
	/// <param name="moraNumber"></param>
	/// <param name="accentType"></param>
	/// <param name="isInterrogative"></param>
	public AccentPhrase(
		int moraNumber,
		string accentType,
		bool isInterrogative
	)
	{
		MoraNumber = moraNumber;
		AccentType = accentType;
		IsInterrogative = isInterrogative;
	}

	/// <inheritdoc />
	public int MoraNumber { get; private set; }
	/// <inheritdoc />
	public string AccentType { get; private set; }
	/// <inheritdoc />
	public bool IsInterrogative { get; private set; }

	#region prev_and_next_only

	/// <summary>
	/// whether pause insertion or not in between the previous/next accent phrase and the current accent phrase <br/>
	/// <strong>previous/next accent phrase only</strong>
	/// </summary>
	public bool? IsPoseInsertion { get; private set; }

	#endregion prev_and_next_only

	#region current_only

	/// <summary>
	/// position of the current breath group identity by breath group (forward)<br/>
	/// <strong>current accent phrase only</strong>
	/// </summary>
	/// <value>1 ∼ 49</value>
	public int? PosInForwordBreathGroup { get; private set; }

	/// <summary>
	///position of the current breath group identity by breath group (backward)<br/>
	/// <value>1 ∼ 49</value>
	/// <strong>current accent phrase only</strong>
	/// </summary>
	public int? PosInBackwordBreathGroup { get; private set; }

	/// <summary>
	/// position of the current accent phrase identity in the current breath group by the accent phrase (forward)<br/>
	/// <strong>current accent phrase only</strong>
	/// </summary>
	/// <value>1 ∼ 49</value>
	public int? PosInForwordAccentPhrase { get; private set; }
	/// <summary>
	/// position of the current accent phrase identity in the current breath group by the accent phrase (backward)<br/>
	/// <strong>current accent phrase only</strong>
	/// </summary>
	/// <value>1 ∼ 49</value>
	public int? PosInBackwordAccentPhrase { get; private set; }
	/// <summary>
	/// position of the current accent phrase identity in the current breath group by the mora (forward)<br/>
	/// <strong>current accent phrase only</strong>
	/// </summary>
	/// <value>1 ∼ 199</value>
	public int? PosInForwordMora { get; private set; }
	/// <summary>
	/// position of the current accent phrase identity in the current breath group by the mora (backward)<br/>
	/// <strong>current accent phrase only</strong>
	/// </summary>
	/// <value>1 ∼ 199</value>
	public int? PosInBackwordMora { get; private  set; }

	#endregion
}

/// <inheritdoc cref="AccentPhrase"/>
public interface IAccentPhrase: IFullContext
{
	/// <summary>
	/// the number of moras
	/// </summary>
	/// <value>1 ∼ 49</value>
	public int MoraNumber { get; }
	/// <summary>
	/// accent type
	/// </summary>
	/// <value>1 ∼ 49</value>
	public string AccentType { get; }
	/// <summary>
	/// whether the current accent phrase interrogative or not
	/// </summary>
	public bool IsInterrogative { get; }
}

/// <summary>
/// 日本語の呼気段落(breath group)
/// </summary>
public sealed record BreathGroup: IFullContext
{
	/// <inheritdoc cref="BreathGroup"/>
	public BreathGroup(
		int numAccentPhrase,
		int numMora)
	{
		AccentPhraseNumber = numAccentPhrase;
		MoraNumber = numMora;
	}

	/// <summary>
	/// the number of accent phrases
	/// </summary>
	/// <value>1 ∼ 49</value>
	public int AccentPhraseNumber { get; set; }

	/// <summary>
	/// the number of moras
	/// </summary>
	/// <value>1 ∼ 99</value>
	public int MoraNumber { get; set; }

	#region current_only

	/// <summary>
	/// position of the current breath group identity by breath group (forward)
	/// </summary>
	public int? PosInForwordBreathGroup { get; set; }
	/// <summary>
	/// position of the current breath group identity by breath group (backward)
	/// </summary>
	public int? PosInBackwordBreathGroup { get; set; }
	/// <summary>
	/// position of the current breath group identity by accent phrase (forward)
	/// </summary>
	public int? PosInForwordAccentPhrase { get; set; }
	/// <summary>
	/// position of the current breath group identity by accent phrase (backward)
	/// </summary>
	public int? PosInBackwordAccentPhrase { get; set; }
	/// <summary>
	/// position of the current breath group identity by mora (forward)
	/// </summary>
	public int? PosInForwordMora { get; set; }
	/// <summary>
	/// position of the current breath group identity by mora (backward)
	/// </summary>
	public int? PosInBackwordMora { get; set; }

	#endregion
}

/// <summary>
/// 発話情報
/// </summary>
public interface IUtteranceInfo{
	/// <summary>
	/// 言語
	/// </summary>
	public string Launguage { get; }
}

/// <summary>
/// 日本語トークのこの <see cref="FullContextLabLine"/> が含まれる発話(utterance)情報
/// </summary>
/// <remarks>
/// 同じ<see cref="FullContextLab"/>内のすべての<see cref="FullContextLabLine"/>のこの情報は共通になる
/// </remarks>
public sealed record UtteranceInfoJa: IFullContext, IUtteranceInfo
{
	/// <summary>
	/// 呼気段落の数
	/// </summary>
	public int BreathGroup { get; set; }
	/// <summary>
	/// アクセント句の数
	/// </summary>
	public int AccentPhrase { get; set; }
	/// <summary>
	/// モーラの数
	/// </summary>
	public int Mora { get; set; }

	/// <inheritdoc/>
	public string Launguage => "Japanese";

	/// <inheritdoc cref="UtteranceInfoJa"/>
	public UtteranceInfoJa(
		int breathGroup,
		int accentPhrase,
		int mora)
	{
		BreathGroup = breathGroup;
		AccentPhrase = accentPhrase;
		Mora = mora;
	}

	/// <inheritdoc cref="UtteranceInfoJa"/>
	///
	/// <param name="context"><c>K:k1+k2-k3</c></param>
	public UtteranceInfoJa(
		ReadOnlyMemory<char> context
	){
		var bg = FullContextLabUtil
			.GetCharsFromContext(context, ':', '+')
			.ToString();
		var ap = FullContextLabUtil
			.GetCharsFromContext(context, '+', '-')
			.ToString();
		var mo = FullContextLabUtil
			.GetCharsFromContext(context, '-')
			.ToString();
		BreathGroup = FullContextLabUtil.GetNumber(bg);
		AccentPhrase = FullContextLabUtil.GetNumber(ap);
		Mora = FullContextLabUtil.GetNumber(mo);
	}

	/// <summary>
	/// return string as a <c>K:k1+k2-k3</c>
	/// </summary>
	public override string ToString()
	{
		return $"K:{Check(BreathGroup)}+{Check(AccentPhrase)}-{Check(Mora)}";

		static string Check(int value)
			=> value == -1
				? "xx"
				: value.ToString(CultureInfo.InvariantCulture);
	}
}

/// <summary>
/// 英語トークのこの <see cref="FullContextLabLine"/> が含まれる発話(utterance)情報
/// </summary>
/// <remarks>
/// 同じ<see cref="FullContextLab"/>内のすべての<see cref="FullContextLabLine"/>のこの情報は共通になる
/// </remarks>
public sealed record UtteranceInfoEn: IFullContext, IUtteranceInfo
{
	//TODO:impliment

	/// <inheritdoc/>
	public string Launguage => "English";
}