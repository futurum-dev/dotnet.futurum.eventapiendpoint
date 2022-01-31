using Futurum.Core.Result;
using Futurum.EventApiEndpoint.Metadata;
using Futurum.Microsoft.Extensions.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;

namespace Futurum.EventApiEndpoint;

public static class Batch
{
    public record EnvelopeEventDto(string Route, string Payload);

    public record EnvelopeEvent(MetadataRoute Route, string Payload);

    public record EventDto(ICollection<EnvelopeEventDto> EnvelopeEvents);

    public record Event(IEnumerable<EnvelopeEvent> EnvelopeEvents);

    public class ApiEndpoint : IEventApiEndpoint<EventDto, Event>
    {
        private readonly EventApiEndpointConfiguration _configuration;
        private readonly IEventApiEndpointMetadataCache _metadataCache;
        private readonly IServiceProvider _serviceProvider;

        public ApiEndpoint(EventApiEndpointConfiguration configuration,
                           IEventApiEndpointMetadataCache metadataCache,
                           IServiceProvider serviceProvider)
        {
            _configuration = configuration;
            _metadataCache = metadataCache;
            _serviceProvider = serviceProvider;
        }

        public Task<Result> ExecuteAsync(Event @event, CancellationToken cancellationToken) =>
            @event.EnvelopeEvents.Count() switch
            {
                0 => Result.OkAsync(),
                1 => ProcessEnvelopeEventAsync(@event.EnvelopeEvents.Single(), cancellationToken),
                _ => @event.EnvelopeEvents.FlatMapAsync(_configuration.BatchParallelOptions, envelopeEvent => ProcessEnvelopeEventAsync(envelopeEvent, cancellationToken))
            };

        private Task<Result> ProcessEnvelopeEventAsync(EnvelopeEvent @event, CancellationToken cancellationToken) =>
            FilterEventDefinitions(@event)
                .FlatMap(GetExecutorWithDefinition)
                .ThenAsync(executorWithDefinitions => CallApiEndpointExecutorService(@event, executorWithDefinitions, cancellationToken))
                .ToNonGenericAsync();

        private IEnumerable<MetadataEnvelopeEventDefinition> FilterEventDefinitions(EnvelopeEvent @event) =>
            _metadataCache.GetMetadataEnvelopeEventDefinitions()
                          .Where(metadataEnvelopeEventDefinition => metadataEnvelopeEventDefinition.MetadataSubscriptionEventDefinition.Route == @event.Route);

        private Result<ExecutorWithDefinition> GetExecutorWithDefinition(MetadataEnvelopeEventDefinition metadataEnvelopeEventDefinition)
        {
            Result<ExecutorWithDefinition> Execute()
            {
                using var scope = _serviceProvider.CreateScope();

                return scope.ServiceProvider.TryGetService<IEventApiEndpointExecutorService>(metadataEnvelopeEventDefinition.MetadataTypeDefinition.EventApiEndpointExecutorServiceType)
                            .Map(apiEndpointExecutorService => new ExecutorWithDefinition(metadataEnvelopeEventDefinition, apiEndpointExecutorService));
            }

            return Result.Try(Execute, () => $"Failed to resolve Type : '{metadataEnvelopeEventDefinition.MetadataTypeDefinition.EventApiEndpointExecutorServiceType.FullName}' in new Scope");
        }

        private Task<Result> CallApiEndpointExecutorService(EnvelopeEvent envelopeEvent, IEnumerable<ExecutorWithDefinition> executorWithDefinitions, CancellationToken cancellationToken)
        {
            switch (executorWithDefinitions.Count())
            {
                case 0:
                    return Result.OkAsync();
                case 1:
                {
                    var (metadataEnvelopeEventDefinition, eventApiEndpointExecutorService) = executorWithDefinitions.Single();

                    return eventApiEndpointExecutorService.ExecuteAsync(metadataEnvelopeEventDefinition.MetadataSubscriptionEventDefinition, envelopeEvent.Payload, cancellationToken);
                }
                default:
                    return executorWithDefinitions.FlatMapAsync(_configuration.BatchParallelOptions,
                                                                executorWithDefinition =>
                                                                {
                                                                    var (metadataEnvelopeEventDefinition, eventApiEndpointExecutorService) = executorWithDefinition;

                                                                    return eventApiEndpointExecutorService.ExecuteAsync(metadataEnvelopeEventDefinition.MetadataSubscriptionEventDefinition,
                                                                                                                        envelopeEvent.Payload,
                                                                                                                        cancellationToken);
                                                                });
            }
        }

        private record struct ExecutorWithDefinition(MetadataEnvelopeEventDefinition MetadataEnvelopeEventDefinition, IEventApiEndpointExecutorService ApiEndpointExecutorService);
    }

    public class Mapper : IEventApiEndpointEventMapper<EventDto, Event>
    {
        public Result<Event> Map(EventDto dto) =>
            new Event(dto.EnvelopeEvents.Select(x => new EnvelopeEvent(new MetadataRoute(x.Route), x.Payload))).ToResultOk();
    }
}