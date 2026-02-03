public record DeviceSettings
{
    public string IP { get; init; } = string.Empty;
    public BmsType DeviceType { get; init; }
}
