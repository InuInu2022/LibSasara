using System.Globalization;
using System.Linq;
using System.Xml.Serialization;

namespace LibSasara.Model.Serialize;

/// <summary>
/// 音設定
/// </summary>
public record SoundSetting
{
	/// <summary>
	/// 拍子。n / m
	/// </summary>
	[XmlIgnore]
	public Time? Rhythm {
		get {
			if(string.IsNullOrEmpty(RhythmStr)){
				return new(){Beats=4,BeatType=4};
			}

			var rhythms = RhythmStr!
				.Split("/"[0])
				.Select(v => System.Convert.ToUInt32(v, CultureInfo.InvariantCulture));
			return new() {
				Beats = rhythms.ElementAt(0),
				BeatType = rhythms.ElementAt(1),
			};
		}

		set { RhythmStr = $"{value?.Beats}/{value?.BeatType}"; }
	}
	/// <summary>
	/// <inheritdoc cref="Rhythm"/>
	/// </summary>
	[XmlAttribute(nameof(Rhythm))]
	public string? RhythmStr { get; set; }
	/// <summary>
	/// テンポ。
	/// </summary>
	[XmlAttribute]
	public double Tempo { get; set; }
	/// <summary>
	/// マスターボリューム
	/// </summary>
	[XmlAttribute]
	public double MasterVolume { get; set; }
}