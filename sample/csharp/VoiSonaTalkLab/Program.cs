using LibSasara.Model;
using LibSasara.VoiSona;
using LibSasara.VoiSona.Model.Talk;

ConsoleApp.Run<Label>(args);

public class Label: ConsoleAppBase
{
	[RootCommand]
	public async ValueTask<int> ExportAsync(
		[Option("t", "path to a target tstprj file")]
		string pathToPrj,
		[Option("d", "path to export lab files")]
		string? destination = ""
	)
	{
		if(!Path.Exists(pathToPrj)){
			return 1;
		}
		if(!Directory.Exists(destination)){
			destination = Path.GetDirectoryName(pathToPrj);
			if (string.IsNullOrEmpty(destination)) return 1;
		}


		var tstprj = await LibVoiSona
			.LoadAsync<TstPrj>(pathToPrj);
		var labs = tstprj
			.GetAllTracks()
			.SelectMany(v => v.Utterances)
			.Where(v => v.Text != "")
			.Select(v => (v.Text, v.Label))
			;
		foreach(var lab in labs){
			var name = $"{destination}{Path.DirectorySeparatorChar}{lab.Text}.lab";
			string content = lab.Label!.ToString();
			await File
				.WriteAllTextAsync(name, content);
		}
		return 0;
	}
}