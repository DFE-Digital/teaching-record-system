using FakeXrmEasy.Extensions;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Optional;
using Optional.Unsafe;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Services.TrnRequests;
using static TeachingRecordSystem.Core.Dqt.RequestBuilder;
using SystemUser = TeachingRecordSystem.Core.DataStore.Postgres.Models.SystemUser;

namespace TeachingRecordSystem.TestCommon;

public partial class TestData
{
    public Task<CreatePersonResult> CreatePersonAsync(Action<CreatePersonBuilder>? configure = null)
    {
        var builder = new CreatePersonBuilder();
        configure?.Invoke(builder);
        return builder.ExecuteAsync(this);
    }

    public class CreatePersonBuilder
    {
        private const string TeacherStatusQualifiedTeacherTrained = "71";

        private static readonly DateOnly _defaultQtsDate = new DateOnly(2022, 9, 1);

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
        private readonly List<QtsRegistration> _qtsRegistrations = new();
        private readonly List<CreatePersonAlertBuilder> _alertBuilders = [];
        private readonly List<CreatePersonMandatoryQualificationBuilder> _mqBuilders = [];
        private readonly List<CreatePersonProfessionalStatusBuilder> _professionalStatusBuilders = [];
        private readonly List<ProfessionalStatusType> _awardedProfessionalStatuses = [];
        private readonly List<(string FirstName, string MiddleName, string LastName, DateTime Created)> _previousNames = [];
        private DateOnly? _qtlsDate;
        private (Guid ApplicationUserId, string RequestId, bool WriteMetadata, bool? IdentityVerified, string? OneLoginUserSubject, bool? PotentialDuplicate)? _trnRequest;
        private string? _trnToken;
        private string? _slugId;
        private int? _loginFailedCounter;
        private CreatePersonInductionBuilder? _inductionBuilder;

        public Guid PersonId { get; } = Guid.NewGuid();

        public CreatePersonBuilder WithSyncOverride(bool enabled)
        {
            _syncEnabledOverride = enabled;
            return this;
        }

        public CreatePersonBuilder WithDateOfBirth(DateOnly dateOfBirth)
        {
            _dateOfBirth = dateOfBirth;
            return this;
        }

        public CreatePersonBuilder WithFirstName(string firstName)
        {
            _firstName = firstName;
            return this;
        }

        public CreatePersonBuilder WithMiddleName(string? middleName)
        {
            _middleName = middleName;
            return this;
        }

        public CreatePersonBuilder WithLastName(string lastName)
        {
            _lastName = lastName;
            return this;
        }

        public CreatePersonBuilder WithEmail(string? email)
        {
            _email = email;
            return this;
        }

        public CreatePersonBuilder WithMobileNumber(string? mobileNumber)
        {
            _mobileNumber = mobileNumber;
            return this;
        }

        public CreatePersonBuilder WithAlert(Action<CreatePersonAlertBuilder>? configure = null)
        {
            EnsureTrn();

            var alertBuilder = new CreatePersonAlertBuilder();
            configure?.Invoke(alertBuilder);
            _alertBuilders.Add(alertBuilder);

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
            EnsureTrn();

            _qualifications.Add(new(qualificationId ?? Guid.NewGuid(), type, completionOrAwardDate, isActive!.Value, heQualificationValue, heSubject1Value, heSubject2Value, heSubject3Value));

            return this;
        }

        public CreatePersonBuilder WithMandatoryQualification(Action<CreatePersonMandatoryQualificationBuilder>? configure = null)
        {
            EnsureTrn();

            var mqBuilder = new CreatePersonMandatoryQualificationBuilder();
            configure?.Invoke(mqBuilder);
            _mqBuilders.Add(mqBuilder);

            return this;
        }

        public CreatePersonBuilder WithRouteToProfessionalStatus(Action<CreatePersonProfessionalStatusBuilder>? configure = null)
        {
            EnsureTrn();

            var builder = new CreatePersonProfessionalStatusBuilder();
            configure?.Invoke(builder);
            _professionalStatusBuilders.Add(builder);

            return this;
        }

