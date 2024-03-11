#nullable disable
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

namespace TeachingRecordSystem.Core.Dqt;

public partial class DataverseAdapter
{
    internal delegate Task<CreateTeacherDuplicateTeacherResult[]> FindExistingTeacher();

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

        var newContact = helper.CreateContactEntity();

        // Send a single Transaction request with all the data changes in.
        // This is important for atomicity; we really do not want torn writes here.
        var txnRequest = new ExecuteTransactionRequest()
        {
            ReturnResponses = true,
            Requests = new()
            {
                new CreateRequest() { Target = newContact },
                new CreateRequest() { Target = helper.CreateInitialTeacherTrainingEntity(referenceData) },
                new CreateRequest() { Target = helper.CreateQualificationEntity(referenceData) }
            }
        };

        helper.FlagBadData(txnRequest);

        var findExistingTeacherResult = await (findExistingTeacher ?? helper.FindExistingTeacher)();
        var allocateTrn = findExistingTeacherResult.Length == 0;
        string trn = null;

        if (allocateTrn)
        {
            trn = await GenerateTrn();
            newContact.dfeta_TRN = trn;
        }
        else
        {
            // Create a Task to review the potential duplicate
            Debug.Assert(findExistingTeacherResult != null);

            foreach (var duplicate in findExistingTeacherResult)
            {
                txnRequest.Requests.Add(new CreateRequest()
                {
                    Target = helper.CreateDuplicateReviewTaskEntity(duplicate)
                });
            }
        }

        var qtsEntity = helper.CreateQtsRegistrationEntity(referenceData);
        txnRequest.Requests.Add(new CreateRequest() { Target = qtsEntity });

        Guid? inductionId = null;
        if (command.TeacherType == CreateTeacherType.OverseasQualifiedTeacher)
        {
            var inductionEntity = helper.CreateInductionEntity();
            inductionId = inductionEntity.Id;
            txnRequest.Requests.Add(new CreateRequest() { Target = inductionEntity });

            // Update the QTS record with the induction ID; we can't set it in the Create above as Induction can't be created before QTS
            txnRequest.Requests.Add(new UpdateRequest()
            {
                Target = new dfeta_qtsregistration()
                {
                    Id = qtsEntity.Id,
                    dfeta_InductionId = inductionId.Value.ToEntityReference(dfeta_induction.EntityLogicalName)
                }
            });
        }

        var txnResponse = (ExecuteTransactionResponse)await _service.ExecuteAsync(txnRequest);

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

            var category = _command.TeacherType == CreateTeacherType.OverseasQualifiedTeacher ? "ApplyForQts" :
                !string.IsNullOrEmpty(_command.HusId) ? "HESAImportTrn" :
                "DMSImportTrn";

