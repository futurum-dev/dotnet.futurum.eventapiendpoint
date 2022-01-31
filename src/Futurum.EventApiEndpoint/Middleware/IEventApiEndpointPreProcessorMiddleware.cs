using Futurum.Core.Result;

namespace Futurum.EventApiEndpoint.Middleware;

public interface IEventApiEndpointPreProcessorMiddleware<in TEvent>
{
    Task<Result> ExecuteAsync(TEvent @event, CancellationToken cancellationToken);
}