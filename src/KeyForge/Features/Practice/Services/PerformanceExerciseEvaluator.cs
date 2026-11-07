namespace KeyForge.Features.Practice.Services;

/// <summary>Matches combined pitched attacks; voice/hand metadata never filters physical MIDI input.</summary>
public sealed class PerformanceExerciseEvaluator
{
    public ExerciseEvaluationResult Evaluate(ExerciseChallenge challenge, IReadOnlyList<ExerciseInputEvent> inputs)
    {
        ArgumentNullException.ThrowIfNull(challenge);
        ArgumentNullException.ThrowIfNull(inputs);
        var details = challenge.Performance ?? throw new InvalidOperationException("Missing performance timeline.");
        var expected = details.Events.Where(e => e.Pitch is not null).OrderBy(e => e.Onset).ThenBy(e => MusicalNote.Parse(e.Pitch!).MidiNumber).ToArray();
        var actual = inputs.Where(e => e.Type == InputEventType.Note).OrderBy(e => e.Offset).ThenBy(e => e.Sequence).ToList();
        var shift = details.TimingReference == PerformanceTimingReference.FirstAttack && actual.Count > 0
            ? actual[0].Offset - expected[0].Onset : TimeSpan.Zero;
        var pairing = PerformanceMidiPairing.Pair(inputs);
        var remaining = new HashSet<int>(Enumerable.Range(0, actual.Count));
        var notes = new List<PerformanceNoteEvaluation>();
        foreach (var target in expected)
        {
            var candidate = remaining.Where(i => MusicalNote.TryParse(actual[i].Value, out var note) &&
                    note.MidiNumber == MusicalNote.Parse(target.Pitch!).MidiNumber)
                .OrderBy(i => Math.Abs((actual[i].Offset - shift - target.Onset).TotalMilliseconds)).ThenBy(i => i)
                .Select(i => (int?)i).FirstOrDefault();
            var error = candidate is { } index ? (actual[index].Offset - shift - target.Onset).TotalMilliseconds : (double?)null;
            var matched = error is not null && Math.Abs(error.Value) <= details.OnsetToleranceMilliseconds;
            if (matched) remaining.Remove(candidate!.Value);
            var releaseAccuracy = PerformanceReleaseAccuracy.NotScored;
            double? durationError = null;
            if (details.ScoringMode == PerformanceScoringMode.KeyHold)
            {
                releaseAccuracy = PerformanceReleaseAccuracy.Missing;
                if (matched && pairing.Releases.TryGetValue(actual[candidate!.Value], out var releasedAt))
                {
                    durationError = (releasedAt - actual[candidate.Value].Offset - target.Duration).TotalMilliseconds;
                    releaseAccuracy = Math.Abs(durationError.Value) <= details.ReleaseToleranceMilliseconds
                        ? PerformanceReleaseAccuracy.Correct : durationError < 0 ? PerformanceReleaseAccuracy.Early : PerformanceReleaseAccuracy.Late;
                }
            }
            notes.Add(new PerformanceNoteEvaluation(target.Pitch!, target.Onset, matched ? actual[candidate!.Value].Offset - shift : null, error,
                matched && releaseAccuracy is PerformanceReleaseAccuracy.NotScored or PerformanceReleaseAccuracy.Correct)
            { Voice = target.Voice, Hand = target.Hand, DurationErrorMilliseconds = durationError, ReleaseAccuracy = releaseAccuracy });
        }
        var score = Math.Clamp((int)Math.Round(notes.Count(n => n.IsCorrect) * 100d / expected.Length) - remaining.Count * 5 - (details.ScoringMode == PerformanceScoringMode.KeyHold ? pairing.UnexpectedReleases * 5 : 0), 0, 100);
        var completeChords = notes.GroupBy(n => n.ExpectedOnset).Where(g => g.Count() > 1).All(g => g.All(n => n.IsCorrect));
        var successful = score >= 80 && completeChords;
        return new ExerciseEvaluationResult
        {
            Score = score,
            IsSuccessful = successful,
            Feedback = actual.Count == 0 ? ExerciseEvaluationFeedback.NoInput : successful ? ExerciseEvaluationFeedback.Correct : ExerciseEvaluationFeedback.Incorrect,
            Performance = new PerformanceEvaluationDetails { Notes = notes, UnexpectedReleases = pairing.UnexpectedReleases, ExtraNotes = remaining.Count, TimingReference = details.TimingReference }
        };
    }
}
