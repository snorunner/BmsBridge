public interface IErrorFileService
{
    Task CreateBlankAsync(string name);
    Task CreateOrAppendAsync(string name, string content);
    Task RemoveAsync(string name);
    Task CleanupAllAsync();
}
