namespace TeachingRecordSystem.Core.Services.PublishApi;

public interface IPublishApiClient
{
    Task<IReadOnlyCollection<ProviderResource>> GetAccreditedProvidersAsync();
}

public record ProviderListResponse
{
    public required List<ProviderResource> Data { get; set; }
}

public record ProviderResource
{
    public required ProviderAttributes Attributes { get; set; }
}

public record ProviderAttributes
{
    public required string Ukprn { get; set; }
    public required string Name { get; set; }
}
