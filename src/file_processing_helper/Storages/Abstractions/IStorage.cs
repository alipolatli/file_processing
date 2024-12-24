namespace file_processing_helper.Storages.Abstractions;

public interface IStorage
{
    Task<MemoryStream> GetObjectAsync(string bucketName, string objectName, CancellationToken cancellationToken = default);
    Task PutObjectAsync(Stream stream, string bucketName, string objectName, CancellationToken cancellationToken = default);
    Task<string> GetFileLinkAsync(string bucketName, string fileName, CancellationToken cancellationToken = default);
}