using System.Text.Json.Nodes;

public sealed class DanfossReadUnitsOperation : DanfossBaseDeviceOperation
{
    public override string Name => "read_units";

    public DanfossReadUnitsOperation(Uri endpoint, ILoggerFactory loggerFactory)
        : base(endpoint, loggerFactory)
    {
    }

    protected override JsonArray GetRelevantData(JsonNode? json)
    {
        var node = json?["resp"];

        return EnforceData(node);
    }
}
