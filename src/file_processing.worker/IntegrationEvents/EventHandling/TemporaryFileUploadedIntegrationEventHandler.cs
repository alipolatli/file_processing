using file_processing.worker.IntegrationEvents.Events;
using file_processing.worker.Services.Abstractions;
using file_processing.worker.Storages.Abstractions;
using rabbitmq_bus.Abstracts;

namespace file_processing.worker.IntegrationEvents.EventHandling;

public sealed class TemporaryFileUploadedIntegrationEventHandler(IEventBus eventBus, IFileProcessorContext fileProcessorContext, IStorage storage) : IIntegrationEventHandler<TemporaryFileUploadedIntegrationEvent>
{
    public async Task Handle(TemporaryFileUploadedIntegrationEvent @event)
    {
        using var fileStream = await storage.GetObjectAsync(@event.Bucket, @event.Object);

        var processed = await fileProcessorContext.ProcessAsync(fileStream, @event.ContentType);

        var fileMeta = await storage.PutObjectAsync(processed.Stream, "bucket", @event.Object);

        eventBus.Publish(new ProcessedFileUploadedIntegrationEvent(fileMeta.Bucket, fileMeta.Object));
    }
}