public interface IDeviceOperation
{
    string Name { get; }

    Task ExecuteAsync(HttpPipelineExecutor executor, CancellationToken ct);
}