        public CreatePersonBuilder WithHoldsRouteToProfessionalStatus(ProfessionalStatusType professionalStatusType)
        {
            EnsureTrn();
            _awardedProfessionalStatuses.Add(professionalStatusType);
            return this;
        }

        public CreatePersonBuilder WithoutTrn()
        {
            if (_alertBuilders.Any() ||
                _mqBuilders.Any() ||
                _qtlsDate.HasValue ||
                _qtsRegistrations.Any() ||
                _qualifications.Any() ||
                _inductionBuilder?.HasStatusRequiringQts == true)
            {
                throw new InvalidOperationException("Person requires a TRN.");
            }

            _hasTrn = false;
            return this;
        }

        public CreatePersonBuilder WithTrn()
        {
            _hasTrn = true;
            return this;
        }

        public CreatePersonBuilder WithGender(Contact_GenderCode? gender)
        {
            _gender = gender;
            return this;
        }

        public CreatePersonBuilder WithNationalInsuranceNumber(bool hasNationalInsuranceNumber = true)
        {
            _hasNationalInsuranceNumber = hasNationalInsuranceNumber;

            if (_hasNationalInsuranceNumber is false)
            {
                _nationalInsuranceNumber = null;
            }

            return this;
        }

        public CreatePersonBuilder WithNationalInsuranceNumber(string nationalInsuranceNumber)
        {
            _hasNationalInsuranceNumber = true;
            _nationalInsuranceNumber = nationalInsuranceNumber;
            return this;
        }

        public CreatePersonBuilder WithQts(DateOnly? qtsDate = null)
        {
            EnsureTrn();

            _qtsRegistrations.Add(
                new QtsRegistration(
                    qtsDate ?? _defaultQtsDate,
                    TeacherStatusValue: TeacherStatusQualifiedTeacherTrained,
                    CreatedOn: null,
                    EytsDate: null,
                    EytsStatusValue: null));

            return this;
        }

        public CreatePersonBuilder WithQtlsDateInDqt(DateOnly? qtlsDate)
        {
            EnsureTrn();

            _qtlsDate = qtlsDate;

            return this;
        }

        public CreatePersonBuilder WithQtsRegistration(DateOnly? qtsDate, string? teacherStatusValue, DateTime? createdDate, DateOnly? eytsDate, string? eytsTeacherStatus)
        {
            EnsureTrn();

            _qtsRegistrations.Add(new QtsRegistration(qtsDate, teacherStatusValue, createdDate, eytsDate, eytsTeacherStatus));

            return this;
        }

        public CreatePersonBuilder WithEyts(DateOnly? eytsDate, string? eytsStatusValue, DateTime? createdDate = null)
        {
            EnsureTrn();

            _qtsRegistrations.Add(new QtsRegistration(null, null, createdDate, eytsDate, eytsStatusValue));

            return this;
        }

        public CreatePersonBuilder WithQtls(DateOnly holdsFrom) =>
            WithRouteToProfessionalStatus(p => p
                .WithStatus(RouteToProfessionalStatusStatus.Holds)
                .WithHoldsFrom(holdsFrom)
                .WithRouteType(RouteToProfessionalStatusType.QtlsAndSetMembershipId));

        public CreatePersonBuilder WithTrnRequest(
            Guid applicationUserId,
            string requestId,
            bool? identityVerified = null,
            string? oneLoginUserSubject = null,
            bool writeMetadata = true,
            bool? potentialDuplicate = null)
        {
            _trnRequest = (applicationUserId, requestId, writeMetadata, identityVerified, oneLoginUserSubject, potentialDuplicate);
            return this;
        }

        public CreatePersonBuilder WithTrnToken(string trnToken)
        {
            _trnToken = trnToken;
            return this;
        }

        public CreatePersonBuilder WithSlugId(string slugId)
        {
            _slugId = slugId;
            return this;
        }

        public CreatePersonBuilder WithLoginFailedCounter(int? loginFailedCounter)
        {
            if (_loginFailedCounter is not null && _loginFailedCounter != loginFailedCounter)
            {
                throw new InvalidOperationException("WithLoginFailedCounter cannot be changed after it's set.");
            }

            _loginFailedCounter = loginFailedCounter;
            return this;
        }

