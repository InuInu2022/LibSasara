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
			.Find(v => v.Key == "name")
			.Value;
		set => AddAttribute("name", value, VoiSonaValueType.String);
	}

	/// <summary>
	/// Track volume (dB)
	/// </summary>
	public decimal Volume{
		get => GetAttribute<decimal>("volume")
			.Value;
		set => AddAttribute("volume", value, VoiSonaValueType.Double);
	}

	/// <summary>
	/// Track pan
	/// </summary>
	public decimal Pan{
		get => GetAttribute<decimal>("pan")
			.Value;
		set => AddAttribute("pan", value, VoiSonaValueType.Double);
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