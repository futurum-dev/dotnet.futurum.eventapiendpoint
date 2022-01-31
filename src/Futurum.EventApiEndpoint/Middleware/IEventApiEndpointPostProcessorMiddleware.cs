using Futurum.Core.Result;

namespace Futurum.EventApiEndpoint.Middleware;

public interface IEventApiEndpointPostProcessorMiddleware<in TEvent>
{
    Task<Result> ExecuteAsync(TEvent @event, CancellationToken cancellationToken);
}