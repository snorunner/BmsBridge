using System.Text.Json.Nodes;

public sealed class DanfossDeviceClient : BaseDeviceClient
{
    private bool _initialized;

    public override BmsType DeviceType => BmsType.Danfoss;

    // Oneshot objects
    private JsonObject? _unitsData;
    private JsonObject? _parmVersions;
    private JsonObject? _storeSchedule;
    private List<JsonObject> _sensors = new();
    private List<JsonObject> _inputs = new();
    private List<JsonObject> _relays = new();
    private List<JsonObject> _var_outs = new();
    private List<JsonObject> _lighting = new();

    // Polling data objects
    private JsonArray _polledData = new();
    private List<JsonObject> _hvacs = new();
    private List<JsonObject> _devices = new();
    private List<JsonObject> _lightingZones = new();

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
        try
        {
            // _unitsData = await ReadUnitsAsync(ct);
            // _parmVersions = await ReadParmVersionsAsync(ct);
            // _storeSchedule = await ReadStoreScheduleAsync(ct);
            // _sensors = await ReadSensorsAsync(ct);
            // _inputs = await ReadInputsAsync(ct);
            // _relays = await ReadRelaysAsync(ct);
            // _var_outs = await ReadVarOutsAsync(ct);
            // _lighting = await ReadLightingAsync(ct);

            _polledData = new();
        }
        catch
        {
            _logger.LogError($"Failed to initialize device at {DeviceIp}.");
            _initialized = false;
        }

