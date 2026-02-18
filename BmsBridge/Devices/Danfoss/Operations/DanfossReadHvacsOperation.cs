using System.Text.Json.Nodes;

public sealed class DanfossReadHvacsOperation : DanfossBaseDeviceOperation
{
    public override string Name => "read_hvacs";

    public DanfossReadHvacsOperation(Uri endpoint, ILoggerFactory loggerFactory)
        : base(endpoint, loggerFactory)
    {
    }

    protected override JsonArray GetRelevantData(JsonNode? json)
    {
        var node = json?["resp"]?["hvacs"]?["hvac"];

        return EnforceData(node);
    }
}
