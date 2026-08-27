using Optional;
using Optional.Unsafe;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using SystemUser = TeachingRecordSystem.Core.DataStore.Postgres.Models.SystemUser;

namespace TeachingRecordSystem.TestCommon;

public partial class TestData
{
    public async Task<Person> CreatePersonAsync(Action<CreatePersonBuilder>? configure = null)
    {
        var referenceData = new CreatePersonBuilder.ReferenceData(
            await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync(),
            await ReferenceDataCache.GetDegreeTypesAsync(),
            (await ReferenceDataCache.GetTrainingProvidersAsync()).Where(x => !x.Name.Contains('\'')).AsReadOnly(),
            (await ReferenceDataCache.GetTrainingSubjectsAsync()).Where(x => !x.Name.Contains('\'')).AsReadOnly(),
            await ReferenceDataCache.GetTrainingCountriesAsync());

        var builder = new CreatePersonBuilder(referenceData);
        configure?.Invoke(builder);
        return await builder.ExecuteAsync(this, TimeProvider);
    }

    public class CreatePersonBuilder
    {
        private static readonly DateOnly _defaultQtsDate = new DateOnly(2022, 9, 1);
        private static readonly DateOnly _defaultEytsDate = new DateOnly(2023, 4, 13);

        private readonly ReferenceData _referenceData;
        private DateOnly? _dateOfBirth;
        private string? _firstName;
        private string? _middleName;
        private string? _lastName;
        private bool? _hasEmail;
        private string? _email;
        private bool? _hasNationalInsuranceNumber;
        private string? _nationalInsuranceNumber;
        private bool? _hasGender;
        private Gender? _gender;
        private readonly List<CreatePersonAlertBuilder> _alertBuilders = [];
        private readonly List<CreatePersonMandatoryQualificationBuilder> _mqBuilders = [];
        private readonly List<CreatePersonRouteToProfessionalStatusBuilder> _routeToProfessionalStatusBuilders = [];
        private readonly List<(string FirstName, string MiddleName, string LastName, DateTime Created)> _previousNames = [];
        private DateOnly? _qtlsDate;
        private (Guid ApplicationUserId, string RequestId, bool? IdentityVerified, string? OneLoginUserSubject, bool? PotentialDuplicate)? _trnRequest;
        private string? _trnToken;
        private string? _slugId;
        private int? _loginFailedCounter;
        private CreatePersonInductionBuilder? _inductionBuilder;
        private QtlsStatus? _qtlsStatus;
        private Guid? _mergedWithPersonId;
        private bool? _createdByTps;


        internal CreatePersonBuilder(ReferenceData referenceData)
        {
            _referenceData = referenceData;
        }

        public Guid PersonId { get; } = Guid.NewGuid();

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

        public CreatePersonBuilder WithEmailAddress(bool hasEmail = true)
        {
            _hasEmail = hasEmail;

            if (_hasEmail is false)
            {
                _email = null;
            }

            return this;
        }

        public CreatePersonBuilder WithEmailAddress(string? email)
        {
            _hasEmail = email != null;
            _email = email;
            return this;
        }

        public CreatePersonBuilder WithAlert(Action<CreatePersonAlertBuilder>? configure = null)
        {
            var alertBuilder = new CreatePersonAlertBuilder();
            configure?.Invoke(alertBuilder);
            _alertBuilders.Add(alertBuilder);

            return this;
        }

        public CreatePersonBuilder WithMandatoryQualification(Action<CreatePersonMandatoryQualificationBuilder>? configure = null)
        {
            var mqBuilder = new CreatePersonMandatoryQualificationBuilder();
            configure?.Invoke(mqBuilder);
            _mqBuilders.Add(mqBuilder);

            return this;
        }

        public CreatePersonBuilder WithRouteToProfessionalStatus(Action<CreatePersonRouteToProfessionalStatusBuilder> configure)
        {
            var builder = new CreatePersonRouteToProfessionalStatusBuilder();
            configure.Invoke(builder);
            _routeToProfessionalStatusBuilders.Add(builder);

            return this;
        }

        public CreatePersonBuilder WithHoldsRouteToProfessionalStatus(ProfessionalStatusType professionalStatusType) =>
            WithHoldsRouteToProfessionalStatus(professionalStatusType, _defaultQtsDate);

