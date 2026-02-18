using System.Collections.Concurrent;

public sealed class HealthMonitorWorker : BackgroundService
{
    private readonly ILogger<DeviceWorker> _logger;
    private readonly IIotDevice _iotDevice;
    private readonly IDeviceHealthRegistry _deviceHealthRegistry;
    private readonly ICircuitBreakerService _circuitBreaker;
    private readonly IHealthTelemetryService _healthTelemetryService;
    private readonly IRunnerControlService _runnerControlService;
    private readonly IDeviceRunnerRegistry _deviceRunnerRegistry;
    private readonly ConcurrentDictionary<string, DeviceHealthSnapshot> _lastLogged = new();

    public HealthMonitorWorker(
        IDeviceHealthRegistry deviceHealthRegistry,
        IIotDevice iotDevice,
        ILogger<DeviceWorker> logger,
        ICircuitBreakerService circuitBreaker,
        IHealthTelemetryService healthTelemetryService,
        IRunnerControlService runnerControlService,
        IDeviceRunnerRegistry deviceRunnerRegistry)
    {
        _deviceHealthRegistry = deviceHealthRegistry;
        _iotDevice = iotDevice;
        _logger = logger;
        _circuitBreaker = circuitBreaker;
        _healthTelemetryService = healthTelemetryService;
        _runnerControlService = runnerControlService;
        _deviceRunnerRegistry = deviceRunnerRegistry;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(10_000, stoppingToken);

            var snapshots = _deviceHealthRegistry.GetAllSnapshots();

            foreach (var snapshot in snapshots)
            {
                _circuitBreaker.EvaluateAndUpdate(snapshot);
                _runnerControlService.ApplyControl(snapshot);

                if (ShouldLog(snapshot))
                    _logger.LogDebug("Device health: {@Snap}", snapshot);
            }

            await _healthTelemetryService.SendSnapshotAsync(snapshots, stoppingToken);
        }
    }

    private bool ShouldLog(DeviceHealthSnapshot snapshot)
    {
        if (!_lastLogged.TryGetValue(snapshot.DeviceIp, out var last))
        {
            _lastLogged[snapshot.DeviceIp] = snapshot;
            return true;
        }

        // Only log when something meaningful changes
        if (snapshot.CircuitState != last.CircuitState ||
            snapshot.LastErrorType != last.LastErrorType ||
            snapshot.ConsecutiveFailures != last.ConsecutiveFailures)
        {
            _lastLogged[snapshot.DeviceIp] = snapshot;
            return true;
        }

        return false;
    }
}
