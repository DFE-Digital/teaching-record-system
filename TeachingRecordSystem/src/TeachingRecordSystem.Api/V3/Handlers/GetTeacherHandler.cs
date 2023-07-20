using System.Text;
using MediatR;
using Optional;
using TeachingRecordSystem.Api.V3.ApiModels;
using TeachingRecordSystem.Api.V3.Requests;
using TeachingRecordSystem.Api.V3.Responses;
using TeachingRecordSystem.Dqt;
using TeachingRecordSystem.Dqt.Models;

namespace TeachingRecordSystem.Api.V3.Handlers;

public class GetTeacherHandler : IRequestHandler<GetTeacherRequest, GetTeacherResponse?>
{
    private const string QtsAwardedInWalesTeacherStatusValue = "213";

    private readonly IDataverseAdapter _dataverseAdapter;

    public GetTeacherHandler(IDataverseAdapter dataverseAdapter)
    {
        _dataverseAdapter = dataverseAdapter;
    }

    public async Task<GetTeacherResponse?> Handle(GetTeacherRequest request, CancellationToken cancellationToken)
    {
        var teacher = await _dataverseAdapter.GetTeacherByTrn(
            request.Trn,
            columnNames: new[]
            {
                Contact.Fields.FirstName,
                Contact.Fields.MiddleName,
                Contact.Fields.LastName,
                Contact.Fields.dfeta_StatedFirstName,
                Contact.Fields.dfeta_StatedMiddleName,
                Contact.Fields.dfeta_StatedLastName,
                Contact.Fields.BirthDate,
                Contact.Fields.dfeta_NINumber,
                Contact.Fields.dfeta_QTSDate,
                Contact.Fields.dfeta_EYTSDate,
                Contact.Fields.EMailAddress1
            });

        if (teacher is null)
        {
            return null;
        }

        dfeta_induction? induction = default;
        dfeta_inductionperiod[]? inductionPeriods = default;

        if (request.Include.HasFlag(GetTeacherRequestIncludes.Induction))
        {
            (induction, inductionPeriods) = await _dataverseAdapter.GetInductionByTeacher(
                teacher.Id,
                columnNames: new[]
                {
                    dfeta_induction.PrimaryIdAttribute,
                    dfeta_induction.Fields.dfeta_StartDate,
                    dfeta_induction.Fields.dfeta_CompletionDate,
                    dfeta_induction.Fields.dfeta_InductionStatus
                },
                inductionPeriodColumnNames: new[]
                {
                    dfeta_inductionperiod.Fields.dfeta_InductionId,
                    dfeta_inductionperiod.Fields.dfeta_StartDate,
                    dfeta_inductionperiod.Fields.dfeta_EndDate,
                    dfeta_inductionperiod.Fields.dfeta_Numberofterms,
                    dfeta_inductionperiod.Fields.dfeta_AppropriateBodyId
                },
                appropriateBodyColumnNames: new[]
                {
                    Account.PrimaryIdAttribute,
                    Account.Fields.Name
                });
        }

        dfeta_initialteachertraining[]? itt = default;

        if (request.Include.HasFlag(GetTeacherRequestIncludes.InitialTeacherTraining))
        {
            itt = await _dataverseAdapter.GetInitialTeacherTrainingByTeacher(
                teacher.Id,
                columnNames: new[]
                {
                    dfeta_initialteachertraining.Fields.dfeta_ProgrammeEndDate,
                    dfeta_initialteachertraining.Fields.dfeta_ProgrammeStartDate,
                    dfeta_initialteachertraining.Fields.dfeta_ProgrammeType,
                    dfeta_initialteachertraining.Fields.dfeta_Result,
                    dfeta_initialteachertraining.Fields.dfeta_AgeRangeFrom,
                    dfeta_initialteachertraining.Fields.dfeta_AgeRangeTo,
                    dfeta_initialteachertraining.Fields.dfeta_EstablishmentId,
                    dfeta_initialteachertraining.Fields.dfeta_TraineeID,
                    dfeta_initialteachertraining.Fields.StateCode
                },
                establishmentColumnNames: new[]
                {
                    Account.PrimaryIdAttribute,
                    Account.Fields.dfeta_UKPRN,
                    Account.Fields.Name
                },
                subjectColumnNames: new[]
                {
                    dfeta_ittsubject.PrimaryIdAttribute,
                    dfeta_ittsubject.Fields.dfeta_name,
                    dfeta_ittsubject.Fields.dfeta_Value
                },
                qualificationColumnNames: new[]
                {
                    dfeta_ittqualification.PrimaryIdAttribute,
                    dfeta_ittqualification.Fields.dfeta_name
                },
                false);
        }

        dfeta_qualification[]? qualifications = default;

        if ((request.Include & (GetTeacherRequestIncludes.MandatoryQualifications | GetTeacherRequestIncludes.NpqQualifications | GetTeacherRequestIncludes.HigherEducationQualifications)) != 0)
        {
            string[]? columnNames = new[]
            {
                dfeta_qualification.Fields.dfeta_CompletionorAwardDate,
                dfeta_qualification.Fields.dfeta_Type,
                dfeta_qualification.Fields.StateCode
            };

            string[]? specialismColumnNames = null;
            string[]? heQualificationColumnNames = null;
            string[]? heSubjectColumnNames = null;

            if (request.Include.HasFlag(GetTeacherRequestIncludes.MandatoryQualifications))
            {
                columnNames = new[]
                {
                    dfeta_qualification.Fields.dfeta_CompletionorAwardDate,
                    dfeta_qualification.Fields.dfeta_Type,
                    dfeta_qualification.Fields.StateCode,
                    dfeta_qualification.Fields.dfeta_MQ_Date,
                    dfeta_qualification.Fields.dfeta_MQ_SpecialismId
                };

                specialismColumnNames = new[]
                {
                    dfeta_specialism.PrimaryIdAttribute,
                    dfeta_specialism.Fields.dfeta_name
                };
            }

            if (request.Include.HasFlag(GetTeacherRequestIncludes.HigherEducationQualifications))
            {
                heQualificationColumnNames = new[]
                {
                    dfeta_hequalification.PrimaryIdAttribute,
                    dfeta_hequalification.Fields.dfeta_name
                };

                heSubjectColumnNames = new[]
                {
                    dfeta_hesubject.PrimaryIdAttribute,
                    dfeta_hesubject.Fields.dfeta_name,
                    dfeta_hesubject.Fields.dfeta_Value
                };
            }

            qualifications = await _dataverseAdapter.GetQualificationsForTeacher(
                teacher.Id,
                columnNames,
                heQualificationColumnNames,
                heSubjectColumnNames,
                specialismColumnNames);
        }

        bool pendingNameChange = default, pendingDateOfBirthChange = default;

        if (request.Include.HasFlag(GetTeacherRequestIncludes.PendingDetailChanges))
        {
            var nameChangeSubject = await _dataverseAdapter.GetSubjectByTitle("Change of Name", columnNames: Array.Empty<string>());
            var dateOfBirthChangeSubject = await _dataverseAdapter.GetSubjectByTitle("Change of Date of Birth", columnNames: Array.Empty<string>());

            var incidents = await _dataverseAdapter.GetIncidentsByContactId(
                teacher.Id,
                IncidentState.Active,
                columnNames: new[] { Incident.Fields.SubjectId, Incident.Fields.StateCode });

            pendingNameChange = incidents.Any(i => i.SubjectId.Id == nameChangeSubject.Id);
            pendingDateOfBirthChange = incidents.Any(i => i.SubjectId.Id == dateOfBirthChangeSubject.Id);
        }

        IEnumerable<string>? sanctions = null;

        if (request.Include.HasFlag(GetTeacherRequestIncludes.Sanctions))
        {
            sanctions = (await _dataverseAdapter.GetSanctionsByContactIds(new[] { teacher.Id }, liveOnly: true))[teacher.Id]
                .Intersect(Constants.ExposableSanctionCodes);
        }

        var firstName = teacher.FirstName;
        var middleName = teacher.MiddleName ?? string.Empty;
        var lastName = teacher.LastName;
        if (!string.IsNullOrEmpty(teacher.dfeta_StatedFirstName) && !string.IsNullOrEmpty(teacher.dfeta_StatedLastName))
        {
            firstName = teacher.dfeta_StatedFirstName;
            middleName = teacher.dfeta_StatedMiddleName ?? string.Empty;
            lastName = teacher.dfeta_StatedLastName;
        }

        var qtsAwardedInWalesStatus = await _dataverseAdapter.GetTeacherStatus(QtsAwardedInWalesTeacherStatusValue, null);
        var qtsRegistrations = await _dataverseAdapter.GetQtsRegistrationsByTeacher(
            teacher.Id,
            columnNames: new[]
            {
                dfeta_qtsregistration.Fields.dfeta_QTSDate,
                dfeta_qtsregistration.Fields.dfeta_TeacherStatusId
            });

        var qtsAwardedInWales = qtsRegistrations.Any(qts => qts.dfeta_QTSDate is not null && qts.dfeta_TeacherStatusId.Id == qtsAwardedInWalesStatus.Id);

        return new GetTeacherResponse()
        {
            Trn = request.Trn,
            FirstName = firstName,
            MiddleName = middleName,
            LastName = lastName,
            DateOfBirth = teacher.BirthDate!.Value.ToDateOnlyWithDqtBstFix(isLocalTime: false),
            NationalInsuranceNumber = teacher.dfeta_NINumber,
            PendingNameChange = request.Include.HasFlag(GetTeacherRequestIncludes.PendingDetailChanges) ? Option.Some(pendingNameChange) : default,
            PendingDateOfBirthChange = request.Include.HasFlag(GetTeacherRequestIncludes.PendingDetailChanges) ? Option.Some(pendingDateOfBirthChange) : default,
            Qts = MapQts(teacher.dfeta_QTSDate?.ToDateOnlyWithDqtBstFix(isLocalTime: true), qtsAwardedInWales, request.AccessMode),
            Eyts = MapEyts(teacher.dfeta_EYTSDate?.ToDateOnlyWithDqtBstFix(isLocalTime: true), request.AccessMode),
            Email = teacher.EMailAddress1,
            Induction = request.Include.HasFlag(GetTeacherRequestIncludes.Induction) ?
                Option.Some(MapInduction(induction!, inductionPeriods!, request.AccessMode)) :
                default,
            InitialTeacherTraining = request.Include.HasFlag(GetTeacherRequestIncludes.InitialTeacherTraining) ?
                Option.Some(itt!.Select(i => new GetTeacherResponseInitialTeacherTraining()
                {
                    Qualification = MapIttQualification(i),
                    ProgrammeType = i.dfeta_ProgrammeType?.ConvertToEnum<dfeta_ITTProgrammeType, IttProgrammeType>(),
                    ProgrammeTypeDescription = i.dfeta_ProgrammeType?.ConvertToEnum<dfeta_ITTProgrammeType, IttProgrammeType>().GetDescription(),
                    StartDate = i.dfeta_ProgrammeStartDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                    EndDate = i.dfeta_ProgrammeEndDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                    Result = i.dfeta_Result.HasValue ? i.dfeta_Result.Value.ConvertFromITTResult() : null,
                    AgeRange = MapAgeRange(i.dfeta_AgeRangeFrom, i.dfeta_AgeRangeTo),
                    Provider = MapIttProvider(i),
                    Subjects = MapSubjects(i)
                })) :
                default,
            NpqQualifications = request.Include.HasFlag(GetTeacherRequestIncludes.NpqQualifications) ?
                Option.Some(MapNpqQualifications(qualifications!, request.AccessMode)) :
                default,
            MandatoryQualifications = request.Include.HasFlag(GetTeacherRequestIncludes.MandatoryQualifications) ?
                Option.Some(MapMandatoryQualifications(qualifications!)) :
                default,
            HigherEducationQualifications = request.Include.HasFlag(GetTeacherRequestIncludes.HigherEducationQualifications) ?
                Option.Some(MapHigherEducationQualifications(qualifications!)) :
                default,
            Sanctions = request.Include.HasFlag(GetTeacherRequestIncludes.Sanctions) ?
                Option.Some(sanctions!) :
                default
        };
    }