        public CreatePersonBuilder WithInductionStatus(InductionStatus status) =>
            WithInductionStatus(i =>
            {
                var qtsDate = GetQtsDate();
                var startDate = CreatePersonInductionBuilder.GetDefaultStartDate(status, qtsDate);
                var completedDate = CreatePersonInductionBuilder.GetDefaultCompletedDate(status, startDate);
                var exemptionReasons = CreatePersonInductionBuilder.GetDefaultExemptionReasonIds(status);

                if (!Person.ValidateInductionData(status, startDate, completedDate, exemptionReasons, out var error))
                {
                    throw new InvalidOperationException(error);
                }

                i
                    .WithStatus(status)
                    .WithStartDate(startDate)
                    .WithCompletedDate(completedDate)
                    .WithExemptionReasons(exemptionReasons);
            });

        public CreatePersonBuilder WithInductionStatus(Action<CreatePersonInductionBuilder> configure)
        {
            EnsureTrn();

            _inductionBuilder ??= new();
            configure(_inductionBuilder);

            return this;
        }

        public CreatePersonBuilder WithPreviousNames(params (string FirstName, string MiddleName, string LastName, DateTime Created)[] previousNames)
        {
            _previousNames.AddRange(previousNames);
            return this;
        }

        internal async Task<CreatePersonResult> ExecuteAsync(TestData testData)
        {
            var trn = _hasTrn == true ? await testData.GenerateTrnAsync() : null;
            var statedFirstName = _firstName ?? testData.GenerateFirstName();
            var statedMiddleName = _middleName ?? testData.GenerateMiddleName();
            var firstAndMiddleNames = $"{statedFirstName} {statedMiddleName}".Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var firstName = firstAndMiddleNames.First();
            var middleName = string.Join(" ", firstAndMiddleNames.Skip(1));
            var lastName = _lastName ?? testData.GenerateLastName();
            var dateOfBirth = _dateOfBirth ?? testData.GenerateDateOfBirth();
            var gender = _gender ?? testData.GenerateGender();

            var events = new List<EventBase>();

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
                GenderCode = gender,
                dfeta_qtlsdate = _qtlsDate.ToDateTimeWithDqtBstFix(isLocalTime: false),
                dfeta_TrnRequestID = _trnRequest is { } trnRequest ? TrnRequestService.GetCrmTrnRequestId(trnRequest.ApplicationUserId, trnRequest.RequestId) : null,
                dfeta_TrnToken = _trnToken,
                dfeta_SlugId = _slugId,
                dfeta_loginfailedcounter = _loginFailedCounter
            };

            // The conditional is to work around issue in CRM where an explicit `null` TRN breaks a plugin
            if (trn is not null)
            {
                contact.dfeta_TRN = trn;
            }

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

            if (trn is not null && _trnToken is null && _email is not null)
            {
                _trnToken = Guid.NewGuid().ToString();
            }

            var txnRequestBuilder = RequestBuilder.CreateTransaction(testData.OrganizationService);
            txnRequestBuilder.AddRequest(new CreateRequest() { Target = contact });

            IInnerRequestHandle<RetrieveResponse>? getQtsRegistationTask = null;
            var qts = _qtsRegistrations.Where(x => x.TeacherStatusValue != null && x.QtsDate != null);
            foreach (var item in qts)
            {
                var teacherStatus = await testData.ReferenceDataCache.GetTeacherStatusByValueAsync(item.TeacherStatusValue!);
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
                var earlyYearsStatus = await testData.ReferenceDataCache.GetEarlyYearsStatusByValueAsync(item.EytsStatusValue!);
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
                        var heQualification = await testData.ReferenceDataCache.GetHeQualificationByValueAsync(qualification.HeQualificationValue!);
                        crmQualification.dfeta_HE_HEQualificationId = heQualification.Id.ToEntityReference(dfeta_hequalification.EntityLogicalName);
                    }

                    if (qualification.HeSubject1Value is not null)
                    {
                        var heSubject1 = await testData.ReferenceDataCache.GetHeSubjectByValueAsync(qualification.HeSubject1Value!);
                        crmQualification.dfeta_HE_HESubject1Id = heSubject1.Id.ToEntityReference(dfeta_hesubject.EntityLogicalName);
                    }

