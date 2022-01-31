using Futurum.Core.Result;

namespace Futurum.EventApiEndpoint.Middleware;

public class DisabledEventApiEndpointMiddlewareExecutor<TEvent> : IEventApiEndpointMiddlewareExecutor<TEvent>
{
    public Task<Result> ExecuteAsync(TEvent @event, Func<TEvent, CancellationToken, Task<Result>> apiEndpointFunc, CancellationToken cancellationToken) =>
        apiEndpointFunc(@event, cancellationToken);
}