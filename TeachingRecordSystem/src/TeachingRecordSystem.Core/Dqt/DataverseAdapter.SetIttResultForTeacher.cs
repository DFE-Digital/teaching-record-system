#nullable disable
using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Xrm.Sdk.Messages;

namespace TeachingRecordSystem.Core.Dqt;

public partial class DataverseAdapter
{
    public async Task<SetIttResultForTeacherResult> SetIttResultForTeacher(
        Guid teacherId,
        string ittProviderUkprn,
        dfeta_ITTResult result,
        DateOnly? assessmentDate,
        string slugId = null)
    {
        var (r, _) = await SetIttResultForTeacherImpl(teacherId, ittProviderUkprn, result, assessmentDate, slugId);
        return r;
    }

    // Helper method that outputs the write requests that were sent for testing
    internal async Task<(SetIttResultForTeacherResult Result, ExecuteTransactionRequest TransactionRequest)> SetIttResultForTeacherImpl(
        Guid teacherId,
        string ittProviderUkprn,
        dfeta_ITTResult result,
        DateOnly? assessmentDate,
        string slugId = null)
    {
        switch (result)
        {
            case dfeta_ITTResult.Pass:
            case dfeta_ITTResult.Fail:
            case dfeta_ITTResult.Withdrawn:
            case dfeta_ITTResult.Deferred:
            case dfeta_ITTResult.DeferredforSkillsTests:
                break;
            default:
                throw new ArgumentException($"Invalid ITT outcome: '{result}'.", nameof(result));
        }

        if (result == dfeta_ITTResult.Pass && !assessmentDate.HasValue)
        {
            throw new ArgumentNullException(nameof(assessmentDate));
        }
        else if (result != dfeta_ITTResult.Pass && assessmentDate.HasValue)
        {
            throw new ArgumentException(
                $"Cannot specify {nameof(assessmentDate)} unless result is {dfeta_ITTResult.Pass}.",
                nameof(assessmentDate));
        }

        var helper = new SetIttResultForTeacherHelper(this, teacherId, ittProviderUkprn);

        var lookupData = await helper.LookupData();

        if (lookupData.Teacher is null)
        {
            throw new ArgumentException("Teacher does not exist.", nameof(teacherId));
        }

        if (lookupData.Teacher.StateCode != ContactState.Active)
        {
            throw new ArgumentException("Teacher is not active.", nameof(teacherId));
        }

        if (lookupData.IttProvider is null)
        {
            return (SetIttResultForTeacherResult.Failed(SetIttResultForTeacherFailedReason.NoMatchingIttRecord), null);
        }

        var (itt, ittLookupFailed) = helper.SelectIttRecord(lookupData.Itt, lookupData.IttProvider.Id, slugId);

        if (ittLookupFailed.HasValue)
        {
            return (SetIttResultForTeacherResult.Failed(ittLookupFailed.Value), null);
        }

        if (itt.dfeta_Result == dfeta_ITTResult.Fail)
        {
            return result == dfeta_ITTResult.Fail ?
                (SetIttResultForTeacherResult.Success(null), null) :
                (SetIttResultForTeacherResult.Failed(SetIttResultForTeacherFailedReason.NoMatchingIttRecord), null);
        }

        if (itt.dfeta_Result == dfeta_ITTResult.Withdrawn)
        {
            return result == dfeta_ITTResult.Withdrawn ?
                (SetIttResultForTeacherResult.Success(null), null) :
                (SetIttResultForTeacherResult.Failed(SetIttResultForTeacherFailedReason.NoMatchingIttRecord), null);
        }

        bool isEarlyYears = itt.dfeta_ProgrammeType.Value.IsEarlyYears();

        var (qtsRegistration, qtsLookupFailed) = helper.SelectQtsRegistrationRecord(
            lookupData.QtsRegistrations,
            itt.dfeta_ProgrammeType.Value,
            isEarlyYears,
            lookupData.EarlyYearsTraineeStatusId,
            lookupData.AorCandidateTeacherStatusId,
            lookupData.TraineeTeacherDmsTeacherStatusId,
            lookupData.EarlyYearsTeacherStatusId,
            lookupData.QualifiedTeacherTrainedStatusId,
            lookupData.QualifiedTeacherAssessmentOnlyRouteId,
            lookupData.QualifiedTeacherInternationalTeacherStatusId);

        if (qtsLookupFailed.HasValue)
        {
            return (SetIttResultForTeacherResult.Failed(qtsLookupFailed.Value), null);
        }

        if (assessmentDate.HasValue)
        {
            if (isEarlyYears && lookupData.Teacher.dfeta_EYTSDate.HasValue &&
                lookupData.Teacher.dfeta_EYTSDate.Value.ToDateOnlyWithDqtBstFix(isLocalTime: true) != assessmentDate.Value)
            {
                return (SetIttResultForTeacherResult.Failed(SetIttResultForTeacherFailedReason.EytsDateMismatch), null);
            }
            else if (!isEarlyYears && qtsRegistration.dfeta_QTSDate.HasValue &&
                qtsRegistration.dfeta_QTSDate.Value.ToDateOnlyWithDqtBstFix(isLocalTime: true) != assessmentDate.Value)
            {
                return (SetIttResultForTeacherResult.Failed(SetIttResultForTeacherFailedReason.QtsDateMismatch), null);
            }
        }

        var qtsUpdate = new dfeta_qtsregistration()
        {
            Id = qtsRegistration.Id
        };

        var txnRequest = new ExecuteTransactionRequest()
        {
            ReturnResponses = true,
            Requests = new()
            {
                new UpdateRequest()
                {
                    Target = new dfeta_initialteachertraining()
                    {
                        Id = itt.Id,
                        dfeta_Result = result
                    }
                },
                new UpdateRequest()
                {
                    Target = qtsUpdate
                }
            }
        };

        var qtsDate = assessmentDate;

        if (result == dfeta_ITTResult.Pass)
        {
            if (isEarlyYears)
            {
                var earlyYearsStatus = await GetEarlyYearsStatus("221");  // 221 == 'Early Years Teacher Status'
                Debug.Assert(earlyYearsStatus != null);

                qtsUpdate.dfeta_EarlyYearsStatusId = earlyYearsStatus.Id.ToEntityReference(dfeta_earlyyearsstatus.EntityLogicalName);
                qtsUpdate.dfeta_EYTSDate = qtsDate.Value.ToDateTime();
            }
            else
            {
                var teacherStatus = await GetTeacherStatus(itt.dfeta_ProgrammeType switch
                {
                    dfeta_ITTProgrammeType.Internationalqualifiedteacherstatus => "90",  // Qualified teacher: by virtue of achieving international qualified teacher status
                    dfeta_ITTProgrammeType.AssessmentOnlyRoute => "100",  // 'Qualified Teacher: Assessment Only Route'
                    _ => "71"  // 'Qualified teacher (trained)'
                });
                Debug.Assert(teacherStatus != null);

                if (!qtsRegistration.dfeta_QTSDate.HasValue)
                {
                    txnRequest.Requests.Add(new CreateRequest()
                    {
                        Target = new dfeta_induction()
                        {
                            dfeta_PersonId = teacherId.ToEntityReference(Contact.EntityLogicalName),
                            dfeta_InductionStatus = dfeta_InductionStatus.RequiredtoComplete
                        }
                    });
                }

                qtsUpdate.dfeta_TeacherStatusId = teacherStatus.Id.ToEntityReference(dfeta_teacherstatus.EntityLogicalName);
                qtsUpdate.dfeta_QTSDate = qtsDate.Value.ToDateTime();
            }
        }
        else
        {
            if (result == dfeta_ITTResult.Withdrawn)
            {
                if (isEarlyYears)
                {
                    qtsUpdate.Attributes[dfeta_qtsregistration.Fields.dfeta_EarlyYearsStatusId] = null;
                }
                else
                {
                    qtsUpdate.Attributes[dfeta_qtsregistration.Fields.dfeta_TeacherStatusId] = null;
                }
            }
        }

        if (lookupData.Teacher.dfeta_ActiveSanctions == true)
        {
            txnRequest.Requests.Add(new CreateRequest()
            {
                Target = helper.CreateReviewTaskEntityForActiveSanctions(lookupData.Teacher.dfeta_TRN)
            });
        }

        await _service.ExecuteAsync(txnRequest);

        return (SetIttResultForTeacherResult.Success(qtsDate), txnRequest);

        Task<dfeta_earlyyearsstatus> GetEarlyYearsStatus(string value)
        {
            var cacheKey = CacheKeys.GetEarlyYearsStatusKey(value);

            return _cache.GetOrCreateAsync(
                cacheKey,
                _ => this.GetEarlyYearsStatus(value, null));
        }

        Task<dfeta_teacherstatus> GetTeacherStatus(string value)
        {
            var cacheKey = CacheKeys.GetTeacherStatusKey(value);

            return _cache.GetOrCreateAsync(
                cacheKey,
                _ => this.GetTeacherStatus(value, null));
        }
    }

