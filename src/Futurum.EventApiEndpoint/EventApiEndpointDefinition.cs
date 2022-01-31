using Futurum.ApiEndpoint;
using Futurum.ApiEndpoint.DebugLogger;
using Futurum.Core.Result;

namespace Futurum.EventApiEndpoint;

public class EventApiEndpointDefinition : IApiEndpointDefinitionBuilder
{
    private readonly List<IApiEndpointDefinitionBuilder> _apiEndpointDefinitions = new();

    public Result<Dictionary<Type, List<IMetadataDefinition>>> Build() =>
        _apiEndpointDefinitions.FlatMap(x => x.Build())
                               .TryToDictionary(keyValuePair => keyValuePair.Key,
                                                keyValuePair => keyValuePair.Value.ToList());

    public ApiEndpointDebugNode Debug()
    {
        return new ApiEndpointDebugNode
        {
            Name = "ApiEndpointDefinition",
            Children = 
            {
                new ApiEndpointDebugNode
                {
                    Name = "EVENT",
                    Children = _apiEndpointDefinitions.Select(x => x.Debug()).ToList()
                }
            }
        };
    }

    public void Add(IApiEndpointDefinitionBuilder apiEndpointDefinitionBuilder)
    {
        _apiEndpointDefinitions.Add(apiEndpointDefinitionBuilder);
    }
}