using file_processing.worker;
using file_processing.worker.IntegrationEvents.EventHandling;
using file_processing.worker.IntegrationEvents.Events;
using file_processing.worker.Services;
using file_processing.worker.Services.Abstractions;
using file_processing.worker.Storages;
using file_processing.worker.Storages.Abstractions;
using file_processing_helper.Extensions;
using rabbitmq_bus.Abstracts;
using rabbitmq_bus.Extensions;

var builder = Host.CreateApplicationBuilder(args);

builder.AddStorage();

builder.AddRabbitMqEventBus()
    .AddSubscription<TemporaryFileUploadedIntegrationEvent, TemporaryFileUploadedIntegrationEventHandler>();

builder.Services.AddSingleton<IStorage, MinioStorage>();

builder.Services.AddSingleton<IFileProcessorContext, FileProcessorContext>();

builder.Services.AddKeyedSingleton<IFileProcessor, ImageProcessor>("image/png");

builder.Services.AddKeyedSingleton<IFileProcessor, VideoProcessor>("video");

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
//    var subscriptionClientName = "fileprocessingworker";
//    var rabbitMQPersistentConnection = sp.GetRequiredService<IRabbitMQPersistentConnection>();
//    var logger = sp.GetRequiredService<ILogger<EventBusRabbitMQ>>();
//    var eventBusSubscriptionsManager = sp.GetRequiredService<IEventBusSubscriptionsManager>();
//    var retryCount = 5;

//    return new EventBusRabbitMQ(rabbitMQPersistentConnection, logger, sp, eventBusSubscriptionsManager, subscriptionClientName, retryCount);
//});

//builder.Services.AddTransient<IIntegrationEventHandler<TemporaryFileUploadedIntegrationEvent>, TemporaryFileUploadedIntegrationEventHandler>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();

var eventBus = host.Services.GetRequiredService<IEventBus>();

eventBus.Subscribe<TemporaryFileUploadedIntegrationEvent, IIntegrationEventHandler<TemporaryFileUploadedIntegrationEvent>>();

host.Run();