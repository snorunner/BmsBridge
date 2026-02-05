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

    public BaseDeviceRunner(
        Uri endpoint,
        IDeviceHttpExecutor pipelineExecutor,
        IE2IndexMappingProvider indexProvider,
        INormalizerService normalizerService,
        ILoggerFactory loggerFactory,
        IIotDevice iotDevice
            )
    {
        _endpoint = endpoint;
        _pipelineExecutor = pipelineExecutor;
        _indexProvider = indexProvider;
        _normalizer = normalizerService;
        _loggerFactory = loggerFactory;
        _iotDevice = iotDevice;
        _logger = loggerFactory.CreateLogger(GetType());
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

        await _bmsClient!.InitializeAsync(ct);

        while (!ct.IsCancellationRequested)
        {
            try
            {
                await _bmsClient.PollAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occured while polling at {_endpoint}");
            }

            // temporary to avoid flooding the console
            await Task.Delay(30_000, ct);
        }
    }

    protected abstract IDeviceClient CreateClient();
}
