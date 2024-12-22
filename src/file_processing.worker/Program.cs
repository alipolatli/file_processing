using file_processing.worker;
using file_processing.worker.IntegrationEvents.EventHandling;
using file_processing.worker.IntegrationEvents.Events;
using file_processing_helper.Extensions;
using rabbitmq_bus.Abstracts;
using rabbitmq_bus.Extensions;

var builder = Host.CreateApplicationBuilder(args);

builder.AddStorage();

builder.AddRabbitMqEventBus()
    .AddSubscription<TemporaryFileUploadedIntegrationEvent, TemporaryFileUploadedIntegrationEventHandler>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();

var eventBus = host.Services.GetRequiredService<IEventBus>();

eventBus.Subscribe<TemporaryFileUploadedIntegrationEvent, IIntegrationEventHandler<TemporaryFileUploadedIntegrationEvent>>();

host.Run();