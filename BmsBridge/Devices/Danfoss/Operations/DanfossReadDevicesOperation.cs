using System.Text.Json.Nodes;

public sealed class DanfossReadDevicesOperation : DanfossBaseDeviceOperation
{
    public override string Name => "read_devices";

    public DanfossReadDevicesOperation(Uri endpoint, ILoggerFactory loggerFactory)
        : base(endpoint, loggerFactory)
    {
    }

    protected override JsonArray? GetRelevantData(JsonNode? json)
    {
        var node = json?["resp"]?["device"];

        return EnforceData(node);
    }
}