        public CreatePersonBuilder WithHoldsRouteToProfessionalStatus(
            ProfessionalStatusType professionalStatusType,
            DateOnly holdsFrom)
        {
            var routeType = _referenceData.RouteTypes
                .Where(r => r.ProfessionalStatusType == professionalStatusType)
                .SingleRandom();

            return WithHoldsRouteToProfessionalStatus(routeType.RouteToProfessionalStatusTypeId, holdsFrom);
        }

        public CreatePersonBuilder WithHoldsRouteToProfessionalStatus(
            Guid routeToProfessionalStatusTypeId,
            DateOnly holdsFrom)
        {
            var routeType = _referenceData.RouteTypes.Single(r => r.RouteToProfessionalStatusTypeId == routeToProfessionalStatusTypeId);

            return WithRouteToProfessionalStatus(b =>
            {
                b
                    .WithRouteType(routeToProfessionalStatusTypeId)
                    .WithStatus(RouteToProfessionalStatusStatus.Holds)
                    .WithHoldsFrom(holdsFrom);

                ConfigureUnlessNotApplicable(
                    routeType.TrainingStartDateRequired,
                    () => b.WithTrainingStartDate(new(2021, 10, 1)));

                ConfigureUnlessNotApplicable(
                    routeType.TrainingEndDateRequired,
                    () => b.WithTrainingEndDate(new(2022, 7, 5)));

                ConfigureUnlessNotApplicable(
                    routeType.TrainingSubjectsRequired,
                    () => b.WithTrainingSubjectIds([_referenceData.TrainingSubjects.SingleRandom().TrainingSubjectId]));

                ConfigureUnlessNotApplicable(
                    routeType.TrainingAgeSpecialismTypeRequired,
                    () => b.WithTrainingAgeSpecialismType(TrainingAgeSpecialismType.FoundationStage));

                ConfigureUnlessNotApplicable(
                    routeType.TrainingCountryRequired,
                    () => b.WithTrainingCountryId(_referenceData.Countries.SingleRandom().CountryId));

                ConfigureUnlessNotApplicable(
                    routeType.TrainingProviderRequired,
                    () => b.WithTrainingProviderId(_referenceData.TrainingProviders.SingleRandom().TrainingProviderId));

                ConfigureUnlessNotApplicable(
                    routeType.DegreeTypeRequired,
                    () => b.WithDegreeTypeId(_referenceData.DegreeTypes.SingleRandom().DegreeTypeId));

                ConfigureUnlessNotApplicable(
                    routeType.InductionExemptionRequired,
                    () => b.WithInductionExemption(false));

                void ConfigureUnlessNotApplicable(FieldRequirement requirement, System.Action configure)
                {
                    if (requirement is not FieldRequirement.NotApplicable)
                    {
                        configure();
                    }
                }
            });
        }

        public CreatePersonBuilder WithGender(bool hasGender = true)
        {
            _hasGender = hasGender;

            if (_hasGender is false)
            {
                _gender = null;
            }

            return this;
        }

