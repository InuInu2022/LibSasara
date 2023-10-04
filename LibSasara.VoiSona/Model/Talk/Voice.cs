using System;

namespace LibSasara.VoiSona.Model.Talk;

/// <summary>
/// トラックの話者（キャスト）情報
/// </summary>
public sealed class Voice : Tree
{
	/// <inheritdoc/>
	public new static int Count { get => 0; }

	/// <summary>
	/// 話者（キャスト）
	/// </summary>
	public string Speaker { get; }

	/// <summary>
	/// 内部ID(ライブラリファイル名)
	/// </summary>
	public string Id { get; }

	/// <summary>
	/// ボイスライブラリのバージョン
	/// </summary>
	/// <remarks>
	/// ボイスライブラリのバージョン表記は"1.0.0 ported from CeVIO AI"等の様にSemantic versioningにしたがっていません。
	/// <see cref="System.Version"/>型ではなく文字列で返ります。
	/// </remarks>
	public string Version { get; }

	/// <inheritdoc cref="Voice"/>
	/// <param name="speaker"></param>
	/// <param name="name"></param>
	/// <param name="version"></param>
	public Voice(
		string speaker,
		string name,
		string version
	) : base(nameof(Voice))
	{
		Speaker = speaker;
		Id = name;
		Version = version;

		AddAttribute(nameof(speaker), speaker, VoiSonaValueType.String);
		AddAttribute(nameof(name), name, VoiSonaValueType.String);
		AddAttribute(nameof(version), version, VoiSonaValueType.String);
	}
}