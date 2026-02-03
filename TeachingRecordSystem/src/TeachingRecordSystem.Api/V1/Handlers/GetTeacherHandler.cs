#nullable disable
using MediatR;
using TeachingRecordSystem.Api.V1.Requests;
using TeachingRecordSystem.Api.V1.Responses;
using TeachingRecordSystem.Core.DataStore.Postgres;

namespace TeachingRecordSystem.Api.V1.Handlers;

public class GetTeacherHandler(TrsDbContext dbContext) : IRequestHandler<GetTeacherRequest, GetTeacherResponse>
{
    public async Task<GetTeacherResponse> Handle(GetTeacherRequest request, CancellationToken cancellationToken)
    {
        if (request.BirthDate is null)
        {
            return null;
        }

        var birthDate = request.BirthDate!.Value.ToString("yyyy-MM-dd");
        var nationalInsuranceNumber = NationalInsuranceNumber.Normalize(request.NationalInsuranceNumber);

        var matched = await dbContext.Persons.FromSql(
                $"""
                SELECT * FROM persons
                WHERE status = 0 AND
                date_of_birth = {birthDate}::date AND (
                    trn = {request.Trn} OR
                    (national_insurance_numbers && array_remove(ARRAY[{nationalInsuranceNumber}]::varchar[], null) COLLATE "case_insensitive")
                )
                """)
            .Select(p => new
            {
                p.Trn,
                p.FirstName,
                p.MiddleName,
                p.LastName,
                p.NationalInsuranceNumber,
                p.QtsDate,
                p.DateOfBirth,
                p.InductionStatus,
                p.InductionStartDate,
                p.InductionCompletedDate,
                HasOpenAlert = p.Alerts.Any(a => a.IsOpen)
            })
            .ToArrayAsync(cancellationToken);

        // Prefer matches on TRN
        var person = matched.SingleOrDefault(p => p.Trn == request.Trn) ??
            (matched.Length == 1 ? matched[0] : null);

        if (person is null)
        {
            return null;
        }

        return new GetTeacherResponse
        {
            Trn = person.Trn,
            NationalInsuranceNumber = person.NationalInsuranceNumber,
            QualifiedTeacherStatus = MapQualifiedTeacherStatus(),
            Induction = MapInduction(),
            InitialTeacherTraining = null,
            Qualifications = [],
            Name = ' '.JoinNonEmpty(person.FirstName, person.MiddleName, person.LastName),
            DateOfBirth = person.DateOfBirth.ToDateTime(),
            ActiveAlert = person.HasOpenAlert,
            State = 0,
            StateName = "Active"
        };

        Induction MapInduction()
        {
            var dqtStatus = person.InductionStatus.ToDqtInductionStatus(out var statusDescription);

            return dqtStatus != null ?
                new Induction
                {
                    StartDate = person.InductionStartDate.ToDateTime(),
                    CompletionDate = person.InductionCompletedDate.ToDateTime(),
                    InductionStatusName = statusDescription,
                    State = 0,
                    StateName = "Active"
                } :
                null;
        }

        QualifiedTeacherStatus MapQualifiedTeacherStatus()
        {
            if (person.QtsDate is not null)
            {
                return new QualifiedTeacherStatus
                {
                    Name = "",
                    State = 0,
                    StateName = "Active",
                    QtsDate = person.QtsDate.ToDateTime()
                };
            }

            return null;
        }
    }
}
