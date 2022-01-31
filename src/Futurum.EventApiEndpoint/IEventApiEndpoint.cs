using Futurum.Core.Result;

namespace Futurum.EventApiEndpoint;

public interface IEventApiEndpoint
{
}

public interface IEventApiEndpoint<TEventDto, TEvent> : IEventApiEndpoint
{
    Task<Result> ExecuteAsync(TEvent @event, CancellationToken cancellationToken);
}

public interface EventApiEndpoint<TEventDto, TEvent> : IEventApiEndpoint<TEventDto, TEvent>
{
}