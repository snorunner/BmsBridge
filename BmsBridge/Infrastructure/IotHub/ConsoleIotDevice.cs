using System.Text.Json;
using System.Text.Json.Nodes;

public class ConsoleIotDevice : IIotDevice
{
    public bool IsConnected { get; init; } = true;

    public Task ConnectAsync(CancellationToken ct = default)
    {
        // No-op for dummy device
        return Task.CompletedTask;
    }

    public Task SendMessageAsync(JsonNode payload, CancellationToken ct = default)
    {
        var json = payload.ToJsonString(new JsonSerializerOptions
        {
            WriteIndented = true
        });

        Console.WriteLine("=== IoT Device Message ===");
        Console.WriteLine(json);
        Console.WriteLine("==========================");

        return Task.CompletedTask;
    }

    public Task DisconnectAsync(CancellationToken ct = default)
    {
        // No-op for dummy device
        return Task.CompletedTask;
    }
}
