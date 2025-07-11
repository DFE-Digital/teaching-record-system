using System.Diagnostics;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Dqt.Queries;
using TeachingRecordSystem.Core.Services.DqtOutbox;
using TeachingRecordSystem.Core.Services.DqtOutbox.Messages;

namespace TeachingRecordSystem.Api.V3.Implementation.Operations;

public partial class SetRouteToProfessionalStatusHandler
{
    public async Task<ApiResult<SetRouteToProfessionalStatusResult>> HandleOverDqtAsync(SetRouteToProfessionalStatusCommand command)
    {
        var dqtContact = await crmQueryDispatcher.ExecuteQueryAsync(
            new GetActiveContactByTrnQuery(
                command.Trn,
                new ColumnSet(
                    Contact.Fields.dfeta_SlugId,
                    Contact.Fields.dfeta_HUSID,
                    Contact.Fields.dfeta_QTSDate,
                    Contact.Fields.dfeta_EYTSDate)));
        if (dqtContact is null)
        {
            return ApiError.PersonNotFound(command.Trn);
        }

        // Alerts and inductions are in TRS so make sure contact record is synced to TRS
        var person = await GetPersonAsync();
        if (person is null)
        {
            var synced = await syncHelper.SyncPersonAsync(dqtContact.Id, syncAudit: true);
            if (!synced)
            {
                throw new Exception($"Could not sync Person with contact ID: '{dqtContact.Id}'.");
            }

            person = await GetPersonAsync();
            Debug.Assert(person is not null);
        }

        dfeta_ITTProgrammeType? mappedIttProgrammeType = null;
        if (!command.RouteToProfessionalStatusTypeId.IsOverseas() && !command.RouteToProfessionalStatusTypeId.TryConvertFromTrsRouteType(out mappedIttProgrammeType))
        {
            return ApiError.InvalidRouteType(command.RouteToProfessionalStatusTypeId);
        }

        var ittResult = GetIttResultFromStatus(command.Status, command.RouteToProfessionalStatusTypeId.IsOverseas());

        var lookupData = await LookupDataAsync(dqtContact, command.SourceApplicationReference, command.RouteToProfessionalStatusTypeId, command.Status);
        var isEarlyYears = command.RouteToProfessionalStatusTypeId.IsEarlyYears();
        var inductionExemptionId = command.IsExemptFromInduction.HasValue && command.IsExemptFromInduction.Value ? DeriveInductionExemptionId(command.RouteToProfessionalStatusTypeId) : null;

        (dfeta_AgeRange From, dfeta_AgeRange To)? ageRange = null;
        if (command.TrainingAgeSpecialism is not null && !command.TrainingAgeSpecialism.TryConvertToAgeRange(out ageRange))
        {
            return ApiError.InvalidTrainingAgeSpecialism(command.TrainingAgeSpecialism.Type.ToString());
        }

        Guid? subjectId1 = null;
        if (command.TrainingSubjectReferences?.Length > 0)
        {
            var (IsSuccess, Result) = await command.TrainingSubjectReferences[0].TryConvertFromTrsTrainingSubjectReferenceAsync(referenceDataCache);
            if (!IsSuccess)
            {
                return ApiError.InvalidTrainingSubjectReference(command.TrainingSubjectReferences[0]);
            }

            subjectId1 = Result!.Id;
        }

        Guid? subjectId2 = null;
        if (command.TrainingSubjectReferences?.Length > 1)
        {
            var (IsSuccess, Result) = await command.TrainingSubjectReferences[1].TryConvertFromTrsTrainingSubjectReferenceAsync(referenceDataCache);
            if (!IsSuccess)
            {
                return ApiError.InvalidTrainingSubjectReference(command.TrainingSubjectReferences[1]);
            }

            subjectId2 = Result!.Id;
        }

        Guid? subjectId3 = null;
        if (command.TrainingSubjectReferences?.Length > 2)
        {
            var (IsSuccess, Result) = await command.TrainingSubjectReferences[2].TryConvertFromTrsTrainingSubjectReferenceAsync(referenceDataCache);
            if (!IsSuccess)
            {
                return ApiError.InvalidTrainingSubjectReference(command.TrainingSubjectReferences[2]);
            }

            subjectId3 = Result!.Id;
        }

        Guid? countryId = null;
        if (command.TrainingCountryReference is not null)
        {
            var (IsSuccess, Result) = await command.TrainingCountryReference!.TryConvertFromTrsCountryReferenceAsync(referenceDataCache);
            if (!IsSuccess)
            {
                return ApiError.InvalidTrainingCountryReference(command.TrainingCountryReference!);
            }

            countryId = Result!.Id;
        }

        Guid? providerId = null;
        if (command.TrainingProviderUkprn is not null)
        {
            var (IsSuccess, Result) = await command.TrainingProviderUkprn.TryConvertFromUkprnAsync(referenceDataCache);
            if (!IsSuccess)
            {
                return ApiError.InvalidTrainingProviderUkprn(command.TrainingProviderUkprn);
            }

            providerId = Result!.Id;
        }

        if (command.RouteToProfessionalStatusTypeId.IsOverseas())
        {
            providerId = await DeriveIttProviderForOverseasQualifiedTeacherAsync(command.RouteToProfessionalStatusTypeId);
        }

        Guid? ittQualificationId = null;
        if (command.DegreeTypeId is not null)
        {
            var (IsSuccess, Result) = await command.DegreeTypeId.Value.TryConvertFromTrsDegreeTypeIdAsync(referenceDataCache);
            if (!IsSuccess)
            {
                return ApiError.InvalidDegreeType(command.DegreeTypeId.Value);
            }

            ittQualificationId = Result!.Id;
        }

        var itt = lookupData.Itt;
        var isNewItt = itt is null;
        var cohortYear = command.TrainingEndDate?.Year.ToString();
        var existingProgrammeType = itt?.dfeta_ProgrammeType;
        if (isNewItt)
        {
            itt = new dfeta_initialteachertraining
            {
                Id = Guid.NewGuid(),
                dfeta_PersonId = dqtContact.Id.ToEntityReference(Contact.EntityLogicalName),
                dfeta_SlugId = command.SourceApplicationReference,
                dfeta_TraineeID = dqtContact.dfeta_HUSID
            };
        }
        else
        {
            // No current provision for changing overseas
            if (command.RouteToProfessionalStatusTypeId.IsOverseas())
            {
                return ApiError.UpdatesNotAllowedForRouteType(command.RouteToProfessionalStatusTypeId);
            }

            // Can't change between Early Years and non-Early Years
            if (isEarlyYears != existingProgrammeType?.IsEarlyYears())
            {
                return ApiError.UnableToChangeRouteType();
            }

            switch (itt!.dfeta_Result)
            {
                // If the route has already been Awarded then this can't be changed via the API - needs to be altered via TRS console
                case dfeta_ITTResult.Pass:
                    return ApiError.RouteToProfessionalStatusAlreadyAwarded();
                case dfeta_ITTResult.Fail:
                    switch (command.Status)
                    {
                        case RouteToProfessionalStatusStatus.Deferred:
                        case RouteToProfessionalStatusStatus.InTraining:
                        case RouteToProfessionalStatusStatus.UnderAssessment:
                            return ApiError.UnableToChangeFailProfessionalStatusStatus();
                    }
                    break;
                case dfeta_ITTResult.Withdrawn:
                    if (command.Status == RouteToProfessionalStatusStatus.Deferred)
                    {
                        return ApiError.UnableToChangeWithdrawnProfessionalStatusStatus();
                    }
                    break;
            }
        }

        itt!.dfeta_EstablishmentId = providerId?.ToEntityReference(Account.EntityLogicalName);
        itt.dfeta_ProgrammeStartDate = command.TrainingStartDate.ToDateTimeWithDqtBstFix(isLocalTime: true);
        itt.dfeta_ProgrammeEndDate = command.TrainingEndDate.ToDateTimeWithDqtBstFix(isLocalTime: true);
        itt.dfeta_ProgrammeType = mappedIttProgrammeType;
        itt.dfeta_CohortYear = cohortYear;
        itt.dfeta_Subject1Id = subjectId1?.ToEntityReference(dfeta_ittsubject.EntityLogicalName);
        itt.dfeta_Subject2Id = subjectId2?.ToEntityReference(dfeta_ittsubject.EntityLogicalName);
        itt.dfeta_Subject3Id = subjectId3?.ToEntityReference(dfeta_ittsubject.EntityLogicalName);
        itt.dfeta_AgeRangeFrom = ageRange is not null ? ageRange!.Value.From : null;
        itt.dfeta_AgeRangeTo = ageRange is not null ? ageRange!.Value.To : null;
        itt.dfeta_Result = ittResult;
        itt.dfeta_ITTQualificationId = ittQualificationId?.ToEntityReference(dfeta_ittqualification.EntityLogicalName);
        itt.dfeta_CountryId = countryId?.ToEntityReference(dfeta_country.EntityLogicalName);

        var qtsRegistration = !isNewItt && itt!.dfeta_qtsregistration is not null ? lookupData.QtsRegistrations.FirstOrDefault(qts => qts.dfeta_qtsregistrationId == itt!.dfeta_qtsregistration.Id) : null;
        if (qtsRegistration is null)
        {
            // Programme type could change in this API call so need to match against original one if there is a matching QTS record
            var compatibleQtsRegistrations = SelectCompatibleQtsRegistrationRecords(
                lookupData.QtsRegistrations,
                existingProgrammeType ?? (mappedIttProgrammeType.HasValue ? mappedIttProgrammeType!.Value : null),
                isEarlyYears,
                lookupData.EarlyYearsTraineeStatusId,
                lookupData.AorCandidateTeacherStatusId,
                lookupData.TraineeTeacherDmsTeacherStatusId);

            if (compatibleQtsRegistrations.Count() > 1)
            {
                return ApiError.MultipleQtsRecords();
            }

            qtsRegistration = compatibleQtsRegistrations.SingleOrDefault();
        }

        var isNewQts = qtsRegistration is null;
        if (isNewQts)
        {
            qtsRegistration = new dfeta_qtsregistration
            {
                Id = Guid.NewGuid(),
                dfeta_PersonId = dqtContact.Id.ToEntityReference(Contact.EntityLogicalName)
            };
        }
        else
        {
            // Remove existing teaching status if status is withdrawn
            if (command.Status == RouteToProfessionalStatusStatus.Withdrawn)
            {
                if (isEarlyYears)
                {
                    qtsRegistration!.Attributes[dfeta_qtsregistration.Fields.dfeta_EarlyYearsStatusId] = null;
                }
                else
                {
                    qtsRegistration!.Attributes[dfeta_qtsregistration.Fields.dfeta_TeacherStatusId] = null;
                }
            }
        }

        if (command.Status != RouteToProfessionalStatusStatus.Withdrawn)
        {
            // Set teaching status and awarded date appropriate to route type and status in API call
            if (isEarlyYears)
            {
                qtsRegistration!.dfeta_EarlyYearsStatusId = lookupData.DerivedEarlyYearsTeacherStatus!.Id.ToEntityReference(dfeta_earlyyearsstatus.EntityLogicalName);
                if (command.Status == RouteToProfessionalStatusStatus.Holds)
                {
                    qtsRegistration.dfeta_EYTSDate = command.HoldsFrom!.Value.ToDateTimeWithDqtBstFix(isLocalTime: true);
                }
            }
            else
            {
                qtsRegistration!.dfeta_TeacherStatusId = lookupData.DerivedTeacherStatus!.Id.ToEntityReference(dfeta_teacherstatus.EntityLogicalName);
                if (command.Status == RouteToProfessionalStatusStatus.Holds)
                {
                    qtsRegistration.dfeta_QTSDate = command.HoldsFrom!.Value.ToDateTimeWithDqtBstFix(isLocalTime: true);
                }
            }
        }

        // Create link between ITT and QTS if it doesn't exist yet
        var updateIttLinkToQts = itt.dfeta_qtsregistration is null;

        // Trigger induction related actions if status is awarded or approved
        dfeta_TrsOutboxMessage? inductionOutboxMessage = null;
        if (command.Status == RouteToProfessionalStatusStatus.Holds)
        {
            var (currentUserId, _) = currentUserProvider.GetCurrentApplicationUser();

            var serializer = new MessageSerializer();
            if (inductionExemptionId.HasValue)
            {
                inductionOutboxMessage = serializer.CreateCrmOutboxMessage(new AddInductionExemptionMessage()
                {
                    PersonId = dqtContact.Id,
                    ExemptionReasonId = inductionExemptionId.Value,
                    TrsUserId = currentUserId
                });
            }
            else
            {
                inductionOutboxMessage = serializer.CreateCrmOutboxMessage(new SetInductionRequiredToCompleteMessage()
                {
                    PersonId = dqtContact.Id,
                    TrsUserId = currentUserId
                });
            }
        }

        var setQuery = new SetProfessionalStatusQuery(
            dqtContact.Id,
            command.Trn,
            lookupData.HasActiveAlert,
            itt!,
            isNewItt,
            qtsRegistration!,
            isNewQts,
            updateIttLinkToQts,
            inductionOutboxMessage);

        await crmQueryDispatcher.ExecuteQueryAsync(setQuery);

        return new SetRouteToProfessionalStatusResult();

        Task<Person?> GetPersonAsync() => dbContext.Persons.SingleOrDefaultAsync(p => p.Trn == command.Trn);
    }

