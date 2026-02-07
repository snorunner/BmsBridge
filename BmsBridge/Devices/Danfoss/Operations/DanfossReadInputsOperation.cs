using System.Text.Json.Nodes;

public sealed class DanfossReadInputsOperation : DanfossBaseDeviceOperation
{
    public override string Name => "read_inputs";

    public DanfossReadInputsOperation(Uri endpoint, ILoggerFactory loggerFactory)
        : base(endpoint, loggerFactory)
    {
    }

    protected override JsonArray? GetRelevantData(JsonNode? json)
    {
        var response = json?["resp"]?["input"] as JsonArray;

        if (response is null)
            return new JsonArray();

        return response;
    }
}
