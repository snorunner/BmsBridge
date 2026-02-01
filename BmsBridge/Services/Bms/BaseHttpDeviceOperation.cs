public abstract class BaseDeviceOperation : IDeviceOperation
{
    public abstract string Name { get; }

    protected readonly Uri Endpoint;

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

    protected abstract HttpRequestMessage BuildRequest();

    public virtual Task ExecuteAsync(HttpPipelineExecutor executor, CancellationToken ct)
    {
        var request = BuildRequest();
        return executor.SendAsync(request, ct);
    }
}
