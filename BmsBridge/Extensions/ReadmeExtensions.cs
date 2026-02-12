using System.Reflection;

public static class ReadmeExtensions
{
    public static void ExtractReadme(this IHostApplicationBuilder builder)
    {
        var exeDir = AppContext.BaseDirectory;
        var readmePath = Path.Combine(exeDir, "README.md");

        // The embedded resource name must match your assembly's namespace + file name
        using var stream = Assembly
            .GetExecutingAssembly()
            .GetManifestResourceStream("BmsBridge.README.md");

        if (stream == null)
            return;

        using var reader = new StreamReader(stream);
        var content = reader.ReadToEnd();

        File.WriteAllText(readmePath, content);
    }
}
