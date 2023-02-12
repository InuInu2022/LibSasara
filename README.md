# LibSasara

<p align="center">
	<img src="./documents/images/libsasara-logo.png" alt="logo" width="256" />
</p>

[![MIT License](http://img.shields.io/badge/license-MIT-blue.svg?style=flat)](LICENSE) [![C Sharp 10](https://img.shields.io/badge/C%20Sharp-10-4FC08D.svg?logo=csharp&style=flat)](https://learn.microsoft.com/ja-jp/dotnet/csharp/) ![.NET Standard 2.0](https://img.shields.io/badge/%20.NET%20Standard-2.0-blue.svg?logo=dotnet&style=flat)
![GitHub release (latest SemVer including pre-releases)](https://img.shields.io/github/v/release/inuinu2022/libsasara?include_prereleases&label=%F0%9F%9A%80release) ![GitHub all releases](https://img.shields.io/github/downloads/InuInu2022/LibSasara/total?color=green&label=%E2%AC%87%20downloads) ![GitHub Repo stars](https://img.shields.io/github/stars/InuInu2022/LibSasara?label=%E2%98%85&logo=github)
[![CeVIO CS](https://img.shields.io/badge/CeVIO_Creative_Studio-7.0-d08cbb.svg?logo=&style=flat)](https://cevio.jp/) [![CeVIO AI](https://img.shields.io/badge/CeVIO_AI-8.3-lightgray.svg?logo=&style=flat)](https://cevio.jp/) [![VoiSona](https://img.shields.io/badge/VoiSona-1.1-53abdb.svg?logo=&style=flat)](https://voisona.com/)

The utility library for **[CeVIO](https://cevio.jp/)** project file (`.ccs` / `.ccst`) and timing label file (`.lab`).

## What's this?

The .NET library for convenient handling of project files (`.ccs`), track files (`.ccst`), and timing label files (`.lab`) of the speech synthesis software [CeVIO](https://cevio.jp/).

## Features

- The utility .NET class library for CeVIO files
- Written in C# 10
- [.NET Standard 2.0](https://learn.microsoft.com/en-US/dotnet/standard/net-standard?tabs=net-standard-2-0#tabpanel_1_net-standard-2-0)
- Supported file formats:
  - CeVIO project file (`.ccs`)
  - CeVIO track file (`.ccst`)
  - timing label file (`.lab`)

## Supported softwares

- CeVIO Creative Studio
  - import / export ccs
  - import / export ccst
  - export lab
- CeVIO AI
  - import / export ccs
  - import / export ccst
  - export lab
- VoiSona
  - import / export ccs
    - `HUS`: supported (>= 0.1.0)
    - `TUNE`: not supported yet
  - import / export ccst
  - export lab

## How to use

### .NET

1. DL `.nupkg` package file from Releases
2. Add and Install this package from your local NuGet repository
3. `using LibSasara;`

## API Documents

https://InuInu2022.github.io/LibSasara/

## Show cases

### [NodoAme](https://inuinu2022.github.io/NodoAme.Home/)

**NodoAme** is a tool to make a song software (vocal synthesizer) TALK by imitating talk voices of a talk software (TTS). This is compatible with CeVIO songs and VoiSona. It is a tool for so-called "Talkloid".

LibSasara was developed by porting the know-how of NodoAme.
Since NodoAme ver. 0.4, LibSasara has been used directly.


## Projects

- [LibSasara](./LibSasara/)
  - .NET Starndard 2.0 class lib.
- [test](./test/)
  - xunit tests
- [sample](./sample/)
  - sample projects
  - [csharp](./sample/csharp/)
    - C# samples
    - [SampleConsole](./sample/csharp/SampleConsole/)
      - A sample console app.
    - [SongToTalk](./sample/csharp/SongToTalk/)
      - A converter from song notes to talk serifs.
      - This is also a sample that works with the library [Fluent CeVIO Wrapper](https://github.com/InuInu2022/FluentCeVIOWrapper).
  - [wasm](./sample/wasm/)
    - future plan: Web Assembly (wasm) sample
- [documents](./documents/)
  - a document project with docfx.

## Todo/Wants

- [x] キャスト置き換え / Replace a cast of track
  - エディタ上で差し替えると一部調声データが飛ぶ
  - エディタ上で変えると合成処理が掛かって重い
- [ ] 調声データの展開・圧縮 / Expand or Compress xml tune data
  - ccs/ccst上で省略されている調声データ`<Data>`を展開・元に圧縮する
- [ ] 調声データのマージ / Merge tune data
- [ ] 外部.labファイルの反映 / Set timing data from external .lab file
  - .labファイルを元にTMGパラメータを反映する（ソング）
  - A：上書き生成、B:既にあるデータとマージ
- [x] 相対時間と絶対時間の相互変換 / Mutual converting of relative time and absolute time
  - ソングのtickは相対時間
- [ ] [FluentCeVIOWrapper](https://github.com/InuInu2022/FluentCeVIOWrapper)との連携 / [FluentCeVIOWrapper](https://github.com/InuInu2022/FluentCeVIOWrapper) integration extension
  - `LibSasara.Extensions.FluentCeVIO`
- [ ] YMM4との連携 / YMM4 integration extension
  - `LibSasara.Extensions.YMM4`
- [ ] [OpenTimelineIO](https://github.com/AcademySoftwareFoundation/OpenTimelineIO) (OTIO) との連携 / [OpenTimelineIO](https://github.com/AcademySoftwareFoundation/OpenTimelineIO) integration extension
  - `LibSasara.Extensions.OTIO`

## Libraries

- LibSasara
  - [System.Threading.Tasks.Extensions](https://www.nuget.org/packages/System.Threading.Tasks.Extensions/)
  - [System.Memory](https://www.nuget.org/packages/System.Memory)
  - [MinVer](https://github.com/adamralph/minver)
- test
  - [xunit](https://github.com/xunit/xunit)
  - [coverlet.collector](https://github.com/coverlet-coverage/coverlet)
- sample/csharp/SongToTalk
  - [DotnetWorld](https://github.com/yamachu/DotnetWorld)
  - [MathNet.Numerics](https://numerics.mathdotnet.com/)
  - [NAudio](https://github.com/naudio/NAudio)
  - [System.Linq.Async](https://github.com/dotnet/reactive)
  - [ConsoleAppFramework](https://github.com/Cysharp/ConsoleAppFramework)
  - [Fluent CeVIO Wrapper](https://github.com/InuInu2022/FluentCeVIOWrapper)
    - [H.Pipes](https://github.com/HavenDV/H.Pipes)
    - [ConsoleAppFramework](https://github.com/Cysharp/ConsoleAppFramework)
    - [System.Reactive](https://github.com/dotnet/reactive)
- documents
  - [docfx](https://dotnet.github.io/docfx/)

## LibSasara license

> The MIT License
>
> Copyright (c) 2022 - 2023 InuInu

- [LICENSE.txt](LICENSE.txt)

## 🐶Developed by InuInu

- InuInu（いぬいぬ）
  - YouTube [YouTube](https://bit.ly/InuInuMusic)
  - Twitter [@InuInuGames](https://twitter.com/InuInuGames)
  - Blog [note.com](https://note.com/inuinu_)
  - niconico [niconico](https://nico.ms/user/98013232)
