// Create app builder
var builder = Host.CreateApplicationBuilder(args);


// Configuration
builder.Services.AddAppSettings(builder.Configuration, builder.Environment);

// Logging
builder.AddAppLogging();

// Dump README on startup
builder.ExtractReadme();


<<<<<<< HEAD
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
=======
// DI registration
builder.Services.AddCertificateSource(builder.Environment);
builder.Services.AddIotDevice(builder.Configuration, builder.Environment);
builder.Services.AddDeviceRunnerFactory(args, builder.Environment);
>>>>>>> ea542b6a9c25c27668c32ef7af0bbd98a8774376

builder.Services.AddSingleton<CertificateProvider>();
builder.Services.AddSingleton<KeyvaultService>();
builder.Services.AddSingleton<DpsService>();
builder.Services.AddSingleton<IE2IndexMappingProvider, EmbeddedE2IndexMappingProvider>();
builder.Services.AddSingleton<INormalizerService, NormalizerService>();

builder.Services.AddSingleton<IDeviceHealthRegistry, InMemoryDeviceHealthRegistry>();
builder.Services.AddSingleton<ICircuitBreakerService, CircuitBreakerService>();
builder.Services.AddSingleton<IHealthTelemetryService, HealthTelemetryService>();
builder.Services.AddSingleton<IRunnerControlService, RunnerControlService>();
builder.Services.AddSingleton<IDeviceRunnerRegistry, DeviceRunnerRegistry>();
builder.Services.AddSingleton<IErrorFileService, ErrorFileService>();


// Workers
builder.Services.AddHostedService<DeviceWorker>();
builder.Services.AddHostedService<HealthMonitorWorker>();


// Build app and run
var app = builder.Build();

// On fresh restart, clear all existing error files.
using (var scope = app.Services.CreateScope())
{
    var svc = scope.ServiceProvider.GetRequiredService<IErrorFileService>();
    await svc.CleanupAllAsync();
}


try
{
    app.Run();
}
finally
{
    Environment.Exit(0); // for unconfigured nssm
}
