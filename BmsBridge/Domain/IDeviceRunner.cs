public interface IDeviceRunner
{
    Task RunLoopAsync(CancellationToken ct);
}
