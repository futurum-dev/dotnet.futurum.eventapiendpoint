using Futurum.ApiEndpoint;

namespace Futurum.EventApiEndpoint;

public static class ApiEndpointDefinitionWebExtensions
{
    public static EventApiEndpointDefinition Event(this ApiEndpointDefinitionBuilder apiEndpointDefinitionBuilder)
    {
        var eventApiEndpointDefinition = new EventApiEndpointDefinition();

        apiEndpointDefinitionBuilder.Add(eventApiEndpointDefinition);

        return eventApiEndpointDefinition;
    }
}