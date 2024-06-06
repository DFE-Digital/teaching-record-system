using Microsoft.Xrm.Sdk.Messages;
using Optional;
using Optional.Unsafe;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;
using static TeachingRecordSystem.Core.Dqt.RequestBuilder;

namespace TeachingRecordSystem.TestCommon;

public partial class TestData
{
    public Task<CreatePersonResult> CreatePerson(Action<CreatePersonBuilder>? configure = null)
    {
        var builder = new CreatePersonBuilder();
        configure?.Invoke(builder);
        return builder.Execute(this);
    }

    public class CreatePersonBuilder
    {
        private const string TeacherStatusQualifiedTeacherTrained = "71";

        private bool? _syncEnabledOverride;
        private DateOnly? _dateOfBirth;
        private bool? _hasTrn;
        private string? _firstName;
        private string? _middleName;
        private string? _lastName;
        private string? _email;
        private string? _mobileNumber;
        private Contact_GenderCode? _gender;
        private bool? _hasNationalInsuranceNumber;
        private string? _nationalInsuranceNumber;
        private readonly List<Qualification> _qualifications = new();
        private readonly List<MandatoryQualificationInfo> _mandatoryQualifications = new();
        private readonly List<QtsRegistration> _qtsRegistrations = new();
        private readonly List<Sanction> _sanctions = [];
        private readonly List<CreatePersonMandatoryQualificationBuilder> _mqBuilders = [];
        private DateOnly? _qtlsDate;
        private readonly List<Induction> _inductions = [];
        private readonly List<InductionPeriod> _inductionPeriods = [];

        public Guid PersonId { get; } = Guid.NewGuid();

        public CreatePersonBuilder WithSyncOverride(bool enabled)
        {
            _syncEnabledOverride = enabled;
            return this;
        }

        public CreatePersonBuilder WithDateOfBirth(DateOnly dateOfBirth)
        {
            if (_dateOfBirth is not null && _dateOfBirth != dateOfBirth)
            {
                throw new InvalidOperationException("WithDateOfBirth cannot be changed after it's set.");
            }

            _dateOfBirth = dateOfBirth;
            return this;
        }

        public CreatePersonBuilder WithFirstName(string firstName)
        {
            if (_firstName is not null && _firstName != firstName)
            {
                throw new InvalidOperationException("WithFirstName cannot be changed after it's set.");
            }

            _firstName = firstName;
            return this;
        }

        public CreatePersonBuilder WithMiddleName(string middleName)
        {
            if (_middleName is not null && _middleName != middleName)
            {
                throw new InvalidOperationException("WithMiddleName cannot be changed after it's set.");
            }

            _middleName = middleName;
            return this;
        }

        public CreatePersonBuilder WithLastName(string lastName)
        {
            if (_lastName is not null && _lastName != lastName)
            {
                throw new InvalidOperationException("WithLastName cannot be changed after it's set.");
            }

            _lastName = lastName;
            return this;
        }

        public CreatePersonBuilder WithEmail(string email)
        {
            if (_email is not null && _email != email)
            {
                throw new InvalidOperationException("WithEmail cannot be changed after it's set.");
            }

            _email = email;
            return this;
        }

        public CreatePersonBuilder WithInduction(dfeta_InductionStatus inductionStatus, dfeta_InductionExemptionReason? inductionExemptionReason, DateOnly? inductionStartDate, DateOnly? completedDate, DateOnly? inductionPeriodStartDate, DateOnly? inductionPeriodEndDate, Guid? appropriateBodyOrgId)
        {
            var inductionId = Guid.NewGuid();
            if (inductionStatus == dfeta_InductionStatus.Exempt && inductionExemptionReason == null)
            {
                throw new InvalidOperationException("WithInduction must provide InductionExemptionReason if InductionStatus is Exempt");
            }
            _inductions.Add(new Induction(inductionId, inductionStatus, inductionExemptionReason, inductionStartDate, completedDate));

            //inductionPeriod is optional
            if (!appropriateBodyOrgId.HasValue && inductionPeriodStartDate.HasValue || !appropriateBodyOrgId.HasValue && inductionPeriodEndDate.HasValue)
            {
                throw new InvalidOperationException("WithInductionPeriod must be associated with an appropriate body");
            }
            if (appropriateBodyOrgId.HasValue)
            {
                _inductionPeriods.Add(new InductionPeriod(inductionId, inductionPeriodStartDate, inductionPeriodEndDate, appropriateBodyOrgId!.Value));
            }
            return this;
        }

