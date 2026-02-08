using BmsBridge.Configuration;
using System.Text.Json;

public static class SettingsConfigurator
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = null
    };

    public static void EnsureConfig(string configPath)
    {
        if (!File.Exists(configPath))
        {
            GenerateDefault(configPath);
            Console.WriteLine("Default configuration file has been generated. Please edit and re-run.");
            Environment.Exit(0);
        }

        Reconcile(configPath);
    }

    private static void GenerateDefault(string configPath)
    {

        // var defaultSerilog = new SerilogSettings
        // {
        //     Using = new()
        //     {
        //         "Serilog.Sinks.Console",
        //         "Serilog.Sinks.File",
        //         "Serilog.Formatting.Compact"
        //     },
        //     MinimumLevel = LogEventLevel.Debug,
        //     WriteTo = new()
        //     {
        //         new SerilogSink
        //         {
        //             Name = "Console"
        //         },
        //         new SerilogSink
        //         {
        //             Name = "File",
        //             Args = new SerilogSinkArgs
        //             {
        //                 Path = "logs/app.log",
        //                 RollingInterval = RollingInterval.Day,
        //                 FileSizeLimitBytes = 1_000_000,
        //                 RetainedFileCountLimit = 7,
        //                 Formatter = "Serilog.Formatting.Compact.CompactJsonFormatter, Serilog.Formatting.Compact",
        //                 RollOnFileSizeLimit = false
        //             }
        //         }
        //     },
        //     Enrich = new() { "FromLogContext" },
        //     Properties = new()
        //     {
        //         ["Application"] = "BmsBridge"
        //     }
        // };

        var defaultConfig = new
        {
            // Serilog = defaultSerilog,
            LoggingSettings = new LoggingSettings(),
            AzureSettings = new AzureSettings(),
            GeneralSettings = new GeneralSettings(),
            NetworkSettings = new NetworkSettings()
        };

        var json = JsonSerializer.Serialize(defaultConfig, _jsonOptions);
        File.WriteAllText(configPath, json);
    }

    private static void Reconcile(string path)
    {
        var jsonText = File.ReadAllText(path);

        var existing = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonText)!;

        // var defaultSerilog = new SerilogSettings
        // {
        //     Using = new()
        //     {
        //         "Serilog.Sinks.Console",
        //         "Serilog.Sinks.File",
        //         "Serilog.Formatting.Compact"
        //     },
        //     MinimumLevel = LogEventLevel.Debug,
        //     WriteTo = new()
        //     {
        //         new SerilogSink
        //         {
        //             Name = "Console",
        //         },
        //         new SerilogSink
        //         {
        //             Name = "File",
        //             Args = new SerilogSinkArgs
        //             {
        //                 Path = "logs/app.log",
        //                 RollingInterval = RollingInterval.Day,
        //                 FileSizeLimitBytes = 1_000_000,
        //                 RetainedFileCountLimit = 7,
        //                 Formatter = "Serilog.Formatting.Compact.CompactJsonFormatter, Serilog.Formatting.Compact",
        //                 RollOnFileSizeLimit = false
        //             }
        //         }
        //     },
        //     Enrich = new() { "FromLogContext" },
        //     Properties = new()
        //     {
        //         ["Application"] = "BmsBridge"
        //     }
        // };

        var defaultConfig = new
        {
            // Serilog = defaultSerilog,
            LoggingSettings = new LoggingSettings(),
            AzureSettings = new AzureSettings(),
            GeneralSettings = new GeneralSettings(),
            NetworkSettings = new NetworkSettings()
        };

        var defaultsJson = JsonSerializer.Serialize(defaultConfig, _jsonOptions);
        var defaults = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(defaultsJson)!;

        var merged = Merge(defaults, existing);

        var updatedJson = JsonSerializer.Serialize(merged, _jsonOptions);
        File.WriteAllText(path, updatedJson);
    }

    private static object? MergeValue(JsonElement defValue, JsonElement existValue)
    {
        if (defValue.ValueKind == JsonValueKind.Object &&
            existValue.ValueKind == JsonValueKind.Object)
        {
            var defDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(defValue.GetRawText())!;
            var existDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(existValue.GetRawText())!;
            return Merge(defDict, existDict);
        }

        if (defValue.ValueKind == JsonValueKind.Array &&
            existValue.ValueKind == JsonValueKind.Array)
        {
            // Keep existing array entirely
            return JsonSerializer.Deserialize<object?>(existValue.GetRawText());
        }

        // Primitive : keep existing
        return JsonSerializer.Deserialize<object?>(existValue.GetRawText());
    }

    private static Dictionary<string, object?> Merge(
        Dictionary<string, JsonElement> defaults,
        Dictionary<string, JsonElement> existing)
    {
        var result = new Dictionary<string, object?>();

        foreach (var (key, defValue) in defaults)
        {
            if (existing.TryGetValue(key, out var existValue))
            {
                result[key] = MergeValue(defValue, existValue);
            }
            else
            {
                result[key] = JsonSerializer.Deserialize<object?>(defValue.GetRawText());
            }
        }

        return result;
    }
}
