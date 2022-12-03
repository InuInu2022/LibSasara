using System;
using System.Xml.Serialization;

namespace LibSasara.Model.Serialize;

/// <summary>
/// versionを持つ要素
/// </summary>
public interface IHasVersion
{
    /// <summary>
	/// バージョン
	/// </summary>
    [XmlIgnore]
    Version? Version { get; set; }

    /// <summary>
	/// <inheritdoc cref="Version"/>
	/// </summary>
    [XmlElement("Version")]
    string? VersionString { get; set; }
}