    public IEnumerable<dfeta_qtsregistration> SelectCompatibleQtsRegistrationRecords(
        IEnumerable<dfeta_qtsregistration> qtsRecords,
        dfeta_ITTProgrammeType? programmeType,
        bool isEarlyYears,
        Guid earlyYearsTraineeStatusId,
        Guid aorCandidateTeacherStatusId,
        Guid traineeTeacherDmsTeacherStatusId)
    {
        // Find an active QTS registration entity where either
        //   programme type is Early Years and the Early Years Status is 220 ('Early Years Trainee') *OR*
        //   programme type is AssessmentOnly and the Teacher Status is 212 ('AOR Candidate') *OR*
        //   programme type is neither Early Years nor Assessment Only and Teacher Status is 211 ('Trainee Teacher:DMS') *OR*
        var matching = new List<dfeta_qtsregistration>();

        foreach (var qts in qtsRecords)
        {
            if (isEarlyYears)
            {
                //programme type is Early Years and the Early Years Status is 220 ('Early Years Trainee') *OR*
                if (qts.dfeta_EarlyYearsStatusId?.Id == earlyYearsTraineeStatusId)
                {
                    matching.Add(qts);
                }
            }
            else
            {
                //programme type is AssessmentOnly and the Teacher Status is 212('AOR Candidate') * OR *
                if (programmeType == dfeta_ITTProgrammeType.AssessmentOnlyRoute && qts.dfeta_TeacherStatusId?.Id == aorCandidateTeacherStatusId)
                {
                    matching.Add(qts);
                }

                //programme type is neither Early Years nor Assessment Only and Teacher Status is 211 ('Trainee Teacher:DMS')
                else if ((programmeType != dfeta_ITTProgrammeType.AssessmentOnlyRoute && qts.dfeta_TeacherStatusId?.Id == traineeTeacherDmsTeacherStatusId))
                {
                    matching.Add(qts);
                }
            }
        }

        // if there are no matches based on status and programme type then look for Withdrawn ones i.e. with no status set
        if (matching.Count == 0)
        {
            if (isEarlyYears)
            {
                matching.AddRange(qtsRecords.Where(qts => qts.dfeta_EarlyYearsStatusId is null));
            }
            else
            {
                matching.AddRange(qtsRecords.Where(qts => qts.dfeta_TeacherStatusId is null));
            }
        }

        return matching;
    }

