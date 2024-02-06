using System.Text;
using MediatR;
using Microsoft.Xrm.Sdk.Query;
using Optional;
using TeachingRecordSystem.Api.V3.ApiModels;
using TeachingRecordSystem.Api.V3.Requests;
using TeachingRecordSystem.Api.V3.Responses;
using TeachingRecordSystem.Api.V3.V20240101.ApiModels;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;

namespace TeachingRecordSystem.Api.V3.Handlers;

public class GetTeacherHandler : IRequestHandler<GetTeacherRequest, GetTeacherResponse?>
{
    private const string QtsAwardedInWalesTeacherStatusValue = "213";

    private readonly IDataverseAdapter _dataverseAdapter;
    private readonly ICrmQueryDispatcher _crmQueryDispatcher;
    private readonly TimeSpan _concurrentNameChangeWindow;
    private readonly ReferenceDataCache _referenceDataCache;

    public GetTeacherHandler(
        IDataverseAdapter dataverseAdapter,
        ICrmQueryDispatcher crmQueryDispatcher,
        IConfiguration configuration,
        ReferenceDataCache referenceDataCache)
    {
        _dataverseAdapter = dataverseAdapter;
        _crmQueryDispatcher = crmQueryDispatcher;
        _concurrentNameChangeWindow = TimeSpan.FromSeconds(configuration.GetValue<int>("ConcurrentNameChangeWindowSeconds", 5));
        _referenceDataCache = referenceDataCache;
    }

    public async Task<GetTeacherResponse?> Handle(GetTeacherRequest request, CancellationToken cancellationToken)
    {
        var contactDetail = await _crmQueryDispatcher.ExecuteQuery(
            new GetContactDetailByTrnQuery(
                request.Trn,
                new ColumnSet(
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
                    Contact.Fields.EMailAddress1,
                    Contact.Fields.dfeta_AllowIDSignInWithProhibitions)));

        if (contactDetail is null)
        {
            return null;
        }

        var teacher = contactDetail.Contact;

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
                activeOnly: true);
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
            var nameChangeSubject = await _dataverseAdapter.GetSubjectByTitle("Change of Name");
            var dateOfBirthChangeSubject = await _dataverseAdapter.GetSubjectByTitle("Change of Date of Birth");

            var incidents = await _dataverseAdapter.GetIncidentsByContactId(
                teacher.Id,
                IncidentState.Active,
                columnNames: new[] { Incident.Fields.SubjectId, Incident.Fields.StateCode });