        public CreatePersonBuilder WithGender(Gender gender)
        {
            _hasGender = true;
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

        public CreatePersonBuilder WithQts() => WithQts(_defaultQtsDate);

        public CreatePersonBuilder WithQts(DateOnly holdsFrom)
        {
            WithHoldsRouteToProfessionalStatus(
                routeToProfessionalStatusTypeId: new("4163C2FB-6163-409F-85FD-56E7C70A54DD"),
                holdsFrom);

            return this;
        }

        public CreatePersonBuilder WithQtls() => WithQtls(_defaultQtsDate);

        public CreatePersonBuilder WithQtls(DateOnly holdsFrom)
        {
            _qtlsDate = holdsFrom;

            WithRouteToProfessionalStatus(p => p
                .WithStatus(RouteToProfessionalStatusStatus.Holds)
                .WithHoldsFrom(holdsFrom)
                .WithRouteType(RouteToProfessionalStatusType.QtlsAndSetMembershipId));

            return this;
        }

        public CreatePersonBuilder WithQtlsStatus(QtlsStatus qtlsStatus)
        {
            if (qtlsStatus is QtlsStatus.Active)
            {
                return WithQtls();
            }

            _qtlsStatus = qtlsStatus;

            return this;
        }

        public CreatePersonBuilder WithEyts() => WithEyts(_defaultEytsDate);

        public CreatePersonBuilder WithEyts(DateOnly holdsFrom)
        {
            WithHoldsRouteToProfessionalStatus(ProfessionalStatusType.EarlyYearsTeacherStatus, holdsFrom);

            return this;
        }

        public CreatePersonBuilder WithTrnRequest(
            Guid applicationUserId,
            string requestId,
            bool? identityVerified = null,
            string? oneLoginUserSubject = null,
            bool? potentialDuplicate = null)
        {
            _trnRequest = (applicationUserId, requestId, identityVerified, oneLoginUserSubject, potentialDuplicate);
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
            _inductionBuilder ??= new();
            configure(_inductionBuilder);

            return this;
        }

        public CreatePersonBuilder WithPreviousNames(params (string FirstName, string MiddleName, string LastName, DateTime Created)[] previousNames)
        {
            _previousNames.AddRange(previousNames);
            return this;
        }

        public CreatePersonBuilder WithMergedWithPersonId(Guid mergedWithPersonId)
        {
            _mergedWithPersonId = mergedWithPersonId;
            return this;
        }

        public CreatePersonBuilder WithCreatedByTps(bool? createdByTps)
        {
            if (_createdByTps is not null && _createdByTps != createdByTps)
            {
                throw new InvalidOperationException("WithCreatedByTps cannot be changed after it's set.");
            }

            _createdByTps = createdByTps;

            return this;
        }

        internal async Task<Person> ExecuteAsync(TestData testData, TimeProvider clock)
        {
            var statedFirstName = _firstName ?? testData.GenerateFirstName();
            var statedMiddleName = _middleName ?? testData.GenerateMiddleName();
            var firstAndMiddleNames = $"{statedFirstName} {statedMiddleName}".Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var firstName = firstAndMiddleNames.First();
            var middleName = string.Join(" ", firstAndMiddleNames.Skip(1));
            var lastName = _lastName ?? testData.GenerateLastName();
            var dateOfBirth = _dateOfBirth ?? testData.GenerateDateOfBirth();
            var createdByTps = _createdByTps ?? false;
            var createdOn = clock.UtcNow;
            var updatedOn = clock.UtcNow;
            var personStatus = PersonStatus.Active;
            var gender = _gender ?? testData.GenerateGender();

            if (_trnToken is null && _email is not null)
            {
                _trnToken = Guid.NewGuid().ToString();
            }

            // The collection navigations are initialized so that the entities we add below are fixed up into them;
            // that way we don't have to query the person back to get a fully-populated record.
            var person = new Person()
            {
                PersonId = PersonId,
                CreatedOn = createdOn,
                UpdatedOn = updatedOn,
                Status = personStatus,
                FirstName = firstName,
                MiddleName = middleName,
                LastName = lastName,
                DateOfBirth = dateOfBirth,
                EmailAddress = _email,
                NationalInsuranceNumber = _nationalInsuranceNumber,
                Gender = _gender,
                CreatedByTps = createdByTps,
                MergedWithPersonId = _mergedWithPersonId,
                Qualifications = [],
                Alerts = [],
                PreviousNames = []
            };

            if (_hasNationalInsuranceNumber ?? false)
            {
                person.NationalInsuranceNumber = _nationalInsuranceNumber ?? testData.GenerateNationalInsuranceNumber();
            }

            if (_hasGender ?? false)
            {
                person.Gender = gender;
            }

            if (_hasEmail ?? false)
            {
                person.EmailAddress = _email ?? testData.GenerateUniqueEmail();
            }

            await testData.WithDbContextAsync(async dbContext =>
            {
                dbContext.Persons.Add(person);

                if (_qtlsStatus is QtlsStatus qtlsStatus)
                {
                    person.UnsafeSetQtlsStatus(qtlsStatus);
                }

                await TrackReferenceDataAsync();

                AddTrnRequestMetadata();
                await AddMqsAsync();
                await AddAlertsAsync();
                await AddProfessionalStatusRoutesAsync();
                _inductionBuilder?.Execute(person, testData, dbContext);
                AddPreviousNames();

                await dbContext.SaveChangesAsync();

                // The lookups the alerts and routes we're about to add point at are auto-included, so a query would
                // have returned them populated. Tracking them here gets EF to fix up those navigations for us.
                async Task TrackReferenceDataAsync()
                {
                    if (_alertBuilders.Count > 0)
                    {
                        await dbContext.AlertTypes.LoadAsync();
                    }

                    if (_routeToProfessionalStatusBuilders.Count > 0)
                    {
                        await dbContext.RouteToProfessionalStatusTypes.LoadAsync();
                        await dbContext.DegreeTypes.LoadAsync();
                        await dbContext.TrainingProviders.LoadAsync();
                        await dbContext.Countries.LoadAsync();
                    }
                }

                async Task AddMqsAsync()
                {
                    foreach (var builder in _mqBuilders)
                    {
                        await builder.ExecuteAsync(this, testData, dbContext);
                    }
                }

                async Task AddProfessionalStatusRoutesAsync()
                {
                    foreach (var builder in _routeToProfessionalStatusBuilders)
                    {
                        await builder.ExecuteAsync(this, person, testData, dbContext);
                    }
                }

                async Task AddAlertsAsync()
                {
                    foreach (var builder in _alertBuilders)
                    {
                        await builder.ExecuteAsync(this, testData, dbContext);
                    }
                }

                void AddPreviousNames()
                {
                    foreach (var pn in _previousNames)
                    {
                        var previousName = new PreviousName
                        {
                            PreviousNameId = Guid.NewGuid(),
                            PersonId = PersonId,
                            FirstName = pn.FirstName,
                            MiddleName = pn.MiddleName ?? string.Empty,
                            LastName = pn.LastName,
                            CreatedOn = pn.Created,
                            UpdatedOn = pn.Created
                        };

                        dbContext.PreviousNames.Add(previousName);
                    }
                }

                void AddTrnRequestMetadata()
                {
                    if (_trnRequest is not { } trnRequest)
                    {
                        return;
                    }

                    var trnRequestMetadata = new TrnRequestMetadata()
                    {
                        ApplicationUserId = trnRequest.ApplicationUserId,
                        RequestId = trnRequest.RequestId,
                        CreatedOn = testData.TimeProvider.UtcNow,
                        IdentityVerified = trnRequest.IdentityVerified,
                        OneLoginUserSubject = trnRequest.OneLoginUserSubject,
                        EmailAddress = _email,
                        Name = [firstName, lastName],
                        FirstName = firstName,
                        MiddleName = middleName,
                        LastName = lastName,
                        DateOfBirth = dateOfBirth,
                        NationalInsuranceNumber = _nationalInsuranceNumber,
                        TrnToken = _trnToken,
                        PotentialDuplicate = false
                    };
                    trnRequestMetadata.ResolvedPersonId = PersonId;
                    trnRequestMetadata.Status = TrnRequestStatus.Completed;

                    person.SourceApplicationUserId = trnRequestMetadata.ApplicationUserId;
                    person.SourceTrnRequestId = trnRequestMetadata.RequestId;

                    dbContext.TrnRequestMetadata.Add(trnRequestMetadata);
                }
            });

            return person;
        }

        internal DateOnly? GetQtsDate() =>
            _routeToProfessionalStatusBuilders
                .Where(r =>
                    r.Status == RouteToProfessionalStatusStatus.Holds &&
                    _referenceData.RouteTypes.Single(
                        t => t.RouteToProfessionalStatusTypeId == r.RouteToProfessionalStatusTypeId).ProfessionalStatusType == ProfessionalStatusType.QualifiedTeacherStatus)
                .OrderBy(r => r.HoldsFrom)
                .FirstOrDefault()
                ?.HoldsFrom;

        internal DateOnly EnsureQts() => GetQtsDate() ??
            throw new InvalidOperationException("Person requires QTS.");

        internal record ReferenceData(
            IReadOnlyCollection<RouteToProfessionalStatusType> RouteTypes,
            IReadOnlyCollection<DegreeType> DegreeTypes,
            IReadOnlyCollection<TrainingProvider> TrainingProviders,
            IReadOnlyCollection<TrainingSubject> TrainingSubjects,
            IReadOnlyCollection<Country> Countries);
    }

