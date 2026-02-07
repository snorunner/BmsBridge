public sealed class DanfossDeviceRunner : BaseDeviceRunner
{
    public DanfossDeviceRunner(
        Uri endpoint,
        IDeviceHttpExecutor pipelineExecutor,
        IE2IndexMappingProvider indexProvider,
        INormalizerService normalizerService,
        ILoggerFactory loggerFactory,
        IIotDevice iotDevice
    ) : base(endpoint,
        pipelineExecutor,
        indexProvider,
        normalizerService,
        loggerFactory,
        iotDevice
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
