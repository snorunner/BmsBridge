public sealed class DeviceRunnerRegistry
{
    private readonly List<IDeviceRunner> _runners = new();

    public void Add(IDeviceRunner runner) => _runners.Add(runner);

    public IReadOnlyList<IDeviceRunner> Runners => _runners;
}