    internal class SetIttResultForTeacherHelper
    {
        private readonly DataverseAdapter _dataverseAdapter;
        private readonly Guid _teacherId;
        private readonly string _ittProviderUkprn;

        public SetIttResultForTeacherHelper(DataverseAdapter dataverseAdapter, Guid teacherId, string ittProviderUkprn)
        {
            _dataverseAdapter = dataverseAdapter;
            _teacherId = teacherId;
            _ittProviderUkprn = ittProviderUkprn;
        }

        public Models.Task CreateReviewTaskEntityForActiveSanctions(string trn)
        {
            var description = GetDescription();

            return new Models.Task()
            {
                RegardingObjectId = _teacherId.ToEntityReference(Contact.EntityLogicalName),
                Category = "Notification for QTS unit - Register: matched record holds active sanction",
                Subject = "Register: active sanction match",
                Description = description,
                ScheduledEnd = _dataverseAdapter._clock.UtcNow
            };

            string GetDescription()
            {
                var sb = new StringBuilder();
                sb.Append($"Active sanction found: TRN {trn}");
                return sb.ToString();
            }
        }

        public async Task<SetIttResultForTeacherLookupResult> LookupData()
        {
            var requestBuilder = _dataverseAdapter.CreateMultipleRequestBuilder();
            var getAllEytsTeacherStatusesTask = _dataverseAdapter.GetAllEarlyYearsStatuses(requestBuilder);
            var getAllTeacherStatuses = _dataverseAdapter.GetAllTeacherStatuses(requestBuilder);

            var getTeacherTask = _dataverseAdapter.GetTeacher(
                _teacherId,
                columnNames: new[]
                {
                    Contact.Fields.dfeta_QTSDate,
                    Contact.Fields.dfeta_EYTSDate,
                    Contact.Fields.StateCode,
                    Contact.Fields.dfeta_ActiveSanctions,
                    Contact.Fields.dfeta_TRN
                });

            var getIttProviderTask = _dataverseAdapter.GetIttProviderOrganizationsByUkprn(_ittProviderUkprn, columnNames: Array.Empty<string>(), activeOnly: true)
                .ContinueWith(t => t.Result.SingleOrDefault());

            var getIttRecordsTask = _dataverseAdapter.GetInitialTeacherTrainingByTeacher(
                _teacherId,
                columnNames: new[]
                {
                    dfeta_initialteachertraining.Fields.dfeta_ProgrammeType,
                    dfeta_initialteachertraining.Fields.dfeta_Result,
                    dfeta_initialteachertraining.Fields.dfeta_EstablishmentId,
                    dfeta_initialteachertraining.Fields.StateCode,
                    dfeta_initialteachertraining.Fields.dfeta_SlugId
                });

            var getQtsRegistrationsTask = _dataverseAdapter.GetQtsRegistrationsByTeacher(
                _teacherId,
                columnNames: new[]
                {
                    dfeta_qtsregistration.Fields.dfeta_EarlyYearsStatusId,
                    dfeta_qtsregistration.Fields.dfeta_TeacherStatusId,
                    dfeta_qtsregistration.Fields.StateCode,
                    dfeta_qtsregistration.Fields.dfeta_QTSDate
                });

            await requestBuilder.Execute();
            await Task.WhenAll(
                getTeacherTask,
                getIttProviderTask,
                getIttRecordsTask,
                getQtsRegistrationsTask,
                getAllEytsTeacherStatusesTask,
                getAllTeacherStatuses
                );

            var getEarlyYearsTraineeStatusId = getAllEytsTeacherStatusesTask.Result.Single(x => x.dfeta_Value == "220");
            var getAorCandidateTeacherStatusId = getAllTeacherStatuses.Result.Single(x => x.dfeta_Value == "212");
            var getTraineeTeacherDmsTeacherStatusId = getAllTeacherStatuses.Result.Single(x => x.dfeta_Value == "211");
            var earlyYearsTeacherStatusId = getAllEytsTeacherStatusesTask.Result.Single(x => x.dfeta_Value == "221");
            var qualifiedTeacherTrainedStatusId = getAllTeacherStatuses.Result.Single(x => x.dfeta_Value == "71");
            var qualifiedTeacherAssessmentOnlyRouteId = getAllTeacherStatuses.Result.Single(x => x.dfeta_Value == "100");
            var qualifiedTeacherInternationalTeacherStatusId = getAllTeacherStatuses.Result.Single(x => x.dfeta_Value == "90");

            return new SetIttResultForTeacherLookupResult()
            {
                Teacher = getTeacherTask.Result,
                IttProvider = getIttProviderTask.Result,
                Itt = getIttRecordsTask.Result,
                QtsRegistrations = getQtsRegistrationsTask.Result,
                EarlyYearsTraineeStatusId = getEarlyYearsTraineeStatusId.Id,
                AorCandidateTeacherStatusId = getAorCandidateTeacherStatusId.Id,
                TraineeTeacherDmsTeacherStatusId = getTraineeTeacherDmsTeacherStatusId.Id,
                EarlyYearsTeacherStatusId = earlyYearsTeacherStatusId.Id,
                QualifiedTeacherTrainedStatusId = qualifiedTeacherTrainedStatusId.Id,
                QualifiedTeacherAssessmentOnlyRouteId = qualifiedTeacherAssessmentOnlyRouteId.Id,
                QualifiedTeacherInternationalTeacherStatusId = qualifiedTeacherInternationalTeacherStatusId.Id
            };
        }