    public class CreatePersonAlertBuilder
    {
        private Option<Guid?> _alertTypeId;
        private Option<string?> _details;
        private Option<string?> _externalLink;
        private Option<DateOnly> _startDate;
        private Option<DateOnly?> _endDate;
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

        public CreatePersonAlertBuilder WithCreatedUtc(DateTime? createdUtc)
        {
            _createdUtc = Option.Some(createdUtc);
            return this;
        }

        internal async Task ExecuteAsync(
            CreatePersonBuilder createPersonBuilder,
            TestData testData,
            TrsDbContext dbContext)
        {
            var personId = createPersonBuilder.PersonId;

            var alertTypeId = _alertTypeId.ValueOr((await testData.ReferenceDataCache.GetAlertTypesAsync()).SingleRandom().AlertTypeId);
            var details = _details.ValueOr(testData.GenerateLoremIpsum());
            var externalLink = _externalLink.ValueOr((string?)null);
            var startDate = _startDate.ValueOr(testData.GenerateDate(min: new DateOnly(2000, 1, 1)));
            var endDate = _endDate.ValueOr((DateOnly?)null);

            var alert = new Alert
            {
                AlertId = Guid.NewGuid(),
                PersonId = personId,
                AlertTypeId = alertTypeId!.Value,
                Details = details,
                ExternalLink = externalLink,
                StartDate = startDate,
                EndDate = endDate,
                CreatedOn = _createdUtc.ValueOr(testData.TimeProvider.UtcNow)!.Value,
                UpdatedOn = _createdUtc.ValueOr(testData.TimeProvider.UtcNow)!.Value
            };

            dbContext.Alerts.Add(alert);
        }
    }

