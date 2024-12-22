using rabbitmq_bus;

namespace file_processing.worker.IntegrationEvents.Events;

public sealed record TemporaryFileUploadedIntegrationEvent(string Bucket, string Object, string ContentType) : IntegrationEvent;
