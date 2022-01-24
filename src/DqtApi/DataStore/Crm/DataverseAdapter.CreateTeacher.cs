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
using Microsoft.Xrm.Sdk.Query;

namespace DqtApi.DataStore.Crm
{
    public partial class DataverseAdapter
    {
        internal delegate Task<CreateTeacherDuplicateTeacherResult> FindExistingTeacher();

        public async Task<CreateTeacherResult> CreateTeacher(CreateTeacherCommand command)
        {
            var (result, _) = await CreateTeacherImpl(command);
            return result;
        }

        // Helper method that outputs the write requests that were sent for testing
        internal async Task<(CreateTeacherResult Result, ExecuteTransactionRequest TransactionRequest)> CreateTeacherImpl(
            CreateTeacherCommand command,
            FindExistingTeacher findExistingTeacher = null)  // This is parameterised so we can swap out in tests
        {
            var helper = new CreateTeacherHelper(this, command);

            var referenceData = await helper.LookupReferenceData();

            var failedReasons = helper.ValidateReferenceData(referenceData);
            if (failedReasons != CreateTeacherFailedReasons.None)
            {
                return (CreateTeacherResult.Failed(failedReasons), null);
            }

            // Send a single Transaction request with all the data changes in.
            // This is important for atomicity; we really do not want torn writes here.
            var txnRequest = new ExecuteTransactionRequest()
            {
                ReturnResponses = true,
                Requests = new()
                {
                    new CreateRequest() { Target = helper.CreateContactEntity() },
                    new CreateRequest() { Target = helper.CreateInitialTeacherTrainingEntity(referenceData) },
                    new CreateRequest() { Target = helper.CreateQualificationEntity(referenceData) },
                    new CreateRequest() { Target = helper.CreateQtsRegistrationEntity(referenceData) }
                }
            };

            var findExistingTeacherResult = await (findExistingTeacher ?? helper.FindExistingTeacher)();
            var allocateTrn = findExistingTeacherResult == null;

            if (allocateTrn)
            {
                // Set the flag to allocate a TRN
                // N.B. setting this attribute has to be in an Update, setting it in the initial Create doesn't work
                txnRequest.Requests.Add(new UpdateRequest()
                {
                    Target = new Contact()
                    {
                        Id = helper.TeacherId,
                        dfeta_TRNAllocateRequest = _clock.UtcNow
                    }
                });

                // Retrieve the generated TRN
                txnRequest.Requests.Add(new RetrieveRequest()
                {
                    Target = new EntityReference(Contact.EntityLogicalName, helper.TeacherId),
                    ColumnSet = new ColumnSet(Contact.Fields.dfeta_TRN)
                });
            }
            else
            {
                // Create a Task to review the potential duplicate
                Debug.Assert(findExistingTeacherResult != null);

                txnRequest.Requests.Add(new CreateRequest()
                {
                    Target = helper.CreateDuplicateReviewTaskEntity(findExistingTeacherResult)
                });
            }

            var txnResponse = (ExecuteTransactionResponse)await _service.ExecuteAsync(txnRequest);

            // If a TRN was allocated the final response in the ExecuteTransactionResponse has it
            string trn = allocateTrn ?
                ((RetrieveResponse)txnResponse.Responses.Last()).Entity.ToEntity<Contact>().dfeta_TRN :
                null;

            return (CreateTeacherResult.Success(helper.TeacherId, trn), txnRequest);
        }

        internal class CreateTeacherHelper
        {
            private readonly DataverseAdapter _dataverseAdapter;
            private readonly CreateTeacherCommand _command;

            public CreateTeacherHelper(DataverseAdapter dataverseAdapter, CreateTeacherCommand command)
            {
                _dataverseAdapter = dataverseAdapter;
                _command = command;

                // Allocate the ID ourselves so we can reference the Contact entity in subsequent requests within the same ExecuteTransactionRequest
                // https://stackoverflow.com/a/34278011
                TeacherId = Guid.NewGuid();
            }

            public Guid TeacherId { get; }

