namespace KeyForge.Components.Music;

/// <summary>
/// Internal boundary model passed from the KeyForge notation component to its
/// JavaScript module. No VexFlow object crosses this boundary.
/// </summary>
public sealed class VexFlowStaffRenderRequest
{
    public required string Clef { get; init; }
    public required string NoteKey { get; init; }
}
