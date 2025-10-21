namespace Infrastructure.Settings
{
    public class RedisSettings
    {
        public string ConnectionString { get; set; } = default!;
        public int DefaultDatabase { get; set; } = default!;
    }
}
