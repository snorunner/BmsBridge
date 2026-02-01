using System.Text.Json.Nodes;
using System.Text;

public abstract class E2BaseDeviceOperation : BaseDeviceOperation
{
    protected E2BaseDeviceOperation(Uri endpoint)
        : base(endpoint)
    {
    }

    protected override IReadOnlyDictionary<string, string> DefaultHeaders =>
        new Dictionary<string, string>
        {
            ["Connection"] = "close",
            ["Content-Type"] = "application/json"
        };

    protected HttpRequestMessage BuildRequest(string method, JsonArray? parameters = null)
    {
        var payload = new JsonObject
        {
            ["id"] = "0",
            ["method"] = method,
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
    {
        return BuildRequest(Name);
    }
}