        public CreatePersonBuilder WithMobileNumber(string mobileNumber)
        {
            if (_mobileNumber is not null && _mobileNumber != mobileNumber)
            {
                throw new InvalidOperationException("WithMobileNumber cannot be changed after it's set.");
            }

            _mobileNumber = mobileNumber;
            return this;
        }

        public CreatePersonBuilder WithSanction(
            string sanctionCode,
            DateOnly? startDate = null,
            DateOnly? endDate = null,
            DateOnly? reviewDate = null,
            bool spent = false,
            string? details = null,
            string? detailsLink = null,
            bool isActive = true)
        {
            _sanctions.Add(new(Guid.NewGuid(), sanctionCode, startDate, endDate, reviewDate, spent, details, detailsLink, isActive));
            return this;
        }

        public CreatePersonBuilder WithQualification(
            Guid? qualificationId,
            dfeta_qualification_dfeta_Type type,
            DateOnly? completionOrAwardDate = null,
            bool? isActive = true,
            string? heQualificationValue = null,
            string? heSubject1Value = null,
            string? heSubject2Value = null,
            string? heSubject3Value = null)
        {
            _qualifications.Add(new(qualificationId ?? Guid.NewGuid(), type, completionOrAwardDate, isActive!.Value, heQualificationValue, heSubject1Value, heSubject2Value, heSubject3Value));
            return this;
        }

        public CreatePersonBuilder WithMandatoryQualification(Action<CreatePersonMandatoryQualificationBuilder>? configure = null)
        {
            var mqBuilder = new CreatePersonMandatoryQualificationBuilder();
            configure?.Invoke(mqBuilder);
            _mqBuilders.Add(mqBuilder);
            return this;
        }

        public CreatePersonBuilder WithTrn(bool hasTrn = true)
        {
            if (_hasTrn is not null && _hasTrn != hasTrn)
            {
                throw new InvalidOperationException("WithTrn cannot be changed after it's set.");
            }

            _hasTrn = hasTrn;
            return this;
        }

        public CreatePersonBuilder WithGender(Contact_GenderCode gender)
        {
            if (_gender is not null && _gender != gender)
            {
                throw new InvalidOperationException("WithGender cannot be changed after it's set.");
            }

            _gender = gender;
            return this;
        }

        public CreatePersonBuilder WithNationalInsuranceNumber(bool? hasNationalInsuranceNumber = true, string? nationalInsuranceNumber = null)
        {
            if ((_hasNationalInsuranceNumber is not null && _hasNationalInsuranceNumber != hasNationalInsuranceNumber)
                || (_nationalInsuranceNumber is not null && _nationalInsuranceNumber != nationalInsuranceNumber))
            {
                throw new InvalidOperationException("WithNationalInsuranceNumber cannot be changed after it's set.");
            }

            _hasNationalInsuranceNumber = hasNationalInsuranceNumber;
            _nationalInsuranceNumber = nationalInsuranceNumber;
            return this;
        }

        public CreatePersonBuilder WithNationalInsuranceNumber(string nationalInsuranceNumber)
        {
            var hasNationalInsuranceNumber = true;

            if ((_hasNationalInsuranceNumber is not null && _hasNationalInsuranceNumber != hasNationalInsuranceNumber)
                || (_nationalInsuranceNumber is not null && _nationalInsuranceNumber != nationalInsuranceNumber))
            {
                throw new InvalidOperationException("WithNationalInsuranceNumber cannot be changed after it's set.");
            }

            _hasNationalInsuranceNumber = hasNationalInsuranceNumber;
            _nationalInsuranceNumber = nationalInsuranceNumber;
            return this;
        }

        public CreatePersonBuilder WithQts(DateOnly? qtsDate = null)
        {
            _qtsRegistrations.Add(
                new QtsRegistration(
                    qtsDate ?? new DateOnly(2022, 9, 1),
                    TeacherStatusValue: TeacherStatusQualifiedTeacherTrained,
                    CreatedOn: null,
                    EytsDate: null,
                    EytsStatusValue: null));

            return this;
        }

