using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Jobs.EwcWalesImport;

namespace TeachingRecordSystem.Core.Tests.Jobs;

[Collection(nameof(DisableParallelization))]
public class InductionimporterTests(InductionimporterFixture fixture) : IClassFixture<InductionimporterFixture>, IAsyncLifetime
{
    private IDbContextFactory<TrsDbContext> DbContextFactory => fixture.DbContextFactory;

    private TestData TestData => fixture.TestData;

    private TestableClock Clock => fixture.Clock;

    async ValueTask IAsyncLifetime.InitializeAsync() => await DbContextFactory.WithDbContextAsync(dbContext => dbContext.Events.ExecuteDeleteAsync());

    ValueTask IAsyncDisposable.DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public Task Validate_MissingReferenceNumber_ReturnsError() => fixture.WithInductionimporter(async importer =>
    {
        // Arrange
        var row = GetDefaultRow();
        var lookups = await importer.GetLookupDataAsync(row);

        // Act
        var (failures, errors) = importer.Validate(row, lookups);

        // Assert
        Assert.Contains(errors, item => item.Contains($"Missing Reference No"));
    });

    [Fact]
    public Task Validate_MissingDateOfBirth_ReturnsError() => Task.FromResult(fixture.WithInductionimporter(async importer =>
    {
        // Arrange
        var row = GetDefaultRow(x =>
        {
            x.DateOfBirth = "";
            return x;
        });
        var lookups = await importer.GetLookupDataAsync(row);

        // Act
        var (failures, errors) = importer.Validate(row, lookups);

        // Assert
        Assert.Contains(errors, item => item.Contains($"Missing Date of Birth"));
    }));

    [Fact]
    public Task Validate_InvalidDateOfBirth_ReturnsError() => Task.FromResult(fixture.WithInductionimporter(async importer =>
    {
        // Arrange
        var row = GetDefaultRow(x =>
        {
            x.DateOfBirth = "45/11/19990";
            return x;
        });
        var lookups = await importer.GetLookupDataAsync(row);

        // Act
        var (failures, errors) = importer.Validate(row, lookups);

        // Assert
        Assert.Contains(errors, item => item.Contains($"Validation Failed: Invalid Date of Birth"));
    }));

    [Fact]
    public Task Validate_MissingStartDate_ReturnsError() => Task.FromResult(fixture.WithInductionimporter(async importer =>
    {
        // Arrange
        var row = GetDefaultRow(x =>
        {
            x.StartDate = "";
            return x;
        });
        var lookups = await importer.GetLookupDataAsync(row);

        // Act
        var (failures, errors) = importer.Validate(row, lookups);

        // Assert
        Assert.Contains(errors, item => item.Contains($"Missing Induction Start date"));
    }));

    [Fact]
    public Task Validate_PassedDateBeforeStartDate_ReturnsError() => Task.FromResult(fixture.WithInductionimporter(async importer =>
    {
        // Arrange
        var row = GetDefaultRow(x =>
        {
            x.StartDate = "01/01/2022";
            x.PassedDate = "01/01/2021";
            return x;
        });
        var lookups = await importer.GetLookupDataAsync(row);

        // Act
        var (failures, errors) = importer.Validate(row, lookups);

        // Assert
        Assert.Contains(errors, item => item.Contains($"Induction passed date cannot be before start date"));
    }));

    [Fact]
    public Task Validate_PassedDateBeforeQtsDate_ReturnsError() => Task.FromResult(fixture.WithInductionimporter(async importer =>
    {
        // Arrange
        var awardDate = new DateOnly(2011, 01, 1);
        var person1AwardedDate = new DateOnly(2011, 01, 04);
        var person = await TestData.CreatePersonAsync(x =>
        {
            x.WithRouteToProfessionalStatus(s => s
                .WithRouteType(RouteToProfessionalStatusType.WelshRId)
                .WithHoldsFrom(person1AwardedDate)
                .WithStatus(RouteToProfessionalStatusStatus.Holds));
        });
        var row = GetDefaultRow(x =>
        {
            x.ReferenceNumber = person.Trn!;
            x.DateOfBirth = person.DateOfBirth.ToString("dd/MM/yyyy");
            x.StartDate = person1AwardedDate.AddDays(-10).ToString("dd/MM/yyyy");
            x.PassedDate = person1AwardedDate.AddDays(-8).ToString("dd/MM/yyyy");
            return x;
        });
        var lookups = await importer.GetLookupDataAsync(row);

        // Act
        var (failures, errors) = importer.Validate(row, lookups);

        // Assert
        Assert.Contains(errors, item => item.Contains($"Induction passed date cannot be before Qts Date."));
    }));

