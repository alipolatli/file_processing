using file_processing_helper.Services;
using file_processing_helper.Services.Abstractions;
using file_processing_helper.Storages;
using file_processing_helper.Storages.Abstractions;
using Microsoft.Extensions.DependencyInjection;
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


        builder.Services.AddSingleton<IStorage, MinioStorage>();

        builder.Services.AddSingleton<IFileProcessorContext, FileProcessorContext>();

        builder.Services.AddKeyedSingleton<IFileProcessor, ImageProcessor>("image/png");

        builder.Services.AddKeyedSingleton<IFileProcessor, VideoProcessor>("video");

        return builder;
    }
}
