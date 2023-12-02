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

### STEP.2

歌唱指導を使って歌わせます。
なめらかな歌声になります。

WIP

## DL

WIP

## Usage

```cmd
//windows
SongToTalkVoiSona.exe -s path/to/ccs -e path/to/dist.ttsprj -c 田中傘

//mac, linux
```

## Build

* .NET SDK 7以降
* [Open JTalk](https://open-jtalk.sourceforge.net/)の辞書が必要です。→[DL先](http://downloads.sourceforge.net/open-jtalk/open_jtalk_dic_utf_8-1.11.tar.gz)
  * `/lib/`フォルダに置いてください

実行

```cmd
dotnet run -s path/to/ccs -e path/to/dist.ttsprj -c タカハシ -emotions [1.0, 0.0, 0.0]
```
