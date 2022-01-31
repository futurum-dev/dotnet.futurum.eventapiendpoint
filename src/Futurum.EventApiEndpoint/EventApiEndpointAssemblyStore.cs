using System.Reflection;

namespace Futurum.EventApiEndpoint;

public interface IEventApiEndpointAssemblyStore
{
    IEnumerable<Assembly> Get();
}

public class EventApiEndpointAssemblyStore : IEventApiEndpointAssemblyStore
{
    private readonly IEnumerable<Assembly> _assemblies;

    public EventApiEndpointAssemblyStore(IEnumerable<Assembly>assemblies)
    {
        _assemblies = assemblies;
    }
    
    public IEnumerable<Assembly> Get() =>
        _assemblies;
}