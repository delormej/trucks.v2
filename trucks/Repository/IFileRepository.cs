namespace Trucks
{
    /// <summary>
    /// Defines the interface for where persistent unstructured storage should be used as
    /// files are downloaded and converted, but before canonicalized into structured / nosql
    /// persistence.  
    /// </summary>
    /// <notes>
    /// Example implementations include local disk and cloud storage. 
    /// </notes>
    public interface IFileRepository
    {
        /// <summary>
        /// Takes the local file name, storages to the destination and returns the 
        /// destination path.
        /// </summary>
        Task<string> SaveAsync(string localPath);
    }
}