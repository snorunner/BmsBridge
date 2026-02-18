using System.Text.Json.Nodes;

public sealed class DanfossReadInputsOperation : DanfossBaseDeviceOperation
{
    public override string Name => "read_inputs";

    public DanfossReadInputsOperation(Uri endpoint, ILoggerFactory loggerFactory)
        : base(endpoint, loggerFactory)
    {
    }

    protected override JsonArray GetRelevantData(JsonNode? json)
    {
        var node = json?["resp"]?["input"];

        return EnforceData(node);
    }
}