        public CreatePersonBuilder WithQtlsDate(DateOnly? qtlsDate)
        {
            if (_qtlsDate is not null && _qtlsDate != qtlsDate)
            {
                throw new InvalidOperationException("WithQtlsDate cannot be changed after it's set.");
            }
            _qtlsDate = qtlsDate;
            return this;
        }

        public CreatePersonBuilder WithQtsRegistration(DateOnly? qtsDate, string? teacherStatusValue, DateTime? createdDate, DateOnly? eytsDate, string? eytsTeacherStatus)
        {
            _qtsRegistrations.Add(new QtsRegistration(qtsDate, teacherStatusValue, createdDate, eytsDate, eytsTeacherStatus));
            return this;
        }

        public CreatePersonBuilder WithEyts(DateOnly? eytsDate, string? eytsStatusValue, DateTime? createdDate)
        {
            _qtsRegistrations.Add(new QtsRegistration(null, null, createdDate, eytsDate, eytsStatusValue));
            return this;
        }

        internal async Task<CreatePersonResult> Execute(TestData testData)
        {
            var hasTrn = _hasTrn ?? true;
            var trn = hasTrn ? await testData.GenerateTrn() : null;
            var statedFirstName = _firstName ?? testData.GenerateFirstName();
            var statedMiddleName = _middleName ?? testData.GenerateMiddleName();
            var firstAndMiddleNames = $"{statedFirstName} {statedMiddleName}".Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var firstName = firstAndMiddleNames.First();
            var middleName = string.Join(" ", firstAndMiddleNames.Skip(1));
            var lastName = _lastName ?? testData.GenerateLastName();
            var dateOfBirth = _dateOfBirth ?? testData.GenerateDateOfBirth();
            var gender = _gender ?? testData.GenerateGender();

            var contact = new Contact()
            {
                Id = PersonId,
                FirstName = firstName,
                MiddleName = middleName,
                LastName = lastName,
                dfeta_StatedFirstName = statedFirstName,
                dfeta_StatedMiddleName = statedMiddleName,
                dfeta_StatedLastName = lastName,
                BirthDate = dateOfBirth.ToDateTime(new TimeOnly()),
                dfeta_TRN = trn,
                GenderCode = gender,
                dfeta_qtlsdate = _qtlsDate.ToDateTimeWithDqtBstFix(isLocalTime: false)
            };

            if (_email is not null)
            {
                contact.EMailAddress1 = _email;
            }

            if (_mobileNumber is not null)
            {
                contact.MobilePhone = _mobileNumber;
            }

            if (_hasNationalInsuranceNumber ?? false)
            {
                contact.dfeta_NINumber = _nationalInsuranceNumber ?? testData.GenerateNationalInsuranceNumber();
            }

            if (_qtlsDate is not null)
            {
                contact.dfeta_qtlsdate = _qtlsDate.ToDateTimeWithDqtBstFix(isLocalTime: false);
            }

            var txnRequestBuilder = RequestBuilder.CreateTransaction(testData.OrganizationService);
            txnRequestBuilder.AddRequest(new CreateRequest() { Target = contact });

            IInnerRequestHandle<RetrieveResponse>? getQtsRegistationTask = null;
            var qts = _qtsRegistrations.Where(x => x.TeacherStatusValue != null && x.QtsDate != null);
            foreach (var item in qts)
            {
                var teacherStatus = await testData.ReferenceDataCache.GetTeacherStatusByValue(item.TeacherStatusValue!);
                var qtsRegistrationId = Guid.NewGuid();
                txnRequestBuilder.AddRequest(new CreateRequest()
                {
                    Target = new dfeta_qtsregistration()
                    {
                        Id = qtsRegistrationId,
                        dfeta_PersonId = PersonId.ToEntityReference(Contact.EntityLogicalName),
                        CreatedOn = item.CreatedOn
                    }
                });

                txnRequestBuilder.AddRequest(new CreateRequest()
                {
                    Target = new dfeta_initialteachertraining()
                    {
                        dfeta_PersonId = PersonId.ToEntityReference(Contact.EntityLogicalName),
                        dfeta_Result = dfeta_ITTResult.Pass,
                    }
                });

                getQtsRegistationTask = txnRequestBuilder.AddRequest<RetrieveResponse>(new RetrieveRequest()
                {
                    ColumnSet = new Microsoft.Xrm.Sdk.Query.ColumnSet(new[] { dfeta_qtsregistration.Fields.dfeta_QTSDate, dfeta_qtsregistration.Fields.dfeta_EYTSDate }),
                    Target = qtsRegistrationId.ToEntityReference(dfeta_qtsregistration.EntityLogicalName),
                });

                // Plugin which updates Contact with QTS Date only fires on Update or Delete
                txnRequestBuilder.AddRequest(new UpdateRequest()
                {
                    Target = new dfeta_qtsregistration()
                    {
                        Id = qtsRegistrationId,
                        dfeta_QTSDate = item.QtsDate!.Value.ToDateTimeWithDqtBstFix(isLocalTime: true),
                        dfeta_TeacherStatusId = teacherStatus.Id.ToEntityReference(dfeta_teacherstatus.EntityLogicalName),
                        CreatedOn = item.CreatedOn
                    }
                });

                getQtsRegistationTask = txnRequestBuilder.AddRequest<RetrieveResponse>(new RetrieveRequest()
                {
                    ColumnSet = new Microsoft.Xrm.Sdk.Query.ColumnSet(new[] { dfeta_qtsregistration.Fields.dfeta_QTSDate, dfeta_qtsregistration.Fields.dfeta_EYTSDate }),
                    Target = qtsRegistrationId.ToEntityReference(dfeta_qtsregistration.EntityLogicalName),
                });
            }

            var eyts = _qtsRegistrations.Where(x => x.EytsStatusValue != null && x.EytsDate != null);
            IInnerRequestHandle<RetrieveResponse>? getEytsRegistationTask = null;
            foreach (var item in eyts)
            {
                var eytsRegistrationId = Guid.NewGuid();
                var earlyYearsStatus = await testData.ReferenceDataCache.GetEarlyYearsStatusByValue(item.EytsStatusValue!);
                txnRequestBuilder.AddRequest(new CreateRequest()
                {
                    Target = new dfeta_qtsregistration()
                    {
                        Id = eytsRegistrationId,
                        dfeta_PersonId = PersonId.ToEntityReference(Contact.EntityLogicalName),
                        CreatedOn = item.CreatedOn
                    }
                });

                getEytsRegistationTask = txnRequestBuilder.AddRequest<RetrieveResponse>(new RetrieveRequest()
                {
                    ColumnSet = new Microsoft.Xrm.Sdk.Query.ColumnSet(new[] { dfeta_qtsregistration.Fields.dfeta_QTSDate, dfeta_qtsregistration.Fields.dfeta_EYTSDate }),
                    Target = eytsRegistrationId.ToEntityReference(dfeta_qtsregistration.EntityLogicalName)
                });

                // Plugin which updates Contact with EYTS Date only fires on Update or Delete
                txnRequestBuilder.AddRequest(new UpdateRequest()
                {
                    Target = new dfeta_qtsregistration()
                    {
                        Id = eytsRegistrationId,
                        dfeta_EYTSDate = item.EytsDate!.Value.ToDateTimeWithDqtBstFix(isLocalTime: true),
                        dfeta_EarlyYearsStatusId = earlyYearsStatus.Id.ToEntityReference(dfeta_earlyyearsstatus.EntityLogicalName),
                        CreatedOn = item.CreatedOn
                    }
                });

                getEytsRegistationTask = txnRequestBuilder.AddRequest<RetrieveResponse>(new RetrieveRequest()
                {
                    ColumnSet = new Microsoft.Xrm.Sdk.Query.ColumnSet(new[] { dfeta_qtsregistration.Fields.dfeta_QTSDate, dfeta_qtsregistration.Fields.dfeta_EYTSDate }),
                    Target = eytsRegistrationId.ToEntityReference(dfeta_qtsregistration.EntityLogicalName)
                });
            }

            foreach (var qualification in _qualifications)
            {
                var crmQualification = new dfeta_qualification()
                {
                    Id = qualification.QualificationId,
                    dfeta_PersonId = PersonId.ToEntityReference(Contact.EntityLogicalName),
                    dfeta_Type = qualification.Type,
                    dfeta_CompletionorAwardDate = qualification.CompletionOrAwardDate?.ToDateTimeWithDqtBstFix(isLocalTime: true)
                };

                if (qualification.Type == dfeta_qualification_dfeta_Type.HigherEducation)
                {
                    if (qualification.HeQualificationValue is not null)
                    {
                        var heQualification = await testData.ReferenceDataCache.GetHeQualificationByValue(qualification.HeQualificationValue!);
                        crmQualification.dfeta_HE_HEQualificationId = heQualification.Id.ToEntityReference(dfeta_hequalification.EntityLogicalName);
                    }

                    if (qualification.HeSubject1Value is not null)
                    {
                        var heSubject1 = await testData.ReferenceDataCache.GetHeSubjectByValue(qualification.HeSubject1Value!);
                        crmQualification.dfeta_HE_HESubject1Id = heSubject1.Id.ToEntityReference(dfeta_hesubject.EntityLogicalName);
                    }

                    if (qualification.HeSubject2Value is not null)
                    {
                        var heSubject2 = await testData.ReferenceDataCache.GetHeSubjectByValue(qualification.HeSubject2Value!);
                        crmQualification.dfeta_HE_HESubject2Id = heSubject2.Id.ToEntityReference(dfeta_hesubject.EntityLogicalName);
                    }

                    if (qualification.HeSubject3Value is not null)
                    {
                        var heSubject3 = await testData.ReferenceDataCache.GetHeSubjectByValue(qualification.HeSubject3Value!);
                        crmQualification.dfeta_HE_HESubject3Id = heSubject3.Id.ToEntityReference(dfeta_hesubject.EntityLogicalName);
                    }
                }

                txnRequestBuilder.AddRequest(new CreateRequest()
                {
                    Target = crmQualification
                });

                if (!qualification.IsActive)
                {
                    txnRequestBuilder.AddRequest(new UpdateRequest()
                    {
                        Target = new dfeta_qualification()
                        {
                            Id = qualification.QualificationId,
                            StateCode = dfeta_qualificationState.Inactive
                        }
                    });
                }
            }

            foreach (var induction in _inductions)
            {
                txnRequestBuilder.AddRequest(new CreateRequest()
                {
                    Target = new dfeta_induction()
                    {
                        Id = induction.InductionId,
                        dfeta_PersonId = PersonId.ToEntityReference(Contact.EntityLogicalName),
                        dfeta_InductionStatus = induction.inductionStatus,
                        dfeta_InductionExemptionReason = induction.inductionExemptionReason,
                        dfeta_StartDate = induction.StartDate.ToDateTimeWithDqtBstFix(isLocalTime: false),
                        dfeta_CompletionDate = induction.CompletetionDate.ToDateTimeWithDqtBstFix(isLocalTime: false)
                    }
                });
            }

            foreach (var inductionperiod in _inductionPeriods)
            {
                var induction = _inductions.First();
                txnRequestBuilder.AddRequest(new CreateRequest()
                {
                    Target = new dfeta_inductionperiod()
                    {
                        Id = Guid.NewGuid(),
                        dfeta_InductionId = inductionperiod!.InductionId.ToEntityReference(dfeta_induction.EntityLogicalName),
                        dfeta_StartDate = inductionperiod.startDate.ToDateTimeWithDqtBstFix(isLocalTime: false),
                        dfeta_EndDate = inductionperiod.endDate.ToDateTimeWithDqtBstFix(isLocalTime: false),
                        dfeta_AppropriateBodyId = inductionperiod.AppropriateBodyOrgId.ToEntityReference(Core.Dqt.Models.Account.EntityLogicalName)
                    }
                });
            }

            foreach (var sanction in _sanctions)
            {
                var sanctionCode = await testData.ReferenceDataCache.GetSanctionCodeByValue(sanction.SanctionCode);
                var crmSanction = new dfeta_sanction()
                {
                    Id = sanction.SanctionId,
                    dfeta_PersonId = PersonId.ToEntityReference(Contact.EntityLogicalName),
                    dfeta_SanctionCodeId = sanctionCode.Id.ToEntityReference(dfeta_sanctioncode.EntityLogicalName),
                    dfeta_StartDate = sanction.StartDate?.ToDateTimeWithDqtBstFix(isLocalTime: true),
                    dfeta_EndDate = sanction.EndDate?.ToDateTimeWithDqtBstFix(isLocalTime: true),
                    dfeta_NoReAppuntildate = sanction.ReviewDate?.ToDateTimeWithDqtBstFix(isLocalTime: true),
                    dfeta_Spent = sanction.Spent,
                    dfeta_SanctionDetails = sanction.Details
                };

                if (!string.IsNullOrWhiteSpace(sanction.DetailsLink))
                {
                    crmSanction.dfeta_DetailsLink = sanction.DetailsLink;
                }

                txnRequestBuilder.AddRequest(new CreateRequest()
                {
                    Target = crmSanction
                });

                if (!sanction.IsActive)
                {
                    txnRequestBuilder.AddRequest(new UpdateRequest()
                    {
                        Target = new dfeta_sanction()
                        {
                            Id = sanction.SanctionId,
                            StateCode = dfeta_sanctionState.Inactive
                        }
                    });
                }
            }

            var retrieveContactHandle = txnRequestBuilder.AddRequest<RetrieveResponse>(new RetrieveRequest()
            {
                ColumnSet = new(allColumns: true),
                Target = PersonId.ToEntityReference(Contact.EntityLogicalName)
            });

            await txnRequestBuilder.Execute();

            // Read the contact record back (plugins may have added/amended data so our original record will be stale)
            contact = retrieveContactHandle.GetResponse().Entity.ToEntity<Contact>();

            await testData.SyncConfiguration.SyncIfEnabled(
                helper => helper.SyncPerson(contact, ignoreInvalid: false, CancellationToken.None),
                _syncEnabledOverride);

            var mqs = await Task.WhenAll(_mqBuilders.Select(mqb => mqb.Execute(this, testData)));

            return new CreatePersonResult()
            {
                PersonId = PersonId,
                Contact = contact,
                Trn = trn,
                DateOfBirth = dateOfBirth,
                FirstName = firstName,
                MiddleName = middleName,
                LastName = lastName,
                StatedFirstName = statedFirstName,
                StatedMiddleName = statedMiddleName,
                StatedLastName = lastName,
                Email = _email,
                MobileNumber = _mobileNumber,
                Gender = gender.ToString(),
                NationalInsuranceNumber = contact.dfeta_NINumber,
                QtsDate = getQtsRegistationTask != null ? getQtsRegistationTask.GetResponse().Entity.ToEntity<dfeta_qtsregistration>().dfeta_QTSDate.ToDateOnlyWithDqtBstFix(true) : null,
                EytsDate = getEytsRegistationTask != null ? getEytsRegistationTask.GetResponse().Entity.ToEntity<dfeta_qtsregistration>().dfeta_EYTSDate.ToDateOnlyWithDqtBstFix(true) : null,
                Sanctions = [.. _sanctions],
                MandatoryQualifications = mqs,
                Inductions = [.. _inductions],
                InductionPeriods = [.. _inductionPeriods]
            };
        }
    }

