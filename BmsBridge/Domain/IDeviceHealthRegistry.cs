public interface IDeviceHealthRegistry
{
    void RegisterDevice(string deviceIp, BmsType deviceType);

    void RecordSuccess(string deviceIp, TimeSpan latency);

    void RecordFailure(string deviceIp, DeviceErrorType errorType, TimeSpan? latency = null);

    void SetCircuitState(string deviceIp, DeviceCircuitState state);

    DeviceHealthSnapshot? GetSnapshot(string deviceIp);

    IReadOnlyCollection<DeviceHealthSnapshot> GetAllSnapshots();
}
