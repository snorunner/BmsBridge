using System.Text.Json.Nodes;

public sealed class DanfossReadRelaysOperation : DanfossBaseDeviceOperation
{
    public override string Name => "read_relays";

    public DanfossReadRelaysOperation(Uri endpoint, ILoggerFactory loggerFactory)
        : base(endpoint, loggerFactory)
    {
    }

    protected override JsonArray GetRelevantData(JsonNode? json)
    {
        var node = json?["resp"]?["relay"];

        return EnforceData(node);
    }
}