    public class CreatePersonMandatoryQualificationBuilder
    {
        private Option<Guid?> _mandatoryQualificationProviderId;
        private Option<Guid?> _mqEstablishmentId;
        private Option<MandatoryQualificationSpecialism?> _specialism;
        private Option<Guid?> _dqtSpecialismId;
        private Option<MandatoryQualificationStatus?> _status;
        private Option<DateOnly?> _startDate;
        private Option<DateOnly?> _endDate;
        private Option<DateTime?> _createdUtc;
        private Option<EventModels.RaisedByUserInfo> _createdByUser;
        private Option<EventModels.RaisedByUserInfo> _importedByUser;

        public Guid QualificationId { get; } = Guid.NewGuid();

        public CreatePersonMandatoryQualificationBuilder WithProvider(Guid? mandatoryQualificationProviderId)
        {
            _mandatoryQualificationProviderId = Option.Some(mandatoryQualificationProviderId);
            _mqEstablishmentId = default;
            return this;
        }

        public CreatePersonMandatoryQualificationBuilder WithDqtMqEstablishment(dfeta_mqestablishment? mqEstablishment)
        {
            Guid? mandatoryQualificationProviderId = null;

            if (mqEstablishment is not null)
            {
                MandatoryQualificationProvider.TryMapFromDqtMqEstablishment(mqEstablishment, out var provider);
                mandatoryQualificationProviderId = provider?.MandatoryQualificationProviderId;
            }

            return WithDqtMqEstablishment(mqEstablishment, mandatoryQualificationProviderId);
        }

