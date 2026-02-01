using System.Text.Json.Nodes;
using System.Text.Json;

public abstract class BaseDeviceOperation : IDeviceOperation
{
    public abstract string Name { get; }

    protected readonly Uri Endpoint;
    public object? ExportObject { get; set; }

    protected BaseDeviceOperation(Uri endpoint)
    {
        Endpoint = endpoint;
    }

    protected virtual IReadOnlyDictionary<string, string> DefaultHeaders =>
        new Dictionary<string, string>();

    protected void ApplyHeaders(HttpRequestMessage request, IDictionary<string, string>? extraHeaders = null)
    {
        foreach (var kvp in DefaultHeaders)
            request.Headers.TryAddWithoutValidation(kvp.Key, kvp.Value);

        if (extraHeaders != null)
        {
            foreach (var kvp in extraHeaders)
                request.Headers.TryAddWithoutValidation(kvp.Key, kvp.Value);
        }
    }

    protected abstract Task ParseAsync(HttpResponseMessage response, CancellationToken ct);

    protected abstract HttpRequestMessage BuildRequest();

    public virtual async Task ExecuteAsync(IHttpPipelineExecutor executor, CancellationToken ct)
    {
        var request = BuildRequest();
        var response = await executor.SendAsync(request, ct, Name);
        await ParseAsync(response, ct);
    }

    public virtual JsonNode? ToJson()
    {
        if (ExportObject is null)
            return null;

        var json = JsonSerializer.SerializeToNode(ExportObject);
        return new JsonObject
        {
            ["data"] = json
        };
    }
}
