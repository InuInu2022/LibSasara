using LibSasara;
using LibSasara.Model;

const string pathCcs = @".\file\test_ai8_project.ccs";
const string pathCcst = @".\file\test_ai8_track_ソング.ccst";

//ccs or ccst読み込み
//ICeVIOFileインターフェイス経由でいいならこれでOK
var cevioProj = await SasaraCcs.LoadAsync(pathCcs);
Console.WriteLine($"ver:{cevioProj.AutherVersion}");

//ccsを指定して読み込み
var ccs = await SasaraCcs.LoadAsync<CcsProject>(pathCcst);
Console.WriteLine($"ver:{ccs?.AutherVersion}");

//ccstを指定して読み込み
var ccst = await SasaraCcs.LoadAsync<CcstTrack>(pathCcst);
Console.WriteLine($"ver:{ccst?.AutherVersion}");