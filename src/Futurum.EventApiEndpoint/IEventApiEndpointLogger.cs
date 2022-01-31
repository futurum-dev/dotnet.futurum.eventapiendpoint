namespace Futurum.EventApiEndpoint;

public interface IEventApiEndpointLogger
{
    void EventReceived<TEvent>(TEvent @event);
}