using Serilog;

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

// builder.Services.AddHostedService<Worker>();


// TEMPORARY MANUAL TEST HARNESS
if (args.Contains("--test-e2"))
{
    var endpoint = new Uri("http://10.128.223.180:14106/JSON-RPC"); // your E2 IP

    var settings = new GeneralSettings
    {
        keep_alive = false,
        http_request_delay_seconds = 1,
        http_retry_count = 0,
        http_timeout_delay_seconds = 5
    };

    var executor = new HttpPipelineExecutor(settings);

    var op = new E2GetControllerListOperation(endpoint);

    await op.ExecuteAsync(executor, CancellationToken.None);

    Console.WriteLine("Raw response:");
    Console.WriteLine(op.RawJson);

    // Save replay
    Directory.CreateDirectory("ReplayData");
    File.WriteAllText("ReplayData/E2.GetControllerList.txt", op.RawJson ?? "");

    return;
}

// var app = builder.Build();
// app.Run();
