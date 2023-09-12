#nullable disable
using System.Diagnostics;
using System.Text;
using Microsoft.Xrm.Sdk.Messages;
using Optional.Unsafe;

namespace TeachingRecordSystem.Core.Dqt;

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

        var (itt, ittLookupFailedReasons) = helper.SelectIttRecord(referenceData.Itt, referenceData.IttProviderId.Value, command.SlugId.ValueOrDefault());
        var isEarlyYears = command.InitialTeacherTraining.ProgrammeType.IsEarlyYears();

        if (isEarlyYears && referenceData.Teacher.dfeta_EYTSDate.HasValue)
        {
            return (UpdateTeacherResult.Failed(UpdateTeacherFailedReasons.AlreadyHaveEytsDate), null);
        }
        else if (!isEarlyYears && referenceData.Teacher.dfeta_QTSDate.HasValue)
        {
            return (UpdateTeacherResult.Failed(UpdateTeacherFailedReasons.AlreadyHaveQtsDate), null);
        }

        if (itt != null && itt.dfeta_ProgrammeType?.IsEarlyYears() != command.InitialTeacherTraining.ProgrammeType.IsEarlyYears())
        {
            return (UpdateTeacherResult.Failed(UpdateTeacherFailedReasons.CannotChangeProgrammeType), null);
        }

        //only allow result to go to deferred,intraining, underassessment
        //if dqt hold withdrawn & request is withdrawn - return succeess, without updating
        if (command.InitialTeacherTraining.Result != null)
        {
            switch (command.InitialTeacherTraining.Result)
            {
                case dfeta_ITTResult.Withdrawn:
                    {
                        if (itt != null && itt.dfeta_Result == dfeta_ITTResult.Withdrawn)
                        {
                            return (UpdateTeacherResult.Success(helper.TeacherId, null), null);
                        }
                        else
                        {
                            throw new ArgumentException($"Invalid ITT outcome: '{command.InitialTeacherTraining.Result}'.", nameof(command.InitialTeacherTraining.Result));
                        }
                    }
                case dfeta_ITTResult.Deferred:
                case dfeta_ITTResult.InTraining:
                case dfeta_ITTResult.UnderAssessment:
                    break;
                default:
                    throw new ArgumentException($"Invalid ITT outcome: '{command.InitialTeacherTraining.Result}'.", nameof(command.InitialTeacherTraining.Result));
            }

            // Unable to transition from failed to deferred,intraining or underassessment
            if (itt != null && itt.dfeta_Result == dfeta_ITTResult.Fail)
            {
                switch (command.InitialTeacherTraining.Result)
                {
                    case dfeta_ITTResult.Deferred:
                    case dfeta_ITTResult.InTraining:
                    case dfeta_ITTResult.UnderAssessment:
                        return (UpdateTeacherResult.Failed(UpdateTeacherFailedReasons.UnableToChangeFailedResult), null);
                }
            }
        }

        if (itt != null && itt.dfeta_Result == dfeta_ITTResult.Withdrawn)
        {
            // Do not allow incomming result to be set to deferred if existing result is withdrawn
            // Do not allow result to be underassessment when programmetype is not assessmentOnlyRoute
            // Do not allow result to be intraining when programmetype is assessmentonlyroute
            if (command.InitialTeacherTraining.Result == dfeta_ITTResult.Deferred)
            {
                return (UpdateTeacherResult.Failed(UpdateTeacherFailedReasons.UnableToUnwithdrawToDeferredStatus), null);
            }
            else if (itt.dfeta_ProgrammeType == dfeta_ITTProgrammeType.AssessmentOnlyRoute && command.InitialTeacherTraining.Result != dfeta_ITTResult.UnderAssessment)
            {
                return (UpdateTeacherResult.Failed(UpdateTeacherFailedReasons.InTrainingResultNotPermittedForProgrammeType), null);
            }
            else if (itt.dfeta_ProgrammeType != dfeta_ITTProgrammeType.AssessmentOnlyRoute && command.InitialTeacherTraining.Result == dfeta_ITTResult.UnderAssessment)
            {
                return (UpdateTeacherResult.Failed(UpdateTeacherFailedReasons.UnderAssessmentOnlyPermittedForProgrammeType), null);
            }
        }

        // Send a single Transaction request with all the data changes in.
        // This is important for atomicity; we really do not want torn writes here.
        var txnRequest = new ExecuteTransactionRequest()
        {
            ReturnResponses = true,
            Requests = new()
        };

        //When a non early programme type is changed to AOR/Trainee Teacher

        if (itt != null && itt.dfeta_Result == dfeta_ITTResult.Withdrawn)
        {
            //match an existing qts record based on programmetype
            //if earlyyears programme type then earlyyearsstatusid must be empty
            //if not earlyyears then teacherstatusid must be empty
            var (existingQTS, failedQts) = helper.SelectWithdrawnQtsRegistrationRecord(referenceData.QtsRegistrations, itt.dfeta_ProgrammeType.Value);

            if (!failedQts.HasValue)
            {
                var qtsUpdate = new dfeta_qtsregistration() { Id = existingQTS.Id };
                if (isEarlyYears)
                {
                    //Withdrawn EYTS programmeType can go to earlyyearsTrainee
                    qtsUpdate.dfeta_EarlyYearsStatusId = referenceData.EarlyYearsTraineeStatusId.ToEntityReference(dfeta_earlyyearsstatus.EntityLogicalName);
                }
                else
                {
                    //Withdrawn AOR programmetype can go to AOR candidate, otherwise go to Trainee teacher
                    if (itt.dfeta_ProgrammeType == dfeta_ITTProgrammeType.AssessmentOnlyRoute)
                    {
                        qtsUpdate.dfeta_TeacherStatusId = referenceData.AorCandidateTeacherStatusId.ToEntityReference(dfeta_teacherstatus.EntityLogicalName);
                    }
                    else
                    {
                        qtsUpdate.dfeta_TeacherStatusId = referenceData.TraineeTeacherDmsTeacherStatusId.ToEntityReference(dfeta_teacherstatus.EntityLogicalName);
                    }
                }
                txnRequest.Requests.Add(new UpdateRequest()
                {
                    Target = qtsUpdate
                });
            }
            else
            {
                if (failedQts == UpdateTeacherFailedReasons.NoMatchingQtsRecord)
                {
                    return (UpdateTeacherResult.Failed(UpdateTeacherFailedReasons.NoMatchingQtsRecord), null);
                }
            }
        }
        else if (itt != null && itt.dfeta_ProgrammeType?.IsEarlyYears() == false && itt.dfeta_ProgrammeType != command.InitialTeacherTraining.ProgrammeType)
        {
            var (existingQTS, failedQts) = helper.SelectAOROrTraineeeTeacherQtsRecord(referenceData.QtsRegistrations, itt.dfeta_ProgrammeType.Value, referenceData.AorCandidateTeacherStatusId, referenceData.TraineeTeacherDmsTeacherStatusId);
            if (!failedQts.HasValue)
            {
                //AOR programmetype can go to AOR candidate, otherwise go to Trainee teacher
                if (command.InitialTeacherTraining.ProgrammeType == dfeta_ITTProgrammeType.AssessmentOnlyRoute)
                {
                    existingQTS.dfeta_TeacherStatusId = referenceData.AorCandidateTeacherStatusId.ToEntityReference(dfeta_teacherstatus.EntityLogicalName);
                }
                else
                {
                    existingQTS.dfeta_TeacherStatusId = referenceData.TraineeTeacherDmsTeacherStatusId.ToEntityReference(dfeta_teacherstatus.EntityLogicalName);
                }
            }
            txnRequest.Requests.Add(new UpdateRequest()
            {
                Target = existingQTS
            });

            if (failedQts == UpdateTeacherFailedReasons.NoMatchingQtsRecord)
            {
                return (UpdateTeacherResult.Failed(UpdateTeacherFailedReasons.NoMatchingQtsRecord), null);
            }
        }

        if (ittLookupFailedReasons == UpdateTeacherFailedReasons.MultipleInTrainingIttRecords)
        {
            return (UpdateTeacherResult.Failed(UpdateTeacherFailedReasons.MultipleInTrainingIttRecords), null);
        }
        else if (ittLookupFailedReasons == UpdateTeacherFailedReasons.NoMatchingIttRecord)
        {
            return (UpdateTeacherResult.Failed(UpdateTeacherFailedReasons.NoMatchingIttRecord), null);
        }
        else
        {
            // update existing itt record
            Debug.Assert(itt != null);
            txnRequest.Requests.Add(new UpdateRequest()
            {
                Target = helper.CreateInitialTeacherTrainingEntity(referenceData, itt.Id, itt)
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

        if (referenceData.TeacherHusId != command.HusId.ValueOrDefault() || referenceData.Teacher.dfeta_SlugId != command.SlugId.ValueOrDefault() || helper.ShouldUpdatePII(referenceData.Teacher.dfeta_AllowPiiUpdatesFromRegister) == true)
        {
            var contact = helper.CreateContactEntity(referenceData.HavePendingPiiChanges, referenceData.Teacher.dfeta_TSPersonID, referenceData.Teacher.dfeta_AllowPiiUpdatesFromRegister);

            var updateRequest = new UpdateRequest()
            {
                Target = contact,
            };

            //parameter is set so that the PersonPiiUpdated plugin will not
            //attempt to set the dfeta_allowpiiupdatesfromregister field to false.
            //teachers created through the api can have their pii data updated as long
            //as there is no pending pii data changed & the teacher has not logged in
            //through the TSSP.
            updateRequest.Parameters.Add("tag", "AllowRegisterPiiUpdates");
            txnRequest.Requests.Add(updateRequest);
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
            SlugId = command.SlugId.ValueOrDefault();
        }

        public Guid TeacherId { get; }

        public string Trn { get; }

        public string SlugId { get; }


        public (dfeta_qtsregistration, UpdateTeacherFailedReasons? FailedReason) SelectAOROrTraineeeTeacherQtsRecord(
    IEnumerable<dfeta_qtsregistration> qtsRecords,
    dfeta_ITTProgrammeType programmeType,
    Guid aorCandidateTeacherStatusId,
    Guid traineeTeacherDmsTeacherStatusId)
        {
            var matching = new List<dfeta_qtsregistration>();

            foreach (var qts in qtsRecords)
            {
                if (!programmeType.IsEarlyYears() && (qts.dfeta_TeacherStatusId?.Id == aorCandidateTeacherStatusId || qts.dfeta_TeacherStatusId?.Id == traineeTeacherDmsTeacherStatusId))
                {
                    matching.Add(qts);
                }
            }

            if (matching.Count == 0)
            {
                return (null, UpdateTeacherFailedReasons.NoMatchingQtsRecord);
            }
            else if (matching.Count > 1)
            {
                return (null, UpdateTeacherFailedReasons.MultipleQtsRecords);
            }
            else
            {
                return (matching[0], null);
            }
        }

        public (dfeta_qtsregistration, UpdateTeacherFailedReasons? FailedReason) SelectWithdrawnQtsRegistrationRecord(
            IEnumerable<dfeta_qtsregistration> qtsRecords,
            dfeta_ITTProgrammeType programmeType)
        {
            var matching = new List<dfeta_qtsregistration>();

            foreach (var qts in qtsRecords)
            {
                //If early years programmetype and earlyyearsstatusid is empty (i.e. cleared out from an itt record being withdrawn)
                //else if not early years programmetype and teacherstatusid is empty (i.e. cleared out from itt record being withdrawn)
                if (programmeType.IsEarlyYears() && qts.dfeta_EarlyYearsStatusId == null)
                {
                    matching.Add(qts);
                }
                else if (!programmeType.IsEarlyYears() && qts.dfeta_TeacherStatusId == null)
                {
                    matching.Add(qts);
                }
            }

            if (matching.Count == 0)
            {
                return (null, UpdateTeacherFailedReasons.NoMatchingQtsRecord);
            }
            else if (matching.Count > 1)
            {
                return (null, UpdateTeacherFailedReasons.MultipleQtsRecords);
            }
            else
            {
                return (matching[0], null);
            }
        }

        public (dfeta_initialteachertraining Result, UpdateTeacherFailedReasons? FailedReason) SelectIttRecord(
                IEnumerable<dfeta_initialteachertraining> ittRecords,
                Guid ittProviderId,
                string slugId)
        {
            // if SlugId is passed in, use slugid for matching
            // otherwise fallback to matching on establishmet
            // The record should be at the InTraining or Deferred or Withdrawn status unless the programme is 'assessment only',
            // in which case the status should be UnderAssessment or Deferred or withdrawn.

            List<dfeta_initialteachertraining> matching = new List<dfeta_initialteachertraining>();
            var activeForProvider = Array.Empty<dfeta_initialteachertraining>();

            if (!string.IsNullOrEmpty(slugId))
            {
                activeForProvider = ittRecords
                    .Where(r => r.dfeta_SlugId == slugId && r.StateCode == dfeta_initialteachertrainingState.Active)
                    .ToArray();
            }

            if (activeForProvider.Length == 0)
            {
                activeForProvider = ittRecords
                    .Where(r => r.dfeta_EstablishmentId?.Id == ittProviderId && string.IsNullOrEmpty(r.dfeta_SlugId) && r.StateCode == dfeta_initialteachertrainingState.Active)
                    .ToArray();
            }

            // All incomplete qts/eyts records, regardless of provider
            var incompleteEytsOrQts = ittRecords
                .Where(r => r.StateCode == dfeta_initialteachertrainingState.Active &&
                 (r.dfeta_Result == dfeta_ITTResult.UnderAssessment ||
                  r.dfeta_Result == dfeta_ITTResult.Withdrawn ||
                  r.dfeta_Result == dfeta_ITTResult.Deferred ||
                  r.dfeta_Result == dfeta_ITTResult.Fail ||
                  r.dfeta_Result == dfeta_ITTResult.InTraining))
                .ToArray();
            var hasMoreThanOneActiveEytsOrQts = incompleteEytsOrQts.Count() > 1;

            foreach (var itt in activeForProvider)
            {
                if (itt.dfeta_ProgrammeType == dfeta_ITTProgrammeType.AssessmentOnlyRoute)
                {
                    switch (itt.dfeta_Result)
                    {
                        case dfeta_ITTResult.UnderAssessment:
                        case dfeta_ITTResult.Withdrawn:
                        case dfeta_ITTResult.Deferred:
                        case dfeta_ITTResult.Fail:
                            {
                                matching.Add(itt);
                                break;
                            }
                    }
                }
                else if (itt.dfeta_ProgrammeType != dfeta_ITTProgrammeType.AssessmentOnlyRoute)
                {
                    switch (itt.dfeta_Result)
                    {
                        case dfeta_ITTResult.InTraining:
                        case dfeta_ITTResult.Withdrawn:
                        case dfeta_ITTResult.Deferred:
                        case dfeta_ITTResult.Fail:
                            {
                                matching.Add(itt);
                                break;
                            }
                    }
                }
            }

            if (matching.Count == 1 && !hasMoreThanOneActiveEytsOrQts)
            {
                return (matching[0], null);
            }
            else if (matching.Count > 1 || hasMoreThanOneActiveEytsOrQts)
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

        public bool ShouldUpdatePII(bool? allowPiiUpdatesFromRegisterfield)
        {
            if (allowPiiUpdatesFromRegisterfield == true && (_command.FirstName.HasValue ||
                _command.MiddleName.HasValue ||
                _command.LastName.HasValue ||
                _command.DateOfBirth.HasValue ||
                _command.EmailAddress.HasValue ||
                _command.GenderCode.HasValue))
            {
                return true;
            }
            return false;
        }

        public Contact CreateContactEntity(bool havePendingPiiChanges, string tspersonid, bool? allowPiiUpdatesFromRegister = false)
        {
            var contact = new Contact()
            {
                Id = TeacherId,
            };

            _command.SlugId.MatchSome(value => contact.dfeta_SlugId = value);
            _command.HusId.MatchSome(value => contact.dfeta_HUSID = value);

            //If teacher was created via TRS api AND
            //   teacher has not logged into TSSP AND
            //   teacher does not have an existing pending pii change
            //Then pii changes are permitted
            if (havePendingPiiChanges == false && allowPiiUpdatesFromRegister == true)
            {
                _command.FirstName.MatchSome(value => contact.FirstName = value);
                _command.MiddleName.MatchSome(value => contact.MiddleName = value);
                _command.LastName.MatchSome(value => contact.LastName = value);
                _command.GenderCode.MatchSome(value => contact.GenderCode = value);
                _command.DateOfBirth.MatchSome(value => contact.BirthDate = value);
                _command.StatedFirstName.MatchSome(value => contact.dfeta_StatedFirstName = value);
                _command.StatedLastName.MatchSome(value => contact.dfeta_StatedLastName = value);
                _command.StatedMiddleName.MatchSome(value => contact.dfeta_StatedMiddleName = value);
            }

            // make sure teacher doesn't already have an identity.
            if (string.IsNullOrEmpty(tspersonid))
            {
                _command.EmailAddress.MatchSome(value => contact.EMailAddress1 = value);
            }

            return contact;
        }

        public Models.Task CreateReviewTaskEntityForActiveSanctions()
        {
            var description = GetDescription();

            return new Models.Task()
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

        public Models.Task CreateReviewTaskEntityForMultipleQualifications()
        {
            var description = GetDescription();

            return new Models.Task()
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

        public Models.Task CreateNoMatchIttReviewTask()
        {
            var description = GetDescription();

            return new Models.Task()
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

        public Models.Task CreateMultipleMatchIttReviewTask()
        {
            var description = GetDescription();

            return new Models.Task()
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
                sb.Append($"Multiple ITT UKPRNs found for TRN {Trn}, SlugId {SlugId}");
                return sb.ToString();
            }
        }

        public dfeta_initialteachertraining CreateInitialTeacherTrainingEntity(UpdateTeacherReferenceLookupResult referenceData, Guid? id, dfeta_initialteachertraining existingItt)
        {
            Debug.Assert(referenceData.IttProviderId.HasValue);
            Debug.Assert(referenceData.IttSubject1Id.HasValue);

            var cohortYear = _command.InitialTeacherTraining.ProgrammeEndDate?.Year.ToString();
            var result = (_command.InitialTeacherTraining.Result ?? //update result to request
                existingItt?.dfeta_Result ??                        //keep existing result
                (_command.InitialTeacherTraining.ProgrammeType == dfeta_ITTProgrammeType.AssessmentOnlyRoute ? dfeta_ITTResult.UnderAssessment : dfeta_ITTResult.InTraining)); //create new itt record

            if (_command.InitialTeacherTraining.ProgrammeType == dfeta_ITTProgrammeType.AssessmentOnlyRoute && _command.InitialTeacherTraining.Result == dfeta_ITTResult.InTraining)
            {
                throw new ArgumentException("InTraining not permitted for AsessmentOnlyRoute", nameof(_command.InitialTeacherTraining));
            }
            if (_command.InitialTeacherTraining.ProgrammeType != dfeta_ITTProgrammeType.AssessmentOnlyRoute && _command.InitialTeacherTraining.Result == dfeta_ITTResult.UnderAssessment)
            {
                throw new ArgumentException("UnderAsessment only permitted for AsessmentOnlyRoute", nameof(_command.InitialTeacherTraining));
            }

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
                dfeta_Result = result,
                dfeta_TraineeID = _command.HusId.ValueOrDefault(),
                dfeta_ITTQualificationId = referenceData.IttQualificationId?.ToEntityReference(dfeta_ittqualification.EntityLogicalName),
                dfeta_ittqualificationaim = _command.InitialTeacherTraining.IttQualificationAim
            };

            if (referenceData.IttCountryId is Guid countryId)
            {
                entity.dfeta_CountryId = countryId.ToEntityReference(dfeta_country.EntityLogicalName);
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

            if (referenceData.QualificationCountryId == null && !string.IsNullOrEmpty(_command.Qualification?.CountryCode))
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
                    Contact.Fields.StateCode,
                    Contact.Fields.dfeta_AllowPiiUpdatesFromRegister,
                    Contact.Fields.dfeta_TSPersonID
                });

            var getIttRecordsTask = _dataverseAdapter.GetInitialTeacherTrainingByTeacher(
                TeacherId,
                columnNames: new[]
                {
                    dfeta_initialteachertraining.Fields.dfeta_ProgrammeType,
                    dfeta_initialteachertraining.Fields.dfeta_Result,
                    dfeta_initialteachertraining.Fields.dfeta_EstablishmentId,
                    dfeta_initialteachertraining.Fields.StateCode,
                    dfeta_initialteachertraining.Fields.dfeta_SlugId
                });

            var getIttRecordsBySlugIdTask = !string.IsNullOrEmpty(_command.SlugId.ValueOrDefault()) ?
                _dataverseAdapter.GetInitialTeacherTrainingBySlugId(_command.SlugId.ValueOrDefault(), columnNames: new[]
                {
                    dfeta_initialteachertraining.Fields.dfeta_ProgrammeType,
                    dfeta_initialteachertraining.Fields.dfeta_Result,
                    dfeta_initialteachertraining.Fields.dfeta_EstablishmentId,
                    dfeta_initialteachertraining.Fields.StateCode,
                    dfeta_initialteachertraining.Fields.dfeta_SlugId
                }, null, true)
                :
                Task.FromResult(Array.Empty<dfeta_initialteachertraining>());

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

            var getHavePendingPiiChanges = _dataverseAdapter.DoesTeacherHavePendingPIIChanges(TeacherId);

            var isEarlyYears = _command.InitialTeacherTraining.ProgrammeType.IsEarlyYears();

            var requestBuilder = _dataverseAdapter.CreateMultipleRequestBuilder();

            var getAllEytsTeacherStatusesTask = _dataverseAdapter.GetAllEarlyYearsStatuses(requestBuilder);
            var getAllTeacherStatuses = _dataverseAdapter.GetAllTeacherStatuses(requestBuilder);

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

            var existingTeachersWithHusIdTask = !string.IsNullOrEmpty(_command.HusId.ValueOrDefault()) ? _dataverseAdapter.GetTeachersByHusId(_command.HusId.ValueOrDefault(), columnNames: new[]
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
                    getQualificationProviderTask,
                    getTeacherTask,
                    getIttRecordsTask,
                    getQtsRegistrationsTask,
                    getQualifications,
                    getQualificationSubject2Task,
                    getQualificationSubject3Task,
                    existingTeachersWithHusIdTask,
                    getAllEytsTeacherStatusesTask,
                    getAllTeacherStatuses,
                    getIttRecordsBySlugIdTask,
                    getHavePendingPiiChanges
                }
                .Where(t => t != null);

            await requestBuilder.Execute();
            await Task.WhenAll(lookupTasks);

            var getEarlyYearsTraineeStatusId = getAllEytsTeacherStatusesTask.Result.Single(x => x.dfeta_Value == "220");
            var getAorCandidateTeacherStatusId = getAllTeacherStatuses.Result.Single(x => x.dfeta_Value == "212");
            var getTraineeTeacherDmsTeacherStatusId = getAllTeacherStatuses.Result.Single(x => x.dfeta_Value == "211");

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
                QualificationProviderId = getQualificationProviderTask?.Result?.Id,
                Teacher = getTeacherTask.Result,
                Itt = getIttRecordsBySlugIdTask?.Result.Length > 0 ? getIttRecordsBySlugIdTask.Result : getIttRecordsTask.Result,
                Qualifications = getQualifications.Result,
                TeacherHasActiveSanctions = getTeacherTask.Result?.dfeta_ActiveSanctions == true,
                TeacherHusId = getTeacherTask.Result?.dfeta_HUSID,
                HaveExistingTeacherWithHusId = existingTeachersWithHusIdTask?.Result != null && existingTeachersWithHusIdTask.Result.Count(x => x.Id != _command.TeacherId) > 0,
                QtsRegistrations = getQtsRegistrationsTask?.Result,
                EarlyYearsTraineeStatusId = getEarlyYearsTraineeStatusId.Id,
                AorCandidateTeacherStatusId = getAorCandidateTeacherStatusId.Id,
                TraineeTeacherDmsTeacherStatusId = getTraineeTeacherDmsTeacherStatusId.Id,
                HavePendingPiiChanges = getHavePendingPiiChanges.Result
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
        public Contact Teacher { get; set; }
        public IEnumerable<dfeta_initialteachertraining> Itt { get; set; }
        public IEnumerable<dfeta_qualification> Qualifications { get; set; }
        public bool TeacherHasActiveSanctions { get; set; }
        public string TeacherHusId { get; set; }
        public Guid? QualificationSubject2Id { get; set; }
        public Guid? QualificationSubject3Id { get; set; }
        public bool HaveExistingTeacherWithHusId { get; set; }
        public IEnumerable<dfeta_qtsregistration> QtsRegistrations { get; set; }

        public Guid EarlyYearsTraineeStatusId { get; set; }
        public Guid AorCandidateTeacherStatusId { get; set; }
        public Guid TraineeTeacherDmsTeacherStatusId { get; set; }
        public bool HavePendingPiiChanges { get; set; }
    }
}
