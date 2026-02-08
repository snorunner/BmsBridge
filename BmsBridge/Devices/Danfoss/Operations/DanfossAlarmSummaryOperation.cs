using System.Text.Json.Nodes;

public sealed class DanfossAlarmSummaryOperation : DanfossBaseDeviceOperation
{
    public override string Name => "alarm_summary";

    public DanfossAlarmSummaryOperation(Uri endpoint, ILoggerFactory loggerFactory)
        : base(endpoint, loggerFactory)
    {
    }

    protected override JsonArray? GetRelevantData(JsonNode? json)
    {
        var response = json?["resp"];

        if (response is null)
            return new JsonArray();

        return new JsonArray { response.DeepClone() };
    }
}
