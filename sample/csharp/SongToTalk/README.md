# LibSasara sample "SongToTalk"

## What's this?

これは[LibSasara](https://github.com/InuInu2022/LibSasara)のサンプルコンソールアプリケーションです。
CeVIOのソングトラックのノートのデータを元に、トークトラックを生成し、ノートのタイミングに合わせてセリフを並べます。
**要は[ボイパロイド](https://dic.nicovideo.jp/a/%E3%83%9C%E3%82%A4%E3%83%91%E3%83%AD%E3%82%A4%E3%83%89)をCeVIOトークでやるためのツール**です。

[FluentCeVIOWrapper](https://github.com/InuInu2022/FluentCeVIOWrapper)と[World](http://www.isc.meiji.ac.jp/~mmorise/world/index.html) (DotnetWorld)との連携のサンプルにもなっています。

This is a sample console application for LibSasara.
It generates a talk track based on note data of a CeVIO song track and arranges the dialogue according to the timing of the notes.
**In short, it is a tool to do "voipaloid"** (_to make voice percussion sing in TTS software_) **with CeVIO talk.**

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

### EasySongToTalk.bat

[Release](https://github.com/InuInu2022/LibSasara/releases)から`SongToTalk-v*.*.*.zip`をDLして展開。
フォルダ内の `EasySongToTalk.bat`をダブルクリックで動きます。

Please download a `SongToTalk-v*.*.*.zip` from github [Release](https://github.com/InuInu2022/LibSasara/releases) and extract it.
Double-click `EasySongToTalk.bat` in the folder to run the program.

`EasySongToTalk.bat`をメモ帳とかで開いて中身の以下の部分を書き換えることでお好きなccs/ccstファイルとキャラでボイパが作れます。

Open `EasySongToTalk.bat` with Notepad or something and rewrite the following parts of the contents to create a "voipaloid" with your favorite ccs/ccst files and characters.

```bat
@rem 【書き換えてOK】ソングトラックのあるccs/ccstへのパス
@set SRC="./file/kaeru.ccs"
@rem 【書き換えてOK】上書きするccsへのパス
@set DIST="./file/dist.ccs"
@rem 【書き換えてOK】ボイパさせるキャラ名。持ってるキャラ名にしてね。
@set CAST="さとうささら"
@rem 【書き換えてOK】CeVIO_AI か CeVIO_CSか
@set TTS="CeVIO_AI"
```

### cmd

```cmd
SongToTalk.exe s path/to/sourcesong.ccst -d path/to/override.ccs --cast フィーちゃん -tts CeVIO_AI
```

詳しくは`--help`コマンドで。

For more information, use the --help command.

### Build with dotnet

```cmd
dotnet run -s path/to/sourcesong.ccst -d path/to/override.ccs --cast フィーちゃん
```

## FAQ

- Q. CeVIOソングエディタ持ってない
- Q. I don't have a CeVIO song editor.
- A. 無料のVoiSonaかUtaFormatixを使ってください
- A. Use the free VoiSona or UtaFormatix!

## Requirements

### Run

- .NET 7 Runtime
- .NET Framework 4.8.x Runtime
- CeVIO (CS7 or AI) Talk

### Build

- .NET SDK 7
- FluentCeVIOWrapper
  - [FluentCeVIOWrapper](https://github.com/InuInu2022/FluentCeVIOWrapper)のサーバーをダウンロードして`./server/`以下に展開してください
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