                    if (qualification.HeSubject2Value is not null)
                    {
                        var heSubject2 = await testData.ReferenceDataCache.GetHeSubjectByValueAsync(qualification.HeSubject2Value!);
                        crmQualification.dfeta_HE_HESubject2Id = heSubject2.Id.ToEntityReference(dfeta_hesubject.EntityLogicalName);
                    }

                    if (qualification.HeSubject3Value is not null)
                    {
                        var heSubject3 = await testData.ReferenceDataCache.GetHeSubjectByValueAsync(qualification.HeSubject3Value!);
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

            var retrieveContactHandle = txnRequestBuilder.AddRequest<RetrieveResponse>(new RetrieveRequest()
            {
                ColumnSet = new(allColumns: true),
                Target = PersonId.ToEntityReference(Contact.EntityLogicalName)
            });

            await txnRequestBuilder.ExecuteAsync();

            // Read the contact record back (plugins may have added/amended data so our original record will be stale)
            contact = retrieveContactHandle.GetResponse().Entity.ToEntity<Contact>();

            var syncedPerson = await testData.SyncConfiguration.SyncIfEnabledAsync(
                helper => helper.SyncPersonAsync(contact, syncAudit: true, ignoreInvalid: false),
                _syncEnabledOverride);

            var (mqs, alerts, person, routes, previousNames) = await testData.WithDbContextAsync(async dbContext =>
            {
                if (!syncedPerson)
                {
                    if (_alertBuilders.Any() ||
                        _mqBuilders.Any() ||
                        _trnRequest is { WriteMetadata: true } ||
                        _inductionBuilder != null)
                    {
                        throw new InvalidOperationException("Cannot write TRS-owned data unless sync is enabled.");
                    }

                    return default;
                }

                var person = await dbContext.Persons
                    .Include(p => p.Qualifications)
                    .SingleAsync(p => p.PersonId == PersonId);

                AddTrnRequestMetadata();
                _inductionBuilder?.Execute(person, this, testData, dbContext);
                var mqIds = await AddMqsAsync();
                var alertIds = await AddAlertsAsync();
                var professionalStatusIds = await AddProfessionalStatusRoutesAsync(person);
                var awardedProfessionalStatusIds = await AddAwardedProfessionalStatusRoutesAsync();
                var previousNameIds = await AddPreviousNamesAsync();

                await dbContext.SaveChangesAsync();

                person = await dbContext.Persons
                    .Include(p => p.Alerts!)
                    .AsSplitQuery()
                    .Include(p => p.PreviousNames)
                    .AsSplitQuery()
                    .SingleAsync(p => p.PersonId == contact.Id);

                // Can't include this above https://github.com/dotnet/efcore/issues/7623
                var personMqs = await dbContext.MandatoryQualifications
                    .Where(q => q.PersonId == PersonId)
                    .ToArrayAsync();

                var personProfessionalStatuses = await dbContext.RouteToProfessionalStatuses
                    .Where(p => p.PersonId == PersonId)
                    .ToArrayAsync();

                // Get MQs, Alerts and Professional Statuses that we've added *in the same order they were specified*.
                var mqs = mqIds.Select(id => personMqs.Single(q => q.QualificationId == id)).AsReadOnly();
                var alerts = alertIds.Select(id => person.Alerts!.Single(a => a.AlertId == id)).AsReadOnly();
                var routesToProfessionalStatus = professionalStatusIds.Concat(awardedProfessionalStatusIds)
                    .Select(id => personProfessionalStatuses.Single(q => q.QualificationId == id))
                    .AsReadOnly();
                var previousNames = previousNameIds.Select(id => person.PreviousNames!.Single(a => a.PreviousNameId == id)).AsReadOnly();

                return (mqs, alerts, person, routesToProfessionalStatus, previousNames);

                async Task<IReadOnlyCollection<Guid>> AddMqsAsync()
                {
                    var mqIds = new List<Guid>();

                    foreach (var builder in _mqBuilders)
                    {
                        var (mqId, mqEvents) = await builder.ExecuteAsync(this, testData, dbContext);
                        mqIds.Add(mqId);
                        events.AddRange(mqEvents);
                    }

                    return mqIds;
                }

                async Task<IReadOnlyCollection<Guid>> AddProfessionalStatusRoutesAsync(Person person)
                {
                    var routeIds = new List<Guid>();

                    foreach (var builder in _professionalStatusBuilders)
                    {
                        var (routeId, createdEvents) = await builder.ExecuteAsync(this, person, testData, dbContext);
                        routeIds.Add(routeId);
                        events.AddRange(createdEvents);
                    }

                    return routeIds;
                }

                async Task<IReadOnlyCollection<Guid>> AddAwardedProfessionalStatusRoutesAsync()
                {
                    var allRoutes = await testData.ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync(activeOnly: false);
                    var allSubjects = await testData.ReferenceDataCache.GetTrainingSubjectsAsync();
                    var allCountries = await testData.ReferenceDataCache.GetTrainingCountriesAsync();
                    var allProviders = await testData.ReferenceDataCache.GetTrainingProvidersAsync();
                    var allDegreeTypes = await testData.ReferenceDataCache.GetDegreeTypesAsync();

                    var createdProfessionalStatusIds = new List<Guid>();

                    foreach (var professionalStatusType in _awardedProfessionalStatuses)
                    {
                        var route = allRoutes.Where(r => r.ProfessionalStatusType == professionalStatusType).RandomOne();

                        var professionalStatus = RouteToProfessionalStatus.Create(
                                person,
                                allRoutes,
                                route.RouteToProfessionalStatusTypeId,
                                RouteToProfessionalStatusStatus.Holds,
                                testData.GenerateDate(min: new(2022, 8, 1), max: new(2025, 1, 1)),
                                route.TrainingStartDateRequired is not FieldRequirement.NotApplicable ? new(2021, 10, 1) : null,
                                route.TrainingEndDateRequired is not FieldRequirement.NotApplicable ? new(2022, 7, 5) : null,
                                route.TrainingSubjectsRequired is not FieldRequirement.NotApplicable ?
                                new[] { allSubjects.RandomOne().TrainingSubjectId } :
                                [],
                                route.TrainingAgeSpecialismTypeRequired is not FieldRequirement.NotApplicable ? TrainingAgeSpecialismType.FoundationStage : null,
                                null,
                                null,
                                route.TrainingCountryRequired is not FieldRequirement.NotApplicable ? allCountries.RandomOne().CountryId : null,
                                allProviders.RandomOne().TrainingProviderId,
                                route.DegreeTypeRequired is not FieldRequirement.NotApplicable ? allDegreeTypes.RandomOne().DegreeTypeId : null,
                                route.InductionExemptionRequired is not FieldRequirement.NotApplicable ? false : null,
                                EventModels.RaisedByUserInfo.FromUserId(Core.DataStore.Postgres.Models.SystemUser.SystemUserId),
                                DateTime.UtcNow,
                                out var @createdEvent
                                );

                        dbContext.RouteToProfessionalStatuses.Add(professionalStatus);
                        dbContext.AddEventWithoutBroadcast(createdEvent);

                        createdProfessionalStatusIds.Add(professionalStatus.QualificationId);
                    }

                    return createdProfessionalStatusIds;
                }

                async Task<IReadOnlyCollection<Guid>> AddAlertsAsync()
                {
                    var alertIds = new List<Guid>();

                    foreach (var builder in _alertBuilders)
                    {
                        var (alertId, alertEvents) = await builder.ExecuteAsync(this, testData, dbContext);
                        alertIds.Add(alertId);
                        events.AddRange(alertEvents);
                    }

                    return alertIds;
                }

                async Task<IReadOnlyCollection<Guid>> AddPreviousNamesAsync()
                {
                    return await testData.WithDbContextAsync(async dbContext =>
                    {
                        var previousNameIds = new List<Guid>();
                        foreach (var pn in _previousNames)
                        {
                            var id = Guid.NewGuid();
                            var previousName = new PreviousName
                            {
                                PreviousNameId = id,
                                PersonId = PersonId,
                                FirstName = pn.FirstName,
                                MiddleName = pn.MiddleName ?? string.Empty,
                                LastName = pn.LastName,
                                CreatedOn = pn.Created,
                                UpdatedOn = pn.Created
                            };

                            previousNameIds.Add(id);
                            dbContext.PreviousNames.Add(previousName);
                        }

                        await dbContext.SaveChangesAsync();

                        return previousNameIds;
                    });
                }

                void AddTrnRequestMetadata()
                {
                    if (_trnRequest is not { WriteMetadata: true } trnRequest)
                    {
                        return;
                    }

                    dbContext.TrnRequestMetadata.Add(new TrnRequestMetadata()
                    {
                        ApplicationUserId = trnRequest.ApplicationUserId,
                        RequestId = trnRequest.RequestId,
                        CreatedOn = testData.Clock.UtcNow,
                        IdentityVerified = trnRequest.IdentityVerified,
                        OneLoginUserSubject = trnRequest.OneLoginUserSubject,
                        EmailAddress = _email,
                        Name = [firstName, lastName],
                        FirstName = firstName,
                        MiddleName = "",
                        LastName = lastName,
                        DateOfBirth = dateOfBirth,
                        TrnToken = _trnToken,
                        PotentialDuplicate = trnRequest.PotentialDuplicate ?? _hasTrn != true
                    });
                }
            });

            var currentDqtUser = await testData.GetCurrentCrmUserAsync();
            var auditId = Guid.NewGuid();
            var auditDetail = new AttributeAuditDetail()
            {
                AuditRecord = new Audit()
                {
                    Action = Audit_Action.Create,
                    AuditId = auditId,
                    CreatedOn = testData.Clock.UtcNow,
                    Id = auditId,
                    Operation = Audit_Operation.Create,
                    UserId = currentDqtUser
                },
                OldValue = new Entity(Contact.EntityLogicalName),
                NewValue = contact.Clone()
            };

            return new CreatePersonResult()
            {
                PersonId = PersonId,
                Person = person,
                Events = events.AsReadOnly(),
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
                MandatoryQualifications = mqs,
                Alerts = alerts,
                DqtContactAuditDetail = auditDetail,
                ProfessionalStatuses = routes,
                PreviousNames = previousNames
            };
        }

