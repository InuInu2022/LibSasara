using System.Xml.Linq;
using System;
using System.Collections.Generic;
using LibSasara.Model;

namespace LibSasara.Builder;

/// <summary>
/// <see cref="TalkUnit"/>を生成するBuilder
/// </summary>
/// <seealso cref="TalkUnit"/>
/// <seealso cref="SongUnitBuilder"/>
public sealed class TalkUnitBuilder : IUnitBuilder<TalkUnit, TalkUnitBuilder>
{
	private TalkUnitBuilder(
		CeVIOFileBase ccs,
		TimeSpan StartTime,
		TimeSpan Duration,
		string CastId,
		string? Text
	)
	{
		this.ccs = ccs;
		startTime = StartTime;
		duration = Duration;
		castId = CastId;
		text = Text;
	}

	/// <summary>
	/// Builderパターンで<see cref="TalkUnit"/>を生成する
	/// <see cref="TalkUnitBuilder"/>を作成
	/// </summary>
	/// <example>
	/// <code>
	/// var talkUnit = TalkUnitBuilder
    /// 	.Create(ccs, start, duration, id, "serif")
	/// 	.Build();
	/// </code>
	/// </example>
	/// <param name="ccs"><see cref="TalkUnit"/>を追加する<see cref="CcsProject"/> or <see cref="CcstTrack"/></param>
	/// <param name="StartTime"><see cref="UnitBase.StartTime"/>の値。</param>
	/// <param name="Duration"><see cref="UnitBase.Duration"/>の値。</param>
	/// <param name="CastId"><see cref="TalkUnit.CastId"/>の値。</param>
	/// <param name="Text"><see cref="TalkUnit.Text"/>の値。台詞。日本語200文字、英語500文字まで。</param>
	/// <seealso cref="Build"/>
	public static TalkUnitBuilder Create(
		CeVIOFileBase ccs,
		TimeSpan StartTime,
		TimeSpan Duration,
		string CastId,
		string Text
	)
		=> new(ccs, StartTime, Duration, CastId, Text);

	/// <inheritdoc cref="UnitBase.Group"/>
	public TalkUnitBuilder Group(Guid guid)
	{
		group = guid;
		return this;
	}

	/// <inheritdoc cref="TalkUnit.Language"/>
	public TalkUnitBuilder Language(string lang)
	{
		language = lang;
		return this;
	}

	/// <inheritdoc cref="TalkUnit.Volume"/>
    /// <seealso cref="Volume(double)"/>
	public TalkUnitBuilder Volume(decimal volume)
	{
		this.volume = volume;
		return this;
	}

	/// <inheritdoc cref="Volume(decimal)"/>
    /// <seealso cref="Volume(decimal)"/>
	public TalkUnitBuilder Volume(double volume)
		=> Volume(Convert.ToDecimal(volume));

	/// <inheritdoc cref="TalkUnit.Speed"/>
    /// <seealso cref="Speed(double)"/>
	public TalkUnitBuilder Speed(decimal speed = 1.0m)
	{
		this.speed = speed;
		return this;
	}

	/// <inheritdoc cref="TalkUnit.Speed"/>
    /// <seealso cref="Speed(decimal)"/>
	public TalkUnitBuilder Speed(double speed = 1.0)

		=> Speed(Convert.ToDecimal(speed));

	/// <inheritdoc cref="TalkUnit.Tone"/>
	public TalkUnitBuilder Tone(decimal tone)
	{
		this.tone = tone;
		return this;
	}

	/// <inheritdoc cref="TalkUnit.Tone"/>
	public TalkUnitBuilder Tone(double tone)
		=> Tone(Convert.ToDecimal(tone));

	/// <inheritdoc cref="TalkUnit.Alpha"/>
	public TalkUnitBuilder Alpha(decimal alpha = 0.55m)
	{
		this.alpha = alpha;
		return this;
	}

	/// <inheritdoc cref="TalkUnit.Alpha" path="/summary"/>
	public TalkUnitBuilder Alpha(double alpha = 0.55)
		=> Alpha(Convert.ToDecimal(alpha));

	/// <inheritdoc cref="TalkUnit.LogF0Scale" path="/summary"/>
    /// <seealso cref="LogF0Scale(double)"/>
	public TalkUnitBuilder LogF0Scale(decimal scale = 1m)
	{
		this.logF0Scale = scale;
		return this;
	}

	/// <inheritdoc cref="TalkUnit.LogF0Scale" path="/summary"/>
    /// <seealso cref="LogF0Scale(decimal)"/>
	public TalkUnitBuilder LogF0Scale(double scale = 1)
		=> LogF0Scale(Convert.ToDecimal(scale));

	/// <inheritdoc cref="TalkUnit.Components"/>
	public TalkUnitBuilder Components(IEnumerable<(string id, decimal value)> comps)
	{
		this.components = comps;
		return this;
	}

	/// <inheritdoc cref="TalkUnit.Phonemes" path="/summary"/>
    /// <param name="phonemes"><see cref="TalkUnit.Phonemes"/>の値。<see cref="TalkPhoneme"/>のコレクション。</param>
    /// <seealso cref="TalkUnit.Phonemes"/>
    /// <seealso cref="TalkPhoneme"/>
    /// <seealso cref="SasaraLabel"/>
	public TalkUnitBuilder Phonemes(IEnumerable<TalkPhoneme> phonemes)
	{
		this.phonemes = phonemes;
		return this;
	}

	/// <summary>
	/// 最後に呼ぶ
	/// </summary>
	/// <param name="canAdd"></param>
	/// <returns>作成した<see cref="TalkUnit"/></returns>
    /// <seealso cref="Create(CeVIOFileBase, TimeSpan, TimeSpan, string, string)"/>
	public TalkUnit Build(bool canAdd = true)
	{
		var rawElem = TalkUnit.CreateTalkUnitRaw(
			startTime,
			duration,
			castId,
			text,
			group,
			language,
			volume,
			speed,
			tone,
			alpha,
			logF0Scale,
			components,
			phonemes
		);
		if(canAdd){
			ccs.AddUnits(
				[
					rawElem,
				]
			);
		}

		return new(rawElem, ccs);
	}

	#region required params

	private readonly CeVIOFileBase ccs;
	private readonly TimeSpan startTime;
	private readonly TimeSpan duration;
	private readonly string castId;
	private readonly string? text;

	#endregion required params

	#region optional params

	private Guid? group;
	private string? language = "Japanese";
	private decimal volume;
	private decimal speed = 1;
	private decimal tone;
	private decimal alpha = 0.55m;
	private decimal logF0Scale = 1m;
	private IEnumerable<(string Id, decimal Value)>? components;
	private IEnumerable<TalkPhoneme>? phonemes;

	#endregion optional params
}
