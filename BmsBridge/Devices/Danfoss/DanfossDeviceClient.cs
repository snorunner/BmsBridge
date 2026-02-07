using System.Text.Json.Nodes;

public sealed class DanfossDeviceClient : BaseDeviceClient
{
    private bool _initialized;

    public override BmsType DeviceType => BmsType.Danfoss;

    // Data objects
    private JsonObject? _unitsData;
    private JsonObject? _parmVersions;

    public DanfossDeviceClient(
        Uri endpoint,
        IDeviceHttpExecutor pipelineExecutor,
        INormalizerService normalizer,
        ILoggerFactory loggerFactory,
        IIotDevice iotDevice
    ) : base(endpoint: endpoint,
            pipelineExecutor: pipelineExecutor,
            normalizer: normalizer,
            loggerFactory: loggerFactory,
            iotDevice: iotDevice
        )
    { }

    public override async Task InitializeAsync(CancellationToken ct = default)
    {
        _logger.LogInformation($"Initializing Danfoss device client at {_endpoint}");

        _initialized = true;
        // await TestPrintAsync(ct);

        // Only poll once per restart:
        _unitsData = await ReadUnitsAsync(ct);
        _parmVersions = await ReadParmVersionsAsync(ct);
    }

    public override async Task PollAsync(CancellationToken ct = default)
    {
        await EnsureInitialized();

        var polledData = new JsonArray();

        polledData.Add(_unitsData);
        polledData.Add(_parmVersions);

        var diff = _dataWarehouse.ProcessIncoming(polledData);
        await _iotDevice.SendMessageAsync(diff, ct);
    }

    // ------------------------------------------------------------
    // Initialization helpers
    // ------------------------------------------------------------

    private async Task TestPrintAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Testing E2 get cell list operation");
        var op = new DanfossReadUnitsOperation(_endpoint, _loggerFactory);
        var result = await op.ExecuteAsync(_pipelineExecutor, ct);

        if (!result.Success)
        {
            _logger.LogError($"Operation failed: {result.ErrorType}, {result.ErrorMessage}");
            _initialized = false;
            return;
        }

        _logger.LogInformation("Raw JSON result:\n{Json}", result.Data?.ToJsonString());
    }

    // ------------------------------------------------------------
    // Oneshot helpers
    // ------------------------------------------------------------

    private async Task<JsonObject> ReadUnitsAsync(CancellationToken ct = default)
    {
        var op = new DanfossReadUnitsOperation(_endpoint, _loggerFactory);
        var result = await op.ExecuteAsync(_pipelineExecutor, ct);
        return ControllerLevelParse(result);
    }

    private async Task<JsonObject> ReadParmVersionsAsync(CancellationToken ct = default)
    {
        var op = new DanfossReadParmVersionsOperation(_endpoint, _loggerFactory);
        var result = await op.ExecuteAsync(_pipelineExecutor, ct);
        return ControllerLevelParse(result);
    }

    // ------------------------------------------------------------
    // Polling helpers
    // ------------------------------------------------------------

    // ------------------------------------------------------------
    // Utility
    // ------------------------------------------------------------

    private async Task EnsureInitialized()
    {
        if (!_initialized)
            await InitializeAsync();
    }

    private JsonObject ControllerLevelParse(DeviceOperationResult<JsonNode?> result)
    {
        if (!result.Success)
            return new JsonObject();

        var entry = result.Data?[0]?.AsObject();

        return _normalizer.Normalize(
            DeviceIp,
            DeviceType.ToString(),
            "ControllerInfo",
            entry
        );
    }
}
