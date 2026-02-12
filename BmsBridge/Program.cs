using Microsoft.Extensions.Options;
using System.Runtime.InteropServices;

var builder = Host.CreateApplicationBuilder(args);

// Configuration
builder.Services.AddAppSettings(builder.Configuration, builder.Environment);

// Logging
builder.AddAppLogging();

// Dump README on startup
builder.ExtractReadme();

// Singletons
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSingleton<IIotDevice, VoidIotDevice>();
    // or:
    // builder.Services.AddSingleton<IIotDevice, ConsoleIotDevice>();
    // or:
    // builder.Services.AddSingleton<IIotDevice, AzureIotDevice>();
}
else
{
    builder.Services.AddSingleton<IIotDevice, AzureIotDevice>();
}

if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
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
builder.Services.AddSingleton<IErrorFileService, ErrorFileService>();

// Workers
builder.Services.AddHostedService<DeviceWorker>();
builder.Services.AddHostedService<HealthMonitorWorker>();


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var svc = scope.ServiceProvider.GetRequiredService<IErrorFileService>();
    await svc.CleanupAllAsync();
}

app.Run();
