using System.Xml.Linq;

public sealed class DanfossReadHvacServiceOperation : DanfossBaseDeviceOperation
{
    public override string Name => "read_hvac_service";

    public string AirHandlerIndex;

    public DanfossReadHvacServiceOperation(Uri endpoint, string airHandlerIndex, ILoggerFactory loggerFactory)
        : base(endpoint, loggerFactory)
    {
        AirHandlerIndex = airHandlerIndex;
    }

    public override async Task ExecuteAsync(IHttpPipelineExecutor executor, CancellationToken ct)
    {
        var parameters = new List<XAttribute>() {
            new XAttribute("ahindex", AirHandlerIndex)
        };
        var request = BuildRequest(Name, parameters);
        _logger.LogInformation($"Sending {Name} to {Endpoint}");
        var response = await executor.SendAsync(request, ct, Name);
        await ParseAsync(response, ct);
    }
}
