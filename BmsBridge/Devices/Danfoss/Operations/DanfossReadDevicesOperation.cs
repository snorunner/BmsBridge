public sealed class DanfossReadDevicesOperation : DanfossBaseDeviceOperation
{
    public override string Name => "read_devices";

    public DanfossReadDevicesOperation(Uri endpoint, ILoggerFactory loggerFactory)
        : base(endpoint, loggerFactory)
    {
    }

    protected override async Task ParseAsync(HttpResponseMessage response, CancellationToken ct)
    {
        var txt = await response.Content.ReadAsStringAsync(ct);
    }
}
