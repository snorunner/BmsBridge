using System.Text.Json.Nodes;

public sealed class DanfossDeviceClient : BaseDeviceClient
{
    public override string DeviceType => "E2";

    private DanfossReadUnitsOperation? _controllers;
    private DanfossReadHvacsOperation? _hvacs;

    public DanfossDeviceClient(
        Uri endpoint,
        IHttpPipelineExecutor executor,
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
    }

    public override async Task InitializeAsync(CancellationToken ct = default)
    {
        _logger.LogInformation($"Initializing Danfoss device client device at {_endpoint}");

        _controllers = await GetControllersAsync(ct);
        _hvacs = await GetHvacsAsync(ct);
    }

    public override async Task PollAsync(CancellationToken ct = default)
    {
        _logger.LogInformation($"Starting polling loop of Danfoss device at {_endpoint}");

        var polledData = await PollDevicesAsync(ct);
        // Console.WriteLine(polledData.ToString());

        var diff = _dataWarehouse.ProcessIncoming(polledData);
        // await _iotDevice.SendMessageAsync(diff, ct);
        //
        // var newOp = new DanfossReadHvacsOperation(_endpoint, _loggerFactory);
        // await newOp.ExecuteAsync(_executor, ct);
    }

    // ------------------------------------------------------------
    // Initialization helpers
    // ------------------------------------------------------------

    private async Task<DanfossReadUnitsOperation> GetControllersAsync(CancellationToken ct)
    {
        var op = new DanfossReadUnitsOperation(_endpoint, _loggerFactory);
        await op.ExecuteAsync(_executor, ct);
        return op;
    }

    private async Task<DanfossReadHvacsOperation> GetHvacsAsync(CancellationToken ct)
    {
        var op = new DanfossReadHvacsOperation(_endpoint, _loggerFactory);
        await op.ExecuteAsync(_executor, ct);
        return op;
    }

    // ------------------------------------------------------------
    // Polling helpers
    // ------------------------------------------------------------

    private async Task<JsonArray> PollDevicesAsync(CancellationToken ct)
    {
        var op = new DanfossReadDevicesOperation(_endpoint, _loggerFactory);
        await op.ExecuteAsync(_executor, ct);

        var polledDeviceData = new JsonArray();

        if (op.Devices is null)
        {
            return polledDeviceData;
        }

        foreach (var dev in op.Devices)
        {
            polledDeviceData.Add(
                _normalizer.Normalize(
                    _endpoint.Host,
                    "Danfoss",
                    dev.DeviceKey,
                    dev.Data
                )
            );
        }
        return polledDeviceData;
    }
}