            public CrmTask CreateDuplicateReviewTaskEntity(CreateTeacherDuplicateTeacherResult duplicate)
            {
                var description = GetDescription();

                return new CrmTask()
                {
                    RegardingObjectId = new EntityReference(Contact.EntityLogicalName, TeacherId),
                    dfeta_potentialduplicateid = new EntityReference(Contact.EntityLogicalName, duplicate.TeacherId),
                    Category = "DMSImportTrn",
                    Subject = "Notification for QTS Unit Team",
                    Description = description,
                    ScheduledEnd = _dataverseAdapter._clock.UtcNow
                };

                string GetDescription()
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("Potential duplicate");
                    sb.AppendLine("Matched on");

                    foreach (var matchedAttribute in duplicate.MatchedAttributes)
                    {
                        sb.AppendLine(matchedAttribute switch
                        {
                            Contact.Fields.FirstName => $"\t- First name: '{_command.FirstName}'",
                            Contact.Fields.MiddleName => $"\t- Middle name: '{_command.MiddleName}'",
                            Contact.Fields.LastName => $"\t- Last name: '{_command.LastName}'",
                            Contact.Fields.BirthDate => $"\t- Date of birth: '{_command.BirthDate:dd/MM/yyyy}'",
                            _ => throw new Exception($"Unknown matched field: '{matchedAttribute}'.")
                        });
                    }

                    Debug.Assert(!duplicate.HasEytsDate || !duplicate.HasQtsDate);

                    var additionalFlags = new List<string>();

                    if (duplicate.HasActiveSanctions)
                    {
                        additionalFlags.Add("active sanctions");
                    }

                    if (duplicate.HasQtsDate)
                    {
                        additionalFlags.Add("QTS date");
                    }
                    else if (duplicate.HasEytsDate)
                    {
                        additionalFlags.Add("EYTS date");
                    }

                    if (additionalFlags.Count > 0)
                    {
                        sb.AppendLine($"Matched record has {string.Join(" & ", additionalFlags)}");
                    }

                    return sb.ToString();
                }
            }

            public Contact CreateContactEntity()
            {
                // PO REVIEW: Fields we're no longer populating:
                // Title
                // Telephone1
                // NINumber
                // PreviousSurname
                // HUSID

                return new Contact()
                {
                    Id = TeacherId,
                    FirstName = _command.FirstName,
                    MiddleName = _command.MiddleName,
                    LastName = _command.LastName,
                    BirthDate = _command.BirthDate,
                    EMailAddress1 = _command.EmailAddress,
                    Address1_Line1 = _command.Address?.AddressLine1,
                    Address1_Line2 = _command.Address?.AddressLine2,
                    Address1_Line3 = _command.Address?.AddressLine3,
                    Address1_City = _command.Address?.City,
                    Address1_PostalCode = _command.Address?.PostalCode,
                    Address1_Country = _command.Address?.Country,
                    GenderCode = _command.GenderCode,
                };
            }

            public dfeta_initialteachertraining CreateInitialTeacherTrainingEntity(CreateTeacherReferenceLookupResult referenceData)
            {
                // PO REVIEW: Fields we're no longer populating:
                // HUSID - not in inputs
                // ITTQualificationId (looked up from QualAimCode historically)
                // AgerangeFrom
                // AgeRangeTo
                // TroopsToTeach

                Debug.Assert(referenceData.IttCountryId.HasValue);
                Debug.Assert(referenceData.IttProviderId.HasValue);
                Debug.Assert(referenceData.IttSubject1Id.HasValue);
                Debug.Assert(referenceData.IttSubject2Id.HasValue);

                var cohortYear = _command.InitialTeacherTraining.ProgrammeEndDate.Year.ToString();

                return new dfeta_initialteachertraining()
                {
                    dfeta_PersonId = new EntityReference(Contact.EntityLogicalName, TeacherId),
                    dfeta_CountryId = new EntityReference(dfeta_country.EntityLogicalName, referenceData.IttCountryId.Value),
                    dfeta_EstablishmentId = new EntityReference(Account.EntityLogicalName, referenceData.IttProviderId.Value),
                    dfeta_ProgrammeStartDate = _command.InitialTeacherTraining.ProgrammeStartDate,
                    dfeta_ProgrammeEndDate = _command.InitialTeacherTraining.ProgrammeEndDate,
                    dfeta_ProgrammeType = _command.InitialTeacherTraining.ProgrammeType,
                    dfeta_CohortYear = cohortYear,
                    dfeta_Subject1Id = new EntityReference(dfeta_ittsubject.EntityLogicalName, referenceData.IttSubject1Id.Value),
                    dfeta_Subject2Id = new EntityReference(dfeta_ittsubject.EntityLogicalName, referenceData.IttSubject2Id.Value),
                    dfeta_Result = _command.InitialTeacherTraining.Result
                };
            }

