public sealed class HealthMonitorWorker : BackgroundService
{
    private readonly ILogger<DeviceWorker> _logger;
    private readonly IDeviceHealthRegistry _deviceHealthRegistry;

    public HealthMonitorWorker(IDeviceHealthRegistry deviceHealthRegistry, ILogger<DeviceWorker> logger)
    {
        _deviceHealthRegistry = deviceHealthRegistry;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var snapshots = _deviceHealthRegistry.GetAllSnapshots();

            foreach (var snapshot in snapshots)
            {
                Console.WriteLine(snapshot.LastErrorType);
            }

            await Task.Delay(10_000, stoppingToken);
        }
    }
}
