using Futurum.ApiEndpoint;
using Futurum.ApiEndpoint.DebugLogger;

namespace Futurum.EventApiEndpoint.Metadata;

public interface IMetadataEventSubscriptionDefinitionBuilder
{
    IEnumerable<IMetadataDefinition> Build();

    ApiEndpointDebugNode Debug();
}