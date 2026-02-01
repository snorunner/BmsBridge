using System.Text.Json.Nodes;
using System.Text;

public sealed class E2GetControllerListOperation : E2BaseDeviceOperation
{
    public override string Name => "E2.GetControllerList";

    /// <summary>
    /// Raw JSON returned by the controller. 
    /// Useful for debugging before writing a real parser.
    /// </summary>
    public string? RawJson { get; private set; }

    public E2GetControllerListOperation(Uri endpoint)
        : base(endpoint)
    {
    }

    public override async Task ExecuteAsync(HttpPipelineExecutor executor, CancellationToken ct)
    {
        // Build the JSON-RPC style request with no parameters
        var request = BuildRequest(Name);

        // Send it through the pipeline
        var response = await executor.SendAsync(request, ct);

        // Read the raw JSON for debugging
        RawJson = await response.Content.ReadAsStringAsync(ct);

        // For now, just print it (or log it)
        Console.WriteLine($"[E2.GetControllerList] Raw response: {RawJson}");
    }
}
