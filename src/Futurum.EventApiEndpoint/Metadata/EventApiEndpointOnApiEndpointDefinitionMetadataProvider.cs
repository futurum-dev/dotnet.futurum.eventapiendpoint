using System.Reflection;

using Futurum.ApiEndpoint;
using Futurum.ApiEndpoint.DebugLogger;
using Futurum.Core.Result;
using Futurum.EventApiEndpoint.Internal;

namespace Futurum.EventApiEndpoint.Metadata;

public static class EventApiEndpointOnApiEndpointDefinitionMetadataProvider
{
    public static IEnumerable<MetadataEventDefinition> GetMetadataEventDefinition(IEnumerable<Assembly> assemblies)
    {
        var apiEndpointDefinitions = assemblies.SelectMany(s => s.GetTypes())
                                               .Where(p => typeof(IApiEndpointDefinition).IsAssignableFrom(p))
                                               .OrderBy(x => x.GetType().FullName)
                                               .Select(Activator.CreateInstance)
                                               .Cast<IApiEndpointDefinition>();

        return GetMetadataEventDefinition(apiEndpointDefinitions);
    }

    public static IEnumerable<MetadataEventDefinition> GetMetadataEventDefinition(IEnumerable<IApiEndpointDefinition> apiEndpointDefinitions)
    {
        ApiEndpointDefinitionBuilder apiEndpointDefinitionBuilder = new();

        foreach (var apiEndpointDefinition in apiEndpointDefinitions)
        {
            apiEndpointDefinition.Configure(apiEndpointDefinitionBuilder);
        }

        return apiEndpointDefinitionBuilder.Get()
                                           .Filter(x => x.MetadataDefinition is MetadataSubscriptionEnvelopeEventDefinition)
                                           .FlatMap(GetMetadataEventDefinition)
                                           .MapSwitch(Enumerable.ToList, () => new List<MetadataEventDefinition>())
                                           .EnhanceWithError(() => $"Failed to set cache in {nameof(EventApiEndpointOnApiEndpointDefinitionMetadataProvider)}")
                                           .Unwrap();
    }

    public static IEnumerable<MetadataEnvelopeEventDefinition> GetMetadataEnvelopeEventDefinition(IEnumerable<Assembly> assemblies)
    {
        var apiEndpointDefinitions = assemblies.SelectMany(assembly => assembly.GetTypes())
                                               .Where(type => typeof(IApiEndpointDefinition).IsAssignableFrom(type))
                                               .OrderBy(type => type.FullName)
                                               .Select(Activator.CreateInstance)
                                               .Cast<IApiEndpointDefinition>();

        return GetMetadataEnvelopeEventDefinition(apiEndpointDefinitions);
    }

    public static IEnumerable<MetadataEnvelopeEventDefinition> GetMetadataEnvelopeEventDefinition(IEnumerable<IApiEndpointDefinition> apiEndpointDefinitions)
    {
        ApiEndpointDefinitionBuilder apiEndpointDefinitionBuilder = new();

        foreach (var apiEndpointDefinition in apiEndpointDefinitions)
        {
            apiEndpointDefinition.Configure(apiEndpointDefinitionBuilder);
        }

        return apiEndpointDefinitionBuilder.Get()
                                           .Filter(x => x.MetadataDefinition is MetadataSubscriptionEnvelopeEventDefinition)
                                           .FlatMap(GetMetadataEnvelopeEventDefinition)
                                           .MapSwitch(Enumerable.ToList, () => new List<MetadataEnvelopeEventDefinition>())
                                           .EnhanceWithError(() => $"Failed to set cache in {nameof(EventApiEndpointOnApiEndpointDefinitionMetadataProvider)}")
                                           .Unwrap();
    }

    private static IEnumerable<MetadataEventDefinition> GetMetadataEventDefinition(ApiEndpointDefinition apiEndpointDefinition)
    {
        var metadataRouteDefinition = apiEndpointDefinition.MetadataDefinition;
        var apiEndpointType = apiEndpointDefinition.ApiEndpointType;

        if (apiEndpointType.IsClosedTypeOf(typeof(IEventApiEndpoint<,>)))
        {
            var apiEndpointInterfaceType = apiEndpointType.GetInterfaces().Single(type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEventApiEndpoint<,>));

            var metadataTypeDefinition = EventApiEndpointMetadataTypeService.Get(apiEndpointInterfaceType, apiEndpointType);

            yield return new MetadataEventDefinition(metadataRouteDefinition, metadataTypeDefinition);
        }
    }

    private static IEnumerable<MetadataEnvelopeEventDefinition> GetMetadataEnvelopeEventDefinition(ApiEndpointDefinition apiEndpointDefinition)
    {
        var metadataSubscriptionEnvelopeEventDefinition = apiEndpointDefinition.MetadataDefinition as MetadataSubscriptionEnvelopeEventDefinition;
        var apiEndpointType = apiEndpointDefinition.ApiEndpointType;

        if (apiEndpointType.IsClosedTypeOf(typeof(IEventApiEndpoint<,>)))
        {
            var apiEndpointInterfaceType = apiEndpointType.GetInterfaces().Single(type => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEventApiEndpoint<,>));

            var metadataTypeDefinition = EventApiEndpointMetadataTypeService.Get(apiEndpointInterfaceType, apiEndpointType);

            yield return new MetadataEnvelopeEventDefinition(metadataSubscriptionEnvelopeEventDefinition, metadataTypeDefinition);
        }
    }

    public static IEnumerable<ApiEndpointDebugNode> GetDebug(IEnumerable<Assembly> assemblies)
    {
        var apiEndpointDefinitions = assemblies.SelectMany(assembly => assembly.GetTypes())
                                               .Where(type => typeof(IApiEndpointDefinition).IsAssignableFrom(type))
                                               .OrderBy(type => type.FullName)
                                               .Select(Activator.CreateInstance)
                                               .Cast<IApiEndpointDefinition>();

        ApiEndpointDefinitionBuilder apiEndpointDefinitionBuilder = new();

        foreach (var apiEndpointDefinition in apiEndpointDefinitions)
        {
            apiEndpointDefinition.Configure(apiEndpointDefinitionBuilder);
        }

        return apiEndpointDefinitionBuilder.Debug();
    }
}