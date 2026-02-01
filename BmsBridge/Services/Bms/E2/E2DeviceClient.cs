using System.Text.Json.Nodes;
using System.Text.Json;

public sealed class E2DeviceClient : IDeviceClient
{
    private readonly Uri _endpoint;
    private readonly IHttpPipelineExecutor _executor;
    private readonly IE2IndexMappingProvider _indexProvider;
    private readonly INormalizerService _normalizer;

    private string? _primaryController;
    private List<E2CellListInfo>? _cells;

    public string DeviceIp => _endpoint.Host;
    public string DeviceType => "E2";

    public E2DeviceClient(
        Uri endpoint,
        IHttpPipelineExecutor executor,
        IE2IndexMappingProvider indexProvider,
        INormalizerService normalizer)
    {
        _endpoint = endpoint;
        _executor = executor;
        _indexProvider = indexProvider;
        _normalizer = normalizer;
    }

    public async Task InitializeAsync(CancellationToken ct = default)
    {
        // 1. Get controller list
        var controllerOp = new E2GetControllerListOperation(_endpoint);
        await controllerOp.ExecuteAsync(_executor, ct);

        _primaryController = controllerOp.PrimaryController!.Name
            ?? throw new InvalidOperationException("No primary controller found.");

        // 2. Get cell list
        var cellOp = new E2GetCellListOperation(_endpoint, _primaryController);
        await cellOp.ExecuteAsync(_executor, ct);

        _cells = cellOp.Cells?.ToList()
            ?? throw new InvalidOperationException("Cell list missing.");
    }

    public async Task PollAsync(CancellationToken ct = default)
    {
        if (_primaryController is null || _cells is null)
            throw new InvalidOperationException("DeviceClient not initialized.");

        foreach (var cell in _cells)
        {
            var points = _indexProvider.GetPointsForCellType(cell.CellType);

            if (points.Count == 0)
                continue; // or log missing mapping

            var op = new E2GetPointsOperation(
                _endpoint,
                _primaryController,
                cell.CellName,
                points
            );

            await op.ExecuteAsync(_executor, ct);

            // Normalize and emit
            var normalized = _normalizer.Normalize(
                deviceIp: DeviceIp,
                deviceType: DeviceType,
                dataAddress: $"{_primaryController}:{cell.CellName}",
                rawData: (JsonObject?)op.ToJson()
            );


            // Temporary for testing
            Console.WriteLine(normalized.ToJsonString(new JsonSerializerOptions
            {
                WriteIndented = true
            }));

            // TODO: send normalized data to IoT Hub
        }

        // Alarms
        var alarmOp = new E2GetAlarmListOperation(_endpoint, _primaryController);
        await alarmOp.ExecuteAsync(_executor, ct);

        var normalizedAlarms = _normalizer.Normalize(
            deviceIp: DeviceIp,
            deviceType: DeviceType,
            dataAddress: $"{_primaryController}:ALARMS",
            rawData: (JsonObject?)alarmOp.ToJson()
        );

        // TODO: send normalized alarms to IoT Hub
    }
}
