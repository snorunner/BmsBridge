public sealed class E2DeviceRunner : BaseDeviceRunner
{
    public E2DeviceRunner(
        Uri endpoint,
        IHttpPipelineExecutor executor,
        IE2IndexMappingProvider indexProvider,
        INormalizerService normalizerService,
        ILoggerFactory loggerFactory,
        IIotDevice iotDevice
    ) : base(endpoint,
        executor,
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
            _executor,
            _indexProvider,
            _normalizer,
            _loggerFactory,
            _iotDevice
        );
    }
}
