namespace KeyForge.Features.Exercises.Models;

/// <summary>
/// Identifies the kind of exercise a definition represents.
/// <para>
/// This value is the polymorphic discriminator stored in the YAML
/// (<c>type: rhythm</c>, <c>type: speed</c>, ...) that decides which concrete
/// <see cref="ExerciseDefinition"/> subclass an exercise entry is deserialized into.
/// </para>
/// </summary>
public enum ExerciseType
{
    /// <summary>Practises timing, subdivisions and rhythm-pattern reading.</summary>
    Rhythm,

    /// <summary>Practises reading notes from the staff and finding them on the keyboard.</summary>
    NoteReading,

    /// <summary>Practises identifying musical elements by ear (chords, scales, melodies, ...).</summary>
    EarTraining,

    /// <summary>Practises recognising the distance between two notes.</summary>
    Interval,

    /// <summary>Practises recognising and/or playing octaves.</summary>
    Octave,

    /// <summary>Practises playing individual fingers independently.</summary>
    FingerIndependence,

    /// <summary>Practises visualising the keyboard mentally without looking.</summary>
    MentalKeyboard,

    /// <summary>Practises playing patterns at increasingly high tempos.</summary>
    Speed
}
