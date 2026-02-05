public sealed class DanfossReadMetersOperation : DanfossBaseDeviceOperation
{
    public override string Name => "read_meters";

    public DanfossReadMetersOperation(Uri endpoint, ILoggerFactory loggerFactory)
        : base(endpoint, loggerFactory)
    {
    }
}