    public async Task<dfeta_teacherstatus?> DeriveTeacherStatusAsync(Guid routeTypeId, RouteToProfessionalStatusStatus status)
    {
        if (routeTypeId.IsEarlyYears())
        {
            return null;
        }

        var teacherStatusValue = routeTypeId switch
        {
            var guid when guid == RouteToProfessionalStatusType.ApplyForQtsId => "104", // Apply for QTS -> Qualified Teacher (by virtue of non-UK teaching qualifications)
            var guid when guid == RouteToProfessionalStatusType.EuropeanRecognitionId => "223",  // European Recognition -> Qualified Teacher (by virtue of European teaching qualifications)
            var guid when guid == RouteToProfessionalStatusType.NiRId => "69",  // NI R -> Qualified Teacher: Teachers trained/recognised by the Department of Education for Northern Ireland (DENI)
            var guid when guid == RouteToProfessionalStatusType.OverseasTrainedTeacherRecognitionId => "103",  // Overseas Trained Teacher Recognition -> Qualified Teacher: By virtue of overseas qualifications
            var guid when guid == RouteToProfessionalStatusType.ScotlandRId => "68",  // Scotland R -> Qualified Teacher: Teachers trained/registered in Scotland
            var guid when guid == RouteToProfessionalStatusType.InternationalQualifiedTeacherStatusId =>
                status switch
                {
                    RouteToProfessionalStatusStatus.Holds => "90", // International Qualified Teacher Status (Awarded) -> Qualified teacher: by virtue of achieving international qualified teacher status
                    RouteToProfessionalStatusStatus.Withdrawn => null, // International Qualified Teacher Status (Withdrawn) -> null
                    _ => "211" // International Qualified Teacher Status (Not Awarded) -> Trainee Teacher
                },
            var guid when guid == RouteToProfessionalStatusType.AssessmentOnlyRouteId =>
                status switch
                {
                    RouteToProfessionalStatusStatus.Holds => "100",  // Assessment Only Route (Awarded) -> Qualified Teacher: Assessment Only Route
                    RouteToProfessionalStatusStatus.Withdrawn => null, // Assessment Only Route (Withdrawn) -> null
                    _ => "212",  // Assessment Only Route (Not Awarded) -> AOR Candidate
                },
            _ =>
                status switch
                {
                    RouteToProfessionalStatusStatus.Holds => "71", // Other route types (Awarded) -> Qualified teacher (trained)
                    RouteToProfessionalStatusStatus.Withdrawn => null, // Other route types (Withdrawn) -> null
                    _ => "211" // Other route types -> Trainee Teacher
                },
        };

        if (teacherStatusValue is null)
        {
            return null;
        }

        var teacherStatus = await referenceDataCache.GetTeacherStatusByValueAsync(teacherStatusValue);
        return teacherStatus;
    }