            pendingNameChange = incidents.Any(i => i.SubjectId.Id == nameChangeSubject.Id);
            pendingDateOfBirthChange = incidents.Any(i => i.SubjectId.Id == dateOfBirthChangeSubject.Id);
        }

        SanctionResult[]? sanctions = null;

        if (request.Include.HasFlag(GetTeacherRequestIncludes.Sanctions) || request.Include.HasFlag(GetTeacherRequestIncludes.Alerts))
        {
            var getSanctionsQuery = new GetSanctionsByContactIdsQuery(
                new[] { teacher.Id },
                ActiveOnly: true,
                ColumnSet: new(
                    dfeta_sanction.Fields.dfeta_StartDate,
                    dfeta_sanction.Fields.dfeta_EndDate,
                    dfeta_sanction.Fields.dfeta_Spent));

            sanctions = (await _crmQueryDispatcher.ExecuteQuery(getSanctionsQuery))[teacher.Id];
        }

        IEnumerable<NameInfo>? previousNames = null;

        if (request.Include.HasFlag(GetTeacherRequestIncludes.PreviousNames))
        {
            previousNames = PreviousNameHelper.GetFullPreviousNames(contactDetail.PreviousNames, contactDetail.Contact, _concurrentNameChangeWindow)
                .Select(name => new NameInfo()
                {
                    FirstName = name.FirstName,
                    MiddleName = name.MiddleName,
                    LastName = name.LastName
                })
                .ToArray();
        }

        var firstName = teacher.ResolveFirstName();
        var middleName = teacher.ResolveMiddleName();
        var lastName = teacher.ResolveLastName();

        var qtsAwardedInWalesStatus = await _dataverseAdapter.GetTeacherStatus(QtsAwardedInWalesTeacherStatusValue, null);
        var qtsRegistrations = await _dataverseAdapter.GetQtsRegistrationsByTeacher(
            teacher.Id,
            columnNames: new[]
            {
                dfeta_qtsregistration.Fields.dfeta_QTSDate,
                dfeta_qtsregistration.Fields.dfeta_TeacherStatusId,
                dfeta_qtsregistration.Fields.dfeta_name,
                dfeta_qtsregistration.Fields.dfeta_EYTSDate,
                dfeta_qtsregistration.Fields.dfeta_EarlyYearsStatusId
            });


        var qts = qtsRegistrations.OrderByDescending(x => x.CreatedOn).FirstOrDefault(qts => qts.dfeta_QTSDate is not null);
        var eyts = qtsRegistrations.OrderByDescending(x => x.CreatedOn).FirstOrDefault(qts => qts.dfeta_EYTSDate is not null);
        var qtsAwardedInWales = qts?.dfeta_TeacherStatusId.Id == qtsAwardedInWalesStatus.Id;
        var eytsTeacherStatus = eyts != null ? await _dataverseAdapter.GetEarlyYearsStatus(eyts!.dfeta_EarlyYearsStatusId.Id) : null;
        var allTeacherStatuses = await _referenceDataCache.GetTeacherStatuses();
        var qtsStatus = qts != null ? allTeacherStatuses.Single(x => x.Id == qts.dfeta_TeacherStatusId.Id) : null;

        var allowIdSignInWithProhibitions = request.Include.HasFlag(GetTeacherRequestIncludes._AllowIdSignInWithProhibitions) ?
            Option.Some(teacher.dfeta_AllowIDSignInWithProhibitions == true) :
            default;

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
            Qts = MapQts(qts?.dfeta_QTSDate?.ToDateOnlyWithDqtBstFix(isLocalTime: true), qtsAwardedInWales, request.AccessMode, qtsStatus != null ? GetQTSStatusDescription(qtsStatus!.dfeta_Value!, qtsStatus.dfeta_name) : null),
            Eyts = MapEyts(eyts?.dfeta_EYTSDate?.ToDateOnlyWithDqtBstFix(isLocalTime: true), request.AccessMode, eytsTeacherStatus != null ? GetEytsStatusDescription(eytsTeacherStatus!.dfeta_Value!) : null),
            Email = teacher.EMailAddress1,
            Induction = request.Include.HasFlag(GetTeacherRequestIncludes.Induction) ?
                Option.Some(MapInduction(induction!, inductionPeriods!, request.AccessMode)) :
                default,
            InitialTeacherTraining = request.Include.HasFlag(GetTeacherRequestIncludes.InitialTeacherTraining) ?
                Option.Some(itt!
                    .Select(i => new GetTeacherResponseInitialTeacherTraining()
                    {
                        Qualification = MapIttQualification(i),
                        ProgrammeType = i.dfeta_ProgrammeType?.ConvertToEnumByValue<dfeta_ITTProgrammeType, IttProgrammeType>(),
                        ProgrammeTypeDescription = i.dfeta_ProgrammeType?.ConvertToEnumByValue<dfeta_ITTProgrammeType, IttProgrammeType>().GetDescription(),
                        StartDate = i.dfeta_ProgrammeStartDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                        EndDate = i.dfeta_ProgrammeEndDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                        Result = i.dfeta_Result.HasValue ? i.dfeta_Result.Value.ConvertFromITTResult() : null,
                        AgeRange = MapAgeRange(i.dfeta_AgeRangeFrom, i.dfeta_AgeRangeTo),
                        Provider = MapIttProvider(i),
                        Subjects = MapSubjects(i)
                    })
                    .AsReadOnly()) :
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
                Option.Some(sanctions!
                    .Where(s => Constants.ExposableSanctionCodes.Contains(s.SanctionCode))
                    .Where(s => s.Sanction.dfeta_EndDate is null && s.Sanction.dfeta_Spent != true)
                    .Select(s => new SanctionInfo()
                    {
                        Code = s.SanctionCode,
                        StartDate = s.Sanction.dfeta_StartDate?.ToDateOnlyWithDqtBstFix(isLocalTime: true)
                    })
                    .AsReadOnly()) :
                default,
            Alerts = request.Include.HasFlag(GetTeacherRequestIncludes.Alerts) ?
                Option.Some(sanctions!
                    .Where(s => Constants.ProhibitionSanctionCodes.Contains(s.SanctionCode))
                    .Select(s => new AlertInfo()
                    {
                        AlertType = AlertType.Prohibition,
                        DqtSanctionCode = s.SanctionCode,
                        StartDate = s.Sanction.dfeta_StartDate?.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                        EndDate = s.Sanction.dfeta_EndDate?.ToDateOnlyWithDqtBstFix(isLocalTime: true)
                    })
                    .AsReadOnly()) :
                default,
            PreviousNames = request.Include.HasFlag(GetTeacherRequestIncludes.PreviousNames) ?
                Option.Some(previousNames!.Select(n => n).AsReadOnly()) :
                default,
            AllowIdSignInWithProhibitions = allowIdSignInWithProhibitions
        };
    }

    private string? GetEytsStatusDescription(string? value) => value switch
    {
        "222" => "Early years professional status",
        "221" => "Qualified",
        "220" => "Early years trainee",
        _ => throw new ArgumentException("Invalid EYTS Status")
    };

    private string? GetQTSStatusDescription(string value, string statusDescription) => value switch
    {
        "28" => "Qualified",
        "50" => "Qualified",
        "67" => "Qualified",
        "68" => "Qualified",
        "69" => "Qualified",
        "71" => "Qualified",
        "87" => "Qualified",
        "90" => "Qualified",
        "100" => "Qualified",
        "103" => "Qualified",
        "104" => "Qualified",
        "206" => "Qualified",
        "211" => "Trainee teacher",
        "212" => "Assessment only route candidate",
        "213" => "Qualified",
        "214" => "Partial qualified teacher status",
        "223" => "Qualified",
        _ when statusDescription.StartsWith("Qualified teacher", StringComparison.InvariantCultureIgnoreCase) => "Qualified",
        _ => throw new ArgumentException("Invalid QTS Status")
    };


    private static GetTeacherResponseQts? MapQts(DateOnly? qtsDate, bool qtsAwardedInWales, AccessMode accessMode, string? statusDescription) =>
        statusDescription is not null ?
            new GetTeacherResponseQts()
            {
                Awarded = qtsDate,
                CertificateUrl = accessMode == AccessMode.IdentityAccessToken && !qtsAwardedInWales && qtsDate.HasValue ? "/v3/certificates/qts" : null,
                StatusDescription = statusDescription
            } :
            null;

    private static GetTeacherResponseEyts? MapEyts(DateOnly? eytsDate, AccessMode accessMode, string? statusDescription) =>
        statusDescription != null ?
            new GetTeacherResponseEyts()
            {
                Awarded = eytsDate,
                CertificateUrl = accessMode == AccessMode.IdentityAccessToken ? "/v3/certificates/eyts" : null,
                StatusDescription = statusDescription
            } :
            null;

    private static GetTeacherResponseInduction? MapInduction(dfeta_induction induction, dfeta_inductionperiod[] inductionperiods, AccessMode accessMode) =>
        induction != null ?
            new GetTeacherResponseInduction()
            {
                StartDate = induction.dfeta_StartDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                EndDate = induction.dfeta_CompletionDate.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                Status = induction.dfeta_InductionStatus?.ConvertToInductionStatus(),
                StatusDescription = induction.dfeta_InductionStatus?.GetDescription(),
                CertificateUrl =
                    (induction.dfeta_InductionStatus == dfeta_InductionStatus.Pass || induction.dfeta_InductionStatus == dfeta_InductionStatus.PassedinWales) &&
                        induction.dfeta_CompletionDate is not null &&
                        accessMode == AccessMode.IdentityAccessToken ?
                    "/v3/certificates/induction" :
                    null,
                Periods = inductionperiods.Select(MapInductionPeriod).ToArray()
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
            AppropriateBody = appropriateBody is not null ?
                new GetTeacherResponseInductionPeriodAppropriateBody()
                {
                    Name = appropriateBody.Name
                } :
                null
        };
    }

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

    private static IReadOnlyCollection<GetTeacherResponseInitialTeacherTrainingSubject> MapSubjects(dfeta_initialteachertraining initialTeacherTraining)
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

    private static IReadOnlyCollection<GetTeacherResponseNpqQualification> MapNpqQualifications(dfeta_qualification[] qualifications, AccessMode accessMode) =>
        qualifications
            ?.Where(q => q.dfeta_Type.HasValue
                && q.dfeta_Type.Value.IsNpq()
                && q.StateCode == dfeta_qualificationState.Active
                && q.dfeta_CompletionorAwardDate.HasValue)
            .Select(q => new GetTeacherResponseNpqQualification()
            {
                Awarded = q.dfeta_CompletionorAwardDate!.Value.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                Type = new GetTeacherResponseNpqQualificationType()
                {
                    Code = MapNpqQualificationType(q.dfeta_Type!.Value),
                    Name = q.dfeta_Type.Value.GetName(),
                },
                CertificateUrl = accessMode == AccessMode.IdentityAccessToken ? $"/v3/certificates/npq/{q.Id}" : null
            })
            .ToArray() ?? [];

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

    private static IReadOnlyCollection<GetTeacherResponseMandatoryQualification> MapMandatoryQualifications(dfeta_qualification[] qualifications) =>
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
            .Select(mq => new GetTeacherResponseMandatoryQualification()
            {
                Awarded = mq.Awarded,
                Specialism = mq.Specialism.dfeta_name
            })
            .ToArray() ?? [];

    private static IReadOnlyCollection<GetTeacherResponseHigherEducationQualification> MapHigherEducationQualifications(dfeta_qualification[] qualifications) =>
        qualifications
            ?.Where(q =>
                q.dfeta_Type.HasValue &&
                q.dfeta_Type.Value == dfeta_qualification_dfeta_Type.HigherEducation &&
                q.StateCode == dfeta_qualificationState.Active &&
                q.Extract<dfeta_hequalification>() is not null)
            ?.Select(q =>
            {
                var heQualification = q.Extract<dfeta_hequalification>();

                var heSubject1 = q.Extract<dfeta_hesubject>($"{nameof(dfeta_hesubject)}1", dfeta_hesubject.PrimaryIdAttribute);
                var heSubject2 = q.Extract<dfeta_hesubject>($"{nameof(dfeta_hesubject)}2", dfeta_hesubject.PrimaryIdAttribute);
                var heSubject3 = q.Extract<dfeta_hesubject>($"{nameof(dfeta_hesubject)}3", dfeta_hesubject.PrimaryIdAttribute);

                var heSubjects = new List<GetTeacherResponseHigherEducationQualificationSubject>();

                if (heSubject1 != null)
                {
                    heSubjects.Add(new GetTeacherResponseHigherEducationQualificationSubject
                    {
                        Code = heSubject1.dfeta_Value,
                        Name = heSubject1.dfeta_name,
                    });
                }

                if (heSubject2 != null)
                {
                    heSubjects.Add(new GetTeacherResponseHigherEducationQualificationSubject
                    {
                        Code = heSubject2.dfeta_Value,
                        Name = heSubject2.dfeta_name,
                    });
                }

                if (heSubject3 != null)
                {
                    heSubjects.Add(new GetTeacherResponseHigherEducationQualificationSubject
                    {
                        Code = heSubject3.dfeta_Value,
                        Name = heSubject3.dfeta_name,
                    });
                }

                return new GetTeacherResponseHigherEducationQualification
                {
                    Name = heQualification.dfeta_name,
                    Awarded = q.dfeta_CompletionorAwardDate?.ToDateOnlyWithDqtBstFix(isLocalTime: true),
                    Subjects = heSubjects.ToArray()
                };
            })
            .ToArray() ?? [];
}
