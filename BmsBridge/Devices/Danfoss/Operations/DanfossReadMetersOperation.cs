using System.Text.Json.Nodes;

public sealed class DanfossReadMetersOperation : DanfossBaseDeviceOperation
{
    public override string Name => "read_meters";

    public DanfossReadMetersOperation(Uri endpoint, ILoggerFactory loggerFactory)
        : base(endpoint, loggerFactory)
    {
    }

    protected override JsonArray GetRelevantData(JsonNode? json)
    {
        var node = json?["resp"];

        return EnforceData(node);
    }
}
