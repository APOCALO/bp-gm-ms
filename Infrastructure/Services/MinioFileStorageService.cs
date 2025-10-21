using Application.Interfaces;
using ErrorOr;
using Minio;
using Minio.DataModel.Args;

namespace Infrastructure.Services
{
    public class MinioFileStorageService : IFileStorageService
    {
        private readonly IMinioClient _minioClient;

        public MinioFileStorageService(IMinioClient minioClient)
        {
            _minioClient = minioClient;
        }

        public async Task<ErrorOr<bool>> UploadFileAsync(
            string bucketName,
            string objectName,
            string filePath,
            string contentType)
        {
            if (!File.Exists(filePath))
                return Error.NotFound("UploadFileAsync.FileNotFound", $"The file '{filePath}' does not exist.");

            bool found = await _minioClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucketName));
            if (!found)
                return Error.Validation("UploadFileAsync.BucketNotFound", $"The bucket '{bucketName}' does not exist.");

            await _minioClient.PutObjectAsync(new PutObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName)
                .WithFileName(filePath)
                .WithContentType(contentType));

            return true;
        }

        public async Task<ErrorOr<bool>> UploadFileAsync(
            string bucketName,
            string objectName,
            Stream fileStream,
            string contentType)
        {
            if (fileStream == null || !fileStream.CanRead)
                return Error.Validation("UploadFileAsync.InvalidStream", "The provided stream is null or unreadable.");

            if (fileStream.CanSeek)
                fileStream.Seek(0, SeekOrigin.Begin);

            bool found = await _minioClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucketName));
            if (!found)
                return Error.Validation("UploadFileAsync.BucketNotFound", $"The bucket '{bucketName}' does not exist.");

            await _minioClient.PutObjectAsync(new PutObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName)
                .WithStreamData(fileStream)
                .WithObjectSize(fileStream.Length)
                .WithContentType(contentType));

            return true;
        }

        // Generar URL temporal para un archivo ya subido
        public async Task<ErrorOr<string>> GetFileUrlAsync(
            string bucketName,
            string objectName,
            int expirySeconds = 3600)
        {
            bool bucketExists = await _minioClient.BucketExistsAsync(
                new BucketExistsArgs().WithBucket(bucketName)
            );

            if (!bucketExists)
                return Error.NotFound("GetFileUrlAsync.BucketNotFound", $"Bucket '{bucketName}' does not exist.");

            try
            {
                string presignedUrl = await _minioClient.PresignedGetObjectAsync(
                    new PresignedGetObjectArgs()
                        .WithBucket(bucketName)
                        .WithObject(objectName)
                        .WithExpiry(expirySeconds)
                );

                return presignedUrl;
            }
            catch (Exception ex)
            {
                return Error.Failure("GetFileUrlAsync.MinioError", $"Failed to generate URL: {ex.Message}");
            }
        }

        public async Task<ErrorOr<bool>> DeleteFileAsync(string bucketName, string objectName)
        {
            try
            {
                bool bucketExists = await _minioClient.BucketExistsAsync(
                    new BucketExistsArgs().WithBucket(bucketName)
                );

                if (!bucketExists)
                    return Error.NotFound("DeleteFileAsync.BucketNotFound", $"Bucket '{bucketName}' does not exist.");

                // Verifica si el objeto existe (opcional pero recomendado)
                try
                {
                    await _minioClient.StatObjectAsync(
                        new StatObjectArgs().WithBucket(bucketName).WithObject(objectName)
                    );
                }
                catch (Exception ex)
                {
                    return Error.NotFound("DeleteFileAsync.ObjectNotFound", $"Object '{objectName}' not found: {ex.Message}");
                }

                await _minioClient.RemoveObjectAsync(
                    new RemoveObjectArgs()
                        .WithBucket(bucketName)
                        .WithObject(objectName)
                );

                return true;
            }
            catch (Exception ex)
            {
                return Error.Failure("DeleteFileAsync.MinioError", $"An error occurred while deleting the file: {ex.Message}");
            }
        }

    }
}
