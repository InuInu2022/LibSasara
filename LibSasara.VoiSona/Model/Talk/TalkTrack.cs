using System;
using System.Collections.Generic;
using System.Linq;

namespace LibSasara.VoiSona.Model.Talk;

/// <summary>
/// tstprj Track
/// </summary>
public class TalkTrack : Tree
{
	/// <inheritdoc cref="TalkTrack"/>
	public TalkTrack() : base("Track")
	{
	}

	/// <summary>
	/// Track name
	/// </summary>
	public string TrackName {
		get => Attributes
			.FirstOrDefault(v => v.Key == "name")
			.Value;
	}

	/// <summary>
	/// Voice
	/// </summary>
	public Voice? Voice{
		get => Children
			.FirstOrDefault(v => v.Name == nameof(Voice)) as Voice
			;
	}

	/// <summary>
	/// セリフ一覧
	/// </summary>
	public List<Utterance> Utterances
	{
		get
		{
			var hasContents = Children.Exists(v => v.Name == "Contents");

			return hasContents ?
				Children?
					.FirstOrDefault(v => v.Name == "Contents")
					.Children
					.Cast<Utterance>()
					.ToList()
					?? new List<Utterance>()
				: new List<Utterance>()
			;
		}
	}
}