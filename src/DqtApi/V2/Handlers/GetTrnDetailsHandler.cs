using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DqtApi.DataStore.Crm;
using DqtApi.DataStore.Crm.Models;
using DqtApi.V2.Requests;
using DqtApi.V2.Responses;
using DqtApi.Validation;
using MediatR;

namespace DqtApi.V2.Handlers
{
    public class GetTrnDetailsHandler : IRequestHandler<GetTrnDetailsRequest, GetTrnDetailsResponse>
    {
        private readonly IDataverseAdapter _dataverseAdapter;

        public GetTrnDetailsHandler(IDataverseAdapter dataverseAdapter)
        {
            _dataverseAdapter = dataverseAdapter;
        }

        public async Task<GetTrnDetailsResponse> Handle(GetTrnDetailsRequest request, CancellationToken cancellationToken)
        {
            Debug.Assert(request != null);
            var getIttProviderTask = default(Account);
            if (!string.IsNullOrEmpty(request.IttProviderUkprn))
            {
                getIttProviderTask = await _dataverseAdapter.GetOrganizationByUkprn(request.IttProviderUkprn);
                if (getIttProviderTask == null)
                    throw new ErrorException(ErrorRegistry.OrganisationNotFound());
            }
            else if (!string.IsNullOrEmpty(request.IttProviderName))
            {
                getIttProviderTask = await _dataverseAdapter.GetOrganizationByProviderName(request.IttProviderName);
                if (getIttProviderTask == null)
                    throw new ErrorException(ErrorRegistry.OrganisationNotFound());
            }

            var query = new FindTeachersQuery()
            {
                FirstName = request.FirstName,
                MiddleName = request.MiddleName,
                LastName = request.LastName,
                EmailAddress = request.EmailAddress,
                PreviousFirstName = request.PreviousFirstName,
                PreviousLastName = request.PreviousLastName,
                Nino = request.Nino,
                DateOfBirth = request.DateOfBirth,
                IttProviderOrganizationId = getIttProviderTask != null ? getIttProviderTask.Id : null
            };

            var result = await _dataverseAdapter.FindTeachers(query);
            if (result?.Count > 0)
                return new GetTrnDetailsResponse()
                {
                    Details = result?.Select(a => new TrnDetails()
                    {
                        Trn = a.dfeta_TRN,
                        EmailAddresses = !string.IsNullOrEmpty(a.EMailAddress1) ? new List<string> { a.EMailAddress1 } : null,
                        FirstName = a.FirstName,
                        LastName = a.LastName,
                        DateOfBirth = a.BirthDate?.ToString(),
                        Nino = a.dfeta_NINumber,
                        Uid = a.Id.ToString()
                    })
                };
            return null;
        }
    }
}
