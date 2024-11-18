#nullable disable
using MediatR;
using TeachingRecordSystem.Api.V2.Requests;
using TeachingRecordSystem.Api.V2.Responses;
using TeachingRecordSystem.Core.Dqt;

namespace TeachingRecordSystem.Api.V2.Handlers;

public class GetIttProvidersHandler : IRequestHandler<GetIttProvidersRequest, GetIttProvidersResponse>
{
    private readonly IDataverseAdapter _dataverseAdapter;

    public GetIttProvidersHandler(IDataverseAdapter dataverseAdapter)
    {
        _dataverseAdapter = dataverseAdapter;
    }

    public async Task<GetIttProvidersResponse> Handle(GetIttProvidersRequest request, CancellationToken cancellationToken)
    {
        var ittProviders = await _dataverseAdapter.GetIttProvidersAsync(false);

        return new GetIttProvidersResponse()
        {
            IttProviders = ittProviders.Select(a => new IttProviderInfo()
            {
                ProviderName = a.Name,
                Ukprn = a.dfeta_UKPRN
            })
        };
    }
}
