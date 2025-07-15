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
        var query = new FindTeachersByTrnBirthDateAndNinoQuery()
        {
            BirthDate = request.BirthDate,
            NationalInsuranceNumber = request.NationalInsuranceNumber,
            Trn = request.Trn
        };

        if (!query.BirthDate.HasValue)
        {
            return null;
        }

        var result = await dataverseAdapter.FindTeachersAsync(query);

        var teacher = result.FirstOrDefault(match => match.dfeta_TRN == query.Trn) ??
                      result.FirstOrDefault(match => match.dfeta_NINumber == query.NationalInsuranceNumber);

        if (teacher == null)
        {
            return null;
        }

        var person = await dbContext.Persons.SingleAsync(p => p.PersonId == teacher.Id);

        var qualifications = await dataverseAdapter.GetQualificationsForTeacherAsync(
            teacher.Id,
            columnNames: new[]
            {
                dfeta_qualification.Fields.dfeta_CompletionorAwardDate,
                dfeta_qualification.Fields.dfeta_Type,
                dfeta_qualification.Fields.dfeta_HE_ClassDivision
            },
            heQualificationColumnNames: new[]
            {
                dfeta_hequalification.PrimaryIdAttribute,
                dfeta_hequalification.Fields.dfeta_name
            },
            heSubjectColumnNames: new[]
            {
                dfeta_hesubject.PrimaryIdAttribute,
                dfeta_hesubject.Fields.dfeta_name,
                dfeta_hesubject.Fields.dfeta_Value
            });

        if (qualifications.Any())
        {
            teacher.dfeta_contact_dfeta_qualification = qualifications;
        }

        var hasActiveAlert = await dbContext.Alerts.Where(a => a.PersonId == teacher.Id && a.IsOpen).AnyAsync();

        var response = MapContactToResponse(teacher, hasActiveAlert, person);
        return response;
    }

    internal static GetTeacherResponse MapContactToResponse(
        Contact teacher,
        bool hasActiveAlert,
        PostgresModels.Person person)
    {
        return new GetTeacherResponse()
        {
            Trn = teacher.dfeta_TRN,
            NationalInsuranceNumber = teacher.dfeta_NINumber,
            QualifiedTeacherStatus = MapQualifiedTeacherStatus(),
            Induction = MapInduction(),
            InitialTeacherTraining = null,
            Qualifications = MapQualifications(),
            Name = teacher.FullName,
            DateOfBirth = teacher.BirthDate,
            ActiveAlert = hasActiveAlert,
            State = teacher.StateCode.Value,
            StateName = teacher.FormattedValues[Contact.Fields.StateCode]
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

        Qualification[] MapQualifications() =>
            teacher.dfeta_contact_dfeta_qualification?.Select(MapQualification)?.ToArray() ??
            Array.Empty<Qualification>();

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
