namespace Futurum.EventApiEndpoint;

/// <summary>
/// EventApiEndpoint configuration
/// </summary>
public record EventApiEndpointConfiguration(ParallelOptions BatchParallelOptions, bool EnableMiddleware)
{
    public static EventApiEndpointConfiguration Default =>
        new(new ParallelOptions(), false);
}