    private static GetTeacherResponseQts? MapQts(DateOnly? qtsDate, bool qtsAwardedInWales, AccessMode accessMode) =>
        qtsDate != null ?
            new GetTeacherResponseQts()
            {
                Awarded = qtsDate.Value,
                CertificateUrl = accessMode == AccessMode.IdentityAccessToken && !qtsAwardedInWales ? "/v3/certificates/qts" : null
            } :
            null;

    private static GetTeacherResponseEyts? MapEyts(DateOnly? eytsDate, AccessMode accessMode) =>
        eytsDate != null ?
            new GetTeacherResponseEyts()
            {
                Awarded = eytsDate.Value,
                CertificateUrl = accessMode == AccessMode.IdentityAccessToken ? "/v3/certificates/eyts" : null
            } :
            null;

    private static GetTeacherResponseInduction? MapInduction(dfeta_induction induction, dfeta_inductionperiod[] inductionperiods, AccessMode accessMode) =>
        induction != null ?
            new GetTeacherResponseInduction()
            {
                StartDate = induction.dfeta_StartDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                EndDate = induction.dfeta_CompletionDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                Status = MapInductionStatus(induction.dfeta_InductionStatus),
                CertificateUrl =
                    (induction.dfeta_InductionStatus == dfeta_InductionStatus.Pass || induction.dfeta_InductionStatus == dfeta_InductionStatus.PassedinWales) &&
                        induction.dfeta_CompletionDate is not null &&
                        accessMode == AccessMode.IdentityAccessToken ?
                    "/v3/certificates/induction" :
                    null,
                Periods = inductionperiods.Select(p => MapInductionPeriod(p)).ToArray()
            } :
            null;

