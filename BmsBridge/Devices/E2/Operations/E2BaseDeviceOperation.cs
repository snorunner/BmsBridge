using System.Text.Json.Nodes;
using System.Text;

public abstract class E2BaseDeviceOperation : BaseDeviceOperation
{
    protected virtual JsonArray? Parameters => null;

    protected E2BaseDeviceOperation(Uri endpoint, ILoggerFactory loggerFactory)
        : base(endpoint, loggerFactory)
    {
    }

    protected override IReadOnlyDictionary<string, string> DefaultHeaders =>
        new Dictionary<string, string>
        {
            ["Connection"] = "close",
            ["Content-Type"] = "application/json"
        };

    protected HttpRequestMessage BuildRequest(JsonArray? parameters = null)
    {
        var payload = new JsonObject
        {
            ["id"] = "0",
            ["method"] = Name,
            ["params"] = parameters ?? new JsonArray()
        };

        var request = new HttpRequestMessage(HttpMethod.Post, Endpoint)
        {
            Content = new StringContent(payload.ToJsonString(), Encoding.UTF8, "application/json")
        };

        ApplyHeaders(request);

        return request;
    }

    protected override HttpRequestMessage BuildRequest()
        => BuildRequest(parameters: Parameters);

    protected override JsonNode? Translate(HttpResponseMessage response)
        => JsonNode.Parse(response.Content.ReadAsStringAsync().Result);
}