    [Fact]
    public Task Validate_StartDateBeforeQtsDate_ReturnsError() => Task.FromResult(fixture.WithInductionimporter(async importer =>
    {
        // Arrange
        var awardDate = new DateOnly(2011, 01, 1);
        var person1AwardedDate = Clock.Today.AddDays(-100);
        var qtsDate = Clock.Today.AddDays(-110);
        var person = await TestData.CreatePersonAsync(x =>
        {
            x.WithQtls(qtsDate);
            x.WithRouteToProfessionalStatus(s => s
                .WithRouteType(RouteToProfessionalStatusType.WelshRId)
                .WithHoldsFrom(person1AwardedDate)
                .WithStatus(RouteToProfessionalStatusStatus.Holds));
        });
        var row = GetDefaultRow(x =>
        {
            x.ReferenceNumber = person.Trn!;
            x.DateOfBirth = person.DateOfBirth.ToString("dd/MM/yyyy");
            x.StartDate = qtsDate.AddDays(-10).ToString("dd/MM/yyyy");
            x.PassedDate = person1AwardedDate.AddDays(-8).ToString("dd/MM/yyyy");
            return x;
        });
        var lookups = await importer.GetLookupDataAsync(row);

        // Act
        var (failures, errors) = importer.Validate(row, lookups);

        // Assert
        Assert.Contains(errors, item => item.Contains($"Induction start date cannot be before qts date"));
    }));

    [Fact]
    public Task Validate_InvalidStartDate_ReturnsError() => Task.FromResult(fixture.WithInductionimporter(async importer =>
    {
        // Arrange
        var row = GetDefaultRow(x =>
        {
            x.StartDate = "55/13/20001111";
            return x;
        });
        var lookups = await importer.GetLookupDataAsync(row);

        // Act
        var (failures, errors) = importer.Validate(row, lookups);

        // Assert
        Assert.Contains(errors, item => item.Contains($"Validation Failed: Invalid Induction start date"));
    }));

    [Fact]
    public Task Validate_MissinPassedDate_ReturnsError() => Task.FromResult(fixture.WithInductionimporter(async importer =>
    {
        // Arrange
        var row = GetDefaultRow(x =>
        {
            x.PassedDate = "";
            return x;
        });
        var lookups = await importer.GetLookupDataAsync(row);

        // Act
        var (failures, errors) = importer.Validate(row, lookups);

        // Assert
        Assert.Contains(errors, item => item.Contains($"Missing Induction passed date"));
    }));

    [Fact]
    public Task Validate_InvalidPassedDate_ReturnsError() => Task.FromResult(fixture.WithInductionimporter(async importer =>
    {
        // Arrange
        var row = GetDefaultRow(x =>
        {
            x.PassedDate = "25/13/20001";
            return x;
        });
        var lookups = await importer.GetLookupDataAsync(row);

        // Act
        var (failures, errors) = importer.Validate(row, lookups);

        // Assert
        Assert.Contains(errors, item => item.Contains($"Validation Failed: Invalid Induction passed date"));
    }));

    [Fact]
    public Task Validate_ReferenceNumberNotFound_ReturnsError() => Task.FromResult(fixture.WithInductionimporter(async importer =>
    {
        // Arrange
        var row = GetDefaultRow(x =>
        {
            x.ReferenceNumber = "NONE EXISTENT";
            return x;
        });
        var lookups = await importer.GetLookupDataAsync(row);

        // Act
        var (failures, errors) = importer.Validate(row, lookups);

        // Assert
        Assert.Contains(errors, item => item.Contains($"Teacher with TRN {row.ReferenceNumber} was not found."));
    }));

