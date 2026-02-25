using Microsoft.Extensions.Options;

public class ErrorFileService : IErrorFileService
{
    private readonly string _root;
    private readonly ILogger<ErrorFileService> _logger;
    private readonly GeneralSettings _generalSettings;

    public ErrorFileService(ILogger<ErrorFileService> logger, IOptions<GeneralSettings> generalSettings)
    {
        // Root directory where the executable lives
        _root = AppContext.BaseDirectory;
        _logger = logger;
        _generalSettings = generalSettings.Value;
    }

    public Task CreateBlankAsync(string name)
    {
        string filePath = Path.Combine(_root, $"{name}.err");

        _logger.LogInformation($"Creating {name}.err.");
        // Create a blank file (overwrite if exists)
        using (File.Create(filePath)) { }

        return Task.CompletedTask;
    }

    public Task CreateOrAppendAsync(string name, string content)
    {
        string filePath = Path.Combine(_root, $"{name}.err");

        _logger.LogInformation($"Writing to {name}.err.");

        // Ensure directory exists (safe guard)
        Directory.CreateDirectory(_root);

        if (!File.Exists(filePath))
        {
            // Create and write initial content
            File.WriteAllText(filePath, content);
        }
        else
        {
            // Append newline + content
            File.AppendAllText(filePath, Environment.NewLine + content);
        }

        return Task.CompletedTask;
    }

    public Task RemoveAsync(string name)
    {
        string filePath = Path.Combine(_root, $"{name}.err");

        _logger.LogInformation($"Clearing {name}.err.");
        if (File.Exists(filePath))
            File.Delete(filePath);

        return Task.CompletedTask;
    }

    public Task CleanupAllAsync()
    {
        _logger.LogInformation("Clearing all err files.");
        var files = Directory.GetFiles(_root, "*.err");

        foreach (var file in files)
        {
            var fileName = Path.GetFileNameWithoutExtension(file);

            if (fileName.Equals("FATAL_LOCK", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    // Read timestamp from file
                    string content = File.ReadAllText(file).Trim();

                    if (DateTime.TryParse(content, out DateTime timestamp))
                    {
                        var age = DateTime.Now - timestamp;

                        if (age < TimeSpan.FromMinutes(_generalSettings.lock_file_minutes))
                        {
                            _logger.LogCritical(
                                $"FATAL_LOCK.err is recent ({age.TotalMinutes:F0} minutes old).");

                            throw new InvalidOperationException(
                                "FATAL_LOCK indicates a fatal condition within the time period.");
                        }
                    }
                    else
                    {
                        _logger.LogWarning("FATAL_LOCK.err exists but timestamp is invalid.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, "Error while processing FATAL_LOCK.err.");
                    throw;
                }
            }

            // Delete file after checks
            File.Delete(file);
        }

        return Task.CompletedTask;
    }
}
