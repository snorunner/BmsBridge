using System.Text.Json.Nodes;
using System.Collections.Concurrent;
using Microsoft.Extensions.Options;

public sealed class HealthTelemetryService : IHealthTelemetryService
{
    private readonly IIotDevice _iotDevice;
    private readonly ILogger<HealthTelemetryService> _logger;
    private readonly INormalizerService _normalizer;

    private readonly ConcurrentDictionary<string, DeviceHealthSnapshot> _lastSent = new();
    private readonly TimeSpan _heartbeatInterval;
    private readonly GeneralSettings _generalSettings;

    public HealthTelemetryService(
        IIotDevice iotDevice,
        ILogger<HealthTelemetryService> logger,
        INormalizerService normalizerService,
        IOptions<GeneralSettings> generalSettings)
    {
        _iotDevice = iotDevice;
        _logger = logger;
        _normalizer = normalizerService;
        _generalSettings = generalSettings.Value;
        var heartbeatInterval = _generalSettings.health_telemetry_max_interval_seconds;
        _heartbeatInterval = TimeSpan.FromSeconds(heartbeatInterval);
    }

    public async Task SendSnapshotAsync(IReadOnlyCollection<DeviceHealthSnapshot> snapshots, CancellationToken ct = default)
    {
        var payloads = new JsonArray();

        foreach (var snapshot in snapshots)
        {
            if (!ShouldSend(snapshot))
                return;

            payloads.Add(BuildPayload(snapshot));

            _lastSent[snapshot.DeviceIp] = snapshot;

            _logger.LogInformation("Queuing health telemetry for {Ip}", snapshot.DeviceIp);
        }

        await _iotDevice.SendMessageAsync(payloads, ct);
    }

    private bool ShouldSend(DeviceHealthSnapshot snapshot)
    {
        if (!_lastSent.TryGetValue(snapshot.DeviceIp, out var last))
            return true; // first time sending

        // Circuit breaker state changed
        if (snapshot.CircuitState != last.CircuitState)
            return true;

        // Error type changed
        if (snapshot.LastErrorType != last.LastErrorType)
            return true;

        // Failure threshold crossed
        if (snapshot.ConsecutiveFailures > 1)
            return true;

        // Recovery
        if (snapshot.ConsecutiveFailures == 0 &&
            last.ConsecutiveFailures > 0)
            return true;

        // Heartbeat
        if (snapshot.LastSuccessUtc.HasValue &&
            last.LastSuccessUtc.HasValue &&
            (snapshot.LastSuccessUtc.Value - last.LastSuccessUtc.Value) >= _heartbeatInterval)
            return true;

        return false;
    }

    private JsonNode BuildPayload(DeviceHealthSnapshot snapshot)
    {
        var jsonSnapshot = new JsonObject
        {
            ["deviceIp"] = snapshot.DeviceIp,
            ["deviceType"] = snapshot.DeviceType.ToString(),
            ["circuitState"] = snapshot.CircuitState.ToString(),
            ["lastSuccessUtc"] = snapshot.LastSuccessUtc?.ToString("o"),
            ["lastFailureUtc"] = snapshot.LastFailureUtc?.ToString("o"),
            ["consecutiveFailures"] = snapshot.ConsecutiveFailures,
            ["lastLatencyMs"] = snapshot.LastLatency?.TotalMilliseconds,
            ["lastErrorType"] = snapshot.LastErrorType?.ToString()
        };

        return _normalizer.Normalize(
            snapshot.DeviceIp,
            snapshot.DeviceType.ToString(),
            "health_status",
            jsonSnapshot
        );
    }
}
