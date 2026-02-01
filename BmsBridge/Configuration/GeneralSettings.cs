public record GeneralSettings
{
    public int http_request_delay_seconds { get; init; } = 5;
    public int http_timeout_delay_seconds { get; init; } = 15;
    public int http_retry_count { get; init; } = 3;
    public int soft_reset_interval_hours { get; init; } = 12;
    public int e2_max_buffer_length { get; init; } = 100;
    public int log_file_max_size_mb { get; init; } = 10;
    public int log_files_max_number { get; init; } = 3;
    public bool keep_alive { get; init; }
}