    private static GetTeacherResponseInductionPeriod MapInductionPeriod(dfeta_inductionperiod inductionPeriod)
    {
        var appropriateBody = inductionPeriod.Extract<Account>("appropriatebody", Account.PrimaryIdAttribute);

        return new GetTeacherResponseInductionPeriod()
        {
            StartDate = inductionPeriod.dfeta_StartDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
            EndDate = inductionPeriod.dfeta_EndDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
            Terms = inductionPeriod.dfeta_Numberofterms,
            AppropriateBody = new GetTeacherResponseInductionPeriodAppropriateBody()
            {
                Name = appropriateBody.Name
            }
        };
    }

    private static InductionStatus? MapInductionStatus(dfeta_InductionStatus? inductionStatus) =>
        inductionStatus switch
        {
            dfeta_InductionStatus.Exempt => InductionStatus.Exempt,
            dfeta_InductionStatus.Fail => InductionStatus.Fail,
            dfeta_InductionStatus.FailedinWales => InductionStatus.FailedinWales,
            dfeta_InductionStatus.InductionExtended => InductionStatus.InductionExtended,
            dfeta_InductionStatus.InProgress => InductionStatus.InProgress,
            dfeta_InductionStatus.NotYetCompleted => InductionStatus.NotYetCompleted,
            dfeta_InductionStatus.Pass => InductionStatus.Pass,
            dfeta_InductionStatus.PassedinWales => InductionStatus.PassedinWales,
            dfeta_InductionStatus.RequiredtoComplete => InductionStatus.RequiredtoComplete,
            null => (InductionStatus?)null,
            _ => throw new NotImplementedException($"{nameof(InductionStatus)}: {inductionStatus} is not currently supported.")
        };