            public dfeta_qualification CreateQualificationEntity(CreateTeacherReferenceLookupResult referenceData)
            {
                Debug.Assert(referenceData.QualificationId.HasValue);
                Debug.Assert(referenceData.QualificationProviderId.HasValue);
                Debug.Assert(referenceData.QualificationCountryId.HasValue);
                Debug.Assert(referenceData.QualificationSubjectId.HasValue);

                return new dfeta_qualification()
                {
                    dfeta_PersonId = new EntityReference(Contact.EntityLogicalName, TeacherId),
                    dfeta_Type = dfeta_qualification_dfeta_Type.HigherEducation,
                    dfeta_HE_CountryId = new EntityReference(dfeta_country.EntityLogicalName, referenceData.QualificationCountryId.Value),
                    dfeta_HE_HESubject1Id = new EntityReference(dfeta_hesubject.EntityLogicalName, referenceData.QualificationSubjectId.Value),
                    dfeta_HE_ClassDivision = _command.Qualification.Class,
                    dfeta_HE_EstablishmentId = new EntityReference(Account.EntityLogicalName, referenceData.QualificationProviderId.Value),
                    dfeta_HE_CompletionDate = _command.Qualification.Date,
                    dfeta_HE_HEQualificationId = new EntityReference(dfeta_hequalification.EntityLogicalName, referenceData.QualificationId.Value)
                };
            }

            public dfeta_qtsregistration CreateQtsRegistrationEntity(CreateTeacherReferenceLookupResult referenceData)
            {
                return new dfeta_qtsregistration()
                {
                    dfeta_PersonId = new EntityReference(Contact.EntityLogicalName, TeacherId),
                    dfeta_EarlyYearsStatusId = referenceData.EarlyYearsStatusId.HasValue ?
                        new EntityReference(dfeta_earlyyearsstatus.EntityLogicalName, referenceData.EarlyYearsStatusId.Value) :
                        null,
                    dfeta_TeacherStatusId = referenceData.TeacherStatusId.HasValue ?
                        new EntityReference(dfeta_teacherstatus.EntityLogicalName, referenceData.TeacherStatusId.Value) :
                        null
                };
            }

            public async Task<CreateTeacherDuplicateTeacherResult> FindExistingTeacher()
            {
                var filter = new FilterExpression(LogicalOperator.And);
                filter.AddCondition(Contact.Fields.StateCode, ConditionOperator.Equal, (int)ContactState.Active);

                if (TryGetMatchCombinationsFilter(out var matchCombinationsFilter))
                {
                    filter.AddFilter(matchCombinationsFilter);
                }
                else
                {
                    // Not enough data in the input to match on
                    return null;
                }

                var query = new QueryExpression(Contact.EntityLogicalName)
                {
                    ColumnSet = new()
                    {
                        Columns =
                        {
                            Contact.Fields.dfeta_ActiveSanctions,
                            Contact.Fields.dfeta_QTSDate,
                            Contact.Fields.dfeta_EYTSDate
                        }
                    },
                    Criteria = filter
                };

                var result = await _dataverseAdapter._service.RetrieveMultipleAsync(query);

                // Old implementation returns the first record that matches on at least three attributes; replicating that here
                var match = result.Entities.Select(entity => entity.ToEntity<Contact>()).FirstOrDefault();

                if (match == null)
                {
                    return null;
                }

                var attributeMatches = new[]
                {
                    (
                        Attribute: Contact.Fields.FirstName,
                        Matches: _command.FirstName?.Equals(match.FirstName, StringComparison.OrdinalIgnoreCase) ?? false
                    ),
                    (
                        Attribute: Contact.Fields.MiddleName,
                        Matches: _command.MiddleName?.Equals(match.MiddleName, StringComparison.OrdinalIgnoreCase) ?? false
                    ),
                    (
                        Attribute: Contact.Fields.LastName,
                        Matches: _command.LastName?.Equals(match.LastName, StringComparison.OrdinalIgnoreCase) ?? false
                    ),
                    (
                        Attribute: Contact.Fields.BirthDate,
                        Matches: _command.BirthDate.Equals(match.BirthDate)
                    )
                };

                var matchedAttributeNames = attributeMatches.Where(m => m.Matches).Select(m => m.Attribute).ToArray();

                return new CreateTeacherDuplicateTeacherResult()
                {
                    TeacherId = match.Id,
                    MatchedAttributes = matchedAttributeNames,
                    HasActiveSanctions = match.dfeta_ActiveSanctions == true,
                    HasQtsDate = match.dfeta_QTSDate.HasValue,
                    HasEytsDate = match.dfeta_EYTSDate.HasValue
                };

                bool TryGetMatchCombinationsFilter(out FilterExpression filter)
                {
                    // Find an existing active record that matches on at least 3 of FirstName, MiddleName, LastName & BirthDate

                    var fields = new[]
                    {
                        (FieldName: Contact.Fields.FirstName, Value: _command.FirstName),
                        (FieldName: Contact.Fields.MiddleName, Value: _command.MiddleName),
                        (FieldName: Contact.Fields.LastName, Value: _command.LastName),
                        (FieldName: Contact.Fields.BirthDate, Value: (object)_command.BirthDate)
                    }.ToList();

                    // If fields are null in the input then don't try to match them (typically MiddleName)
                    fields.RemoveAll(f => f.Value == null);

                    var combinations = fields.GetCombinations(length: 3).ToArray();

                    if (combinations.Length == 0)
                    {
                        filter = default;
                        return false;
                    }

                    var combinationsFilter = new FilterExpression(LogicalOperator.Or);

                    foreach (var combination in combinations)
                    {
                        var innerFilter = new FilterExpression(LogicalOperator.And);

                        foreach (var (fieldName, value) in combination)
                        {
                            innerFilter.AddCondition(fieldName, ConditionOperator.Equal, value);
                        }

                        combinationsFilter.AddFilter(innerFilter);
                    }

                    filter = combinationsFilter;
                    return true;
                }
            }

