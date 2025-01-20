using Azure.Storage.Blobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.Core.Jobs.EWCWalesImport;
using TeachingRecordSystem.Core.Services.TrsDataSync;

namespace TeachingRecordSystem.Core.Tests.Jobs;

[Collection(nameof(DisableParallelization))]
public class InductionImporterTests : IAsyncLifetime
{
    public InductionImporterTests(
      DbFixture dbFixture,
      IOrganizationServiceAsync2 organizationService,
      ReferenceDataCache referenceDataCache,
      FakeTrnGenerator trnGenerator,
      IServiceProvider provider,
      ILoggerFactory loggerFactory)
    {
        DbFixture = dbFixture;
        Clock = new();
        Helper = new TrsDataSyncHelper(
            dbFixture.GetDataSource(),
            organizationService,
            referenceDataCache,
            Clock,
            new TestableAuditRepository(),
            loggerFactory.CreateLogger<TrsDataSyncHelper>());

        TestData = new TestData(
            dbFixture.GetDbContextFactory(),
            organizationService,
            referenceDataCache,
            Clock,
            trnGenerator,
            TestDataSyncConfiguration.Sync(Helper));
        var blobServiceClient = new Mock<BlobServiceClient>();
        Importer = ActivatorUtilities.CreateInstance<InductionImporter>(provider);
    }
    private DbFixture DbFixture { get; }

    private TestData TestData { get; }

    private TestableClock Clock { get; }

    public TrsDataSyncHelper Helper { get; }

    Task IAsyncLifetime.InitializeAsync() => DbFixture.WithDbContextAsync(dbContext => dbContext.Events.ExecuteDeleteAsync());

    Task IAsyncLifetime.DisposeAsync() => Task.CompletedTask;

    public InductionImporter Importer { get; }

    [Fact]
    public async Task Validate_MissingReferenceNumber_ReturnsError()
    {
        // Arrange
        var row = GetDefaultRow();
        var lookups = await Importer.GetLookupDataAsync(row);

        // Act
        var (failures, errors) = Importer.Validate(row, lookups);

        // Assert
        Assert.Contains(errors, item => item.Contains($"Missing Reference No"));
    }

    [Fact]
    public async Task Validate_MissingDateOfBirth_ReturnsError()
    {
        // Arrange
        var row = GetDefaultRow(x =>
        {
            x.DateOfBirth = "";
            return x;
        });
        var lookups = await Importer.GetLookupDataAsync(row);

        // Act
        var (failures, errors) = Importer.Validate(row, lookups);

        // Assert
        Assert.Contains(errors, item => item.Contains($"Missing Date of Birth"));
    }

    [Fact]
    public async Task Validate_InvalidDateOfBirth_ReturnsError()
    {
        // Arrange
        var row = GetDefaultRow(x =>
        {
            x.DateOfBirth = "45/11/19990";
            return x;
        });
        var lookups = await Importer.GetLookupDataAsync(row);

        // Act
        var (failures, errors) = Importer.Validate(row, lookups);

        // Assert
        Assert.Contains(errors, item => item.Contains($"Validation Failed: Invalid Date of Birth"));
    }

    [Fact]
    public async Task Validate_MissingStartDate_ReturnsError()
    {
        // Arrange
        var row = GetDefaultRow(x =>
        {
            x.StartDate = "";
            return x;
        });
        var lookups = await Importer.GetLookupDataAsync(row);

        // Act
        var (failures, errors) = Importer.Validate(row, lookups);

        // Assert
        Assert.Contains(errors, item => item.Contains($"Missing Induction Start date"));
    }

    [Fact]
    public async Task Validate_InvalidStartDate_ReturnsError()
    {
        // Arrange
        var row = GetDefaultRow(x =>
        {
            x.StartDate = "55/13/20001111";
            return x;
        });
        var lookups = await Importer.GetLookupDataAsync(row);

        // Act
        var (failures, errors) = Importer.Validate(row, lookups);

        // Assert
        Assert.Contains(errors, item => item.Contains($"Validation Failed: Invalid Induction start date"));
    }

    [Fact]
    public async Task Validate_MissinPassedDate_ReturnsError()
    {
        // Arrange
        var row = GetDefaultRow(x =>
        {
            x.PassedDate = "";
            return x;
        });
        var lookups = await Importer.GetLookupDataAsync(row);

        // Act
        var (failures, errors) = Importer.Validate(row, lookups);

        // Assert
        Assert.Contains(errors, item => item.Contains($"Missing Induction passed date"));
    }

