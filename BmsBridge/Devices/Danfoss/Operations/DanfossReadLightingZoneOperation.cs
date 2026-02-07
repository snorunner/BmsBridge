using System.Text.Json.Nodes;

public sealed class DanfossReadLightingZoneOperation : DanfossBaseDeviceOperation
{
    public override string Name => "read_lighting_zone";

    public DanfossReadLightingZoneOperation(Uri endpoint, string index, ILoggerFactory loggerFactory)
        : base(endpoint, loggerFactory)
    {
    }

    protected override JsonArray? GetRelevantData(JsonNode? json)
    {
        var response = json?["resp"]?["device"] as JsonArray;

        if (response is null)
            return new JsonArray();

        return response;
    }
}
