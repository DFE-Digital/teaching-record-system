#nullable disable
using MediatR;
using TeachingRecordSystem.Api.V1.Requests;
using TeachingRecordSystem.Api.V1.Responses;
using TeachingRecordSystem.Api.V2.ApiModels;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.Api.V1.Handlers;

public class GetTeacherHandler(IDataverseAdapter dataverseAdapter, TrsDbContext dbContext) :
    IRequestHandler<GetTeacherRequest, GetTeacherResponse>
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

        var qualifications = await dataverseAdapter.GetQualificationsForTeacherAsync(
            person.DqtContactId!.Value,
            columnNames:
            [
                dfeta_qualification.Fields.dfeta_CompletionorAwardDate,
                dfeta_qualification.Fields.dfeta_Type,
                dfeta_qualification.Fields.dfeta_HE_ClassDivision
            ],
            heQualificationColumnNames:
            [
                dfeta_hequalification.PrimaryIdAttribute,
                dfeta_hequalification.Fields.dfeta_name
            ],
            heSubjectColumnNames:
            [
                dfeta_hesubject.PrimaryIdAttribute,
                dfeta_hesubject.Fields.dfeta_name,
                dfeta_hesubject.Fields.dfeta_Value
            ]);

        return MapContactToResponse(person, qualifications);
    }

    internal static GetTeacherResponse MapContactToResponse(
        PostgresModels.Person person,
        IEnumerable<dfeta_qualification> qualifications)
    {
        return new GetTeacherResponse()
        {
            Trn = person.Trn,
            NationalInsuranceNumber = person.NationalInsuranceNumber,
            QualifiedTeacherStatus = MapQualifiedTeacherStatus(),
            Induction = MapInduction(),
            InitialTeacherTraining = null,
            Qualifications = MapQualifications(),
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

        Qualification[] MapQualifications() => qualifications.Select(MapQualification).ToArray();

        Qualification MapQualification(dfeta_qualification qualification)
        {
            var heQualification = qualification.Extract<dfeta_hequalification>();

            var subject1 = qualification.Extract<dfeta_hesubject>($"{nameof(dfeta_hesubject)}1", dfeta_hesubject.PrimaryIdAttribute);
            var subject2 = qualification.Extract<dfeta_hesubject>($"{nameof(dfeta_hesubject)}2", dfeta_hesubject.PrimaryIdAttribute);
            var subject3 = qualification.Extract<dfeta_hesubject>($"{nameof(dfeta_hesubject)}3", dfeta_hesubject.PrimaryIdAttribute);

            return new Qualification()
            {
                Name = qualification.FormattedValues.ValueOrNull(dfeta_qualification.Fields.dfeta_Type),
                DateAwarded = qualification.dfeta_CompletionorAwardDate,
                Subject1 = subject1?.dfeta_name,
                Subject2 = subject2?.dfeta_name,
                Subject3 = subject3?.dfeta_name,
                Subject1Code = subject1?.dfeta_Value,
                Subject2Code = subject2?.dfeta_Value,
                Subject3Code = subject3?.dfeta_Value,
                HeQualificationName = heQualification?.dfeta_name,
                ClassDivision = qualification.dfeta_HE_ClassDivision?.ConvertToEnumByValue<dfeta_classdivision, ClassDivision>()
            };
        }
    }
}