    [Fact]
    public async Task Validate_InvalidPassedDate_ReturnsError()
    {
        // Arrange
        var row = GetDefaultRow(x =>
        {
            x.PassedDate = "25/13/20001";
            return x;
        });
        var lookups = await Importer.GetLookupDataAsync(row);

        // Act
        var (failures, errors) = Importer.Validate(row, lookups);

        // Assert
        Assert.Contains(errors, item => item.Contains($"Validation Failed: Invalid Induction passed date"));
    }

    [Fact]
    public async Task Validate_ReferenceNumberNotFound_ReturnsError()
    {
        // Arrange
        var row = GetDefaultRow(x =>
        {
            x.ReferenceNumber = "NONE EXISTENT";
            return x;
        });
        var lookups = await Importer.GetLookupDataAsync(row);

        // Act
        var (failures, errors) = Importer.Validate(row, lookups);

        // Assert
        Assert.Contains(errors, item => item.Contains($"Teacher with TRN {row.ReferenceNumber} was not found."));
    }

    [Fact]
    public async Task Validate_WithQtlsDateNoInduction_ReturnsError()
    {
        // Arrange
        var accountNumber = "1357";
        var account = await TestData.CreateAccountAsync(x =>
        {
            x.WithName("SomeName");
            x.WithAccountNumber(accountNumber);
        });
        var person = await TestData.CreatePersonAsync(x =>
        {
            x.WithQtlsDate(new DateOnly(2024, 01, 01));
        });
        var row = GetDefaultRow(x =>
        {
            x.ReferenceNumber = person.Trn!;
            x.DateOfBirth = person.DateOfBirth.ToString("dd/MM/yyyy");
            return x;
        });
        var lookups = await Importer.GetLookupDataAsync(row);

        // Act
        var (failures, errors) = Importer.Validate(row, lookups);

        // Assert
        Assert.Contains(errors, item => item.Contains($"may need to update either/both the 'TRA induction status' and 'Overall induction status"));
    }

    [Theory]
    [InlineData(dfeta_InductionStatus.Pass, null)]
    [InlineData(dfeta_InductionStatus.PassedinWales, null)]
    [InlineData(dfeta_InductionStatus.Exempt, dfeta_InductionExemptionReason.Exempt)]
    [InlineData(dfeta_InductionStatus.FailedinWales, null)]
    [InlineData(dfeta_InductionStatus.Fail, null)]
    [InlineData(dfeta_InductionStatus.InProgress, null)]
    public async Task Validate_WithCompletedInduction_ReturnsError(dfeta_InductionStatus inductionStatus, dfeta_InductionExemptionReason? inductionExemptionReason)
    {
        // Arrange
        var accountNumber = "1357";
        var account = await TestData.CreateAccountAsync(x =>
        {
            x.WithName("SomeName");
            x.WithAccountNumber(accountNumber);
        });
        var person = await TestData.CreatePersonAsync(x =>
        {
            x.WithDqtInduction(inductionStatus: inductionStatus, inductionExemptionReason: inductionExemptionReason, null, null, null, null, null);
        });
        var row = GetDefaultRow(x =>
        {
            x.ReferenceNumber = person.Trn!;
            x.DateOfBirth = person.DateOfBirth.ToString("dd/MM/yyyy");
            return x;
        });
        var lookups = await Importer.GetLookupDataAsync(row);

        // Act
        var (failures, errors) = Importer.Validate(row, lookups);

        // Assert
        Assert.Contains(errors, item => item.Contains($"Teacher with TRN {row.ReferenceNumber} completed induction already or is progress."));
    }

    [Theory]
    [InlineData(dfeta_InductionStatus.RequiredtoComplete, null, "12345")]
    [InlineData(dfeta_InductionStatus.NotYetCompleted, null, "212324")]
    public async Task Validate_WithoutEndDate_ReturnsNoErrors(dfeta_InductionStatus inductionStatus, dfeta_InductionExemptionReason? inductionExemptionReason, string accountNumber)
    {
        // Arrange
        var inductionPeriodStartDate = new DateOnly(2019, 01, 01);
        var inductionStartDate = new DateOnly(2019, 01, 01);
        var account = await TestData.CreateAccountAsync(x =>
        {
            x.WithName("SomeName");
            x.WithAccountNumber(accountNumber);
        });
        var person = await TestData.CreatePersonAsync(x =>
        {
            x.WithDqtInduction(inductionStatus: inductionStatus,
                inductionExemptionReason: inductionExemptionReason,
                inductionStartDate: inductionStartDate,
                completedDate: null,
                inductionPeriodStartDate: inductionPeriodStartDate,
                inductionPeriodEndDate: null,
                appropriateBodyOrgId: account.Id,
                numberOfTerms: 1);
        });
        var row = GetDefaultRow(x =>
        {
            x.ReferenceNumber = person.Trn!;
            x.DateOfBirth = person.DateOfBirth.ToString("dd/MM/yyyy");
            x.EmployerCode = accountNumber;
            return x;
        });
        var lookups = await Importer.GetLookupDataAsync(row);

        // Act
        var (failures, errors) = Importer.Validate(row, lookups);

        // Assert
        Assert.Empty(errors);
        Assert.Empty(failures);
    }

