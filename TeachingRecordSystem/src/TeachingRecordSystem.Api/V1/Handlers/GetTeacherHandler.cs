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

        var birthDate = request.BirthDate.ToDateOnlyWithDqtBstFix(isLocalTime: false);

        var matched = await dbContext.Persons
            .Include(p => p.Alerts).AsSplitQuery()
            .Where(p => p.DateOfBirth == birthDate &&
                (p.NationalInsuranceNumber == request.NationalInsuranceNumber || p.Trn == request.Trn))
            .ToArrayAsync();

        // Prefer matches on TRN
        var person = matched.SingleOrDefault(p => p.Trn == request.Trn) ??
            (matched.Length == 1 ? matched[0] : null);

        if (person is null)
        {
            return null;
        }

        return MapContactToResponse(person);
    }

    internal static GetTeacherResponse MapContactToResponse(PostgresModels.Person person)
    {
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
            ActiveAlert = person.Alerts!.Any(a => a.IsOpen),
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
}
