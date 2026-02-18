using System.Text.Json.Nodes;

public sealed class DanfossReadVarOutsOperation : DanfossBaseDeviceOperation
{
    public override string Name => "read_var_outs";

    public DanfossReadVarOutsOperation(Uri endpoint, ILoggerFactory loggerFactory)
        : base(endpoint, loggerFactory)
    {
    }

    protected override JsonArray GetRelevantData(JsonNode? json)
    {
        var node = json?["resp"]?["var_output"];

        return EnforceData(node);
    }
}
