
using CommunityToolkit.Diagnostics;

namespace LibSasara.Internal;

internal sealed class DryWetMidiConnector
{
	internal static Model.Note Convert(
		Melanchall.DryWetMidi.Interaction.Note note
	)
	{
		Guard.IsNotNull(note, nameof(note));

		var (octave, step) = LibSasaraUtil
			.NoteNumToOctaveStep(note.NoteNumber);

		return new Model.Note()
		{
			Clock = (int)note.Time,
			PitchStep = step,
			PitchOctave = octave,
			Duration = (int)note.Length,
		};
	}
	internal static Melanchall.DryWetMidi.Interaction.Note
	Convert(
		Model.Note note
	)
	{
		Guard.IsNotNull(note, nameof(note));

		var noteNum = LibSasaraUtil
			.OctaveStepToNoteNum(
				note.PitchOctave,
				note.PitchStep
			);

		return new Melanchall.DryWetMidi.Interaction.Note(
			(Melanchall.DryWetMidi.Common.SevenBitNumber) noteNum
		)
		{
			Time = note.Clock,
			Length = note.Duration,
		};
	}
}