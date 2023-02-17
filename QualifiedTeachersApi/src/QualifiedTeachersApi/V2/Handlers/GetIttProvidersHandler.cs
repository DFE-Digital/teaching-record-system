using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using QualifiedTeachersApi.DataStore.Crm;
using QualifiedTeachersApi.V2.Requests;
using QualifiedTeachersApi.V2.Responses;

namespace QualifiedTeachersApi.V2.Handlers
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
