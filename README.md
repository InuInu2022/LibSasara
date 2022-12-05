# LibSasara

[![MIT License](http://img.shields.io/badge/license-MIT-blue.svg?style=flat)](LICENSE) [![C Sharp 10](https://img.shields.io/badge/C%20Sharp-10-4FC08D.svg?logo=csharp&style=flat)](https://learn.microsoft.com/ja-jp/dotnet/csharp/) ![.NET Standard 2.0](https://img.shields.io/badge/%20.NET%20Standard-2.0-blue.svg?logo=dotnet&style=flat)

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
  - import / export ccst
  - export lab

## How to use

### .NET

1. DL `.nupkg` package file from Releases
2. Add and Install this package from your local NuGet repository
3. `using LibSasara;`

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

## KuchiPaku license

> The MIT License
>
> Copyright (c) 2022 InuInu

- [LICENSE.txt](LICENSE.txt)

## 🐶Developed by InuInu

- InuInu（いぬいぬ）
  - YouTube [YouTube](https://bit.ly/InuInuMusic)
  - Twitter [@InuInuGames](https://twitter.com/InuInuGames)
  - Blog [note.com](https://note.com/inuinu_)
  - niconico [niconico](https://nico.ms/user/98013232)
