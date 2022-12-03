using System;
using System.Collections.Generic;
using LibSasara.Model;

namespace LibSasara.Builder;

/// <summary>
/// <see cref="SongUnit"/>を生成するBuilder
/// </summary>
public sealed class SongUnitBuilder : IUnitBuilder<SongUnit, SongUnitBuilder>
{
	private SongUnitBuilder(
		CeVIOFileBase ccs,
		TimeSpan StartTime,
		TimeSpan Duration,
		string CastId
	)
	{
		this.ccs = ccs;
		startTime = StartTime;
		duration = Duration;
		castId = CastId;
	}

	/// <summary>
	/// Builderパターンで<see cref="SongUnit"/>を生成する
	/// <see cref="SongUnitBuilder"/>を作成
	/// </summary>
	/// <param name="ccs">TalkUnitを追加する<see cref="CcsProject"/> or <see cref="CcstTrack"/></param>
	/// <param name="StartTime"><inheritdoc cref="UnitBase.StartTime"/></param>
	/// <param name="Duration"></param>
	/// <param name="CastId"><inheritdoc cref="UnitBase.CastId"/></param>
	/// <seealso cref="Build"/>
	public static SongUnitBuilder Create(
		CeVIOFileBase ccs,
		TimeSpan StartTime,
		TimeSpan Duration,
		string CastId
	)
		=> new(ccs, StartTime, Duration, CastId);

	/// <inheritdoc cref="UnitBase.Group"/>
	public SongUnitBuilder Group(Guid guid)
	{
		group = guid;
		return this;
	}

	/// <inheritdoc cref="UnitBase.Language"/>
	public SongUnitBuilder Language(string lang)
	{
		language = lang;
		return this;
	}

	/// <inheritdoc cref="SongUnit.Tempo"/>
	public SongUnitBuilder Tempo(SortedDictionary<int, int> tempo)
	{
		this.tempo = tempo;
		return this;
	}

    /// <inheritdoc cref="SongUnit.Beat"/>
    public SongUnitBuilder Beat(SortedDictionary<int, (int Beats, int BeatType)> beat)
    {
		this.beat = beat;
		return this;
	}

	/// <summary>
	/// 最後に呼ぶ
	/// </summary>
	/// <param name="doAdd">生成と同時にccs/ccstに追加する</param>
	/// <returns>作成した<see cref="SongUnit"/></returns>
	public SongUnit Build(bool doAdd = true)
	{
		throw new NotImplementedException();
		//return new(rawElem, ccs);
	}

	#region required params

	private readonly CeVIOFileBase ccs;
	private readonly TimeSpan startTime;
	private readonly TimeSpan duration;
	private readonly string castId;

	#endregion required params

	#region optional params

	private Guid? group;
	private string? language;
	private SortedDictionary<int, int>? tempo;

    private SortedDictionary<int, (int Beats, int BeatType)>? beat;

	#endregion optional params
}
