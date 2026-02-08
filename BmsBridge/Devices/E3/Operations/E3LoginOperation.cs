using System.Text.Json.Nodes;

public sealed class E3LoginOperation : E3BaseDeviceOperation
{
    protected override JsonObject? Parameters => _parameters;
    private readonly JsonObject _parameters;

    public override string Name => "Login";

    public E3LoginOperation(Uri endpoint, string sessionId, ILoggerFactory loggerFactory)
        : base(endpoint, loggerFactory)
    {
        _parameters = new JsonObject
        {
            ["key"] = "system.default",
            ["value"] = "",
            ["sid"] = sessionId
        };
    }

    protected override JsonArray? GetRelevantData(JsonNode? json)
    {
        var response = json?["result"] as JsonObject;

        if (response is null)
            return new JsonArray();

        return new JsonArray { response.DeepClone() };
    }
}