        public CreatePersonMandatoryQualificationBuilder WithDqtMqEstablishment(dfeta_mqestablishment? mqEstablishment, Guid? mandatoryQualificationProviderId)
        {
            _mqEstablishmentId = Option.Some(mqEstablishment?.Id);
            _mandatoryQualificationProviderId = Option.Some(mandatoryQualificationProviderId);
            return this;
        }

        public CreatePersonMandatoryQualificationBuilder WithSpecialism(MandatoryQualificationSpecialism? specialism, Guid? dqtSpecialismId = null)
        {
            _specialism = Option.Some(specialism);
            _dqtSpecialismId = Option.Some(dqtSpecialismId);
            return this;
        }

        public CreatePersonMandatoryQualificationBuilder WithStatus(MandatoryQualificationStatus? status, DateOnly? endDate = null)
        {
            if (endDate.HasValue)
            {
                if (status != MandatoryQualificationStatus.Passed)
                {
                    throw new ArgumentException($"{nameof(_endDate)} cannot be specified unless {nameof(status)} is '{MandatoryQualificationStatus.Passed}'.");
                }

                _endDate = Option.Some(endDate);
            }

            _status = Option.Some(status);
            return this;
        }

        public CreatePersonMandatoryQualificationBuilder WithStartDate(DateOnly? startDate)
        {
            _startDate = Option.Some(startDate);
            return this;
        }

