using Futurum.Core.Result;

namespace Futurum.EventApiEndpoint.Middleware;

public class EventApiEndpointPostProcessorMiddleware<TEvent> : IEventApiEndpointMiddleware<TEvent>
{
    private readonly IEventApiEndpointPostProcessorMiddleware<TEvent>[] _middleware;

    public EventApiEndpointPostProcessorMiddleware(IEnumerable<IEventApiEndpointPostProcessorMiddleware<TEvent>> middleware)
    {
        _middleware = middleware.ToArray();
    }

    public Task<Result> ExecuteAsync(TEvent @event, Func<TEvent, CancellationToken, Task<Result>> next, CancellationToken cancellationToken)
    {
        Task<Result> ExecuteNext() =>
            next(@event, cancellationToken);

        Task<Result> ExecuteMiddleware(IEventApiEndpointPostProcessorMiddleware<TEvent> middleware) =>
            middleware.ExecuteAsync(@event, cancellationToken);

        if (_middleware.Length == 0)
            return next(@event, cancellationToken);

        return ExecuteNext().ThenAsync(() => _middleware.FlatMapSequentialUntilFailureAsync(ExecuteMiddleware));
    }
}