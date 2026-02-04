using Microsoft.Extensions.Options;

public sealed class ReplayDeviceRunnerFactory : IDeviceRunnerFactory
{
    private readonly GeneralSettings _generalSettings;
    private readonly IE2IndexMappingProvider _indexProvider;
    private readonly INormalizerService _normalizer;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IIotDevice _iotDevice;

    public ReplayDeviceRunnerFactory(IOptions<GeneralSettings> generalSettings,
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
        IHttpPipelineExecutor executor = new ReplayHttpPipelineExecutor(_generalSettings);

        switch (deviceSettings.DeviceType)
        {
            case BmsType.EmersonE2:
                return new E2DeviceRunner(
                    new Uri($"http://{deviceSettings.IP}:14106/JSON-RPC"),
                    executor,
                    _indexProvider,
                    _normalizer,
                    _loggerFactory,
                    _iotDevice
                );
            case BmsType.Danfoss:
                return new DanfossDeviceRunner(
                    new Uri($"http://{deviceSettings.IP}/http/xml.cgi"),
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
