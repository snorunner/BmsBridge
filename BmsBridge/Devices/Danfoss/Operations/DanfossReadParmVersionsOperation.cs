using System.Text.Json.Nodes;

public sealed class DanfossReadParmVersionsOperation : DanfossBaseDeviceOperation
{
    public override string Name => "read_parm_versions";

    public DanfossReadParmVersionsOperation(Uri endpoint, ILoggerFactory loggerFactory)
        : base(endpoint, loggerFactory)
    {
    }

    protected override JsonArray GetRelevantData(JsonNode? json)
    {
        var node = json?["resp"];

        return EnforceData(node);
    }
}