    public class CreatePersonMandatoryQualificationBuilder
    {
        private Option<Guid?> _mandatoryQualificationProviderId;
        private Option<string?> _mqEstablishmentValue;
        private Option<MandatoryQualificationSpecialism?> _specialism;
        private Option<string?> _dqtSpecialismValue;
        private Option<MandatoryQualificationStatus?> _status;
        private Option<DateOnly?> _startDate;
        private Option<DateOnly?> _endDate;
        private Option<DateTime?> _createdUtc;
        private Option<EventModels.RaisedByUserInfo> _importedByUser;

        public Guid QualificationId { get; } = Guid.NewGuid();

        public CreatePersonMandatoryQualificationBuilder WithProvider(Guid? mandatoryQualificationProviderId)
        {
            _mandatoryQualificationProviderId = Option.Some(mandatoryQualificationProviderId);
            _mqEstablishmentValue = default;
            return this;
        }

        public CreatePersonMandatoryQualificationBuilder WithDqtMqEstablishment(string? mqEstablishmentValue)
        {
            Guid? mandatoryQualificationProviderId = null;

            if (mqEstablishmentValue is not null)
            {
                MandatoryQualificationProvider.TryMapFromDqtMqEstablishmentValue(mqEstablishmentValue, out var provider);
                mandatoryQualificationProviderId = provider?.MandatoryQualificationProviderId;
            }

            return WithDqtMqEstablishment(mqEstablishmentValue, mandatoryQualificationProviderId);
        }

        public CreatePersonMandatoryQualificationBuilder WithDqtMqEstablishment(string? mqEstablishmentValue, Guid? mandatoryQualificationProviderId)
        {
            _mqEstablishmentValue = Option.Some(mqEstablishmentValue);
            _mandatoryQualificationProviderId = Option.Some(mandatoryQualificationProviderId);
            return this;
        }

