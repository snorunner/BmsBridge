using System.Text.Json.Nodes;

public sealed class E3GetSystemInventoryOperation : E3BaseDeviceOperation
{
    public override string Name => "GetSystemInventory";

    protected override JsonObject? Parameters => _parameters;
    private readonly JsonObject _parameters;

    public E3GetSystemInventoryOperation(Uri endpoint, string sessionId, ILoggerFactory loggerFactory)
        : base(endpoint, loggerFactory)
    {
        _parameters = new JsonObject
        {
            ["sid"] = sessionId
        };
    }

    protected override JsonArray? GetRelevantData(JsonNode? json)
    {
        var response = json?["result"]?["aps"] as JsonArray;

        if (response is null)
            return new JsonArray();

        return response;
    }
}
