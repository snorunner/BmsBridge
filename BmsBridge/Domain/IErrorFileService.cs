public interface IErrorFileService
{
    Task CreateBlankAsync(string name);
    Task RemoveAsync(string name);
    Task CleanupAllAsync();
}
