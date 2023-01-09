using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DqtApi.DataStore.Crm.Models;
using FluentValidation;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Xrm.Sdk.Messages;

namespace DqtApi.DataStore.Crm
{
    public partial class DataverseAdapter
    {
        public async Task<UpdateTeacherResult> UpdateTeacher(UpdateTeacherCommand command)
        {
            var (result, _) = await UpdateTeacherImpl(command);
            return result;
        }

        // Helper method that outputs the write requests that were sent for testing
        internal async Task<(UpdateTeacherResult Result, ExecuteTransactionRequest TransactionRequest)> UpdateTeacherImpl(
            UpdateTeacherCommand command)
        {
            var helper = new UpdateTeacherHelper(this, command);

            var referenceData = await helper.LookupReferenceData();

            var failedReasons = helper.ValidateReferenceData(referenceData);
            if (failedReasons != UpdateTeacherFailedReasons.None)
            {
                return (UpdateTeacherResult.Failed(failedReasons), null);
            }

            var (itt, ittLookupFailedReasons) = helper.SelectIttRecord(referenceData.Itt, referenceData.IttProviderId.Value);
            var isEarlyYears = command.InitialTeacherTraining.ProgrammeType.IsEarlyYears();

            if (isEarlyYears && referenceData.Teacher.dfeta_EYTSDate.HasValue)
            {
                return (UpdateTeacherResult.Failed(UpdateTeacherFailedReasons.AlreadyHaveEytsDate), null);
            }
            else if (!isEarlyYears && referenceData.Teacher.dfeta_QTSDate.HasValue)
            {
                return (UpdateTeacherResult.Failed(UpdateTeacherFailedReasons.AlreadyHaveQtsDate), null);
            }

            if (itt != null && itt.dfeta_ProgrammeType.Value.IsEarlyYears() != command.InitialTeacherTraining.ProgrammeType.IsEarlyYears())
            {
                return (UpdateTeacherResult.Failed(UpdateTeacherFailedReasons.CannotChangeProgrammeType), null);
            }

            // Send a single Transaction request with all the data changes in.
            // This is important for atomicity; we really do not want torn writes here.
            var txnRequest = new ExecuteTransactionRequest()
            {
                ReturnResponses = true,
                Requests = new()
            };

            if (ittLookupFailedReasons == UpdateTeacherFailedReasons.MultipleInTrainingIttRecords)
            {
                // unable to determine which itt record to update - create review task.
                var reviewTask = helper.CreateMultipleMatchIttReviewTask();
                txnRequest.Requests.Add(new CreateRequest()
                {
                    Target = reviewTask
                });
            }
            else if (ittLookupFailedReasons == UpdateTeacherFailedReasons.NoMatchingIttRecord)
            {
                // Create itt record & review task
                txnRequest.Requests.Add(new CreateRequest()
                {
                    Target = helper.CreateInitialTeacherTrainingEntity(referenceData, id: null)
                });

                var reviewTask = helper.CreateNoMatchIttReviewTask();
                txnRequest.Requests.Add(new CreateRequest()
                {
                    Target = reviewTask
                });
            }
            else
            {
                // update existing itt record
                Debug.Assert(itt != null);
                txnRequest.Requests.Add(new UpdateRequest()
                {
                    Target = helper.CreateInitialTeacherTrainingEntity(referenceData, itt.Id)
                });
            }

            if (command.Qualification != null)
            {
                var (qualification, qualificationLookupFailed) = helper.SelectQualificationRecord(referenceData.Qualifications);

                // Unable to determine what qualification to update - so create a crm review task
                if (qualificationLookupFailed == UpdateTeacherFailedReasons.MultipleQualificationRecords)
                {
                    var reviewTask = helper.CreateReviewTaskEntityForMultipleQualifications();

                    txnRequest.Requests.Add(new CreateRequest()
                    {
                        Target = reviewTask
                    });
                }
                else
                {
                    txnRequest.Requests.Add(
                        new UpsertRequest() { Target = helper.CreateQualificationEntity(referenceData, qualification?.Id) });
                }
            }

            if (referenceData.TeacherHasActiveSanctions)
            {
                var reviewTask = helper.CreateReviewTaskEntityForActiveSanctions();

                txnRequest.Requests.Add(new CreateRequest()
                {
                    Target = reviewTask
                });
            }

            if (referenceData.TeacherHusId != command.HusId)
            {
                var contact = helper.CreateContactEntity();

                txnRequest.Requests.Add(new UpdateRequest()
                {
                    Target = contact
                });
            }

            await _service.ExecuteAsync(txnRequest);

            return (UpdateTeacherResult.Success(helper.TeacherId, null), txnRequest);
        }

