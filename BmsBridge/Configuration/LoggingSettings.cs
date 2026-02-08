public record LoggingSettings
{
    public string MinimumLevel { get; init; } = "Information";
    public int FileSizeLimitBytes { get; init; } = 1000000;
    public int RetainedFileCountLimit { get; init; } = 7;
}
