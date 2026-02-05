public sealed class ReplayDeviceHttpExecutor : IDeviceHttpExecutor
{
    private readonly IHttpPipelineExecutor _inner;
    private readonly IDeviceHealthRegistry _health;
    private readonly ILogger<ReplayDeviceHttpExecutor> _logger;

    public ReplayDeviceHttpExecutor(
        IHttpPipelineExecutor inner,
        IDeviceHealthRegistry health,
        ILogger<ReplayDeviceHttpExecutor> logger)
    {
        _inner = inner;
        _health = health;
        _logger = logger;
    }

    public async Task<DeviceOperationResult<HttpResponseMessage>> SendAsync(
        string deviceIp,
        HttpRequestMessage request,
        CancellationToken ct,
        string? operationName = null)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            var response = await _inner.SendAsync(request, ct, operationName);
            sw.Stop();

            var errorType = DeviceErrorClassifier.Classify(response);

            if (errorType == DeviceErrorType.None)
            {
                _health.RecordSuccess(deviceIp, sw.Elapsed);
                return DeviceOperationResult<HttpResponseMessage>.FromSuccess(response);
            }

            _health.RecordFailure(deviceIp, errorType, sw.Elapsed);

            return DeviceOperationResult<HttpResponseMessage>.FromError(
                errorType,
                $"HTTP error: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            sw.Stop();

            var errorType = DeviceErrorClassifier.Classify(ex);

            _health.RecordFailure(deviceIp, errorType, sw.Elapsed);

            _logger.LogWarning(ex,
                "Device {DeviceIp} failed during {Operation}. ErrorType={ErrorType}",
                deviceIp, operationName, errorType);

            return DeviceOperationResult<HttpResponseMessage>.FromError(
                errorType,
                ex.Message);
        }
    }
}
