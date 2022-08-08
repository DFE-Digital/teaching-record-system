using System;
using System.Collections.Generic;
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
    public class FindTeachersHandler : IRequestHandler<FindTeachersRequest, FindTeachersResponse>
    {
        private readonly IDataverseAdapter _dataverseAdapter;

        public FindTeachersHandler(IDataverseAdapter dataverseAdapter)
        {
            _dataverseAdapter = dataverseAdapter;
        }

        public async Task<FindTeachersResponse> Handle(FindTeachersRequest request, CancellationToken cancellationToken)
        {
            var ittProviders = Array.Empty<Account>();

            if (!string.IsNullOrEmpty(request.IttProviderUkprn))
            {
                ittProviders = await _dataverseAdapter.GetIttProviderOrganizationsByUkprn(request.IttProviderUkprn, false);

                if (ittProviders.Length == 0)
                {
                    throw new ErrorException(ErrorRegistry.OrganisationNotFound());
                }
            }
            else if (!string.IsNullOrEmpty(request.IttProviderName))
            {
                ittProviders = await _dataverseAdapter.GetIttProviderOrganizationsByName(request.IttProviderName, false);

                if (ittProviders.Length == 0)
                {
                    throw new ErrorException(ErrorRegistry.OrganisationNotFound());
                }
            }

            var query = new FindTeachersQuery()
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                PreviousFirstName = request.PreviousFirstName,
                PreviousLastName = request.PreviousLastName,
                NationalInsuranceNumber = request.NationalInsuranceNumber,
                DateOfBirth = request.DateOfBirth,
                IttProviderOrganizationIds = ittProviders.Select(a => a.Id),
                EmailAddress = request.EmailAddress
            };

            var result = await _dataverseAdapter.FindTeachers(query);

            return new FindTeachersResponse()
            {
                Results = result?.Select(a => new FindTeacherResult()
                {
                    Trn = a.dfeta_TRN,
                    EmailAddresses = !string.IsNullOrEmpty(a.EMailAddress1) ? new List<string> { a.EMailAddress1 } : null,
                    FirstName = a.FirstName,
                    LastName = a.LastName,
                    DateOfBirth = a.BirthDate.HasValue ? DateOnly.FromDateTime(a.BirthDate.Value) : null,
                    NationalInsuranceNumber = a.dfeta_NINumber,
                    Uid = a.Id.ToString(),
                    HasActiveSanctions = a.dfeta_ActiveSanctions == true
                }) ?? Enumerable.Empty<FindTeacherResult>()
            };
        }
    }
}
