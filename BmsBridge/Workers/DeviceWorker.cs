using Microsoft.Extensions.Options;

public sealed class DeviceWorker : BackgroundService
{
    private readonly ILogger<DeviceWorker> _logger;
    private readonly IDeviceRunnerFactory _deviceRunnerFactory;
    private readonly NetworkSettings _networkSettings;
    private readonly GeneralSettings _generalSettings;
    private readonly IDeviceRunnerRegistry _deviceRunnerRegistry;

    public DeviceWorker(
        IDeviceRunnerFactory deviceRunnerFactory,
        IOptions<NetworkSettings> networkSettings,
        IOptions<GeneralSettings> generalSettings,
        ILogger<DeviceWorker> logger,
        IDeviceRunnerRegistry deviceRunnerRegistry)
    {
        _deviceRunnerFactory = deviceRunnerFactory;
        _networkSettings = networkSettings.Value;
        _generalSettings = generalSettings.Value;
        _logger = logger;
        _deviceRunnerRegistry = deviceRunnerRegistry;
    }

    private IEnumerable<IDeviceRunner> GetDeviceRunners()
    {
        List<IDeviceRunner> deviceRunners = new();

        _logger.LogInformation("Loading {Count} devices", _networkSettings.bms_devices.Count);
        foreach (var deviceConfig in _networkSettings.bms_devices)
        {
            var device = _deviceRunnerFactory.Create(deviceConfig);
            _deviceRunnerRegistry.RegisterDevice(device);
            deviceRunners.Add(device);
            _logger.LogInformation("Device: IP={IP}, Type={Type}", deviceConfig.IP, deviceConfig.device_type);
        }

        return deviceRunners;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var cycleCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            var cycleToken = cycleCts.Token;

            var restartAfter = TimeSpan.FromHours(_generalSettings.soft_reset_interval_hours);

            var timerTask = Task.Delay(restartAfter, stoppingToken)
                .ContinueWith(_ => cycleCts.Cancel());

            var runners = GetDeviceRunners();

            _logger.LogInformation("DeviceWorker is starting {Count} device runners.", runners.Count());

            var tasks = runners
                .Select(runner => runner.RunLoopAsync(cycleToken))
                .ToList();


            var completed = await Task.WhenAny(Task.WhenAll(tasks), timerTask);

            if (completed == timerTask)
            {
                cycleCts.Cancel();
                await Task.WhenAll(tasks);
                continue;
            }

            _logger.LogInformation($"Performing soft reset after {restartAfter} hours.");
        }
    }
}
