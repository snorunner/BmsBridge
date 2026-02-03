using System.Text.Json.Nodes;

public interface IIotDevice
{
    bool IsConnected { get; }

    Task ConnectAsync(CancellationToken ct = default);
    Task SendMessageAsync(JsonNode payload, CancellationToken ct = default);
    Task DisconnectAsync(CancellationToken ct = default);
}
