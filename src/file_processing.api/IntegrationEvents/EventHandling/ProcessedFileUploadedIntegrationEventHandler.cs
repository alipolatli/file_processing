using file_processing.api.IntegrationEvents.Events;
using rabbitmq_bus.Abstracts;

namespace file_processing.api.IntegrationEvents.EventHandling;

public sealed class ProcessedFileUploadedIntegrationEventHandler : IIntegrationEventHandler<ProcessedFileUploadedIntegrationEvent>
{
    public Task Handle(ProcessedFileUploadedIntegrationEvent @event)
    {
        UploadStatusStore.UploadStatuses.Add($"{@event.Object}_{UploadStatusStore.PROCESSED}", (@event.Bucket, @event.Object));

        return Task.CompletedTask;
    }
}