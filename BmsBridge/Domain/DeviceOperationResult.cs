public sealed class DeviceOperationResult<T>
{
    public bool Success { get; }
    public T? Data { get; }
    public DeviceErrorType ErrorType { get; }
    public string? ErrorMessage { get; }

    private DeviceOperationResult(
        bool success,
        T? data,
        DeviceErrorType errorType,
        string? errorMessage
    )
    {
        Success = success;
        Data = data;
        ErrorType = errorType;
        ErrorMessage = errorMessage;
    }

    public static DeviceOperationResult<T> FromSuccess(T data)
        => new(true, data, DeviceErrorType.None, null);

    public static DeviceOperationResult<T> FromError(DeviceErrorType errorType, string? errorMessage = null)
        => new(false, default, errorType, errorMessage);
}
