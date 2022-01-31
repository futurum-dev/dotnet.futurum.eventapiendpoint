using System.Text.Json;

using Futurum.ApiEndpoint;
using Futurum.Core.Result;

namespace Futurum.EventApiEndpoint;

public interface IEventApiEndpointExecutorService
{
    Task<Result> ExecuteAsync(IMetadataDefinition metadataDefinition, string eventPayload, CancellationToken cancellationToken);
}

public class EventApiEndpointExecutorService<TEventDto, TEvent> : IEventApiEndpointExecutorService
{
    private readonly IEventApiEndpointDispatcher<TEventDto, TEvent> _dispatcher;

    public EventApiEndpointExecutorService(IEventApiEndpointDispatcher<TEventDto, TEvent> dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public Task<Result> ExecuteAsync(IMetadataDefinition metadataDefinition, string eventPayload, CancellationToken cancellationToken)
    {
        Task<Result> Execute() =>
            DeserializeEventDto(eventPayload)
                .ThenAsync(eventDto => _dispatcher.ExecuteAsync(metadataDefinition, eventDto, cancellationToken))
                .ToNonGenericAsync();

        return Result.TryAsync(Execute, () => $"EventApiEndpoint error, for '{metadataDefinition}'");
    }

    private static Result<TEventDto> DeserializeEventDto(string message)
    {
        Result<TEventDto> Execute()
        {
            switch (message.Length)
            {
                case > 0:
                {
                    var eventDto = JsonSerializer.Deserialize<TEventDto>(message);

                    if (eventDto == null)
                    {
                        return Result.Fail<TEventDto>($"Failed to deserialize EventDto payload");
                    }

                    return eventDto.ToResultOk();
                }
                default:
                {
                    return Result.Fail<TEventDto>($"No EventDto payload to deserialize");
                }
            }
        }

        return Result.Try(Execute, () => $"Failed to deserialize EventDto : '{message}'");
    }
}