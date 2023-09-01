using MediatR;
using TeachingRecordSystem.Api.V3.Requests;
using TeachingRecordSystem.Api.V3.Responses;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Api.V3.Handlers;

public class FindTeachersHandler : IRequestHandler<FindTeachersRequest, FindTeachersResponse>
{
    private readonly IDataverseAdapter _dataverseAdapter;
    private readonly ICrmQueryDispatcher _crmQueryDispatcher;

    public FindTeachersHandler(IDataverseAdapter dataverseAdapter, ICrmQueryDispatcher crmQueryDispatcher)
    {
        _dataverseAdapter = dataverseAdapter;
        _crmQueryDispatcher = crmQueryDispatcher;
    }

    public async Task<FindTeachersResponse> Handle(FindTeachersRequest request, CancellationToken cancellationToken)
    {
        var results = await _dataverseAdapter.FindTeachersByLastNameAndDateOfBirth(
            request.LastName!,
            request.DateOfBirth!.Value,
            columnNames: new[]
            {
                Contact.Fields.dfeta_TRN,
                Contact.Fields.BirthDate,
                Contact.Fields.FirstName,
                Contact.Fields.MiddleName,
                Contact.Fields.LastName,
                Contact.Fields.dfeta_StatedFirstName,
                Contact.Fields.dfeta_StatedMiddleName,
                Contact.Fields.dfeta_StatedLastName
            });

        var sanctions = await _crmQueryDispatcher.ExecuteQuery(
            new GetSanctionsByContactIdsQuery(
                results.Select(r => r.Id),
                ActiveOnly: true,
                new()));

        return new FindTeachersResponse()
        {
            Query = request,
            Total = results.Length,
            Results = results.Select(r => new FindTeachersResponseResult()
            {
                Trn = r.dfeta_TRN,
                DateOfBirth = r.BirthDate!.Value.ToDateOnlyWithDqtBstFix(isLocalTime: false),
                FirstName = r.ResolveFirstName(),
                MiddleName = r.ResolveMiddleName(),
                LastName = r.ResolveLastName(),
                Sanctions = sanctions[r.Id].Select(s => s.SanctionCode).ToArray()
            }).ToArray()
        };
    }
}
