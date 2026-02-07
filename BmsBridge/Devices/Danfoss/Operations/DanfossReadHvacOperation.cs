using System.Text.Json.Nodes;

public sealed class DanfossReadHvacOperation : DanfossBaseDeviceOperation
{
    public override string Name => "read_hvac";

    public DanfossReadHvacOperation(Uri endpoint, ILoggerFactory loggerFactory)
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
