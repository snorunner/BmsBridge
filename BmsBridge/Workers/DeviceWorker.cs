using Microsoft.Extensions.Options;

public sealed class DeviceWorker : BackgroundService
{
    private readonly ILogger<DeviceWorker> _logger;
    private readonly IDeviceRunnerFactory _deviceRunnerFactory;
    private readonly NetworkSettings _networkSettings;

    public DeviceWorker(IDeviceRunnerFactory deviceRunnerFactory, IOptions<NetworkSettings> networkSettings, ILogger<DeviceWorker> logger)
    {
        _deviceRunnerFactory = deviceRunnerFactory;
        _networkSettings = networkSettings.Value;
        _logger = logger;
    }

    private IEnumerable<IDeviceRunner> GetDeviceRunners()
    {
        List<IDeviceRunner> deviceRunners = new();

        _logger.LogInformation("Loading {Count} devices", _networkSettings.bms_devices.Count);
        foreach (var deviceConfig in _networkSettings.bms_devices)
        {
            deviceRunners.Add(_deviceRunnerFactory.Create(deviceConfig));
            _logger.LogInformation("Device: IP={IP}, Type={Type}", deviceConfig.IP, deviceConfig.DeviceType);
        }

        return deviceRunners;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

        var runners = GetDeviceRunners();

        var tasks = runners
            .Select(runner => runner.RunLoopAsync(stoppingToken))
            .ToList();

        _logger.LogInformation("DeviceWorker started {Count} device runners.", tasks.Count);

        try
        {
            await Task.WhenAll(tasks);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("DeviceWorker cancellation requested.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in DeviceWorker.");
        }
    }
}
