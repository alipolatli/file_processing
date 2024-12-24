using file_processing.api.IntegrationEvents.EventHandling;
using file_processing.api.IntegrationEvents.Events;
using file_processing_helper.Extensions;
using file_processing_helper.Storages.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Minio.Exceptions;
using rabbitmq_bus.Abstracts;
using rabbitmq_bus.Extensions;
using System.ComponentModel.DataAnnotations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.AddStorage();

builder.AddRabbitMqEventBus()
    .AddSubscription<ProcessedFileUploadedIntegrationEvent, ProcessedFileUploadedIntegrationEventHandler>();

var app = builder.Build();

var eventBus = app.Services.GetRequiredService<IEventBus>();

eventBus.Subscribe<ProcessedFileUploadedIntegrationEvent, IIntegrationEventHandler<ProcessedFileUploadedIntegrationEvent>>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseExceptionHandler();

app.MapPost("upload", async (
    [Required] IFormFile formFile,
    [FromServices] IEventBus eventBus,
    [FromServices] IStorage storage,
    CancellationToken cancellationToken) =>
{
    if (formFile is null || formFile.Length == 0)
    {
        return Results.BadRequest("The uploaded file is invalid or empty.");
    }

    var bucketName = "temp-bucket";
    var objectName = DateTime.Now.ToString("yyyyMMddHHmmss");

    using var stream = formFile.OpenReadStream();
    await storage.PutObjectAsync(stream, bucketName, objectName, cancellationToken);

    eventBus.Publish(new TemporaryFileUploadedIntegrationEvent(bucketName, objectName, formFile.GetFileType()));

    UploadStatusStore.UploadStatuses.Add($"{objectName}_{UploadStatusStore.TEMP}", (bucketName, objectName));

    return Results.Accepted($"/upload-requests/{objectName}", new { Id = objectName });

}).DisableAntiforgery();


app.MapGet("upload-requests/{id}", ([FromRoute] string id, CancellationToken cancellationToken) =>
{
    if (UploadStatusStore.UploadStatuses.TryGetValue($"{id}_{UploadStatusStore.PROCESSED}", out var _))
    {
        return Results.Ok("The upload request has been processed successfully.");
    }

    return Results.Ok("The upload request is still in the temporary state. Not yet processed.");
});

app.MapGet("download/{id}", async ([FromRoute] string id, [FromServices] IStorage storage, CancellationToken cancellationToken) =>
{
    if (UploadStatusStore.UploadStatuses.TryGetValue($"{id}_{UploadStatusStore.PROCESSED}", out var data))
    {
        var link = await storage.GetFileLinkAsync(data.Bucket, data.Object, cancellationToken);
        return Results.Ok(link);
    }
    return Results.NotFound();
});

app.Run();

public static class UploadStatusStore
{
    public const string TEMP = "temp";
    public const string PROCESSED = "processed";
    public static readonly Dictionary<string, (string Bucket, string Object)> UploadStatuses = new();
}
