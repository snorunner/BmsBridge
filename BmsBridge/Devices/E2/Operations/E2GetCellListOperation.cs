using System.Text.Json.Nodes;

public sealed class E2GetCellListOperation : E2BaseDeviceOperation
{
    public override string Name => "E2.GetCellList";

    public string ControllerName;
    protected override JsonArray? Parameters => new JsonArray { ControllerName };

    public E2GetCellListOperation(Uri endpoint, string controllerName, ILoggerFactory loggerFactory)
        : base(endpoint, loggerFactory)
    {
        ControllerName = controllerName;
    }

    protected override JsonArray? GetRelevantData(JsonNode? json)
    {
        return json?["data"]?["result"] as JsonArray;
    }
}
