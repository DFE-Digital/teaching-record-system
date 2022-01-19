using System;
using DqtApi.DataStore.Crm;
using DqtApi.DataStore.Crm.Models;
using Microsoft.PowerPlatform.Dataverse.Client;
using Xunit;

namespace DqtApi.Tests.DataverseIntegration
{
    [Collection(nameof(DataverseTestCollection))]
    public class CreateTeacherTests : IClassFixture<CrmClientFixture>, IAsyncLifetime
    {
        private readonly CrmClientFixture _crmClientFixture;
        private readonly DataverseAdapter _dataverseAdapter;
        private readonly ServiceClient _serviceClient;

        public CreateTeacherTests(CrmClientFixture crmClientFixture)
        {
            _crmClientFixture = crmClientFixture;
            _dataverseAdapter = crmClientFixture.CreateDataverseAdapter();
            _serviceClient = crmClientFixture.ServiceClient;
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public Task DisposeAsync() => _crmClientFixture.CleanupEntities();

        [Fact]
        public async Task Given_valid_request_creates_required_entities()
        {
            var command = CreateCommand();

            // Act
            var (result, transactionRequest) = await _dataverseAdapter.CreateTeacherImpl(command);
            _crmClientFixture.RegisterForCleanup(Contact.EntityLogicalName, result.TeacherId);

            // Assert
            Assert.True(result.Succeeded);

            transactionRequest.AssertSingleCreateRequest<Contact>();
            transactionRequest.AssertSingleCreateRequest<dfeta_initialteachertraining>();
            transactionRequest.AssertSingleCreateRequest<dfeta_qualification>();
            transactionRequest.AssertSingleCreateRequest<dfeta_qtsregistration>();
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
            _crmClientFixture.RegisterForCleanup(Contact.EntityLogicalName, result.TeacherId);

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
            var firstName = "Joe";
            var middleName = "X";
            var lastName = "Bloggs";
            var birthDate = new DateTime(1990, 5, 23);

            var existingTeacherId = await _serviceClient.CreateAsync(new Contact()
            {
                FirstName = firstName,
                MiddleName = middleName,
                LastName = lastName,
                BirthDate = birthDate
            });

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
            _crmClientFixture.RegisterForCleanup(Contact.EntityLogicalName, result.TeacherId);
            _crmClientFixture.RegisterForCleanup(Contact.EntityLogicalName, existingTeacherId);

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
            Assert.Equal(_crmClientFixture.Clock.UtcNow, crmTask.ScheduledEnd);

            var expectedDescription = $"Potential duplicate\nMatched on\n\t- First name: '{firstName}'\n\t- Middle name: '{middleName}'\n\t- Last name: '{lastName}'\n\t- Date of birth: '{birthDate:dd/MM/yyyy}'\n" +
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
        }

        [Fact]
        public void CreateInitialTeacherTrainingEntity_maps_entity_from_command_correctly()
        {
            // Arrange
            var command = CreateCommand();

            var referenceData = new DataverseAdapter.CreateTeacherReferenceLookupResult()
            {
                IttCountryId = Guid.NewGuid(),
                IttProviderId = Guid.NewGuid(),
                IttSubject1Id = Guid.NewGuid(),
                IttSubject2Id = Guid.NewGuid(),
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
            Assert.Equal(command.InitialTeacherTraining.ProgrammeStartDate, result.dfeta_ProgrammeStartDate);
            Assert.Equal(command.InitialTeacherTraining.ProgrammeEndDate, result.dfeta_ProgrammeEndDate);
            Assert.Equal(command.InitialTeacherTraining.ProgrammeType, result.dfeta_ProgrammeType);
            Assert.Equal(command.InitialTeacherTraining.ProgrammeEndDate.Year.ToString(), result.dfeta_CohortYear);
            Assert.Equal(dfeta_ittsubject.EntityLogicalName, result.dfeta_Subject1Id?.LogicalName);
            Assert.Equal(referenceData.IttSubject1Id, result.dfeta_Subject1Id?.Id);
            Assert.Equal(dfeta_ittsubject.EntityLogicalName, result.dfeta_Subject2Id?.LogicalName);
            Assert.Equal(referenceData.IttSubject2Id, result.dfeta_Subject2Id?.Id);
            Assert.Equal(command.InitialTeacherTraining.Result, result.dfeta_Result);
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
            Assert.Equal(command.Qualification.Date, result.dfeta_HE_CompletionDate);
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

            var existingTeacherId = await _serviceClient.CreateAsync(new Contact()
            {
                FirstName = matchOnForename ? command.FirstName : "Glad",
                MiddleName = matchOnMiddlename ? command.MiddleName : "I.",
                LastName = matchOnSurname ? command.LastName : "Oli",
                BirthDate = matchOnDateOfBirth ? command.BirthDate : new(1945, 2, 3)
            });
            _crmClientFixture.RegisterForCleanup(Contact.EntityLogicalName, existingTeacherId);

            var helper = new DataverseAdapter.CreateTeacherHelper(_dataverseAdapter, command);

            // Act
            var result = await helper.FindExistingTeacher();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(existingTeacherId, result?.TeacherId);
            Assert.Equal(expectedMatchedAttributes, result?.MatchedAttributes);
        }

        [Theory]
        [InlineData(dfeta_ITTProgrammeType.EYITTAssessmentOnly, true)]
        [InlineData(dfeta_ITTProgrammeType.EYITTGraduateEmploymentBased, true)]
        [InlineData(dfeta_ITTProgrammeType.EYITTGraduateEntry, true)]
        [InlineData(dfeta_ITTProgrammeType.EYITTSchoolDirect_EarlyYears, true)]
        [InlineData(dfeta_ITTProgrammeType.EYITTUndergraduate, true)]
        [InlineData(dfeta_ITTProgrammeType.Apprenticeship, false)]
        [InlineData(dfeta_ITTProgrammeType.AssessmentOnlyRoute, false)]
        [InlineData(dfeta_ITTProgrammeType.Core, false)]
        [InlineData(dfeta_ITTProgrammeType.CoreFlexible, false)]
        [InlineData(dfeta_ITTProgrammeType.FutureTeachingScholars, false)]
        [InlineData(dfeta_ITTProgrammeType.GraduateTeacherProgramme, false)]
        [InlineData(dfeta_ITTProgrammeType.HEI, false)]
        [InlineData(dfeta_ITTProgrammeType.LicensedTeacherProgramme, false)]
        [InlineData(dfeta_ITTProgrammeType.OverseasTrainedTeacherProgramme, false)]
        [InlineData(dfeta_ITTProgrammeType.RegisteredTeacherProgramme, false)]
        [InlineData(dfeta_ITTProgrammeType.SchoolDirecttrainingprogramme, false)]
        [InlineData(dfeta_ITTProgrammeType.SchoolDirecttrainingprogramme_Salaried, false)]
        [InlineData(dfeta_ITTProgrammeType.SchoolDirecttrainingprogramme_Selffunded, false)]
        [InlineData(dfeta_ITTProgrammeType.TeachFirstProgramme, false)]
        [InlineData(dfeta_ITTProgrammeType.TeachFirstProgramme_CC, false)]
        [InlineData(dfeta_ITTProgrammeType.UndergraduateOptIn, false)]
        public void IsEarlyYears_returns_correct_value(dfeta_ITTProgrammeType programmeType, bool expectedResult)
        {
            // Arrange
            var command = CreateCommand(c => c.InitialTeacherTraining.ProgrammeType = programmeType);

            var helper = new DataverseAdapter.CreateTeacherHelper(_dataverseAdapter, command);

            // Act
            var result = helper.IsEarlyYears;

            // Assert
            Assert.Equal(expectedResult, result);
        }

        private static CreateTeacherCommand CreateCommand(Action<CreateTeacherCommand> configureCommand = null)
        {
            var command = new CreateTeacherCommand()
            {
                FirstName = "Minnie",
                MiddleName = "Van",
                LastName = "Ryder",
                BirthDate = new(1990, 5, 23),
                EmailAddress = "minnie.van.ryder@example.com",
                Address = new()
                {
                    AddressLine1 = "52 Quernmore Road",
                    City = "Liverpool",
                    PostalCode = "L33 6UZ",
                    Country = "United Kingdom"
                },
                GenderCode = Contact_GenderCode.Female,
                InitialTeacherTraining = new()
                {
                    ProviderUkprn = "10044534",  // ARK Teacher Training
                    ProgrammeStartDate = new(2020, 4, 1),
                    ProgrammeEndDate = new(2020, 10, 10),
                    ProgrammeType = dfeta_ITTProgrammeType.GraduateTeacherProgramme,
                    Subject1 = "Computer Science",
                    Subject2 = "Mathematics",
                    Result = dfeta_ITTResult.Approved
                },
                Qualification = new()
                {
                    ProviderUkprn = "10044534",
                    CountryCode = "XK",
                    Subject = "Computing",
                    Class = dfeta_classdivision.Firstclasshonours,
                    Date = new(2021, 5, 3)
                }
            };

            configureCommand?.Invoke(command);

            return command;
        }
    }
}
