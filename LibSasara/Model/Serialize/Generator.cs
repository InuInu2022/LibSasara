using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace LibSasara.Model.Serialize;

/// <summary>
/// ファイルを生成したツール。
/// </summary>
public record Generator
{
	/// <summary>
	/// author
	/// </summary>
	[XmlElement]
	public Author? Author { get; set; }

	/// <summary>
	/// talk engine info
	/// </summary>
	[XmlElement]
	public TalkEngine? TTS { get; set; }

	/// <summary>
	/// song engine info
	/// </summary>
	[XmlElement]
	public SongEngine? SVSS { get; set; }
}

/// <summary>
/// <inheritdoc cref="Generator.Author"/>
/// </summary>
public record Author : IHasVersion
{
	///<inheritdoc/>
	[XmlIgnore]
	public Version? Version {
		get=> VersionString is null ? null : new(VersionString);
		set { VersionString = value?.ToString(); }
	}

	/// <inheritdoc cref="Version"/>
    [XmlAttribute("Version")]
    public string? VersionString { get; set; }
}

/// <summary>
/// synth engine
/// </summary>
public interface IEngine : IHasVersion
{
	/// <summary>
	/// 辞書
	/// </summary>
	[XmlElement]
	public DictionaryInfo? Dictionary { get; set; }

	/// <summary>
	/// ボイスライブラリ
	/// </summary>
	[XmlArray]
	[XmlArrayItem("SoundSource")]
	public List<SoundSource>? SoundSources { get; set; }
}

/// <summary>
/// 辞書情報
/// </summary>
public record DictionaryInfo : IHasVersion
{
	///<inheritdoc/>
	[XmlIgnore]
	public Version? Version {
		get=> VersionString is null ? null : new(VersionString);
		set { VersionString = value?.ToString(); }
	}

	/// <inheritdoc cref="Version"/>
    [XmlAttribute("Version")]
    public string? VersionString { get; set; }

	/// <summary>
	/// 日本語以外の辞書情報
	/// </summary>
	[XmlElement]
	public ExtensionInfo? Extension { get; set; }
}

/// <summary>
/// 追加辞書情報
/// </summary>
public record ExtensionInfo : IHasVersion
{
	///<inheritdoc/>
	[XmlIgnore]
	public Version? Version {
		get=> VersionString is null ? null : new(VersionString);
		set { VersionString = value?.ToString(); }
	}

	/// <inheritdoc cref="Version"/>
    [XmlAttribute("Version")]
    public string? VersionString { get; set; }

	/// <summary>
	/// 追加辞書の言語。English / Chinese
	/// </summary>
	[XmlAttribute]
	public string? Language { get; set; }
}

/// <summary>
/// ボイスライブラリ情報
/// </summary>
public record SoundSource : IHasVersion
{
	///<inheritdoc/>
	[XmlIgnore]
	public Version? Version {
		get=> VersionString is null ? null : new(VersionString);
		set { VersionString = value?.ToString(); }
	}

	/// <inheritdoc cref="Version"/>
    [XmlAttribute("Version")]
    public string? VersionString { get; set; }

	/// <summary>
	/// ボイスライブラリのキャストID
	/// </summary>
	[XmlAttribute("Id")]
	public string? CastId { get; set; }

	/// <summary>
	/// ボイスライブラリのキャスト名
	/// </summary>
	[XmlAttribute]
	public string? Name { get; set; }
}

/// <summary>
/// 生成時のトークエンジン。
/// </summary>
public record TalkEngine : IEngine
{
	///<inheritdoc/>
	[XmlIgnore]
	public Version? Version {
		get=> VersionString is null ? null : new(VersionString);
		set { VersionString = value?.ToString(); }
	}

	/// <inheritdoc cref="Version"/>
    [XmlAttribute("Version")]
    public string? VersionString { get; set; }

	///<inheritdoc />
	[XmlElement]
	public DictionaryInfo? Dictionary { get; set; }

	///<inheritdoc />
	[XmlArray]
	[XmlArrayItem("SoundSource")]
	public List<SoundSource>? SoundSources { get; set; }
}

/// <summary>
/// 生成時のソングエンジン。
/// </summary>
public record SongEngine : IEngine
{
	///<inheritdoc/>
	[XmlIgnore]
	public Version? Version {
		get=> VersionString is null ? null : new(VersionString);
		set { VersionString = value?.ToString(); }
	}

	/// <inheritdoc cref="Version"/>
    [XmlAttribute("Version")]
    public string? VersionString { get; set; }

	///<inheritdoc />
	[XmlElement]
	public DictionaryInfo? Dictionary { get; set; }

	///<inheritdoc />
	[XmlArray]
	[XmlArrayItem("SoundSource")]
	public List<SoundSource>? SoundSources { get; set; }
}