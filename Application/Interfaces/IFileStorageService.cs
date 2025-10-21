using ErrorOr;

namespace Application.Interfaces
{
    public interface IFileStorageService
    {
        /// <summary>
        /// Elimina un archivo del bucket.
        /// </summary>
        /// <param name="bucketName">Nombre del bucket.</param>
        /// <param name="objectName">Ruta/nombre del archivo dentro del bucket.</param>
        /// <returns>True si fue eliminado correctamente, o un Error en caso de fallo.</returns>
        Task<ErrorOr<bool>> DeleteFileAsync(
            string bucketName,
            string objectName);


        /// <summary>
        /// Sube un archivo desde disco.
        /// </summary>
        Task<ErrorOr<bool>> UploadFileAsync(
            string bucketName,
            string objectName,
            string filePath,
            string contentType);

        /// <summary>
        /// Sube un archivo desde un Stream.
        /// </summary>
        Task<ErrorOr<bool>> UploadFileAsync(
            string bucketName,
            string objectName,
            Stream fileStream,
            string contentType);

        /// <summary>
        /// Genera una URL firmada temporal para acceder a un archivo privado.
        /// </summary>
        /// <param name="bucketName">Nombre del bucket</param>
        /// <param name="objectName">Ruta/nombre del archivo dentro del bucket</param>
        /// <param name="expirySeconds">Tiempo en segundos que la URL será válida (por defecto 1 hora)</param>
        /// <returns>Una URL firmada temporalmente o un Error</returns>
        Task<ErrorOr<string>> GetFileUrlAsync(
            string bucketName,
            string objectName,
            int expirySeconds = 3600);
    }
}
