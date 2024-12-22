using Microsoft.Extensions.Hosting;
using Minio;

namespace file_processing_helper.Extensions;

public static class StorageBuilderExtensions
{
    public static IHostApplicationBuilder AddStorage(this IHostApplicationBuilder builder)
    {
        var minioSection = builder.Configuration.GetSection("Minio");

        builder.Services.AddMinio(configureClient => configureClient
            .WithEndpoint(minioSection["Endpoint"])
            .WithCredentials(minioSection["AccessKey"], minioSection["SecretKey"])
            .WithSSL(false));

        return builder;
    }
}
