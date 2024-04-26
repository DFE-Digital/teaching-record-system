using System.Text;
using Microsoft.PowerPlatform.Dataverse.Client;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Services.Establishments.Tps;
using TeachingRecordSystem.Core.Services.TrsDataSync;
using TeachingRecordSystem.Core.Services.WorkforceData;

namespace TeachingRecordSystem.Core.Tests.Services.Establishments.Tps;

[Collection(nameof(DisableParallelization))]
public class TpsEstablishmentRefresherTests : IAsyncLifetime
{
    private const string KnownGiasLaCodeHackney = "204";
    private const string KnownGiasLaNameHackney = "Hackney";
    private const string KnownGiasEstablishmentNumberHackney = "2654";
    private const string KnownGiasEstablishmentNameHackney = "Woodberry Down Community Primary School";
    private const string KnownGiasLaCodeCityOfLondon = "201";
    private const string KnownGiasLaNameCityOfLondon = "City of London";
    private const string KnownGiasEstablishmentNumberCityOfLondon = "6007";
    private const string KnownGiasEstablishmentNameCityOfLondon = "City of London School";
    private const string KnownTpsLaNameCorporationOfLondon = "CORPORATION OF LONDON";
    private const string KnownTpsLaCode = "751";
    private const string KnownTpsEstablishmentNumberWithinTpsEstablishmentTypeRange = "0972";
    private const string KnownTpsEstablishmentTypeShortDescription = "Full and Part-Time Youth and Community Worker";
    private const string KnownTpsEstablishmentNumberOutsideTpsEstablishmentTypeRange = "0000";

    public TpsEstablishmentRefresherTests(
        DbFixture dbFixture,
        IOrganizationServiceAsync2 organizationService,
        ReferenceDataCache referenceDataCache,
        FakeTrnGenerator trnGenerator)
    {
        DbFixture = dbFixture;
        Clock = new();

        var dbContextFactory = dbFixture.GetDbContextFactory();

        Helper = new TrsDataSyncHelper(
            dbContextFactory,
            organizationService,
            referenceDataCache,
            Clock);

        TestData = new TestData(
            dbContextFactory,
            organizationService,
            referenceDataCache,
            Clock,
            trnGenerator,
            TestDataSyncConfiguration.Sync(Helper));
    }

    public static TheoryData<TpsEstablishmentFileImportTestScenarioData> GetImportFileTestScenarioData()
    {
        var validFormatLocalAuthorityCode = "123";
        var invalidFormatLocalAuthorityCode = "1234";
        var validFormatEstablishmentNumber = "1234";
        var invalidFormatEstablishmentNumber = "12345";
        var validFormatSchoolClosedDate = "03/02/2023";
        var invalidFormatSchoolClosedDate = "1234";

        return new TheoryData<TpsEstablishmentFileImportTestScenarioData>
        {
            new TpsEstablishmentFileImportTestScenarioData
            {
                Row = new TpsEstablishmentCsvRow
                {
                    LaCode = validFormatLocalAuthorityCode,
                    EstablishmentCode = validFormatEstablishmentNumber,
                    EmployersName = "Employers Name",
                    SchoolGiasName = "School Gias Name",
                    SchoolClosedDate = validFormatSchoolClosedDate
                },
                IsExpectedToBeImported = true
            },
            new TpsEstablishmentFileImportTestScenarioData
            {
                Row = new TpsEstablishmentCsvRow
                {
                    LaCode = invalidFormatLocalAuthorityCode,
                    EstablishmentCode = validFormatEstablishmentNumber,
                    EmployersName = "Employers Name",
                    SchoolGiasName = "School Gias Name",
                    SchoolClosedDate = validFormatSchoolClosedDate
                },
                IsExpectedToBeImported = false
            },
            new TpsEstablishmentFileImportTestScenarioData
            {
                Row = new TpsEstablishmentCsvRow
                {
                    LaCode = validFormatLocalAuthorityCode,
                    EstablishmentCode = invalidFormatEstablishmentNumber,
                    EmployersName = "Employers Name",
                    SchoolGiasName = "School Gias Name",
                    SchoolClosedDate = validFormatSchoolClosedDate
                },
                IsExpectedToBeImported = false
            },
            new TpsEstablishmentFileImportTestScenarioData
            {
                Row = new TpsEstablishmentCsvRow
                {
                    LaCode = validFormatLocalAuthorityCode,
                    EstablishmentCode = validFormatEstablishmentNumber,
                    EmployersName = "Employers Name",
                    SchoolGiasName = "School Gias Name",
                    SchoolClosedDate = invalidFormatSchoolClosedDate
                },
                IsExpectedToBeImported = false
            }
        };
    }