    [Theory]
    [InlineData(dfeta_InductionStatus.RequiredtoComplete, null)]
    [InlineData(dfeta_InductionStatus.NotYetCompleted, null)]
    public async Task Validate_UpdateInductionPeriodForAnotherAppropriateBody_ReturnsError(dfeta_InductionStatus inductionStatus, dfeta_InductionExemptionReason? inductionExemptionReason)
    {
        // Arrange
        var accountNumber = "1357";
        var inductionPeriodStartDate = new DateOnly(2019, 01, 01);
        var inductionPeriodEndDate = new DateOnly(2019, 01, 01);
        var inductionStartDate = new DateOnly(2019, 01, 01);
        var account = await TestData.CreateAccountAsync(x =>
        {
            x.WithName("SomeName");
            x.WithAccountNumber(accountNumber);
        });
        var person = await TestData.CreatePersonAsync(x =>
        {
            x.WithDqtInduction(inductionStatus: inductionStatus,
                inductionExemptionReason: inductionExemptionReason,
                inductionStartDate: inductionStartDate,
                completedDate: null,
                inductionPeriodStartDate: inductionPeriodStartDate,
                inductionPeriodEndDate: null,
                appropriateBodyOrgId: account.Id,
                numberOfTerms: 1);
        });
        var row = GetDefaultRow(x =>
        {
            x.ReferenceNumber = person.Trn!;
            x.DateOfBirth = person.DateOfBirth.ToString("dd/MM/yyyy");
            return x;
        });
        var lookups = await Importer.GetLookupDataAsync(row);

        // Act
        var (failures, errors) = Importer.Validate(row, lookups);

        // Assert
        Assert.Contains(errors, item => item.Contains("Teacher is claimed by another Appropriate Body."));
    }

    [Theory]
    [InlineData(dfeta_InductionStatus.RequiredtoComplete, null)]
    [InlineData(dfeta_InductionStatus.NotYetCompleted, null)]
    public async Task Validate_UpdateInductionPeriodWithEndDate_ReturnsError(dfeta_InductionStatus inductionStatus, dfeta_InductionExemptionReason? inductionExemptionReason)
    {
        // Arrange
        var accountNumber = "1357";
        var inductionPeriodStartDate = new DateOnly(2019, 01, 01);
        var inductionPeriodEndDate = new DateOnly(2019, 01, 01);
        var inductionStartDate = new DateOnly(2019, 01, 01);
        var account = await TestData.CreateAccountAsync(x =>
        {
            x.WithName("SomeName");
            x.WithAccountNumber(accountNumber);
        });
        var person = await TestData.CreatePersonAsync(x =>
        {
            x.WithDqtInduction(inductionStatus: inductionStatus,
                inductionExemptionReason: inductionExemptionReason,
                inductionStartDate: inductionStartDate,
                completedDate: null,
                inductionPeriodStartDate: inductionPeriodStartDate,
                inductionPeriodEndDate: inductionPeriodEndDate,
                appropriateBodyOrgId: account.Id,
                numberOfTerms: 1);
        });
        var row = GetDefaultRow(x =>
        {
            x.ReferenceNumber = person.Trn!;
            x.DateOfBirth = person.DateOfBirth.ToString("dd/MM/yyyy");
            return x;
        });
        var lookups = await Importer.GetLookupDataAsync(row);

        // Act
        var (failures, errors) = Importer.Validate(row, lookups);

        // Assert
        Assert.Contains(errors, item => item.Contains("Unable to update induction period that has an end date."));
    }

