using rabbitmq_bus;

namespace file_processing.api.IntegrationEvents.Events;

public sealed record TemporaryFileUploadedIntegrationEvent(string Bucket, string Object, string ContentType) : IntegrationEvent;
