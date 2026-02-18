using System.Text.Json.Nodes;

public sealed class DanfossReadStoreScheduleOperation : DanfossBaseDeviceOperation
{
    public override string Name => "read_store_schedule";

    public DanfossReadStoreScheduleOperation(Uri endpoint, ILoggerFactory loggerFactory)
        : base(endpoint, loggerFactory)
    {
    }

    protected override JsonArray GetRelevantData(JsonNode? json)
    {
        var node = json?["resp"];

        return EnforceData(node);
    }
}
