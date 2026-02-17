using System.Text.Json;
using System.Text.Json.Nodes;

public class FileIotDevice : IIotDevice
{
    private readonly string _filePath;

    public bool IsConnected { get; init; } = true;

    public FileIotDevice(string filePath = "localmessages.jsonl")
    {
        _filePath = filePath;
    }

    public Task ConnectAsync(CancellationToken ct = default)
    {
        // No-op for fake device
        return Task.CompletedTask;
    }

    public async Task SendMessageAsync(JsonNode payload, CancellationToken ct = default)
    {
        try
        {
            var json = payload.ToJsonString(new JsonSerializerOptions
            {
                WriteIndented = false
            });

            // Append as a single JSON line
            await File.AppendAllTextAsync(_filePath, json + Environment.NewLine, ct);
        }
        finally
        {

        }
    }

    public Task DisconnectAsync(CancellationToken ct = default)
    {
        // No-op for fake device
        return Task.CompletedTask;
    }
}
