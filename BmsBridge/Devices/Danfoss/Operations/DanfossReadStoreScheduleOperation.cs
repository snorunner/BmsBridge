public sealed class DanfossReadStoreScheduleOperation : DanfossBaseDeviceOperation
{
    public override string Name => "read_store_schedule";

    public DanfossReadStoreScheduleOperation(Uri endpoint, ILoggerFactory loggerFactory)
        : base(endpoint, loggerFactory)
    {
    }
}
