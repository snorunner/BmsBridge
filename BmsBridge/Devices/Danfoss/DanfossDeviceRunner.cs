public sealed class DanfossDeviceRunner : BaseDeviceRunner
{
    public DanfossDeviceRunner(
        Uri endpoint,
        IDeviceHttpExecutor pipelineExecutor,
        IE2IndexMappingProvider indexProvider,
        INormalizerService normalizerService,
        ILoggerFactory loggerFactory,
        IIotDevice iotDevice,
        GeneralSettings generalSettings
    ) : base(endpoint,
        pipelineExecutor,
        indexProvider,
        normalizerService,
        loggerFactory,
        iotDevice,
        generalSettings
    )
    { }

    protected override IDeviceClient CreateClient()
    {
        return new DanfossDeviceClient(
            _endpoint,
            _pipelineExecutor,
            _normalizer,
            _loggerFactory,
            _iotDevice
        );
    }
}