        internal class UpdateTeacherHelper
        {
            private readonly DataverseAdapter _dataverseAdapter;
            private readonly UpdateTeacherCommand _command;

            public UpdateTeacherHelper(DataverseAdapter dataverseAdapter, UpdateTeacherCommand command)
            {
                _dataverseAdapter = dataverseAdapter;
                _command = command;
                TeacherId = command.TeacherId;
                Trn = command.Trn;
            }

            public Guid TeacherId { get; }

            public string Trn { get; }

            public (dfeta_initialteachertraining Result, UpdateTeacherFailedReasons? FailedReason) SelectIttRecord(
                IEnumerable<dfeta_initialteachertraining> ittRecords,
                Guid ittProviderId)
            {
                // Find an ITT record for the specified ITT Provider.
                // The record should be at the InTraining status unless the programme is 'assessment only',
                // in which case the status should be UnderAssessment.

                var inTrainingForProvider = ittRecords
                    .Where(r => (r.dfeta_ProgrammeType != dfeta_ITTProgrammeType.AssessmentOnlyRoute && r.dfeta_Result == dfeta_ITTResult.InTraining) ||
                        (r.dfeta_ProgrammeType == dfeta_ITTProgrammeType.AssessmentOnlyRoute && r.dfeta_Result == dfeta_ITTResult.UnderAssessment))
                    .Where(r => r.StateCode == dfeta_initialteachertrainingState.Active && r.dfeta_EstablishmentId.Id == ittProviderId)
                    .ToArray();

                if (inTrainingForProvider.Length == 1)
                {
                    return (inTrainingForProvider[0], null);
                }
                else if (inTrainingForProvider.Length > 1)
                {
                    return (null, UpdateTeacherFailedReasons.MultipleInTrainingIttRecords);
                }
                else
                {
                    return (null, UpdateTeacherFailedReasons.NoMatchingIttRecord);
                }
            }

            public (dfeta_qualification Result, UpdateTeacherFailedReasons? FailedReason) SelectQualificationRecord(
                IEnumerable<dfeta_qualification> qualificationRecords)
            {
                var qualificationForProvider = qualificationRecords
                    .ToArray();

                if (qualificationForProvider.Length == 1)
                {
                    return (qualificationForProvider[0], null);
                }
                else if (qualificationForProvider.Length > 1)
                {
                    return (null, UpdateTeacherFailedReasons.MultipleQualificationRecords);
                }
                else
                {
                    return (null, null);
                }
            }

            public Contact CreateContactEntity()
            {
                return new Contact()
                {
                    Id = TeacherId,
                    dfeta_HUSID = _command.HusId
                };
            }

            public CrmTask CreateReviewTaskEntityForActiveSanctions()
            {
                var description = GetDescription();

                return new CrmTask()
                {
                    RegardingObjectId = TeacherId.ToEntityReference(Contact.EntityLogicalName),
                    Category = "Notification for QTS unit - Register: matched record holds active sanction",
                    Subject = "Register: active sanction match",
                    Description = description,
                    ScheduledEnd = _dataverseAdapter._clock.UtcNow
                };

                string GetDescription()
                {
                    var sb = new StringBuilder();
                    sb.Append($"Active sanction found: TRN {Trn}");
                    return sb.ToString();
                }
            }

