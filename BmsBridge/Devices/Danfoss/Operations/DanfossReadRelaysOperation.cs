using System.Text.Json.Nodes;

public sealed class DanfossReadRelaysOperation : DanfossBaseDeviceOperation
{
    public override string Name => "read_relays";

    public DanfossReadRelaysOperation(Uri endpoint, ILoggerFactory loggerFactory)
        : base(endpoint, loggerFactory)
    {
    }

    protected override JsonArray? GetRelevantData(JsonNode? json)
    {
        var response = json?["resp"]?["relay"] as JsonArray;

        if (response is null)
            return new JsonArray();

        return response;
    }
}
