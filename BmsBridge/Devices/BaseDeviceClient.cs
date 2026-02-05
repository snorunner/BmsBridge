using System.Text.Json.Nodes;

public abstract class BaseDeviceClient : IDeviceClient
{
    protected readonly Uri _endpoint;
    protected readonly IDeviceHttpExecutor _pipelineExecutor;
    protected readonly ILogger _logger;
    protected readonly INormalizerService _normalizer;
    protected readonly ILoggerFactory _loggerFactory;
    protected readonly IJsonDataWarehouse _dataWarehouse;
    protected readonly IIotDevice _iotDevice;

    public abstract BmsType DeviceType { get; }

    public string DeviceIp => _endpoint.Host;

    protected BaseDeviceClient(
        Uri endpoint,
        ILoggerFactory loggerFactory,
        IDeviceHttpExecutor pipelineExecutor,
        INormalizerService normalizer,
        IIotDevice iotDevice)
    {
        _endpoint = endpoint;
        _logger = loggerFactory.CreateLogger(GetType());
        _pipelineExecutor = pipelineExecutor;
        _normalizer = normalizer;
        _loggerFactory = loggerFactory;

        List<string> preservedFields = new() { "ip", "device_key" };
        _dataWarehouse = new MemoryJsonDataWarehouse(preservedFields);

        _iotDevice = iotDevice;
    }

    public abstract Task InitializeAsync(CancellationToken ct = default);
    public abstract Task PollAsync(CancellationToken ct = default);

    public virtual JsonNode? GetAllStoredData()
        => _dataWarehouse.GetJsonData();
}
