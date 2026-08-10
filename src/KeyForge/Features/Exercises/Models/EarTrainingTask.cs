namespace KeyForge.Features.Exercises.Models;

/// <summary>
/// The specific task an ear-training exercise focuses on.
/// </summary>
public enum EarTrainingTask
{
    /// <summary>Matching a sung/played pitch back by ear.</summary>
    PitchMatching,

    /// <summary>Identifying chords (major, minor, diminished, ...) by ear.</summary>
    ChordRecognition,

    /// <summary>Identifying scales and modes by ear.</summary>
    ScaleRecognition,

    /// <summary>Transcribing a short melody by ear.</summary>
    MelodyDictation
}
