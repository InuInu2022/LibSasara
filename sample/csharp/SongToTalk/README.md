# LibSasara sample "SongToTalk"

## What's this?

これは[LibSasara](https://github.com/InuInu2022/LibSasara)のサンプルコンソールアプリケーションです。
CeVIOのソングトラックのノートのデータを元に、トークトラックを生成し、ノートのタイミングに合わせてセリフを並べます。
要は[ボイパロイド](https://dic.nicovideo.jp/a/%E3%83%9C%E3%82%A4%E3%83%91%E3%83%AD%E3%82%A4%E3%83%89)をCeVIOトークでやるためのツールです。

[FluentCeVIOWrapper](https://github.com/InuInu2022/FluentCeVIOWrapper)と[World](http://www.isc.meiji.ac.jp/~mmorise/world/index.html) (DotnetWorld)との連携のサンプルにもなっています。

This is a sample console application for LibSasara.
It generates a talk track based on note data of a CeVIO song track and arranges the dialogue according to the timing of the notes.
In short, it is a tool to do "voipaloid" with CeVIO talk.

This is also a sample of how [FluentCeVIOWrapper](https://github.com/InuInu2022/FluentCeVIOWrapper) and [World](http://www.isc.meiji.ac.jp/~mmorise/world/index.html) (DotnetWorld), work together.

## Features

- CeVIOソングのccs or ccstファイルを元にトークトラックを生成し、ccsファイルに追記します
- 歌わせるようではなく、ボイスパーカッション用です
- 1つのノートに対し、1つのトークの台詞を作成します
- 台詞はノートの歌詞を元につくられます。
  - 1ノート、日本語で200文字までOK（CeVIO AIの場合）
- 台詞の音程の中央値は元のノートに合うよう自動で調整されます
  - 平坦なピッチにはなりません（現在はわざとそうしてます）

- Generate a talk track based on the ccs or ccst file of the CeVIO song and append it to the ccs file.
- This is not for singing, but for voice percussion.
- Create one talk dialogue per note.
- The dialogue is created based on the lyrics of the note.
  - One note can contain up to 200 Japanese characters (in the case of CeVIO AI).
- The median pitch of the dialogue is automatically adjusted to match the original note
  - The pitch will not be flat (this is currently done on purpose)

## Usage

```cmd
dotnet run -s path/to/sourcesong.ccst -d path/to/override.ccs --cast フィーちゃん
```

詳しくは`--help`コマンドで。

## Requirements

### Run

- .NET
- CeVIO (CS7 or AI) Talk

### Build

- .NET SDK 7
- FluentCeVIOWrapper
  - [FluentCeVIOWrapper](https://github.com/InuInu2022/FluentCeVIOWrapper)のサーバーをダウンロードして`./server/`以下に展開してく浅い
  - please download server from [FluentCeVIOWrapper](https://github.com/InuInu2022/FluentCeVIOWrapper) and decompress as `./server/`
  - 同じく、FluentCeVIOWrapperのnupkgをローカルインストールしてください
  - and install nupkg FluentCeVIOWrapper
- CeVIO Talk Editor (CeVIO CS7 or CeVIO AI)

## Limitaions

- CeVIO CSトークはテストしてません！
  - CeVIO Creative Studio Talk support (no test!)
- 現在、発音の開始タイミングがノートの開始タイミングに合う様になっています。母音のタイミングではないのでやや遅れて聞こえる場合があります。
  - Now the start timing of the pronunciation matches the start timing of the note. It is not vowel timing and may sound slightly delayed.
- stretchオプションは現在機能しません。台詞の長さはノートの長さに追従しません。
  - The stretch option is currently not functional. The length of dialogues does not follow the length of song track notes.