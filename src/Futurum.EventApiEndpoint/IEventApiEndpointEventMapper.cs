using Futurum.Core.Result;

namespace Futurum.EventApiEndpoint;

public interface IEventApiEndpointEventMapper<TEventDto, TEvent>
{
    Result<TEvent> Map(TEventDto dto);
}