    private static GetTeacherResponseInitialTeacherTrainingQualification? MapIttQualification(dfeta_initialteachertraining initialTeacherTraining)
    {
        var qualification = initialTeacherTraining.Extract<dfeta_ittqualification>("qualification", dfeta_ittqualification.PrimaryIdAttribute);

        return qualification != null ?
            new GetTeacherResponseInitialTeacherTrainingQualification()
            {
                Name = qualification.dfeta_name
            } :
            null;
    }

    private static GetTeacherResponseInitialTeacherTrainingAgeRange? MapAgeRange(dfeta_AgeRange? ageRangeFrom, dfeta_AgeRange? ageRangeTo)
    {
        var ageRangeDescription = new StringBuilder();
        var ageRangeFromName = ageRangeFrom.HasValue ? ageRangeFrom.Value.GetMetadata().Name : null;
        var ageRangeToName = ageRangeTo.HasValue ? ageRangeTo.Value.GetMetadata().Name : null;

        if (ageRangeFromName != null)
        {
            ageRangeDescription.AppendFormat("{0} ", ageRangeFromName);
        }

        if (ageRangeToName != null)
        {
            ageRangeDescription.AppendFormat("to {0} ", ageRangeToName);
        }

        if (ageRangeDescription.Length > 0)
        {
            ageRangeDescription.Append("years");
        }

        return ageRangeDescription.Length > 0 ?
            new GetTeacherResponseInitialTeacherTrainingAgeRange()
            {
                Description = ageRangeDescription.ToString()
            } :
            null;
    }

