using Serilog;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using System.Runtime.InteropServices;

var builder = Host.CreateApplicationBuilder(args);

// Configuration
var configPath = Path.Combine(builder.Environment.ContentRootPath, "appsettings.json");
SettingsConfigurator.EnsureConfig(configPath);

// Logging
builder.Logging.ClearProviders();
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();
builder.Logging.AddSerilog();
Log.Information("Starting version v0.1.0");

builder.Services.Configure<AzureSettings>(builder.Configuration.GetSection("AzureSettings"));
builder.Services.Configure<GeneralSettings>(builder.Configuration.GetSection("GeneralSettings"));
builder.Services.Configure<NetworkSettings>(builder.Configuration.GetSection("NetworkSettings"));

// Singletons
// builder.Services.AddSingleton<IIotDevice, AzureIotDevice>(); // prod // TODO: Make dynamic
builder.Services.AddSingleton<IIotDevice, ConsoleIotDevice>(); // test
if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
{
    builder.Services.AddSingleton<ICertificateSource>(sp =>
    {
        var options = sp.GetRequiredService<IOptions<AzureSettings>>();
        return new StoreCertificateSource(options);
    });
}
else
{
    builder.Services.AddSingleton<ICertificateSource>(sp =>
        new PfxCertificateSource("/home/henry/Projects/BmsBridge/BmsBridge/DevelopmentKeys/CertificateTest.pfx")); // TODO: make dynamic/from user file
}
builder.Services.AddSingleton<CertificateProvider>();
builder.Services.AddSingleton<KeyvaultService>();
builder.Services.AddSingleton<DpsService>();
builder.Services.AddSingleton<IE2IndexMappingProvider, EmbeddedE2IndexMappingProvider>();
builder.Services.AddSingleton<INormalizerService, NormalizerService>();
builder.Services.AddSingleton<IDeviceRunnerFactory, DeviceRunnerFactory>(); // prod // TODO: Make dynamic
// builder.Services.AddSingleton<IDeviceRunnerFactory, ReplayDeviceRunnerFactory>(); // test
builder.Services.AddSingleton<IDeviceHealthRegistry, InMemoryDeviceHealthRegistry>();

// Workers
builder.Services.AddHostedService<DeviceWorker>();
builder.Services.AddHostedService<HealthMonitorWorker>();


// TEMPORARY MANUAL TEST HARNESS
if (args.Contains("--test-operator"))
{
    // var endpoint = new Uri("http://10.128.223.180:14106/JSON-RPC");
    var loader = new EmbeddedE2IndexMappingProvider();

    var endpoint = new Uri("http://10.158.71.180/http/xml.cgi");

    var settings = new GeneralSettings
    {
        keep_alive = false,
        http_request_delay_seconds = 1,
        http_retry_count = 0,
        http_timeout_delay_seconds = 9
    };

    var executor = new HttpPipelineExecutor(settings);

    // var op = new E2GetControllerListOperation(endpoint, NullLoggerFactory.Instance);
    // var op = new E2GetPointsOperation(endpoint, "HVAC/LTS", "AC1 FAN", loader.GetPointsForCellType(33), NullLoggerFactory.Instance);
    // var op = new DanfossReadHvacServiceOperation(endpoint, "1", NullLoggerFactory.Instance);


    // await op.ExecuteAsync(executor, CancellationToken.None);

    return;
}

var app = builder.Build();
app.Run();
