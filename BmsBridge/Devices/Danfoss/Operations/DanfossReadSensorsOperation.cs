using System.Text.Json.Nodes;

public sealed class DanfossReadSensorsOperation : DanfossBaseDeviceOperation
{
    public override string Name => "read_sensors";

    public DanfossReadSensorsOperation(Uri endpoint, ILoggerFactory loggerFactory)
        : base(endpoint, loggerFactory)
    {
    }

    protected override JsonArray? GetRelevantData(JsonNode? json)
    {
        var response = json?["resp"]?["sensor"] as JsonArray;

        if (response is null)
            return new JsonArray();

        return response;
    }
}
