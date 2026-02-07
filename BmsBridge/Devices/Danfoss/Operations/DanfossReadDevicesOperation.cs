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
        var response = json?["resp"]?["device"] as JsonArray;

        if (response is null)
            return new JsonArray();

        return response;
    }
}
