using System.Collections.Immutable;
using MediatR;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Api.V3.Requests;
using TeachingRecordSystem.Api.V3.Responses;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Api.V3.Handlers;

public class FindTeachersHandler : IRequestHandler<FindTeachersRequest, FindTeachersResponse>
{
    private readonly ICrmQueryDispatcher _crmQueryDispatcher;

    public FindTeachersHandler(ICrmQueryDispatcher crmQueryDispatcher)
    {
        _crmQueryDispatcher = crmQueryDispatcher;
    }

    public async Task<FindTeachersResponse> Handle(FindTeachersRequest request, CancellationToken cancellationToken)
    {
        var contacts = await _crmQueryDispatcher.ExecuteQuery(
            new GetContactsByLastNameAndDateOfBirthQuery(
                request.LastName!,
                request.DateOfBirth!.Value,
                new ColumnSet(
                    Contact.Fields.dfeta_TRN,
                    Contact.Fields.BirthDate,
                    Contact.Fields.FirstName,
                    Contact.Fields.MiddleName,
                    Contact.Fields.LastName,
                    Contact.Fields.dfeta_StatedFirstName,
                    Contact.Fields.dfeta_StatedMiddleName,
                    Contact.Fields.dfeta_StatedLastName)));

        var sanctions = await _crmQueryDispatcher.ExecuteQuery(
            new GetSanctionsByContactIdsQuery(
                contacts.Select(r => r.Id),
                ActiveOnly: true,
                new()));

        return new FindTeachersResponse()
        {
            Query = request,
            Total = contacts.Length,
            Results = contacts.Select(r => new FindTeachersResponseResult()
            {
                Trn = r.dfeta_TRN,
                DateOfBirth = r.BirthDate!.Value.ToDateOnlyWithDqtBstFix(isLocalTime: false),
                FirstName = r.ResolveFirstName(),
                MiddleName = r.ResolveMiddleName(),
                LastName = r.ResolveLastName(),
                Sanctions = sanctions[r.Id]
                    .Where(s => Constants.ExposableSanctionCodes.Contains(s.SanctionCode))
                    .Select(s => new SanctionInfo()
                    {
                        Code = s.SanctionCode,
                        StartDate = s.Sanction.dfeta_StartDate?.ToDateOnlyWithDqtBstFix(isLocalTime: true)
                    })
                    .ToImmutableArray()
            })
            .OrderBy(c => c.Trn)
            .ToImmutableArray()
        };
    }
}