            public async Task<CreateTeacherReferenceLookupResult> LookupReferenceData()
            {
                Debug.Assert(!string.IsNullOrEmpty(_command.InitialTeacherTraining.ProviderUkprn));
                Debug.Assert(!string.IsNullOrEmpty(_command.InitialTeacherTraining.Subject1));
                Debug.Assert(!string.IsNullOrEmpty(_command.InitialTeacherTraining.Subject2));
                Debug.Assert(!string.IsNullOrEmpty(_command.Qualification.ProviderUkprn));
                Debug.Assert(!string.IsNullOrEmpty(_command.Qualification.CountryCode));
                Debug.Assert(!string.IsNullOrEmpty(_command.Qualification.Subject));

                var isEarlyYears = _command.InitialTeacherTraining.ProgrammeType.IsEarlyYears();

                static TResult Let<T, TResult>(T value, Func<T, TResult> getResult) => getResult(value);

                var getIttProviderTask = Let(
                    _command.InitialTeacherTraining.ProviderUkprn,
                    ukprn => _dataverseAdapter._cache.GetOrCreateAsync(
                        CacheKeys.GetOrganizationByUkprnKey(ukprn),
                        _ => _dataverseAdapter.GetOrganizationByUkprn(ukprn)));

                var getIttCountryTask = Let(
                    "XK",  // XK == 'United Kingdom'
                    country => _dataverseAdapter._cache.GetOrCreateAsync(
                        CacheKeys.GetCountryKey(country),
                        _ => _dataverseAdapter.GetCountry(country)));

                var getSubject1Task = Let(
                    _command.InitialTeacherTraining.Subject1,
                    subject => _dataverseAdapter._cache.GetOrCreateAsync(
                        CacheKeys.GetIttSubjectKey(subject),
                        _ => _dataverseAdapter.GetIttSubjectByName(subject)));

                var getSubject2Task = Let(
                    _command.InitialTeacherTraining.Subject2,
                    subject => _dataverseAdapter._cache.GetOrCreateAsync(
                        CacheKeys.GetIttSubjectKey(subject),
                        _ => _dataverseAdapter.GetIttSubjectByName(subject)));

                var getQualificationTask = Let(
                    "First Degree",
                    qualificationName => _dataverseAdapter._cache.GetOrCreateAsync(
                        CacheKeys.GetHeQualificationKey(qualificationName),
                        _ => _dataverseAdapter.GetHeQualificationByName(qualificationName)));

                var getQualificationProviderTask = Let(
                    _command.Qualification.ProviderUkprn,
                    ukprn => _dataverseAdapter._cache.GetOrCreateAsync(
                        CacheKeys.GetOrganizationByUkprnKey(ukprn),
                        _ => _dataverseAdapter.GetOrganizationByUkprn(ukprn)));

                var getQualificationCountryTask = Let(
                    _command.Qualification.CountryCode,
                    country => _dataverseAdapter._cache.GetOrCreateAsync(
                        CacheKeys.GetCountryKey(country),
                        _ => _dataverseAdapter.GetCountry(country)));

                var getQualificationSubjectTask = Let(
                    _command.Qualification.Subject,
                    subjectName => _dataverseAdapter._cache.GetOrCreateAsync(
                        CacheKeys.GetHeSubjectKey(subjectName),
                        _ => _dataverseAdapter.GetHeSubjectByName(subjectName)));

                var getEarlyYearsStatusTask = isEarlyYears ?
                    Let(
                        "220", // 220 == 'Early Years Trainee'
                        earlyYearsStatusId => _dataverseAdapter._cache.GetOrCreateAsync(
                            CacheKeys.GetEarlyYearsStatusKey(earlyYearsStatusId),
                            _ => _dataverseAdapter.GetEarlyYearsStatus(earlyYearsStatusId))) :
                    Task.FromResult<dfeta_earlyyearsstatus>(null);

                var getTeacherStatusTask = !IsEarlyYears ?
                    Let(
                        _command.InitialTeacherTraining.ProgrammeType == dfeta_ITTProgrammeType.AssessmentOnlyRoute ?
                            "212" :  // 212 == 'AOR Candidate'
                            "211",   // 211 == 'Trainee Teacher:DMS'
                        teacherStatusId => _dataverseAdapter.GetTeacherStatus(teacherStatusId)) :
                    Task.FromResult<dfeta_teacherstatus>(null);

                await Task.WhenAll(getIttProviderTask,
                    getIttCountryTask,
                    getSubject1Task,
                    getSubject2Task,
                    getQualificationTask,
                    getQualificationProviderTask,
                    getQualificationCountryTask,
                    getQualificationSubjectTask,
                    getEarlyYearsStatusTask,
                    getTeacherStatusTask);

                Debug.Assert(!isEarlyYears || getEarlyYearsStatusTask.Result != null, "Early years status lookup failed.");
                Debug.Assert(isEarlyYears || getTeacherStatusTask.Result != null, "Teacher status lookup failed.");

                return new()
                {
                    IttProviderId = getIttProviderTask.Result?.Id,
                    IttCountryId = getIttCountryTask.Result?.Id,
                    IttSubject1Id = getSubject1Task.Result?.Id,
                    IttSubject2Id = getSubject2Task.Result?.Id,
                    QualificationId = getQualificationTask.Result?.Id,
                    QualificationProviderId = getQualificationProviderTask.Result?.Id,
                    QualificationCountryId = getQualificationCountryTask.Result?.Id,
                    QualificationSubjectId = getQualificationSubjectTask.Result?.Id,
                    EarlyYearsStatusId = getEarlyYearsStatusTask.Result?.Id,
                    TeacherStatusId = getTeacherStatusTask.Result?.Id
                };
            }

