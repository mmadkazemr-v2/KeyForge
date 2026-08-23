namespace KeyForge.Components.Practice;

/// <summary>Routes browser MIDI input to the one currently active practice workspace.</summary>
public sealed class MidiInputRouter
{
    private readonly object _sync = new();
    private Func<MidiInputMessage, Task>? _handler;
    private long _registrationId;

    public IDisposable Register(Func<MidiInputMessage, Task> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        lock (_sync)
        {
            var registrationId = ++_registrationId;
            _handler = handler;
            return new Registration(this, registrationId);
        }
    }

    public Task DispatchAsync(MidiInputMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);
        Func<MidiInputMessage, Task>? handler;
        lock (_sync)
        {
            handler = _handler;
        }

        return handler?.Invoke(message) ?? Task.CompletedTask;
    }

    private void Unregister(long registrationId)
    {
        lock (_sync)
        {
            if (_registrationId == registrationId)
            {
                _handler = null;
            }
        }
    }

    private sealed class Registration(MidiInputRouter router, long registrationId) : IDisposable
    {
        public void Dispose() => router.Unregister(registrationId);
    }
}
