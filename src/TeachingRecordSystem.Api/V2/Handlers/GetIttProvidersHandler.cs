using TeachingRecordSystem.Api.V2.Responses;

namespace TeachingRecordSystem.Api.V2.Handlers;

public class GetIttProvidersHandler(ReferenceDataCache referenceDataCache)
{
    public async Task<GetIttProvidersResponse> HandleAsync()
    {
        var ittProviders = (await referenceDataCache.GetTrainingProvidersAsync(activeOnly: false))
            .Where(p => p.Ukprn != null)
            .OrderBy(p => p.Name)
            .ThenBy(p => p.Ukprn)
            .Select(p => new IttProviderInfo { ProviderName = p.Name, Ukprn = p.Ukprn! })
            .ToArray();

        return new GetIttProvidersResponse
        {
            IttProviders = ittProviders
        };
    }
}
