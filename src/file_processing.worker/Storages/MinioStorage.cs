using file_processing.worker.Storages.Abstractions;
using Minio;
using Minio.DataModel;
using Minio.DataModel.Args;
using Minio.Exceptions;

namespace file_processing.worker.Storages;

public class MinioStorage(IMinioClient minioClient) : IStorage
{
    public async Task<Stream> GetObjectAsync(string bucketName, string objectName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(bucketName))
        {
            throw new ArgumentException("Bucket name cannot be null or empty.", nameof(bucketName));
        }

        if (string.IsNullOrWhiteSpace(objectName))
        {
            throw new ArgumentException("Object name cannot be null or empty.", nameof(objectName));
        }

        try
        {
            var memoryStream = new MemoryStream();

            var getObjectArgs = new GetObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName)
                .WithCallbackStream(async (stream) =>
                {
                    await stream.CopyToAsync(memoryStream, cancellationToken);
                    memoryStream.Position = 0;
                });

            await minioClient.GetObjectAsync(getObjectArgs, cancellationToken).ConfigureAwait(false);

            return memoryStream;
        }
        catch (MinioException ex)
        {
            throw new InvalidOperationException("An error occurred while retrieving the object from Minio.", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An unexpected error occurred while retrieving the object.", ex);
        }
    }

    public async Task<(string Bucket, string Object)> PutObjectAsync(
        Stream stream,
        string bucketName,
        string objectName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!await DoesBucketExistAsync(bucketName, cancellationToken))
            {
                await CreateBucketAsync(bucketName, cancellationToken);
            }

            var putObjectArgs = new PutObjectArgs()
                .WithBucket(bucketName)
                .WithObject(objectName)
                .WithObjectSize(stream.Length)
                .WithStreamData(stream);

            var putObjectResponse = await minioClient.PutObjectAsync(putObjectArgs, cancellationToken);

            return (bucketName, objectName);
        }
        catch (MinioException ex)
        {
            throw new InvalidOperationException("An error occurred while interacting with the Minio service.", ex);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An unexpected error occurred during the object upload process.", ex);
        }
    }

    private async Task<bool> DoesBucketExistAsync(string bucketName, CancellationToken cancellationToken)
    {
        var bucketExistsArgs = new BucketExistsArgs().WithBucket(bucketName);
        return await minioClient.BucketExistsAsync(bucketExistsArgs, cancellationToken);
    }

    private async Task CreateBucketAsync(string bucketName, CancellationToken cancellationToken)
    {
        var makeBucketArgs = new MakeBucketArgs().WithBucket(bucketName);
        await minioClient.MakeBucketAsync(makeBucketArgs, cancellationToken);
    }
}
