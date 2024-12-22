using rabbitmq_bus;

namespace file_processing.worker.IntegrationEvents.Events;

public sealed record ProcessedFileUploadedIntegrationEvent(string Bucket, string Object) : IntegrationEvent;