        internal DateOnly? GetQtsDate()
        {
            var qtsDates = _qtsRegistrations
                .Where(q => q.QtsDate != null)
                .Select(q => q.QtsDate!.Value)
                .ToArray();

            if (qtsDates.Length == 0)
            {
                return null;
            }

            return qtsDates.Min();
        }

        private void EnsureTrn()
        {
            _hasTrn ??= true;

            if (_hasTrn != true)
            {
                throw new InvalidOperationException("Person requires a TRN.");
            }
        }

        internal DateOnly EnsureQts() => GetQtsDate() ??
            throw new InvalidOperationException("Person requires QTS.");
    }

    public class CreatePersonAlertBuilder
    {
        private Option<Guid?> _alertTypeId;
        private Option<string?> _details;
        private Option<string?> _externalLink;
        private Option<DateOnly> _startDate;
        private Option<DateOnly?> _endDate;
        private Option<string?> _reason;
        private Option<string?> _reasonDetail;
        private Option<EventModels.RaisedByUserInfo> _createdByUser;
        private Option<DateTime?> _createdUtc;

        public CreatePersonAlertBuilder WithAlertTypeId(Guid? alertTypeId)
        {
            _alertTypeId = Option.Some(alertTypeId);
            return this;
        }

        public CreatePersonAlertBuilder WithDetails(string? details)
        {
            _details = Option.Some(details);
            return this;
        }

