using Azure.Storage.Blobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using TeachingRecordSystem.Core.Jobs.EwcWalesImport;
using TeachingRecordSystem.Core.Services.TrsDataSync;

namespace TeachingRecordSystem.Core.Tests.Jobs;

[Collection(nameof(DisableParallelization))]
public class QtsImporterTests : IAsyncLifetime
{
    public QtsImporterTests(
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
        Importer = ActivatorUtilities.CreateInstance<QtsImporter>(provider);
    }

    private DbFixture DbFixture { get; }

    private TestData TestData { get; }

    private TestableClock Clock { get; }

    public TrsDataSyncHelper Helper { get; }

    Task IAsyncLifetime.InitializeAsync() => DbFixture.WithDbContextAsync(dbContext => dbContext.Events.ExecuteDeleteAsync());

    Task IAsyncLifetime.DisposeAsync() => Task.CompletedTask;

    public QtsImporter Importer { get; }

    [Fact]
    public async Task Validate_NoneExistentTeacher_ReturnsErrorMessage()
    {
        // Arrange
        var row = GetDefaultRow(x =>
        {
            x.QtsRefNo = "InvalidTrn";
            return x;
        });

        var lookups = await Importer.GetLookupDataAsync(row);

        // Act
        var (failures, errors) = Importer.Validate(row, lookups);

        // Assert
        Assert.Contains(errors, item => item.Contains($"Teacher with TRN {row.QtsRefNo} was not found."));
    }

    [Fact]
    public async Task Validate_ExistingTeacherWithQTS_ReturnsErrorMessage()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(x =>
        {
            x.WithQts();
        });
        var row = GetDefaultRow(x =>
        {
            x.QtsRefNo = person.Trn!;
            x.DateOfBirth = person.DateOfBirth.ToString("dd/MM/yyyy")!;
            return x;
        });
        var lookups = await Importer.GetLookupDataAsync(row);

        // Act
        var (failures, errors) = Importer.Validate(row, lookups);

