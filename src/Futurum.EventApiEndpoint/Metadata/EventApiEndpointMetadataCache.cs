using System.Reflection;

using Futurum.ApiEndpoint.DebugLogger;

namespace Futurum.EventApiEndpoint.Metadata;

public interface IEventApiEndpointMetadataCache
{
    IEnumerable<MetadataEventDefinition> GetMetadataEventDefinitions();
    IEnumerable<MetadataEnvelopeEventDefinition> GetMetadataEnvelopeEventDefinitions();
}

public class EventApiEndpointMetadataCache : IEventApiEndpointMetadataCache
{
    private readonly List<MetadataEventDefinition> _eventDefinitions = new();
    private readonly List<MetadataEnvelopeEventDefinition> _envelopeEventDefinitions = new();

    public EventApiEndpointMetadataCache(IEventApiEndpointAssemblyStore assemblyStore,
                                         IApiEndpointDebugLogger apiEndpointDebugLogger)
    {
        UpdateCache(assemblyStore.Get());

        LogApiEndpointDefinitionsDebug(apiEndpointDebugLogger, assemblyStore.Get());
    }

    private void UpdateCache(IEnumerable<Assembly> assemblies)
    {
        foreach (var apiEndpointDefinition in EventApiEndpointOnApiEndpointDefinitionMetadataProvider.GetMetadataEventDefinition(assemblies))
        {
            _eventDefinitions.Add(apiEndpointDefinition);
        }

        foreach (var apiEndpointDefinition in EventApiEndpointOnApiEndpointDefinitionMetadataProvider.GetMetadataEnvelopeEventDefinition(assemblies))
        {
            _envelopeEventDefinitions.Add(apiEndpointDefinition);
        }
    }

    private static void LogApiEndpointDefinitionsDebug(IApiEndpointDebugLogger apiEndpointDebugLogger, IEnumerable<Assembly> assemblies)
    {
        var apiEndpointDebugNodes = EventApiEndpointOnApiEndpointDefinitionMetadataProvider.GetDebug(assemblies);
        apiEndpointDebugLogger.Execute(apiEndpointDebugNodes.ToList());
    }

    public IEnumerable<MetadataEventDefinition> GetMetadataEventDefinitions() =>
        _eventDefinitions.AsEnumerable();

    public IEnumerable<MetadataEnvelopeEventDefinition> GetMetadataEnvelopeEventDefinitions() =>
        _envelopeEventDefinitions.AsEnumerable();
}