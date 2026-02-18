public class ErrorFileService : IErrorFileService
{
    private readonly string _root;
    private readonly ILogger<ErrorFileService> _logger;

    public ErrorFileService(ILogger<ErrorFileService> logger)
    {
        // Root directory where the executable lives
        _root = AppContext.BaseDirectory;
        _logger = logger;
    }

    public Task CreateBlankAsync(string name)
    {
        string filePath = Path.Combine(_root, $"{name}.err");

        _logger.LogInformation($"Creating {name}.err.");
        // Create a blank file (overwrite if exists)
        using (File.Create(filePath)) { }

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
            File.Delete(file);

        return Task.CompletedTask;
    }
}
