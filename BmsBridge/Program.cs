// Create app builder
var builder = Host.CreateApplicationBuilder(args);


// Configuration
builder.Services.AddAppSettings(builder.Configuration, builder.Environment);

// Logging
builder.AddAppLogging();

// Dump README on startup
builder.ExtractReadme();


// DI registration
builder.Services.AddCertificateSource(builder.Environment);
builder.Services.AddIotDevice(builder.Configuration, builder.Environment);
builder.Services.AddDeviceRunnerFactory(args, builder.Environment);

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
    Thread.Sleep(3000); // for unconfigured nssm
    Environment.Exit(0); // for unconfigured nssm
}