    private static GetTeacherResponseInitialTeacherTrainingProvider? MapIttProvider(dfeta_initialteachertraining initialTeacherTraining)
    {
        var establishment = initialTeacherTraining.Extract<Account>("establishment", Account.PrimaryIdAttribute);

        return establishment != null ?
            new GetTeacherResponseInitialTeacherTrainingProvider()
            {
                Name = establishment.Name,
                Ukprn = establishment.dfeta_UKPRN
            } :
            null;
    }

    private static IEnumerable<GetTeacherResponseInitialTeacherTrainingSubject> MapSubjects(dfeta_initialteachertraining initialTeacherTraining)
    {
        var subjects = new List<GetTeacherResponseInitialTeacherTrainingSubject>();

        for (var index = 1; index <= 3; index++)
        {
            var subject = initialTeacherTraining.Extract<dfeta_ittsubject>($"subject{index}", dfeta_ittsubject.PrimaryIdAttribute);
            if (subject != null)
            {
                subjects.Add(new GetTeacherResponseInitialTeacherTrainingSubject()
                {
                    Code = subject.dfeta_Value,
                    Name = subject.dfeta_name
                });
            }
        }

        return subjects;
    }

    private static IEnumerable<GetTeacherResponseNpqQualificationsQualification> MapNpqQualifications(dfeta_qualification[] qualifications, AccessMode accessMode) =>
        qualifications
            ?.Where(q => q.dfeta_Type.HasValue
                && q.dfeta_Type.Value.IsNpq()
                && q.StateCode == dfeta_qualificationState.Active
                && q.dfeta_CompletionorAwardDate.HasValue)
            .Select(q => new GetTeacherResponseNpqQualificationsQualification()
            {
                Awarded = q.dfeta_CompletionorAwardDate!.Value.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                Type = new GetTeacherResponseNpqQualificationsQualificationType()
                {
                    Code = MapNpqQualificationType(q.dfeta_Type!.Value),
                    Name = q.dfeta_Type.Value.GetName(),
                },
                CertificateUrl = accessMode == AccessMode.IdentityAccessToken ? $"/v3/certificates/npq/{q.Id}" : null
            })
            .ToArray() ?? Array.Empty<GetTeacherResponseNpqQualificationsQualification>();