        // _polledData.Add(_unitsData);
        // _polledData.Add(_parmVersions);
        // _polledData.Add(_storeSchedule);
        // _sensors.ForEach(_polledData.Add);
        // _inputs.ForEach(_polledData.Add);
        // _relays.ForEach(_polledData.Add);
        // _var_outs.ForEach(_polledData.Add);
        // _lighting.ForEach(_polledData.Add);
    }

    public override async Task PollAsync(CancellationToken ct = default)
    {
        await EnsureInitialized();

        // _hvacs = await ReadHvacAsync(ct);
        // _devices = await ReadDevicesAsync(ct);

        // _hvacs.ForEach(_polledData.Add);
        // _devices.ForEach(_polledData.Add);

        var diff = _dataWarehouse.ProcessIncoming(_polledData);
        await _iotDevice.SendMessageAsync(diff, ct);

        _polledData = new JsonArray();
        ClearPollingDataObjects();
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

    private async Task<JsonObject> ReadUnitsAsync(CancellationToken ct = default)
    {
        var op = new DanfossReadUnitsOperation(_endpoint, _loggerFactory);
        return await ControllerLevelParse(op, ct);
    }

    private async Task<JsonObject> ReadParmVersionsAsync(CancellationToken ct = default)
    {
        var op = new DanfossReadParmVersionsOperation(_endpoint, _loggerFactory);
        return await ControllerLevelParse(op, ct);
    }

    private async Task<JsonObject> ReadStoreScheduleAsync(CancellationToken ct = default)
    {
        var op = new DanfossReadStoreScheduleOperation(_endpoint, _loggerFactory);
        return await ControllerLevelParse(op, ct);
    }

    private async Task<List<JsonObject>> ReadSensorsAsync(CancellationToken ct = default)
    {
        var op = new DanfossReadSensorsOperation(_endpoint, _loggerFactory);
        var result = await op.ExecuteAsync(_pipelineExecutor, ct);

        return InjectedNodetypeParse(result, "2");
    }

    private async Task<List<JsonObject>> ReadInputsAsync(CancellationToken ct = default)
    {
        var op = new DanfossReadInputsOperation(_endpoint, _loggerFactory);
        var result = await op.ExecuteAsync(_pipelineExecutor, ct);

        return InjectedNodetypeParse(result, "0");
    }

    private async Task<List<JsonObject>> ReadRelaysAsync(CancellationToken ct = default)
    {
        var op = new DanfossReadRelaysOperation(_endpoint, _loggerFactory);
        var result = await op.ExecuteAsync(_pipelineExecutor, ct);

        return InjectedNodetypeParse(result, "1");
    }

    private async Task<List<JsonObject>> ReadVarOutsAsync(CancellationToken ct = default)
    {
        var op = new DanfossReadVarOutsOperation(_endpoint, _loggerFactory);
        var result = await op.ExecuteAsync(_pipelineExecutor, ct);

        return InjectedNodetypeParse(result, "3");
    }

    // ------------------------------------------------------------
    // Polling helpers
    // ------------------------------------------------------------

    private async Task<List<JsonObject>> ReadHvacAsync(CancellationToken ct = default)
    {
        var op = new DanfossReadHvacOperation(_endpoint, _loggerFactory);
        var result = await op.ExecuteAsync(_pipelineExecutor, ct);

        return DynamicAddressParse(result);
    }

    private async Task<List<JsonObject>> ReadLightingZonesAsync(CancellationToken ct = default)
    {

    }

    private async Task<List<JsonObject>> ReadLightingAsync(CancellationToken ct = default)
    {
        var op = new DanfossReadLightingOperation(_endpoint, _loggerFactory);
        var result = await op.ExecuteAsync(_pipelineExecutor, ct);

        return DynamicAddressParse(result);
    }

    private async Task<List<JsonObject>> ReadDevicesAsync(CancellationToken ct = default)
    {
        var op = new DanfossReadDevicesOperation(_endpoint, _loggerFactory);
        var result = await op.ExecuteAsync(_pipelineExecutor, ct);

        return DynamicAddressParse(result);
    }

    // ------------------------------------------------------------
    // Utility
    // ------------------------------------------------------------

    private async Task EnsureInitialized()
    {
        if (!_initialized)
            await InitializeAsync();
    }

    private void ClearPollingDataObjects()
    {
        _hvacs = new();
        _devices = new();
        _lightingZones = new();
    }

    private List<JsonObject> DynamicAddressParse(DeviceOperationResult<JsonNode?> result)
    {
        var resultArray = result?.Data?.AsArray();

        if (!result!.Success || resultArray is null)
            return new List<JsonObject>();

        var returnList = new List<JsonObject>();


        foreach (var entry in resultArray)
        {
            if (entry is null)
                continue;

            var entryObj = entry.AsObject();

            if (!entryObj.TryGetPropertyValue("@nodetype", out var nodeType) ||
                !entryObj.TryGetPropertyValue("@node", out var node) ||
                !entryObj.TryGetPropertyValue("@mod", out var mod) ||
                !entryObj.TryGetPropertyValue("@point", out var point))
            {
                continue;
            }

            var normalizedEntry = _normalizer.Normalize(
                DeviceIp,
                DeviceType.ToString(),
                $"nt{nodeType}:n{node}:m{mod}:p{point}",
                entryObj
            );

            returnList.Add(normalizedEntry);
        }

        return returnList;
    }

    private List<JsonObject> InjectedNodetypeParse(DeviceOperationResult<JsonNode?> result, string nodeType)
    {
        var resultArray = result?.Data?.AsArray();

        if (!result!.Success || resultArray is null)
            return new List<JsonObject>();

        var returnList = new List<JsonObject>();


        foreach (var entry in resultArray)
        {
            if (entry is null)
                continue;

            var entryObj = entry.AsObject();

            if (!entryObj.TryGetPropertyValue("node", out var node) ||
                !entryObj.TryGetPropertyValue("mod", out var mod) ||
                !entryObj.TryGetPropertyValue("point", out var point))
            {
                continue;
            }

            var normalizedEntry = _normalizer.Normalize(
                DeviceIp,
                DeviceType.ToString(),
                $"nt{nodeType}:n{node}:m{mod}:p{point}",
                entryObj
            );

            returnList.Add(normalizedEntry);
        }

        return returnList;
    }

    private async Task<JsonObject> ControllerLevelParse(DanfossBaseDeviceOperation op, CancellationToken ct, string dataAddress = "ControllerInfo")
    {
        var result = await op.ExecuteAsync(_pipelineExecutor, ct);

        if (!result.Success)
            return new JsonObject();

        var entry = result.Data?[0]?.AsObject();

        return _normalizer.Normalize(
            DeviceIp,
            DeviceType.ToString(),
            dataAddress,
            entry
        );
    }
}
