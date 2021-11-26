namespace Trucks
{
    public interface IFileRepository
    {
        Task<string> SaveAsync(string localPath);
    }
}