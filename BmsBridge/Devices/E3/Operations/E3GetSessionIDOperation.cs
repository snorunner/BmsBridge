using System.Text.Json.Nodes;
using System.Text.Json;

public sealed class E3GetSessionIDOperation : E3BaseDeviceOperation
{
    public override string Name => "GetSessionID";

    public E3GetSessionIDOperation(Uri endpoint, ILoggerFactory loggerFactory)
        : base(endpoint, loggerFactory)
    {
    }

    protected override HttpRequestMessage BuildRequest()
    {
        // Build the JSON-RPC payload exactly like the base class does
        var payload = new
        {
            jsonrpc = "2.0",
            method = Name,
            id = "0"
        };

        var json = JsonSerializer.Serialize(payload);

        // Wrap in ?m= just like Python
        var formDict = new Dictionary<string, string>
        {
            ["m"] = json
        };

        var formUrlEncoded = new FormUrlEncodedContent(formDict);
        var query = formUrlEncoded.ReadAsStringAsync().Result;

        var newUrl = $"{Endpoint}?{query}";

        // GET request, empty body
        return new HttpRequestMessage(HttpMethod.Get, newUrl);
    }

    protected override JsonArray? GetRelevantData(JsonNode? json)
    {
        var response = json?["result"] as JsonObject;

        if (response is null)
            return new JsonArray();

        return new JsonArray { response.DeepClone() };
    }
}
