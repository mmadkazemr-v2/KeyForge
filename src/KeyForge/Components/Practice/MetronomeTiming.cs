namespace KeyForge.Components.Practice;

/// <summary>Small UI-side timing helpers shared by the metronome controls.</summary>
public static class MetronomeTiming
{
    public static TimeSpan GetBeatInterval(int bpm)
    {
        if (bpm <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(bpm), "BPM must be greater than zero.");
        }

        return TimeSpan.FromMilliseconds(60000d / bpm);
    }

    public static IReadOnlyList<int> GetCountInBeats(int countInBeats)
    {
        if (countInBeats < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(countInBeats));
        }

        return Enumerable.Range(1, countInBeats).ToArray();
    }
}
