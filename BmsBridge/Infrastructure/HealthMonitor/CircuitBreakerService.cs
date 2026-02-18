using Microsoft.Extensions.Options;

public sealed class CircuitBreakerService : ICircuitBreakerService
{
    private readonly IDeviceHealthRegistry _registry;
    private readonly ILogger<CircuitBreakerService> _logger;
    private readonly IErrorFileService _errorFileService;

    private readonly int _failureThreshold;
    private readonly TimeSpan _openDuration = TimeSpan.FromMinutes(30);

    public CircuitBreakerService(
        IOptions<GeneralSettings> generalSettings,
        IDeviceHealthRegistry registry,
        ILogger<CircuitBreakerService> logger,
        IErrorFileService errorFileService)
    {
        _failureThreshold = generalSettings.Value.http_retry_count;
        _registry = registry;
        _logger = logger;
        _errorFileService = errorFileService;
    }

    public void EvaluateAndUpdate(DeviceHealthSnapshot snapshot)
    {
        switch (snapshot.CircuitState)
        {
            case DeviceCircuitState.Closed:
                EvaluateClosed(snapshot);
                break;

            case DeviceCircuitState.Open:
                EvaluateOpen(snapshot);
                break;

            case DeviceCircuitState.HalfOpen:
                EvaluateHalfOpen(snapshot);
                break;
        }
    }

    private void EvaluateClosed(DeviceHealthSnapshot snapshot)
    {
        if (snapshot.ConsecutiveFailures >= _failureThreshold)
        {
            _logger.LogWarning(
                "Circuit OPEN for {Ip} due to {Failures} consecutive failures for {Duration} minutes",
                snapshot.DeviceIp, snapshot.ConsecutiveFailures, _openDuration.TotalMinutes);

            _registry.SetCircuitState(snapshot.DeviceIp, DeviceCircuitState.Open);
            _errorFileService.CreateBlankAsync($"{snapshot.DeviceIp}_BMS");
        }
    }

    private void EvaluateOpen(DeviceHealthSnapshot snapshot)
    {
        if (snapshot.LastFailureUtc is null)
            return;

        var elapsed = DateTimeOffset.UtcNow - snapshot.LastFailureUtc.Value;

        if (elapsed >= _openDuration)
        {
            _logger.LogInformation(
                "Circuit HALF-OPEN for {Ip} after cooldown",
                snapshot.DeviceIp);

            _registry.SetCircuitState(snapshot.DeviceIp, DeviceCircuitState.HalfOpen);
        }
    }

    private void EvaluateHalfOpen(DeviceHealthSnapshot snapshot)
    {
        // If the last event was a success → close the breaker
        if (snapshot.ConsecutiveFailures == 0)
        {
            _logger.LogInformation(
                "Circuit CLOSED for {Ip} after successful probe",
                snapshot.DeviceIp);

            _registry.SetCircuitState(snapshot.DeviceIp, DeviceCircuitState.Closed);
            _errorFileService.RemoveAsync($"{snapshot.DeviceIp}_BMS");
            return;
        }

        // If the last event was a failure → reopen
        if (snapshot.ConsecutiveFailures > 0)
        {
            _logger.LogWarning(
                "Circuit OPEN for {Ip} after failed probe",
                snapshot.DeviceIp);

            _registry.SetCircuitState(snapshot.DeviceIp, DeviceCircuitState.Open);
        }
    }
}
