using System;
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
    public class GetTrnDetailsHandler : IRequestHandler<FindTeachersRequest, FindTeachersResponse>
    {
        private readonly IDataverseAdapter _dataverseAdapter;

        public GetTrnDetailsHandler(IDataverseAdapter dataverseAdapter)
        {
            _dataverseAdapter = dataverseAdapter;
        }

        public async Task<FindTeachersResponse> Handle(FindTeachersRequest request, CancellationToken cancellationToken)
        {
            Debug.Assert(request != null);
            var ittProvider = default(Account);
            if (!string.IsNullOrEmpty(request.IttProviderUkprn))
            {
                ittProvider = await _dataverseAdapter.GetOrganizationByUkprn(request.IttProviderUkprn);
                if (ittProvider == null)
                    throw new ErrorException(ErrorRegistry.OrganisationNotFound());
            }
            else if (!string.IsNullOrEmpty(request.IttProviderName))
            {
                ittProvider = await _dataverseAdapter.GetOrganizationByProviderName(request.IttProviderName);
                if (ittProvider == null)
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
                NationalInsuranceNumber = request.NationalInsuranceNumber,
                DateOfBirth = request.DateOfBirth,
                IttProviderOrganizationId = ittProvider != null ? ittProvider.Id : null
            };

            var result = await _dataverseAdapter.FindTeachers(query);
            if (result?.Count > 0)
                return new FindTeachersResponse()
                {
                    Results = result?.Select(a => new FindTeacherResult()
                    {
                        Trn = a.dfeta_TRN,
                        EmailAddresses = !string.IsNullOrEmpty(a.EMailAddress1) ? new List<string> { a.EMailAddress1 } : null,
                        FirstName = a.FirstName,
                        LastName = a.LastName,
                        DateOfBirth =  a.BirthDate.HasValue ? DateOnly.FromDateTime(a.BirthDate.Value) : null,
                        NationalInsuranceNumber = a.dfeta_NINumber,
                        Uid = a.Id.ToString()
                    })
                };
            return null;
        }
    }
}
