using System.Text.Json.Nodes;

public sealed class E2DeviceClient : BaseDeviceClient
{
    private readonly IE2IndexMappingProvider _indexProvider;

    private bool _initialized;

    public override BmsType DeviceType => BmsType.EmersonE2;

    public E2DeviceClient(
        Uri endpoint,
        IDeviceHttpExecutor pipelineExecutor,
        IE2IndexMappingProvider indexProvider,
        INormalizerService normalizer,
        ILoggerFactory loggerFactory,
        IIotDevice iotDevice
    ) : base(endpoint: endpoint,
            pipelineExecutor: pipelineExecutor,
            normalizer: normalizer,
            loggerFactory: loggerFactory,
            iotDevice: iotDevice
        )
    {
        _indexProvider = indexProvider;
    }

    public override async Task InitializeAsync(CancellationToken ct = default)
    {
        _logger.LogInformation($"Initializing E2 device client at {_endpoint}");

        await TestPrintControllerListAsync(ct);

        _initialized = true;
    }

    public override async Task PollAsync(CancellationToken ct = default)
    {
        EnsureInitialized();

        // polledData.Add(alarms);
        //
        // var diff = _dataWarehouse.ProcessIncoming(polledData);
        // await _iotDevice.SendMessageAsync(diff, ct);
    }

    // ------------------------------------------------------------
    // Initialization helpers
    // ------------------------------------------------------------
    public async Task TestPrintControllerListAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Testing E2 get controller list operation");
        var op = new E2GetControllerListOperation(_endpoint, _loggerFactory);
        var result = await op.ExecuteAsync(_pipelineExecutor, ct);

        if (!result.Success)
        {
            _logger.LogError($"Operation failed: {result.ErrorType}, {result.ErrorMessage}");
            return;
        }

        _logger.LogInformation("Raw JSON result:\n{Json}", result.Data?.ToJsonString());
    }
    // ------------------------------------------------------------
    // Polling helpers
    // ------------------------------------------------------------

    // ------------------------------------------------------------
    // Utility
    // ------------------------------------------------------------

    private void EnsureInitialized()
    {
        if (!_initialized)
            throw new InvalidOperationException("DeviceClient not initialized.");
    }
}
