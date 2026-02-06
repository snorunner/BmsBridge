using System.Runtime.InteropServices;

public sealed class HttpPipelineExecutor : IHttpPipelineExecutor, IDisposable
{
    private readonly object _lock = new();
    private readonly GeneralSettings _settings;
    private IRequestHandler? _pipeline;
    private string? _proxy;
    private bool _disposed;

    public HttpPipelineExecutor(GeneralSettings settings)
    {
        _settings = settings;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            _proxy = "socks5://localhost:1080";
    }

    private IRequestHandler CreatePipeline()
    {
        bool retry = false;

        if (_settings.http_retry_count > 0)
            retry = true;

        return HttpPipelineFactory.Create(
            throttleDelay: TimeSpan.FromSeconds(_settings.http_request_delay_seconds),
            enableRetries: retry,
            retryCount: _settings.http_retry_count,
            timeout: TimeSpan.FromSeconds(_settings.http_timeout_delay_seconds),
            socks5Proxy: _proxy
        );
    }

    private IRequestHandler GetOrCreatePipeline()
    {
        lock (_lock)
        {
            if (_pipeline == null)
                _pipeline = CreatePipeline();

            return _pipeline;
        }
    }

    public async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken ct,
        string? Name = null)
    {
        ThrowIfDisposed();

        HttpResponseMessage response;
        if (_settings.keep_alive)
        {
            // Reuse pipeline
            var pipeline = GetOrCreatePipeline();
            response = await pipeline.SendAsync(request, ct);
        }
        else
        {
            // Create a fresh pipeline for this request
            var pipeline = CreatePipeline();
            response = await pipeline.SendAsync(request, ct);

            pipeline.Dispose();

            // Enforce minimum delay
            await Task.Delay(
                TimeSpan.FromSeconds(_settings.http_request_delay_seconds),
                ct
            );
        }

        // On development, save the response for replay and testing
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && Name is not null)
        {
            var text = await response.Content.ReadAsStringAsync();

            if (!string.IsNullOrEmpty(text))
                await File.WriteAllTextAsync($"/home/henry/Projects/BmsBridge/BmsBridge/ReplayData/{Name}.txt", text);
        }

        return response;
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(HttpPipelineExecutor));
    }

    public void Dispose()
    {
        if (_disposed) return;

        lock (_lock)
        {
            _pipeline?.Dispose();
            _pipeline = null;
            _disposed = true;
        }
    }
}
