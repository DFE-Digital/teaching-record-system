using Microsoft.Extensions.Logging;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Jobs;
using TeachingRecordSystem.Core.Jobs.Scheduling;
using TeachingRecordSystem.Core.Services.PersonMatching;

namespace TeachingRecordSystem.Core.Tests.Jobs;

[Collection(nameof(DisableParallelization))]
public class AllocateTrnsToOverseasNpqApplicantsJobTests(AllocateTrnsToOverseasNpqApplicantsJobFixture fixture)
    : IClassFixture<AllocateTrnsToOverseasNpqApplicantsJobFixture>, IAsyncLifetime
{
    public const string InputCsvHeaders = "First Name (Required),Middle Name (Optional but preferable),Surname/Last Name (Required),Date of Birth (Required),PERSONAL Email Address (Required),NI Number (Optional but preferable),Nationality (Optional but preferable),Gender (Optional but preferable),Has the Participant started their NPQ or is confirmed to start in November 2025 (Y/N) Please Note -  This needs to be a yes for all applicants when this completed  list is returned (Required),TRN";
    public const string OutputCsvHeaders = "First Name (Required),Middle Name (Optional but preferable),Surname/Last Name (Required),Date of Birth (Required),PERSONAL Email Address (Required),NI Number (Optional but preferable),Nationality (Optional but preferable),Gender (Optional but preferable),Has the Participant started their NPQ or is confirmed to start in November 2025 (Y/N) Please Note -  This needs to be a yes for all applicants when this completed  list is returned (Required),Result,Errors,Allocated TRN,Potential duplicate TRNs";

    public TrsDbContext DbContext => fixture.DbContext;
    public DbFixture DbFixture => fixture.DbFixture;
    public TestData TestData => fixture.TestData;
    public TestableClock Clock => fixture.Clock;
    public TestFileStorageService FileStorageService => fixture.FileStorageService;
    public Mock<IBackgroundJobScheduler> BackgroundJobScheduler => fixture.BackgroundJobSchedulerMock;
    public FakeTrnGenerator TrnGenerator => fixture.TrnGenerator;

    public async Task InitializeAsync()
    {
        await DbFixture.DbHelper.ClearDataAsync();
        FileStorageService.Clear();
        BackgroundJobScheduler.Reset();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Execute_WhenInputFileDoesNotExist_OutputFileIsNotProduced()
    {
        // Arrange
        var job = CreateAllocateTrnsToOverseasNpqApplicantsJob();

        // Act
        await job.ExecuteAsync(CancellationToken.None);
        var file = FileStorageService.GetLastUploadedFile();

        // Assert
        Assert.Null(file);
    }

    [Fact]
    public async Task Execute_CreatesOutputFile()
    {
        // Arrange
        FileStorageService.WriteFile(AllocateTrnsToOverseasNpqApplicantsJob.ContainerName, $"{AllocateTrnsToOverseasNpqApplicantsJob.PendingFolderName}/import.csv",
            $"""
            {InputCsvHeaders}
            
            """);

        var job = CreateAllocateTrnsToOverseasNpqApplicantsJob();

        // Act
        await job.ExecuteAsync(CancellationToken.None);
        var file = FileStorageService.GetLastUploadedFile();

        // Assert
        Assert.NotNull(file);
        Assert.Equal(AllocateTrnsToOverseasNpqApplicantsJob.ContainerName, file.ContainerName);
        Assert.StartsWith($"{AllocateTrnsToOverseasNpqApplicantsJob.OutputFolderName}/{AllocateTrnsToOverseasNpqApplicantsJob.OutputFileNamePrefix}", file.FileName, StringComparison.InvariantCultureIgnoreCase);
        Assert.Equal(
            $"""
            {OutputCsvHeaders}
            
            """, file.Content, ignoreLineEndingDifferences: true);
    }

    [Fact]
    public async Task Execute_WhenInputFileIsEmpty_OutputFileIsEmptyWithHeaders()
    {
        // Arrange
        FileStorageService.WriteFile(AllocateTrnsToOverseasNpqApplicantsJob.ContainerName, $"{AllocateTrnsToOverseasNpqApplicantsJob.PendingFolderName}/import.csv",
            $"""
            {InputCsvHeaders}
            
            """);

        var job = CreateAllocateTrnsToOverseasNpqApplicantsJob();

        // Act
        await job.ExecuteAsync(CancellationToken.None);
        var file = FileStorageService.GetLastUploadedFile();

        // Assert
        Assert.NotNull(file);
        Assert.Equal(
            $"""
            {OutputCsvHeaders}
            
            """, file.Content, ignoreLineEndingDifferences: true);
    }

    [Fact]
    public async Task Execute_WhenInputFileIsEmptyWithHeaders_OutputFileIsEmptyWithHeaders()
    {
        // Arrange
        FileStorageService.WriteFile(AllocateTrnsToOverseasNpqApplicantsJob.ContainerName, $"{AllocateTrnsToOverseasNpqApplicantsJob.PendingFolderName}/import.csv",
            $"""
            {InputCsvHeaders}

            """);

        var job = CreateAllocateTrnsToOverseasNpqApplicantsJob();

        // Act
        await job.ExecuteAsync(CancellationToken.None);
        var file = FileStorageService.GetLastUploadedFile();

        // Assert
        Assert.NotNull(file);
        Assert.Equal(
            $"""
            {OutputCsvHeaders}

            """, file.Content, ignoreLineEndingDifferences: true);
    }

    [Fact]
    public async Task Execute_Validation_RequiredFields()
    {
        // Arrange
        FileStorageService.WriteFile(AllocateTrnsToOverseasNpqApplicantsJob.ContainerName, $"{AllocateTrnsToOverseasNpqApplicantsJob.PendingFolderName}/import.csv",
            $"""
            {InputCsvHeaders}
            ,,Smith,01/01/2001,jim.smith@email.com,,,,,
            Jim,,,01/01/2001,jim.smith@email.com,,,,,
            Jim,,Smith,,jim.smith@email.com,,,,,
            Jim,,Smith,01/01/2001,,,,,,
            ,,,,,,,,,

            """);

        var job = CreateAllocateTrnsToOverseasNpqApplicantsJob();

        // Act
        await job.ExecuteAsync(CancellationToken.None);
        var file = FileStorageService.GetLastUploadedFile();

        // Assert
        Assert.NotNull(file);
        Assert.Equal(
            $"""
            {OutputCsvHeaders}
            ,,Smith,01/01/2001,jim.smith@email.com,,,,,Validation errors,First Name is required,,
            Jim,,,01/01/2001,jim.smith@email.com,,,,,Validation errors,Last Name is required,,
            Jim,,Smith,,jim.smith@email.com,,,,,Validation errors,Date of Birth is required,,
            Jim,,Smith,01/01/2001,,,,,,Validation errors,Email Address is required,,
            ,,,,,,,,,Validation errors,"First Name is required,Last Name is required,Date of Birth is required,Email Address is required",,

            """, file.Content, ignoreLineEndingDifferences: true);
    }

    [Fact]
    public async Task Execute_WhenCsvHasIncorrectNumberOfFields_TreatsMissingFieldsAsEmptyValues()
    {
        // Arrange
        FileStorageService.WriteFile(AllocateTrnsToOverseasNpqApplicantsJob.ContainerName, $"{AllocateTrnsToOverseasNpqApplicantsJob.PendingFolderName}/import.csv",
            $"""
            {InputCsvHeaders}
            ,
            ,,
            ,,,
            ,,,,
            ,,,,,
            ,,,,,,
            ,,,,,,,
            ,,,,,,,,
            ,,,,,,,,,

            """);

        var job = CreateAllocateTrnsToOverseasNpqApplicantsJob();

        // Act
        await job.ExecuteAsync(CancellationToken.None);
        var file = FileStorageService.GetLastUploadedFile();

        // Assert
        Assert.NotNull(file);
        Assert.Equal(
            $"""
            {OutputCsvHeaders}
            ,,,,,,,,,Validation errors,"First Name is required,Last Name is required,Date of Birth is required,Email Address is required",,
            ,,,,,,,,,Validation errors,"First Name is required,Last Name is required,Date of Birth is required,Email Address is required",,
            ,,,,,,,,,Validation errors,"First Name is required,Last Name is required,Date of Birth is required,Email Address is required",,
            ,,,,,,,,,Validation errors,"First Name is required,Last Name is required,Date of Birth is required,Email Address is required",,
            ,,,,,,,,,Validation errors,"First Name is required,Last Name is required,Date of Birth is required,Email Address is required",,
            ,,,,,,,,,Validation errors,"First Name is required,Last Name is required,Date of Birth is required,Email Address is required",,
            ,,,,,,,,,Validation errors,"First Name is required,Last Name is required,Date of Birth is required,Email Address is required",,
            ,,,,,,,,,Validation errors,"First Name is required,Last Name is required,Date of Birth is required,Email Address is required",,
            ,,,,,,,,,Validation errors,"First Name is required,Last Name is required,Date of Birth is required,Email Address is required",,

            """, file.Content, ignoreLineEndingDifferences: true);
    }

    [Fact]
    public async Task Execute_Validation_DateOfBirth()
    {
        // Arrange
        FileStorageService.WriteFile(AllocateTrnsToOverseasNpqApplicantsJob.ContainerName, $"{AllocateTrnsToOverseasNpqApplicantsJob.PendingFolderName}/import.csv",
            $"""
            {InputCsvHeaders}
            Jim,,Smith,XXX,jim.smith@email.com
            Jim,,Smith,1/1/2001,jim.smith@email.com
            Jim,,Smith,01/01/01,jim.smith@email.com
            Jim,,Smith,01-01-2001,jim.smith@email.com
            Jim,,Smith,12/31/2001,jim.smith@email.com
            Jim,,Smith,1 Jan 2001,jim.smith@email.com
            Jim,,Smith,01/01/2001,jim.smith@email.com

            """);

        var job = CreateAllocateTrnsToOverseasNpqApplicantsJob();

        // Act
        await job.ExecuteAsync(CancellationToken.None);
        var file = FileStorageService.GetLastUploadedFile();

        // Assert
        Assert.NotNull(file);
        Assert.Equal(
            $"""
            {OutputCsvHeaders}
            Jim,,Smith,XXX,jim.smith@email.com,,,,,Validation errors,Date of Birth is in an incorrect format,,
            Jim,,Smith,1/1/2001,jim.smith@email.com,,,,,Validation errors,Date of Birth is in an incorrect format,,
            Jim,,Smith,01/01/01,jim.smith@email.com,,,,,Validation errors,Date of Birth is in an incorrect format,,
            Jim,,Smith,01-01-2001,jim.smith@email.com,,,,,Validation errors,Date of Birth is in an incorrect format,,
            Jim,,Smith,12/31/2001,jim.smith@email.com,,,,,Validation errors,Date of Birth is in an incorrect format,,
            Jim,,Smith,1 Jan 2001,jim.smith@email.com,,,,,Validation errors,Date of Birth is in an incorrect format,,
            Jim,,Smith,01/01/2001,jim.smith@email.com,,,,,TRN allocated,,{TrnGenerator.LastGeneratedTrn},
            
            """, file.Content, ignoreLineEndingDifferences: true);
    }

    [Fact]
    public async Task Execute_Validation_EmailAddress()
    {
        // Arrange
        FileStorageService.WriteFile(AllocateTrnsToOverseasNpqApplicantsJob.ContainerName, $"{AllocateTrnsToOverseasNpqApplicantsJob.PendingFolderName}/import.csv",
            $"""
            {InputCsvHeaders}
            Jim,,Smith,01/01/2001,email
            Jim,,Smith,01/01/2001,.com
            Jim,,Smith,01/01/2001,email.com
            Jim,,Smith,01/01/2001,jim@email
            Jim,,Smith,01/01/2001,jim@.com
            Jim,,Smith,01/01/2001,jim@email.com

            """);

        var job = CreateAllocateTrnsToOverseasNpqApplicantsJob();

        // Act
        await job.ExecuteAsync(CancellationToken.None);
        var file = FileStorageService.GetLastUploadedFile();

        // Assert
        Assert.NotNull(file);
        Assert.Equal(
            $"""
            {OutputCsvHeaders}
            Jim,,Smith,01/01/2001,email,,,,,Validation errors,Email Address is in an incorrect format,,
            Jim,,Smith,01/01/2001,.com,,,,,Validation errors,Email Address is in an incorrect format,,
            Jim,,Smith,01/01/2001,email.com,,,,,Validation errors,Email Address is in an incorrect format,,
            Jim,,Smith,01/01/2001,jim@email,,,,,Validation errors,Email Address is in an incorrect format,,
            Jim,,Smith,01/01/2001,jim@.com,,,,,Validation errors,Email Address is in an incorrect format,,
            Jim,,Smith,01/01/2001,jim@email.com,,,,,TRN allocated,,{TrnGenerator.LastGeneratedTrn},
            
            """, file.Content, ignoreLineEndingDifferences: true);
    }

    [Fact]
    public async Task Execute_Validation_NINumber()
    {
        // Arrange
        FileStorageService.WriteFile(AllocateTrnsToOverseasNpqApplicantsJob.ContainerName, $"{AllocateTrnsToOverseasNpqApplicantsJob.PendingFolderName}/import.csv",
            $"""
            {InputCsvHeaders}
            Jim,,Smith,01/01/2001,jim@email.com,XXX
            Jim,,Smith,01/01/2001,jim@email.com,AB123456
            Jim,,Smith,01/01/2001,jim@email.com,123456C
            Jim,,Smith,01/01/2001,jim@email.com,QV123456E
            Jim,,Smith,01/01/2001,jim@email.com,AB123456C
            Jim,,Smith,01/01/2001,jim@email.com,AB 12 34 56 C

            """);

        var job = CreateAllocateTrnsToOverseasNpqApplicantsJob();

        // Act
        await job.ExecuteAsync(CancellationToken.None);
        var file = FileStorageService.GetLastUploadedFile();

        // Assert
        Assert.NotNull(file);
        Assert.Equal(
            $"""
            {OutputCsvHeaders}
            Jim,,Smith,01/01/2001,jim@email.com,XXX,,,,Validation errors,NI Number is in an incorrect format,,
            Jim,,Smith,01/01/2001,jim@email.com,AB123456,,,,Validation errors,NI Number is in an incorrect format,,
            Jim,,Smith,01/01/2001,jim@email.com,123456C,,,,Validation errors,NI Number is in an incorrect format,,
            Jim,,Smith,01/01/2001,jim@email.com,QV123456E,,,,Validation errors,NI Number is in an incorrect format,,
            Jim,,Smith,01/01/2001,jim@email.com,AB123456C,,,,TRN allocated,,{TrnGenerator.LastGeneratedTrn - 1},
            Jim,,Smith,01/01/2001,jim@email.com,AB 12 34 56 C,,,,TRN allocated,,{TrnGenerator.LastGeneratedTrn},
            
            """, file.Content, ignoreLineEndingDifferences: true);
    }

    [Fact]
    public async Task Execute_Validation_Gender()
    {
        // Arrange
        FileStorageService.WriteFile(AllocateTrnsToOverseasNpqApplicantsJob.ContainerName, $"{AllocateTrnsToOverseasNpqApplicantsJob.PendingFolderName}/import.csv",
            $"""
            {InputCsvHeaders}
            Jim,,Smith,01/01/2001,jim@email.com,,,XXX
            Jim,,Smith,01/01/2001,jim@email.com,,,male
            Jim,,Smith,01/01/2001,jim@email.com,,,female
            Jim,,Smith,01/01/2001,jim@email.com,,,other
            Jim,,Smith,01/01/2001,jim@email.com,,,Male
            Jim,,Smith,01/01/2001,jim@email.com,,,Female
            Jim,,Smith,01/01/2001,jim@email.com,,,Other

            """);

        var job = CreateAllocateTrnsToOverseasNpqApplicantsJob();

        // Act
        await job.ExecuteAsync(CancellationToken.None);
        var file = FileStorageService.GetLastUploadedFile();

        // Assert
        Assert.NotNull(file);
        Assert.Equal(
            $"""
            {OutputCsvHeaders}
            Jim,,Smith,01/01/2001,jim@email.com,,,XXX,,Validation errors,Gender is in an incorrect format,,
            Jim,,Smith,01/01/2001,jim@email.com,,,male,,Validation errors,Gender is in an incorrect format,,
            Jim,,Smith,01/01/2001,jim@email.com,,,female,,Validation errors,Gender is in an incorrect format,,
            Jim,,Smith,01/01/2001,jim@email.com,,,other,,Validation errors,Gender is in an incorrect format,,
            Jim,,Smith,01/01/2001,jim@email.com,,,Male,,TRN allocated,,{TrnGenerator.LastGeneratedTrn - 2},
            Jim,,Smith,01/01/2001,jim@email.com,,,Female,,TRN allocated,,{TrnGenerator.LastGeneratedTrn - 1},
            Jim,,Smith,01/01/2001,jim@email.com,,,Other,,TRN allocated,,{TrnGenerator.LastGeneratedTrn},
            
            """, file.Content, ignoreLineEndingDifferences: true);
    }

    [Fact]
    public async Task Execute_WhenSingleExistingPersonMatchesRow_OutputsTrnAsPotentialDuplicate()
    {
        // Arrange
        var person1 = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Jim")
            .WithLastName("Smith")
            .WithDateOfBirth(DateOnly.Parse("01/01/2001")));

        FileStorageService.WriteFile(AllocateTrnsToOverseasNpqApplicantsJob.ContainerName, $"{AllocateTrnsToOverseasNpqApplicantsJob.PendingFolderName}/import.csv",
            $"""
            {InputCsvHeaders}
            Jim,,Smith,01/01/2001,jim.smith@email.com,,,,Y,,,,

            """);

        var job = CreateAllocateTrnsToOverseasNpqApplicantsJob();

        // Act
        await job.ExecuteAsync(CancellationToken.None);
        var file = FileStorageService.GetLastUploadedFile();

        // Assert
        Assert.NotNull(file);
        Assert.Equal(
            $"""
            {OutputCsvHeaders}
            Jim,,Smith,01/01/2001,jim.smith@email.com,,,,Y,TRN not allocated due to potential duplicates,,,{person1.Trn}

            """, file.Content, ignoreLineEndingDifferences: true);
    }

    [Fact]
    public async Task Execute_WhenMultipleExistingPersonsMatchRow_OutputsTrnsAsPotentialDuplicates()
    {
        // Arrange
        var person1 = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Bob")
            .WithLastName("Jones")
            .WithDateOfBirth(DateOnly.Parse("01/01/2001"))
            .WithEmailAddress("jim.smith@email.com"));

        var person2 = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Jim")
            .WithLastName("Smith")
            .WithDateOfBirth(DateOnly.Parse("01/01/2001"))
            .WithEmailAddress("jim.smith@email.com"));

        var person3 = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Fred")
            .WithLastName("Smith")
            .WithDateOfBirth(DateOnly.Parse("01/01/2001"))
            .WithEmailAddress("fred.smith@email.com"));

        var person4 = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Billy")
            .WithLastName("Bragg")
            .WithDateOfBirth(DateOnly.Parse("01/01/2001"))
            .WithEmailAddress("billy.bragg@email.com")
            .WithNationalInsuranceNumber("AB123456D"));

        FileStorageService.WriteFile(AllocateTrnsToOverseasNpqApplicantsJob.ContainerName, $"{AllocateTrnsToOverseasNpqApplicantsJob.PendingFolderName}/import.csv",
            $"""
            {InputCsvHeaders}
            Jim,,Smith,01/01/2001,jim.smith@email.com,AB123456D,,,Y,,,,

            """);

        var job = CreateAllocateTrnsToOverseasNpqApplicantsJob();

        // Act
        await job.ExecuteAsync(CancellationToken.None);
        var file = FileStorageService.GetLastUploadedFile();

        // Assert
        Assert.NotNull(file);
        Assert.Equal(
            $"""
            {OutputCsvHeaders}
            Jim,,Smith,01/01/2001,jim.smith@email.com,AB123456D,,,Y,TRN not allocated due to potential duplicates,,,"{person1.Trn},{person2.Trn},{person4.Trn}"

            """, file.Content, ignoreLineEndingDifferences: true);
    }

    [Fact]
    public async Task Execute_WhenExistingPersonDoesNotMatchRow_AllocatesTrnCreatesPersonAndSendsEmail()
    {
        // Arrange
        var existingPerson = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Jim")
            .WithLastName("Smith")
            .WithDateOfBirth(DateOnly.Parse("01/01/2001")));

        FileStorageService.WriteFile(AllocateTrnsToOverseasNpqApplicantsJob.ContainerName, $"{AllocateTrnsToOverseasNpqApplicantsJob.PendingFolderName}/import.csv",
            $"""
            {InputCsvHeaders}
            Bob,Middle,Jones,02/02/2002,bob.jones@email.com,AB123456D,British,Male,Y

            """);

        var job = CreateAllocateTrnsToOverseasNpqApplicantsJob();

        // Act
        await job.ExecuteAsync(CancellationToken.None);
        var file = FileStorageService.GetLastUploadedFile();

        // Assert
        Assert.NotNull(file);
        Assert.Equal(
            $"""
            {OutputCsvHeaders}
            Bob,Middle,Jones,02/02/2002,bob.jones@email.com,AB123456D,British,Male,Y,TRN allocated,,{TrnGenerator.LastGeneratedTrn},

            """, file.Content, ignoreLineEndingDifferences: true);

        var person = await DbContext.Persons
            .SingleAsync(p => p.Trn == TrnGenerator.LastGeneratedTrn.ToString());

        Assert.Equal("Bob", person.FirstName);
        Assert.Equal("Middle", person.MiddleName);
        Assert.Equal("Jones", person.LastName);
        Assert.Equal(DateOnly.Parse("02/02/2002"), person.DateOfBirth);
        Assert.Equal("bob.jones@email.com", person.EmailAddress);
        Assert.Equal("AB123456D", person.NationalInsuranceNumber);
        Assert.Equal(Gender.Male, person.Gender);
        Assert.Equal(ApplicationUser.NpqApplicationUserGuid, person.SourceApplicationUserId);

        var metadata = await DbContext.TrnRequestMetadata
            .SingleAsync(m => m.ApplicationUserId == ApplicationUser.NpqApplicationUserGuid && m.RequestId == person.SourceTrnRequestId);

        Assert.Equal("Bob", metadata.FirstName);
        Assert.Equal("Middle", metadata.MiddleName);
        Assert.Equal("Jones", metadata.LastName);
        Assert.Equivalent(new[] { "Bob", "Middle", "Jones" }, metadata.Name);
        Assert.Equal(DateOnly.Parse("02/02/2002"), metadata.DateOfBirth);
        Assert.Equal("bob.jones@email.com", metadata.EmailAddress);
        Assert.Equal("AB123456D", metadata.NationalInsuranceNumber);
        Assert.Equal(Gender.Male, metadata.Gender);

        var events = await DbContext.Events
            .Where(e => e.PersonId == person.PersonId).ToArrayAsync();
        var payloads = events.Select(e => e.ToEventBase());
        var personCreatedEvent = payloads.OfType<TeachingRecordSystem.Core.Events.Legacy.PersonCreatedEvent>().FirstOrDefault();

        Assert.NotNull(personCreatedEvent);
        Assert.Equal("Bob", personCreatedEvent.PersonAttributes.FirstName);
        Assert.Equal("Middle", personCreatedEvent.PersonAttributes.MiddleName);
        Assert.Equal("Jones", personCreatedEvent.PersonAttributes.LastName);
        Assert.Equal(DateOnly.Parse("02/02/2002"), personCreatedEvent.PersonAttributes.DateOfBirth);
        Assert.Equal("bob.jones@email.com", personCreatedEvent.PersonAttributes.EmailAddress);
        Assert.Equal("AB123456D", personCreatedEvent.PersonAttributes.NationalInsuranceNumber);
        Assert.Equal(Gender.Male, personCreatedEvent.PersonAttributes.Gender);

        var email = await DbContext.Emails.SingleAsync(e => e.EmailAddress == "bob.jones@email.com");
        Assert.NotNull(email);
        Assert.Equal(EmailTemplateIds.TrnGeneratedForNpq, email.TemplateId);
        Assert.Equal(person.FirstName, email.Personalization["first name"]);
        Assert.Equal(person.LastName, email.Personalization["last name"]);
        Assert.Equal(person.Trn, email.Personalization["trn"].ToString());

        BackgroundJobScheduler
            .Verify(
                s => s.EnqueueAsync(
                    It.IsAny<System.Linq.Expressions.Expression<Func<SendEmailJob, Task>>>()),
                Times.Once);
    }

    [Fact]
    public async Task Execute_RowsProcessedIndependently()
    {
        // Arrange
        var person1 = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Jim")
            .WithLastName("Smith")
            .WithDateOfBirth(DateOnly.Parse("01/01/2001"))
            .WithNationalInsuranceNumber("AB123456D"));

        var person2 = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Bob")
            .WithLastName("Smith")
            .WithDateOfBirth(DateOnly.Parse("01/01/2001"))
            .WithNationalInsuranceNumber("AB123456D"));

        FileStorageService.WriteFile(AllocateTrnsToOverseasNpqApplicantsJob.ContainerName, $"{AllocateTrnsToOverseasNpqApplicantsJob.PendingFolderName}/import.csv",
            $"""
            {InputCsvHeaders}
            Jim,,Smith,XXX,jim.smith@email.com
            Bob,Middle,Jones,02/02/2002,bob.jones@email.com,AB123456C,British,Male,Y
            Jim,,,01/01/2001,jim.smith@email.com
            Jim,,Smith,01/01/2001,jim.smith@email.com,AB123456D

            """);

        var job = CreateAllocateTrnsToOverseasNpqApplicantsJob();

        // Act
        await job.ExecuteAsync(CancellationToken.None);
        var file = FileStorageService.GetLastUploadedFile();

        // Assert
        Assert.NotNull(file);
        Assert.Equal(
            $"""
            {OutputCsvHeaders}
            Jim,,Smith,XXX,jim.smith@email.com,,,,,Validation errors,Date of Birth is in an incorrect format,,
            Bob,Middle,Jones,02/02/2002,bob.jones@email.com,AB123456C,British,Male,Y,TRN allocated,,{TrnGenerator.LastGeneratedTrn},
            Jim,,,01/01/2001,jim.smith@email.com,,,,,Validation errors,Last Name is required,,
            Jim,,Smith,01/01/2001,jim.smith@email.com,AB123456D,,,,TRN not allocated due to potential duplicates,,,"{person1.Trn},{person2.Trn}"

            """, file.Content, ignoreLineEndingDifferences: true);
    }

    private AllocateTrnsToOverseasNpqApplicantsJob CreateAllocateTrnsToOverseasNpqApplicantsJob() =>
        new AllocateTrnsToOverseasNpqApplicantsJob(
            DbContext,
            fixture.PersonMatchingService,
            BackgroundJobScheduler.Object,
            TrnGenerator,
            FileStorageService,
            Clock);
}

