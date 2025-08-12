using MediatR;
using TeachingRecordSystem.Api.V2.Requests;
using TeachingRecordSystem.Api.V2.Responses;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.Api.V2.Handlers;

public class GetIttProvidersHandler(TrsDbContext dbContext) : IRequestHandler<GetIttProvidersRequest, GetIttProvidersResponse>
{
    public async Task<GetIttProvidersResponse> Handle(GetIttProvidersRequest request, CancellationToken cancellationToken)
    {
        var ittProviders = await dbContext.TrainingProviders
            .Where(p => p.Ukprn != null)
            .OrderBy(p => p.Name)
            .ThenBy(p => p.Ukprn)
            .Select(p => new IttProviderInfo() { ProviderName = p.Name, Ukprn = p.Ukprn! })
            .ToArrayAsync();

        return new GetIttProvidersResponse()
        {
            IttProviders = ittProviders
        };
    }
}
