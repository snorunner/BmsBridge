public sealed class HealthMonitorWorker : BackgroundService
{
    private readonly ILogger<DeviceWorker> _logger;
    private readonly IIotDevice _iotDevice;
    private readonly IDeviceHealthRegistry _deviceHealthRegistry;

    public HealthMonitorWorker(IDeviceHealthRegistry deviceHealthRegistry, IIotDevice iotDevice, ILogger<DeviceWorker> logger)
    {
        _deviceHealthRegistry = deviceHealthRegistry;
        _iotDevice = iotDevice;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var snapshots = _deviceHealthRegistry.GetAllSnapshots();

            foreach (var snapshot in snapshots)
            {
                _logger.LogInformation("Device health: {@Snap}", snapshot);
            }

            await Task.Delay(10_000, stoppingToken);
        }
    }
}