        public CreatePersonMandatoryQualificationBuilder WithCreatedUtc(DateTime? createdUtc)
        {
            _createdUtc = Option.Some(createdUtc);
            return this;
        }

        public CreatePersonMandatoryQualificationBuilder WithCreatedByUser(EventModels.RaisedByUserInfo user)
        {
            if (_importedByUser.HasValue)
            {
                throw new InvalidOperationException("Cannot define both an 'imported by' and 'created by' user.");
            }

            _createdByUser = Option.Some(user);
            return this;
        }

        public CreatePersonMandatoryQualificationBuilder WithImportedByDqtUser(EventModels.RaisedByUserInfo user)
        {
            if (!user.IsDqtUser)
            {
                throw new ArgumentException("User must be a DQT user.", nameof(user));
            }

            if (_createdByUser.HasValue)
            {
                throw new InvalidOperationException("Cannot define both an 'imported by' and 'created by' user.");
            }

            _importedByUser = Option.Some(user);
            return this;
        }

        internal async Task<MandatoryQualificationInfo> Execute(CreatePersonBuilder createPersonBuilder, TestData testData)
        {
            var personId = createPersonBuilder.PersonId;

            var providerId = _mandatoryQualificationProviderId.ValueOr(MandatoryQualificationProvider.All.RandomOne().MandatoryQualificationProviderId);
            var specialism = _specialism.ValueOr(MandatoryQualificationSpecialismRegistry.GetAll(includeLegacy: true).RandomOne().Value);
            var status = _status.ValueOr(_endDate.ValueOrDefault() is DateOnly ? MandatoryQualificationStatus.Passed : MandatoryQualificationStatusRegistry.All.RandomOne().Value);
            var startDate = _startDate.ValueOr(testData.GenerateDate(min: new DateOnly(2000, 1, 1)));
            var endDate = _endDate.ValueOr(status == MandatoryQualificationStatus.Passed ? testData.GenerateDate(min: (startDate ?? new DateOnly(2000, 1, 1)).AddYears(1)) : null);
            var createdUtc = _createdUtc.ValueOr(testData.Clock.UtcNow);

            await testData.WithDbContext(async dbContext =>
            {
                var provider = providerId.HasValue ?
                    await dbContext.MandatoryQualificationProviders.SingleAsync(p => p.MandatoryQualificationProviderId == providerId) :
                    null;

                dbContext.MandatoryQualifications.Add(new MandatoryQualification()
                {
                    QualificationId = QualificationId,
                    CreatedOn = testData.Clock.UtcNow,
                    UpdatedOn = testData.Clock.UtcNow,
                    PersonId = personId,
                    ProviderId = providerId,
                    Status = status,
                    Specialism = specialism,
                    StartDate = startDate,
                    EndDate = endDate,
                    DqtSpecialismId = _dqtSpecialismId.ValueOr((Guid?)null),
                    DqtMqEstablishmentId = _mqEstablishmentId.ValueOr((Guid?)null)
                });

                if (_importedByUser.HasValue)
                {
                    var createdEvent = new MandatoryQualificationDqtImportedEvent()
                    {
                        EventId = Guid.NewGuid(),
                        CreatedUtc = createdUtc!.Value,
                        RaisedBy = _importedByUser.ValueOrFailure(),
                        PersonId = personId,
                        MandatoryQualification = new()
                        {
                            QualificationId = QualificationId,
                            Provider = provider is not null ?
                                new()
                                {
                                    MandatoryQualificationProviderId = provider.MandatoryQualificationProviderId,
                                    Name = provider.Name,
                                } :
                                null,
                            Specialism = specialism,
                            Status = status,
                            StartDate = startDate,
                            EndDate = endDate
                        },
                        DqtState = 0
                    };
                    dbContext.AddEvent(createdEvent);
                }
                else
                {
                    var createdByUser = _createdByUser.ValueOr(EventModels.RaisedByUserInfo.FromUserId(Core.DataStore.Postgres.Models.SystemUser.SystemUserId));

                    var createdEvent = new MandatoryQualificationCreatedEvent()
                    {
                        EventId = Guid.NewGuid(),
                        CreatedUtc = createdUtc!.Value,
                        RaisedBy = createdByUser,
                        PersonId = personId,
                        MandatoryQualification = new()
                        {
                            QualificationId = QualificationId,
                            Provider = provider is not null ?
                                new()
                                {
                                    MandatoryQualificationProviderId = provider.MandatoryQualificationProviderId,
                                    Name = provider.Name,
                                } :
                                null,
                            Specialism = specialism,
                            Status = status,
                            StartDate = startDate,
                            EndDate = endDate
                        }
                    };
                    dbContext.AddEvent(createdEvent);
                }

                await dbContext.SaveChangesAsync();
            });

            var mqInfo = new MandatoryQualificationInfo(
                QualificationId,
                providerId,
                specialism,
                status,
                startDate,
                endDate
            );

            return mqInfo;
        }
    }