    public async Task<dfeta_earlyyearsstatus?> DeriveEarlyYearsTeacherStatusAsync(Guid routeTypeId, RouteToProfessionalStatusStatus status)
    {
        if (!routeTypeId.IsEarlyYears())
        {
            return null;
        }

        var earlyYearsTeacherStatusValue = status switch
        {
            RouteToProfessionalStatusStatus.Holds => "221", // Early Years Teacher Status
            RouteToProfessionalStatusStatus.Withdrawn => null,
            _ => "220" // Early Years Trainee
        };

        if (earlyYearsTeacherStatusValue is null)
        {
            return null;
        }

        var earlyYearsTeacherStatus = await referenceDataCache.GetEarlyYearsStatusByValueAsync(earlyYearsTeacherStatusValue);
        return earlyYearsTeacherStatus;
    }

    public async Task<Guid> DeriveIttProviderForOverseasQualifiedTeacherAsync(Guid routeTypeId)
    {
        var ittProviderName = routeTypeId switch
        {
            var guid when guid == RouteToProfessionalStatusType.ApplyForQtsId => "Non-UK establishment", // Apply for QTS
            var guid when guid == RouteToProfessionalStatusType.EuropeanRecognitionId => "Non-UK establishment", // European Recognition
            var guid when guid == RouteToProfessionalStatusType.NiRId => "UK establishment (Scotland/Northern Ireland)", // NI R
            var guid when guid == RouteToProfessionalStatusType.OverseasTrainedTeacherRecognitionId => "Non-UK establishment",  // Overseas Trained Teacher Recognition
            var guid when guid == RouteToProfessionalStatusType.ScotlandRId => "UK establishment (Scotland/Northern Ireland)", // Scotland R
            _ => throw new ArgumentException($"Unknown route type ID: '{routeTypeId}'.", nameof(routeTypeId))
        };

        var ittProvider = await referenceDataCache.GetIttProviderByNameAsync(ittProviderName);
        return ittProvider!.Id;
    }

