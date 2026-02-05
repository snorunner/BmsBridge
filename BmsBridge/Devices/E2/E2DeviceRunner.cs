public sealed class E2DeviceRunner : BaseDeviceRunner
{
    public E2DeviceRunner(
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
        return new E2DeviceClient(
            _endpoint,
            _pipelineExecutor,
            _indexProvider,
            _normalizer,
            _loggerFactory,
            _iotDevice
        );
    }
}