    [Fact]
    public async Task GetLookupData_TrnDoesNotExist_ReturnsNoMatch()
    {
        // Arrange
        var accountNumber = "1357";
        var account = await TestData.CreateAccountAsync(x =>
        {
            x.WithName("SomeName");
            x.WithAccountNumber(accountNumber);
        });
        var person = await TestData.CreatePersonAsync();
        var row = GetDefaultRow(x =>
        {
            x.ReferenceNumber = "InvalidTrn";
            return x;
        });

        // Act
        var lookups = await Importer.GetLookupDataAsync(row);

        // Assert
        Assert.Equal(ContactLookupResult.NoMatch, lookups.PersonMatchStatus);
        Assert.Null(lookups.Person);
    }

    [Fact]
    public async Task GetLookupData_ValidTrnWithoutQTS_ReturnsNoAssociatedQTS()
    {
        // Arrange
        var accountNumber = "1357";
        var account = await TestData.CreateAccountAsync(x =>
        {
            x.WithName("SomeName");
            x.WithAccountNumber(accountNumber);
        });
        var person = await TestData.CreatePersonAsync(x => x.WithTrn());
        var row = GetDefaultRow(x =>
        {
            x.ReferenceNumber = person.Trn!;
            x.DateOfBirth = person.DateOfBirth.ToString("dd/MM/yyyy");
            return x;
        });

        // Act
        var lookups = await Importer.GetLookupDataAsync(row);

        // Assert
        Assert.Equal(ContactLookupResult.NoAssociatedQts, lookups.PersonMatchStatus);
        Assert.NotNull(lookups.Person);
        Assert.Equal(person.ContactId, lookups.Person!.ContactId);
    }

    [Fact]
    public async Task GetLookupData_ValidTrnWithQTS_ReturnsTeacherHasQTS()
    {
        // Arrange
        var accountNumber = "1357";
        var account = await TestData.CreateAccountAsync(x =>
        {
            x.WithName("SomeName");
            x.WithAccountNumber(accountNumber);
        });
        var person = await TestData.CreatePersonAsync(x =>
        {
            x.WithQts();
        });
        var row = GetDefaultRow(x =>
        {
            x.ReferenceNumber = person.Trn!;
            x.DateOfBirth = person.DateOfBirth.ToString("dd/MM/yyyy");
            return x;
        });

        // Act
        var lookups = await Importer.GetLookupDataAsync(row);

        // Assert
        Assert.Equal(ContactLookupResult.TeacherHasQts, lookups.PersonMatchStatus);
        Assert.NotNull(lookups.Person);
        Assert.Equal(person.ContactId, lookups.Person!.ContactId);
    }

    [Fact]
    public async Task GetLookupData_WithActiveAlert_ReturnsExpected()
    {
        // Arrange
        var accountNumber = "1357";
        var account = await TestData.CreateAccountAsync(x =>
        {
            x.WithName("SomeName");
            x.WithAccountNumber(accountNumber);
        });
        var person = await TestData.CreatePersonAsync(x =>
        {
            x.WithQts();
            x.WithAlert();
        });
        var row = GetDefaultRow(x =>
        {
            x.ReferenceNumber = person.Trn!;
            x.DateOfBirth = person.DateOfBirth.ToString("dd/MM/yyyy");
            return x;
        });

        // Act
        var lookups = await Importer.GetLookupDataAsync(row);

        // Assert
        Assert.True(lookups.HasActiveAlerts);
    }

    [Fact]
    public async Task GetLookupData_InvalidOrganisationCode_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(x =>
        {
            x.WithQts();
        });
        var row = GetDefaultRow(x =>
        {
            x.EmployerCode = "SOMEINVALID";
            return x;
        });

        // Act
        var lookups = await Importer.GetLookupDataAsync(row);

        // Assert
        Assert.Equal(OrganisationLookupResult.NoMatch, lookups.OrganisationMatchStatus);
        Assert.Null(lookups.OrganisationId);
    }

    private EwcWalesInductionImportData GetDefaultRow(Func<EwcWalesInductionImportData, EwcWalesInductionImportData>? configurator = null)
    {
        var row = new EwcWalesInductionImportData()
        {
            ReferenceNumber = "",
            FirstName = Faker.Name.First(),
            LastName = Faker.Name.First(),
            DateOfBirth = Faker.Identification.DateOfBirth().ToString(),
            StartDate = "10/10/2023",
            PassedDate = "10/10/2024",
            FailDate = "",
            EmployerCode = "",
            EmployerName = "",
            InductionStatusName = "01/07/2024",
        };
        var configuredRow = configurator != null ? configurator(row) : row;
        return configuredRow;
    }

}
