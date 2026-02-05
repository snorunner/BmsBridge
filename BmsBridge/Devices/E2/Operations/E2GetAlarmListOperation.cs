using System.Text.Json.Nodes;

public sealed class E2GetAlarmListOperation : E2BaseDeviceOperation
{
    public override string Name => "E2.GetAlarmList";

    public string ControllerName;
    protected override JsonArray? Parameters => new JsonArray { ControllerName };

    public E2GetAlarmListOperation(Uri endpoint, string controllerName, ILoggerFactory loggerFactory)
        : base(endpoint, loggerFactory)
    {
        ControllerName = controllerName;
    }

    protected override JsonArray? GetRelevantData(JsonNode? json)
    {
        return json?["data"]?["result"] as JsonArray;
    }
}
