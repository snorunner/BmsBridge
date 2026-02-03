using System.Text.Json.Nodes;

public sealed class E2DeviceClient : BaseDeviceClient
{
    private readonly IE2IndexMappingProvider _indexProvider;

    private string? _primaryController;
    private List<E2CellListInfo>? _cells;

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
                iotDevice: iotDevice)
    {
        _indexProvider = indexProvider;
    }

    public override async Task InitializeAsync(CancellationToken ct = default)
    {
        // 1. Get controller list
        var controllerOp = new E2GetControllerListOperation(_endpoint, _loggerFactory);
        await controllerOp.ExecuteAsync(_executor, ct);

        _primaryController = controllerOp.PrimaryController!.Name
            ?? throw new InvalidOperationException("No primary controller found.");

        // 2. Get cell list
        var cellOp = new E2GetCellListOperation(_endpoint, _primaryController, _loggerFactory);
        await cellOp.ExecuteAsync(_executor, ct);

        _cells = cellOp.Cells?.ToList()
            ?? throw new InvalidOperationException("Cell list missing.");
    }

    public override async Task PollAsync(CancellationToken ct = default)
    {
        if (_primaryController is null || _cells is null)
            throw new InvalidOperationException("DeviceClient not initialized.");

        JsonArray polledData = new();

        foreach (var cell in _cells)
        {
            var points = _indexProvider.GetPointsForCellType(cell.CellType);

            if (points.Count == 0)
                continue; // or log missing mapping

            var op = new E2GetPointsOperation(
                _endpoint,
                _primaryController,
                cell.CellName,
                points,
                _loggerFactory
            );

            await op.ExecuteAsync(_executor, ct);

            // Normalize and emit
            var normalized = _normalizer.Normalize(
                deviceIp: DeviceIp,
                deviceType: DeviceType,
                dataAddress: $"{_primaryController}:{cell.CellName}",
                rawData: (JsonObject?)op.ToJson()
            );

            polledData.Add(normalized);
        }

        // Alarms
        var alarmOp = new E2GetAlarmListOperation(_endpoint, _primaryController, _loggerFactory);
        await alarmOp.ExecuteAsync(_executor, ct);

        var normalizedAlarms = _normalizer.Normalize(
            deviceIp: DeviceIp,
            deviceType: DeviceType,
            dataAddress: $"{_primaryController}:ALARMS",
            rawData: (JsonObject?)alarmOp.ToJson()
        );

        polledData.Add(normalizedAlarms);

        var outData = _dataWarehouse.ProcessIncoming(polledData);
        await _iotDevice.SendMessageAsync(outData, ct);
    }
}