            return new CrmTask()
            {
                RegardingObjectId = TeacherId.ToEntityReference(Contact.EntityLogicalName),
                dfeta_potentialduplicateid = duplicate.TeacherId.ToEntityReference(Contact.EntityLogicalName),
                Category = category,
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
                        Contact.Fields.FirstName => $"  - First name: '{_command.FirstName}'",
                        Contact.Fields.MiddleName => $"  - Middle name: '{_command.MiddleName}'",
                        Contact.Fields.LastName => $"  - Last name: '{_command.LastName}'",
                        Contact.Fields.BirthDate => $"  - Date of birth: '{_command.BirthDate:dd/MM/yyyy}'",
                        Contact.Fields.dfeta_HUSID => $"  - HusId: '{_command.HusId}'",
                        Contact.Fields.dfeta_SlugId => $"  - SlugId: '{_command.SlugId}'",
                        $"{nameof(dfeta_initialteachertraining)}_{dfeta_initialteachertraining.Fields.dfeta_SlugId}" => $"  - ITT SlugId: '{_command.SlugId}'",
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

        public Models.Task CreateNameWithDigitsReviewTaskEntity(
            bool firstNameContainsDigit,
            bool middleNameContainsDigit,
            bool lastNameContainsDigit)
        {
            var description = GetDescription();

            return new Models.Task()
            {
                RegardingObjectId = TeacherId.ToEntityReference(Contact.EntityLogicalName),
                Category = "DMSImportTrn",
                Subject = "Notification for QTS Unit Team",
                Description = description,
                ScheduledEnd = _dataverseAdapter._clock.UtcNow
            };

            string GetDescription()
            {
                var badFields = new List<string>();

                if (firstNameContainsDigit)
                {
                    badFields.Add("first name");
                }

                if (middleNameContainsDigit)
                {
                    badFields.Add("middle name");
                }

                if (lastNameContainsDigit)
                {
                    badFields.Add("last name");
                }

                Debug.Assert(badFields.Count > 0);

                var description = badFields.ToCommaSeparatedString(finalValuesConjunction: "and")
                                  + $" contain{(badFields.Count == 1 ? "s" : "")} a digit";

                description = description[0..1].ToUpper() + description[1..];

                return description;
            }
        }

        public Contact CreateContactEntity()
        {
            var contact = new Contact()
            {
                Id = TeacherId,
                FirstName = _command.FirstName,
                MiddleName = _command.MiddleName,
                LastName = _command.LastName,
                dfeta_StatedFirstName = _command.StatedFirstName,
                dfeta_StatedMiddleName = _command.StatedMiddleName,
                dfeta_StatedLastName = _command.StatedLastName,
                BirthDate = _command.BirthDate,
                EMailAddress1 = _command.EmailAddress,
                Address1_Line1 = _command.Address?.AddressLine1,
                Address1_Line2 = _command.Address?.AddressLine2,
                Address1_Line3 = _command.Address?.AddressLine3,
                Address1_City = _command.Address?.City,
                Address1_PostalCode = _command.Address?.PostalCode,
                Address1_Country = _command.Address?.Country,
                GenderCode = _command.GenderCode,
                dfeta_HUSID = _command.HusId,
                dfeta_SlugId = _command.SlugId,
                dfeta_AllowPiiUpdatesFromRegister = true
            };

            // We get a NullReferenceException back from CRM if City is null or empty
            // (likely from a broken plugin).
            // Removing the attribute if it's empty solves the problem.
            if (string.IsNullOrEmpty(contact.Address1_City))
            {
                contact.Attributes.Remove(Contact.Fields.Address1_City);
            }

            return contact;
        }

        public dfeta_induction CreateInductionEntity()
        {
            Debug.Assert(_command.TeacherType == CreateTeacherType.OverseasQualifiedTeacher);

            var exemptionReason = _command.InductionRequired.Value ?
                (dfeta_InductionExemptionReason?)null :
                _command.RecognitionRoute switch
                {
                    CreateTeacherRecognitionRoute.Scotland => dfeta_InductionExemptionReason.HasoriseligibleforfullregistrationinScotland,
                    CreateTeacherRecognitionRoute.NorthernIreland => dfeta_InductionExemptionReason.SuccessfullycompletedinductioninNorthernIreland,
                    CreateTeacherRecognitionRoute.OverseasTrainedTeachers => dfeta_InductionExemptionReason.OverseasTrainedTeacher,
                    CreateTeacherRecognitionRoute.EuropeanEconomicArea => dfeta_InductionExemptionReason.QualifiedthroughEEAmutualrecognitionroute,
                    _ => throw new NotImplementedException($"Unknown {nameof(CreateTeacherRecognitionRoute)}: '{_command.RecognitionRoute}'.")
                };

            return new dfeta_induction()
            {
                Id = Guid.NewGuid(),
                dfeta_PersonId = TeacherId.ToEntityReference(Contact.EntityLogicalName),
                dfeta_InductionStatus = _command.InductionRequired.Value ? dfeta_InductionStatus.RequiredtoComplete : dfeta_InductionStatus.Exempt,
                dfeta_InductionExemptionReason = exemptionReason
            };
        }

        public dfeta_initialteachertraining CreateInitialTeacherTrainingEntity(CreateTeacherReferenceLookupResult referenceData)
        {
            Debug.Assert(referenceData.IttCountryId.HasValue);
            Debug.Assert(referenceData.IttProviderId.HasValue);

            var cohortYear = _command.InitialTeacherTraining.ProgrammeEndDate.Year.ToString();

            var result = _command.TeacherType == CreateTeacherType.OverseasQualifiedTeacher ?
                dfeta_ITTResult.Approved :
                _command.InitialTeacherTraining.ProgrammeType == dfeta_ITTProgrammeType.AssessmentOnlyRoute ?
                    dfeta_ITTResult.UnderAssessment :
                    dfeta_ITTResult.InTraining;

            return new dfeta_initialteachertraining()
            {
                dfeta_PersonId = TeacherId.ToEntityReference(Contact.EntityLogicalName),
                dfeta_CountryId = referenceData.IttCountryId.Value.ToEntityReference(dfeta_country.EntityLogicalName),
                dfeta_EstablishmentId = referenceData.IttProviderId.Value.ToEntityReference(Account.EntityLogicalName),
                dfeta_ProgrammeStartDate = _command.InitialTeacherTraining.ProgrammeStartDate.ToDateTime(),
                dfeta_ProgrammeEndDate = _command.InitialTeacherTraining.ProgrammeEndDate.ToDateTime(),
                dfeta_ProgrammeType = _command.InitialTeacherTraining.ProgrammeType,
                dfeta_CohortYear = cohortYear,
                dfeta_Subject1Id = referenceData.IttSubject1Id?.ToEntityReference(dfeta_ittsubject.EntityLogicalName),
                dfeta_Subject2Id = referenceData.IttSubject2Id?.ToEntityReference(dfeta_ittsubject.EntityLogicalName),
                dfeta_Subject3Id = referenceData.IttSubject3Id?.ToEntityReference(dfeta_ittsubject.EntityLogicalName),
                dfeta_Result = result,
                dfeta_AgeRangeFrom = _command.InitialTeacherTraining.AgeRangeFrom,
                dfeta_AgeRangeTo = _command.InitialTeacherTraining.AgeRangeTo,
                dfeta_TraineeID = _command.HusId,
                dfeta_ITTQualificationId = referenceData.IttQualificationId?.ToEntityReference(dfeta_ittqualification.EntityLogicalName),
                dfeta_ittqualificationaim = _command.InitialTeacherTraining.IttQualificationAim,
                dfeta_SlugId = _command.SlugId //slugid is the same as contact.slugid
            };
        }

        public dfeta_qualification CreateQualificationEntity(CreateTeacherReferenceLookupResult referenceData)
        {
            Debug.Assert(referenceData.QualificationId.HasValue);

            return new dfeta_qualification()
            {
                dfeta_PersonId = TeacherId.ToEntityReference(Contact.EntityLogicalName),
                dfeta_Type = dfeta_qualification_dfeta_Type.HigherEducation,
                dfeta_HE_CountryId = referenceData.QualificationCountryId?.ToEntityReference(dfeta_country.EntityLogicalName),
                dfeta_HE_HESubject1Id = referenceData.QualificationSubjectId?.ToEntityReference(dfeta_hesubject.EntityLogicalName),
                dfeta_HE_ClassDivision = _command.Qualification?.Class,
                dfeta_HE_EstablishmentId = referenceData.QualificationProviderId?.ToEntityReference(Account.EntityLogicalName),
                dfeta_HE_CompletionDate = _command.Qualification?.Date?.ToDateTime(),
                dfeta_HE_HEQualificationId = referenceData.QualificationId.Value.ToEntityReference(dfeta_hequalification.EntityLogicalName),
                dfeta_HE_HESubject2Id = referenceData.QualificationSubject2Id?.ToEntityReference(dfeta_hesubject.EntityLogicalName),
                dfeta_HE_HESubject3Id = referenceData.QualificationSubject3Id?.ToEntityReference(dfeta_hesubject.EntityLogicalName),
            };
        }

        public dfeta_qtsregistration CreateQtsRegistrationEntity(CreateTeacherReferenceLookupResult referenceData)
        {
            return new dfeta_qtsregistration()
            {
                Id = Guid.NewGuid(),
                dfeta_PersonId = TeacherId.ToEntityReference(Contact.EntityLogicalName),
                dfeta_EarlyYearsStatusId = referenceData.EarlyYearsStatusId?.ToEntityReference(dfeta_earlyyearsstatus.EntityLogicalName),
                dfeta_TeacherStatusId = referenceData.TeacherStatusId?.ToEntityReference(dfeta_teacherstatus.EntityLogicalName),
                dfeta_QTSDate = _command.QtsDate.ToDateTime()
            };
        }

        public async Task<CreateTeacherDuplicateTeacherResult[]> FindExistingTeacher()
        {
            var duplicateResults = new List<CreateTeacherDuplicateTeacherResult>();
            var filter = new FilterExpression(LogicalOperator.And);
            filter.AddCondition(Contact.Fields.StateCode, ConditionOperator.Equal, (int)ContactState.Active);

            if (TryGetMatchCombinationsFilter(out var matchCombinationsFilter))
            {
                filter.AddFilter(matchCombinationsFilter);
            }
            else
            {
                // Not enough data in the input to match on
                return Array.Empty<CreateTeacherDuplicateTeacherResult>();
            }

            var query = new QueryExpression(Contact.EntityLogicalName)
            {
                ColumnSet = new()
                {
                    Columns =
                    {
                        Contact.Fields.dfeta_ActiveSanctions,
                        Contact.Fields.dfeta_QTSDate,
                        Contact.Fields.dfeta_EYTSDate,
                        Contact.Fields.FirstName,
                        Contact.Fields.MiddleName,
                        Contact.Fields.LastName,
                        Contact.Fields.BirthDate,
                        Contact.Fields.dfeta_HUSID,
                        Contact.Fields.dfeta_SlugId
                    }
                },
                Criteria = filter
            };

            var result = await _dataverseAdapter._service.RetrieveMultipleAsync(query);

            // Old implementation returns the first record that matches on at least three attributes; replicating that here
            var matches = result.Entities.Select(entity => entity.ToEntity<Contact>()).ToList();

            // if a teacher exists that contains an itt record with a slugid that matches request slugid, use it
            // in potential duplicate check
            var teachersWithIttWithSlugs = string.IsNullOrEmpty(_command.SlugId) ? Array.Empty<Contact>() : await _dataverseAdapter.GetTeachersByInitialTeacherTrainingSlugId(_command.SlugId, columnNames: new[] { Contact.Fields.dfeta_TRN, Contact.Fields.dfeta_SlugId }, null);
            if (teachersWithIttWithSlugs.Any())
            {
                matches.AddRange(teachersWithIttWithSlugs);
            }


            foreach (var match in matches)
            {
                if (match == null)
                {
                    return null;
                }

                var attributeMatches = new[]
                {
                (
                    Attribute: Contact.Fields.FirstName,
                    Matches: NamesAreEqual(_command.FirstName, match.FirstName)
                ),
                (
                    Attribute: Contact.Fields.MiddleName,
                    Matches: NamesAreEqual(_command.MiddleName, match.MiddleName)
                ),
                (
                    Attribute: Contact.Fields.LastName,
                    Matches: NamesAreEqual(_command.LastName, match.LastName)
                ),
                (
                    Attribute: Contact.Fields.BirthDate,
                    Matches: _command.BirthDate.Equals(match.BirthDate)
                )
            };

                if (!string.IsNullOrEmpty(_command.HusId))
                {
                    attributeMatches = attributeMatches.Concat(new[] { (Contact.Fields.dfeta_HUSID, _command.HusId.Equals(match.dfeta_HUSID)) }).ToArray();
                }

                if (!string.IsNullOrEmpty(_command.SlugId))
                {
                    attributeMatches = attributeMatches.Concat(new[] { (Contact.Fields.dfeta_SlugId, _command.SlugId.Equals(match.dfeta_SlugId, StringComparison.OrdinalIgnoreCase)) }).ToArray();
                }

                if (!string.IsNullOrEmpty(_command.SlugId) && teachersWithIttWithSlugs.Any())
                {
                    attributeMatches = attributeMatches.Concat(new[] { ($"{nameof(dfeta_initialteachertraining)}_{dfeta_initialteachertraining.Fields.dfeta_SlugId}", true) }).ToArray();
                }

                var matchedAttributeNames = attributeMatches.Where(m => m.Matches).Select(m => m.Attribute).ToArray();

                duplicateResults.Add(new CreateTeacherDuplicateTeacherResult()
                {
                    TeacherId = match.Id,
                    MatchedAttributes = matchedAttributeNames,
                    HasActiveSanctions = match.dfeta_ActiveSanctions == true,
                    HasQtsDate = match.dfeta_QTSDate.HasValue,
                    HasEytsDate = match.dfeta_EYTSDate.HasValue,
                    HusId = match.dfeta_HUSID,
                    SlugId = match.dfeta_SlugId
                });
            }

            return duplicateResults.ToArray();

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
                fields.RemoveAll(f => f.Value == null || (f.Value is string stringValue && string.IsNullOrEmpty(stringValue)));

                var combinations = fields.GetCombinations(length: 3).ToArray();

                if (combinations.Length == 0)
                {
                    filter = default;
                    return false;
                }

                var combinationsFilter = new FilterExpression(LogicalOperator.Or);

                // HusId overrides at least 3 matches so needs to be in its own block
                if (!string.IsNullOrEmpty(_command.HusId))
                {
                    var husIdFilter = new FilterExpression(LogicalOperator.Or);
                    husIdFilter.AddCondition(Contact.Fields.dfeta_HUSID, ConditionOperator.Equal, _command.HusId);
                    combinationsFilter.AddFilter(husIdFilter);
                }

                // SlugId overrides at least 3 matches so needs to be in its own block
                if (!string.IsNullOrEmpty(_command.SlugId))
                {
                    var slugIdFilter = new FilterExpression(LogicalOperator.Or);
                    slugIdFilter.AddCondition(Contact.Fields.dfeta_SlugId, ConditionOperator.Equal, _command.SlugId);
                    combinationsFilter.AddFilter(slugIdFilter);
                }

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

            static bool NamesAreEqual(string a, string b) =>
                string.Compare(a, b, CultureInfo.InvariantCulture, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) == 0;
        }

        public string DeriveTeacherStatus(out bool qtsDateRequired)
        {
            if (_command.TeacherType == CreateTeacherType.OverseasQualifiedTeacher)
            {
                Debug.Assert(_command.RecognitionRoute.HasValue);

                qtsDateRequired = true;
                return (_command.RecognitionRoute.Value, _command.UnderNewOverseasRegulations.GetValueOrDefault(false)) switch
                {
                    (CreateTeacherRecognitionRoute.Scotland, _) => "68",
                    (CreateTeacherRecognitionRoute.NorthernIreland, _) => "69",
                    (CreateTeacherRecognitionRoute.EuropeanEconomicArea, _) => "223",
                    (CreateTeacherRecognitionRoute.OverseasTrainedTeachers, false) => "103",
                    (CreateTeacherRecognitionRoute.OverseasTrainedTeachers, true) => "104",
                    _ => throw new NotImplementedException($"Unknown {nameof(CreateTeacherRecognitionRoute)}: '{_command.RecognitionRoute.Value}'.")
                };
            }

            Debug.Assert(_command.TeacherType == CreateTeacherType.TraineeTeacher);

            qtsDateRequired = false;
            return _command.InitialTeacherTraining.ProgrammeType == dfeta_ITTProgrammeType.AssessmentOnlyRoute ?
                "212" :  // 212 == 'AOR Candidate'
                "211";   // 211 == 'Trainee Teacher'
        }

        public string DeriveIttProviderNameForOverseasQualifiedTeacher()
        {
            Debug.Assert(_command.TeacherType == CreateTeacherType.OverseasQualifiedTeacher);

            return _command.RecognitionRoute.Value switch
            {
                CreateTeacherRecognitionRoute.Scotland or CreateTeacherRecognitionRoute.NorthernIreland => "UK establishment (Scotland/Northern Ireland)",
                CreateTeacherRecognitionRoute.EuropeanEconomicArea or CreateTeacherRecognitionRoute.OverseasTrainedTeachers => "Non-UK establishment",
                _ => throw new NotImplementedException($"Unknown {nameof(CreateTeacherRecognitionRoute)}: '{_command.RecognitionRoute.Value}'.")
            };
        }

        public void FlagBadData(ExecuteTransactionRequest txnRequest)
        {
            var firstNameContainsDigit = _command.FirstName.Any(Char.IsDigit);
            var middleNameContainsDigit = _command.MiddleName?.Any(Char.IsDigit) ?? false;
            var lastNameContainsDigit = _command.LastName.Any(Char.IsDigit);

            if (firstNameContainsDigit || middleNameContainsDigit || lastNameContainsDigit)
            {
                txnRequest.Requests.Add(new CreateRequest()
                {
                    Target = CreateNameWithDigitsReviewTaskEntity(firstNameContainsDigit, middleNameContainsDigit, lastNameContainsDigit)
                });
            }
        }

        public async Task<CreateTeacherReferenceLookupResult> LookupReferenceData()
        {
            Debug.Assert(_command.TeacherType == CreateTeacherType.OverseasQualifiedTeacher || !string.IsNullOrEmpty(_command.InitialTeacherTraining.ProviderUkprn));
            Debug.Assert(_command.TeacherType != CreateTeacherType.OverseasQualifiedTeacher || !string.IsNullOrEmpty(_command.InitialTeacherTraining.TrainingCountryCode));

            var isEarlyYears = _command.InitialTeacherTraining.ProgrammeType?.IsEarlyYears() == true;

            var requestBuilder = _dataverseAdapter.CreateMultipleRequestBuilder();

            static TResult Let<T, TResult>(T value, Func<T, TResult> getResult) => getResult(value);

            var getIttProviderTask = !string.IsNullOrEmpty(_command.InitialTeacherTraining.ProviderUkprn) ?
                Let(
                    _command.InitialTeacherTraining.ProviderUkprn,
                    ukprn => _dataverseAdapter._cache.GetOrCreateUnlessNullAsync(
                        CacheKeys.GetIttProviderOrganizationByUkprnKey(ukprn),
                        () => _dataverseAdapter.GetIttProviderOrganizationsByUkprn(ukprn, true, columnNames: Array.Empty<string>(), requestBuilder)
                            .ContinueWith(t => t.Result.SingleOrDefault()))) :
                Let(
                    DeriveIttProviderNameForOverseasQualifiedTeacher(),
                    providerName => _dataverseAdapter._cache.GetOrCreateUnlessNullAsync(
                        CacheKeys.GetIttProviderOrganizationByNameKey(providerName),
                        () => _dataverseAdapter.GetIttProviderOrganizationsByName(providerName, true, columnNames: Array.Empty<string>(), requestBuilder)
                            .ContinueWith(t => t.Result.SingleOrDefault())));

            var getIttCountryTask = Let(
                string.IsNullOrEmpty(_command.InitialTeacherTraining.TrainingCountryCode) ?
                    "XK" :  // XK == 'United Kingdom'
                    _command.InitialTeacherTraining.TrainingCountryCode,
                country => _dataverseAdapter._cache.GetOrCreateUnlessNullAsync(
                    CacheKeys.GetCountryKey(country),
                    () => _dataverseAdapter.GetCountry(country, requestBuilder)));

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

            var getQualificationSubjectTask2 = !string.IsNullOrEmpty(_command.Qualification?.Subject2) ?
                Let(
                    _command.Qualification.Subject2,
                    subjectName => _dataverseAdapter._cache.GetOrCreateUnlessNullAsync(
                        CacheKeys.GetHeSubjectKey(subjectName),
                        () => _dataverseAdapter.GetHeSubjectByCode(subjectName, requestBuilder))) :
                null;

            var getQualificationSubjectTask3 = !string.IsNullOrEmpty(_command.Qualification?.Subject3) ?
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
                    DeriveTeacherStatus(out var qtsDateRequired),
                    teacherStatusId => _dataverseAdapter._cache.GetOrCreateUnlessNullAsync(
                        CacheKeys.GetTeacherStatusKey(teacherStatusId),
                        () => _dataverseAdapter.GetTeacherStatus(teacherStatusId, requestBuilder))) :
                Task.FromResult<dfeta_teacherstatus>(null);

            var existingTeachersWithHusIdTask = !string.IsNullOrEmpty(_command.HusId) ?
                _dataverseAdapter.GetTeachersByHusId(_command.HusId, columnNames: Array.Empty<string>()) :
                Task.FromResult<Contact[]>(null);

            var lookupTasks = new Task[]
                {
                    getIttProviderTask,
                    getIttCountryTask,
                    getSubject1Task,
                    getSubject2Task,
                    getSubject3Task,
                    getIttQualificationTask,
                    getQualificationTask,
                    getQualificationProviderTask,
                    getQualificationCountryTask,
                    getQualificationSubjectTask,
                    getEarlyYearsStatusTask,
                    getTeacherStatusTask,
                    getQualificationSubjectTask2,
                    getQualificationSubjectTask3,
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
                QualificationProviderId = getQualificationProviderTask?.Result?.Id,
                QualificationCountryId = getQualificationCountryTask?.Result?.Id,
                QualificationSubjectId = getQualificationSubjectTask?.Result?.Id,
                EarlyYearsStatusId = getEarlyYearsStatusTask?.Result?.Id,
                TeacherStatusId = getTeacherStatusTask?.Result?.Id,
                QualificationSubject2Id = getQualificationSubjectTask2?.Result?.Id,
                QualificationSubject3Id = getQualificationSubjectTask3?.Result?.Id,
                HaveExistingTeacherWithHusId = existingTeachersWithHusIdTask?.Result?.Length > 0
            };
        }