            public CreateTeacherFailedReasons ValidateReferenceData(CreateTeacherReferenceLookupResult referenceData)
            {
                var failedReasons = CreateTeacherFailedReasons.None;

                if (referenceData.IttProviderId == null)
                {
                    failedReasons |= CreateTeacherFailedReasons.IttProviderNotFound;
                }

                if (referenceData.IttSubject1Id == null)
                {
                    failedReasons |= CreateTeacherFailedReasons.Subject1NotFound;
                }

                if (referenceData.IttSubject2Id == null)
                {
                    failedReasons |= CreateTeacherFailedReasons.Subject2NotFound;
                }

                if (referenceData.QualificationId == null)
                {
                    failedReasons |= CreateTeacherFailedReasons.QualificationNotFound;
                }

                if (referenceData.QualificationProviderId == null)
                {
                    failedReasons |= CreateTeacherFailedReasons.QualificationProviderNotFound;
                }

                if (referenceData.QualificationCountryId == null)
                {
                    failedReasons |= CreateTeacherFailedReasons.QualificationCountryNotFound;
                }

                if (referenceData.QualificationSubjectId == null)
                {
                    failedReasons |= CreateTeacherFailedReasons.QualificationSubjectNotFound;
                }

                return failedReasons;
            }
        }

        internal class CreateTeacherReferenceLookupResult
        {
            public Guid? IttProviderId { get; set; }
            public Guid? IttCountryId { get; set; }
            public Guid? IttSubject1Id { get; set; }
            public Guid? IttSubject2Id { get; set; }
            public Guid? QualificationId { get; set; }
            public Guid? QualificationProviderId { get; set; }
            public Guid? QualificationCountryId { get; set; }
            public Guid? QualificationSubjectId { get; set; }
            public Guid? TeacherStatusId { get; set; }
            public Guid? EarlyYearsStatusId { get; set; }
        }

        internal class CreateTeacherDuplicateTeacherResult
        {
            public Guid TeacherId { get; set; }
            public string[] MatchedAttributes { get; set; }
            public bool HasActiveSanctions { get; set; }
            public bool HasQtsDate { get; set; }
            public bool HasEytsDate { get; set; }
        }
    }
}
