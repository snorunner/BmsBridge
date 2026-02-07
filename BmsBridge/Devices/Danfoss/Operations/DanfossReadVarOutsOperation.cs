using System.Text.Json.Nodes;

public sealed class DanfossReadVarOutsOperation : DanfossBaseDeviceOperation
{
    public override string Name => "read_var_outs";

    public DanfossReadVarOutsOperation(Uri endpoint, ILoggerFactory loggerFactory)
        : base(endpoint, loggerFactory)
    {
    }

    protected override JsonArray? GetRelevantData(JsonNode? json)
    {
        var response = json?["resp"]?["var_output"] as JsonArray;

        if (response is null)
            return new JsonArray();

        return response;
    }
}
