public sealed class DanfossDeviceRunner : BaseDeviceRunner
{
    public DanfossDeviceRunner(
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
        return new DanfossDeviceClient(
            _endpoint,
            _executor,
            _normalizer,
            _loggerFactory,
            _iotDevice
        );
    }
}