            public CrmTask CreateReviewTaskEntityForMultipleQualifications()
            {
                var description = GetDescription();

                return new CrmTask()
                {
                    RegardingObjectId = TeacherId.ToEntityReference(Contact.EntityLogicalName),
                    Category = "Notification for QTS unit - Register: matched record holds multiple qualifications",
                    Subject = "Register: multiple qualifications",
                    Description = description,
                    ScheduledEnd = _dataverseAdapter._clock.UtcNow
                };

                string GetDescription()
                {
                    var sb = new StringBuilder();
                    sb.Append($"Incoming Subject: {_command.Qualification?.Subject},");
                    sb.Append($"Incoming Date: {_command.Qualification?.Date},");
                    sb.Append($"Incoming Class {_command.Qualification?.Class},");
                    sb.Append($"Incoming ProviderUkprn {_command.Qualification?.ProviderUkprn},");
                    sb.Append($"Incoming CountryCode: {_command.Qualification?.CountryCode}");
                    return sb.ToString();
                }
            }

            public CrmTask CreateNoMatchIttReviewTask()
            {
                var description = GetDescription();

                return new CrmTask()
                {
                    RegardingObjectId = TeacherId.ToEntityReference(Contact.EntityLogicalName),
                    Category = "Notification for QTS unit - Register: matched record holds no ITT UKPRN",
                    Subject = "Register: missing ITT UKPRN",
                    Description = description,
                    ScheduledEnd = _dataverseAdapter._clock.UtcNow
                };

                string GetDescription()
                {
                    var sb = new StringBuilder();
                    sb.Append($"No ITT UKPRN match for TRN {Trn}");
                    return sb.ToString();
                }
            }

            public CrmTask CreateMultipleMatchIttReviewTask()
            {
                var description = GetDescription();

                return new CrmTask()
                {
                    RegardingObjectId = TeacherId.ToEntityReference(Contact.EntityLogicalName),
                    Category = "Notification for QTS unit - Register: matched record holds multiple ITT UKPRNs",
                    Subject = "Register: multiple ITT UKPRNs",
                    Description = description,
                    ScheduledEnd = _dataverseAdapter._clock.UtcNow
                };

                string GetDescription()
                {
                    var sb = new StringBuilder();
                    sb.Append($"Multiple ITT UKPRNs found for TRN {Trn}");
                    return sb.ToString();
                }
            }

            public dfeta_initialteachertraining CreateInitialTeacherTrainingEntity(UpdateTeacherReferenceLookupResult referenceData, Guid? id)
            {
                Debug.Assert(referenceData.IttProviderId.HasValue);
                Debug.Assert(referenceData.IttSubject1Id.HasValue);

                var cohortYear = _command.InitialTeacherTraining.ProgrammeEndDate?.Year.ToString();
                var result = _command.InitialTeacherTraining.ProgrammeType == dfeta_ITTProgrammeType.AssessmentOnlyRoute ? dfeta_ITTResult.UnderAssessment : dfeta_ITTResult.InTraining;

                var entity = new dfeta_initialteachertraining()
                {
                    Id = id ?? Guid.NewGuid(),
                    dfeta_PersonId = TeacherId.ToEntityReference(Contact.EntityLogicalName),
                    dfeta_EstablishmentId = referenceData.IttProviderId.Value.ToEntityReference(Account.EntityLogicalName),
                    dfeta_ProgrammeStartDate = _command.InitialTeacherTraining.ProgrammeStartDate?.ToDateTime(),
                    dfeta_ProgrammeEndDate = _command.InitialTeacherTraining.ProgrammeEndDate?.ToDateTime(),
                    dfeta_ProgrammeType = _command.InitialTeacherTraining.ProgrammeType,
                    dfeta_CohortYear = cohortYear,
                    dfeta_Subject1Id = referenceData.IttSubject1Id?.ToEntityReference(dfeta_ittsubject.EntityLogicalName),
                    dfeta_Subject2Id = referenceData.IttSubject2Id?.ToEntityReference(dfeta_ittsubject.EntityLogicalName),
                    dfeta_Subject3Id = referenceData.IttSubject3Id?.ToEntityReference(dfeta_ittsubject.EntityLogicalName),
                    dfeta_AgeRangeFrom = _command.InitialTeacherTraining.AgeRangeFrom,
                    dfeta_AgeRangeTo = _command.InitialTeacherTraining.AgeRangeTo,
                    dfeta_Result = !id.HasValue ? result : null,
                    dfeta_TraineeID = _command.HusId,
                    dfeta_ITTQualificationId = referenceData.IttQualificationId?.ToEntityReference(dfeta_ittqualification.EntityLogicalName),
                    dfeta_ittqualificationaim = _command.InitialTeacherTraining.IttQualificationAim
                };

                if (referenceData.IttCountryId is Guid countryId)
                {
                    entity.dfeta_CountryId = countryId.ToEntityReference(dfeta_country.EntityLogicalName);
                }

                if (id.HasValue)
                {
                    entity.Attributes.Remove(dfeta_initialteachertraining.Fields.dfeta_Result);
                }

                return entity;
            }

