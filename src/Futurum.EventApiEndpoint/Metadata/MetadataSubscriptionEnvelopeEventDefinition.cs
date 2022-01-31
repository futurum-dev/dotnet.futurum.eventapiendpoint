using Futurum.ApiEndpoint;

namespace Futurum.EventApiEndpoint.Metadata;

public record MetadataSubscriptionEnvelopeEventDefinition(MetadataTopic FromTopic, MetadataRoute Route, Type ApiEndpointType) : IMetadataDefinition;