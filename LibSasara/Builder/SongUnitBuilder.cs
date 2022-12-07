using System;
using System.Collections.Generic;
using System.Xml.Linq;
using LibSasara.Model;

namespace LibSasara.Builder;

/// <summary>
/// <see cref="SongUnit"/>を生成するBuilder
/// </summary>
/// <seealso cref="SongUnit"/>
/// <seealso cref="TalkUnitBuilder"/>
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
	/// <param name="StartTime"><see cref="UnitBase.StartTime"/></param>
	/// <param name="Duration"><see cref="UnitBase.Duration"/></param>
	/// <param name="CastId"><see cref="UnitBase.CastId"/></param>
	/// <seealso cref="Build"/>
	public static SongUnitBuilder Create(
		CeVIOFileBase ccs,
		TimeSpan StartTime,
		TimeSpan Duration,
		string CastId
	)
		=> new(ccs, StartTime, Duration, CastId);

	/// <inheritdoc cref="UnitBase.Group"/>
    /// <param name="guid"><see cref="UnitBase.Group"/></param>
    /// <seealso cref="UnitBase.Group"/>
	public SongUnitBuilder Group(Guid guid)
	{
		group = guid;
		return this;
	}

	/// <inheritdoc cref="UnitBase.Language"/>
    /// <seealso cref="UnitBase.Language"/>
	public SongUnitBuilder Language(string lang)
	{
		language = lang;
		return this;
	}

	/// <inheritdoc cref="SongUnit.Tempo"/>
    /// <param name="tempo"><see cref="SongUnit.Tempo"/></param>
    /// <seealso cref="SongUnit.Tempo"/>
	public SongUnitBuilder Tempo(SortedDictionary<int, int> tempo)
	{
		this.tempo = tempo;
		return this;
	}

    /// <inheritdoc cref="SongUnit.Beat"/>
    /// <param name="beat"><see cref="SongUnit.Beat"/></param>
    /// <seealso cref="SongUnit.Beat"/>
    public SongUnitBuilder Beat(SortedDictionary<int, (int Beats, int BeatType)> beat)
    {
		this.beat = beat;
		return this;
	}

	/// <summary>
	/// 最後に呼ぶ
	/// </summary>
	/// <param name="canAdd">生成と同時にccs/ccstに追加する</param>
	/// <returns>作成した<see cref="SongUnit"/></returns>
    /// <seealso cref="Create(CeVIOFileBase, TimeSpan, TimeSpan, string)"/>
	public SongUnit Build(bool canAdd = true)
	{
		var rawElem = SongUnit.CreateSongUnitRaw(
			startTime,
			duration,
			castId,
			group,
			language,
			tempo:tempo,
			beat:beat
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

	#endregion required params

	#region optional params

	private Guid? group;
	private string? language;
	private SortedDictionary<int, int>? tempo;

    private SortedDictionary<int, (int Beats, int BeatType)>? beat;

	#endregion optional params
}