        public CreatePersonMandatoryQualificationBuilder WithSpecialism(MandatoryQualificationSpecialism? specialism, string? dqtSpecialismValue = null)
        {
            _specialism = Option.Some(specialism);
            _dqtSpecialismValue = Option.Some(dqtSpecialismValue);
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

        public CreatePersonMandatoryQualificationBuilder WithImportedByDqtUser(EventModels.RaisedByUserInfo user)
        {
            if (!user.IsDqtUser)
            {
                throw new ArgumentException("User must be a DQT user.", nameof(user));
            }

            _importedByUser = Option.Some(user);
            return this;
        }

        internal async Task ExecuteAsync(
            CreatePersonBuilder createPersonBuilder,
            TestData testData,
            TrsDbContext dbContext)
        {
            var personId = createPersonBuilder.PersonId;

            var providerId = _mandatoryQualificationProviderId.ValueOr(MandatoryQualificationProvider.All.SingleRandom().MandatoryQualificationProviderId);
            var specialism = _specialism.ValueOr(MandatoryQualificationSpecialismRegistry.GetAll(includeLegacy: true).SingleRandom().Value);
            var status = _status.ValueOr(_endDate.ValueOrDefault() is DateOnly ? MandatoryQualificationStatus.Passed : MandatoryQualificationStatusRegistry.All.SingleRandom().Value);
            var startDate = _startDate.ValueOr(testData.GenerateDate(min: new DateOnly(2000, 1, 1)));
            var endDate = _endDate.ValueOr(status == MandatoryQualificationStatus.Passed ? testData.GenerateDate(min: (startDate ?? new DateOnly(2000, 1, 1)).AddYears(1)) : null);
            var createdUtc = _createdUtc.ValueOr(testData.TimeProvider.UtcNow);

            var provider = providerId.HasValue ?
                await dbContext.MandatoryQualificationProviders.SingleAsync(p => p.MandatoryQualificationProviderId == providerId) :
                null;

            var mq = new MandatoryQualification()
            {
                QualificationId = QualificationId,
                CreatedOn = testData.TimeProvider.UtcNow,
                UpdatedOn = testData.TimeProvider.UtcNow,
                PersonId = personId,
                ProviderId = providerId,
                Status = status,
                Specialism = specialism,
                StartDate = startDate,
                EndDate = endDate,
                DqtSpecialismValue = _dqtSpecialismValue.ValueOr((string?)null),
                DqtMqEstablishmentValue = _mqEstablishmentValue.ValueOr((string?)null)
            };

            dbContext.MandatoryQualifications.Add(mq);

            if (_importedByUser.HasValue)
            {
                var importedByUser = _importedByUser.ValueOrFailure();

                var importedEvent = new Core.Events.MandatoryQualificationDqtImportedEvent()
                {
                    EventId = Guid.NewGuid(),
                    PersonId = personId,
                    MandatoryQualification = new()
                    {
                        QualificationId = QualificationId,
                        Provider = provider is not null ?
                            new()
                            {
                                MandatoryQualificationProviderId = provider.MandatoryQualificationProviderId,
                                Name = provider.Name
                            } :
                            null,
                        Specialism = specialism,
                        Status = status,
                        StartDate = startDate,
                        EndDate = endDate
                    },
                    DqtState = 0
                };

                var processId = Guid.NewGuid();

                dbContext.Processes.Add(new Process
                {
                    ProcessId = processId,
                    ProcessType = ProcessType.MandatoryQualificationImportingIntoDqt,
                    CreatedOn = createdUtc!.Value,
                    UpdatedOn = createdUtc!.Value,
                    UserId = importedByUser.UserId,
                    DqtUserId = importedByUser.DqtUserId,
                    DqtUserName = importedByUser.DqtUserName,
                    PersonIds = [personId],
                    OneLoginUserSubjects = [],
                    SupportTaskReferences = [],
                    ChangeReason = null
                });

                dbContext.Set<ProcessEvent>().Add(new ProcessEvent
                {
                    ProcessEventId = importedEvent.EventId,
                    ProcessId = processId,
                    EventName = nameof(Core.Events.MandatoryQualificationDqtImportedEvent),
                    Payload = importedEvent,
                    PersonIds = [personId],
                    OneLoginUserSubjects = [],
                    SupportTaskReferences = [],
                    CreatedOn = createdUtc!.Value
                });
            }
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

        internal void Execute(
            Person person,
            TestData testData,
            TrsDbContext dbContext)
        {
            var status = _status.ValueOr(person.QtsDate.HasValue ? InductionStatus.RequiredToComplete : InductionStatus.None);
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
                testData.TimeProvider.UtcNow,
                additionalInformation: null,
                out var @event);

            if (@event is not null)
            {
                dbContext.AddEventWithoutBroadcast(@event);
            }
        }

        internal static DateOnly? GetDefaultStartDate(InductionStatus status, DateOnly? qtsDate) =>
            status.RequiresStartDate() ? qtsDate!.Value.AddMonths(6) : null;

        internal static DateOnly? GetDefaultCompletedDate(InductionStatus status, DateOnly? startDate) =>
            status.RequiresCompletedDate() ? startDate!.Value.AddMonths(12) : null;

        internal static Guid[] GetDefaultExemptionReasonIds(InductionStatus status) =>
            status is InductionStatus.Exempt ? new[] { InductionExemptionReason.PassedInWalesId } : [];
    }
}
