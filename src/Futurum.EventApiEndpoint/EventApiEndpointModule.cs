using System.Reflection;

using FluentValidation;

using Futurum.ApiEndpoint;
using Futurum.EventApiEndpoint.Internal;
using Futurum.EventApiEndpoint.Metadata;
using Futurum.EventApiEndpoint.Middleware;
using Futurum.Microsoft.Extensions.DependencyInjection;

using Microsoft.Extensions.DependencyInjection;

namespace Futurum.EventApiEndpoint;

public class EventApiEndpointModule : IModule
{
    private readonly EventApiEndpointConfiguration _configuration;
    private readonly Assembly[] _assemblies;

    public EventApiEndpointModule(EventApiEndpointConfiguration configuration, params Assembly[] assemblies)
    {
        _configuration = configuration;
        _assemblies = assemblies;
    }

    public void Load(IServiceCollection services)
    {
        services.AddSingleton<IEventApiEndpointAssemblyStore>(new EventApiEndpointAssemblyStore(_assemblies));

        services.RegisterModule(new ApiEndpointModule(_assemblies));
        services.RegisterModule(new EventApiEndpointAndMapperModule(_assemblies));

        services.AddScoped(typeof(EventApiEndpointExecutorService<,>));
        services.AddScoped(typeof(IEventApiEndpointDispatcher<,>), typeof(EventApiEndpointDispatcher<,>));

        RegisterConfiguration(services, _configuration);

        RegisterValidation(services, _assemblies);

        RegisterMiddleware(services, _configuration);

        RegisterBatchEventApiEndpoint(services);

        RegisterMappers(services);

        RegisterMetadata(services);
    }

    private static void RegisterConfiguration(IServiceCollection services, EventApiEndpointConfiguration configuration)
    {
        services.AddSingleton(configuration);
    }

    private static void RegisterBatchEventApiEndpoint(IServiceCollection services)
    {
        services.AddSingleton<IEventApiEndpoint<Batch.EventDto, Batch.Event>, Batch.ApiEndpoint>();
    }

    private static void RegisterMappers(IServiceCollection services)
    {
        RegisterBatchMapper(services);
    }

    private static void RegisterBatchMapper(IServiceCollection services)
    {
        services.AddSingleton<IEventApiEndpointEventMapper<Batch.EventDto, Batch.Event>, Batch.Mapper>();
    }

    private static void RegisterValidation(IServiceCollection services, Assembly[] assemblies)
    {
        services.AddSingleton(typeof(IEventApiEndpointEventValidation<>), typeof(EventApiEndpointEventValidation<>));

        services.Scan(scan => scan.FromAssemblies(assemblies)
                                  .AddClasses(classes => classes.Where(type => type.IsClosedTypeOf(typeof(IValidator<>))))
                                  .AsImplementedInterfaces()
                                  .WithSingletonLifetime());
    }

    private static void RegisterMiddleware(IServiceCollection services, EventApiEndpointConfiguration configuration)
    {
        if (configuration.EnableMiddleware)
        {
            RegisterEnabledMiddleware(services);
        }
        else
        {
            RegisterDisabledMiddleware(services);
        }
    }

    private static void RegisterDisabledMiddleware(IServiceCollection services)
    {
        services.AddSingleton(typeof(IEventApiEndpointMiddlewareExecutor<>), typeof(DisabledEventApiEndpointMiddlewareExecutor<>));
    }

    private static void RegisterEnabledMiddleware(IServiceCollection services)
    {
        services.AddSingleton(typeof(IEventApiEndpointMiddlewareExecutor<>), typeof(EventApiEndpointMiddlewareExecutor<>));

        services.AddSingleton(typeof(IEventApiEndpointMiddleware<>), typeof(EventApiEndpointPreProcessorMiddleware<>));

        services.AddSingleton(typeof(IEventApiEndpointMiddleware<>), typeof(EventApiEndpointPostProcessorMiddleware<>));
    }

    private static void RegisterMetadata(IServiceCollection services)
    {
        services.AddSingleton<IEventApiEndpointMetadataCache, EventApiEndpointMetadataCache>();
    }
}