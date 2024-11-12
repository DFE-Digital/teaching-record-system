using System.Diagnostics;
using MediatR;
using TeachingRecordSystem.Api.V2.Requests;
using TeachingRecordSystem.Api.V2.Responses;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.Api.V2.Handlers;

public class FindTeachersHandler : IRequestHandler<FindTeachersRequest, FindTeachersResponse>
{
    private readonly TrsDbContext _dbContext;
    private readonly IDataverseAdapter _dataverseAdapter;
    private readonly ILogger<FindTeachersHandler> _logger;

    public FindTeachersHandler(TrsDbContext dbContext, IDataverseAdapter dataverseAdapter, ILogger<FindTeachersHandler> logger)
    {
        _dbContext = dbContext;
        _dataverseAdapter = dataverseAdapter;
        _logger = logger;
    }

    public async Task<FindTeachersResponse> Handle(FindTeachersRequest request, CancellationToken cancellationToken)
    {
        var result = request.MatchPolicy.GetValueOrDefault() == FindTeachersMatchPolicy.Default ?
            await HandleDefaultRequest(request) :
            await HandleStrictRequest(request);

        var matchedPersonIds = result.Select(c => c.Id).ToHashSet();
        var resultsWithActiveAlerts = await _dbContext.Alerts
            .Where(a => matchedPersonIds.Contains(a.PersonId) && a.IsOpen)
            .Select(a => a.PersonId)
            .Distinct()
            .ToArrayAsync();

        return new FindTeachersResponse()
        {
            Results = result.Select(a => new FindTeacherResult()
            {
                Trn = a.dfeta_TRN,
                EmailAddresses = !string.IsNullOrEmpty(a.EMailAddress1) ? new[] { a.EMailAddress1 } : Array.Empty<string>(),
                FirstName = a.FirstName,
                MiddleName = a.MiddleName,
                LastName = a.LastName,
                DateOfBirth = a.BirthDate.HasValue ? DateOnly.FromDateTime(a.BirthDate.Value) : null,
                NationalInsuranceNumber = a.dfeta_NINumber,
                Uid = a.Id.ToString(),
                HasActiveSanctions = resultsWithActiveAlerts.Contains(a.Id)
            })
        };
    }

    private async Task<Contact[]> HandleDefaultRequest(FindTeachersRequest request)
    {
        Debug.Assert(request.MatchPolicy.GetValueOrDefault() == FindTeachersMatchPolicy.Default);

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

        return await _dataverseAdapter.FindTeachers(query);
    }

    private async Task<Contact[]> HandleStrictRequest(FindTeachersRequest request)
    {
        Debug.Assert(request.MatchPolicy == FindTeachersMatchPolicy.Strict);

        var query = new FindTeachersQuery()
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            PreviousFirstName = request.PreviousFirstName,
            PreviousLastName = request.PreviousLastName,
            NationalInsuranceNumber = request.NationalInsuranceNumber,
            DateOfBirth = request.DateOfBirth,
            EmailAddress = request.EmailAddress,
            Trn = request.Trn
        };

        return await _dataverseAdapter.FindTeachersStrict(query);
    }
}
