namespace file_processing_helper.Storages.Abstractions;

public interface IStorage
{
    Task<Stream> GetObjectAsync(string bucketName, string objectName, CancellationToken cancellationToken = default);
    Task<(string Bucket, string Object)> PutObjectAsync(Stream stream, string bucketName, string objectName, CancellationToken cancellationToken = default);
}