    public Guid? DeriveInductionExemptionId(Guid routeTypeId) => routeTypeId switch
    {
        var guid when guid == RouteToProfessionalStatusType.ApplyForQtsId => InductionExemptionReason.OverseasTrainedTeacherId, // Apply for QTS -> Overseas Trained Teacher
        var guid when guid == RouteToProfessionalStatusType.NiRId => InductionExemptionReason.PassedInductionInNorthernIrelandId, // NI R -> Passed induction in Northern Ireland
        var guid when guid == RouteToProfessionalStatusType.ScotlandRId => InductionExemptionReason.HasOrIsEligibleForFullRegistrationInScotlandId, // Scotland R -> Has, or is eligible for, full registration in Scotland
        var guid when guid == RouteToProfessionalStatusType.QtlsAndSetMembershipId => InductionExemptionReason.QtlsId, // QTLS and SET Membership -> Exempt through QTLS status provided they maintain membership of The Society of Education and Training
        _ => null
    };

    private async Task<SetProfessionalStatusLookupResult> LookupDataAsync(Contact contact, string sourceApplicationReference, Guid routeTypeId, RouteToProfessionalStatusStatus status)
    {
        var ittTask = crmQueryDispatcher.ExecuteQueryAsync(
            new GetActiveInitialTeacherTrainingRecordByContactIdAndSlugIdQuery(contact.Id, sourceApplicationReference));

        var qtsRegistrationsTask = crmQueryDispatcher.ExecuteQueryAsync(
            new GetActiveQtsRegistrationsByContactIdsQuery(
                [contact.Id],
                new ColumnSet(
                    dfeta_qtsregistration.Fields.dfeta_PersonId,
                    dfeta_qtsregistration.Fields.dfeta_QTSDate,
                    dfeta_qtsregistration.Fields.dfeta_EYTSDate,
                    dfeta_qtsregistration.Fields.dfeta_TeacherStatusId,
                    dfeta_qtsregistration.Fields.dfeta_EarlyYearsStatusId)
                )
            );

        var hasActiveAlertTask = dbContext.Alerts.Where(a => a.PersonId == contact.Id && a.IsOpen).AnyAsync();
        var getEarlyYearsTraineeStatusTask = referenceDataCache.GetEarlyYearsStatusByValueAsync("220");
        var getAorCandidateTeacherStatusTask = referenceDataCache.GetTeacherStatusByValueAsync("212");
        var getTraineeTeacherDmsTeacherStatusTask = referenceDataCache.GetTeacherStatusByValueAsync("211");
        var deriveTeacherStatusTask = DeriveTeacherStatusAsync(routeTypeId, status);
        var deriveEarlyYearsTeacherStatusTask = DeriveEarlyYearsTeacherStatusAsync(routeTypeId, status);
        await Task.WhenAll(
            ittTask,
            qtsRegistrationsTask,
            hasActiveAlertTask,
            getEarlyYearsTraineeStatusTask,
            getAorCandidateTeacherStatusTask,
            getTraineeTeacherDmsTeacherStatusTask,
            deriveTeacherStatusTask,
            deriveEarlyYearsTeacherStatusTask);

        return new SetProfessionalStatusLookupResult()
        {
            Teacher = contact,
            HasActiveAlert = hasActiveAlertTask.Result,
            Itt = ittTask.Result,
            QtsRegistrations = qtsRegistrationsTask.Result[contact.Id],
            EarlyYearsTraineeStatusId = getEarlyYearsTraineeStatusTask.Result.Id,
            AorCandidateTeacherStatusId = getAorCandidateTeacherStatusTask.Result.Id,
            TraineeTeacherDmsTeacherStatusId = getTraineeTeacherDmsTeacherStatusTask.Result.Id,
            DerivedTeacherStatus = deriveTeacherStatusTask.Result,
            DerivedEarlyYearsTeacherStatus = deriveEarlyYearsTeacherStatusTask.Result
        };
    }

