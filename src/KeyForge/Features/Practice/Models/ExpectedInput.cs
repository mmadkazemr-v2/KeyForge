namespace KeyForge.Features.Practice.Models;

/// <summary>
/// One normalized input expected by a generated challenge. ExpectedOffset
/// permits future timing evaluators without coupling this model to a rhythm engine.
/// </summary>
public sealed class ExpectedInput
{
    public required InputEventType Type { get; init; }
    public required string Value { get; init; }
    public TimeSpan? ExpectedOffset { get; init; }
}
