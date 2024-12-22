using file_processing.api.IntegrationEvents.EventHandling;
using file_processing.api.IntegrationEvents.Events;
using file_processing_helper.Extensions;
using Microsoft.AspNetCore.Mvc;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;
using rabbitmq_bus.Abstracts;
using rabbitmq_bus.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.AddStorage();

builder.AddRabbitMqEventBus()
    .AddSubscription<ProcessedFileUploadedIntegrationEvent, ProcessedFileUploadedIntegrationEventHandler>();

#region Deleted
//builder.Services.AddMinio(configureClient => configureClient
//    .WithEndpoint("localhost:9000")
//    .WithCredentials("ROOTUSER", "CHANGEME123")
//    .WithSSL(false));

//builder.Services.AddSingleton<IEventBusSubscriptionsManager, InMemoryEventBusSubscriptionsManager>();

//builder.Services.AddSingleton<IRabbitMQPersistentConnection>(sp =>
//{
//    var logger = sp.GetRequiredService<ILogger<DefaultRabbitMQPersistentConnection>>();

//    var factory = new ConnectionFactory()
//    {
//        HostName = "localhost",
//        DispatchConsumersAsync = true,
//        UserName = "user",
//        Password = "password"
//    };

//    var retryCount = 5;

//    return new DefaultRabbitMQPersistentConnection(factory, logger, retryCount);
//});

//builder.Services.AddSingleton<IEventBus, EventBusRabbitMQ>(sp =>
//{
//    var subscriptionClientName = "fileprocessingapi";
//    var rabbitMQPersistentConnection = sp.GetRequiredService<IRabbitMQPersistentConnection>();
//    var logger = sp.GetRequiredService<ILogger<EventBusRabbitMQ>>();
//    var eventBusSubscriptionsManager = sp.GetRequiredService<IEventBusSubscriptionsManager>();
//    var retryCount = 5;

//    return new EventBusRabbitMQ(rabbitMQPersistentConnection, logger, sp, eventBusSubscriptionsManager, subscriptionClientName, retryCount);
//});

//builder.Services.AddTransient<IIntegrationEventHandler<ProcessedFileUploadedIntegrationEvent>, ProcessedFileUploadedIntegrationEventHandler>();

#endregion

var app = builder.Build();

var eventBus = app.Services.GetRequiredService<IEventBus>();

eventBus.Subscribe<ProcessedFileUploadedIntegrationEvent, IIntegrationEventHandler<ProcessedFileUploadedIntegrationEvent>>();

app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("upload", async (
     IFormFile formFile,
    [FromServices] IEventBus eventBus,
    [FromServices] IMinioClient minioClient) =>
{
    if (formFile is null || formFile.Length == 0)
    {
        return Results.BadRequest("The uploaded file is invalid or empty.");
    }

    try
    {
        var bucketName = "temp-bucket";
        var objectName = DateTime.Now.ToString("yyyyMMddHHmmss");

        var bucketExistsArgs = new BucketExistsArgs().WithBucket(bucketName);
        var bucketExists = await minioClient.BucketExistsAsync(bucketExistsArgs);

        if (!bucketExists)
        {
            var makeBucketArgs = new MakeBucketArgs().WithBucket(bucketName);
            await minioClient.MakeBucketAsync(makeBucketArgs);
        }

        using var stream = formFile.OpenReadStream();
        var putObjectArgs = new PutObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectName)
            .WithStreamData(stream)
            .WithObjectSize(stream.Length)
            .WithContentType(formFile.ContentType);

        await minioClient.PutObjectAsync(putObjectArgs);

        eventBus.Publish(new TemporaryFileUploadedIntegrationEvent(bucketName, objectName, formFile.ContentType));

        UploadStatusStore.UploadStatuses.Add($"{objectName}_{UploadStatusStore.TEMP}", (bucketName, objectName));

        return Results.Accepted($"/upload-requests/{objectName}", new { Id = objectName });
    }
    catch (MinioException)
    {
        return Results.Problem("An error occurred while uploading the file to the object storage.", statusCode: 500);
    }
    catch (Exception)
    {
        return Results.Problem("An unexpected error occurred. Please try again later.", statusCode: 500);
    }
}).DisableAntiforgery();

app.MapGet("upload-requests/{id}", ([FromRoute] string id) =>
{
    if (UploadStatusStore.UploadStatuses.TryGetValue($"{id}_{UploadStatusStore.PROCESSED}", out var status))
    {
        return Results.Ok("The upload request has been processed successfully.");
    }

    return Results.Ok("The upload request is still in the temporary state. Not yet processed.");
});

app.MapGet("download/{id}", ([FromRoute] Guid id, [FromServices] IMinioClient minioClient) =>
{

});

app.Run();

public static class UploadStatusStore
{
    public const string TEMP = "temp";
    public const string PROCESSED = "processed";
    public static readonly Dictionary<string, (string Bucket, string Object)> UploadStatuses = new();
}