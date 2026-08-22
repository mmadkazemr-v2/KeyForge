namespace KeyForge.Components.Music;

/// <summary>Maps KeyForge music concepts to the small VexFlow interop contract.</summary>
public static class VexFlowNotationMapper
{
    public static bool TryMap(
        ClefType clef,
        MusicalNote? note,
        out VexFlowStaffRenderRequest? request)
    {
        request = null;
        if (note is null || note.IsBlackKey)
        {
            return false;
        }

        var clefName = clef switch
        {
            ClefType.Treble => "treble",
            ClefType.Bass => "bass",
            _ => null
        };

        if (clefName is null)
        {
            return false;
        }

        request = new VexFlowStaffRenderRequest
        {
            Clef = clefName,
            NoteKey = $"{note.Name.ToLowerInvariant()}/{note.Octave.ToString(CultureInfo.InvariantCulture)}"
        };
        return true;
    }
}
