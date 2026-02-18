using Serilog;
using Serilog.Events;
using System.Reflection;

public static class LoggingExtensions
{
    public static void AddAppLogging(this IHostApplicationBuilder builder)
    {
        // Pull strongly-typed settings
        var loggingSettings = builder.Configuration
            .GetSection("LoggingSettings")
            .Get<LoggingSettings>();

        // Clear default providers
        builder.Logging.ClearProviders();

        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Is(
                Enum.TryParse<LogEventLevel>(
                    loggingSettings!.MinimumLevel,
                    true,
                    out var parsed
                )
                ? parsed
                : LogEventLevel.Information
            )
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File(
                path: "logs/app.log",
                rollingInterval: RollingInterval.Day,
                fileSizeLimitBytes: loggingSettings.FileSizeLimitBytes,
                retainedFileCountLimit: loggingSettings.RetainedFileCountLimit,
                rollOnFileSizeLimit: false,
                formatter: new Serilog.Formatting.Compact.CompactJsonFormatter()
            )
            .CreateLogger();

        builder.Logging.AddSerilog();

        // Log version on startup
        var version = Assembly
            .GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion;

        Log.Information("Starting version {Version}", version);
    }
}