    private static NpqQualificationType MapNpqQualificationType(dfeta_qualification_dfeta_Type qualificationType) =>
        qualificationType switch
        {
            dfeta_qualification_dfeta_Type.NPQEL => NpqQualificationType.NPQEL,
            dfeta_qualification_dfeta_Type.NPQEYL => NpqQualificationType.NPQEYL,
            dfeta_qualification_dfeta_Type.NPQH => NpqQualificationType.NPQH,
            dfeta_qualification_dfeta_Type.NPQLBC => NpqQualificationType.NPQLBC,
            dfeta_qualification_dfeta_Type.NPQLL => NpqQualificationType.NPQLL,
            dfeta_qualification_dfeta_Type.NPQLT => NpqQualificationType.NPQLT,
            dfeta_qualification_dfeta_Type.NPQLTD => NpqQualificationType.NPQLTD,
            dfeta_qualification_dfeta_Type.NPQML => NpqQualificationType.NPQML,
            dfeta_qualification_dfeta_Type.NPQSL => NpqQualificationType.NPQSL,
            _ => throw new NotImplementedException($"Qualification Type {qualificationType} is not currently supported.")
        };

    private static IEnumerable<GetTeacherResponseMandatoryQualificationsQualification> MapMandatoryQualifications(dfeta_qualification[] qualifications) =>
        qualifications
            ?.Where(q => q.dfeta_Type.HasValue
                && q.dfeta_Type.Value == dfeta_qualification_dfeta_Type.MandatoryQualification
                && q.StateCode == dfeta_qualificationState.Active
                && q.dfeta_MQ_Date.HasValue)
            .Select(mq => new
            {
                Awarded = mq.dfeta_MQ_Date!.Value.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                Specialism = mq.Extract<dfeta_specialism>(dfeta_specialism.EntityLogicalName, dfeta_specialism.PrimaryIdAttribute)
            })
            .Where(mq => mq.Specialism != null)
            .Select(mq => new GetTeacherResponseMandatoryQualificationsQualification()
            {
                Awarded = mq.Awarded,
                Specialism = mq.Specialism.dfeta_name
            })
            .ToArray() ?? Array.Empty<GetTeacherResponseMandatoryQualificationsQualification>();

    private static IEnumerable<GetTeacherResponseHigherEducationQualificationsQualification> MapHigherEducationQualifications(dfeta_qualification[] qualifications) =>
        qualifications
        ?.Where(q => q.dfeta_Type.HasValue
                && q.dfeta_Type.Value == dfeta_qualification_dfeta_Type.HigherEducation
                && q.StateCode == dfeta_qualificationState.Active)
        ?.Select(q =>
        {
            var heQualification = q.Extract<dfeta_hequalification>();
            var heSubject1 = q.Extract<dfeta_hesubject>($"{nameof(dfeta_hesubject)}1", dfeta_hesubject.PrimaryIdAttribute);
            var heSubject2 = q.Extract<dfeta_hesubject>($"{nameof(dfeta_hesubject)}2", dfeta_hesubject.PrimaryIdAttribute);
            var heSubject3 = q.Extract<dfeta_hesubject>($"{nameof(dfeta_hesubject)}3", dfeta_hesubject.PrimaryIdAttribute);
            var heSubjects = new List<GetTeacherResponseHigherEducationQualificationsQualificationSubject>();
            if (heSubject1 != null)
            {
                heSubjects.Add(new GetTeacherResponseHigherEducationQualificationsQualificationSubject
                {
                    Code = heSubject1.dfeta_Value,
                    Name = heSubject1.dfeta_name,
                });
            }

            if (heSubject2 != null)
            {
                heSubjects.Add(new GetTeacherResponseHigherEducationQualificationsQualificationSubject
                {
                    Code = heSubject2.dfeta_Value,
                    Name = heSubject2.dfeta_name,
                });
            }

            if (heSubject3 != null)
            {
                heSubjects.Add(new GetTeacherResponseHigherEducationQualificationsQualificationSubject
                {
                    Code = heSubject3.dfeta_Value,
                    Name = heSubject3.dfeta_name,
                });
            }

            return new GetTeacherResponseHigherEducationQualificationsQualification
            {
                Name = heQualification.dfeta_name,
                Awarded = q.dfeta_CompletionorAwardDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                Subjects = heSubjects.ToArray()
            };
        })
        .ToArray() ?? Array.Empty<GetTeacherResponseHigherEducationQualificationsQualification>();
}
