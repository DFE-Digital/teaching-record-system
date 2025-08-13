using MediatR;
using TeachingRecordSystem.Api.V2.Requests;
using TeachingRecordSystem.Api.V2.Responses;

namespace TeachingRecordSystem.Api.V2.Handlers;

public class GetIttProvidersHandler(ReferenceDataCache referenceDataCache) : IRequestHandler<GetIttProvidersRequest, GetIttProvidersResponse>
{
    public async Task<GetIttProvidersResponse> Handle(GetIttProvidersRequest request, CancellationToken cancellationToken)
    {
        var ittProviders = (await referenceDataCache.GetTrainingProvidersAsync(activeOnly: false))
            .Where(p => p.Ukprn != null)
            .OrderBy(p => p.Name)
            .ThenBy(p => p.Ukprn)
            .Select(p => new IttProviderInfo() { ProviderName = p.Name, Ukprn = p.Ukprn! })
            .ToArray();

        return new GetIttProvidersResponse()
        {
            IttProviders = ittProviders
        };
    }
}
