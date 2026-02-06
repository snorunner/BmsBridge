public interface ICircuitBreakerService
{
    void EvaluateAndUpdate(DeviceHealthSnapshot snapshot);
}
