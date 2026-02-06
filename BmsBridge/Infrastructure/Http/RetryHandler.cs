public class RetryHandler : IRequestHandler
{
    private readonly IRequestHandler _next;
    private readonly int _maxRetries;
    private readonly TimeSpan _delay;

    public RetryHandler(IRequestHandler next, int maxRetries, TimeSpan delay)
    {
        _next = next;
        _maxRetries = maxRetries;
        _delay = delay;
    }

    public async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken = default)
    {
        int attempts = 0;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using var clonedRequest = CloneRequest(request);

            try
            {
                return await _next.SendAsync(clonedRequest, cancellationToken);
            }
            catch when (attempts < _maxRetries)
            {
                attempts++;
                await Task.Delay(_delay, cancellationToken);
            }
        }
    }

    public void Dispose()
    {
        _next.Dispose();
    }

    private static HttpRequestMessage CloneRequest(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri)
        {
            Version = request.Version
        };

        // Copy request headers
        foreach (var header in request.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        // Copy content (CRITICAL)
        if (request.Content != null)
        {
            var ms = new MemoryStream();
            request.Content.CopyToAsync(ms).GetAwaiter().GetResult();
            ms.Position = 0;

            clone.Content = new StreamContent(ms);

            foreach (var header in request.Content.Headers)
            {
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        return clone;
    }
}