    public static TheoryData<TpsEstablishmentRefreshTestScenarioData> GetRefreshEstablishmentsTestScenarioData()
    {
        return new TheoryData<TpsEstablishmentRefreshTestScenarioData>
        {
            // Don't add a new establishment for TPS source if it already exists in GIAS
            new TpsEstablishmentRefreshTestScenarioData
            {
                TpsEstablishments = new[]
                {
                    new TpsEstablishment
                    {
                        TpsEstablishmentId = Guid.NewGuid(),
                        LaCode = KnownGiasLaCodeHackney,
                        EstablishmentCode = KnownGiasEstablishmentNumberHackney,
                        EmployersName = KnownGiasLaNameHackney,
                        SchoolGiasName = null,
                        SchoolClosedDate = null
                    }
                },
                IsExpectedToGenerateEstablishment = false,
                ExpectedLaName = null,
                ExpectedEstablishmentName = null
            },
            // If the establishment is within the range of TPS establishment types and the employers name corresponds to an LA name from GIAS, use the short description from TPS establishment types
            new TpsEstablishmentRefreshTestScenarioData
            {
                TpsEstablishments = new[]
                {
                    new TpsEstablishment
                    {
                        TpsEstablishmentId = Guid.NewGuid(),
                        LaCode = KnownGiasLaCodeHackney,
                        EstablishmentCode = KnownTpsEstablishmentNumberWithinTpsEstablishmentTypeRange,
                        EmployersName = KnownGiasLaNameHackney,
                        SchoolGiasName = null,
                        SchoolClosedDate = null
                    }
                },
                IsExpectedToGenerateEstablishment = true,
                ExpectedLaName = KnownGiasLaNameHackney,
                ExpectedEstablishmentName = KnownTpsEstablishmentTypeShortDescription
            },
            // If the establishment is within the range of TPS establishment types and the employers name corresponds to an LA name from TPS, use the short description from TPS establishment types
            new TpsEstablishmentRefreshTestScenarioData
            {
                TpsEstablishments = new[]
                {
                    new TpsEstablishment
                    {
                        TpsEstablishmentId = Guid.NewGuid(),
                        LaCode = KnownGiasLaCodeCityOfLondon,
                        EstablishmentCode = KnownTpsEstablishmentNumberWithinTpsEstablishmentTypeRange,
                        EmployersName = KnownTpsLaNameCorporationOfLondon,
                        SchoolGiasName = null,
                        SchoolClosedDate = null
                    }
                },
                IsExpectedToGenerateEstablishment = true,
                ExpectedLaName = KnownGiasLaNameCityOfLondon,
                ExpectedEstablishmentName = KnownTpsEstablishmentTypeShortDescription
            },
            // If the establishment is within the range of TPS establishment types and the employers name does not correspond to an LA name from GIAS, use the employers name from TPS
            new TpsEstablishmentRefreshTestScenarioData
            {
                TpsEstablishments = new[]
                {
                    new TpsEstablishment
                    {
                        TpsEstablishmentId = Guid.NewGuid(),
                        LaCode = KnownGiasLaCodeHackney,
                        EstablishmentCode = KnownTpsEstablishmentNumberOutsideTpsEstablishmentTypeRange,
                        EmployersName = "Employers Name",
                        SchoolGiasName = null,
                        SchoolClosedDate = null
                    }
                },
                IsExpectedToGenerateEstablishment = true,
                ExpectedLaName = KnownGiasLaNameHackney,
                ExpectedEstablishmentName = "Employers Name"
            },
            // If the LA code is not in the GIAS data, set the LA Name to null
            new TpsEstablishmentRefreshTestScenarioData
            {
                TpsEstablishments = new[]
                {
                    new TpsEstablishment
                    {
                        TpsEstablishmentId = Guid.NewGuid(),
                        LaCode = KnownTpsLaCode,
                        EstablishmentCode = KnownTpsEstablishmentNumberOutsideTpsEstablishmentTypeRange,
                        EmployersName = "Employers Name",
                        SchoolGiasName = "School Gias Name",
                        SchoolClosedDate = null
                    }
                },
                IsExpectedToGenerateEstablishment = true,
                ExpectedLaName = null,
                ExpectedEstablishmentName = "Employers Name"
            },
            // If there are multiple records for the same LA code and establishment code, prefer the first one where the school closed date is null
            new TpsEstablishmentRefreshTestScenarioData
            {
                TpsEstablishments = new[]
                {
                    new TpsEstablishment
                    {
                        TpsEstablishmentId = Guid.NewGuid(),
                        LaCode = KnownGiasLaCodeHackney,
                        EstablishmentCode = KnownTpsEstablishmentNumberOutsideTpsEstablishmentTypeRange,
                        EmployersName = "Employers Name 1",
                        SchoolGiasName = null,
                        SchoolClosedDate = DateOnly.ParseExact("01/01/2023", "dd/MM/yyyy")
                    },
                    new TpsEstablishment
                    {
                        TpsEstablishmentId = Guid.NewGuid(),
                        LaCode = KnownGiasLaCodeHackney,
                        EstablishmentCode = KnownTpsEstablishmentNumberOutsideTpsEstablishmentTypeRange,
                        EmployersName = "Employers Name 2",
                        SchoolGiasName = null,
                        SchoolClosedDate = null
                    }
                },
                IsExpectedToGenerateEstablishment = true,
                ExpectedLaName = KnownGiasLaNameHackney,
                ExpectedEstablishmentName = "Employers Name 2"
            }
        };
    }

