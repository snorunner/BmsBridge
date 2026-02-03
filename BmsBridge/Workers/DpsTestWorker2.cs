public sealed class DpsTestWorker2 : BackgroundService
{
    private readonly E2DeviceClient _deviceClient;
    private readonly ILogger<DpsTestWorker2> _logger;

    public DpsTestWorker2(
            IIotDevice iotDevice,
            ILogger<DpsTestWorker2> logger,
            IE2IndexMappingProvider indexMappingProvider,
            // IHttpPipelineExecutor pipelineExecutor,
            INormalizerService normalizer,
            ILoggerFactory loggerFactory)
    {
        _logger = logger;

        var endpoint = new Uri("http://10.128.223.180:14106/JSON-RPC");

        _deviceClient = new E2DeviceClient(
            endpoint,
            new ReplayHttpPipelineExecutor(new GeneralSettings()),
            indexMappingProvider,
            normalizer,
            loggerFactory,
            iotDevice
        );
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("IoT Test Worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await _deviceClient.InitializeAsync();
                _logger.LogInformation("Test E2 initialized successfully.");

                await _deviceClient.PollAsync();
                _logger.LogInformation("Test E2 polled successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send test message to IoT Hub.");
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}