            public dfeta_qualification CreateQualificationEntity(UpdateTeacherReferenceLookupResult referenceData, Guid? id)
            {
                Debug.Assert(referenceData.QualificationId.HasValue);
                Debug.Assert(referenceData.QualificationCountryId.HasValue);
                Debug.Assert(referenceData.QualificationSubjectId.HasValue);

                var entity = new dfeta_qualification()
                {
                    Id = id ?? Guid.NewGuid(),
                    dfeta_HE_CountryId = referenceData.QualificationCountryId.Value.ToEntityReference(dfeta_country.EntityLogicalName),
                    dfeta_HE_ClassDivision = _command.Qualification.Class,
                    dfeta_HE_CompletionDate = _command.Qualification.Date?.ToDateTime(),
                    dfeta_HE_HEQualificationId = referenceData.QualificationId.Value.ToEntityReference(dfeta_hequalification.EntityLogicalName),
                    dfeta_Type = dfeta_qualification_dfeta_Type.HigherEducation,
                    dfeta_PersonId = TeacherId.ToEntityReference(Contact.EntityLogicalName),
                    dfeta_HE_EstablishmentId = referenceData.QualificationProviderId?.ToEntityReference(Account.EntityLogicalName),
                    dfeta_HE_HESubject1Id = referenceData.QualificationSubjectId?.ToEntityReference(dfeta_hesubject.EntityLogicalName),
                    dfeta_HE_HESubject2Id = referenceData.QualificationSubject2Id?.ToEntityReference(dfeta_hesubject.EntityLogicalName),
                    dfeta_HE_HESubject3Id = referenceData.QualificationSubject3Id?.ToEntityReference(dfeta_hesubject.EntityLogicalName),
                };

                if (id.HasValue)
                {
                    entity.Attributes.Remove(dfeta_qualification.Fields.dfeta_PersonId);
                    entity.Attributes.Remove(dfeta_qualification.Fields.dfeta_Type);
                }
                return entity;
            }

