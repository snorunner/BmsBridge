using System.Collections.Concurrent;

public sealed class InMemoryDeviceHealthRegistry : IDeviceHealthRegistry
{
    private sealed class MutableHealth
    {
        public string DeviceIp { get; init; } = default!;
        public BmsType DeviceType { get; set; }

        public DateTimeOffset? LastSuccessUtc { get; set; }
        public DateTimeOffset? LastFailureUtc { get; set; }
        public int ConsecutiveFailures { get; set; }
        public TimeSpan? LastLatency { get; set; }
        public DeviceErrorType? LastErrorType { get; set; }
        public DeviceCircuitState CircuitState { get; set; } = DeviceCircuitState.Closed;
    }

    private readonly ConcurrentDictionary<string, MutableHealth> _devices = new();
    private readonly ILogger<InMemoryDeviceHealthRegistry> _logger;

    public InMemoryDeviceHealthRegistry(ILogger<InMemoryDeviceHealthRegistry> logger)
    {
        _logger = logger;
    }

    public void RegisterDevice(string deviceIp, BmsType deviceType)
    {
        _logger.LogInformation($"Registering device {deviceIp} to health registry.");

        _devices.AddOrUpdate(
            deviceIp,
            ip => new MutableHealth
            {
                DeviceIp = ip,
                DeviceType = deviceType
            },
            (ip, existing) =>
            {
                existing.DeviceType = deviceType;
                return existing;
            });
    }

    public void SetCircuitState(string deviceIp, DeviceCircuitState state)
    {
        if (_devices.TryGetValue(deviceIp, out var health))
        {
            lock (health)
            {
                health.CircuitState = state;
            }
        }
    }

    public void RecordSuccess(string deviceIp, TimeSpan latency)
    {
        var now = DateTimeOffset.UtcNow;

        var state = _devices.GetOrAdd(deviceIp, ip => new MutableHealth
        {
            DeviceIp = ip,
            DeviceType = default
        });

        lock (state)
        {
            state.LastSuccessUtc = now;
            state.LastLatency = latency;
            state.LastErrorType = DeviceErrorType.None;
            state.ConsecutiveFailures = 0;
        }
    }

    public void RecordFailure(string deviceIp, DeviceErrorType errorType, TimeSpan? latency = null)
    {
        var now = DateTimeOffset.UtcNow;

        var state = _devices.GetOrAdd(deviceIp, ip => new MutableHealth
        {
            DeviceIp = ip,
            DeviceType = default
        });

        lock (state)
        {
            state.LastFailureUtc = now;
            state.LastLatency = latency;
            state.LastErrorType = errorType;
            state.ConsecutiveFailures++;
        }
    }

    public DeviceHealthSnapshot? GetSnapshot(string deviceIp)
    {
        if (!_devices.TryGetValue(deviceIp, out var state))
            return null;

        lock (state)
        {
            return new DeviceHealthSnapshot
            {
                DeviceIp = state.DeviceIp,
                DeviceType = state.DeviceType,
                LastSuccessUtc = state.LastSuccessUtc,
                LastFailureUtc = state.LastFailureUtc,
                ConsecutiveFailures = state.ConsecutiveFailures,
                LastLatency = state.LastLatency,
                LastErrorType = state.LastErrorType,
                CircuitState = state.CircuitState
            };
        }
    }

    public IReadOnlyCollection<DeviceHealthSnapshot> GetAllSnapshots()
    {
        var list = new List<DeviceHealthSnapshot>();

        foreach (var kvp in _devices)
        {
            var state = kvp.Value;
            lock (state)
            {
                list.Add(new DeviceHealthSnapshot
                {
                    DeviceIp = state.DeviceIp,
                    DeviceType = state.DeviceType,
                    LastSuccessUtc = state.LastSuccessUtc,
                    LastFailureUtc = state.LastFailureUtc,
                    ConsecutiveFailures = state.ConsecutiveFailures,
                    LastLatency = state.LastLatency,
                    LastErrorType = state.LastErrorType,
                    CircuitState = state.CircuitState
                });
            }
        }

        return list;
    }
}
