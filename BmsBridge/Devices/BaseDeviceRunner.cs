public abstract class BaseDeviceRunner : IDeviceRunner
{
    protected readonly Uri _endpoint;
    protected readonly IDeviceHttpExecutor _pipelineExecutor;
    protected readonly IE2IndexMappingProvider _indexProvider;
    protected readonly INormalizerService _normalizer;
    protected readonly ILoggerFactory _loggerFactory;
    protected readonly IIotDevice _iotDevice;
    protected readonly ILogger _logger;
    protected IDeviceClient? _bmsClient;
    private readonly object _stateLock = new();
    private RunnerState _state = RunnerState.Running;
    private CancellationTokenSource? _executionCts;
    private readonly GeneralSettings _generalSettings;

    public string DeviceIp => _endpoint.Host;

    public BaseDeviceRunner(
        Uri endpoint,
        IDeviceHttpExecutor pipelineExecutor,
        IE2IndexMappingProvider indexProvider,
        INormalizerService normalizerService,
        ILoggerFactory loggerFactory,
        IIotDevice iotDevice,
        GeneralSettings generalSettings
            )
    {
        _endpoint = endpoint;
        _pipelineExecutor = pipelineExecutor;
        _indexProvider = indexProvider;
        _normalizer = normalizerService;
        _loggerFactory = loggerFactory;
        _iotDevice = iotDevice;
        _logger = loggerFactory.CreateLogger(GetType());
        _generalSettings = generalSettings;
    }

    protected void EnsureClient()
    {
        if (_bmsClient is not null)
            return;

        _bmsClient = CreateClient();
    }

    public virtual async Task RunLoopAsync(CancellationToken ct)
    {
        EnsureClient();
        try
        {
            await _bmsClient!.InitializeAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Failed to initialize device. Waiting 5 minutes before retrying.");
            await Task.Delay(300_000, ct);
        }

        while (!ct.IsCancellationRequested)
        {
            CancellationToken executionCt;

            lock (_stateLock)
            {
                _executionCts?.Cancel();
                _executionCts?.Dispose();
                _executionCts = null;

                if (_state == RunnerState.Paused)
                {
                    executionCt = CancellationToken.None;
                }
                else
                {
                    _executionCts =
                        CancellationTokenSource.CreateLinkedTokenSource(ct);

                    if (_state == RunnerState.ProbeOnly)
                    {
                        _executionCts.CancelAfter(TimeSpan.FromSeconds(30));
                    }

                    executionCt = _executionCts.Token;
                }
            }

            if (executionCt == CancellationToken.None)
            {
                await Task.Delay(5_000, ct);
                continue;
            }

            try
            {
                _logger.LogInformation($"Device {DeviceIp} started polling.");
                var sw = System.Diagnostics.Stopwatch.StartNew();
                await _bmsClient!.PollAsync(executionCt);
                sw.Stop();
                _logger.LogInformation($"Device {DeviceIp} finished polling. Took {sw.Elapsed.TotalSeconds:F2} seconds");
                _logger.LogInformation($"Device {DeviceIp} is cooling down for {_generalSettings.loop_delay_seconds} seconds.");
                await Task.Delay(TimeSpan.FromSeconds(_generalSettings.loop_delay_seconds), ct);
                // await Task.Delay(_generalSettings.loop_delay_seconds * 1000, ct);
            }
            catch (OperationCanceledException)
            {
                lock (_stateLock)
                {
                    if (_state == RunnerState.ProbeOnly)
                    {
                        _state = RunnerState.Paused;
                        _logger.LogInformation(
                            "Probe window elapsed for {Ip}. Waiting for health controller.",
                            DeviceIp);
                    }
                    else
                    {
                        _logger.LogWarning(
                            "Execution cancelled for device {Ip}.",
                            DeviceIp);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error while polling device {Ip}.",
                    DeviceIp);
                _logger.LogWarning($"Device {DeviceIp} is cooling down for 5 minutes after an unexpected error.");
                await Task.Delay(300_000, ct);
            }

        }
    }

    protected abstract IDeviceClient CreateClient();

    public void Pause()
    {
        lock (_stateLock)
        {
            if (_state != RunnerState.Paused)
            {
                _logger.LogWarning("Pausing runner for {Ip}", DeviceIp);
                _state = RunnerState.Paused;
            }
        }
    }

    public void Resume()
    {
        lock (_stateLock)
        {
            if (_state != RunnerState.Running)
            {
                _logger.LogInformation("Resuming runner for {Ip}", DeviceIp);
                _state = RunnerState.Running;
            }
        }
    }

    public void AllowProbe()
    {
        lock (_stateLock)
        {
            _logger.LogInformation("Allowing probe for {Ip}", DeviceIp);
            _state = RunnerState.ProbeOnly;
        }
    }
}