            public UpdateTeacherFailedReasons ValidateReferenceData(UpdateTeacherReferenceLookupResult referenceData)
            {
                var failedReasons = UpdateTeacherFailedReasons.None;

                if (referenceData.IttProviderId == null)
                {
                    failedReasons |= UpdateTeacherFailedReasons.IttProviderNotFound;
                }

                if (referenceData.IttSubject1Id == null && !string.IsNullOrEmpty(_command.InitialTeacherTraining.Subject1))
                {
                    failedReasons |= UpdateTeacherFailedReasons.Subject1NotFound;
                }

                if (referenceData.IttSubject2Id == null && !string.IsNullOrEmpty(_command.InitialTeacherTraining.Subject2))
                {
                    failedReasons |= UpdateTeacherFailedReasons.Subject2NotFound;
                }

                if (referenceData.IttSubject3Id == null && !string.IsNullOrEmpty(_command.InitialTeacherTraining.Subject3))
                {
                    failedReasons |= UpdateTeacherFailedReasons.Subject3NotFound;
                }

                if (referenceData.IttQualificationId == null && !string.IsNullOrEmpty(_command.InitialTeacherTraining.IttQualificationValue))
                {
                    failedReasons |= UpdateTeacherFailedReasons.IttQualificationNotFound;
                }

                if (referenceData.IttCountryId == null && !string.IsNullOrEmpty(_command.InitialTeacherTraining.TrainingCountryCode))
                {
                    failedReasons |= UpdateTeacherFailedReasons.TrainingCountryNotFound;
                }

                if (referenceData.QualificationId == null)
                {
                    failedReasons |= UpdateTeacherFailedReasons.QualificationNotFound;
                }

                if (referenceData.QualificationCountryId == null && _command.Qualification != null)
                {
                    failedReasons |= UpdateTeacherFailedReasons.QualificationCountryNotFound;
                }

                if (referenceData.QualificationSubjectId == null && !string.IsNullOrEmpty(_command.Qualification?.Subject))
                {
                    failedReasons |= UpdateTeacherFailedReasons.QualificationSubjectNotFound;
                }

                if (referenceData.QualificationSubject2Id == null && !string.IsNullOrEmpty(_command.Qualification?.Subject2))
                {
                    failedReasons |= UpdateTeacherFailedReasons.QualificationSubject2NotFound;
                }

                if (referenceData.QualificationSubject3Id == null && !string.IsNullOrEmpty(_command.Qualification?.Subject3))
                {
                    failedReasons |= UpdateTeacherFailedReasons.QualificationSubject3NotFound;
                }

                if (referenceData.QualificationProviderId == null && !string.IsNullOrEmpty(_command.Qualification?.ProviderUkprn))
                {
                    failedReasons |= UpdateTeacherFailedReasons.QualificationProviderNotFound;
                }

                if (referenceData.HaveExistingTeacherWithHusId == true)
                {
                    failedReasons |= UpdateTeacherFailedReasons.DuplicateHusId;
                }

                return failedReasons;
            }