        public (dfeta_initialteachertraining Result, SetIttResultForTeacherFailedReason? FailedReason) SelectIttRecord(
            IEnumerable<dfeta_initialteachertraining> ittRecords,
            Guid ittProviderId,
            string slugId = null)
        {
            // if SlugId is passed in, use slugid for matching
            // otherwise fallback to matching on establishmet
            // if ProgrammeType is AssessmentOnlyRoute, result should be UnderAssessment,Withdrawn or Deferrred otherwise
            // record should be InTraining,WithDrawn or Deferred
            List<dfeta_initialteachertraining> matching = new List<dfeta_initialteachertraining>();
            var activeForProvider = Array.Empty<dfeta_initialteachertraining>();

            if (!string.IsNullOrEmpty(slugId))
            {
                activeForProvider = ittRecords
                    .Where(r => r.dfeta_SlugId == slugId && r.dfeta_EstablishmentId?.Id == ittProviderId && r.StateCode == dfeta_initialteachertrainingState.Active)
                    .ToArray();
            }

            if (activeForProvider.Length == 0)
            {
                activeForProvider = ittRecords
                    .Where(r => r.dfeta_EstablishmentId?.Id == ittProviderId && string.IsNullOrEmpty(r.dfeta_SlugId) && r.StateCode == dfeta_initialteachertrainingState.Active)
                    .ToArray();
            }

            foreach (var itt in activeForProvider)
            {
                if (itt.dfeta_ProgrammeType == dfeta_ITTProgrammeType.AssessmentOnlyRoute)
                {
                    switch (itt.dfeta_Result)
                    {
                        case dfeta_ITTResult.UnderAssessment:
                        case dfeta_ITTResult.Withdrawn:
                        case dfeta_ITTResult.Deferred:
                        case dfeta_ITTResult.Pass:
                        case dfeta_ITTResult.Fail:
                            {
                                matching.Add(itt);
                                break;
                            }
                    }
                }
                else
                {
                    if (itt.dfeta_ProgrammeType.HasValue)
                    {
                        switch (itt.dfeta_Result)
                        {
                            case dfeta_ITTResult.InTraining:
                            case dfeta_ITTResult.Withdrawn:
                            case dfeta_ITTResult.Deferred:
                            case dfeta_ITTResult.Pass:
                            case dfeta_ITTResult.Fail:
                                {
                                    matching.Add(itt);
                                    break;
                                }
                        }
                    }
                }
            }

            if (matching.Count == 0)
            {
                return (null, SetIttResultForTeacherFailedReason.NoMatchingIttRecord);
            }
            else if (matching.Count > 1)
            {
                return (null, SetIttResultForTeacherFailedReason.MultipleIttRecords);
            }
            else
            {
                return (matching[0], null);
            }
        }

