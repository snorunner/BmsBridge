using System.Text.Json.Nodes;

public sealed class E2DeviceClient : BaseDeviceClient
{
    private readonly IE2IndexMappingProvider _indexProvider;

    private string? _primaryController;
    private List<E2CellListInfo>? _cells;
    private bool _initialized;

    public override string DeviceType => "E2";

    public E2DeviceClient(
        Uri endpoint,
        IHttpPipelineExecutor executor,
        IE2IndexMappingProvider indexProvider,
        INormalizerService normalizer,
        ILoggerFactory loggerFactory,
        IIotDevice iotDevice
    ) : base(endpoint: endpoint,
            executor: executor,
            normalizer: normalizer,
            loggerFactory: loggerFactory,
            iotDevice: iotDevice
        )
    {
        _indexProvider = indexProvider;
    }

    public override async Task InitializeAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Initializing E2 device client");

        _primaryController = await FetchPrimaryControllerAsync(ct);
        _cells = await FetchCellListAsync(ct);

        _initialized = true;
        _logger.LogInformation("E2 device client initialized successfully");
    }

    public override async Task PollAsync(CancellationToken ct = default)
    {
        EnsureInitialized();

        var polledData = await PollAllCellsAsync(ct);
        var alarms = await PollAlarmsAsync(ct);

        polledData.Add(alarms);

        var diff = _dataWarehouse.ProcessIncoming(polledData);
        await _iotDevice.SendMessageAsync(diff, ct);
    }

    // ------------------------------------------------------------
    // Initialization helpers
    // ------------------------------------------------------------

    private async Task<string> FetchPrimaryControllerAsync(CancellationToken ct)
    {
        var op = new E2GetControllerListOperation(_endpoint, _loggerFactory);
        await op.ExecuteAsync(_executor, ct);

        var controller = op.PrimaryController?.Name
            ?? throw new InvalidOperationException("No primary controller found.");

        return controller;
    }

    private async Task<List<E2CellListInfo>> FetchCellListAsync(CancellationToken ct)
    {
        var op = new E2GetCellListOperation(_endpoint, _primaryController!, _loggerFactory);
        await op.ExecuteAsync(_executor, ct);

        return op.Cells?.ToList()
            ?? throw new InvalidOperationException("Cell list missing.");
    }

    // ------------------------------------------------------------
    // Polling helpers
    // ------------------------------------------------------------

    private async Task<JsonArray> PollAllCellsAsync(CancellationToken ct)
    {
        var results = new JsonArray();

        foreach (var cell in _cells!)
        {
            var normalized = await PollSingleCellAsync(cell, ct);
            if (normalized != null)
                results.Add(normalized);
        }

        return results;
    }

    private async Task<JsonObject?> PollSingleCellAsync(E2CellListInfo cell, CancellationToken ct)
    {
        var points = _indexProvider.GetPointsForCellType(cell.CellType);

        if (points.Count == 0)
        {
            _logger.LogWarning("No index mapping found for cell type {CellType}", cell.CellType);
            return null;
        }

        var op = new E2GetPointsOperation(
            _endpoint,
            _primaryController!,
            cell.CellName,
            points,
            _loggerFactory
        );

        await op.ExecuteAsync(_executor, ct);

        return _normalizer.Normalize(
            deviceIp: DeviceIp,
            deviceType: DeviceType,
            dataAddress: $"{_primaryController}:{cell.CellName}",
            rawData: (JsonObject?)op.ToJson()
        );
    }

    private async Task<JsonObject> PollAlarmsAsync(CancellationToken ct)
    {
        var op = new E2GetAlarmListOperation(_endpoint, _primaryController!, _loggerFactory);
        await op.ExecuteAsync(_executor, ct);

        return _normalizer.Normalize(
            deviceIp: DeviceIp,
            deviceType: DeviceType,
            dataAddress: $"{_primaryController}:ALARMS",
            rawData: (JsonObject?)op.ToJson()
        );
    }

    // ------------------------------------------------------------
    // Utility
    // ------------------------------------------------------------

    private void EnsureInitialized()
    {
        if (!_initialized)
            throw new InvalidOperationException("DeviceClient not initialized.");
    }
}
