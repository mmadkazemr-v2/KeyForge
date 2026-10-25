namespace KeyForge.Components.Practice;

/// <summary>Routes browser MIDI input to the one currently active practice workspace.</summary>
public sealed class MidiInputRouter
{
    private readonly object _sync = new();
    private Func<MidiInputMessage, Task>? _handler;
    private long _registrationId;
    private Func<ValueTask<double>>? _browserClock;
    private long _clockRegistrationId;
    private bool _deviceConnected;
    private string? _selectedDeviceId;
    private double? _timelineOriginMilliseconds;

    public event Action<string?, bool>? SelectedDeviceStateChanged;

    public double? TimelineOriginMilliseconds
    {
        get { lock (_sync) return _timelineOriginMilliseconds; }
    }

    public bool Accepts(MidiInputMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);
        lock (_sync)
        {
            return _deviceConnected && string.Equals(_selectedDeviceId, message.DeviceId, StringComparison.Ordinal);
        }
    }

    public IDisposable RegisterBrowserClock(Func<ValueTask<double>> browserClock)
    {
        ArgumentNullException.ThrowIfNull(browserClock);
        lock (_sync)
        {
            var registrationId = ++_clockRegistrationId;
            _browserClock = browserClock;
            return new ClockRegistration(this, registrationId);
        }
    }

    public async ValueTask<double?> CaptureBrowserTimestampAsync()
    {
        Func<ValueTask<double>>? browserClock;
        lock (_sync) browserClock = _browserClock;
        if (browserClock is null) return null;
        var timestamp = await browserClock();
        return double.IsFinite(timestamp) && timestamp >= 0 ? timestamp : null;
    }

    public void SetTimelineOrigin(double? browserTimestampMilliseconds)
    {
        if (browserTimestampMilliseconds is { } timestamp &&
            (!double.IsFinite(timestamp) || timestamp < 0))
        {
            throw new ArgumentOutOfRangeException(nameof(browserTimestampMilliseconds));
        }
        lock (_sync) _timelineOriginMilliseconds = browserTimestampMilliseconds;
    }

    public void SetSelectedDevice(string? deviceId, bool connected)
    {
        Action<string?, bool>? stateChanged = null;
        lock (_sync)
        {
            var isConnected = connected && deviceId is not null;
            if (!string.Equals(_selectedDeviceId, deviceId, StringComparison.Ordinal) || _deviceConnected != isConnected)
            {
                stateChanged = SelectedDeviceStateChanged;
            }
            _selectedDeviceId = deviceId;
            _deviceConnected = isConnected;
        }
        stateChanged?.Invoke(deviceId, connected && deviceId is not null);
    }

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
            if (!_deviceConnected ||
                (_selectedDeviceId is not null && !string.Equals(_selectedDeviceId, message.DeviceId, StringComparison.Ordinal)))
            {
                handler = null;
            }
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

    private void UnregisterBrowserClock(long registrationId)
    {
        lock (_sync)
        {
            if (_clockRegistrationId == registrationId)
            {
                _browserClock = null;
                _timelineOriginMilliseconds = null;
            }
        }
    }

    private sealed class Registration(MidiInputRouter router, long registrationId) : IDisposable
    {
        public void Dispose() => router.Unregister(registrationId);
    }

    private sealed class ClockRegistration(MidiInputRouter router, long registrationId) : IDisposable
    {
        public void Dispose() => router.UnregisterBrowserClock(registrationId);
    }
}
