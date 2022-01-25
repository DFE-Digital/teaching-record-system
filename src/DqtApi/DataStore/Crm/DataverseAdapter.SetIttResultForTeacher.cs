using System;
using System.Diagnostics;
using System.Threading.Tasks;
using DqtApi.DataStore.Crm.Models;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;
using System.Linq;

namespace DqtApi.DataStore.Crm
{
    public partial class DataverseAdapter
    {
        public async Task<SetIttResultForTeacherResult> SetIttResultForTeacher(
            Guid teacherId,
            string ittProviderUkprn,
            dfeta_ITTResult result,
            DateOnly? assessmentDate)
        {
            var (r, _) = await SetIttResultForTeacherImpl(teacherId, ittProviderUkprn, result, assessmentDate);
            return r;
        }

        // Helper method that outputs the write requests that were sent for testing
        internal async Task<(SetIttResultForTeacherResult Result, ExecuteTransactionRequest TransactionRequest)> SetIttResultForTeacherImpl(
            Guid teacherId,
            string ittProviderUkprn,
            dfeta_ITTResult result,
            DateOnly? assessmentDate)
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

            var (itt, ittLookupFailed) = helper.SelectIttRecord(lookupData.Itt, lookupData.IttProvider.Id);

            if (ittLookupFailed.HasValue)
            {
                return (SetIttResultForTeacherResult.Failed(ittLookupFailed.Value), null);
            }

            bool isEarlyYears = itt.dfeta_ProgrammeType.Value.IsEarlyYears();

            var (qtsRegistration, qtsLookupFailed) = helper.SelectQtsRegistrationRecord(
                lookupData.QtsRegistrations,
                itt.dfeta_ProgrammeType.Value,
                isEarlyYears,
                lookupData.EarlyYearsTraineeStatusId,
                lookupData.AorCandidateTeacherStatusId,
                lookupData.TraineeTeacherDmsTeacherStatusId);

            if (qtsLookupFailed.HasValue)
            {
                return (SetIttResultForTeacherResult.Failed(qtsLookupFailed.Value), null);
            }

            if (isEarlyYears && lookupData.Teacher.dfeta_EYTSDate.HasValue)
            {
                return (SetIttResultForTeacherResult.Failed(SetIttResultForTeacherFailedReason.AlreadyHaveEytsDate), null);
            }
            else if (!isEarlyYears && lookupData.Teacher.dfeta_QTSDate.HasValue)
            {
                return (SetIttResultForTeacherResult.Failed(SetIttResultForTeacherFailedReason.AlreadyHaveQtsDate), null);
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

                    qtsUpdate.dfeta_EarlyYearsStatusId = new EntityReference(dfeta_earlyyearsstatus.EntityLogicalName, earlyYearsStatus.Id);
                    qtsUpdate.dfeta_EYTSDate = qtsDate.Value.ToDateTime(new());
                }
                else
                {
                    var teacherStatus = await GetTeacherStatus(
                        itt.dfeta_ProgrammeType == dfeta_ITTProgrammeType.AssessmentOnlyRoute ?
                            "100" :  // 100 == 'Qualified Teacher: Assessment Only Route'
                            "71");  // 71 == 'Qualified teacher (trained)'
                    Debug.Assert(teacherStatus != null);

                    qtsUpdate.dfeta_TeacherStatusId = new EntityReference(dfeta_teacherstatus.EntityLogicalName, teacherStatus.Id);
                    qtsUpdate.dfeta_QTSDate = qtsDate.Value.ToDateTime(new());

                    txnRequest.Requests.Add(new CreateRequest()
                    {
                        Target = new dfeta_induction()
                        {
                            dfeta_PersonId = new EntityReference(Contact.EntityLogicalName, teacherId),
                            dfeta_InductionStatus = dfeta_InductionStatus.RequiredtoComplete
                        }
                    });
                }
            }
            else
            {
                qtsUpdate.Attributes[dfeta_qtsregistration.Fields.dfeta_TeacherStatusId] = null;

                if ((result == dfeta_ITTResult.Fail || result == dfeta_ITTResult.Withdrawn) && isEarlyYears)
                {
                    qtsUpdate.Attributes[dfeta_qtsregistration.Fields.dfeta_EarlyYearsStatusId] = null;
                }
            }

