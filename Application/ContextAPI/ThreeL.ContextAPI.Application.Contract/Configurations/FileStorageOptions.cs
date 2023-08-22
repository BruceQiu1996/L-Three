namespace ThreeL.ContextAPI.Application.Contract.Configurations
{
    public class FileStorageOptions
    {
        public string StorageLocation { get; set; }
        public long MaxSize { get; set; }
        public long AvatarMaxSize { get; set; }
    }
}
