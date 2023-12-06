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
			.Find(v => string.Equals(v.Key, "name", System.StringComparison.Ordinal))
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
			.Find(v => string.Equals(v.Name, nameof(Voice), System.StringComparison.Ordinal)) as Voice
			;
		set {
			if (value is null) return;

			//Voiceはトークトラックに現状一つのみ
			var index = Children
				.FindIndex(v => string.Equals(v.Name, nameof(Voice), System.StringComparison.Ordinal));
			if (index < 0)
			{
				Children.Add(value);
			}
			else{
				Children[index] = value;
			}

			//Children.Add(value);
		}
	}

	/// <summary>
	/// セリフ一覧
	/// </summary>
	public List<Utterance> Utterances
	{
		get
		{
			return HasContents ?
				Children?
					.Find(v => string.Equals(v.Name, nameof(Contents), System.StringComparison.Ordinal))
					.Children
					.Cast<Utterance>()
					.ToList()
					?? []
				: []
			;
		}
		set
		{
			if(!HasContents || Contents is null){
				Contents = new Tree(nameof(Contents));
			}
			Contents.Children = value
				.Cast<Tree>().ToList();
		}
	}

	/// <summary>
	/// Content要素を持つかどうか
	/// </summary>
	public bool HasContents {
		get => Children
			.Exists(v => string.Equals(v.Name, nameof(Contents), System.StringComparison.Ordinal));
	}

	private Tree? _contents;
	private Tree? Contents{
		get => HasContents
			? _contents
			: default;
		set {
			if (value is null) return;
			if(!HasContents){
				Children.Add(value);
			}
			_contents = value;
		}
	}


}