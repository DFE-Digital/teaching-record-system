using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using QualifiedTeachersApi.DataStore.Crm;
using QualifiedTeachersApi.DataStore.Crm.Models;
using QualifiedTeachersApi.V2.Requests;
using QualifiedTeachersApi.V2.Responses;

namespace QualifiedTeachersApi.V2.Handlers;

public class FindTeachersHandler : IRequestHandler<FindTeachersRequest, FindTeachersResponse>
{
    private readonly IDataverseAdapter _dataverseAdapter;
    private readonly ILogger<FindTeachersHandler> _logger;

    public FindTeachersHandler(IDataverseAdapter dataverseAdapter, ILogger<FindTeachersHandler> logger)
    {
        _dataverseAdapter = dataverseAdapter;
        _logger = logger;
    }

    public async Task<FindTeachersResponse> Handle(FindTeachersRequest request, CancellationToken cancellationToken)
    {
        var ittProviders = Array.Empty<Account>();

        if (!string.IsNullOrEmpty(request.IttProviderUkprn))
        {
            ittProviders = await _dataverseAdapter.GetIttProviderOrganizationsByUkprn(
                request.IttProviderUkprn,
                columnNames: Array.Empty<string>(),
                activeOnly: false);

            if (ittProviders.Length == 0)
            {
                _logger.LogDebug("Failed to find an ITT provider by UKPRN: '{IttProviderUkprn}'.", request.IttProviderUkprn);
            }
        }
        else if (!string.IsNullOrEmpty(request.IttProviderName))
        {
            ittProviders = await _dataverseAdapter.GetIttProviderOrganizationsByName(
                request.IttProviderName,
                columnNames: Array.Empty<string>(),
                activeOnly: false);

            if (ittProviders.Length == 0)
            {
                _logger.LogDebug("Failed to find an ITT provider by name: '{IttProviderName}'.", request.IttProviderName);
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
            EmailAddress = request.EmailAddress,
            Trn = request.Trn
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
