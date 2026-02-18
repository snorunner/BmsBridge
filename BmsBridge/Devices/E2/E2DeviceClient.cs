using System.Text.Json.Nodes;

public sealed class E2DeviceClient : BaseDeviceClient
{
    private readonly IE2IndexMappingProvider _indexProvider;

    private bool _initialized;

    public override BmsType DeviceType => BmsType.EmersonE2;

    // Data objects
    private JsonObject? _primaryController;
    private string _controllerName => _primaryController?["data"]?["name"]?.GetValue<string>() ?? "Unknown";
    private List<JsonObject> _cells = new();
    private JsonArray _polledData = new JsonArray();

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

        _initialized = true;

        await GetControllerListAsync(ct);
        await GetCellListAsync(ct);

        // Add initialized data
        _polledData.Add(_primaryController!.DeepClone());
        foreach (var cell in _cells)
            _polledData.Add(cell.DeepClone());
    }

    public override async Task PollAsync(CancellationToken ct = default)
    {
        await EnsureInitialized();

        var points = await GetPointsAsync(ct);
        foreach (var point in points)
            _polledData.Add(point);

        _polledData.Add(await GetAlarmsAsync(ct));

        var diff = _dataWarehouse.ProcessIncoming(_polledData);
        await _iotDevice.SendMessageAsync(diff, ct);

        _polledData = new JsonArray();
    }

    // ------------------------------------------------------------
    // Initialization helpers
    // ------------------------------------------------------------

    private async Task TestPrintAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Testing E2 get cell list operation");
        var idxs = _indexProvider.GetPointsForCellType(98);
        var op = new E2GetPointsOperation(_endpoint, "controllername", "cellname", idxs, _loggerFactory);
        var result = await op.ExecuteAsync(_pipelineExecutor, ct);

        if (!result.Success)
        {
            _logger.LogError($"Operation failed: {result.ErrorType}, {result.ErrorMessage}");
            _initialized = false;
            return;
        }

        _logger.LogInformation("Raw JSON result:\n{Json}", result.Data?.ToJsonString());
    }

    private async Task GetControllerListAsync(CancellationToken ct = default)
    {
        var op = new E2GetControllerListOperation(_endpoint, _loggerFactory);
        var result = await op.ExecuteAsync(_pipelineExecutor, ct);

        if (!result.Success || result.Data is null)
        {
            _initialized = false;
            return;
        }

        var dataArray = result.Data.AsArray();
        if (dataArray.Count == 0)
        {
            _logger.LogWarning("Controller list for {Ip} returned empty", DeviceIp);
            _initialized = false;
            return;
        }

        var primary = dataArray[0]!.AsObject();
        var controllerName = primary["name"]?.GetValue<string>() ?? "Unknown";

        _primaryController = _normalizer.Normalize(
            DeviceIp,
            DeviceType.ToString(),
            controllerName,
            primary
        );

        _logger.LogInformation("Primary controller set for {Ip}: {Name}", DeviceIp, controllerName);
    }

    private async Task GetCellListAsync(CancellationToken ct = default)
    {
        var op = new E2GetCellListOperation(_endpoint, _controllerName, _loggerFactory);
        var result = await op.ExecuteAsync(_pipelineExecutor, ct);

        if (!result.Success || result.Data is null)
        {
            _initialized = false;
            return;
        }

        var dataArray = result.Data.AsArray();
        if (dataArray.Count == 0)
        {
            _logger.LogWarning("Cell list for {Ip} returned empty", DeviceIp);
            _initialized = false;
            return;
        }

        foreach (var cell in dataArray)
        {
            var cellObj = cell!.AsObject();
            var cellName = cellObj["cellname"]?.GetValue<string>() ?? "Unknown";
            var cellData = _normalizer.Normalize(
                DeviceIp,
                DeviceType.ToString(),
                $"{_controllerName}:{cellName}",
                cellObj
            );
            _cells.Add(cellData);
        }
        _logger.LogInformation("Cells set for {Ip}", DeviceIp);
    }

    // ------------------------------------------------------------
    // Polling helpers
    // ------------------------------------------------------------

    private async Task<IEnumerable<JsonObject>> GetPointsAsync(CancellationToken ct = default)
    {
        var returnList = new List<JsonObject>();

        foreach (var cell in _cells)
        {
            if (cell?["data"] is not JsonObject cellData)
            {
                _logger.LogDebug($"cell was not JsonObject.");
                continue;
            }

            var cellIndexStr = cellData["celltype"]?.GetValue<string>();
            if (string.IsNullOrEmpty(cellIndexStr) || !int.TryParse(cellIndexStr, out var cellIndexInt))
            {
                _logger.LogDebug($"there was no cell index found.");
                continue;
            }

            var cellName = cellData["cellname"]?.GetValue<string>();
            if (string.IsNullOrEmpty(cellName))
            {
                _logger.LogDebug($"The cellname wasn't found");
                continue;
            }

            var indices = _indexProvider.GetPointsForCellType(cellIndexInt);

            var op = new E2GetPointsOperation(_endpoint, _controllerName, cellName, indices, _loggerFactory);
            var result = await op.ExecuteAsync(_pipelineExecutor, ct);

            if (!result.Success || result.Data is null)
                continue;

            foreach (var dataEntryNode in result.Data.AsArray())
            {
                if (dataEntryNode is not JsonObject dataEntry)
                    continue;

                var propName = dataEntry["prop"]?.GetValue<string>() ?? "Unknown";

                var normalizedResult = _normalizer.Normalize(
                    DeviceIp,
                    DeviceType.ToString(),
                    propName,
                    dataEntry
                );

                returnList.Add(normalizedResult);
            }
        }

        return returnList;
    }

    private async Task<JsonObject?> GetAlarmsAsync(CancellationToken ct = default)
    {
        var op = new E2GetAlarmListOperation(_endpoint, _controllerName, _loggerFactory);
        var result = await op.ExecuteAsync(_pipelineExecutor, ct);

        if (!result.Success)
            return null;

        var wrappedJson = new JsonObject();
        wrappedJson.Add("alarms", result.Data!.DeepClone());

        return _normalizer.Normalize(
            DeviceIp,
            DeviceType.ToString(),
            $"{_controllerName}:Alarms",
            wrappedJson
        );
    }

    // ------------------------------------------------------------
    // Utility
    // ------------------------------------------------------------

    private async Task EnsureInitialized()
    {
        if (!_initialized)
            await InitializeAsync();
    }
}