        public CreatePersonAlertBuilder WithExternalLink(string? externalLink)
        {
            _externalLink = Option.Some(externalLink);
            return this;
        }

        public CreatePersonAlertBuilder WithStartDate(DateOnly startDate)
        {
            _startDate = Option.Some(startDate);
            return this;
        }

        public CreatePersonAlertBuilder WithEndDate(DateOnly? endDate)
        {
            if (endDate.HasValue && !_startDate.HasValue)
            {
                throw new ArgumentException($"{nameof(endDate)} cannot be specified until {nameof(WithStartDate)} has been called with a non-null startDate.");
            }

            if (endDate.HasValue && endDate < _startDate.ValueOrDefault())
            {
                throw new ArgumentException($"{nameof(endDate)} must be after startDate specified in {nameof(WithStartDate)}.");
            }

            _endDate = Option.Some(endDate);
            return this;
        }

        public CreatePersonAlertBuilder WithAddReason(string? reason, string? reasonDetail)
        {
            _reason = Option.Some(reason);
            _reasonDetail = Option.Some(reasonDetail);
            return this;
        }

        public CreatePersonAlertBuilder WithCreatedUtc(DateTime? createdUtc)
        {
            _createdUtc = Option.Some(createdUtc);
            return this;
        }

        public CreatePersonAlertBuilder WithCreatedByUser(EventModels.RaisedByUserInfo user)
        {
            _createdByUser = Option.Some(user);
            return this;
        }

