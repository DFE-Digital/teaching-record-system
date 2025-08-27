#nullable disable
using MediatR;
using TeachingRecordSystem.Api.V1.Requests;
using TeachingRecordSystem.Api.V1.Responses;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.Api.V1.Handlers;

public class GetTeacherHandler(TrsDbContext dbContext) : IRequestHandler<GetTeacherRequest, GetTeacherResponse>
{
    public async Task<GetTeacherResponse> Handle(GetTeacherRequest request, CancellationToken cancellationToken)
    {
        if (request.BirthDate is null)
        {
            return null;
        }

        var birthDate = request.BirthDate.ToDateOnlyWithDqtBstFix(isLocalTime: false)!.Value.ToString("yyyy-MM-dd");
        var nationalInsuranceNumber = NationalInsuranceNumber.Normalize(request.NationalInsuranceNumber);

        var matched = await dbContext.Database.SqlQuery<PersonIdResult>(
                $"""
                SELECT person_id FROM person_search_attributes
                WHERE (attribute_type = 'DateOfBirth' AND attribute_value = ({birthDate} COLLATE "case_insensitive"))
                OR (attribute_type = 'Trn' AND attribute_value = ({request.Trn} COLLATE "case_insensitive"))
                OR (attribute_type = 'NationalInsuranceNumber' AND attribute_value = ({nationalInsuranceNumber} COLLATE "case_insensitive"))
                GROUP BY person_id
                HAVING 'DateOfBirth' = ANY(array_agg(attribute_type)) AND
                ARRAY['Trn', 'NationalInsuranceNumber']::varchar[] && array_agg(attribute_type)
                """)
            .Join(dbContext.Persons, id => id.person_id, p => p.PersonId, (id, p) => p)
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
            .ToArrayAsync();

        // Prefer matches on TRN
        var person = matched.SingleOrDefault(p => p.Trn == request.Trn) ??
            (matched.Length == 1 ? matched[0] : null);

        if (person is null)
        {
            return null;
        }

        return new GetTeacherResponse()
        {
            Trn = person.Trn,
            NationalInsuranceNumber = person.NationalInsuranceNumber,
            QualifiedTeacherStatus = MapQualifiedTeacherStatus(),
            Induction = MapInduction(),
            InitialTeacherTraining = null,
            Qualifications = [],
            Name = StringHelper.JoinNonEmpty(' ', person.FirstName, person.MiddleName, person.LastName),
            DateOfBirth = person.DateOfBirth.ToDateTime(),
            ActiveAlert = person.HasOpenAlert,
            State = ContactState.Active,
            StateName = "Active"
        };

        Induction MapInduction()
        {
            var dqtStatus = person.InductionStatus.ToDqtInductionStatus(out var statusDescription);

            return dqtStatus != null ?
                new Induction()
                {
                    StartDate = person.InductionStartDate.ToDateTime(),
                    CompletionDate = person.InductionCompletedDate.ToDateTime(),
                    InductionStatusName = statusDescription,
                    State = dfeta_inductionState.Active,
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
                    State = dfeta_qtsregistrationState.Active,
                    StateName = "Active",
                    QtsDate = person.QtsDate.ToDateTime()
                };
            }

            return null;
        }
    }

#pragma warning disable IDE1006 // Naming Styles
    private record PersonIdResult(Guid person_id);
#pragma warning restore IDE1006 // Naming Styles
}
