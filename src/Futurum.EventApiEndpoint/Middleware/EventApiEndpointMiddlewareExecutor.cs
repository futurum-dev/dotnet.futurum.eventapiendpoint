using Futurum.Core.Result;

namespace Futurum.EventApiEndpoint.Middleware;

public interface IEventApiEndpointMiddlewareExecutor<TEvent>
{
    Task<Result> ExecuteAsync(TEvent @event, Func<TEvent, CancellationToken, Task<Result>> apiEndpointFunc, CancellationToken cancellationToken);
}

public class EventApiEndpointMiddlewareExecutor<TEvent> : IEventApiEndpointMiddlewareExecutor<TEvent>
{
    private readonly IEventApiEndpointMiddleware<TEvent>[] _middleware;

    public EventApiEndpointMiddlewareExecutor(IEnumerable<IEventApiEndpointMiddleware<TEvent>> middleware)
    {
        _middleware = middleware.Reverse().ToArray();
    }

    public Task<Result> ExecuteAsync(TEvent @event, Func<TEvent, CancellationToken, Task<Result>> apiEndpointFunc, CancellationToken cancellationToken)
    {
        if (_middleware.Length == 0)
            return apiEndpointFunc(@event, cancellationToken);

        return _middleware.Aggregate(apiEndpointFunc, (next, middleware) => (e, ct) => middleware.ExecuteAsync(e, next, ct))(@event, cancellationToken);
    }
}