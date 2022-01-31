using Futurum.Core.Result;

namespace Futurum.EventApiEndpoint.Middleware;

public class EventApiEndpointPreProcessorMiddleware<TEvent> : IEventApiEndpointMiddleware<TEvent>
{
    private readonly IEventApiEndpointPreProcessorMiddleware<TEvent>[] _middleware;

    public EventApiEndpointPreProcessorMiddleware(IEnumerable<IEventApiEndpointPreProcessorMiddleware<TEvent>> middleware)
    {
        _middleware = middleware.ToArray();
    }

    public Task<Result> ExecuteAsync(TEvent @event, Func<TEvent, CancellationToken, Task<Result>> next, CancellationToken cancellationToken)
    {
        Task<Result> ExecuteMiddleware(IEventApiEndpointPreProcessorMiddleware<TEvent> middleware) =>
            middleware.ExecuteAsync(@event, cancellationToken);

        if (_middleware.Length == 0)
            return next(@event, cancellationToken);

        return _middleware.FlatMapSequentialUntilFailureAsync(ExecuteMiddleware)
                          .ThenAsync(() => next(@event, cancellationToken));
    }
}