        internal async Task<(Guid AlertId, IReadOnlyCollection<EventBase> Events)> ExecuteAsync(
            CreatePersonBuilder createPersonBuilder,
            TestData testData,
            TrsDbContext dbContext)
        {
            var personId = createPersonBuilder.PersonId;

            if (_alertTypeId.HasValue && !(await testData.ReferenceDataCache.GetAlertTypesAsync()).Any(a => a.AlertTypeId == _alertTypeId.ValueOrDefault()))
            {
                throw new ArgumentException("AlertTypeId is invalid.");
            }

            var alertTypeId = _alertTypeId.ValueOr((await testData.ReferenceDataCache.GetAlertTypesAsync()).RandomOne().AlertTypeId);
            var details = _details.ValueOr(testData.GenerateLoremIpsum());
            var externalLink = _externalLink.ValueOr((string?)null);
            var startDate = _startDate.ValueOr(testData.GenerateDate(min: new DateOnly(2000, 1, 1)));
            var endDate = _endDate.ValueOr((DateOnly?)null);
            var reason = _reason.ValueOr("Another reason");
            var reasonDetail = _reasonDetail.ValueOr(testData.GenerateLoremIpsum());
            var createdByUser = _createdByUser.ValueOr(EventModels.RaisedByUserInfo.FromUserId(Core.DataStore.Postgres.Models.SystemUser.SystemUserId));
            var createdUtc = _createdUtc.ValueOr(testData.Clock.UtcNow);

            var alert = Alert.Create(
                alertTypeId!.Value,
                personId,
                details,
                externalLink,
                startDate,
                endDate,
                reason,
                reasonDetail,
                evidenceFile: null,
                createdByUser,
                createdUtc!.Value,
                out var @createdEvent);

            dbContext.Alerts.Add(alert);
            dbContext.AddEventWithoutBroadcast(createdEvent);

            return (alert.AlertId, [createdEvent]);
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

        internal async Task<(Guid QualificationId, IReadOnlyCollection<EventBase> Events)> ExecuteAsync(
            CreatePersonBuilder createPersonBuilder,
            TestData testData,
            TrsDbContext dbContext)
        {
            var personId = createPersonBuilder.PersonId;

            var providerId = _mandatoryQualificationProviderId.ValueOr(MandatoryQualificationProvider.All.RandomOne().MandatoryQualificationProviderId);
            var specialism = _specialism.ValueOr(MandatoryQualificationSpecialismRegistry.GetAll(includeLegacy: true).RandomOne().Value);
            var status = _status.ValueOr(_endDate.ValueOrDefault() is DateOnly ? MandatoryQualificationStatus.Passed : MandatoryQualificationStatusRegistry.All.RandomOne().Value);
            var startDate = _startDate.ValueOr(testData.GenerateDate(min: new DateOnly(2000, 1, 1)));
            var endDate = _endDate.ValueOr(status == MandatoryQualificationStatus.Passed ? testData.GenerateDate(min: (startDate ?? new DateOnly(2000, 1, 1)).AddYears(1)) : null);
            var createdUtc = _createdUtc.ValueOr(testData.Clock.UtcNow);

            var provider = providerId.HasValue ?
                await dbContext.MandatoryQualificationProviders.SingleAsync(p => p.MandatoryQualificationProviderId == providerId) :
                null;

            var mq = new MandatoryQualification()
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
            };

            dbContext.MandatoryQualifications.Add(mq);

            var events = new List<EventBase>();

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

                dbContext.AddEventWithoutBroadcast(createdEvent);
                events.Add(createdEvent);
            }
            else
            {
                var createdByUser = _createdByUser.ValueOr(EventModels.RaisedByUserInfo.FromUserId(SystemUser.SystemUserId));

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

                dbContext.AddEventWithoutBroadcast(createdEvent);
                events.Add(createdEvent);
            }

