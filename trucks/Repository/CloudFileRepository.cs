namespace Trucks
{
    public class CloudFileRepository : IFileRepository
    {
        public Task<string> SaveAsync(string localPath)
        {
            return Task.Run<string>(() => "");
        }
    }
}