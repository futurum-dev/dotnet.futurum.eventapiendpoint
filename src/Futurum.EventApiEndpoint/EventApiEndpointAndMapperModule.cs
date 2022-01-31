using System.Reflection;

using Futurum.EventApiEndpoint.Internal;
using Futurum.Microsoft.Extensions.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;

namespace Futurum.EventApiEndpoint;

public class EventApiEndpointAndMapperModule : IModule
{
    private readonly Assembly[] _assemblies;

    public EventApiEndpointAndMapperModule(params Assembly[] assemblies)
    {
        _assemblies = assemblies;
    }

    public void Load(IServiceCollection services)
    {
        RegisterEventApiEndpoints(services, _assemblies);

        RegisterEventMapper(services, _assemblies);
    }

    private static void RegisterEventApiEndpoints(IServiceCollection services, Assembly[] assemblies)
    {
        services.Scan(scan => scan.FromAssemblies(assemblies)
                                  .AddClasses(classes => classes.Where(type => type.IsClosedTypeOf(typeof(IEventApiEndpoint<,>))))
                                  .AsImplementedInterfaces()
                                  .WithSingletonLifetime());
    }

    private static void RegisterEventMapper(IServiceCollection services, Assembly[] assemblies)
    {
        services.Scan(scan => scan.FromAssemblies(assemblies)
                                  .AddClasses(classes => classes.Where(type => type.IsClosedTypeOf(typeof(IEventApiEndpointEventMapper<,>))))
                                  .AsImplementedInterfaces()
                                  .WithSingletonLifetime());
    }
}