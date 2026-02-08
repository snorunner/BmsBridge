using System.Text.Json.Nodes;

public sealed class E3GetGroupsOperation : E3BaseDeviceOperation
{
    public override string Name => "GetGroups";

    protected override JsonObject? Parameters => _parameters;
    private readonly JsonObject _parameters;

    public E3GetGroupsOperation(Uri endpoint, string sessionId, ILoggerFactory loggerFactory)
        : base(endpoint, loggerFactory)
    {
        _parameters = new JsonObject
        {
            ["sid"] = sessionId,
            ["user"] = "system.default"
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
