#nullable disable
using System.Diagnostics;
using Microsoft.PowerPlatform.Dataverse.Client;
using TeachingRecordSystem.Core.Services.DqtOutbox;
using TeachingRecordSystem.Core.Services.DqtOutbox.Messages;

namespace TeachingRecordSystem.Core.Dqt.CrmIntegrationTests.DataverseAdapterTests;

public class CreateTeacherTests : IClassFixture<CreateTeacherFixture>, IAsyncLifetime
{
    private readonly CreateTeacherFixture _createTeacherFixture;
    private readonly IClock _clock;
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
        var command = CreateCommand(configureCommand: cmd =>
        {
            cmd.InitialTeacherTraining.ProgrammeType = programmeType;
        });

        // Act
        var (result, transactionRequest) = await _dataverseAdapter.CreateTeacherImplAsync(command);

        // Assert
        Assert.True(result.Succeeded);

        transactionRequest.AssertSingleCreateRequest<Contact>();
        transactionRequest.AssertSingleCreateRequest<dfeta_TrsOutboxMessage>();
        transactionRequest.AssertSingleCreateRequest<dfeta_initialteachertraining>();
        transactionRequest.AssertSingleCreateRequest<dfeta_qualification>();
        transactionRequest.AssertSingleCreateRequest<dfeta_qtsregistration>();
    }

    [Fact]
    public async Task Given_Trainee_teacher_does_not_create_induction_entity()
    {
        // Arrange
        var command = CreateCommand(CreateTeacherType.TraineeTeacher);

        // Act
        var (result, transactionRequest) = await _dataverseAdapter.CreateTeacherImplAsync(command);

        // Assert
        Assert.True(result.Succeeded);

        transactionRequest.AssertDoesNotContainCreateRequest<dfeta_induction>();
    }

    [Fact]
    public async Task Given_OverseasQualifiedTeacher_teacher_creates_induction_entity()
    {
        // Arrange
        var command = CreateCommand(CreateTeacherType.OverseasQualifiedTeacher);

        // Act
        var (result, transactionRequest) = await _dataverseAdapter.CreateTeacherImplAsync(command);

        // Assert
        Assert.True(result.Succeeded);

        transactionRequest.AssertSingleCreateRequest<dfeta_induction>();
    }

    [Fact]
    public async Task Given_minimal_details_request_succeeds()
    {
        // Arrange
        var command = new CreateTeacherCommand()
        {
            FirstName = Faker.Name.First(),
            LastName = Faker.Name.Last(),
            BirthDate = Faker.Identification.DateOfBirth(),
            GenderCode = Contact_GenderCode.Female,
            InitialTeacherTraining = new()
            {
                ProviderUkprn = "10044534",  // ARK Teacher Training
                ProgrammeStartDate = new(2020, 4, 1),
                ProgrammeEndDate = new(2020, 10, 10),
                ProgrammeType = dfeta_ITTProgrammeType.GraduateTeacherProgramme,
                IttQualificationAim = dfeta_ITTQualificationAim.Professionalstatusandacademicaward
            }
        };

        // Act
        var (result, transactionRequest) = await _dataverseAdapter.CreateTeacherImplAsync(command);

        // Assert
        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task Given_specified_qualification_type_creates_qualification_with_type()
    {
        // Arrange
        var qualificationValue = "401";  // Higher Degree
        var command = CreateCommand(configureCommand: cmd => cmd.Qualification.HeQualificationValue = qualificationValue);

        // Act
        var (result, transactionRequest) = await _dataverseAdapter.CreateTeacherImplAsync(command);

        // Assert
        Assert.True(result.Succeeded);
        var qualifications = await _dataverseAdapter.GetQualificationsForTeacherAsync(
            result.TeacherId,
            columnNames: Array.Empty<string>(),
            heQualificationColumnNames: new[]
            {
                dfeta_hequalification.PrimaryIdAttribute,
                dfeta_hequalification.Fields.dfeta_name
            });
        Assert.Collection(qualifications, qualification => Assert.Equal("Higher Degree", qualification.Extract<dfeta_hequalification>().dfeta_name));
    }

    [Fact]
    public async Task Given_no_specified_qualification_type_creates_qualification_with_default_type()
    {
        // Arrange
        var command = CreateCommand(configureCommand: cmd => cmd.Qualification.HeQualificationValue = null);

        // Act
        var (result, transactionRequest) = await _dataverseAdapter.CreateTeacherImplAsync(command);

        // Assert
        Assert.True(result.Succeeded);
        var qualifications = await _dataverseAdapter.GetQualificationsForTeacherAsync(
            result.TeacherId,
            columnNames: Array.Empty<string>(),
            heQualificationColumnNames: new[]
            {
                dfeta_hequalification.PrimaryIdAttribute,
                dfeta_hequalification.Fields.dfeta_name
            });
        Assert.Collection(qualifications, qualification => Assert.Equal("First Degree", qualification.Extract<dfeta_hequalification>().dfeta_name));
    }

    [Fact]
    public async Task Given_details_that_do_not_match_existing_records_allocates_trn_and_does_not_create_QTS_task()
    {
        // Arrange
        DataverseAdapter.FindExistingTeacher findExistingTeacher = () =>
            Task.FromResult<DataverseAdapter.CreateTeacherDuplicateTeacherResult[]>(Array.Empty<DataverseAdapter.CreateTeacherDuplicateTeacherResult>());

        var command = CreateCommand();

        // Act
        var (result, transactionRequest) = await _dataverseAdapter.CreateTeacherImplAsync(command, findExistingTeacher);

        // Assert
        Assert.True(result.Succeeded);
        Assert.NotNull(result.Trn);

        transactionRequest.AssertDoesNotContainCreateRequest<CrmTask>();
    }

    [Theory]
    [InlineData(CreateTeacherType.TraineeTeacher, null, false, false, false, "", "DMSImportTrn")]
    //[InlineData(CreateTeacherType.TraineeTeacher, "1234", false, false, false, "", "HESAImportTrn")]
    [InlineData(CreateTeacherType.OverseasQualifiedTeacher, null, false, false, false, "", "ApplyForQts")]
    //[InlineData(CreateTeacherType.OverseasQualifiedTeacher, "2345", false, false, false, "", "ApplyForQts")]
    [InlineData(CreateTeacherType.TraineeTeacher, null, true, false, false, "Matched record has active sanctions\n", "DMSImportTrn")]
    [InlineData(CreateTeacherType.TraineeTeacher, null, false, true, false, "Matched record has QTS date\n", "DMSImportTrn")]
    [InlineData(CreateTeacherType.TraineeTeacher, null, false, false, true, "Matched record has EYTS date\n", "DMSImportTrn")]
    [InlineData(CreateTeacherType.TraineeTeacher, null, true, true, false, "Matched record has active sanctions & QTS date\n", "DMSImportTrn")]
    [InlineData(CreateTeacherType.TraineeTeacher, null, true, false, true, "Matched record has active sanctions & EYTS date\n", "DMSImportTrn")]
    public async Task Given_details_that_does_match_existing_record_does_not_allocate_trn_and_creates_QTS_task(
        CreateTeacherType teacherType,
        string husId,
        bool hasActiveSanctions,
        bool hasQts,
        bool hasEyts,
        string expectedDescriptionSupplement,
        string expectedCategory)
    {
        // Arrange
        var firstName = _createTeacherFixture.ExistingTeacherFirstName;
        var middleName = _createTeacherFixture.ExistingTeacherFirstNameMiddleName;
        var lastName = _createTeacherFixture.ExistingTeacherFirstNameLastName;
        var birthDate = _createTeacherFixture.ExistingTeacherFirstNameBirthDate;
        var existingTeacherId = _createTeacherFixture.ExistingTeacherId;

        DataverseAdapter.FindExistingTeacher findExistingTeacher = () =>
            Task.FromResult(new[] {
                new DataverseAdapter.CreateTeacherDuplicateTeacherResult()
                {
                    TeacherId = existingTeacherId,
                    MatchedAttributes = new[] { Contact.Fields.FirstName, Contact.Fields.MiddleName, Contact.Fields.LastName, Contact.Fields.BirthDate },
                    HasActiveSanctions = hasActiveSanctions,
                    HasQtsDate = hasQts,
                    HasEytsDate = hasEyts
                }
            });

        var command = CreateCommand(teacherType, configureCommand: command =>
        {
            command.FirstName = firstName;
            command.MiddleName = middleName;
            command.LastName = lastName;
            command.BirthDate = birthDate;
            command.HusId = husId;

            if (teacherType == CreateTeacherType.OverseasQualifiedTeacher)
            {
                command.QtsDate = new DateOnly(2020, 10, 1);
                command.RecognitionRoute = CreateTeacherRecognitionRoute.Scotland;
            }
        });

        // Act
        var (result, transactionRequest) = await _dataverseAdapter.CreateTeacherImplAsync(command, findExistingTeacher);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Null(result.Trn);

        var crmTask = transactionRequest.AssertSingleCreateRequest<CrmTask>();
        Assert.Equal(Contact.EntityLogicalName, crmTask.RegardingObjectId?.LogicalName);
        Assert.Equal(result.TeacherId, crmTask.RegardingObjectId?.Id);
        Assert.Equal(Contact.EntityLogicalName, crmTask.dfeta_potentialduplicateid?.LogicalName);
        Assert.Equal(existingTeacherId, crmTask.dfeta_potentialduplicateid?.Id);
        Assert.Equal(expectedCategory, crmTask.Category);
        Assert.Equal("Notification for QTS Unit Team", crmTask.Subject);
        Assert.Equal(_clock.UtcNow, crmTask.ScheduledEnd.Value, TimeSpan.FromSeconds(15));

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
        var command = CreateCommand(configureCommand: command => command.InitialTeacherTraining.ProviderUkprn = "badukprn");

        // Act
        var (result, _) = await _dataverseAdapter.CreateTeacherImplAsync(command);

        // Assert
        Assert.False(result.Succeeded);
        Assert.True(result.FailedReasons.HasFlag(CreateTeacherFailedReasons.IttProviderNotFound));
    }

    [Fact]
    public async Task Given_qualification_subject2_that_is_not_found_returns_failed()
    {
        // Arrange
        var command = CreateCommand(configureCommand: command => command.Qualification.Subject2 = "SOME BAD SUBJECT");

        // Act
        var (result, _) = await _dataverseAdapter.CreateTeacherImplAsync(command);

        // Assert
        Assert.False(result.Succeeded);
        Assert.True(result.FailedReasons.HasFlag(CreateTeacherFailedReasons.QualificationSubject2NotFound));
    }

    [Fact]
    public async Task Given_qualification_subject3_that_is_not_found_returns_failed()
    {
        // Arrange
        var command = CreateCommand(configureCommand: command => command.Qualification.Subject3 = "SOME BAD SUBJECT");

        // Act
        var (result, _) = await _dataverseAdapter.CreateTeacherImplAsync(command);

        // Assert
        Assert.False(result.Succeeded);
        Assert.True(result.FailedReasons.HasFlag(CreateTeacherFailedReasons.QualificationSubject3NotFound));
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
    [InlineData(CreateTeacherType.TraineeTeacher, dfeta_ITTProgrammeType.Apprenticeship, dfeta_ITTResult.InTraining)]
    [InlineData(CreateTeacherType.TraineeTeacher, dfeta_ITTProgrammeType.AssessmentOnlyRoute, dfeta_ITTResult.UnderAssessment)]
    [InlineData(CreateTeacherType.TraineeTeacher, dfeta_ITTProgrammeType.Core, dfeta_ITTResult.InTraining)]
    [InlineData(CreateTeacherType.TraineeTeacher, dfeta_ITTProgrammeType.CoreFlexible, dfeta_ITTResult.InTraining)]
    [InlineData(CreateTeacherType.TraineeTeacher, dfeta_ITTProgrammeType.EYITTAssessmentOnly, dfeta_ITTResult.InTraining)]
    [InlineData(CreateTeacherType.TraineeTeacher, dfeta_ITTProgrammeType.EYITTGraduateEmploymentBased, dfeta_ITTResult.InTraining)]
    [InlineData(CreateTeacherType.TraineeTeacher, dfeta_ITTProgrammeType.EYITTGraduateEntry, dfeta_ITTResult.InTraining)]
    [InlineData(CreateTeacherType.TraineeTeacher, dfeta_ITTProgrammeType.EYITTSchoolDirect_EarlyYears, dfeta_ITTResult.InTraining)]
    [InlineData(CreateTeacherType.TraineeTeacher, dfeta_ITTProgrammeType.EYITTUndergraduate, dfeta_ITTResult.InTraining)]
    [InlineData(CreateTeacherType.TraineeTeacher, dfeta_ITTProgrammeType.FutureTeachingScholars, dfeta_ITTResult.InTraining)]
    [InlineData(CreateTeacherType.TraineeTeacher, dfeta_ITTProgrammeType.GraduateTeacherProgramme, dfeta_ITTResult.InTraining)]
    [InlineData(CreateTeacherType.TraineeTeacher, dfeta_ITTProgrammeType.HEI, dfeta_ITTResult.InTraining)]
    [InlineData(CreateTeacherType.TraineeTeacher, dfeta_ITTProgrammeType.LicensedTeacherProgramme, dfeta_ITTResult.InTraining)]
    [InlineData(CreateTeacherType.TraineeTeacher, dfeta_ITTProgrammeType.OverseasTrainedTeacherProgramme, dfeta_ITTResult.InTraining)]
    [InlineData(CreateTeacherType.TraineeTeacher, dfeta_ITTProgrammeType.RegisteredTeacherProgramme, dfeta_ITTResult.InTraining)]
    [InlineData(CreateTeacherType.TraineeTeacher, dfeta_ITTProgrammeType.SchoolDirecttrainingprogramme, dfeta_ITTResult.InTraining)]
    [InlineData(CreateTeacherType.TraineeTeacher, dfeta_ITTProgrammeType.SchoolDirecttrainingprogramme_Salaried, dfeta_ITTResult.InTraining)]
    [InlineData(CreateTeacherType.TraineeTeacher, dfeta_ITTProgrammeType.SchoolDirecttrainingprogramme_Selffunded, dfeta_ITTResult.InTraining)]
    [InlineData(CreateTeacherType.TraineeTeacher, dfeta_ITTProgrammeType.TeachFirstProgramme, dfeta_ITTResult.InTraining)]
    [InlineData(CreateTeacherType.TraineeTeacher, dfeta_ITTProgrammeType.TeachFirstProgramme_CC, dfeta_ITTResult.InTraining)]
    [InlineData(CreateTeacherType.TraineeTeacher, dfeta_ITTProgrammeType.UndergraduateOptIn, dfeta_ITTResult.InTraining)]
    [InlineData(CreateTeacherType.TraineeTeacher, dfeta_ITTProgrammeType.Providerled_postgrad, dfeta_ITTResult.InTraining)]
    [InlineData(CreateTeacherType.TraineeTeacher, dfeta_ITTProgrammeType.Providerled_undergrad, dfeta_ITTResult.InTraining)]
    [InlineData(CreateTeacherType.TraineeTeacher, dfeta_ITTProgrammeType.HighpotentialITT, dfeta_ITTResult.InTraining)]
    [InlineData(CreateTeacherType.OverseasQualifiedTeacher, dfeta_ITTProgrammeType.Apprenticeship, dfeta_ITTResult.Approved)]
    [InlineData(CreateTeacherType.OverseasQualifiedTeacher, dfeta_ITTProgrammeType.AssessmentOnlyRoute, dfeta_ITTResult.Approved)]
    [InlineData(CreateTeacherType.OverseasQualifiedTeacher, dfeta_ITTProgrammeType.Core, dfeta_ITTResult.Approved)]
    [InlineData(CreateTeacherType.OverseasQualifiedTeacher, dfeta_ITTProgrammeType.CoreFlexible, dfeta_ITTResult.Approved)]
    [InlineData(CreateTeacherType.OverseasQualifiedTeacher, dfeta_ITTProgrammeType.FutureTeachingScholars, dfeta_ITTResult.Approved)]
    [InlineData(CreateTeacherType.OverseasQualifiedTeacher, dfeta_ITTProgrammeType.GraduateTeacherProgramme, dfeta_ITTResult.Approved)]
    [InlineData(CreateTeacherType.OverseasQualifiedTeacher, dfeta_ITTProgrammeType.HEI, dfeta_ITTResult.Approved)]
    [InlineData(CreateTeacherType.OverseasQualifiedTeacher, dfeta_ITTProgrammeType.LicensedTeacherProgramme, dfeta_ITTResult.Approved)]
    [InlineData(CreateTeacherType.OverseasQualifiedTeacher, dfeta_ITTProgrammeType.OverseasTrainedTeacherProgramme, dfeta_ITTResult.Approved)]
    [InlineData(CreateTeacherType.OverseasQualifiedTeacher, dfeta_ITTProgrammeType.RegisteredTeacherProgramme, dfeta_ITTResult.Approved)]
    [InlineData(CreateTeacherType.OverseasQualifiedTeacher, dfeta_ITTProgrammeType.SchoolDirecttrainingprogramme, dfeta_ITTResult.Approved)]
    [InlineData(CreateTeacherType.OverseasQualifiedTeacher, dfeta_ITTProgrammeType.SchoolDirecttrainingprogramme_Salaried, dfeta_ITTResult.Approved)]
    [InlineData(CreateTeacherType.OverseasQualifiedTeacher, dfeta_ITTProgrammeType.SchoolDirecttrainingprogramme_Selffunded, dfeta_ITTResult.Approved)]
    [InlineData(CreateTeacherType.OverseasQualifiedTeacher, dfeta_ITTProgrammeType.TeachFirstProgramme, dfeta_ITTResult.Approved)]
    [InlineData(CreateTeacherType.OverseasQualifiedTeacher, dfeta_ITTProgrammeType.TeachFirstProgramme_CC, dfeta_ITTResult.Approved)]
    [InlineData(CreateTeacherType.OverseasQualifiedTeacher, dfeta_ITTProgrammeType.UndergraduateOptIn, dfeta_ITTResult.Approved)]
    [InlineData(CreateTeacherType.OverseasQualifiedTeacher, dfeta_ITTProgrammeType.Providerled_postgrad, dfeta_ITTResult.Approved)]
    [InlineData(CreateTeacherType.OverseasQualifiedTeacher, dfeta_ITTProgrammeType.Providerled_undergrad, dfeta_ITTResult.Approved)]
    public void CreateInitialTeacherTrainingEntity_maps_entity_from_command_correctly(
        CreateTeacherType createTeacherType,
        dfeta_ITTProgrammeType programmeType,
        dfeta_ITTResult expectedResult)
    {
        // Arrange
        var command = CreateCommand(
            createTeacherType,
            c => c.InitialTeacherTraining.ProgrammeType = programmeType);

        var referenceData = new DataverseAdapter.CreateTeacherReferenceLookupResult()
        {
            IttCountryId = Guid.NewGuid(),
            IttProviderId = Guid.NewGuid(),
            IttSubject1Id = Guid.NewGuid(),
            IttSubject2Id = Guid.NewGuid(),
            IttSubject3Id = Guid.NewGuid(),
            IttQualificationId = Guid.NewGuid()
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
        Assert.Equal(referenceData.IttQualificationId, result.dfeta_ITTQualificationId?.Id);
        Assert.Equal(expectedResult, result.dfeta_Result);
        Assert.Equal(command.InitialTeacherTraining.AgeRangeFrom, result.dfeta_AgeRangeFrom);
        Assert.Equal(command.InitialTeacherTraining.AgeRangeTo, result.dfeta_AgeRangeTo);
        Assert.Equal(dfeta_ittsubject.EntityLogicalName, result.dfeta_Subject3Id?.LogicalName);
        Assert.Equal(referenceData.IttSubject3Id, result.dfeta_Subject3Id?.Id);
        Assert.Equal(command.HusId, result.dfeta_TraineeID);
        Assert.Equal(command.InitialTeacherTraining.IttQualificationAim, result.dfeta_ittqualificationaim);
    }

    [Fact]
    public async Task Given_husid_does_not_exist_request_succeeds_and_does_not_creates_review_task()
    {
        // Arrange
        var husid = Guid.NewGuid().ToString();
        var command = new CreateTeacherCommand()
        {
            FirstName = Faker.Name.First(),
            LastName = Faker.Name.Last(),
            BirthDate = Faker.Identification.DateOfBirth(),
            GenderCode = Contact_GenderCode.Female,
            InitialTeacherTraining = new()
            {
                ProviderUkprn = "10044534",  // ARK Teacher Training
                ProgrammeStartDate = new(2020, 4, 1),
                ProgrammeEndDate = new(2020, 10, 10),
                ProgrammeType = dfeta_ITTProgrammeType.GraduateTeacherProgramme,
                IttQualificationAim = dfeta_ITTQualificationAim.Professionalstatusandacademicaward
            },
            HusId = husid
        };

        // Act
        var (result, transactionRequest) = await _dataverseAdapter.CreateTeacherImplAsync(command);

        // Assert
        Assert.True(result.Succeeded);
        transactionRequest.AssertDoesNotContainCreateRequest<CrmTask>();
    }

    [Fact]
    public async Task Given_itt_slugid_exists_request_succeeds_and_creates_review_task()
    {
        // Arrange
        var slugId = Guid.NewGuid().ToString();
        var teachercommand1 = new CreateTeacherCommand()
        {
            FirstName = Faker.Name.First(),
            LastName = Faker.Name.Last(),
            BirthDate = Faker.Identification.DateOfBirth(),
            GenderCode = Contact_GenderCode.Female,
            InitialTeacherTraining = new()
            {
                ProviderUkprn = "10044534",  // ARK Teacher Training
                ProgrammeStartDate = new(2020, 4, 1),
                ProgrammeEndDate = new(2020, 10, 10),
                ProgrammeType = dfeta_ITTProgrammeType.GraduateTeacherProgramme,
                IttQualificationAim = dfeta_ITTQualificationAim.Professionalstatusandacademicaward
            },
            SlugId = slugId
        };

        //teacher has ITT record with slugid that exists because it was created above
        var teachercommand2 = new CreateTeacherCommand()
        {
            FirstName = Faker.Name.First(),
            LastName = Faker.Name.Last(),
            BirthDate = Faker.Identification.DateOfBirth(),
            GenderCode = Contact_GenderCode.Female,
            InitialTeacherTraining = new()
            {
                ProviderUkprn = "10044534",  // ARK Teacher Training
                ProgrammeStartDate = new(2020, 4, 1),
                ProgrammeEndDate = new(2020, 10, 10),
                ProgrammeType = dfeta_ITTProgrammeType.GraduateTeacherProgramme,
                IttQualificationAim = dfeta_ITTQualificationAim.Professionalstatusandacademicaward

            },
            SlugId = slugId,
            TrnRequestId = Guid.NewGuid().ToString()
        };
        var (result1, transactionRequest1) = await _dataverseAdapter.CreateTeacherImplAsync(teachercommand1);

        // Act
        var (result2, transactionRequest2) = await _dataverseAdapter.CreateTeacherImplAsync(teachercommand2);

        // Assert
        Assert.True(result1.Succeeded);
        transactionRequest1.AssertDoesNotContainCreateRequest<CrmTask>();
        Assert.True(result2.Succeeded);
        transactionRequest2.AssertContainsCreateRequest<CrmTask>(x => x.Description.Contains($"- ITT SlugId: '{slugId}'"));
    }

    [Fact]
    public async Task Given_slugid_does_not_exist_request_succeeds_and_does_not_creates_review_task()
    {
        // Arrange
        var slugId = Guid.NewGuid().ToString();
        var teachercommand1 = new CreateTeacherCommand()
        {
            FirstName = Faker.Name.First(),
            LastName = Faker.Name.Last(),
            BirthDate = Faker.Identification.DateOfBirth(),
            GenderCode = Contact_GenderCode.Female,
            InitialTeacherTraining = new()
            {
                ProviderUkprn = "10044534",  // ARK Teacher Training
                ProgrammeStartDate = new(2020, 4, 1),
                ProgrammeEndDate = new(2020, 10, 10),
                ProgrammeType = dfeta_ITTProgrammeType.GraduateTeacherProgramme,
                IttQualificationAim = dfeta_ITTQualificationAim.Professionalstatusandacademicaward
            },
            SlugId = slugId
        };

        // Act
        var (result1, transactionRequest1) = await _dataverseAdapter.CreateTeacherImplAsync(teachercommand1);

        // Assert
        Assert.True(result1.Succeeded);
        transactionRequest1.AssertDoesNotContainCreateRequest<CrmTask>();
    }

    [Fact]
    public async Task Given_teacher_slugid_exists_request_succeeds_and_creates_review_task()
    {
        // Arrange
        var slugId = Guid.NewGuid().ToString();
        var teachercommand1 = new CreateTeacherCommand()
        {
            FirstName = Faker.Name.First(),
            LastName = Faker.Name.Last(),
            BirthDate = Faker.Identification.DateOfBirth(),
            GenderCode = Contact_GenderCode.Female,
            InitialTeacherTraining = new()
            {
                ProviderUkprn = "10044534",  // ARK Teacher Training
                ProgrammeStartDate = new(2020, 4, 1),
                ProgrammeEndDate = new(2020, 10, 10),
                ProgrammeType = dfeta_ITTProgrammeType.GraduateTeacherProgramme,
                IttQualificationAim = dfeta_ITTQualificationAim.Professionalstatusandacademicaward
            },
            SlugId = slugId
        };

        //teacher with slugid already exists because it was created above
        var teachercommand2 = new CreateTeacherCommand()
        {
            FirstName = Faker.Name.First(),
            LastName = Faker.Name.Last(),
            BirthDate = Faker.Identification.DateOfBirth(),
            GenderCode = Contact_GenderCode.Female,
            InitialTeacherTraining = new()
            {
                ProviderUkprn = "10044534",  // ARK Teacher Training
                ProgrammeStartDate = new(2020, 4, 1),
                ProgrammeEndDate = new(2020, 10, 10),
                ProgrammeType = dfeta_ITTProgrammeType.GraduateTeacherProgramme,
                IttQualificationAim = dfeta_ITTQualificationAim.Professionalstatusandacademicaward
            },
            SlugId = slugId
        };
        var (result1, transactionRequest1) = await _dataverseAdapter.CreateTeacherImplAsync(teachercommand1);

        // Act
        var (result2, transactionRequest2) = await _dataverseAdapter.CreateTeacherImplAsync(teachercommand2);

        // Assert
        Assert.True(result1.Succeeded);
        transactionRequest1.AssertDoesNotContainCreateRequest<CrmTask>();
        Assert.True(result2.Succeeded);
        transactionRequest2.AssertContainsCreateRequest<CrmTask>(x => x.Description.Contains($"- SlugId: '{slugId}'"));
    }

    [Fact]
    public async Task Given_husid_exists_request_succeeds_and_creates_review_task()
    {
        // Arrange
        var husid = Guid.NewGuid().ToString();
        var teachercommand1 = new CreateTeacherCommand()
        {
            FirstName = Faker.Name.First(),
            LastName = Faker.Name.Last(),
            BirthDate = Faker.Identification.DateOfBirth(),
            GenderCode = Contact_GenderCode.Female,
            InitialTeacherTraining = new()
            {
                ProviderUkprn = "10044534",  // ARK Teacher Training
                ProgrammeStartDate = new(2020, 4, 1),
                ProgrammeEndDate = new(2020, 10, 10),
                ProgrammeType = dfeta_ITTProgrammeType.GraduateTeacherProgramme,
                IttQualificationAim = dfeta_ITTQualificationAim.Professionalstatusandacademicaward
            },
            HusId = husid
        };

        //teacher with husid already exists because it was created above
        var teachercommand2 = new CreateTeacherCommand()
        {
            FirstName = Faker.Name.First(),
            LastName = Faker.Name.Last(),
            BirthDate = Faker.Identification.DateOfBirth(),
            GenderCode = Contact_GenderCode.Female,
            InitialTeacherTraining = new()
            {
                ProviderUkprn = "10044534",  // ARK Teacher Training
                ProgrammeStartDate = new(2020, 4, 1),
                ProgrammeEndDate = new(2020, 10, 10),
                ProgrammeType = dfeta_ITTProgrammeType.GraduateTeacherProgramme,
                IttQualificationAim = dfeta_ITTQualificationAim.Professionalstatusandacademicaward
            },
            HusId = husid
        };
        var (result1, transactionRequest1) = await _dataverseAdapter.CreateTeacherImplAsync(teachercommand1);

        // Act
        var (result2, transactionRequest2) = await _dataverseAdapter.CreateTeacherImplAsync(teachercommand2);

        // Assert
        Assert.True(result1.Succeeded);
        transactionRequest1.AssertDoesNotContainCreateRequest<CrmTask>();
        Assert.True(result2.Succeeded);
        transactionRequest2.AssertContainsCreateRequest<CrmTask>(x => x.Description.Contains($"- HusId: '{husid}'"));
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
            QualificationProviderId = Guid.NewGuid(),
            QualificationSubject2Id = Guid.NewGuid(),
            QualificationSubject3Id = Guid.NewGuid()
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
        Assert.Equal(referenceData.QualificationSubject2Id, result.dfeta_HE_HESubject2Id?.Id);
        Assert.Equal(dfeta_hesubject.EntityLogicalName, result.dfeta_HE_HESubject2Id?.LogicalName);
        Assert.Equal(referenceData.QualificationSubject3Id, result.dfeta_HE_HESubject3Id?.Id);
        Assert.Equal(dfeta_hesubject.EntityLogicalName, result.dfeta_HE_HESubject3Id?.LogicalName);
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
        var result = await helper.FindExistingTeacherAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Contains(existingTeacherId, result?.Select(x => x.TeacherId));
        Assert.Contains(expectedMatchedAttributes, result?.Select(x => x.MatchedAttributes));
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
        var command = CreateCommand(configureCommand: cmd =>
        {
            cmd.FirstName = firstName;
            cmd.MiddleName = middleName;
            cmd.LastName = lastName;
        });

        // Act
        var (result, transactionRequest) = await _dataverseAdapter.CreateTeacherImplAsync(command);

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
                ProgrammeType = dfeta_ITTProgrammeType.GraduateTeacherProgramme
            }
        };

        // Act
        var (result, _) = await _dataverseAdapter.CreateTeacherImplAsync(command);
        var getIttRecords = await _dataverseAdapter.GetInitialTeacherTrainingByTeacherAsync(
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

    [Fact]
    public async Task Given_invalid_IttQualificationValue_returns_failed()
    {
        // Arrange
        var command = CreateCommand(configureCommand: cmd => cmd.InitialTeacherTraining.IttQualificationValue = "xxx");

        // Act
        var (result, _) = await _dataverseAdapter.CreateTeacherImplAsync(command);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal(CreateTeacherFailedReasons.IttQualificationNotFound, result.FailedReasons);
    }

    [Fact]
    public async Task Given_invalid_HeQualificationValue_returns_failed()
    {
        // Arrange
        var command = CreateCommand(configureCommand: cmd => cmd.Qualification.HeQualificationValue = "xxx");

        // Act
        var (result, _) = await _dataverseAdapter.CreateTeacherImplAsync(command);

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal(CreateTeacherFailedReasons.QualificationNotFound, result.FailedReasons);
    }

    [Theory]
    [InlineData(CreateTeacherType.TraineeTeacher, null, null, dfeta_ITTProgrammeType.AssessmentOnlyRoute, "212")]
    [InlineData(CreateTeacherType.TraineeTeacher, null, null, dfeta_ITTProgrammeType.GraduateTeacherProgramme, "211")]
    [InlineData(CreateTeacherType.OverseasQualifiedTeacher, CreateTeacherRecognitionRoute.Scotland, null, null, "68")]
    [InlineData(CreateTeacherType.OverseasQualifiedTeacher, CreateTeacherRecognitionRoute.NorthernIreland, null, null, "69")]
    [InlineData(CreateTeacherType.OverseasQualifiedTeacher, CreateTeacherRecognitionRoute.EuropeanEconomicArea, null, null, "223")]
    [InlineData(CreateTeacherType.OverseasQualifiedTeacher, CreateTeacherRecognitionRoute.OverseasTrainedTeachers, false, null, "103")]
    [InlineData(CreateTeacherType.OverseasQualifiedTeacher, CreateTeacherRecognitionRoute.OverseasTrainedTeachers, true, null, "104")]
    public void DeriveTeacherStatus(
        CreateTeacherType teacherType,
        CreateTeacherRecognitionRoute? recognitionRoute,
        bool? underNewOverseasRegulations,
        dfeta_ITTProgrammeType? programmeType,
        string expectedTeacherStatus)
    {
        // Arrange
        var command = CreateCommand(
            teacherType,
            c =>
            {
                c.InitialTeacherTraining.ProgrammeType = programmeType;
                c.RecognitionRoute = recognitionRoute;
                c.UnderNewOverseasRegulations = underNewOverseasRegulations;
            });

        var helper = new DataverseAdapter.CreateTeacherHelper(_dataverseAdapter, command);

        // Act
        var result = helper.DeriveTeacherStatus(out _);

        // Assert
        Assert.Equal(expectedTeacherStatus, result);
    }

    [Theory]
    [InlineData(CreateTeacherRecognitionRoute.Scotland, "UK establishment (Scotland/Northern Ireland)")]
    [InlineData(CreateTeacherRecognitionRoute.NorthernIreland, "UK establishment (Scotland/Northern Ireland)")]
    [InlineData(CreateTeacherRecognitionRoute.OverseasTrainedTeachers, "Non-UK establishment")]
    public void DeriveIttProviderNameForOverseasQualifiedTeacher(
        CreateTeacherRecognitionRoute recognitionRoute,
        string expectedIttProviderName)
    {
        // Arrange
        var command = CreateCommand(
            CreateTeacherType.OverseasQualifiedTeacher,
            c => c.RecognitionRoute = recognitionRoute);

        var helper = new DataverseAdapter.CreateTeacherHelper(_dataverseAdapter, command);

        // Act
        var result = helper.DeriveIttProviderNameForOverseasQualifiedTeacher();

        // Assert
        Assert.Equal(expectedIttProviderName, result);
    }

    [Theory]
    [InlineData(true, CreateTeacherRecognitionRoute.Scotland, InductionStatus.RequiredToComplete, null)]
    [InlineData(true, CreateTeacherRecognitionRoute.NorthernIreland, InductionStatus.RequiredToComplete, null)]
    [InlineData(true, CreateTeacherRecognitionRoute.OverseasTrainedTeachers, InductionStatus.RequiredToComplete, null)]
    [InlineData(false, CreateTeacherRecognitionRoute.Scotland, InductionStatus.Exempt, "a112e691-1694-46a7-8f33-5ec5b845c181")]
    [InlineData(false, CreateTeacherRecognitionRoute.NorthernIreland, InductionStatus.Exempt, "3471ab35-e6e4-4fa9-a72b-b8bd113df591")]
    [InlineData(false, CreateTeacherRecognitionRoute.OverseasTrainedTeachers, InductionStatus.Exempt, "4c97e211-10d2-4c63-8da9-b0fcebe7f2f9")]
    public void CreateSetInductionOutboxMessage(
        bool inductionRequired,
        CreateTeacherRecognitionRoute recognitionRoute,
        InductionStatus expectedInductionStatus,
        string expectedInductionExemptionReasonIdStr)
    {
        // Arrange
        Guid? expectedInductionExemptionReasonId =
            expectedInductionExemptionReasonIdStr is string id ? Guid.Parse(id) : null;

        var command = CreateCommand(
            CreateTeacherType.OverseasQualifiedTeacher,
            c =>
            {
                c.InductionRequired = inductionRequired;
                c.RecognitionRoute = recognitionRoute;
            });

        var helper = new DataverseAdapter.CreateTeacherHelper(_dataverseAdapter, command);

        // Act
        var result = helper.CreateSetInductionOutboxMessage();

        // Assert
        var messageSerializer = new MessageSerializer();

        if (expectedInductionStatus is InductionStatus.Exempt)
        {
            Assert.Equal(nameof(AddInductionExemptionMessage), result.dfeta_MessageName);
            var message = Assert.IsType<AddInductionExemptionMessage>(messageSerializer.DeserializeMessage(result.dfeta_Payload, result.dfeta_MessageName));
            Assert.Equal(helper.TeacherId, message.PersonId);
            Assert.Equal(expectedInductionExemptionReasonId, message.ExemptionReasonId);
        }
        else
        {
            Debug.Assert(expectedInductionStatus is InductionStatus.RequiredToComplete);

            Assert.Equal(nameof(SetInductionRequiredToCompleteMessage), result.dfeta_MessageName);
            var message = Assert.IsType<SetInductionRequiredToCompleteMessage>(messageSerializer.DeserializeMessage(result.dfeta_Payload, result.dfeta_MessageName));
            Assert.Equal(helper.TeacherId, message.PersonId);
        }
    }

    private static CreateTeacherCommand CreateCommand(
        CreateTeacherType type = CreateTeacherType.TraineeTeacher,
        Action<CreateTeacherCommand> configureCommand = null)
    {
        var firstName1 = Faker.Name.First();
        var firstName2 = Faker.Name.First();
        var middleName = Faker.Name.Middle();
        var lastName = Faker.Name.Last();
        var slugId = Guid.NewGuid().ToString();

        var command = new CreateTeacherCommand()
        {
            FirstName = firstName1,
            MiddleName = $"{firstName2} {middleName}",
            LastName = lastName,
            StatedFirstName = $"{firstName1} {firstName2}",
            StatedMiddleName = middleName,
            StatedLastName = lastName,
            BirthDate = Faker.Identification.DateOfBirth(),
            EmailAddress = Faker.Internet.Email(),
            SlugId = slugId,
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
                ProviderUkprn = type == CreateTeacherType.TraineeTeacher ?
                    "10044534" :  // ARK Teacher Training
                    null,
                ProgrammeStartDate = new(2020, 4, 1),
                ProgrammeEndDate = new(2020, 10, 10),
                ProgrammeType = dfeta_ITTProgrammeType.GraduateTeacherProgramme,
                Subject1 = "100366",  // computer science
                Subject2 = "100403",  // mathematics
                Subject3 = "100302",  // history
                AgeRangeFrom = dfeta_AgeRange._05,
                AgeRangeTo = dfeta_AgeRange._11,
                IttQualificationValue = "001",  // BEd
                IttQualificationAim = dfeta_ITTQualificationAim.Professionalstatusandacademicaward,
                TrainingCountryCode = type == CreateTeacherType.OverseasQualifiedTeacher ?
                    "SC" :  // Scotland
                    null
            },
            Qualification = new()
            {
                ProviderUkprn = "10044534",
                CountryCode = "XK",
                Subject = "100366",  // computer science
                Class = dfeta_classdivision.Firstclasshonours,
                Date = new(2021, 5, 3),
                HeQualificationValue = "401",  // Higher Degree,
                Subject2 = "H6601", //Radio Technology
                Subject3 = "V1030" //Regional History
            },
            TeacherType = type,
            InductionRequired = type == CreateTeacherType.OverseasQualifiedTeacher ? false : null,
            QtsDate = type == CreateTeacherType.OverseasQualifiedTeacher ? new DateOnly(2020, 10, 10) : null,
            RecognitionRoute = type == CreateTeacherType.OverseasQualifiedTeacher ? CreateTeacherRecognitionRoute.Scotland : null,
            TrnRequestId = Guid.NewGuid().ToString(),
            ApplicationUserId = Guid.NewGuid()
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
