# Sample: SongToTalkVoiSona

VoiSona Talkを”歌わせる”ツールです。

This is a tool for "Singing" [VoiSona Talk](https://voisona.com/talk/).

## できること

### STEP.1

歌唱指導を使わずに楽譜データから”歌わせ”ます。
機械的な歌声になります。
ノートに合わせた平坦なピッチを生成します。
※手で調声するベースとしても使えます。

#### 用意するもの

* CeVIOの楽譜ファイル(`.ccs` or `.ccst`)
  * [VoiSona(song)](https://voisona.com/) または [Utaformatix](https://sdercolin.github.io/utaformatix3/)でMIDIなどから変換してください

#### 制限

* 歌詞は日本語のみ
* 読み（音素）の差し替えは未対応
  * 逆に助詞の「は」は `[w,a]` と自動で発音します
* ノートのアーティキュレーション未対応

### [WIP]STEP.2

歌唱指導を使って歌わせます。
なめらかな歌声になります。

WIP

## DL

[github Release](https://github.com/InuInu2022/LibSasara/releases/tag/v0.2.3)からDL

* Windows: `SongToTalkVoiSona-win-x64-v.****.zip`
* macOS (intel): `SongToTalkVoiSona-osx-x64-v.****.zip`
* macOS (apple silicon): `SongToTalkVoiSona-osx-arm64-v.****.zip`
* Linux (x64): `SongToTalkVoiSona-linux-x64-v.****.zip`

## Usage

### `EasySongToTalkVoiSona.bat` (win only)

`EasySongToTalkVoiSona.bat`をメモ帳などで開いて編集したあとダブルクリックして起動してください。

```bat
@rem 【書き換えてOK】ソングトラックのあるccs/ccstへのパス
@set SRC="./file/kaeru.ccs"
@rem 【書き換えてOK】上書きするttsprjへのパス
@set DIST="./file/dist.ttsprj"
@rem 【書き換えてOK】歌わせるキャラ名。持ってるキャラ名にしてね。English name is OK!
@set CAST="田中傘"
@rem 【書き換えてOK】感情比率（キャストごとに要変更）
@set EMOTIONS="[1.0,0.0,0.0,0.0]"
```

### command (win/mac/linux)

```cmd
//win
SongToTalkVoiSona.exe -s path\to\ccs -e path\to\dist.ttsprj -c 田中傘 -emotions [1.0, 0.0, 0.0, 0.0, 0.0]

//mac,linux
SongToTalkVoiSona s path/to/ccs -e path/to/dist.ttsprj -c 田中傘 -emotions [1.0, 0.0, 0.0, 0.0, 0.0]
```

詳しくはhelpコマンドで確認してください。

```cmd
SongToTalkVoiSona.exe help
```

## Build

* .NET SDK 7以降
* [Open JTalk](https://open-jtalk.sourceforge.net/)の辞書が必要です。→[DL先](http://downloads.sourceforge.net/open-jtalk/open_jtalk_dic_utf_8-1.11.tar.gz)
  * `/lib/`フォルダに置いてください

実行

```cmd
dotnet run -s path/to/ccs -e path/to/dist.ttsprj -c タカハシ -emotions [1.0, 0.0, 0.0]
```

## Libraries

* LibSasara
* ConsoleAppFramework
* MinVer
* Newtonsoft.Json
* SharpOpenJTalk.Lang
* WanaKana-net