            await _service.ExecuteAsync(txnRequest);

            return (SetIttResultForTeacherResult.Success(qtsDate), txnRequest);

            Task<dfeta_earlyyearsstatus> GetEarlyYearsStatus(string value)
            {
                var cacheKey = CacheKeys.GetEarlyYearsStatusKey(value);

                return _cache.GetOrCreateAsync(
                    cacheKey,
                    _ => this.GetEarlyYearsStatus(value));
            }

            Task<dfeta_teacherstatus> GetTeacherStatus(string value)
            {
                var cacheKey = CacheKeys.GetTeacherStatusKey(value);

                return _cache.GetOrCreateAsync(
                    cacheKey,
                    _ => this.GetTeacherStatus(value, qtsDateRequired: true));
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

            public async Task<SetIttResultForTeacherLookupResult> LookupData()
            {
                var getTeacherTask = _dataverseAdapter.GetTeacherAsync(
                    _teacherId,
                    columnNames: new[]
                    {
                        Contact.Fields.dfeta_QTSDate,
                        Contact.Fields.dfeta_EYTSDate,
                        Contact.Fields.StateCode
                    });

                var getIttProviderTask = _dataverseAdapter.GetOrganizationByUkprn(_ittProviderUkprn);

                var getIttRecordsTask = _dataverseAdapter.GetInitialTeacherTrainingByTeacher(
                    _teacherId,
                    columnNames: new[]
                    {
                        dfeta_initialteachertraining.Fields.dfeta_ProgrammeType,
                        dfeta_initialteachertraining.Fields.dfeta_Result,
                        dfeta_initialteachertraining.Fields.dfeta_EstablishmentId,
                        dfeta_initialteachertraining.Fields.StateCode,
                    });

                var getQtsRegistrationsTask = _dataverseAdapter.GetQtsRegistrationsByTeacher(
                    _teacherId,
                    columnNames: new[]
                    {
                        dfeta_qtsregistration.Fields.dfeta_EarlyYearsStatusId,
                        dfeta_qtsregistration.Fields.dfeta_TeacherStatusId,
                        dfeta_qtsregistration.Fields.StateCode
                    });

                var getEarlyYearsTraineeStatusIdTask = _dataverseAdapter._cache.GetOrCreateAsync(
                    CacheKeys.GetEarlyYearsStatusKey("220"),  // 220 == 'Early Years Trainee'
                    _ => _dataverseAdapter.GetEarlyYearsStatus("220"));

                var getAorCandidateTeacherStatusIdTask = _dataverseAdapter._cache.GetOrCreateAsync(
                    CacheKeys.GetTeacherStatusKey("212"),  // 212 == 'AOR Candidate'
                    _ => _dataverseAdapter.GetTeacherStatus("212", qtsDateRequired: false));

                var getTraineeTeacherDmsTeacherStatusIdTask = _dataverseAdapter._cache.GetOrCreateAsync(
                    CacheKeys.GetTeacherStatusKey("211"),  // 211 == 'Trainee Teacher:DMS'
                    _ => _dataverseAdapter.GetTeacherStatus("211", qtsDateRequired: false));

                await Task.WhenAll(
                    getTeacherTask,
                    getIttProviderTask,
                    getIttRecordsTask,
                    getQtsRegistrationsTask,
                    getAorCandidateTeacherStatusIdTask,
                    getTraineeTeacherDmsTeacherStatusIdTask);

                Debug.Assert(getEarlyYearsTraineeStatusIdTask.Result != null, "'Early Years Trainee' early years status lookup failed");
                Debug.Assert(getAorCandidateTeacherStatusIdTask.Result != null, "'AOR Candidate' teacher status lookup failed");
                Debug.Assert(getTraineeTeacherDmsTeacherStatusIdTask.Result != null, "'Trainee Teacher:DMS' teacher status lookup failed");

                return new SetIttResultForTeacherLookupResult()
                {
                    Teacher = getTeacherTask.Result,
                    IttProvider = getIttProviderTask.Result,
                    Itt = getIttRecordsTask.Result,
                    QtsRegistrations = getQtsRegistrationsTask.Result,
                    EarlyYearsTraineeStatusId = getEarlyYearsTraineeStatusIdTask.Result.Id,
                    AorCandidateTeacherStatusId = getAorCandidateTeacherStatusIdTask.Result.Id,
                    TraineeTeacherDmsTeacherStatusId = getTraineeTeacherDmsTeacherStatusIdTask.Result.Id
                };
            }

            public (dfeta_initialteachertraining Result, SetIttResultForTeacherFailedReason? FailedReason) SelectIttRecord(
                IEnumerable<dfeta_initialteachertraining> ittRecords,
                Guid ittProviderId)
            {
                // Find an ITT record for the specified ITT Provider.
                // The record should be at the InTraining status unless the programme is 'assessment only',
                // in which case the status should be UnderAssessment.

                var inTrainingForProvider = ittRecords
                    .Where(r => r.dfeta_Result == dfeta_ITTResult.InTraining ||
                        (r.dfeta_ProgrammeType == dfeta_ITTProgrammeType.AssessmentOnlyRoute && r.dfeta_Result == dfeta_ITTResult.UnderAssessment))
                    .Where(r => r.dfeta_EstablishmentId.Id == ittProviderId)
                    .Where(r => r.StateCode == dfeta_initialteachertrainingState.Active)
                    .ToArray();

                if (inTrainingForProvider.Length == 0)
                {
                    return (null, SetIttResultForTeacherFailedReason.NoMatchingIttRecord);
                }
                else if (inTrainingForProvider.Length > 1)
                {
                    return (null, SetIttResultForTeacherFailedReason.MultipleInTrainingIttRecords);
                }
                else
                {
                    return (inTrainingForProvider[0], null);
                }
            }

            public (dfeta_qtsregistration, SetIttResultForTeacherFailedReason? FailedReason) SelectQtsRegistrationRecord(
                IEnumerable<dfeta_qtsregistration> qtsRecords,
                dfeta_ITTProgrammeType programmeType,
                bool isEarlyYears,
                Guid earlyYearsTraineeStatusId,
                Guid aorCandidateTeacherStatusId,
                Guid traineeTeacherDmsTeacherStatusId)
            {
                // Find an active QTS registration entity where either
                //   programme type is Early Years and the Early Years Status is 220 ('Early Years Trainee') *OR*
                //   programme type is AssessmentOnly and the Teacher Status is 212 ('AOR Candidate') *OR*
                //   programme type is neither Early Years nor Assessment Only and Teacher Status is 211 ('Trainee Teacher:DMS')

                var matching = qtsRecords
                    .Where(r =>
                        (isEarlyYears && r.dfeta_EarlyYearsStatusId?.Id == earlyYearsTraineeStatusId) ||
                        (programmeType == dfeta_ITTProgrammeType.AssessmentOnlyRoute && r.dfeta_TeacherStatusId?.Id == aorCandidateTeacherStatusId) ||
                        (!isEarlyYears && programmeType != dfeta_ITTProgrammeType.AssessmentOnlyRoute && r.dfeta_TeacherStatusId?.Id == traineeTeacherDmsTeacherStatusId))
                    .Where(r => r.StateCode == dfeta_qtsregistrationState.Active)
                    .ToArray();

                if (matching.Length == 0)
                {
                    return (null, SetIttResultForTeacherFailedReason.NoMatchingQtsRecord);
                }
                else if (matching.Length > 1)
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
        }
    }
}