            public async Task<UpdateTeacherReferenceLookupResult> LookupReferenceData()
            {
                Debug.Assert(!string.IsNullOrEmpty(_command.InitialTeacherTraining.ProviderUkprn));
                Debug.Assert(!string.IsNullOrEmpty(_command.InitialTeacherTraining.Subject1));

                var getTeacherTask = _dataverseAdapter.GetTeacher(
                    TeacherId,
                    columnNames: new[]
                    {
                        Contact.Fields.dfeta_QTSDate,
                        Contact.Fields.dfeta_EYTSDate,
                        Contact.Fields.dfeta_ActiveSanctions,
                        Contact.Fields.dfeta_HUSID,
                        Contact.Fields.StateCode
                    });

                var getIttRecordsTask = _dataverseAdapter.GetInitialTeacherTrainingByTeacher(
                    TeacherId,
                    columnNames: new[]
                    {
                        dfeta_initialteachertraining.Fields.dfeta_ProgrammeType,
                        dfeta_initialteachertraining.Fields.dfeta_Result,
                        dfeta_initialteachertraining.Fields.dfeta_EstablishmentId,
                        dfeta_initialteachertraining.Fields.StateCode,
                    });

                var getQtsRegistrationsTask = _dataverseAdapter.GetQtsRegistrationsByTeacher(
                    TeacherId,
                    columnNames: new[]
                    {
                        dfeta_qtsregistration.Fields.dfeta_EarlyYearsStatusId,
                        dfeta_qtsregistration.Fields.dfeta_TeacherStatusId,
                        dfeta_qtsregistration.Fields.StateCode
                    });

                var getQualifications = _dataverseAdapter.GetQualificationsForTeacher(TeacherId,
                    columnNames: new[]
                    {
                        dfeta_qualification.Fields.dfeta_CompletionorAwardDate,
                        dfeta_qualification.Fields.dfeta_Type,
                        dfeta_qualification.Fields.dfeta_HE_EstablishmentId,
                        dfeta_qualification.Fields.dfeta_PersonId
                    });

                var isEarlyYears = _command.InitialTeacherTraining.ProgrammeType.IsEarlyYears();

                var requestBuilder = _dataverseAdapter.CreateMultipleRequestBuilder();

                static TResult Let<T, TResult>(T value, Func<T, TResult> getResult) => getResult(value);

                var getIttProviderTask = Let(
                    _command.InitialTeacherTraining.ProviderUkprn,
                    ukprn => _dataverseAdapter._cache.GetOrCreateUnlessNullAsync(
                        CacheKeys.GetIttProviderOrganizationByUkprnKey(ukprn),
                        () => _dataverseAdapter.GetIttProviderOrganizationsByUkprn(ukprn, true, columnNames: Array.Empty<string>(), requestBuilder)
                            .ContinueWith(t => t.Result.SingleOrDefault())));

                var getIttCountryTask = !string.IsNullOrEmpty(_command.InitialTeacherTraining.TrainingCountryCode) ?
                    Let(
                        _command.InitialTeacherTraining.TrainingCountryCode,
                        country => _dataverseAdapter._cache.GetOrCreateUnlessNullAsync(
                            CacheKeys.GetCountryKey(country),
                            () => _dataverseAdapter.GetCountry(country, requestBuilder))) :
                    null;

                var getSubject1Task = !string.IsNullOrEmpty(_command.InitialTeacherTraining.Subject1) ?
                    Let(
                        _command.InitialTeacherTraining.Subject1,
                        subject => _dataverseAdapter._cache.GetOrCreateUnlessNullAsync(
                            CacheKeys.GetIttSubjectKey(subject),
                            () => _dataverseAdapter.GetIttSubjectByCode(subject, requestBuilder))) :
                    null;

                var getSubject2Task = !string.IsNullOrEmpty(_command.InitialTeacherTraining.Subject2) ?
                    Let(
                        _command.InitialTeacherTraining.Subject2,
                        subject => _dataverseAdapter._cache.GetOrCreateUnlessNullAsync(
                            CacheKeys.GetIttSubjectKey(subject),
                            () => _dataverseAdapter.GetIttSubjectByCode(subject, requestBuilder))) :
                    null;

                var getSubject3Task = !string.IsNullOrEmpty(_command.InitialTeacherTraining.Subject3) ?
                    Let(
                        _command.InitialTeacherTraining.Subject3,
                        subject => _dataverseAdapter._cache.GetOrCreateUnlessNullAsync(
                            CacheKeys.GetIttSubjectKey(subject),
                            () => _dataverseAdapter.GetIttSubjectByCode(subject, requestBuilder))) :
                    null;

                var getIttQualificationTask = !string.IsNullOrEmpty(_command.InitialTeacherTraining.IttQualificationValue) ?
                    Let(
                        _command.InitialTeacherTraining.IttQualificationValue,
                        ittQualificationCode => _dataverseAdapter._cache.GetOrCreateUnlessNullAsync(
                            CacheKeys.GetIttQualificationKey(ittQualificationCode),
                            () => _dataverseAdapter.GetIttQualificationByCode(ittQualificationCode, requestBuilder))) :
                    null;

                var getQualificationTask = Let(
                    !string.IsNullOrEmpty(_command.Qualification?.HeQualificationValue) ? _command.Qualification.HeQualificationValue : "400",   // 400 = First Degree
                    qualificationCode => _dataverseAdapter._cache.GetOrCreateUnlessNullAsync(
                        CacheKeys.GetHeQualificationKey(qualificationCode),
                        () => _dataverseAdapter.GetHeQualificationByCode(qualificationCode, requestBuilder)));

                var getQualificationProviderTask = !string.IsNullOrEmpty(_command.Qualification?.ProviderUkprn) ?
                    Let(
                        _command.Qualification.ProviderUkprn,
                        ukprn => _dataverseAdapter._cache.GetOrCreateUnlessNullAsync(
                            CacheKeys.GetOrganizationByUkprnKey(ukprn),
                            () => _dataverseAdapter.GetOrganizationsByUkprn(ukprn, columnNames: Array.Empty<string>(), requestBuilder)
                                .ContinueWith(t => t.Result.SingleOrDefault()))) :
                    null;

                var getQualificationCountryTask = !string.IsNullOrEmpty(_command.Qualification?.CountryCode) ?
                    Let(
                        _command.Qualification.CountryCode,
                        country => _dataverseAdapter._cache.GetOrCreateUnlessNullAsync(
                            CacheKeys.GetCountryKey(country),
                            () => _dataverseAdapter.GetCountry(country, requestBuilder))) :
                    null;

                var getQualificationSubjectTask = !string.IsNullOrEmpty(_command.Qualification?.Subject) ?
                    Let(
                        _command.Qualification.Subject,
                        subjectName => _dataverseAdapter._cache.GetOrCreateUnlessNullAsync(
                            CacheKeys.GetHeSubjectKey(subjectName),
                            () => _dataverseAdapter.GetHeSubjectByCode(subjectName, requestBuilder))) :
                    null;

                var getQualificationSubject2Task = !string.IsNullOrEmpty(_command.Qualification?.Subject2) ?
                    Let(
                        _command.Qualification.Subject2,
                        subjectName => _dataverseAdapter._cache.GetOrCreateUnlessNullAsync(
                            CacheKeys.GetHeSubjectKey(subjectName),
                            () => _dataverseAdapter.GetHeSubjectByCode(subjectName, requestBuilder))) :
                    null;

                var getQualificationSubject3Task = !string.IsNullOrEmpty(_command.Qualification?.Subject3) ?
                    Let(
                        _command.Qualification.Subject3,
                        subjectName => _dataverseAdapter._cache.GetOrCreateUnlessNullAsync(
                            CacheKeys.GetHeSubjectKey(subjectName),
                            () => _dataverseAdapter.GetHeSubjectByCode(subjectName, requestBuilder))) :
                    null;

                var getEarlyYearsStatusTask = isEarlyYears ?
                    Let(
                        "220", // 220 == 'Early Years Trainee'
                        earlyYearsStatusId => _dataverseAdapter._cache.GetOrCreateUnlessNullAsync(
                            CacheKeys.GetEarlyYearsStatusKey(earlyYearsStatusId),
                            () => _dataverseAdapter.GetEarlyYearsStatus(earlyYearsStatusId, requestBuilder))) :
                    Task.FromResult<dfeta_earlyyearsstatus>(null);

                var getTeacherStatusTask = !isEarlyYears ?
                    Let(
                        _command.InitialTeacherTraining.ProgrammeType == dfeta_ITTProgrammeType.AssessmentOnlyRoute ?
                            "212" :  // 212 == 'AOR Candidate'
                            "211",   // 211 == 'Trainee Teacher'
                        teacherStatusId => _dataverseAdapter._cache.GetOrCreateUnlessNullAsync(
                            CacheKeys.GetTeacherStatusKey(teacherStatusId),
                            () => _dataverseAdapter.GetTeacherStatus(teacherStatusId, qtsDateRequired: false, requestBuilder))) :
                    Task.FromResult<dfeta_teacherstatus>(null);

                var existingTeachersWithHusIdTask = !string.IsNullOrEmpty(_command.HusId) ? _dataverseAdapter.GetTeachersByHusId(_command.HusId, columnNames: new[]
                {
                    Contact.Fields.dfeta_TRN,
                    Contact.Fields.dfeta_HUSID,
                }) : Task.FromResult<Contact[]>(null);

                var lookupTasks = new Task[]
                {
                   getIttProviderTask,
                   getIttCountryTask,
                   getSubject1Task,
                   getSubject2Task,
                   getSubject3Task,
                   getIttQualificationTask,
                   getQualificationTask,
                   getQualificationCountryTask,
                   getQualificationSubjectTask,
                   getEarlyYearsStatusTask,
                   getTeacherStatusTask,
                   getQualificationProviderTask,
                   getTeacherTask,
                   getIttRecordsTask,
                   getQtsRegistrationsTask,
                   getQualifications,
                   getQualificationSubject2Task,
                   getQualificationSubject3Task,
                   existingTeachersWithHusIdTask
                }
                .Where(t => t != null);

                await requestBuilder.Execute();
                await Task.WhenAll(lookupTasks);

                Debug.Assert(!isEarlyYears || getEarlyYearsStatusTask.Result != null, "Early years status lookup failed.");
                Debug.Assert(isEarlyYears || getTeacherStatusTask.Result != null, "Teacher status lookup failed.");

                return new()
                {
                    IttProviderId = getIttProviderTask?.Result?.Id,
                    IttCountryId = getIttCountryTask?.Result?.Id,
                    IttSubject1Id = getSubject1Task?.Result?.Id,
                    IttSubject2Id = getSubject2Task?.Result?.Id,
                    IttSubject3Id = getSubject3Task?.Result?.Id,
                    IttQualificationId = getIttQualificationTask?.Result?.Id,
                    QualificationId = getQualificationTask?.Result?.Id,
                    QualificationCountryId = getQualificationCountryTask?.Result?.Id,
                    QualificationSubjectId = getQualificationSubjectTask?.Result?.Id,
                    QualificationSubject2Id = getQualificationSubject2Task?.Result?.Id,
                    QualificationSubject3Id = getQualificationSubject3Task?.Result?.Id,
                    EarlyYearsStatusId = getEarlyYearsStatusTask.Result?.Id,
                    TeacherStatusId = getTeacherStatusTask.Result?.Id,
                    QualificationProviderId = getQualificationProviderTask?.Result?.Id,
                    Teacher = getTeacherTask.Result,
                    Itt = getIttRecordsTask.Result,
                    QtsRegistrations = getQtsRegistrationsTask.Result,
                    Qualifications = getQualifications.Result,
                    TeacherHasActiveSanctions = getTeacherTask.Result?.dfeta_ActiveSanctions == true,
                    TeacherHusId = getTeacherTask.Result?.dfeta_HUSID,
                    HaveExistingTeacherWithHusId = existingTeachersWithHusIdTask?.Result != null && existingTeachersWithHusIdTask.Result.Count(x => x.Id != _command.TeacherId) > 0
                };
            }
        }

        internal class UpdateTeacherReferenceLookupResult
        {
            public Guid? IttProviderId { get; set; }
            public Guid? IttCountryId { get; set; }
            public Guid? IttSubject1Id { get; set; }
            public Guid? IttSubject2Id { get; set; }
            public Guid? IttSubject3Id { get; set; }
            public Guid? IttQualificationId { get; set; }
            public Guid? QualificationId { get; set; }
            public Guid? QualificationProviderId { get; set; }
            public Guid? QualificationCountryId { get; set; }
            public Guid? QualificationSubjectId { get; set; }
            public Guid? TeacherStatusId { get; set; }
            public Guid? EarlyYearsStatusId { get; set; }
            public Contact Teacher { get; set; }
            public IEnumerable<dfeta_initialteachertraining> Itt { get; set; }
            public IEnumerable<dfeta_qtsregistration> QtsRegistrations { get; set; }
            public IEnumerable<dfeta_qualification> Qualifications { get; set; }
            public bool TeacherHasActiveSanctions { get; set; }
            public string TeacherHusId { get; set; }
            public Guid? QualificationSubject2Id { get; set; }
            public Guid? QualificationSubject3Id { get; set; }
            public bool HaveExistingTeacherWithHusId { get; set; }
        }
    }
}
