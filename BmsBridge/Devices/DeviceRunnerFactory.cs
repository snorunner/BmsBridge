using Microsoft.Extensions.Options;

public sealed class DeviceRunnerFactory : IDeviceRunnerFactory
{
    private readonly GeneralSettings _generalSettings;
    private readonly IE2IndexMappingProvider _indexProvider;
    private readonly INormalizerService _normalizer;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IIotDevice _iotDevice;

    public DeviceRunnerFactory(IOptions<GeneralSettings> generalSettings,
        IE2IndexMappingProvider indexMappingProvider,
        INormalizerService normalizer,
        ILoggerFactory loggerFactory,
        IIotDevice iotDevice
    )
    {
        _generalSettings = generalSettings.Value;
        _indexProvider = indexMappingProvider;
        _normalizer = normalizer;
        _loggerFactory = loggerFactory;
        _iotDevice = iotDevice;
    }

    public IDeviceRunner Create(DeviceSettings deviceSettings)
    {
        IHttpPipelineExecutor executor = new HttpPipelineExecutor(_generalSettings);

        switch (deviceSettings.DeviceType)
        {
            case BmsType.EmersonE2:
                var endpoint = new Uri($"http://{deviceSettings.IP}:14106/JSON-RPC");
                return new E2DeviceRunner(
                    endpoint,
                    executor,
                    _indexProvider,
                    _normalizer,
                    _loggerFactory,
                    _iotDevice
                );
            default:
                throw new NotImplementedException($"Device type {deviceSettings.DeviceType} is not implemented.");
        }

    }
}
