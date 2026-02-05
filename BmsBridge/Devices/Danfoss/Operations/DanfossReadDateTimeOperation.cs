public sealed class DanfossReadDateTimeOperation : DanfossBaseDeviceOperation
{
    public override string Name => "read_date_time";

    public DanfossReadDateTimeOperation(Uri endpoint, ILoggerFactory loggerFactory)
        : base(endpoint, loggerFactory)
    {
    }
}
