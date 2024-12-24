using file_processing.worker.IntegrationEvents.Events;
using file_processing_helper.Services.Abstractions;
using file_processing_helper.Storages.Abstractions;
using rabbitmq_bus.Abstracts;

namespace file_processing.worker.IntegrationEvents.EventHandling;

public sealed class TemporaryFileUploadedIntegrationEventHandler(IEventBus eventBus, IFileProcessorContext fileProcessorContext, IStorage storage) : IIntegrationEventHandler<TemporaryFileUploadedIntegrationEvent>
{
    private const string BUCKET = "processed-bucket";
    public async Task Handle(TemporaryFileUploadedIntegrationEvent @event)
    {
        using var fileStream = await storage.GetObjectAsync(@event.Bucket, @event.Object);

        var processed = await fileProcessorContext.ProcessAsync(fileStream, @event.ContentType);

        await storage.PutObjectAsync(processed.Stream, BUCKET, @event.Object);

        eventBus.Publish(new ProcessedFileUploadedIntegrationEvent(BUCKET, @event.Object));
    }
}