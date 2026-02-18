using System.Text.Json.Nodes;

public sealed class DanfossAlarmSummaryOperation : DanfossBaseDeviceOperation
{
    public override string Name => "alarm_summary";

    public DanfossAlarmSummaryOperation(Uri endpoint, ILoggerFactory loggerFactory)
        : base(endpoint, loggerFactory)
    {
    }

    protected override JsonArray GetRelevantData(JsonNode? json)
    {
        var node = json?["resp"];

        return EnforceData(node);
    }
}
