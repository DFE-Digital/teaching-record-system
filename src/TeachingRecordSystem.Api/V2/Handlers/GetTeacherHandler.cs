#nullable disable
using MediatR;
using TeachingRecordSystem.Api.V2.Requests;
using TeachingRecordSystem.Api.V2.Responses;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.Api.V2.Handlers;

public class GetTeacherHandler(TrsDbContext dbContext) :
    IRequestHandler<GetTeacherRequest, GetTeacherResponse>
{
    public async Task<GetTeacherResponse> Handle(GetTeacherRequest request, CancellationToken cancellationToken)
    {
        var person = await dbContext.Persons
            .Include(p => p.Alerts).AsSplitQuery()
            .SingleOrDefaultAsync(p => p.Trn == request.Trn, cancellationToken: cancellationToken);

        var hasActiveAlert = person.Alerts!.Any(a => a.IsOpen);

        return new GetTeacherResponse
        {
            DateOfBirth = person.DateOfBirth,
            FirstName = person.FirstName,
            HasActiveSanctions = hasActiveAlert,
            LastName = person.LastName,
            MiddleName = person.MiddleName,
            NationalInsuranceNumber = person.NationalInsuranceNumber,
            Trn = person.Trn,
            QtsDate = person.QtsDate,
            EytsDate = person.EytsDate,
            HusId = null,
            EarlyYearsStatus = null,
            InitialTeacherTraining = [],
            AllowPIIUpdates = person.AllowDetailsUpdatesFromSourceApplication
        };
    }
}
