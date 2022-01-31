using Futurum.ApiEndpoint;
using Futurum.Core.Result;
using Futurum.EventApiEndpoint.Middleware;

namespace Futurum.EventApiEndpoint;

public interface IEventApiEndpointDispatcher<TEventDto, TEvent>
{
    Task<Result> ExecuteAsync(IMetadataDefinition metadataDefinition, TEventDto eventDto, CancellationToken cancellationToken);
}

public class EventApiEndpointDispatcher<TEventDto, TEvent> : IEventApiEndpointDispatcher<TEventDto, TEvent>
{
    private readonly IEventApiEndpointLogger _logger;
    private readonly IEventApiEndpointEventValidation<TEventDto> _eventValidation;
    private readonly IEventApiEndpointMiddlewareExecutor<TEvent> _middlewareExecutor;
    private readonly IEventApiEndpoint<TEventDto, TEvent> _apiEndpoint;
    private readonly IEventApiEndpointEventMapper<TEventDto, TEvent> _eventMapper;

    public EventApiEndpointDispatcher(IEventApiEndpointLogger logger,
                                      IEventApiEndpointEventValidation<TEventDto> eventValidation,
                                      IEventApiEndpointMiddlewareExecutor<TEvent> middlewareExecutor,
                                      IEventApiEndpoint<TEventDto, TEvent> apiEndpoint,
                                      IEventApiEndpointEventMapper<TEventDto, TEvent> eventMapper)
    {
        _logger = logger;
        _eventValidation = eventValidation;
        _middlewareExecutor = middlewareExecutor;
        _apiEndpoint = apiEndpoint;
        _eventMapper = eventMapper;
    }

    public Task<Result> ExecuteAsync(IMetadataDefinition metadataDefinition, TEventDto eventDto, CancellationToken cancellationToken)
    {
        Task<Result> Execute() =>
            _eventValidation.ExecuteAsync(eventDto)
                            .DoAsync(() => _logger.EventReceived(eventDto))
                            .ThenAsync(() => _eventMapper.Map(eventDto))
                            .ThenAsync(@event => _middlewareExecutor.ExecuteAsync(@event, (e, ct) => _apiEndpoint.ExecuteAsync(e, ct), cancellationToken))
                            .ToNonGenericAsync();

        return Result.TryAsync(Execute, () => $"EventApiEndpoint error, for '{metadataDefinition}'");
    }
}