using FastEndpoints;
using TeachingRecordSystem.Api.Infrastructure.Security;
using TeachingRecordSystem.Api.V3.Requests;
using TeachingRecordSystem.Api.V3.Responses;
using TeachingRecordSystem.Dqt;
using TeachingRecordSystem.Dqt.Models;

namespace TeachingRecordSystem.Api.V3.Endpoints;

public class FindTeachersEndpoint : Endpoint<FindTeachersRequest, FindTeachersResponse>
{
    private readonly IDataverseAdapter _dataverseAdapter;

    public FindTeachersEndpoint(IDataverseAdapter dataverseAdapter)
    {
        _dataverseAdapter = dataverseAdapter;
    }

    public override void Configure()
    {
        Version(3);
        Get("teachers");
        Options(b => b.WithTags("Teachers"));
        Policies(AuthorizationPolicies.ApiKey);
        Description(b => b
            .WithName("FindTeachers")
            .Produces<FindTeachersResponse>());
        Summary(s =>
        {
            s.Summary = "Find teachers";
            s.Description = "Finds teachers with a TRN matching the specified criteria.";
            s.RequestParam(r => r.FindBy!, "The policy for matching teachers against the request criteria.");
            s.RequestParam(r => r.LastName!, "The teacher's last name.");
            s.RequestParam(r => r.DateOfBirth!, "The teacher's date of birth.");
        });
    }

    public override async Task HandleAsync(FindTeachersRequest req, CancellationToken ct)
    {
        var results = await _dataverseAdapter.FindTeachersByLastNameAndDateOfBirth(
            req.LastName!,
            req.DateOfBirth!.Value,
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

        var sanctions = (await _dataverseAdapter.GetSanctionsByContactIds(results.Select(r => r.Id), liveOnly: true));

        var response = new FindTeachersResponse()
        {
            Query = req,
            Total = results.Length,
            Results = results.Select(r => new FindTeachersResponseResult()
            {
                Trn = r.dfeta_TRN,
                DateOfBirth = r.BirthDate!.Value.ToDateOnlyWithDqtBstFix(isLocalTime: false),
                FirstName = r.ResolveFirstName(),
                MiddleName = r.ResolveMiddleName(),
                LastName = r.ResolveLastName(),
                Sanctions = sanctions[r.Id]
            }).ToArray()
        };

        await SendAsync(response);
    }
}
