namespace Infrastructure.Settings
{
    public class MinioSettings
    {
        public string Endpoint { get; set; } = default!;
        public string AccessKey { get; set; } = default!;
        public string SecretKey { get; set; } = default!;
        public IEnumerable<string> BucketNames { get; set; } = default!;
    }
}
