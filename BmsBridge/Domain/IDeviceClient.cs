public interface IDeviceClient
{
    string DeviceIp { get; }
    BmsType DeviceType { get; }

    Task InitializeAsync(CancellationToken ct = default);

    Task PollAsync(CancellationToken ct = default);
}
