using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;

namespace LibSasara.Model.Serialize;

/// <summary>
/// ccsの<see cref="Scene"/> / ccstの <c>Definision</c> 要素共通
/// </summary>
public interface IScene: IHasId
{
	/// <summary>
	/// エディタのタイムラインの表示設定
	/// </summary>
	Timeline? Timeline { get; set; }

	/// <summary>
	/// トークエディタの表示設定
	/// </summary>
	TalkEditor? TalkEditor { get; set; }

	/// <summary>
	/// ソングエディタの表示設定
	/// </summary>
	SongEditor? SongEditor { get; set; }

	/// <summary>
	/// ボイスユニット一覧
	/// </summary>
	List<Unit>? Units { get; set; }

	/// <summary>
	/// トラック一覧
	/// </summary>
	Groups<Group>? Groups { get; set; }

	/// <summary>
	/// 音設定
	/// </summary>
	SoundSetting? SoundSetting { get; set; }
}

/// <summary>
/// Scene共通実装
/// </summary>
public abstract record SceneBase: IScene
{
	/// <inheritdoc/>
	[XmlAttribute]
	public string? Id { get; set; }

	/// <inheritdoc/>
	[XmlElement]
	public Timeline? Timeline { get; set; }

	/// <inheritdoc/>
	[XmlElement]
	public TalkEditor? TalkEditor { get; set; }

	/// <inheritdoc/>
	[XmlElement]
	public SongEditor? SongEditor { get; set; }

	/// <inheritdoc/>
	[XmlIgnore]
	public List<Unit>? Units {
		get => UnitsRaw;

		set { UnitsRaw = value; }
	}

	/// <inheritdoc cref="Units"/>
	[XmlArray(nameof(Units))]
	[XmlArrayItem(typeof(Unit))]
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public List<Unit>? UnitsRaw { get; set; }

	/// <inheritdoc/>
	[XmlArray]
	[XmlArrayItem]
	public Groups<Group>? Groups { get; set; }

	/// <inheritdoc/>
	[XmlElement]
	public SoundSetting? SoundSetting { get; set; }
}