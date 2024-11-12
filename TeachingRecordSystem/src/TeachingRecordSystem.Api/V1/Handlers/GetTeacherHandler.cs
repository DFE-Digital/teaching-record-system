#nullable disable
using MediatR;
using TeachingRecordSystem.Api.V1.Requests;
using TeachingRecordSystem.Api.V1.Responses;
using TeachingRecordSystem.Api.V2.ApiModels;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.Api.V1.Handlers;

public class GetTeacherHandler : IRequestHandler<GetTeacherRequest, GetTeacherResponse>
{
    private readonly IDataverseAdapter _dataverseAdapter;
    private readonly TrsDbContext _dbContext;

    public GetTeacherHandler(IDataverseAdapter dataverseAdapter, TrsDbContext dbContext)
    {
        _dataverseAdapter = dataverseAdapter;
        _dbContext = dbContext;
    }

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

        var result = await _dataverseAdapter.FindTeachers(query);

        var teacher = result.FirstOrDefault(match => match.dfeta_TRN == query.Trn) ??
                      result.FirstOrDefault(match => match.dfeta_NINumber == query.NationalInsuranceNumber);

        if (teacher == null)
        {
            return null;
        }

        var qualifications = await _dataverseAdapter.GetQualificationsForTeacher(
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

        var hasActiveAlert = await _dbContext.Alerts.Where(a => a.PersonId == teacher.Id && a.IsOpen).AnyAsync();

        var response = MapContactToResponse(teacher, hasActiveAlert);
        return response;
    }

    internal static GetTeacherResponse MapContactToResponse(Contact teacher, bool hasActiveAlert)
    {
        return new GetTeacherResponse()
        {
            Trn = teacher.dfeta_TRN,
            NationalInsuranceNumber = teacher.dfeta_NINumber,
            QualifiedTeacherStatus = MapQualifiedTeacherStatus(),
            Induction = MapInduction(),
            InitialTeacherTraining = MapInitialTeacherTraining(),
            Qualifications = MapQualifications(),
            Name = teacher.FullName,
            DateOfBirth = teacher.BirthDate,
            ActiveAlert = hasActiveAlert,
            State = teacher.StateCode.Value,
            StateName = teacher.FormattedValues[Contact.Fields.StateCode]
        };

        Induction MapInduction()
        {
            var induction = teacher.Extract<dfeta_induction>();
            var inductionStatus = teacher.FormattedValues.ContainsKey(Contact.Fields.dfeta_InductionStatus) ? teacher.FormattedValues[Contact.Fields.dfeta_InductionStatus] : null;

            return induction != null ?
                new Induction()
                {
                    StartDate = induction.dfeta_StartDate,
                    CompletionDate = induction.dfeta_CompletionDate,
                    InductionStatusName = inductionStatus,
                    State = induction.StateCode.Value,
                    StateName = induction.FormattedValues[dfeta_induction.Fields.StateCode]
                } :
                !string.IsNullOrEmpty(inductionStatus) ?
                    new Induction()
                    {
                        StartDate = null,
                        CompletionDate = null,
                        InductionStatusName = inductionStatus
                    } : null;
        }

        QualifiedTeacherStatus MapQualifiedTeacherStatus()
        {
            var qts = teacher.Extract<dfeta_qtsregistration>();

            return qts != null ?
                new QualifiedTeacherStatus()
                {
                    Name = qts.dfeta_name,
                    State = qts.StateCode.Value,
                    StateName = qts.FormattedValues[dfeta_qtsregistration.Fields.StateCode],
                    QtsDate = qts.dfeta_QTSDate
                } :
                null;
        }

        InitialTeacherTraining MapInitialTeacherTraining()
        {
            var itt = teacher.Extract<dfeta_initialteachertraining>();

            if (itt == null)
            {
                return null;
            }

            var subject1 = itt.Extract<dfeta_ittsubject>($"{nameof(dfeta_ittsubject)}1", dfeta_ittsubject.PrimaryIdAttribute);
            var subject2 = itt.Extract<dfeta_ittsubject>($"{nameof(dfeta_ittsubject)}2", dfeta_ittsubject.PrimaryIdAttribute);
            var subject3 = itt.Extract<dfeta_ittsubject>($"{nameof(dfeta_ittsubject)}3", dfeta_ittsubject.PrimaryIdAttribute);

            return new InitialTeacherTraining()
            {
                State = itt.StateCode.Value,
                StateName = itt.FormattedValues[dfeta_qtsregistration.Fields.StateCode],
                ProgrammeStartDate = itt.dfeta_ProgrammeStartDate,
                ProgrammeEndDate = itt.dfeta_ProgrammeEndDate,
                ProgrammeType = itt.FormattedValues.ValueOrNull(dfeta_initialteachertraining.Fields.dfeta_ProgrammeType),
                Result = itt.FormattedValues.ValueOrNull(dfeta_initialteachertraining.Fields.dfeta_Result),
                Subject1Id = itt.FormattedValues.ValueOrNull(dfeta_initialteachertraining.Fields.dfeta_Subject1Id),
                Subject2Id = itt.FormattedValues.ValueOrNull(dfeta_initialteachertraining.Fields.dfeta_Subject2Id),
                Subject3Id = itt.FormattedValues.ValueOrNull(dfeta_initialteachertraining.Fields.dfeta_Subject3Id),
                Qualification = itt.FormattedValues.ValueOrNull(dfeta_initialteachertraining.Fields.dfeta_ITTQualificationId),
                Subject1Code = subject1?.dfeta_Value,
                Subject2Code = subject2?.dfeta_Value,
                Subject3Code = subject3?.dfeta_Value
            };
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
