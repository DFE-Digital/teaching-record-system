#nullable disable
using MediatR;
using TeachingRecordSystem.Api.V2.Requests;
using TeachingRecordSystem.Api.V2.Responses;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.Api.V2.Handlers;

public class GetTeacherHandler(IDataverseAdapter dataverseAdapter, TrsDbContext dbContext) :
    IRequestHandler<GetTeacherRequest, GetTeacherResponse>
{
    public async Task<GetTeacherResponse> Handle(GetTeacherRequest request, CancellationToken cancellationToken)
    {
        var teacher = await dataverseAdapter.GetTeacherByTrnAsync(
            request.Trn,
            columnNames: new[]
            {
                Contact.Fields.FirstName,
                Contact.Fields.LastName,
                Contact.Fields.MiddleName,
                Contact.Fields.BirthDate,
                Contact.Fields.dfeta_EYTSDate,
                Contact.Fields.dfeta_QTSDate,
                Contact.Fields.dfeta_NINumber,
                Contact.Fields.dfeta_TRN,
                Contact.Fields.dfeta_HUSID,
                Contact.Fields.dfeta_AllowPiiUpdatesFromRegister
            },
            activeOnly: true);

        if (teacher is null)
        {
            return null;
        }

        var person = await dbContext.Persons.SingleAsync(p => p.PersonId == teacher.Id);

        var hasActiveAlert = await dbContext.Alerts.Where(a => a.PersonId == teacher.Id && a.IsOpen).AnyAsync();

        return new GetTeacherResponse()
        {
            DateOfBirth = teacher.BirthDate.ToDateOnlyWithDqtBstFix(isLocalTime: false),
            FirstName = teacher.FirstName,
            HasActiveSanctions = hasActiveAlert,
            LastName = teacher.LastName,
            MiddleName = teacher.MiddleName,
            NationalInsuranceNumber = teacher.dfeta_NINumber,
            Trn = teacher.dfeta_TRN,
            QtsDate = person.QtsDate,
            EytsDate = person.EytsDate,
            HusId = teacher.dfeta_HUSID,
            EarlyYearsStatus = null,
            InitialTeacherTraining = [],
            AllowPIIUpdates = teacher.dfeta_AllowPiiUpdatesFromRegister ?? false
        };
    }
}
