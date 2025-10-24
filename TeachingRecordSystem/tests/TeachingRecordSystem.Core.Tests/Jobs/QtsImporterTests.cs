using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Jobs.EwcWalesImport;

namespace TeachingRecordSystem.Core.Tests.Jobs;

[Collection(nameof(DisableParallelization))]
public class QtsImporterTests(QtsImporterFixture fixture) : IClassFixture<QtsImporterFixture>, IAsyncLifetime
{
    private IDbContextFactory<TrsDbContext> DbContextFactory => fixture.DbContextFactory;

    private TestData TestData => fixture.TestData;

    private TestableClock Clock => fixture.Clock;

    async ValueTask IAsyncLifetime.InitializeAsync() => await DbContextFactory.WithDbContextAsync(dbContext => dbContext.Events.ExecuteDeleteAsync());

    ValueTask IAsyncDisposable.DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public Task Validate_NoneExistentTeacher_ReturnsErrorMessage() => fixture.WithQtsImporter(async importer =>
    {
        // Arrange
        var row = GetDefaultRow(x =>
        {
            x.QtsRefNo = "InvalidTrn";
            return x;
        });

        var lookups = await importer.GetLookupDataAsync(row);

        // Act
        var (failures, errors) = importer.Validate(row, lookups);

        // Assert
        Assert.Contains(errors, item => item.Contains($"Teacher with TRN {row.QtsRefNo} was not found."));
    });

