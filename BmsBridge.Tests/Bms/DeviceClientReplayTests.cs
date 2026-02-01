public class DeviceClientReplayTests
{
    [Fact]
    public async Task E2DeviceClient_Replay_Test()
    {
        var executor = new ReplayHttpPipelineExecutor(new GeneralSettings());
        var indexProvider = new EmbeddedE2IndexMappingProvider();
        var normalizer = new NormalizerService();

        var client = new E2DeviceClient(
            new Uri("http://fake-device"),
            executor,
            indexProvider,
            normalizer
        );

        await client.InitializeAsync();

        // PollAsync prints normalized messages to console
        await client.PollAsync();
    }
}