public class AllocateTrnsToOverseasNpqApplicantsJobFixture : IAsyncLifetime
{
    public AllocateTrnsToOverseasNpqApplicantsJobFixture(
        DbFixture dbFixture,
        ReferenceDataCache referenceDataCache,
        FakeTrnGenerator trnGenerator,
        ILoggerFactory loggerFactory)
    {
        DbFixture = dbFixture;
        LoggerFactory = loggerFactory;
        Clock = new();

        TestData = new TestData(
            dbFixture.GetDbContextFactory(),
            referenceDataCache,
            Clock,
            trnGenerator);

        DbContext = dbFixture.GetDbContextFactory().CreateDbContext();
        TrnGenerator = trnGenerator;
    }

    public TestableClock Clock { get; }
    public DbFixture DbFixture { get; }
    public ILoggerFactory LoggerFactory { get; }
    public TestData TestData { get; }
    public FakeTrnGenerator TrnGenerator { get; }
    public TestFileStorageService FileStorageService { get; } = new();
    public TrsDbContext DbContext { get; }
    public PersonMatchingService PersonMatchingService => new PersonMatchingService(DbContext);
    public Mock<IBackgroundJobScheduler> BackgroundJobSchedulerMock { get; } = new Mock<IBackgroundJobScheduler>();

    public async Task InitializeAsync()
    {
        await DbFixture.DbHelper.ClearDataAsync();
    }

    public async Task DisposeAsync() => await DbContext.DisposeAsync();
}
