using System.Text.Json.Nodes;

public sealed class E2GetControllerListOperation : E2BaseDeviceOperation
{
    public override string Name => "E2.GetControllerList";

    public E2GetControllerListOperation(Uri endpoint, ILoggerFactory loggerFactory)
        : base(endpoint, loggerFactory)
    {
    }

    protected override JsonArray? GetRelevantData(JsonNode? json)
    {
        return json?["result"] as JsonArray;
    }
}
