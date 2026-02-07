using System.Text.Json.Nodes;

public abstract class BaseDeviceOperation : IDeviceOperation
{
    public abstract string Name { get; }

    protected readonly Uri Endpoint;
    protected readonly ILogger _logger;

    protected BaseDeviceOperation(Uri endpoint, ILoggerFactory loggerFactory)
    {
        Endpoint = endpoint;
        _logger = loggerFactory.CreateLogger(GetType());
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

    protected abstract HttpRequestMessage BuildRequest();

    public virtual async Task<DeviceOperationResult<JsonNode?>> ExecuteAsync(
        IDeviceHttpExecutor executor,
        CancellationToken ct)
    {
        var request = BuildRequest();

        // _logger.LogDebug($"Sending {Name} command to {Endpoint.Host}");
        var httpResult = await executor.SendAsync(Endpoint.Host, request, ct, Name);

        if (!httpResult.Success)
        {
            return DeviceOperationResult<JsonNode?>.FromError(
                httpResult.ErrorType,
                httpResult.ErrorMessage
            );
        }

        var result = ParseResponse(httpResult.Data!);

        if (!result.Success)
        {
            _logger.LogError($"Operation failed: {result.ErrorType}, {result.ErrorMessage}");
        }

        return result;
    }

    protected abstract JsonArray? GetRelevantData(JsonNode? json);

    protected abstract JsonNode? Translate(HttpResponseMessage response);

    protected virtual DeviceOperationResult<JsonNode?> ParseResponse(HttpResponseMessage response)
    {
        try
        {
            // var json = JsonNode.Parse(response.Content.ReadAsStringAsync().Result);
            var json = Translate(response);
            var res = GetRelevantData(json);

            if (res is null)
                return DeviceOperationResult<JsonNode?>.FromSuccess(new JsonArray());

            return DeviceOperationResult<JsonNode?>.FromSuccess(res);
        }
        catch (Exception ex)
        {
            return DeviceOperationResult<JsonNode?>.FromError(
                DeviceErrorType.JsonParseError,
                ex.Message
            );
        }
    }
}
