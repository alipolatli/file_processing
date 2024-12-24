using file_processing_helper.Storages.Abstractions;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;

namespace file_processing_helper.Storages;

public class MinioStorage(IMinioClient minioClient) : IStorage
{
    public async Task<MemoryStream> GetObjectAsync(string bucketName, string objectName, CancellationToken cancellationToken = default)
    {
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
            throw new InvalidOperationException("An error occurred while interacting with the Minio service.", ex);
        }
    }

    public async Task PutObjectAsync(Stream stream, string bucketName, string objectName, CancellationToken cancellationToken = default)
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

            _ = await minioClient.PutObjectAsync(putObjectArgs, cancellationToken);
        }
        catch (MinioException ex)
        {
            throw new InvalidOperationException("An error occurred while interacting with the Minio service.", ex);
        }
    }

    public async Task<string> GetFileLinkAsync(string bucketName, string fileName, CancellationToken cancellationToken = default)
    {
        try
        {
            return await minioClient.PresignedGetObjectAsync(
                    new PresignedGetObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(fileName)
                    .WithExpiry(3600));
        }
        catch (MinioException ex)
        {
            throw new InvalidOperationException("An error occurred while interacting with the Minio service.", ex);
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
