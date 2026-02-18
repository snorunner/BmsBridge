using System.Text.Json.Nodes;

public sealed class DanfossReadLightingOperation : DanfossBaseDeviceOperation
{
    public override string Name => "read_lighting";

    public DanfossReadLightingOperation(Uri endpoint, ILoggerFactory loggerFactory)
        : base(endpoint, loggerFactory)
    {
    }

    protected override JsonArray GetRelevantData(JsonNode? json)
    {
        var node = json?["resp"]?["device"];

        return EnforceData(node);
    }
}