        // Assert
        Assert.Contains(errors, item => item.Contains($"Teacher with TRN {row.QtsRefNo} has QTS already."));
    }

    [Fact]
    public async Task Validate_WithMissingMandatoryFields_ReturnsErrorMessages()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var row = GetDefaultRow(x =>
        {
            x.QtsRefNo = "";
            x.DateOfBirth = "";
            x.QtsDate = "";
            return x;
        });
        var lookups = await Importer.GetLookupDataAsync(row);

        // Act
        var (failures, errors) = Importer.Validate(row, lookups);

        // Assert
        Assert.Contains(errors, item => item.Contains("Missing QTS Ref Number"));
        Assert.Contains(errors, item => item.Contains("Missing Date of Birth"));
        Assert.Contains(errors, item => item.Contains("Misssing QTS Date"));
    }

    [Fact]
    public async Task Validate_WithMalformedDateOfBirth_ReturnsErrorMessages()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(x => x.WithTrn());
        var row = GetDefaultRow(x =>
        {
            x.QtsRefNo = person.Trn!;
            x.DateOfBirth = "67/13/2025";
            return x;
        });
        var lookups = await Importer.GetLookupDataAsync(row);

        // Act
        var (failures, errors) = Importer.Validate(row, lookups);

        // Assert
        Assert.Contains(errors, item => item.Contains("Validation Failed: Invalid Date of Birth"));
    }

    [Fact]
    public async Task Validate_WithMalformedQTSDate_ReturnsErrorMessages()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(x => x.WithTrn());
        var row = GetDefaultRow(x =>
        {
            x.QtsRefNo = person.Trn!;
            x.QtsDate = "67/13/2025";
            return x;
        });
        var lookups = await Importer.GetLookupDataAsync(row);

        // Act
        var (failures, errors) = Importer.Validate(row, lookups);

        // Assert
        Assert.Contains(errors, item => item.Contains("Validation Failed: Invalid QTS Date"));
    }

    [Fact]
    public async Task Validate_ExistingTeacherDateOfBirthDoesNotMatch_ReturnsErrorMessage()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(x => x.WithTrn());
        var row = GetDefaultRow(x =>
        {
            x.QtsRefNo = person.Trn!;
            x.DateOfBirth = "01/06/1999";
            return x;
        });
        var lookups = await Importer.GetLookupDataAsync(row);

        // Act
        var (failures, errors) = Importer.Validate(row, lookups);

        // Assert
        Assert.Contains(errors, item => item.Contains($"For TRN {row.QtsRefNo} Date of Birth does not match with the existing record."));
    }

    [Fact]
    public async Task Validate_MultipleMatchingOrganisations_ReturnsErrorMessage()
    {
        // Arrange
        var accountNumber = "12345";
        var account1 = await TestData.CreateAccountAsync(x =>
        {
            x.WithName("test");
            x.WithAccountNumber(accountNumber);
        });
        var account2 = await TestData.CreateAccountAsync(x =>
        {
            x.WithName("test2");
            x.WithAccountNumber(accountNumber);
        });
        var person = await TestData.CreatePersonAsync(x => x.WithTrn());
        var row = GetDefaultRow(x =>
        {
            x.QtsRefNo = person.Trn!;
            x.DateOfBirth = person.DateOfBirth.ToString("dd/MM/yyyy")!;
            x.IttEstabCode = accountNumber;
            return x;
        });
        var lookups = await Importer.GetLookupDataAsync(row);

        // Act
        var (failures, errors) = Importer.Validate(row, lookups);

        // Assert
        Assert.Contains(failures, item => item.Contains($"Multiple organisations with ITT Establishment Code {row.IttEstabCode} found."));
    }

    [Fact]
    public async Task Validate_NoMatchingOrganisations_ReturnsErrorMessage()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(x => x.WithTrn());
        var row = GetDefaultRow(x =>
        {
            x.QtsRefNo = person.Trn!;
            x.DateOfBirth = person.DateOfBirth.ToString("dd/MM/yyyy")!;
            x.IttEstabCode = "SomeInvalidOrg";
            return x;
        });
        var lookups = await Importer.GetLookupDataAsync(row);

        // Act
        var (failures, errors) = Importer.Validate(row, lookups);

        // Assert
        Assert.Contains(failures, item => item.Contains($"Organisation with ITT Establishment Code {row.IttEstabCode} was not found."));
    }

    [Fact]
    public async Task Validate_NoPqSubjects_ReturnsErrorMessage()
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
            x.QtsRefNo = person.Trn!;
            x.DateOfBirth = person.DateOfBirth.ToString("dd/MM/yyyy")!;
            x.IttEstabCode = account.AccountNumber;
            x.PqSubjectCode1 = "3333333";
            x.PqSubjectCode2 = "5555555";
            x.PqSubjectCode3 = "6666666";
            return x;
        });
        var lookups = await Importer.GetLookupDataAsync(row);

        // Act
        var (failures, errors) = Importer.Validate(row, lookups);

        // Assert
        Assert.Contains(failures, item => item.Contains($"Subject with PQ Subject Code {row.PqSubjectCode1} was not found."));
        Assert.Contains(failures, item => item.Contains($"Subject with PQ Subject Code {row.PqSubjectCode2} was not found."));
        Assert.Contains(failures, item => item.Contains($"Subject with PQ Subject Code {row.PqSubjectCode3} was not found."));
    }

    [Fact]
    public async Task Validate_ValidPqSubjects_DoesNotReturnsErrorMessage()
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
            x.QtsRefNo = person.Trn!;
            x.DateOfBirth = person.DateOfBirth.ToString("dd/MM/yyyy")!;
            x.IttEstabCode = account.AccountNumber;
            return x;
        });
        var lookups = await Importer.GetLookupDataAsync(row);

        // Act
        var (failures, errors) = Importer.Validate(row, lookups);

        // Assert
        Assert.DoesNotContain(failures, item => item.Contains($"Subject with PQ Subject Code {row.PqSubjectCode1} was not found."));
        Assert.DoesNotContain(failures, item => item.Contains($"Subject with PQ Subject Code {row.PqSubjectCode2} was not found."));
        Assert.DoesNotContain(failures, item => item.Contains($"Subject with PQ Subject Code {row.PqSubjectCode3} was not found."));
    }

    [Fact]
    public async Task Validate_ValidCountry_ReturnsErrorMessage()
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
            x.QtsRefNo = person.Trn!;
            x.DateOfBirth = person.DateOfBirth.ToString()!;
            x.IttEstabCode = account.AccountNumber;
            return x;
        });
        var lookups = await Importer.GetLookupDataAsync(row);

        // Act
        var (failures, errors) = Importer.Validate(row, lookups);

        // Assert
        Assert.DoesNotContain(failures, item => item.Contains($"Country with PQ Country Code {row.Country} was not found."));
    }

    [Fact]
    public async Task Validate_InvalidCountry_ReturnsErrorMessage()
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
            x.QtsRefNo = person.Trn!;
            x.DateOfBirth = person.DateOfBirth.ToString("dd/MM/yyyy")!;
            x.IttEstabCode = account.AccountNumber;
            x.Country = "INVALID";
            return x;
        });
        var lookups = await Importer.GetLookupDataAsync(row);

        // Act
        var (failures, errors) = Importer.Validate(row, lookups);

        // Assert
        Assert.Contains(failures, item => item.Contains($"Country with PQ Country Code {row.Country} was not found."));
    }

    [Fact]
    public async Task Validate_ValidITTSubjetCodes_DoesNotReturnErrorMessage()
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
            x.QtsRefNo = person.Trn!;
            x.DateOfBirth = person.DateOfBirth.ToString("dd/MM/yyyy")!;
            x.IttEstabCode = account.AccountNumber;
            return x;
        });
        var lookups = await Importer.GetLookupDataAsync(row);

        // Act
        var (failures, errors) = Importer.Validate(row, lookups);

        // Assert
        Assert.DoesNotContain(failures, item => item.Contains($"ITT subject with code {row.IttSubjectCode1} was not found."));
        Assert.DoesNotContain(failures, item => item.Contains($"ITT subject with code {row.IttSubjectCode2} was not found."));
    }

    [Fact]
    public async Task Validate_InvalidITTSubjetCodes_ReturnsErrorMessage()
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
            x.QtsRefNo = person.Trn!;
            x.DateOfBirth = person.DateOfBirth.ToString("dd/MM/yyyy")!;
            x.IttEstabCode = account.AccountNumber;
            x.IttSubjectCode1 = "Invalid1";
            x.IttSubjectCode2 = "Invalid2";
            return x;
        });
        var lookups = await Importer.GetLookupDataAsync(row);

        // Act
        var (failures, errors) = Importer.Validate(row, lookups);

        // Assert
        Assert.Contains(failures, item => item.Contains($"ITT subject with code {row.IttSubjectCode1} was not found."));
        Assert.Contains(failures, item => item.Contains($"ITT subject with code {row.IttSubjectCode2} was not found."));
    }

    [Fact]
    public async Task Validate_InvalidITTQualification_ReturnsErrorMessage()
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
            x.QtsRefNo = person.Trn!;
            x.DateOfBirth = person.DateOfBirth.ToString("dd/MM/yyyy")!;
            x.IttEstabCode = account.AccountNumber;
            x.IttQualCode = "InvalidIttQualCode";
            return x;
        });
        var lookups = await Importer.GetLookupDataAsync(row);

        // Act
        var (failures, errors) = Importer.Validate(row, lookups);

        // Assert
        Assert.Contains(failures, item => item.Contains($"TT qualification with code {row.IttQualCode} was not found."));
    }

    [Fact]
    public async Task Validate_ValidITTQualification_DoesNotReturnErrorMessage()
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
            x.QtsRefNo = person.Trn!;
            x.DateOfBirth = person.DateOfBirth.ToString("dd/MM/yyyy")!;
            x.IttEstabCode = account.AccountNumber;
            return x;
        });
        var lookups = await Importer.GetLookupDataAsync(row);

        // Act
        var (failures, errors) = Importer.Validate(row, lookups);

        // Assert
        Assert.DoesNotContain(failures, item => item.Contains($"ITT qualification with code {row.IttQualCode} was not found."));
    }

    [Fact]
    public async Task Validate_InvalidPqEstabCode_ReturnErrorMessage()
    {
        // Arrange
        var accountNumber = "13571";
        var account = await TestData.CreateAccountAsync(x =>
        {
            x.WithName("SomeName");
            x.WithAccountNumber(accountNumber);
        });
        var person = await TestData.CreatePersonAsync(x => x.WithTrn());
        var row = GetDefaultRow(x =>
        {
            x.QtsRefNo = person.Trn!;
            x.DateOfBirth = person.DateOfBirth.ToString("dd/MM/yyyy")!;
            x.PqEstabCode = "InvalidOrg";
            return x;
        });
        var lookups = await Importer.GetLookupDataAsync(row);

        // Act
        var (failures, errors) = Importer.Validate(row, lookups);

        // Assert
        Assert.Contains(failures, item => item.Contains($"Organisation with PQ Establishment Code {row.PqEstabCode} was not found."));
    }

    [Fact]
    public async Task Validate_ValidPqEstabCode_DoesNotReturnErrorMessage()
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
            x.QtsRefNo = person.Trn!;
            x.IttEstabCode = account.AccountNumber;
            x.DateOfBirth = person.DateOfBirth.ToString("dd/MM/yyyy")!;
            x.PqEstabCode = account.AccountNumber;
            return x;
        });
        var lookups = await Importer.GetLookupDataAsync(row);

        // Act
        var (failures, errors) = Importer.Validate(row, lookups);

        // Assert
        Assert.DoesNotContain(failures, item => item.Contains($"Organisation with PQ Establishment Code {row.PqEstabCode} was not found."));
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
            x.QtsRefNo = "InvalidTrn";
            x.IttEstabCode = account.AccountNumber;
            x.DateOfBirth = person.DateOfBirth.ToString("dd/MM/yyyy")!;
            x.PqEstabCode = account.AccountNumber;
            return x;
        });

        // Act
        var lookups = await Importer.GetLookupDataAsync(row);

        // Assert
        Assert.Equal(EwcWalesMatchStatus.NoMatch, lookups.PersonMatchStatus);
        Assert.Null(lookups.PersonId);
    }

    [Fact]
    public async Task GetLookupData_IttEstabCodeDoesNotExist_ReturnsNoMatch()
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
            x.QtsRefNo = person.Trn!;
            x.IttEstabCode = "Invalid";
            x.DateOfBirth = person.DateOfBirth.ToString("dd/MM/yyyy")!;
            x.PqEstabCode = account.AccountNumber;
            return x;
        });

        // Act
        var lookups = await Importer.GetLookupDataAsync(row);

        // Assert
        Assert.Equal(EwcWalesMatchStatus.NoMatch, lookups.IttEstabCodeMatchStatus);
        Assert.Null(lookups.IttEstabCodeId);
    }

    [Fact]
    public async Task GetLookupData_IttEstabCodeMultipleMatching_ReturnsMultipleMatchesFound()
    {
        // Arrange
        var accountNumber = "1357";
        var account1 = await TestData.CreateAccountAsync(x =>
        {
            x.WithName("SomeName");
            x.WithAccountNumber(accountNumber);
        });
        var account2 = await TestData.CreateAccountAsync(x =>
        {
            x.WithName("SomeName2");
            x.WithAccountNumber(accountNumber);
        });
        var person = await TestData.CreatePersonAsync(x => x.WithTrn());
        var row = GetDefaultRow(x =>
        {
            x.QtsRefNo = person.Trn!;
            x.IttEstabCode = accountNumber;
            x.DateOfBirth = person.DateOfBirth.ToString("dd/MM/yyyy")!;
            return x;
        });

        // Act
        var lookups = await Importer.GetLookupDataAsync(row);

        // Assert
        Assert.Equal(EwcWalesMatchStatus.MultipleMatchesFound, lookups.IttEstabCodeMatchStatus);
        Assert.Null(lookups.IttEstabCodeId);
    }

    [Fact]
    public async Task GetLookupData_IttQualificationDoesNotExist_ReturnsNotFound()
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
            x.QtsRefNo = person.Trn!;
            x.IttEstabCode = accountNumber;
            x.DateOfBirth = person.DateOfBirth.ToString("dd/MM/yyyy")!;
            x.IttQualCode = "InvalidQual Code";
            return x;
        });

        // Act
        var lookups = await Importer.GetLookupDataAsync(row);

        // Assert
        Assert.Equal(EwcWalesMatchStatus.NoMatch, lookups.IttQualificationMatchStatus);
        Assert.Null(lookups.IttQualificationId);
    }

    [Fact]
    public async Task GetLookupData_IttSubjectsDoNotExist_ReturnsNotFound()
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
            x.QtsRefNo = person.Trn!;
            x.IttEstabCode = accountNumber;
            x.DateOfBirth = person.DateOfBirth.ToString("dd/MM/yyyy")!;
            x.IttSubjectCode1 = "Invalid Subject1";
            x.IttSubjectCode2 = "Invalid Subject2";
            return x;
        });

        // Act
        var lookups = await Importer.GetLookupDataAsync(row);

        // Assert
        Assert.Equal(EwcWalesMatchStatus.NoMatch, lookups.IttSubject1MatchStatus);
        Assert.Null(lookups.IttSubject1Id);
        Assert.Equal(EwcWalesMatchStatus.NoMatch, lookups.IttSubject2MatchStatus);
        Assert.Null(lookups.IttSubject2Id);
    }

    [Fact]
    public async Task GetLookupData_PqSubjectCodesDoNotExist_ReturnsNotFound()
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
            x.QtsRefNo = person.Trn!;
            x.IttEstabCode = accountNumber;
            x.DateOfBirth = person.DateOfBirth.ToString("dd/MM/yyyy")!;
            x.PqSubjectCode1 = "InvalidSubject1";
            x.PqSubjectCode2 = "InvalidSubject2";
            x.PqSubjectCode3 = "InvalidSubject3";
            return x;
        });

        // Act
        var lookups = await Importer.GetLookupDataAsync(row);

        // Assert
        Assert.Equal(EwcWalesMatchStatus.NoMatch, lookups.PQSubject1MatchStatus);
        Assert.Null(lookups.PQSubject1Id);
        Assert.Equal(EwcWalesMatchStatus.NoMatch, lookups.PQSubject2MatchStatus);
        Assert.Null(lookups.PQSubject2Id);
        Assert.Equal(EwcWalesMatchStatus.NoMatch, lookups.PQSubject3MatchStatus);
        Assert.Null(lookups.PQSubject3Id);
    }

    [Fact]
    public async Task GetLookupData_PqEstablishmentIdDoesNotExist_ReturnsNotFound()
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
            x.QtsRefNo = person.Trn!;
            x.IttEstabCode = accountNumber;
            x.DateOfBirth = person.DateOfBirth.ToString("dd/MM/yyyy")!;
            x.PqEstabCode = "Invalid Org number";
            return x;
        });

        // Act
        var lookups = await Importer.GetLookupDataAsync(row);

        // Assert
        Assert.Equal(EwcWalesMatchStatus.NoMatch, lookups.PqEstablishmentMatchStatus);
        Assert.Null(lookups.PqEstablishmentId);
    }

    [Fact]
    public async Task GetLookupData_PQCountryDoesNotExist_ReturnsNotFound()
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
            x.QtsRefNo = person.Trn!;
            x.IttEstabCode = accountNumber;
            x.DateOfBirth = person.DateOfBirth.ToString("dd/MM/yyyy")!;
            x.Country = "Invalid";
            return x;
        });

        // Act
        var lookups = await Importer.GetLookupDataAsync(row);

        // Assert
        Assert.Equal(EwcWalesMatchStatus.NoMatch, lookups.PQCountryMatchStatus);
        Assert.Null(lookups.PQCountryId);
    }

    [Fact]
    public async Task GetLookupData_PQHEQualificationNotExist_ReturnsNotFound()
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
            x.QtsRefNo = person.Trn!;
            x.IttEstabCode = accountNumber;
            x.DateOfBirth = person.DateOfBirth.ToString("dd/MM/yyyy")!;
            x.PqQualCode = "Invalid Qualification Code";
            return x;
        });

        // Act
        var lookups = await Importer.GetLookupDataAsync(row);

        // Assert
        Assert.Equal(EwcWalesMatchStatus.NoMatch, lookups.PQHEQualificationMatchStatus);
        Assert.Null(lookups.PQHEQualificationId);
    }

    [Theory]
    [InlineData("63")]
    [InlineData("")]
    public async Task GetLookupData_TeacherStatusIsNotNull(string qtsStatus)
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
            x.QtsRefNo = person.Trn!;
            x.IttEstabCode = accountNumber;
            x.DateOfBirth = person.DateOfBirth.ToString("dd/MM/yyyy")!;
            x.QtsStatus = qtsStatus;
            return x;
        });

        // Act
        var lookups = await Importer.GetLookupDataAsync(row);

        // Assert
        Assert.NotNull(lookups.TeacherStatusId);
    }

    [Fact]
    public async Task GetLookupData_ConvertToCSVString_ReturnsExpectedCSV()
    {
        // Arrange
        var accountNumber = "135711";
        var account = await TestData.CreateAccountAsync(x =>
        {
            x.WithName("SomeName");
            x.WithAccountNumber(accountNumber);
        });
        var person = await TestData.CreatePersonAsync(x => x.WithQts(new DateOnly(2024, 01, 01)));
        var row = GetDefaultRow(x =>
        {
            x.QtsRefNo = person.Trn!;
            x.PqEstabCode = account.AccountNumber;
            x.IttEstabCode = account.AccountNumber;
            return x;
        });
        var expectedJson = $"{row.QtsRefNo},{row.Forename},{row.Surname},{row.DateOfBirth},{row.QtsStatus},{row.QtsDate},{row.IttStartMonth},{row.IttStartYear},{row.IttEndDate},{row.ITTCourseLength},{row.IttEstabLeaCode},{row.IttEstabCode},{row.IttQualCode},{row.IttClassCode},{row.IttSubjectCode1},{row.IttSubjectCode2},{row.IttMinAgeRange},{row.IttMaxAgeRange},{row.IttMinSpAgeRange},{row.IttMaxSpAgeRange},{row.PqCourseLength},{row.PqYearOfAward},{row.Country},{row.PqEstabCode},{row.PqQualCode},{row.Honours},{row.PqClassCode},{row.PqSubjectCode1},{row.PqSubjectCode2},{row.PqSubjectCode3}\r\n";

        // Act
        var json = Importer.ConvertToCsvString(row);

        // Assert
        Assert.Equal(json, expectedJson);
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
            x.WithTrn();
            x.WithAlert();
        });
        var row = GetDefaultRow(x =>
        {
            x.QtsRefNo = person.Trn!;
            x.IttEstabCode = accountNumber;
            x.DateOfBirth = person.DateOfBirth.ToString("dd/MM/yyyy")!;
            return x;
        });

        // Act
        var lookups = await Importer.GetLookupDataAsync(row);

        // Assert
        Assert.True(lookups.HasActiveAlerts);
    }

    [Fact]
    public async Task GetLookupData_ValidRow_PopulatesLookupDate()
    {
        // Arrange
        var accountNumber = "1357111";
        var account = await TestData.CreateAccountAsync(x =>
        {
            x.WithName("SomeName");
            x.WithAccountNumber(accountNumber);
        });
        var person = await TestData.CreatePersonAsync(x => x.WithQts(new DateOnly(2024, 01, 01)));
        var row = GetDefaultRow(x =>
        {
            x.QtsRefNo = person.Trn!;
            x.PqEstabCode = account.AccountNumber;
            x.IttEstabCode = account.AccountNumber;
            return x;
        });

        // Act
        var lookups = await Importer.GetLookupDataAsync(row);

        // Assert
        Assert.NotNull(lookups.PersonId);
        Assert.Equal(EwcWalesMatchStatus.TeacherHasQts, lookups.PersonMatchStatus);
        Assert.NotNull(lookups.IttEstabCodeId);
        Assert.Equal(EwcWalesMatchStatus.OneMatch, lookups.IttEstabCodeMatchStatus);
        Assert.NotNull(lookups.IttQualificationId);
        Assert.Equal(EwcWalesMatchStatus.OneMatch, lookups.IttQualificationMatchStatus);
        Assert.NotNull(lookups.IttSubject1Id);
        Assert.Equal(EwcWalesMatchStatus.OneMatch, lookups.IttSubject1MatchStatus);
        Assert.NotNull(lookups.IttSubject2Id);
        Assert.Equal(EwcWalesMatchStatus.OneMatch, lookups.IttSubject2MatchStatus);
        Assert.NotNull(lookups.PqEstablishmentId);
        Assert.Equal(EwcWalesMatchStatus.OneMatch, lookups.PqEstablishmentMatchStatus);
        Assert.NotNull(lookups.PQCountryId);
        Assert.Equal(EwcWalesMatchStatus.OneMatch, lookups.PQCountryMatchStatus);
        Assert.NotNull(lookups.PQHEQualificationId);
        Assert.Equal(EwcWalesMatchStatus.OneMatch, lookups.PQHEQualificationMatchStatus);
        Assert.NotNull(lookups.PQSubject1Id);
        Assert.Equal(EwcWalesMatchStatus.OneMatch, lookups.PQSubject1MatchStatus);
        Assert.NotNull(lookups.PQSubject2Id);
        Assert.Equal(EwcWalesMatchStatus.OneMatch, lookups.PQSubject2MatchStatus);
        Assert.NotNull(lookups.PQSubject3Id);
        Assert.Equal(EwcWalesMatchStatus.OneMatch, lookups.PQSubject3MatchStatus);
        Assert.NotNull(lookups.TeacherStatusId);
        Assert.Equal(EwcWalesMatchStatus.OneMatch, lookups.TeacherStatusMatchStatus);
        Assert.NotNull(lookups.ClassDivision);
    }

    private EwcWalesQtsFileImportData GetDefaultRow(Func<EwcWalesQtsFileImportData, EwcWalesQtsFileImportData>? configurator = null)
    {
        var row = new EwcWalesQtsFileImportData()
        {
            QtsRefNo = "",
            Forename = Faker.Name.First(),
            Surname = Faker.Name.First(),
            DateOfBirth = Faker.Identification.DateOfBirth().ToString(),
            QtsStatus = "63",
            QtsDate = "01/04/2024",
            IttStartMonth = "07",
            IttStartYear = "2023",
            IttEndDate = "01/07/2024",
            ITTCourseLength = "1",
            IttEstabLeaCode = "",
            IttEstabCode = "",
            IttQualCode = "400", //degree
            IttClassCode = "",
            IttSubjectCode1 = "100078", //business and management
            IttSubjectCode2 = "100300", //classical studies
            IttMinAgeRange = "",
            IttMaxAgeRange = "",
            IttMinSpAgeRange = "",
            IttMaxSpAgeRange = "",
            PqCourseLength = "",
            PqYearOfAward = "",
            Country = "XK", //United Kingdom
            PqEstabCode = "",
            PqQualCode = "001", //bED
            Honours = "",
            PqClassCode = "01",
            PqSubjectCode1 = "002", //English
            PqSubjectCode2 = "003", //Science
            PqSubjectCode3 = "004"  //Art
        };
        var configuredRow = configurator != null ? configurator(row) : row;
        return configuredRow;
    }
}