        public CreateTeacherFailedReasons ValidateReferenceData(CreateTeacherReferenceLookupResult referenceData)
        {
            var failedReasons = CreateTeacherFailedReasons.None;

            if (referenceData.IttProviderId == null)
            {
                failedReasons |= CreateTeacherFailedReasons.IttProviderNotFound;
            }

            if (referenceData.IttSubject1Id == null && !string.IsNullOrEmpty(_command.InitialTeacherTraining.Subject1))
            {
                failedReasons |= CreateTeacherFailedReasons.Subject1NotFound;
            }

            if (referenceData.IttSubject2Id == null && !string.IsNullOrEmpty(_command.InitialTeacherTraining.Subject2))
            {
                failedReasons |= CreateTeacherFailedReasons.Subject2NotFound;
            }

            if (referenceData.IttSubject3Id == null && !string.IsNullOrEmpty(_command.InitialTeacherTraining.Subject3))
            {
                failedReasons |= CreateTeacherFailedReasons.Subject3NotFound;
            }

            if (referenceData.IttQualificationId == null && !string.IsNullOrEmpty(_command.InitialTeacherTraining.IttQualificationValue))
            {
                failedReasons |= CreateTeacherFailedReasons.IttQualificationNotFound;
            }

            if (referenceData.QualificationProviderId == null && !string.IsNullOrEmpty(_command.Qualification?.ProviderUkprn))
            {
                failedReasons |= CreateTeacherFailedReasons.QualificationProviderNotFound;
            }

            if (referenceData.QualificationCountryId == null && !string.IsNullOrEmpty(_command.Qualification?.CountryCode))
            {
                failedReasons |= CreateTeacherFailedReasons.QualificationCountryNotFound;
            }

            if (referenceData.QualificationSubjectId == null && !string.IsNullOrEmpty(_command.Qualification?.Subject))
            {
                failedReasons |= CreateTeacherFailedReasons.QualificationSubjectNotFound;
            }

            if (referenceData.QualificationSubject2Id == null && !string.IsNullOrEmpty(_command.Qualification?.Subject2))
            {
                failedReasons |= CreateTeacherFailedReasons.QualificationSubject2NotFound;
            }

            if (referenceData.QualificationSubject3Id == null && !string.IsNullOrEmpty(_command.Qualification?.Subject3))
            {
                failedReasons |= CreateTeacherFailedReasons.QualificationSubject3NotFound;
            }

            if (referenceData.QualificationId == null)
            {
                failedReasons |= CreateTeacherFailedReasons.QualificationNotFound;
            }

            if (referenceData.IttCountryId == null)
            {
                failedReasons |= CreateTeacherFailedReasons.TrainingCountryNotFound;
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
        public Guid? IttSubject3Id { get; set; }
        public Guid? IttQualificationId { get; set; }
        public Guid? QualificationId { get; set; }
        public Guid? QualificationProviderId { get; set; }
        public Guid? QualificationCountryId { get; set; }
        public Guid? QualificationSubjectId { get; set; }
        public Guid? QualificationSubject2Id { get; set; }
        public Guid? QualificationSubject3Id { get; set; }
        public Guid? TeacherStatusId { get; set; }
        public Guid? EarlyYearsStatusId { get; set; }
        public bool? HaveExistingTeacherWithHusId { get; set; }
    }

    internal class CreateTeacherDuplicateTeacherResult
    {
        public Guid TeacherId { get; set; }
        public string[] MatchedAttributes { get; set; }
        public bool HasActiveSanctions { get; set; }
        public bool HasQtsDate { get; set; }
        public bool HasEytsDate { get; set; }
        public string HusId { get; set; }
        public string SlugId { get; set; }
    }
}
