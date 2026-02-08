using System.Text.Json.Nodes;

public sealed class E3GetAppDescriptionOperation : E3BaseDeviceOperation
{
    public override string Name => "GetAppDescription";

    protected override JsonObject? Parameters => _parameters;
    private readonly JsonObject _parameters;

    public E3GetAppDescriptionOperation(Uri endpoint, string sessionId, ILoggerFactory loggerFactory)
        : base(endpoint, loggerFactory)
    {
        _parameters = new JsonObject
        {
            ["iid"] = "1748892653",
            ["sid"] = sessionId,
        };
    }

    protected override JsonArray? GetRelevantData(JsonNode? json)
    {
        var response = json?["result"]?["groups"] as JsonArray;

        if (response is null)
            return new JsonArray();

        return response;
    }
}