    private static dfeta_ITTResult GetIttResultFromStatus(RouteToProfessionalStatusStatus status, bool isOverseas) =>
        status switch
        {
            RouteToProfessionalStatusStatus.InTraining => dfeta_ITTResult.InTraining,
            RouteToProfessionalStatusStatus.Holds when isOverseas => dfeta_ITTResult.Approved,
            RouteToProfessionalStatusStatus.Holds => dfeta_ITTResult.Pass,
            RouteToProfessionalStatusStatus.Deferred => dfeta_ITTResult.Deferred,
            RouteToProfessionalStatusStatus.DeferredForSkillsTest => dfeta_ITTResult.DeferredforSkillsTests,
            RouteToProfessionalStatusStatus.Failed => dfeta_ITTResult.Fail,
            RouteToProfessionalStatusStatus.Withdrawn => dfeta_ITTResult.Withdrawn,
            RouteToProfessionalStatusStatus.UnderAssessment => dfeta_ITTResult.UnderAssessment,
            _ => throw new ArgumentException($"Invalid status: '{status}'.", nameof(status))
        };

    internal class SetProfessionalStatusLookupResult
    {
        public Contact Teacher { get; set; } = null!;
        public bool HasActiveAlert { get; set; }
        public dfeta_initialteachertraining? Itt { get; set; }
        public IEnumerable<dfeta_qtsregistration> QtsRegistrations { get; set; } = [];
        public Guid EarlyYearsTraineeStatusId { get; set; }
        public Guid AorCandidateTeacherStatusId { get; set; }
        public Guid TraineeTeacherDmsTeacherStatusId { get; set; }
        public dfeta_teacherstatus? DerivedTeacherStatus { get; set; }
        public dfeta_earlyyearsstatus? DerivedEarlyYearsTeacherStatus { get; set; }
    }
}