    [Theory]
    [InlineData(InductionStatus.Passed)]
    [InlineData(InductionStatus.Failed)]
    [InlineData(InductionStatus.InProgress)]
    [InlineData(InductionStatus.FailedInWales)]
    public Task Validate_WithCompletedInduction_ReturnsError(InductionStatus inductionStatus) => Task.FromResult(fixture.WithInductionimporter(async importer =>
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(x =>
        {
            x.WithInductionStatus(builder => builder.WithStatus(inductionStatus));
        });
        var row = GetDefaultRow(x =>
        {
            x.ReferenceNumber = person.Trn!;
            x.DateOfBirth = person.DateOfBirth.ToString("dd/MM/yyyy");
            return x;
        });
        var lookups = await importer.GetLookupDataAsync(row);

        // Act
        var (failures, errors) = importer.Validate(row, lookups);

        // Assert
        Assert.Contains(errors, item => item.Contains($"Teacher with TRN {row.ReferenceNumber} completed induction already or is progress."));
    }));

    [Theory]
    [InlineData("12345")]
    [InlineData("212324")]
    public Task Validate_WithoutEndDate_ReturnsNoErrors(string accountNumber) => Task.FromResult(fixture.WithInductionimporter(async importer =>
    {
        // Arrange
        var inductionPeriodStartDate = new DateOnly(2019, 01, 01);
        var inductionStartDate = new DateOnly(2019, 01, 01);
        var person = await TestData.CreatePersonAsync(x =>
        {
            x.WithRouteToProfessionalStatus(s => s
                .WithRouteType(RouteToProfessionalStatusType.WelshRId)
                .WithHoldsFrom(Clock.Today.AddDays(-10))
                .WithStatus(RouteToProfessionalStatusStatus.Holds));
        });
        var row = GetDefaultRow(x =>
        {
            x.ReferenceNumber = person.Trn!;
            x.DateOfBirth = person.DateOfBirth.ToString("dd/MM/yyyy");
            x.EmployerCode = accountNumber;
            return x;
        });
        var lookups = await importer.GetLookupDataAsync(row);

        // Act
        var (failures, errors) = importer.Validate(row, lookups);

        // Assert
        Assert.Empty(errors);
        Assert.Empty(failures);
    }));

    [Fact]
    public Task GetLookupData_TrnDoesNotExist_ReturnsNoMatch() => Task.FromResult(fixture.WithInductionimporter(async importer =>
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var row = GetDefaultRow(x =>
        {
            x.ReferenceNumber = "InvalidTrn";
            return x;
        });

        // Act
        var lookups = await importer.GetLookupDataAsync(row);

        // Assert
        Assert.Equal(ContactLookupResult.NoMatch, lookups.PersonMatchStatus);
        Assert.Null(lookups.Person);
    }));

    [Fact]
    public Task GetLookupData_WithActiveAlert_ReturnsExpected() => Task.FromResult(fixture.WithInductionimporter(async importer =>
    {
        // Arrange
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
        var lookups = await importer.GetLookupDataAsync(row);

        // Assert
        Assert.True(lookups.HasActiveAlerts);
    }));

    [Fact]
    public Task GetLookupData_ValidTrnWithoutQTS_ReturnsNoAssociatedQTS() => Task.FromResult(fixture.WithInductionimporter(async importer =>
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var row = GetDefaultRow(x =>
        {
            x.ReferenceNumber = person.Trn!;
            x.DateOfBirth = person.DateOfBirth.ToString("dd/MM/yyyy");
            return x;
        });

        // Act
        var lookups = await importer.GetLookupDataAsync(row);

        // Assert
        Assert.Equal(ContactLookupResult.NoAssociatedQts, lookups.PersonMatchStatus);
        Assert.NotNull(lookups.Person);
        Assert.Equal(person.PersonId, lookups.Person!.PersonId);
    }));

    [Fact]
    public Task GetLookupData_ValidTrnWithQTS_ReturnsTeacherHasQTS() => Task.FromResult(fixture.WithInductionimporter(async importer =>
    {
        // Arrange
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
        var lookups = await importer.GetLookupDataAsync(row);

        // Assert
        Assert.Equal(ContactLookupResult.TeacherHasQts, lookups.PersonMatchStatus);
        Assert.NotNull(lookups.Person);
        Assert.Equal(person.PersonId, lookups.Person!.PersonId);
    }));

    [Fact]
    public Task Validate_WithQtlsDate_DoesNotReturnError() => Task.FromResult(fixture.WithInductionimporter(async importer =>
    {
        // Arrange
        var awardDate = new DateOnly(2011, 01, 1);
        var person1AwardedDate = new DateOnly(2011, 01, 04);
        var route = (await TestData.ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).Where(r => r.RouteToProfessionalStatusTypeId == RouteToProfessionalStatusType.WelshRId).First();
        var person = await TestData.CreatePersonAsync(p => p
            .WithQtls()
            .WithHoldsRouteToProfessionalStatus(route.RouteToProfessionalStatusTypeId, person1AwardedDate));
        var row = GetDefaultRow(x =>
        {
            x.ReferenceNumber = person.Trn!;
            x.DateOfBirth = person.DateOfBirth.ToString("dd/MM/yyyy");
            return x;
        });
        var lookups = await importer.GetLookupDataAsync(row);

        // Act
        var (failures, errors) = importer.Validate(row, lookups);

        // Assert
        Assert.Empty(errors);
    }));

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
            InductionStatusName = "01/07/2024"
        };
        var configuredRow = configurator != null ? configurator(row) : row;
        return configuredRow;
    }
}

public class InductionimporterFixture : IDisposable
{
    public InductionimporterFixture()
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

    public Task WithInductionimporter(Func<InductionImporter, Task> action) => DbContextFactory.WithDbContextAsync(async dbContext =>
    {
        var inductionimporter = new InductionImporter(new NullLogger<InductionImporter>(), dbContext, Clock);
        await action(inductionimporter);
    });

    void IDisposable.Dispose() => (Services as IDisposable)?.Dispose();
}
