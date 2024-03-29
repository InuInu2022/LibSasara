# Sample: SongToTalkVoiSona

VoiSona Talkを”歌わせる”ツールです。

This is a tool for "Singing" [VoiSona Talk](https://voisona.com/talk/).

## Sample

- [【VoiSona Talk歌唱】いかないで / 想太 (offset ver.2)【すずきつづみcover】](https://utaloader.net/music/20231229200356536133)
- [【VoiSona Talk歌唱】いかないで / 想太 【タカハシcover】](https://utaloader.net/music/20231229204427612049)
- [「いかないで / 想太」 田中傘カバー](https://utaloader.net/music/20231202201954332325) (旧バージョン、すこしタイミングずれます)
- [「いかないで / 想太」 すずきつづみカバー](https://utaloader.net/music/20231202155012082509)(旧バージョン、すこしタイミングずれます)
- [「宇宙戦艦"ア"マト / ささきいさお」 タカハシアカペラカバー１フレーズ](https://youtu.be/lnJEOS__mTo)
- [「酔いどれ知らず / Kanaria」 田中傘アカペラカバー](https://youtu.be/LGDpAN4goIs)

## できること

### STEP.1

歌唱指導を使わずに楽譜データから”歌わせ”ます。
（この段階では）機械的な歌声になります。
ノートに合わせた平坦なピッチを生成します。
※手で調声するベースとしても使えます。

v0.2.6以降、子音をオフセットするようになりました。ノートの開始時間と母音のタイミングが合います。子音の種類ごとに変更、とかは、やっていません（初期値: `0.05`秒）。

#### 用意するもの

- CeVIOの楽譜ファイル(`.ccs` or `.ccst`)
  - [VoiSona(song)](https://voisona.com/) または [Utaformatix](https://sdercolin.github.io/utaformatix3/)でMIDIなどから変換してください

#### 制限

- 歌詞は日本語のみ
  - 歌詞はCeVIOソングなどと異なり、漢字かな交じりに対応しています
  - むしろ漢字かな交じりの方が発音が正確になります
- 読み（音素）の差し替えは未対応
  - 逆に助詞の「は」は `[w,a]` と自動で発音します
    - `[h,a]`と喋らせたい場合、歌詞をカタカナの「ハ」にかえるとうまくいきます
- ノートのアーティキュレーション未対応
- ブレス記号はセリフの分割に使われます
  - なのでノートくっつけても大丈夫 むしろ推奨
- 歌詞の記号は無視されます
  - `※`,`$`,`’`,`@`,`%`,`^`,`_`
- 子音オフセット値は固定値
  - 自分で調整する時の基準にしてください
  - 長過ぎる時間を指定すると破綻します！！！

#### コツ

- 「は」を[w,a]と読んでしまう時
  - 歌詞をカタカナの「ハ」にかえる
- 「きょう」を[k,y,o]ではなく[k,y,u]とよんでしまう時
  - 歌詞を「きょー」にかえる

### [WIP]STEP.2

歌唱指導を使って歌わせます。
なめらかな歌声になります。

WIP

## DL

[github Release](https://github.com/InuInu2022/LibSasara/releases/latest)からDL

- Windows: `SongToTalkVoiSona-win-x64-v.****.zip`
- macOS (intel): `SongToTalkVoiSona-osx-x64-v.****.zip`
- macOS (apple silicon): `SongToTalkVoiSona-osx-arm64-v.****.zip`
- Linux (x64): `SongToTalkVoiSona-linux-x64-v.****.zip`

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
@rem 【書き換えてOK】子音部分のオフセット秒。子音部分をノートの開始時間から前にオフセットします。
@set OFFSET="0.075"
```

### command (win/mac/linux)

```cmd
//win
SongToTalkVoiSona.exe -s path\to\ccs -e path\to\dist.ttsprj -c 田中傘 -emotions [1.0, 0.0, 0.0, 0.0, 0.0]

//mac,linux
SongToTalkVoiSona -s path/to/ccs -e path/to/dist.ttsprj -c 田中傘 -emotions [1.0, 0.0, 0.0, 0.0, 0.0]
```

- `-emotions`
  - 感情の比率を`[1.0, 0.0, 0.0, 0.0, 0.0]`みたいに指定します
  - `1.0`～`0.0`の範囲です
  - ボイスによって感情の数が違うので調整してください
- `--co 0.05`
  - 子音をノートの開始時間より指定秒前にオフセットします
  - 長すぎるとおかしくなると思います（特に早口の歌の場合）
- `--split (true|false)`
  - 長いノートを分割するか
  - トークボイスは長い発音に弱いため、分割して母音を加える処理を行います
- `--th 250`
  - 分割するときに何ミリ秒以上なら分割するか
  - 最小は`100`

詳しくはhelpコマンドで確認してください。

```cmd
//win
SongToTalkVoiSona.exe help

//mac linux
SongToTalkVoiSona help
```

## Build

- .NET SDK 8以降
- [Open JTalk](https://open-jtalk.sourceforge.net/)の辞書が必要です。→[DL先](http://downloads.sourceforge.net/open-jtalk/open_jtalk_dic_utf_8-1.11.tar.gz)
  - `/lib/`フォルダに置いてください

実行

```cmd
dotnet run -s path/to/ccs -e path/to/dist.ttsprj -c タカハシ -emotions [1.0, 0.0, 0.0]
```

## Libraries

- LibSasara
- ConsoleAppFramework
- MinVer
- Newtonsoft.Json
- SharpOpenJTalk.Lang
- WanaKana-net
- PolySharp
