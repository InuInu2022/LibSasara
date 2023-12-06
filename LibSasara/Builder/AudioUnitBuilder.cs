using System;
using System.Collections.Generic;
using System.Xml.Linq;
using LibSasara.Model;

namespace LibSasara.Builder;

/// <summary>
/// <see cref="AudioUnit"/>を生成するBuilder
/// </summary>
/// <seealso cref="AudioUnit"/>
/// <seealso cref="TalkUnitBuilder"/>
/// <seealso cref="SongUnitBuilder"/>
public sealed class AudioUnitBuilder : IUnitBuilder<AudioUnit, AudioUnitBuilder>
{
	private AudioUnitBuilder(
		CeVIOFileBase ccs,
		TimeSpan StartTime,
		TimeSpan Duration,
		string FilePath
	)
	{
		this.ccs = ccs;
		startTime = StartTime;
		duration = Duration;
		filePath = FilePath;
	}

	/// <summary>
	/// Builderパターンで<see cref="AudioUnit"/>を生成する
	/// <see cref="AudioUnitBuilder"/>を作成
	/// </summary>
	/// <param name="ccs"><see cref="AudioUnit"/>を追加する<see cref="CcsProject"/> or <see cref="CcstTrack"/></param>
	/// <param name="StartTime"><see cref="UnitBase.StartTime"/></param>
	/// <param name="Duration"><see cref="UnitBase.Duration"/></param>
	/// <param name="FilePath"><see cref="AudioUnit.FilePath"/></param>
	/// <seealso cref="Build"/>
    /// <seealso cref="AudioUnit"/>
	public static AudioUnitBuilder Create(
		CeVIOFileBase ccs,
		TimeSpan StartTime,
		TimeSpan Duration,
		string FilePath
	)
		=> new(ccs, StartTime, Duration, FilePath);

	/// <summary>
	/// 最後に呼ぶ
	/// </summary>
	/// <param name="canAdd">生成と同時にccs/ccstに追加する</param>
	/// <returns>作成した<see cref="AudioUnit"/></returns>
	public AudioUnit Build(bool canAdd = true)
	{
		var rawElem = AudioUnit.CreateAudioUnitRaw(
			startTime,
			duration,
			filePath,
			group
		);
		if(canAdd){
			ccs.AddUnits(
				new List<XElement>(1)
				{
					rawElem,
				}
			);
		}

		return new(rawElem, ccs);
	}

	/// <inheritdoc/>
    /// <exception cref="NotImplementedException"></exception>
	public AudioUnitBuilder Group(Guid guid)
	{
		group = guid;
		return this;
	}

	#region required params

	private readonly CeVIOFileBase ccs;
	private readonly TimeSpan startTime;
	private readonly TimeSpan duration;
	private readonly string filePath;

	#endregion required params

	#region optional params

	private Guid? group;

	#endregion optional params
}