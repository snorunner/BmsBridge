using System.Text.Json.Nodes;

public interface IDeviceOperation
{
    string Name { get; }

    Task<DeviceOperationResult<JsonNode?>> ExecuteAsync(IDeviceHttpExecutor executor, CancellationToken ct);
}
