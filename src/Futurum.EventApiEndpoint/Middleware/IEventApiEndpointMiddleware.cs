using Futurum.Core.Result;

namespace Futurum.EventApiEndpoint.Middleware;

public interface IEventApiEndpointMiddleware<TEvent>
{
    Task<Result> ExecuteAsync(TEvent @event, Func<TEvent, CancellationToken, Task<Result>> next, CancellationToken cancellationToken);
}