namespace file_processing.worker.Storages.Abstractions;

public interface IStorage
{
    Task<Stream> GetObjectAsync(string bucketName, string objectName, CancellationToken cancellationToken = default);
    Task<(string Bucket, string Object)> PutObjectAsync(Stream stream, string bucketName, string objectName, CancellationToken cancellationToken = default);
}