    [Fact]
    public Task Validate_WithMissingMandatoryFields_ReturnsErrorMessages() => fixture.WithQtsImporter(async importer =>
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
        var lookups = await importer.GetLookupDataAsync(row);

        // Act
        var (failures, errors) = importer.Validate(row, lookups);

        // Assert
        Assert.Contains(errors, item => item.Contains("Missing QTS Ref Number"));
        Assert.Contains(errors, item => item.Contains("Missing Date of Birth"));
        Assert.Contains(errors, item => item.Contains("Misssing QTS Date"));
    });

    [Fact]
    public Task Validate_WithMalformedDateOfBirth_ReturnsErrorMessages() => fixture.WithQtsImporter(async importer =>
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var row = GetDefaultRow(x =>
        {
            x.QtsRefNo = person.Trn!;
            x.DateOfBirth = "67/13/2025";
            return x;
        });
        var lookups = await importer.GetLookupDataAsync(row);

        // Act
        var (failures, errors) = importer.Validate(row, lookups);

        // Assert
        Assert.Contains(errors, item => item.Contains("Validation Failed: Invalid Date of Birth"));
    });

    [Fact]
    public Task Validate_WithMalformedQTSDate_ReturnsErrorMessages() => fixture.WithQtsImporter(async importer =>
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var row = GetDefaultRow(x =>
        {
            x.QtsRefNo = person.Trn!;
            x.QtsDate = "67/13/2025";
            return x;
        });
        var lookups = await importer.GetLookupDataAsync(row);

        // Act
        var (failures, errors) = importer.Validate(row, lookups);

        // Assert
        Assert.Contains(errors, item => item.Contains("Validation Failed: Invalid QTS Date"));
    });

    [Fact]
    public Task Validate_ExistingTeacherDateOfBirthDoesNotMatch_ReturnsErrorMessage() => fixture.WithQtsImporter(async importer =>
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var row = GetDefaultRow(x =>
        {
            x.QtsRefNo = person.Trn!;
            x.DateOfBirth = "01/06/1999";
            return x;
        });
        var lookups = await importer.GetLookupDataAsync(row);

        // Act
        var (failures, errors) = importer.Validate(row, lookups);

        // Assert
        Assert.Contains(errors, item => item.Contains($"For TRN {row.QtsRefNo} Date of Birth does not match with the existing record."));
    });

    [Fact]
    public Task Validate_ValidCountry_ReturnsErrorMessage() => fixture.WithQtsImporter(async importer =>
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var row = GetDefaultRow(x =>
        {
            x.QtsRefNo = person.Trn!;
            x.DateOfBirth = person.DateOfBirth.ToString()!;
            return x;
        });
        var lookups = await importer.GetLookupDataAsync(row);

        // Act
        var (failures, errors) = importer.Validate(row, lookups);

        // Assert
        Assert.DoesNotContain(failures, item => item.Contains($"Country with PQ Country Code {row.Country} was not found."));
    });

    [Fact]
    public Task Validate_MatchesExistingWelshR_ReturnsErrorMessage() => fixture.WithQtsImporter(async importer =>
    {
        // Arrange
        var holdsDate = Clock.Today.AddDays(-10);
        var person = await TestData.CreatePersonAsync(x =>
        {
            x.WithRouteToProfessionalStatus(s => s
                .WithRouteType(RouteToProfessionalStatusType.WelshRId)
                .WithHoldsFrom(holdsDate)
                .WithStatus(RouteToProfessionalStatusStatus.Holds));
        });
        var row = GetDefaultRow(x =>
        {
            x.QtsRefNo = person.Trn!;
            x.DateOfBirth = person.DateOfBirth.ToString("dd/MM/yyyy")!;
            x.QtsDate = holdsDate.ToString("dd/MM/yyyy");
            return x;
        });
        var lookups = await importer.GetLookupDataAsync(row);

        // Act
        var (failures, errors) = importer.Validate(row, lookups);

        // Assert
        Assert.Contains(errors, item => item.Contains($"{person.Trn} already holds welshr route with holdsfrom {holdsDate}"));
    });

    [Fact]
    public Task Validate_ExistingWelshR_ReturnsNoErrors() => fixture.WithQtsImporter(async importer =>
    {
        // Arrange
        var holdsDate = Clock.Today.AddDays(-10);
        var person = await TestData.CreatePersonAsync(x =>
        {
            x.WithRouteToProfessionalStatus(s => s
                .WithRouteType(RouteToProfessionalStatusType.WelshRId)
                .WithHoldsFrom(holdsDate)
                .WithStatus(RouteToProfessionalStatusStatus.Holds));
        });
        var row = GetDefaultRow(x =>
        {
            x.QtsRefNo = person.Trn!;
            x.DateOfBirth = person.DateOfBirth.ToString("dd/MM/yyyy")!;
            x.QtsDate = holdsDate.AddDays(1).ToString("dd/MM/yyyy");
            x.QtsStatus = "71";
            return x;
        });
        var lookups = await importer.GetLookupDataAsync(row);

        // Act
        var (failures, errors) = importer.Validate(row, lookups);

        // Assert
        Assert.DoesNotContain(errors, item => item.Contains($"{person.Trn} already holds welshr route with holdsfrom {holdsDate}"));
        Assert.Empty(errors);
    });

    [Fact]
    public Task Validate_QualifiedTeacherEcDirectiveStatusAfterRegsChangeDate_ReturnErrorMessage() => fixture.WithQtsImporter(async importer =>
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var row = GetDefaultRow(x =>
        {
            x.QtsRefNo = person.Trn!;
            x.DateOfBirth = person.DateOfBirth.ToString("dd/MM/yyyy")!;
            x.PqEstabCode = "InvalidOrg";
            x.QtsStatus = "67";
            x.QtsDate = "01/05/2025";
            return x;
        });
        var lookups = await importer.GetLookupDataAsync(row);

        // Act
        var (failures, errors) = importer.Validate(row, lookups);

        // Assert
        Assert.Contains(errors, item => item.Contains("Qts Status can only be 71 or 49 when qts date is on or past 01/02/2023"));
    });

    [Fact]
    public Task Validate_QtsDateInTheFuture_ReturnErrorMessage() => fixture.WithQtsImporter(async importer =>
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var row = GetDefaultRow(x =>
        {
            x.QtsRefNo = person.Trn!;
            x.DateOfBirth = person.DateOfBirth.ToString("dd/MM/yyyy")!;
            x.QtsStatus = "67";
            x.QtsDate = Clock.UtcNow.AddDays(1).ToString("dd/MM/yyyy");
            return x;
        });
        var lookups = await importer.GetLookupDataAsync(row);

        // Act
        var (failures, errors) = importer.Validate(row, lookups);

        // Assert
        Assert.Contains(errors, item => item.Contains("Qts date cannot be set in the future"));
    });

    [Fact]
    public Task Validate_QualifiedTeacherEcDirectiveStatusBeforeRegsChangeDate_DoesNotReturnErrorMessage() => fixture.WithQtsImporter(async importer =>
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var row = GetDefaultRow(x =>
        {
            x.QtsRefNo = person.Trn!;
            x.DateOfBirth = person.DateOfBirth.ToString("dd/MM/yyyy")!;
            x.PqEstabCode = "InvalidOrg";
            x.QtsStatus = "67";
            x.QtsDate = "01/01/2023";
            return x;
        });
        var lookups = await importer.GetLookupDataAsync(row);

        // Act
        var (failures, errors) = importer.Validate(row, lookups);

        // Assert
        Assert.DoesNotContain(errors, item => item.Contains("Qualified Teacher: under the EC Directive must be before 01/02/2023"));
    });

    [Fact]
    public Task GetLookupData_TrnDoesNotExist_ReturnsNoMatch() => fixture.WithQtsImporter(async importer =>
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var row = GetDefaultRow(x =>
        {
            x.QtsRefNo = "InvalidTrn";
            x.DateOfBirth = person.DateOfBirth.ToString("dd/MM/yyyy")!;
            return x;
        });

        // Act
        var lookups = await importer.GetLookupDataAsync(row);

        // Assert
        Assert.Equal(EwcWalesMatchStatus.NoMatch, lookups.PersonMatchStatus);
        Assert.Null(lookups.Person);
    });

    [Theory]
    [InlineData("67")]
    [InlineData("49")]
    [InlineData("71")]
    public Task GetLookupData_TeacherStatusIsNotNullForRecognizedStatus(string qtsStatus) => fixture.WithQtsImporter(async importer =>
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var row = GetDefaultRow(x =>
        {
            x.QtsRefNo = person.Trn!;
            x.DateOfBirth = person.DateOfBirth.ToString("dd/MM/yyyy")!;
            x.QtsStatus = qtsStatus;
            return x;
        });

        // Act
        var lookups = await importer.GetLookupDataAsync(row);

        // Assert
        Assert.Equal(EwcWalesMatchStatus.OneMatch, lookups.TeacherStatusMatchStatus);
    });

    [Theory]
    [InlineData("63")]
    [InlineData("")]
    public Task GetLookupData_TeacherStatusIsNotMatchForUnrecognizedStatus(string qtsStatus) => fixture.WithQtsImporter(async importer =>
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var row = GetDefaultRow(x =>
        {
            x.QtsRefNo = person.Trn!;
            x.DateOfBirth = person.DateOfBirth.ToString("dd/MM/yyyy")!;
            x.QtsStatus = qtsStatus;
            return x;
        });

        // Act
        var lookups = await importer.GetLookupDataAsync(row);

        // Assert
        Assert.Equal(EwcWalesMatchStatus.NoMatch, lookups.TeacherStatusMatchStatus);
    });

    [Fact]
    public Task GetLookupData_ConvertToCSVString_ReturnsExpectedCSV() => fixture.WithQtsImporter(async importer =>
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(x => x.WithQts(new DateOnly(2024, 01, 01)));
        var row = GetDefaultRow(x =>
        {
            x.QtsRefNo = person.Trn!;
            return x;
        });
        var expectedJson = $"{row.QtsRefNo},{row.Forename},{row.Surname},{row.DateOfBirth},{row.QtsStatus},{row.QtsDate},{row.IttStartMonth},{row.IttStartYear},{row.IttEndDate},{row.ITTCourseLength},{row.IttEstabLeaCode},{row.IttEstabCode},{row.IttQualCode},{row.IttClassCode},{row.IttSubjectCode1},{row.IttSubjectCode2},{row.IttMinAgeRange},{row.IttMaxAgeRange},{row.IttMinSpAgeRange},{row.IttMaxSpAgeRange},{row.PqCourseLength},{row.PqYearOfAward},{row.Country},{row.PqEstabCode},{row.PqQualCode},{row.Honours},{row.PqClassCode},{row.PqSubjectCode1},{row.PqSubjectCode2},{row.PqSubjectCode3}";

        // Act
        var json = importer.ConvertToCsvString(row);

        // Assert
        Assert.Contains(expectedJson, json);
    });

    [Fact]
    public Task GetLookupData_WithActiveAlert_ReturnsExpected() => fixture.WithQtsImporter(async importer =>
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(x =>
        {
            x.WithAlert();
        });
        var row = GetDefaultRow(x =>
        {
            x.QtsRefNo = person.Trn!;
            x.DateOfBirth = person.DateOfBirth.ToString("dd/MM/yyyy")!;
            return x;
        });

        // Act
        var lookups = await importer.GetLookupDataAsync(row);

        // Assert
        Assert.True(lookups.HasActiveAlerts);
    });

    [Fact]
    public Task GetLookupData_ValidRow_PopulatesLookupDate() => fixture.WithQtsImporter(async importer =>
    {
        // Arrange
        var awardDate = new DateOnly(2011, 01, 1);
        var person1AwardedDate = new DateOnly(2011, 01, 04);
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(s => s
                .WithRouteType(RouteToProfessionalStatusType.WelshRId)
                .WithHoldsFrom(Clock.Today.AddDays(-10))
                .WithStatus(RouteToProfessionalStatusStatus.Holds)));
        var row = GetDefaultRow(x =>
        {
            x.QtsRefNo = person.Trn!;
            x.QtsStatus = "67";
            return x;
        });

        // Act
        var lookups = await importer.GetLookupDataAsync(row);

        // Assert
        Assert.NotNull(lookups.Person);
        Assert.Equal(EwcWalesMatchStatus.TeacherHasQts, lookups.PersonMatchStatus);
        Assert.Equal(EwcWalesMatchStatus.OneMatch, lookups.TeacherStatusMatchStatus);
    });

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

public class QtsImporterFixture : IDisposable
{
    public QtsImporterFixture()
    {
        var services = new ServiceCollection();
        CoreFixture.AddCoreServices(services);
        services.AddSingleton(new TestableClock());
        services.AddSingleton<IClock>(sp => sp.GetRequiredService<TestableClock>());
        services.AddSingleton<ReferenceDataCache>();
        services.AddSingleton<TestData>();
        Services = services.BuildServiceProvider();
    }

    public TestableClock Clock => Services.GetRequiredService<TestableClock>();

    public IDbContextFactory<TrsDbContext> DbContextFactory => Services.GetRequiredService<IDbContextFactory<TrsDbContext>>();

    public DbHelper DbHelper => Services.GetRequiredService<DbHelper>();

    public IServiceProvider Services { get; }

    public TestData TestData => Services.GetRequiredService<TestData>();

    public Task WithQtsImporter(Func<QtsImporter, Task> action) => DbContextFactory.WithDbContextAsync(async dbContext =>
    {
        var qtsImporter = new QtsImporter(new NullLogger<InductionImporter>(), dbContext, TestData.ReferenceDataCache, Clock);
        await action(qtsImporter);
    });

    void IDisposable.Dispose() => (Services as IDisposable)?.Dispose();
}