        public (dfeta_qtsregistration, SetIttResultForTeacherFailedReason? FailedReason) SelectQtsRegistrationRecord(
            IEnumerable<dfeta_qtsregistration> qtsRecords,
            dfeta_ITTProgrammeType programmeType,
            bool isEarlyYears,
            Guid earlyYearsTraineeStatusId,
            Guid aorCandidateTeacherStatusId,
            Guid traineeTeacherDmsTeacherStatusId,
            Guid earlyYearsTeacherStatusId,
            Guid qualifiedTeacherTrainedStatusId,
            Guid qualifiedTeacherAssessmentOnlyRouteId,
            Guid qualifiedTeacherInternationalTeacherStatusId)
        {
            // Find an active QTS registration entity where either
            //   programme type is Early Years and the Early Years Status is 220 ('Early Years Trainee') *OR*
            //   programme type is AssessmentOnly and the Teacher Status is 212 ('AOR Candidate') *OR*
            //   programme type is neither Early Years nor Assessment Only and Teacher Status is 211 ('Trainee Teacher:DMS') *OR*
            //   has EYTS/QTS Status

            var matching = new List<dfeta_qtsregistration>();

            foreach (var qts in qtsRecords)
            {
                // has eyts or qts status
                if (qts.dfeta_EarlyYearsStatusId?.Id == earlyYearsTeacherStatusId ||
                    qts.dfeta_TeacherStatusId?.Id == qualifiedTeacherTrainedStatusId ||
                    qts.dfeta_TeacherStatusId?.Id == qualifiedTeacherAssessmentOnlyRouteId ||
                    qts.dfeta_TeacherStatusId?.Id == qualifiedTeacherInternationalTeacherStatusId)
                {
                    matching.Add(qts);
                }

                else if (isEarlyYears)
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

            if (matching.Count == 0)
            {
                return (null, SetIttResultForTeacherFailedReason.NoMatchingQtsRecord);
            }
            else if (matching.Count > 1)
            {
                return (null, SetIttResultForTeacherFailedReason.MultipleQtsRecords);
            }
            else
            {
                return (matching[0], null);
            }
        }
    }

    internal class SetIttResultForTeacherLookupResult
    {
        public Contact Teacher { get; set; }
        public Account IttProvider { get; set; }
        public IEnumerable<dfeta_initialteachertraining> Itt { get; set; }
        public IEnumerable<dfeta_qtsregistration> QtsRegistrations { get; set; }
        public Guid EarlyYearsTraineeStatusId { get; set; }
        public Guid AorCandidateTeacherStatusId { get; set; }
        public Guid TraineeTeacherDmsTeacherStatusId { get; set; }
        public Guid EarlyYearsTeacherStatusId { get; set; }
        public Guid QualifiedTeacherTrainedStatusId { get; set; }
        public Guid QualifiedTeacherAssessmentOnlyRouteId { get; set; }
        public Guid QualifiedTeacherInternationalTeacherStatusId { get; set; }
    }
}
