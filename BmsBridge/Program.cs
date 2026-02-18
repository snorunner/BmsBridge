using Serilog;
using Serilog.Events;
using Microsoft.Extensions.Options;
using System.Runtime.InteropServices;
using System.Reflection;

var builder = Host.CreateApplicationBuilder(args);

// Configuration
var configPath = Path.Combine(builder.Environment.ContentRootPath, "appsettings.json");
SettingsConfigurator.EnsureConfig(configPath);


builder.Services.Configure<AzureSettings>(builder.Configuration.GetSection("AzureSettings"));
builder.Services.Configure<GeneralSettings>(builder.Configuration.GetSection("GeneralSettings"));
builder.Services.Configure<NetworkSettings>(builder.Configuration.GetSection("NetworkSettings"));
builder.Services.Configure<LoggingSettings>(builder.Configuration.GetSection("LoggingSettings"));

var loggingSettings = builder.Configuration
    .GetSection("LoggingSettings")
    .Get<LoggingSettings>();

// Logging
builder.Logging.ClearProviders();

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Is(Enum.TryParse<LogEventLevel>(loggingSettings!.MinimumLevel, true, out var parsed) ? parsed : LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        path: "logs/app.log",
        rollingInterval: RollingInterval.Day,
        fileSizeLimitBytes: loggingSettings!.FileSizeLimitBytes,
        retainedFileCountLimit: loggingSettings!.RetainedFileCountLimit,
        rollOnFileSizeLimit: false,
        formatter: new Serilog.Formatting.Compact.CompactJsonFormatter()
    )
    .CreateLogger();
builder.Logging.AddSerilog();

var version = Assembly
    .GetExecutingAssembly()
    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
    .InformationalVersion;

Log.Information("Starting version {Version}", version);

// Dump README on startup
var exeDir = AppContext.BaseDirectory;
var readmePath = Path.Combine(exeDir, "README.md");

using var stream = Assembly.GetExecutingAssembly()
    .GetManifestResourceStream("BmsBridge.README.md");

if (stream != null)
{
    using var reader = new StreamReader(stream);
    var content = reader.ReadToEnd();
    File.WriteAllText(readmePath, content);
}

// Singletons
if (builder.Environment.IsDevelopment())
{
    // builder.Services.AddSingleton<IIotDevice, VoidIotDevice>();
    // or:
    builder.Services.AddSingleton<IIotDevice, ConsoleIotDevice>();
    // or:
    //builder.Services.AddSingleton<IIotDevice, AzureIotDevice>();
}
else
{
    builder.Services.AddSingleton<IIotDevice, AzureIotDevice>();
}

if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
{
    builder.Services.AddSingleton<ICertificateSource>(sp =>
    {
        var options = sp.GetRequiredService<IOptions<AzureSettings>>();
        return new StoreCertificateSource(options);
    });
}
else
{
    var certPath = Path.Combine(
        builder.Environment.ContentRootPath,
        "DevelopmentKeys",
        "CertificateTest.pfx"
    );

    builder.Services.AddSingleton<ICertificateSource>(
        _ => new PfxCertificateSource(certPath)
    );
}

builder.Services.AddSingleton<CertificateProvider>();
builder.Services.AddSingleton<KeyvaultService>();
builder.Services.AddSingleton<DpsService>();
builder.Services.AddSingleton<IE2IndexMappingProvider, EmbeddedE2IndexMappingProvider>();
builder.Services.AddSingleton<INormalizerService, NormalizerService>();

if (args.Contains("--replay", StringComparer.OrdinalIgnoreCase) && builder.Environment.IsDevelopment())
{
    builder.Services.AddSingleton<IDeviceRunnerFactory, ReplayDeviceRunnerFactory>();
}
else
{
    builder.Services.AddSingleton<IDeviceRunnerFactory, DeviceRunnerFactory>();
}

builder.Services.AddSingleton<IDeviceHealthRegistry, InMemoryDeviceHealthRegistry>();
builder.Services.AddSingleton<ICircuitBreakerService, CircuitBreakerService>();
builder.Services.AddSingleton<IHealthTelemetryService, HealthTelemetryService>();
builder.Services.AddSingleton<IRunnerControlService, RunnerControlService>();
builder.Services.AddSingleton<IDeviceRunnerRegistry, DeviceRunnerRegistry>();

// Workers
builder.Services.AddHostedService<DeviceWorker>();
builder.Services.AddHostedService<HealthMonitorWorker>();


var app = builder.Build();
app.Run();
