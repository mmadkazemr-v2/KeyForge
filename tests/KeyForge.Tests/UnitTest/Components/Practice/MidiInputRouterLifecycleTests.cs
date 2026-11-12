namespace KeyForge.Tests.UnitTest.Components.Practice;

using KeyForge.Components.Practice;

public sealed class MidiInputRouterLifecycleTests
{
    [Fact]
    public async Task DeviceSelectionChangeRejectsQueuedOldDeviceEvents()
    {
        var router = new MidiInputRouter();
        var received = new List<string>();
        using var registration = router.Register(message =>
        {
            received.Add(message.DeviceId);
            return Task.CompletedTask;
        });

        router.SetSelectedDevice("old-device", connected: true);
        router.SetSelectedDevice("new-device", connected: true);

        await router.DispatchAsync(CreateMessage("old-device"));
        await router.DispatchAsync(CreateMessage("new-device"));

        Assert.Equal(["new-device"], received);
    }

    [Fact]
    public async Task DisconnectAndReconnectRequiresEventsFromCurrentSelectedDevice()
    {
        var router = new MidiInputRouter();
        var received = new List<string>();
        using var registration = router.Register(message =>
        {
            received.Add(message.DeviceId);
            return Task.CompletedTask;
        });

        router.SetSelectedDevice("keys", connected: true);
        router.SetSelectedDevice("keys", connected: false);
        await router.DispatchAsync(CreateMessage("keys"));

        router.SetSelectedDevice("keys", connected: true);
        await router.DispatchAsync(CreateMessage("keys"));

        Assert.Single(received);
    }

    [Fact]
    public async Task DisposedRegistrationStopsFurtherEventDelivery()
    {
        var router = new MidiInputRouter();
        var count = 0;
        var registration = router.Register(_ =>
        {
            count++;
            return Task.CompletedTask;
        });
        router.SetSelectedDevice("keys", connected: true);

        await router.DispatchAsync(CreateMessage("keys"));
        registration.Dispose();
        await router.DispatchAsync(CreateMessage("keys"));

        Assert.Equal(1, count);
    }

    private static MidiInputMessage CreateMessage(string deviceId) => new()
    {
        DeviceId = deviceId,
        Status = 0x90,
        Data1 = 60,
        Data2 = 80,
        TimestampMilliseconds = 1000
    };
}
