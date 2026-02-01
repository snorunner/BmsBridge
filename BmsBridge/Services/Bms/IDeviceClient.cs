public interface IDeviceClient
{
    string DeviceIp { get; }
    string DeviceType { get; }

    Task InitializeAsync(CancellationToken ct = default);

    Task PollAsync(CancellationToken ct = default);
}