    public record CreatePersonResult
    {
        public required Guid PersonId { get; init; }
        public Guid ContactId => PersonId;
        public required Contact Contact { get; init; }
        public required string? Trn { get; init; }
        public required DateOnly DateOfBirth { get; init; }
        public required string FirstName { get; init; }
        public required string MiddleName { get; init; }
        public required string LastName { get; init; }
        public required string StatedFirstName { get; init; }
        public required string StatedMiddleName { get; init; }
        public required string StatedLastName { get; init; }
        public required string? Email { get; init; }
        public required string? MobileNumber { get; init; }
        public required string Gender { get; init; }
        public required string? NationalInsuranceNumber { get; init; }
        public required DateOnly? QtsDate { get; init; }
        public required DateOnly? EytsDate { get; init; }
        public required IReadOnlyCollection<Sanction> Sanctions { get; init; }
        public required IReadOnlyCollection<MandatoryQualificationInfo> MandatoryQualifications { get; init; }
        public required IReadOnlyCollection<Induction> Inductions { get; init; }
        public required IReadOnlyCollection<InductionPeriod> InductionPeriods { get; init; }
    }

    public record Induction(Guid InductionId, dfeta_InductionStatus inductionStatus, dfeta_InductionExemptionReason? inductionExemptionReason, DateOnly? StartDate, DateOnly? CompletetionDate);
    public record InductionPeriod(Guid InductionId, DateOnly? startDate, DateOnly? endDate, Guid AppropriateBodyOrgId);

    public record Sanction(Guid SanctionId, string SanctionCode, DateOnly? StartDate, DateOnly? EndDate, DateOnly? ReviewDate, bool Spent, string? Details, string? DetailsLink, bool IsActive);

    public record MandatoryQualificationInfo(
        Guid QualificationId,
        Guid? ProviderId,
        MandatoryQualificationSpecialism? Specialism,
        MandatoryQualificationStatus? Status,
        DateOnly? StartDate,
        DateOnly? EndDate);

    public record QtsRegistration(DateOnly? QtsDate, string? TeacherStatusValue, DateTime? CreatedOn, DateOnly? EytsDate, string? EytsStatusValue);

    public record Qualification(
        Guid QualificationId,
        dfeta_qualification_dfeta_Type Type,
        DateOnly? CompletionOrAwardDate = null,
        bool IsActive = true,
        string? HeQualificationValue = null,
        string? HeSubject1Value = null,
        string? HeSubject2Value = null,
        string? HeSubject3Value = null);
}