            return (QualificationId, events);
        }
    }

    public class CreatePersonInductionBuilder
    {
        private Option<InductionStatus> _status;
        private Option<DateOnly?> _startDate;
        private Option<DateOnly?> _completedDate;
        private Option<Guid[]> _exemptionReasonIds;

        public bool HasStatusRequiringQts => _status.HasValue && _status.ValueOrFailure() != InductionStatus.None;

        public CreatePersonInductionBuilder WithStatus(InductionStatus status)
        {
            if (_status.HasValue && _status.ValueOrFailure() != status)
            {
                throw new InvalidOperationException("Status has already been set.");
            }

            _status = Option.Some(status);
            return this;
        }

        public CreatePersonInductionBuilder WithStartDate(DateOnly? startDate)
        {
            if (_startDate.HasValue)
            {
                throw new InvalidOperationException("Start date has already been set.");
            }

            _startDate = Option.Some(startDate);
            return this;
        }

        public CreatePersonInductionBuilder WithCompletedDate(DateOnly? completedDate)
        {
            if (_completedDate.HasValue)
            {
                throw new InvalidOperationException("Completed date has already been set.");
            }

            _completedDate = Option.Some(completedDate);
            return this;
        }

        public CreatePersonInductionBuilder WithExemptionReasons(params Guid[] exemptionReasonIds)
        {
            if (_exemptionReasonIds.HasValue)
            {
                throw new InvalidOperationException("Exemption reasons have already been set.");
            }

            _exemptionReasonIds = Option.Some(exemptionReasonIds);
            return this;
        }

        internal IReadOnlyCollection<EventBase> Execute(
            Person person,
            CreatePersonBuilder createPersonBuilder,
            TestData testData,
            TrsDbContext dbContext)
        {
            var qtsDate = createPersonBuilder.GetQtsDate();

            var status = _status.ValueOr(qtsDate.HasValue ? InductionStatus.RequiredToComplete : InductionStatus.None);
            var startDate = _startDate.ValueOrDefault();
            var completedDate = _completedDate.ValueOrDefault();
            var exemptionReasons = _exemptionReasonIds.ValueOr([]);

            person.SetInductionStatus(
                status,
                startDate,
                completedDate,
                exemptionReasons,
                changeReason: null,
                changeReasonDetail: null,
                evidenceFile: null,
                updatedBy: SystemUser.SystemUserId,
                testData.Clock.UtcNow,
                out var @event);

            if (@event is not null)
            {
                dbContext.AddEventWithoutBroadcast(@event);
                return [@event];
            }

            return [];
        }

        internal static DateOnly? GetDefaultStartDate(InductionStatus status, DateOnly? qtsDate) =>
            status.RequiresStartDate() ? qtsDate!.Value.AddMonths(6) : null;

        internal static DateOnly? GetDefaultCompletedDate(InductionStatus status, DateOnly? startDate) =>
            status.RequiresCompletedDate() ? startDate!.Value.AddMonths(12) : null;

        internal static Guid[] GetDefaultExemptionReasonIds(InductionStatus status) =>
            status is InductionStatus.Exempt ? new[] { InductionExemptionReason.PassedInWalesId } : Array.Empty<Guid>();
    }

    public record CreatePersonResult
    {
        public required Guid PersonId { get; init; }
        public required Person Person { get; init; }
        public Guid ContactId => PersonId;
        public required IReadOnlyCollection<EventBase> Events { get; init; }
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
        public required IReadOnlyCollection<MandatoryQualification> MandatoryQualifications { get; init; }
        public required IReadOnlyCollection<Alert> Alerts { get; init; }
        public required IReadOnlyCollection<RouteToProfessionalStatus> ProfessionalStatuses { get; init; }
        public required IReadOnlyCollection<PreviousName> PreviousNames { get; init; }
        public required AuditDetail? DqtContactAuditDetail { get; init; }
    }

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
