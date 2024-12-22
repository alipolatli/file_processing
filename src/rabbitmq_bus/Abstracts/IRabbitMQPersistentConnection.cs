using RabbitMQ.Client;

namespace rabbitmq_bus.Abstracts;

public interface IRabbitMQPersistentConnection : IDisposable
{
    bool IsConnected { get; }

    bool TryConnect();

    IModel CreateModel();
}