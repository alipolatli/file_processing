namespace rabbitmq_bus.Abstracts;

public interface IIntegrationEventHandler<TIntegrationEvent> where TIntegrationEvent : IntegrationEvent
{
    Task Handle(TIntegrationEvent @event);
}

