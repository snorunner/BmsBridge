using System.Text.Json.Nodes;

public sealed class DanfossReadSensorsOperation : DanfossBaseDeviceOperation
{
    public override string Name => "read_sensors";

    public DanfossReadSensorsOperation(Uri endpoint, ILoggerFactory loggerFactory)
        : base(endpoint, loggerFactory)
    {
    }

    protected override JsonArray GetRelevantData(JsonNode? json)
    {
        var node = json?["resp"]?["sensor"];

        return EnforceData(node);
    }
}
