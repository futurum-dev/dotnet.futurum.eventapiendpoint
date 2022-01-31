namespace Futurum.EventApiEndpoint.Metadata;

public static class EventApiEndpointMetadataTypeService
{
    public static MetadataTypeDefinition Get(Type apiEndpointInterfaceType, Type apiEndpointType)
    {
        var eventDtoType = apiEndpointInterfaceType?.GetGenericArguments()[0];
        var eventType = apiEndpointInterfaceType?.GetGenericArguments()[1];

        var apiEndpointExecutorServiceType = typeof(EventApiEndpointExecutorService<,>).MakeGenericType(eventDtoType, eventType);

        return new MetadataTypeDefinition(apiEndpointExecutorServiceType);
    }
}