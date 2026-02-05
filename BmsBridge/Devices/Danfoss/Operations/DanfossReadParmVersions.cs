public sealed class DanfossReadParmVersionsOperation : DanfossBaseDeviceOperation
{
    public override string Name => "read_parm_versions";

    public DanfossReadParmVersionsOperation(Uri endpoint, ILoggerFactory loggerFactory)
        : base(endpoint, loggerFactory)
    {
    }
}
