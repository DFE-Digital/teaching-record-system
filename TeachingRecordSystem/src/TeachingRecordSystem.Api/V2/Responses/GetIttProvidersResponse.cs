namespace TeachingRecordSystem.Api.V2.Responses;

public class GetIttProvidersResponse
{
    public required IEnumerable<IttProviderInfo> IttProviders { get; set; }
}
