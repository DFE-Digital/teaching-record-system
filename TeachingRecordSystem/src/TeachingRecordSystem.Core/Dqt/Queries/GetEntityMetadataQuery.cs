using Microsoft.Xrm.Sdk.Metadata;

namespace TeachingRecordSystem.Core.Dqt.Queries;

public record GetEntityMetadataQuery(string EntityLogicalName, EntityFilters EntityFilters = EntityFilters.Default) : ICrmQuery<EntityMetadata>;
