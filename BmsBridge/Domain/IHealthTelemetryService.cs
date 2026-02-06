public interface IHealthTelemetryService
{
    public Task SendSnapshotAsync(IReadOnlyCollection<DeviceHealthSnapshot> snapshots, CancellationToken ct = default);
}
