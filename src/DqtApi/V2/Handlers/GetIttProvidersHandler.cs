using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DqtApi.DAL;
using DqtApi.V2.Requests;
using DqtApi.V2.Responses;
using MediatR;

namespace DqtApi.V2.Handlers
{
    public class GetIttProvidersHandler : IRequestHandler<GetIttProvidersRequest, GetIttProvidersResponse>
    {
        private readonly IDataverseAdaptor _dataverseAdaptor;

        public GetIttProvidersHandler(IDataverseAdaptor dataverseAdaptor)
        {
            _dataverseAdaptor = dataverseAdaptor;
        }

        public async Task<GetIttProvidersResponse> Handle(GetIttProvidersRequest request, CancellationToken cancellationToken)
        {
            var ittProviders = await _dataverseAdaptor.GetIttProviders();

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
