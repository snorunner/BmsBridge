public sealed class DanfossReadUnitsOperation : DanfossBaseDeviceOperation
{
    public override string Name => "read_units";

    public DanfossReadUnitsOperation(Uri endpoint, ILoggerFactory loggerFactory)
        : base(endpoint, loggerFactory)
    {
    }
}
