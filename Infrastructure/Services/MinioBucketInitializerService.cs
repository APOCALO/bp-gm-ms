using Infrastructure.Settings;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Minio.DataModel.Args;
using Minio;

namespace Infrastructure.Services
{
    public class MinioBucketInitializerService : BackgroundService
    {
        private readonly IMinioClient _minioClient;
        private readonly MinioSettings _settings;
        private readonly ILogger<MinioBucketInitializerService> _logger;

        public MinioBucketInitializerService(
            IMinioClient minioClient,
            MinioSettings settings,
            ILogger<MinioBucketInitializerService> logger)
        {
            _minioClient = minioClient;
            _settings = settings;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var bucketNames = _settings.BucketNames;

            foreach (var bucketName in bucketNames)
            {
                try
                {
                    bool bucketExists = await _minioClient.BucketExistsAsync(
                        new BucketExistsArgs().WithBucket(bucketName), cancellationToken: stoppingToken);

                    if (!bucketExists)
                    {
                        _logger.LogInformation("Bucket '{Bucket}' does not exist. Creating...", bucketName);
                        await _minioClient.MakeBucketAsync(
                            new MakeBucketArgs().WithBucket(bucketName), cancellationToken: stoppingToken);
                        _logger.LogInformation("Bucket '{Bucket}' created successfully.", bucketName);
                    }
                    else
                    {
                        _logger.LogInformation("Bucket '{Bucket}' already exists. No need to create.", bucketName);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error initializing the MinIO bucket.");
                }
            }
        }
    }
}
