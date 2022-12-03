using System.Xml.Linq;
using System;
using System.Collections.Generic;
using LibSasara.Model;

namespace LibSasara.Builder;

/// <summary>
/// <see cref="TalkUnit"/>を生成するBuilder
/// </summary>
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
	/// <param name="ccs">TalkUnitを追加する<see cref="CcsProject"/> or <see cref="CcstTrack"/></param>
	/// <param name="StartTime"><inheritdoc cref="UnitBase.StartTime"/></param>
	/// <param name="Duration"></param>
	/// <param name="CastId"><inheritdoc cref="UnitBase.CastId"/></param>
	/// <param name="Text"><inheritdoc cref="TalkUnit.Text"/></param>
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

	/// <inheritdoc cref="UnitBase.Language"/>
	public TalkUnitBuilder Language(string lang)
	{
		language = lang;
		return this;
	}

	/// <inheritdoc cref="TalkUnit.Volume"/>
	public TalkUnitBuilder Volume(decimal volume)
	{
		this.volume = volume;
		return this;
	}

	/// <inheritdoc cref="Volume(decimal)"/>
	public TalkUnitBuilder Volume(double volume)
		=> Volume(Convert.ToDecimal(volume));

	/// <inheritdoc cref="TalkUnit.Speed"/>
	public TalkUnitBuilder Speed(decimal speed = 1.0m)
	{
		this.speed = speed;
		return this;
	}

	/// <inheritdoc cref="TalkUnit.Speed"/>
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

	/// <inheritdoc cref="TalkUnit.Alpha"/>
	public TalkUnitBuilder Alpha(double alpha = 0.55)
		=> Alpha(Convert.ToDecimal(alpha));

	/// <inheritdoc cref="TalkUnit.LogF0Scale"/>
	public TalkUnitBuilder LogF0Scale(decimal scale = 1m)
	{
		this.logF0Scale = scale;
		return this;
	}

	/// <inheritdoc cref="TalkUnit.LogF0Scale"/>
	public TalkUnitBuilder LogF0Scale(double scale = 1)
		=> LogF0Scale(Convert.ToDecimal(scale));

	/// <inheritdoc cref="TalkUnit.Components"/>
	public TalkUnitBuilder Components(IEnumerable<(string id, decimal value)> comps)
	{
		this.components = comps;
		return this;
	}

	/// <inheritdoc cref="TalkUnit.Phonemes"/>
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
				new List<XElement>(1)
				{
					rawElem
				}
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
