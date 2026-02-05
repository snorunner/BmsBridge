public sealed class DanfossReadHvacOperation : DanfossBaseDeviceOperation
{
    public override string Name => "read_hvac";

    public DanfossReadHvacOperation(Uri endpoint, ILoggerFactory loggerFactory)
        : base(endpoint, loggerFactory)
    {
    }
}
