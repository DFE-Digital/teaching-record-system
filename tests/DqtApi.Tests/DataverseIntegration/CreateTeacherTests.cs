using System;
using DqtApi.DataStore.Crm;
using DqtApi.DataStore.Crm.Models;
using Microsoft.PowerPlatform.Dataverse.Client;
using Xunit;

namespace DqtApi.Tests.DataverseIntegration
{
    public class CreateTeacherTests : IClassFixture<CreateTeacherFixture>, IAsyncLifetime
    {
        private readonly CreateTeacherFixture _createTeacherFixture;
        private readonly TestableClock _clock;
        private readonly CrmClientFixture.TestDataScope _dataScope;
        private readonly DataverseAdapter _dataverseAdapter;
        private readonly IOrganizationServiceAsync _organizationService;

        public CreateTeacherTests(CreateTeacherFixture createTeacherFixture, CrmClientFixture crmClientFixture)
        {
            _createTeacherFixture = createTeacherFixture;
            _clock = crmClientFixture.Clock;
            _dataScope = crmClientFixture.CreateTestDataScope();
            _dataverseAdapter = _dataScope.CreateDataverseAdapter();
            _organizationService = _dataScope.OrganizationService;
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await _dataScope.DisposeAsync();

        [Theory]
        [InlineData(dfeta_ITTProgrammeType.AssessmentOnlyRoute)]
        [InlineData(dfeta_ITTProgrammeType.EYITTAssessmentOnly)]
        [InlineData(dfeta_ITTProgrammeType.LicensedTeacherProgramme)]
        public async Task Given_valid_request_creates_required_entities(dfeta_ITTProgrammeType programmeType)
        {
            // Arrange
            var command = CreateCommand(cmd =>
            {
                cmd.InitialTeacherTraining.ProgrammeType = programmeType;
            });

            // Act
            var (result, transactionRequest) = await _dataverseAdapter.CreateTeacherImpl(command);

            // Assert
            Assert.True(result.Succeeded);

            transactionRequest.AssertSingleCreateRequest<Contact>();
            transactionRequest.AssertSingleCreateRequest<dfeta_initialteachertraining>();
            transactionRequest.AssertSingleCreateRequest<dfeta_qualification>();
            transactionRequest.AssertSingleCreateRequest<dfeta_qtsregistration>();
        }

        [Fact]
        public async Task Given_minimal_details_request_succeeds()
        {
            // Arrange
            var command = new CreateTeacherCommand()
            {
                FirstName = "Minnie",
                LastName = "Ryder",
                BirthDate = new(1990, 5, 23),
                GenderCode = Contact_GenderCode.Female,
                InitialTeacherTraining = new()
                {
                    ProviderUkprn = "10044534",  // ARK Teacher Training
                    ProgrammeStartDate = new(2020, 4, 1),
                    ProgrammeEndDate = new(2020, 10, 10),
                    ProgrammeType = dfeta_ITTProgrammeType.GraduateTeacherProgramme,
                }
            };

            // Act
            var (result, transactionRequest) = await _dataverseAdapter.CreateTeacherImpl(command);

            // Assert
            Assert.True(result.Succeeded);
        }

        [Fact]
        public async Task Given_details_that_do_not_match_existing_records_allocates_trn_and_does_not_create_QTS_task()
        {
            // Arrange
            DataverseAdapter.FindExistingTeacher findExistingTeacher = () =>
                Task.FromResult<DataverseAdapter.CreateTeacherDuplicateTeacherResult>(null);

            var command = CreateCommand();

            // Act
            var (result, transactionRequest) = await _dataverseAdapter.CreateTeacherImpl(command, findExistingTeacher);

            // Assert
            Assert.True(result.Succeeded);
            Assert.NotNull(result.Trn);

            transactionRequest.AssertDoesNotContainCreateRequest<CrmTask>();
        }

        [Theory]
        [InlineData(false, false, false, "")]
        [InlineData(true, false, false, "Matched record has active sanctions\n")]
        [InlineData(false, true, false, "Matched record has QTS date\n")]
        [InlineData(false, false, true, "Matched record has EYTS date\n")]
        [InlineData(true, true, false, "Matched record has active sanctions & QTS date\n")]
        [InlineData(true, false, true, "Matched record has active sanctions & EYTS date\n")]
        public async Task Given_details_that_does_match_existing_record_does_not_allocate_trn_and_creates_QTS_task(
            bool hasActiveSanctions,
            bool hasQts,
            bool hasEyts,
            string expectedDescriptionSupplement)
        {
            // Arrange
            var firstName = _createTeacherFixture.ExistingTeacherFirstName;
            var middleName = _createTeacherFixture.ExistingTeacherFirstNameMiddleName;
            var lastName = _createTeacherFixture.ExistingTeacherFirstNameLastName;
            var birthDate = _createTeacherFixture.ExistingTeacherFirstNameBirthDate;
            var existingTeacherId = _createTeacherFixture.ExistingTeacherId;

            DataverseAdapter.FindExistingTeacher findExistingTeacher = () =>
                Task.FromResult(new DataverseAdapter.CreateTeacherDuplicateTeacherResult()
                {
                    TeacherId = existingTeacherId,
                    MatchedAttributes = new[] { Contact.Fields.FirstName, Contact.Fields.MiddleName, Contact.Fields.LastName, Contact.Fields.BirthDate },
                    HasActiveSanctions = hasActiveSanctions,
                    HasQtsDate = hasQts,
                    HasEytsDate = hasEyts
                });

            var command = CreateCommand(command =>
            {
                command.FirstName = firstName;
                command.MiddleName = middleName;
                command.LastName = lastName;
                command.BirthDate = birthDate;
            });

            // Act
            var (result, transactionRequest) = await _dataverseAdapter.CreateTeacherImpl(command, findExistingTeacher);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Null(result.Trn);

            var crmTask = transactionRequest.AssertSingleCreateRequest<CrmTask>();
            Assert.Equal(Contact.EntityLogicalName, crmTask.RegardingObjectId?.LogicalName);
            Assert.Equal(result.TeacherId, crmTask.RegardingObjectId?.Id);
            Assert.Equal(Contact.EntityLogicalName, crmTask.dfeta_potentialduplicateid?.LogicalName);
            Assert.Equal(existingTeacherId, crmTask.dfeta_potentialduplicateid?.Id);
            Assert.Equal("DMSImportTrn", crmTask.Category);
            Assert.Equal("Notification for QTS Unit Team", crmTask.Subject);
            Assert.Equal(_clock.UtcNow, crmTask.ScheduledEnd);

            var expectedDescription = $"Potential duplicate\nMatched on\n  - First name: '{firstName}'\n  - Middle name: '{middleName}'\n  - Last name: '{lastName}'\n  - Date of birth: '{birthDate:dd/MM/yyyy}'\n" +
                expectedDescriptionSupplement;

            Assert.Equal(
                expectedDescription,
                crmTask.Description,
                ignoreLineEndingDifferences: true);
        }

        [Fact]
        public async Task Given_itt_provider_ukprn_that_is_not_found_returns_failed()
        {
            // Arrange
            var command = CreateCommand(command => command.InitialTeacherTraining.ProviderUkprn = "badukprn");

            // Act
            var (result, _) = await _dataverseAdapter.CreateTeacherImpl(command);

            // Assert
            Assert.False(result.Succeeded);
            Assert.True(result.FailedReasons.HasFlag(CreateTeacherFailedReasons.IttProviderNotFound));
        }

        [Fact]
        public void CreateTeacherEntity_maps_contact_record_correctly()
        {
            // Arrange
            var command = CreateCommand();

            var helper = new DataverseAdapter.CreateTeacherHelper(_dataverseAdapter, command);

            // Act
            var entity = helper.CreateContactEntity();

            // Assert
            Assert.Equal(helper.TeacherId, entity.Id);
            Assert.Equal(command.FirstName, entity.FirstName);
            Assert.Equal(command.MiddleName, entity.MiddleName);
            Assert.Equal(command.LastName, entity.LastName);
            Assert.Equal(command.BirthDate, entity.BirthDate);
            Assert.Equal(command.EmailAddress, entity.EMailAddress1);
            Assert.Equal(command.GenderCode, entity.GenderCode);
            Assert.Equal(command.HusId, entity.dfeta_HUSID);
        }

        [Theory]
        [InlineData(dfeta_ITTProgrammeType.Apprenticeship, dfeta_ITTResult.InTraining)]
        [InlineData(dfeta_ITTProgrammeType.AssessmentOnlyRoute, dfeta_ITTResult.UnderAssessment)]
        [InlineData(dfeta_ITTProgrammeType.Core, dfeta_ITTResult.InTraining)]
        [InlineData(dfeta_ITTProgrammeType.CoreFlexible, dfeta_ITTResult.InTraining)]
        [InlineData(dfeta_ITTProgrammeType.EYITTAssessmentOnly, dfeta_ITTResult.InTraining)]
        [InlineData(dfeta_ITTProgrammeType.EYITTGraduateEmploymentBased, dfeta_ITTResult.InTraining)]
        [InlineData(dfeta_ITTProgrammeType.EYITTGraduateEntry, dfeta_ITTResult.InTraining)]
        [InlineData(dfeta_ITTProgrammeType.EYITTSchoolDirect_EarlyYears, dfeta_ITTResult.InTraining)]
        [InlineData(dfeta_ITTProgrammeType.EYITTUndergraduate, dfeta_ITTResult.InTraining)]
        [InlineData(dfeta_ITTProgrammeType.FutureTeachingScholars, dfeta_ITTResult.InTraining)]
        [InlineData(dfeta_ITTProgrammeType.GraduateTeacherProgramme, dfeta_ITTResult.InTraining)]
        [InlineData(dfeta_ITTProgrammeType.HEI, dfeta_ITTResult.InTraining)]
        [InlineData(dfeta_ITTProgrammeType.LicensedTeacherProgramme, dfeta_ITTResult.InTraining)]
        [InlineData(dfeta_ITTProgrammeType.OverseasTrainedTeacherProgramme, dfeta_ITTResult.InTraining)]
        [InlineData(dfeta_ITTProgrammeType.RegisteredTeacherProgramme, dfeta_ITTResult.InTraining)]
        [InlineData(dfeta_ITTProgrammeType.SchoolDirecttrainingprogramme, dfeta_ITTResult.InTraining)]
        [InlineData(dfeta_ITTProgrammeType.SchoolDirecttrainingprogramme_Salaried, dfeta_ITTResult.InTraining)]
        [InlineData(dfeta_ITTProgrammeType.SchoolDirecttrainingprogramme_Selffunded, dfeta_ITTResult.InTraining)]
        [InlineData(dfeta_ITTProgrammeType.TeachFirstProgramme, dfeta_ITTResult.InTraining)]
        [InlineData(dfeta_ITTProgrammeType.TeachFirstProgramme_CC, dfeta_ITTResult.InTraining)]
        [InlineData(dfeta_ITTProgrammeType.UndergraduateOptIn, dfeta_ITTResult.InTraining)]
        public void CreateInitialTeacherTrainingEntity_maps_entity_from_command_correctly(
            dfeta_ITTProgrammeType programmeType,
            dfeta_ITTResult expectedResult)
        {
            // Arrange
            var command = CreateCommand(c => c.InitialTeacherTraining.ProgrammeType = programmeType);

            var referenceData = new DataverseAdapter.CreateTeacherReferenceLookupResult()
            {
                IttCountryId = Guid.NewGuid(),
                IttProviderId = Guid.NewGuid(),
                IttSubject1Id = Guid.NewGuid(),
                IttSubject2Id = Guid.NewGuid(),
                IttSubject3Id = Guid.NewGuid(),
            };

            var helper = new DataverseAdapter.CreateTeacherHelper(_dataverseAdapter, command);

            // Act
            var result = helper.CreateInitialTeacherTrainingEntity(referenceData);

            // Assert
            Assert.Equal(Contact.EntityLogicalName, result.dfeta_PersonId?.LogicalName);
            Assert.Equal(helper.TeacherId, result.dfeta_PersonId?.Id);
            Assert.Equal(dfeta_country.EntityLogicalName, result.dfeta_CountryId?.LogicalName);
            Assert.Equal(referenceData.IttCountryId, result.dfeta_CountryId?.Id);
            Assert.Equal(Account.EntityLogicalName, result.dfeta_EstablishmentId?.LogicalName);
            Assert.Equal(referenceData.IttProviderId, result.dfeta_EstablishmentId?.Id);
            Assert.Equal(command.InitialTeacherTraining.ProgrammeStartDate, DateOnly.FromDateTime(result.dfeta_ProgrammeStartDate.Value));
            Assert.Equal(command.InitialTeacherTraining.ProgrammeEndDate, DateOnly.FromDateTime(result.dfeta_ProgrammeEndDate.Value));
            Assert.Equal(command.InitialTeacherTraining.ProgrammeType, result.dfeta_ProgrammeType);
            Assert.Equal(command.InitialTeacherTraining.ProgrammeEndDate.Year.ToString(), result.dfeta_CohortYear);
            Assert.Equal(dfeta_ittsubject.EntityLogicalName, result.dfeta_Subject1Id?.LogicalName);
            Assert.Equal(referenceData.IttSubject1Id, result.dfeta_Subject1Id?.Id);
            Assert.Equal(dfeta_ittsubject.EntityLogicalName, result.dfeta_Subject2Id?.LogicalName);
            Assert.Equal(referenceData.IttSubject2Id, result.dfeta_Subject2Id?.Id);
            Assert.Equal(expectedResult, result.dfeta_Result);
            Assert.Equal(command.InitialTeacherTraining.AgeRangeFrom, result.dfeta_AgeRangeFrom);
            Assert.Equal(command.InitialTeacherTraining.AgeRangeTo, result.dfeta_AgeRangeTo);
            Assert.Equal(dfeta_ittsubject.EntityLogicalName, result.dfeta_Subject3Id?.LogicalName);
            Assert.Equal(referenceData.IttSubject3Id, result.dfeta_Subject3Id?.Id);
            Assert.Equal(command.HusId, result.dfeta_TraineeID);
        }

        [Fact]
        public void CreateQualificationEntity()
        {
            // Arrange
            var command = CreateCommand();

            var referenceData = new DataverseAdapter.CreateTeacherReferenceLookupResult()
            {
                QualificationId = Guid.NewGuid(),
                QualificationCountryId = Guid.NewGuid(),
                QualificationSubjectId = Guid.NewGuid(),
                QualificationProviderId = Guid.NewGuid()
            };

            var helper = new DataverseAdapter.CreateTeacherHelper(_dataverseAdapter, command);

            // Act
            var result = helper.CreateQualificationEntity(referenceData);

            // Assert
            Assert.Equal(Contact.EntityLogicalName, result.dfeta_PersonId?.LogicalName);
            Assert.Equal(helper.TeacherId, result.dfeta_PersonId?.Id);
            Assert.Equal(dfeta_qualification_dfeta_Type.HigherEducation, result.dfeta_Type);
            Assert.Equal(dfeta_country.EntityLogicalName, result.dfeta_HE_CountryId?.LogicalName);
            Assert.Equal(referenceData.QualificationCountryId, result.dfeta_HE_CountryId?.Id);
            Assert.Equal(dfeta_hesubject.EntityLogicalName, result.dfeta_HE_HESubject1Id?.LogicalName);
            Assert.Equal(referenceData.QualificationSubjectId, result.dfeta_HE_HESubject1Id?.Id);
            Assert.Equal(command.Qualification.Class, result.dfeta_HE_ClassDivision);
            Assert.Equal(Account.EntityLogicalName, result.dfeta_HE_EstablishmentId?.LogicalName);
            Assert.Equal(referenceData.QualificationProviderId, result.dfeta_HE_EstablishmentId?.Id);
            Assert.Equal(command.Qualification.Date, DateOnly.FromDateTime(result.dfeta_HE_CompletionDate.Value));
        }

        [Fact]
        public void CreateQtsRegistrationEntity()
        {
            // Arrange
            var command = CreateCommand();

            var referenceData = new DataverseAdapter.CreateTeacherReferenceLookupResult()
            {
                EarlyYearsStatusId = Guid.NewGuid(),
                TeacherStatusId = Guid.NewGuid()
            };

            var helper = new DataverseAdapter.CreateTeacherHelper(_dataverseAdapter, command);

            // Act
            var result = helper.CreateQtsRegistrationEntity(referenceData);

            // Assert
            Assert.Equal(Contact.EntityLogicalName, result.dfeta_PersonId?.LogicalName);
            Assert.Equal(helper.TeacherId, result.dfeta_PersonId?.Id);
            Assert.Equal(referenceData.EarlyYearsStatusId, result.dfeta_EarlyYearsStatusId?.Id);
            Assert.Equal(referenceData.TeacherStatusId, result.dfeta_TeacherStatusId?.Id);
        }

        [Theory(Skip = "This is flaky because of existing data in the environment - review when we have a way of resetting the environment")]
        [InlineData(false, true, true, true, new[] { Contact.Fields.MiddleName, Contact.Fields.LastName, Contact.Fields.BirthDate })]
        [InlineData(true, true, true, false, new[] { Contact.Fields.FirstName, Contact.Fields.MiddleName, Contact.Fields.LastName })]
        [InlineData(true, true, false, true, new[] { Contact.Fields.FirstName, Contact.Fields.MiddleName, Contact.Fields.BirthDate })]
        [InlineData(true, false, true, true, new[] { Contact.Fields.FirstName, Contact.Fields.LastName, Contact.Fields.BirthDate })]
        [InlineData(true, true, true, true, new[] { Contact.Fields.FirstName, Contact.Fields.MiddleName, Contact.Fields.LastName, Contact.Fields.BirthDate })]
        public async Task FindExistingTeacher_given_at_least_three_matching_fields_returns_matched_id(
            bool matchOnForename,
            bool matchOnMiddlename,
            bool matchOnSurname,
            bool matchOnDateOfBirth,
            string[] expectedMatchedAttributes)
        {
            // Arrange
            var command = CreateCommand();

            var existingTeacherId = await _organizationService.CreateAsync(new Contact()
            {
                FirstName = matchOnForename ? command.FirstName : "Glad",
                MiddleName = matchOnMiddlename ? command.MiddleName : "I.",
                LastName = matchOnSurname ? command.LastName : "Oli",
                BirthDate = matchOnDateOfBirth ? command.BirthDate : new(1945, 2, 3)
            });

            var helper = new DataverseAdapter.CreateTeacherHelper(_dataverseAdapter, command);

            // Act
            var result = await helper.FindExistingTeacher();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(existingTeacherId, result?.TeacherId);
            Assert.Equal(expectedMatchedAttributes, result?.MatchedAttributes);
        }

        [Theory]
        [InlineData("Joe3", "X", "Bloggs", "First name contains a digit")]
        [InlineData("Joe", "X3", "Bloggs", "Middle name contains a digit")]
        [InlineData("Joe", "X", "Bloggs3", "Last name contains a digit")]
        [InlineData("Jo3e", "3X", "Bloggs", "First name and middle name contain a digit")]
        [InlineData("Joe", "X3", "Blog3gs", "Middle name and last name contain a digit")]
        [InlineData("Joe3", "X", "Bloggs3", "First name and last name contain a digit")]
        [InlineData("Joe3", "X3", "Bloggs3", "First name, middle name and last name contain a digit")]
        public async Task Given_name_containing_digits_creates_review_task(
            string firstName,
            string middleName,
            string lastName,
            string expectedDescription)
        {
            // Arrange
            var command = CreateCommand(cmd =>
            {
                cmd.FirstName = firstName;
                cmd.MiddleName = middleName;
                cmd.LastName = lastName;
            });

            // Act
            var (result, transactionRequest) = await _dataverseAdapter.CreateTeacherImpl(command);

            // Assert
            transactionRequest.AssertContainsCreateRequest<CrmTask>(t =>
                t.RegardingObjectId?.Id == result.TeacherId &&
                t.Description == expectedDescription);
        }

        [Fact]
        public async Task Given_record_successfully_created_itt_programmestartdate_and_programmeenddate_matches_request()
        {
            // Arrange
            var startDate = new DateOnly(2020, 01, 13);
            var endDate = new DateOnly(2021, 01, 07);
            var birthDate = new DateOnly(1970, 06, 06);
            var command = new CreateTeacherCommand()
            {
                FirstName = "Minnie",
                LastName = "Ryder",
                BirthDate = birthDate.ToDateTime(),
                GenderCode = Contact_GenderCode.Female,
                InitialTeacherTraining = new()
                {
                    ProviderUkprn = "10044534",  // ARK Teacher Training
                    ProgrammeStartDate = startDate,
                    ProgrammeEndDate = endDate,
                    ProgrammeType = dfeta_ITTProgrammeType.GraduateTeacherProgramme,
                }
            };

            // Act
            var (result, _) = await _dataverseAdapter.CreateTeacherImpl(command);
            var getIttRecords = await _dataverseAdapter.GetInitialTeacherTrainingByTeacher(
                result.TeacherId,
                columnNames: new[]
                {
                    dfeta_initialteachertraining.Fields.dfeta_ProgrammeStartDate,
                    dfeta_initialteachertraining.Fields.dfeta_ProgrammeEndDate,
                });
            var savedProgrammeStartDate = DateOnly.FromDateTime(getIttRecords[0].dfeta_ProgrammeStartDate.Value);
            var savedProgrammeEndDate = DateOnly.FromDateTime(getIttRecords[0].dfeta_ProgrammeEndDate.Value);

            // Assert
            Assert.True(result.Succeeded);
            Assert.Equal(startDate, savedProgrammeStartDate);
            Assert.Equal(endDate, savedProgrammeEndDate);
        }

        private static CreateTeacherCommand CreateCommand(Action<CreateTeacherCommand> configureCommand = null)
        {
            var command = new CreateTeacherCommand()
            {
                FirstName = Faker.Name.First(),
                MiddleName = Faker.Name.Middle(),
                LastName = Faker.Name.Last(),
                BirthDate = Faker.Identification.DateOfBirth(),
                EmailAddress = Faker.Internet.Email(),
                Address = new()
                {
                    AddressLine1 = Faker.Address.StreetAddress(),
                    City = Faker.Address.City(),
                    PostalCode = Faker.Address.UkPostCode(),
                    Country = "United Kingdom"
                },
                GenderCode = Contact_GenderCode.Female,
                InitialTeacherTraining = new()
                {
                    ProviderUkprn = "10044534",  // ARK Teacher Training
                    ProgrammeStartDate = new(2020, 4, 1),
                    ProgrammeEndDate = new(2020, 10, 10),
                    ProgrammeType = dfeta_ITTProgrammeType.GraduateTeacherProgramme,
                    Subject1 = "100366",  // computer science
                    Subject2 = "100403",  // mathematics
                    Subject3 = "100302",  // history
                    AgeRangeFrom = dfeta_AgeRange._05,
                    AgeRangeTo = dfeta_AgeRange._11
                },
                Qualification = new()
                {
                    ProviderUkprn = "10044534",
                    CountryCode = "XK",
                    Subject = "100366",  // computer science
                    Class = dfeta_classdivision.Firstclasshonours,
                    Date = new(2021, 5, 3)
                }
            };

            configureCommand?.Invoke(command);

            return command;
        }
    }

    public class CreateTeacherFixture : IAsyncLifetime
    {
        private readonly CrmClientFixture.TestDataScope _dataScope;

        public CreateTeacherFixture(CrmClientFixture crmClientFixture)
        {
            _dataScope = crmClientFixture.CreateTestDataScope();
        }

        public Guid ExistingTeacherId { get; private set; }
        public string ExistingTeacherFirstName => "Joe";
        public string ExistingTeacherFirstNameMiddleName => "X";
        public string ExistingTeacherFirstNameLastName => "Bloggs";
        public DateTime ExistingTeacherFirstNameBirthDate => new DateTime(1990, 5, 23);

        public async Task DisposeAsync() => await _dataScope.DisposeAsync();

        public async Task InitializeAsync()
        {
            ExistingTeacherId = await _dataScope.OrganizationService.CreateAsync(new Contact()
            {
                FirstName = ExistingTeacherFirstName,
                MiddleName = ExistingTeacherFirstNameMiddleName,
                LastName = ExistingTeacherFirstNameLastName,
                BirthDate = ExistingTeacherFirstNameBirthDate
            });
        }
    }
}
