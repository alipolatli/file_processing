using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using rabbitmq_bus.Abstracts;

namespace rabbitmq_bus.Extensions;

public static class EventBusBuilderExtensions
{
    public static IEventBusBuilder AddRabbitMqEventBus(this IHostApplicationBuilder builder)
    {
        var eventBusSection = builder.Configuration.GetSection("EventBus");

        var x = eventBusSection["HostName"];

        builder.Services.AddSingleton<IEventBusSubscriptionsManager, InMemoryEventBusSubscriptionsManager>();

        builder.Services.AddSingleton<IRabbitMQPersistentConnection>(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<DefaultRabbitMQPersistentConnection>>();

            var factory = new ConnectionFactory()
            {
                HostName = eventBusSection["HostName"],
                DispatchConsumersAsync = true,
                UserName = eventBusSection["UserName"],
                Password = eventBusSection["Password"],
            };

            var retryCount = Convert.ToInt32(eventBusSection["RetryCount"]);

            return new DefaultRabbitMQPersistentConnection(factory, logger, retryCount);
        });

        builder.Services.AddSingleton<IEventBus, EventBusRabbitMQ>(sp =>
        {
            var subscriptionClientName = eventBusSection["SubscriptionClientName"];
            var rabbitMQPersistentConnection = sp.GetRequiredService<IRabbitMQPersistentConnection>();
            var logger = sp.GetRequiredService<ILogger<EventBusRabbitMQ>>();
            var eventBusSubscriptionsManager = sp.GetRequiredService<IEventBusSubscriptionsManager>();
            var retryCount = 5;

            return new EventBusRabbitMQ(rabbitMQPersistentConnection, logger, sp, eventBusSubscriptionsManager, subscriptionClientName, retryCount);
        });

        return new EventBusBuilder(builder.Services);
    }

    public static IEventBusBuilder AddSubscription<T, TH>(this IEventBusBuilder eventBusBuilder)
        where T : IntegrationEvent
        where TH : class, IIntegrationEventHandler<T>
    {
        eventBusBuilder.Services.AddTransient<IIntegrationEventHandler<T>, TH>();

        return eventBusBuilder;
    }

    private class EventBusBuilder(IServiceCollection services) : IEventBusBuilder
    {
        public IServiceCollection Services => services;
    }
}

public interface IEventBusBuilder
{
    public IServiceCollection Services { get; }
}

