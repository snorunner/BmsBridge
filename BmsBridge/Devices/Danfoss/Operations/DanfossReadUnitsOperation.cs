public sealed class DanfossReadUnitsOperation : DanfossBaseDeviceOperation
{
    public override string Name => "read_units";

    public DanfossReadUnitsOperation(Uri endpoint, ILoggerFactory loggerFactory)
        : base(endpoint, loggerFactory)
    {
    }

    protected override async Task ParseAsync(HttpResponseMessage response, CancellationToken ct)
    {
        var txt = await response.Content.ReadAsStringAsync(ct);
    }
}
