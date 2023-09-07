using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using LibSasara.Model.Serialize;
using Xunit;
using Xunit.Abstractions;

namespace test;

public class SerializeTest
{
	private readonly ITestOutputHelper _output;

	public SerializeTest(ITestOutputHelper output)
			=> _output = output;

	[Fact]
	public async Task RootOfProjectAsync()
	{
		// Given
		const string ccs = "<?xml version=\"1.0\" encoding=\"utf-8\"?><Scenario Code=\"7251BC4B6168E7B2992FA620BD3E1E77\"></Scenario>";

		// When
		var result = await DeserializeAsync<Project>(ccs);

		// Then
		Assert.NotNull(result);
	}

	[Fact]
	public async Task GeneratorAsync()
	{
		// Given
		const string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
		<TTS Version=""6.0.20"">
			<Dictionary Version=""3.0.1"">
				<Extension Version=""2.0.2"" Language=""English"" />
			</Dictionary>
			<SoundSources>
				<SoundSource Version=""1.0.0"" Id=""CTNV-JPF-A"" Name=""さとうささら"" />
				<SoundSource Version=""1.1.1"" Id=""CTNV-JPF-AHS2"" Name=""小春六花"" />
				<SoundSource Version=""1.0.0"" Id=""CTNV-ENF-AHS3"" Name=""弦巻マキ (英)"" />
				<SoundSource Version=""1.0.1"" Id=""CTNV-JPF-B"" Name=""すずきつづみ"" />
			</SoundSources>
		</TTS>";

		// When
		var result = await DeserializeAsync<TalkEngine>(xml);

		// Then
		Assert.NotNull(result);
	}

	private static async ValueTask<T?> DeserializeAsync<T>(string xml)
	{
		//var namespaces = new XmlSerializerNamespaces();
		//namespaces.Add(string.Empty, string.Empty);
		var slzr = new XmlSerializer(typeof(T), string.Empty);
		if (slzr is null)
		{
			return default;
		}

		return await Task.Run(() => (T?)slzr.Deserialize(new StringReader(xml)));
	}
}
