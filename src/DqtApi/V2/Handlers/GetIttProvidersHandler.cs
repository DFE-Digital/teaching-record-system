using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DqtApi.DataStore.Crm;
using DqtApi.V2.Requests;
using DqtApi.V2.Responses;
using MediatR;

namespace DqtApi.V2.Handlers
{
    public class GetIttProvidersHandler : IRequestHandler<GetIttProvidersRequest, GetIttProvidersResponse>
    {
        private readonly IDataverseAdapter _dataverseAdapter;

        public GetIttProvidersHandler(IDataverseAdapter dataverseAdapter)
        {
            _dataverseAdapter = dataverseAdapter;
        }

        public async Task<GetIttProvidersResponse> Handle(GetIttProvidersRequest request, CancellationToken cancellationToken)
        {
            var ittProviders = await _dataverseAdapter.GetIttProviders(false);

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
}
