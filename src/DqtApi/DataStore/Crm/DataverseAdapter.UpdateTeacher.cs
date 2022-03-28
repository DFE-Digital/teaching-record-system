using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DqtApi.DataStore.Crm.Models;
using FluentValidation;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Xrm.Sdk;
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

            var (itt, ittLookupFailed) = helper.SelectIttRecord(referenceData.Itt, referenceData.IttProviderId.Value);
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

            var findExistingTeacherResult = await helper.FindExistingTeacherToUpdate();

            if (ittLookupFailed == UpdateTeacherFailedReasons.MultipleInTrainingIttRecords)
            {
                // unable to determine which itt record to update - create review task.
                var reviewTask = helper.CreateMultipleMatchIttReviewTask(findExistingTeacherResult);
                txnRequest.Requests.Add(new CreateRequest()
                {
                    Target = reviewTask
                });
            }
            else if (ittLookupFailed == UpdateTeacherFailedReasons.NoMatchingIttRecord)
            {
                // Create itt record & review task
                txnRequest.Requests.Add(new UpsertRequest()
                {
                    Target = helper.CreateInitialTeacherTrainingEntity(referenceData, itt?.Id)
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
                txnRequest.Requests.Add(new UpsertRequest()
                {
                    Target = helper.CreateInitialTeacherTrainingEntity(referenceData, itt?.Id)
                });
            }

            if (command.Qualification != null)
            {
                var (qualification, qualificationLookupFailed) = helper.SelectQualificationRecord(referenceData.Qualifications);

                // Unable to determine what qualification to update - so create a crm review task
                if (qualificationLookupFailed == UpdateTeacherFailedReasons.MultipleQualificationRecords)
                {
                    var reviewTask = helper.CreateReviewTaskEntityForMultipleQualifications(command);

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

            if (findExistingTeacherResult.HasActiveSanctions)
            {
                var reviewTask = helper.CreateReviewTaskEntityForActiveSanctions();

                txnRequest.Requests.Add(new CreateRequest()
                {
                    Target = reviewTask
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
            }

            public Guid TeacherId { get; }

            public (dfeta_initialteachertraining Result, UpdateTeacherFailedReasons? FailedReason) SelectIttRecord(
                IEnumerable<dfeta_initialteachertraining> ittRecords,
                Guid ittProviderId)
            {
                // Find an ITT record for the specified ITT Provider.
                // The record should be at the InTraining status unless the programme is 'assessment only',
                // in which case the status should be UnderAssessment.

                var inTrainingForProvider = ittRecords
                    .Where(r => r.dfeta_Result == dfeta_ITTResult.InTraining ||
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

            public CrmTask CreateReviewTaskEntityForActiveSanctions()
            {
                var description = GetDescription();

                return new CrmTask()
                {
                    RegardingObjectId = new EntityReference(Contact.EntityLogicalName, TeacherId),
                    Category = "Notification for QTS unit - Register: matched record holds active sanction",
                    Subject = "Register: active sanction match",
                    Description = description,
                    ScheduledEnd = _dataverseAdapter._clock.UtcNow
                };

                string GetDescription()
                {
                    var sb = new StringBuilder();
                    sb.Append($"Active sanction found: TRN {TeacherId}");
                    return sb.ToString();
                }
            }

            public CrmTask CreateReviewTaskEntityForMultipleQualifications(UpdateTeacherCommand command)
            {
                var description = GetDescription();

                return new CrmTask()
                {
                    RegardingObjectId = new EntityReference(Contact.EntityLogicalName, TeacherId),
                    Category = "Notification for QTS unit - Register: matched record holds multiple qualifications",
                    Subject = "Register: multiple qualifications",
                    Description = description,
                    ScheduledEnd = _dataverseAdapter._clock.UtcNow
                };

                string GetDescription()
                {
                    var sb = new StringBuilder();
                    sb.Append($"Incoming Subject: {command.Qualification?.Subject},");
                    sb.Append($"Incoming Date: {command.Qualification?.Date},");
                    sb.Append($"Incoming Class {command.Qualification?.Class},");
                    sb.Append($"Incoming ProviderUkprn {command.Qualification?.ProviderUkprn},");
                    sb.Append($"Incoming CountryCode: {command.Qualification?.CountryCode}");
                    return sb.ToString();
                }
            }

            public CrmTask CreateNoMatchIttReviewTask()
            {
                var description = GetDescription();

                return new CrmTask()
                {
                    RegardingObjectId = new EntityReference(Contact.EntityLogicalName, TeacherId),
                    Category = "Notification for QTS unit - Register: matched record holds no ITT UKPRN",
                    Subject = "Register: missing ITT UKPRN",
                    Description = description,
                    ScheduledEnd = _dataverseAdapter._clock.UtcNow
                };

                string GetDescription()
                {
                    var sb = new StringBuilder();
                    sb.Append($"No ITT UKPRN match for TeacherId {TeacherId}");
                    return sb.ToString();
                }
            }

            public CrmTask CreateMultipleMatchIttReviewTask(UpdateTeacherFindResult teacher)
            {
                var description = GetDescription();

                return new CrmTask()
                {
                    RegardingObjectId = new EntityReference(Contact.EntityLogicalName, TeacherId),
                    Category = "Notification for QTS unit - Register: matched record holds multiple ITT UKPRNs",
                    Subject = "Register: multiple ITT UKPRNs",
                    Description = description,
                    ScheduledEnd = _dataverseAdapter._clock.UtcNow
                };

                string GetDescription()
                {
                    var sb = new StringBuilder();
                    sb.Append($"Multiple ITT UKPRNs found for TeacherId {TeacherId}");
                    return sb.ToString();
                }
            }

            public dfeta_initialteachertraining CreateInitialTeacherTrainingEntity(UpdateTeacherReferenceLookupResult referenceData, Guid? id)
            {
                Debug.Assert(referenceData.IttCountryId.HasValue);
                Debug.Assert(referenceData.IttProviderId.HasValue);
                Debug.Assert(referenceData.IttSubject1Id.HasValue);
                Debug.Assert(referenceData.IttSubject2Id.HasValue);

                var cohortYear = _command.InitialTeacherTraining.ProgrammeEndDate?.Year.ToString();
                var result = _command.InitialTeacherTraining.ProgrammeType == dfeta_ITTProgrammeType.AssessmentOnlyRoute ? dfeta_ITTResult.UnderAssessment : dfeta_ITTResult.InTraining;
                var entity =  new dfeta_initialteachertraining()
                {
                    Id = id ?? Guid.NewGuid(),
                    dfeta_PersonId = new EntityReference(Contact.EntityLogicalName, TeacherId),
                    dfeta_CountryId = new EntityReference(dfeta_country.EntityLogicalName, referenceData.IttCountryId.Value),
                    dfeta_EstablishmentId = new EntityReference(Account.EntityLogicalName, referenceData.IttProviderId.Value),
                    dfeta_ProgrammeStartDate = _command.InitialTeacherTraining.ProgrammeStartDate?.ToDateTime(),
                    dfeta_ProgrammeEndDate = _command.InitialTeacherTraining.ProgrammeEndDate?.ToDateTime(),
                    dfeta_ProgrammeType = _command.InitialTeacherTraining.ProgrammeType,
                    dfeta_CohortYear = cohortYear,
                    dfeta_Subject1Id = referenceData.IttSubject1Id.HasValue ? new EntityReference(dfeta_ittsubject.EntityLogicalName, referenceData.IttSubject1Id.Value) : null,
                    dfeta_Subject2Id = referenceData.IttSubject2Id.HasValue ? new EntityReference(dfeta_ittsubject.EntityLogicalName, referenceData.IttSubject2Id.Value) : null,
                    dfeta_Subject3Id = referenceData.IttSubject3Id.HasValue ? new EntityReference(dfeta_ittsubject.EntityLogicalName, referenceData.IttSubject3Id.Value) : null,
                    dfeta_AgeRangeFrom = _command.InitialTeacherTraining.AgeRangeFrom,
                    dfeta_AgeRangeTo = _command.InitialTeacherTraining.AgeRangeTo,
                    dfeta_Result = !id.HasValue ? result : null
                };

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
                    dfeta_HE_CountryId = new EntityReference(dfeta_country.EntityLogicalName, referenceData.QualificationCountryId.Value),
                    dfeta_HE_HESubject1Id = new EntityReference(dfeta_hesubject.EntityLogicalName, referenceData.QualificationSubjectId.Value),
                    dfeta_HE_ClassDivision = _command.Qualification.Class,
                    dfeta_HE_CompletionDate = _command.Qualification.Date?.ToDateTime(),
                    dfeta_HE_HEQualificationId = new EntityReference(dfeta_hequalification.EntityLogicalName, referenceData.QualificationId.Value),
                    dfeta_Type = dfeta_qualification_dfeta_Type.HigherEducation,
                    dfeta_PersonId = new EntityReference(Contact.EntityLogicalName, TeacherId),
                    dfeta_HE_EstablishmentId = referenceData.QualificationProviderId.HasValue ? new EntityReference(Account.EntityLogicalName, referenceData.QualificationProviderId.Value) : null,
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

                if (referenceData.QualificationId == null)
                {
                    failedReasons |= UpdateTeacherFailedReasons.QualificationNotFound;
                }

                if (referenceData.QualificationCountryId == null && _command.Qualification != null)
                {
                    failedReasons |= UpdateTeacherFailedReasons.QualificationCountryNotFound;
                }

                if (referenceData.QualificationSubjectId == null && _command.Qualification != null)
                {
                    failedReasons |= UpdateTeacherFailedReasons.QualificationSubjectNotFound;
                }

                if (referenceData.QualificationProviderId == null && !string.IsNullOrEmpty(_command.Qualification?.ProviderUkprn))
                {
                    failedReasons |= UpdateTeacherFailedReasons.QualificationProviderNotFound;
                }

                return failedReasons;
            }

            public async Task<UpdateTeacherFindResult> FindExistingTeacherToUpdate()
            {
                var match = await _dataverseAdapter.GetTeacher(
                    TeacherId,
                    columnNames: new[]
                    {
                        Contact.Fields.dfeta_ActiveSanctions,
                        Contact.Fields.dfeta_QTSDate,
                        Contact.Fields.dfeta_EYTSDate,
                        Contact.Fields.BirthDate
                    });

                return new UpdateTeacherFindResult()
                {
                    TeacherId = match.Id,
                    HasActiveSanctions = match.dfeta_ActiveSanctions == true,
                    HasQtsDate = match.dfeta_QTSDate.HasValue,
                    HasEytsDate = match.dfeta_EYTSDate.HasValue,
                    DateOfBirth = match.BirthDate.HasValue ? DateOnly.FromDateTime(match.BirthDate.Value) : null
                };
            }

            public async Task<UpdateTeacherReferenceLookupResult> LookupReferenceData()
            {
                Debug.Assert(!string.IsNullOrEmpty(_command.InitialTeacherTraining.ProviderUkprn));
                Debug.Assert(!string.IsNullOrEmpty(_command.InitialTeacherTraining.Subject1));
                Debug.Assert(!string.IsNullOrEmpty(_command.InitialTeacherTraining.Subject2));

                var getTeacherTask = _dataverseAdapter.GetTeacher(
                    TeacherId,
                    columnNames: new[]
                    {
                        Contact.Fields.dfeta_QTSDate,
                        Contact.Fields.dfeta_EYTSDate,
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

                static TResult Let<T, TResult>(T value, Func<T, TResult> getResult) => getResult(value);

                var getIttProviderTask = Let(
                    _command.InitialTeacherTraining.ProviderUkprn,
                    ukprn => _dataverseAdapter._cache.GetOrCreateAsync(
                        CacheKeys.GetIttProviderOrganizationByUkprnKey(ukprn),
                        _ => _dataverseAdapter.GetIttProviderOrganizationByUkprn(ukprn)));

                var getQualificationProviderTask = !string.IsNullOrEmpty(_command.Qualification?.ProviderUkprn) ?
                     Let(
                         _command.Qualification.ProviderUkprn,
                         ukprn => _dataverseAdapter._cache.GetOrCreateAsync(
                             CacheKeys.GetOrganizationByUkprnKey(ukprn),
                             _ => _dataverseAdapter.GetOrganizationByUkprn(ukprn))) :
                     null;

                var getIttCountryTask = Let(
                    "XK",  // XK == 'United Kingdom'
                    country => _dataverseAdapter._cache.GetOrCreateAsync(
                        CacheKeys.GetCountryKey(country),
                        _ => _dataverseAdapter.GetCountry(country)));

                var getSubject1Task = Let(
                    _command.InitialTeacherTraining.Subject1,
                    subject => _dataverseAdapter._cache.GetOrCreateAsync(
                        CacheKeys.GetIttSubjectKey(subject),
                        _ => _dataverseAdapter.GetIttSubjectByCode(subject)));

                var getSubject2Task = Let(
                    _command.InitialTeacherTraining.Subject2,
                    subject => _dataverseAdapter._cache.GetOrCreateAsync(
                        CacheKeys.GetIttSubjectKey(subject),
                        _ => _dataverseAdapter.GetIttSubjectByCode(subject)));

                var getSubject3Task = Let(
                    _command.InitialTeacherTraining.Subject3,
                    subject => _dataverseAdapter._cache.GetOrCreateAsync(
                        CacheKeys.GetIttSubjectKey(subject),
                        _ => _dataverseAdapter.GetIttSubjectByCode(subject)));

                var getQualificationTask = Let(
                    "First Degree",
                    qualificationName => _dataverseAdapter._cache.GetOrCreateAsync(
                        CacheKeys.GetHeQualificationKey(qualificationName),
                        _ => _dataverseAdapter.GetHeQualificationByName(qualificationName)));

                var getQualificationCountryTask = _command.Qualification != null ?
                    Let(
                        _command.Qualification.CountryCode,
                        country => _dataverseAdapter._cache.GetOrCreateAsync(
                            CacheKeys.GetCountryKey(country),
                            _ => _dataverseAdapter.GetCountry(country))) :
                    null;

                var getQualificationSubjectTask = _command.Qualification != null ?
                    Let(
                        _command.Qualification.Subject,
                        subjectName => _dataverseAdapter._cache.GetOrCreateAsync(
                            CacheKeys.GetHeSubjectKey(subjectName),
                            _ => _dataverseAdapter.GetHeSubjectByCode(subjectName))) :
                    null;

                var getEarlyYearsStatusTask = isEarlyYears ?
                    Let(
                        "220", // 220 == 'Early Years Trainee'
                        earlyYearsStatusId => _dataverseAdapter._cache.GetOrCreateAsync(
                            CacheKeys.GetEarlyYearsStatusKey(earlyYearsStatusId),
                            _ => _dataverseAdapter.GetEarlyYearsStatus(earlyYearsStatusId))) :
                    Task.FromResult<dfeta_earlyyearsstatus>(null);

                var getTeacherStatusTask = !isEarlyYears ?
                    Let(
                        _command.InitialTeacherTraining.ProgrammeType == dfeta_ITTProgrammeType.AssessmentOnlyRoute ?
                            "212" :  // 212 == 'AOR Candidate'
                            "211",   // 211 == 'Trainee Teacher:DMS'
                        teacherStatusId => _dataverseAdapter._cache.GetOrCreateAsync(
                            CacheKeys.GetTeacherStatusKey(teacherStatusId),
                            _ => _dataverseAdapter.GetTeacherStatus(teacherStatusId, qtsDateRequired: false))) :
                    Task.FromResult<dfeta_teacherstatus>(null);


                var lookupTasks = new Task[]
                {
                   getIttProviderTask,
                   getIttCountryTask,
                   getSubject1Task,
                   getSubject2Task,
                   getSubject3Task,
                   getQualificationTask,
                   getQualificationCountryTask,
                   getQualificationSubjectTask,
                   getEarlyYearsStatusTask,
                   getTeacherStatusTask,
                   getQualificationProviderTask,
                   getTeacherTask,
                   getIttRecordsTask,
                   getQtsRegistrationsTask,
                   getQualifications
                }
                .Where(t => t != null);

                await Task.WhenAll(lookupTasks);

                Debug.Assert(!isEarlyYears || getEarlyYearsStatusTask.Result != null, "Early years status lookup failed.");
                Debug.Assert(isEarlyYears || getTeacherStatusTask.Result != null, "Teacher status lookup failed.");

                return new()
                {
                    IttProviderId = getIttProviderTask.Result?.Id,
                    IttCountryId = getIttCountryTask.Result?.Id,
                    IttSubject1Id = getSubject1Task.Result?.Id,
                    IttSubject2Id = getSubject2Task.Result?.Id,
                    IttSubject3Id = getSubject3Task.Result?.Id,
                    QualificationId = getQualificationTask?.Result?.Id,
                    QualificationCountryId = getQualificationCountryTask?.Result?.Id,
                    QualificationSubjectId = getQualificationSubjectTask?.Result?.Id,
                    EarlyYearsStatusId = getEarlyYearsStatusTask.Result?.Id,
                    TeacherStatusId = getTeacherStatusTask.Result?.Id,
                    QualificationProviderId = getQualificationProviderTask?.Result?.Id,
                    Teacher = getTeacherTask.Result,
                    Itt = getIttRecordsTask.Result,
                    QtsRegistrations = getQtsRegistrationsTask.Result,
                    Qualifications = getQualifications.Result
                };
            }
        }

        internal class UpdateTeacherFindResult
        {
            public Guid TeacherId { get; set; }
            public string[] MatchedAttributes { get; set; }
            public bool HasActiveSanctions { get; set; }
            public bool HasQtsDate { get; set; }
            public bool HasEytsDate { get; set; }
            public DateOnly? DateOfBirth { get; set; }
        }

        internal class UpdateTeacherReferenceLookupResult
        {
            public Guid? IttProviderId { get; set; }
            public Guid? IttCountryId { get; set; }
            public Guid? IttSubject1Id { get; set; }
            public Guid? IttSubject2Id { get; set; }
            public Guid? IttSubject3Id { get; set; }
            public Guid? QualificationId { get; set; }
            public Guid? QualificationProviderId { get; set; }
            public Guid? QualificationCountryId { get; set; }
            public Guid? QualificationSubjectId { get; set; }
            public Guid? TeacherStatusId { get; set; }
            public Guid? EarlyYearsStatusId { get; set; }
            public Contact Teacher { get; set; }
            public IEnumerable<dfeta_initialteachertraining> Itt { get; set; }
            public IEnumerable<dfeta_qtsregistration> QtsRegistrations { get; set; }
            public Guid EarlyYearsTraineeStatusId { get; set; }
            public Guid AorCandidateTeacherStatusId { get; set; }
            public Guid TraineeTeacherDmsTeacherStatusId { get; set; }
            public IEnumerable<dfeta_qualification> Qualifications { get; set; }
            public Account QualificationProvider { get; set; }
        }
    }
}
