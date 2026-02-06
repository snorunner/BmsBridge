public sealed class HealthMonitorWorker : BackgroundService
{
    private readonly ILogger<DeviceWorker> _logger;
    private readonly IIotDevice _iotDevice;
    private readonly IDeviceHealthRegistry _deviceHealthRegistry;
    private readonly ICircuitBreakerService _circuitBreaker;
    private readonly IHealthTelemetryService _healthTelemetryService;

    public HealthMonitorWorker(
        IDeviceHealthRegistry deviceHealthRegistry,
        IIotDevice iotDevice,
        ILogger<DeviceWorker> logger,
        ICircuitBreakerService circuitBreaker,
        IHealthTelemetryService healthTelemetryService)
    {
        _deviceHealthRegistry = deviceHealthRegistry;
        _iotDevice = iotDevice;
        _logger = logger;
        _circuitBreaker = circuitBreaker;
        _healthTelemetryService = healthTelemetryService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(10_000);

            var snapshots = _deviceHealthRegistry.GetAllSnapshots();

            foreach (var snapshot in snapshots)
            {
                _circuitBreaker.EvaluateAndUpdate(snapshot);
                _logger.LogInformation("Device health: {@Snap}", snapshot);
            }

            await _healthTelemetryService.SendSnapshotAsync(snapshots, stoppingToken);

        }
    }
}