file static class Extensions
{
    public static (dfeta_AgeRange, dfeta_AgeRange)? ConvertToAgeRange(this SetRouteToProfessionalStatusCommandTrainingAgeSpecialism input)
    {
        if (!input.TryConvertToAgeRange(out var result))
        {
            throw new FormatException($"Unknown {typeof(TrainingAgeSpecialismType).Name}: '{input.Type}'.");
        }

        return result;
    }

    public static bool TryConvertToAgeRange(this SetRouteToProfessionalStatusCommandTrainingAgeSpecialism input, out (dfeta_AgeRange, dfeta_AgeRange)? result)
    {
        var mapped = input.Type switch
        {
            TrainingAgeSpecialismType.Range => (AgeRange.ConvertFromValue(input.From!.Value), AgeRange.ConvertFromValue(input.To!.Value)),
            TrainingAgeSpecialismType.FoundationStage => (dfeta_AgeRange.FoundationStage, dfeta_AgeRange.FoundationStage),
            TrainingAgeSpecialismType.FurtherEducation => (dfeta_AgeRange.FurtherEducation, dfeta_AgeRange.FurtherEducation),
            TrainingAgeSpecialismType.KeyStage1 => (dfeta_AgeRange.KeyStage1, dfeta_AgeRange.KeyStage1),
            TrainingAgeSpecialismType.KeyStage2 => (dfeta_AgeRange.KeyStage2, dfeta_AgeRange.KeyStage2),
            TrainingAgeSpecialismType.KeyStage3 => (dfeta_AgeRange.KeyStage3, dfeta_AgeRange.KeyStage3),
            TrainingAgeSpecialismType.KeyStage4 => (dfeta_AgeRange.KeyStage4, dfeta_AgeRange.KeyStage4),
            _ => ((dfeta_AgeRange, dfeta_AgeRange)?)null
        };

        if (mapped is null)
        {
            result = default;
            return false;
        }

        result = mapped.Value;
        return true;
    }
}