    [Theory]
    [MemberData(nameof(GetImportFileTestScenarioData))]
    public async Task ImportFile_WithRowData_InsertsRecordsAsExpected(TpsEstablishmentFileImportTestScenarioData scenarioData)
    {
        // Arrange
        var tpsExtractStorageService = Mock.Of<ITpsExtractStorageService>();
        var dbContextFactory = DbFixture.GetDbContextFactory();
        var clock = new TestableClock();
        var tpsCsvExtractId = Guid.NewGuid();
        var filename = "establishments/test.csv";
        var csvContent = new StringBuilder();
        csvContent.AppendLine("LA Code,Establishment Code,EMPS Name,SCHL (GIAS) Name,School Closed Date");
        csvContent.AppendLine($"{scenarioData.Row.LaCode},{scenarioData.Row.EstablishmentCode},{scenarioData.Row.EmployersName},{scenarioData.Row.SchoolGiasName},{scenarioData.Row.SchoolClosedDate}");

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent.ToString()));
        Mock.Get(tpsExtractStorageService)
            .Setup(x => x.GetFile(filename, CancellationToken.None))
            .ReturnsAsync(stream);

        // Act
        var refresher = new TpsEstablishmentRefresher(
            tpsExtractStorageService,
            dbContextFactory);
        await refresher.ImportFile(filename, CancellationToken.None);

        // Assert
        using var dbContext = dbContextFactory.CreateDbContext();
        var establishment = dbContext.TpsEstablishments.FirstOrDefault();
        if (scenarioData.IsExpectedToBeImported)
        {
            Assert.NotNull(establishment);
            Assert.Equal(scenarioData.Row.LaCode, establishment.LaCode);
            Assert.Equal(scenarioData.Row.EstablishmentCode, establishment.EstablishmentCode);
            Assert.Equal(scenarioData.Row.EmployersName, establishment.EmployersName);
            Assert.Equal(scenarioData.Row.SchoolGiasName, establishment.SchoolGiasName);
            if (!string.IsNullOrEmpty(scenarioData.Row.SchoolClosedDate))
            {
                Assert.Equal(DateOnly.ParseExact(scenarioData.Row.SchoolClosedDate, "dd/MM/yyyy"), establishment.SchoolClosedDate);
            }
        }
        else
        {
            Assert.Null(establishment);
        }
    }

    [Theory]
    [MemberData(nameof(GetRefreshEstablishmentsTestScenarioData))]
    public async Task RefreshEstablishments_WithTpsEstablishments_UpdatesEstablishmentsAsExpected(TpsEstablishmentRefreshTestScenarioData scenarioData)
    {
        // Arrange
        var tpsExtractStorageService = Mock.Of<ITpsExtractStorageService>();
        var dbContextFactory = DbFixture.GetDbContextFactory();
        var giasEstablishmentHackney = await TestData.CreateEstablishment(localAuthorityCode: KnownGiasLaCodeHackney, localAuthorityName: KnownGiasLaNameHackney, establishmentNumber: KnownGiasEstablishmentNumberHackney, establishmentName: KnownGiasEstablishmentNameHackney);
        var giasEstablishmentCityOfLondon = await TestData.CreateEstablishment(localAuthorityCode: KnownGiasLaCodeCityOfLondon, localAuthorityName: KnownGiasLaNameCityOfLondon, establishmentNumber: KnownGiasEstablishmentNumberCityOfLondon, establishmentName: KnownGiasEstablishmentNameCityOfLondon);

        using var dbContext = dbContextFactory.CreateDbContext();
        foreach (var tpsEstablishment in scenarioData.TpsEstablishments)
        {
            await dbContext.TpsEstablishments.AddAsync(tpsEstablishment);
        }
        await dbContext.SaveChangesAsync();

        // Act
        var refresher = new TpsEstablishmentRefresher(
            tpsExtractStorageService,
            dbContextFactory);
        await refresher.RefreshEstablishments(CancellationToken.None);

        // Assert
        var nonGiasEstablishments = await dbContext.Establishments
            .Where(e => e.EstablishmentSourceId == 2)
            .ToListAsync();
        if (scenarioData.IsExpectedToGenerateEstablishment)
        {
            var establishment = Assert.Single(nonGiasEstablishments);
            Assert.Equal(scenarioData.ExpectedLaName, establishment.LaName);
            Assert.Equal(scenarioData.ExpectedEstablishmentName, establishment.EstablishmentName);
        }
        else
        {
            Assert.Empty(nonGiasEstablishments);
        }
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => DbFixture.DbHelper.ClearData();

    private DbFixture DbFixture { get; }

    private TestData TestData { get; }

    private TestableClock Clock { get; }

    public TrsDataSyncHelper Helper { get; }
}

public class TpsEstablishmentFileImportTestScenarioData
{
    public required TpsEstablishmentCsvRow Row { get; init; }
    public required bool IsExpectedToBeImported { get; init; }
}

public class TpsEstablishmentRefreshTestScenarioData
{
    public required TpsEstablishment[] TpsEstablishments { get; init; }
    public required bool IsExpectedToGenerateEstablishment { get; init; }
    public required string? ExpectedLaName { get; init; }
    public required string? ExpectedEstablishmentName { get; init; }
}
