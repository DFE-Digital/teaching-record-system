using Microsoft.PowerPlatform.Dataverse.Client;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Services.TrsDataSync;
using TeachingRecordSystem.Core.Services.WorkforceData;

namespace TeachingRecordSystem.Core.Tests.Services.WorkforceData;

public class TpsCsvExtractProcessorTests
{
    public TpsCsvExtractProcessorTests(
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

    [Fact]
    public async Task ProcessNonMatchingTrns_WhenCalledWithTrnsNotMatchingPersonsInTrs_SetsResultToInvalidTrn()
    {
        // Arrange
        var establishment1 = await TestData.CreateEstablishment(localAuthorityCode: "123", establishmentNumber: "1234");

        var trn = await TestData.GenerateTrn();
        var tpsCsvExtractId = Guid.NewGuid();
        await TestData.CreateTpsCsvExtract(b => b.WithTpsCsvExtractId(tpsCsvExtractId).WithItem(trn, establishment1.LaCode, establishment1.EstablishmentNumber, establishment1.Postcode!, new DateOnly(2023, 02, 03)));

        // Act
        var processor = new TpsCsvExtractProcessor(
            TestData.DbContextFactory,
            TestData.Clock);
        await processor.ProcessNonMatchingTrns(tpsCsvExtractId, CancellationToken.None);

        // Assert
        using var dbContext = TestData.DbContextFactory.CreateDbContext();
        var items = await dbContext.TpsCsvExtractItems.Where(i => i.TpsCsvExtractId == tpsCsvExtractId).ToListAsync();
        Assert.All(items, i => Assert.Equal(TpsCsvExtractItemResult.InvalidTrn, i.Result));
    }

    [Fact]
    public async Task ProcessNonMatchingEstablishments_WhenCalledWithEstablishmentsNotMatchingEstablishmentsInTrs_SetsResultToInvalidEstablishment()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var tpsCsvExtractId = Guid.NewGuid();
        var establishment1 = await TestData.CreateEstablishment(localAuthorityCode: "124", establishmentNumber: "1235");
        var nonExistentEstablishmentNumber = "4321";
        await TestData.CreateTpsCsvExtract(b => b.WithTpsCsvExtractId(tpsCsvExtractId).WithItem(person.Trn!, establishment1.LaCode, nonExistentEstablishmentNumber, establishment1.Postcode!, new DateOnly(2023, 02, 03)));

        // Act
        var processor = new TpsCsvExtractProcessor(
            TestData.DbContextFactory,
            TestData.Clock);
        await processor.ProcessNonMatchingEstablishments(tpsCsvExtractId, CancellationToken.None);

        // Assert
        using var dbContext = TestData.DbContextFactory.CreateDbContext();
        var items = await dbContext.TpsCsvExtractItems.Where(i => i.TpsCsvExtractId == tpsCsvExtractId).ToListAsync();
        Assert.All(items, i => Assert.Equal(TpsCsvExtractItemResult.InvalidEstablishment, i.Result));
    }

    [Fact]
    public async Task ProcessNewEmploymentHistory_WhenCalledWithNewEmploymentHistory_InsertsNewPersonEmploymentRecord()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var tpsCsvExtractId = Guid.NewGuid();
        var establishment = await TestData.CreateEstablishment(localAuthorityCode: "125", establishmentNumber: "1236");
        await TestData.CreateTpsCsvExtract(b => b.WithTpsCsvExtractId(tpsCsvExtractId).WithItem(person!.Trn!, establishment.LaCode, establishment.EstablishmentNumber, establishment.Postcode!, new DateOnly(2023, 02, 03)));

        // Act
        var processor = new TpsCsvExtractProcessor(
            TestData.DbContextFactory,
            TestData.Clock);
        await processor.ProcessNewEmploymentHistory(tpsCsvExtractId, CancellationToken.None);

        // Assert
        using var dbContext = TestData.DbContextFactory.CreateDbContext();
        var items = await dbContext.TpsCsvExtractItems.Where(i => i.TpsCsvExtractId == tpsCsvExtractId).ToListAsync();
        Assert.All(items, i => Assert.Equal(TpsCsvExtractItemResult.ValidDataAdded, i.Result));
        var employmentHistory = await dbContext.PersonEmployments.Where(e => e.PersonId == person.PersonId).ToListAsync();
        Assert.Single(employmentHistory);
        var personEmployment = employmentHistory.Single();
        Assert.Equal(establishment.EstablishmentId, personEmployment.EstablishmentId);
    }

    [Fact]
    public async Task ProcessNewEmploymentHistory_ForLaCodeAndEstablishmentNumberWithMultipleEstablishmentEntries_MatchesToTheMostOpenEstablishment()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var tpsCsvExtractId = Guid.NewGuid();
        var laCode = "321";
        var establishmentNumber = "4321";
        var postcode = Faker.Address.UkPostCode();
        var closedEstablishment = await TestData.CreateEstablishment(laCode, establishmentNumber: establishmentNumber, establishmentStatusCode: 2, postcode: postcode);
        var openEstablishment = await TestData.CreateEstablishment(laCode, establishmentNumber: establishmentNumber, establishmentStatusCode: 1, postcode: postcode);
        var proposedToOpenEstablishment = await TestData.CreateEstablishment(laCode, establishmentNumber: establishmentNumber, establishmentStatusCode: 4, postcode: postcode);
        var openButProposedToCloseEstablishment = await TestData.CreateEstablishment(laCode, establishmentNumber: establishmentNumber, establishmentStatusCode: 3, postcode: postcode);
        await TestData.CreateTpsCsvExtract(b => b.WithTpsCsvExtractId(tpsCsvExtractId).WithItem(person!.Trn!, laCode, establishmentNumber, postcode, new DateOnly(2023, 02, 03)));

        // Act
        var processor = new TpsCsvExtractProcessor(
            TestData.DbContextFactory,
            TestData.Clock);
        await processor.ProcessNewEmploymentHistory(tpsCsvExtractId, CancellationToken.None);

        // Assert
        using var dbContext = TestData.DbContextFactory.CreateDbContext();
        var items = await dbContext.TpsCsvExtractItems.Where(i => i.TpsCsvExtractId == tpsCsvExtractId).ToListAsync();
        Assert.All(items, i => Assert.Equal(TpsCsvExtractItemResult.ValidDataAdded, i.Result));
        var employmentHistory = await dbContext.PersonEmployments.Where(e => e.PersonId == person.PersonId).ToListAsync();
        Assert.Single(employmentHistory);
        var personEmployment = employmentHistory.Single();
        Assert.Equal(openEstablishment.EstablishmentId, personEmployment.EstablishmentId);
    }

    [Fact]
    public async Task ProcessNewEmploymentHistory_WithValidData_OnlyMatchesToLaCodeAndPostCodeForHigherEducationIfNoMatchOnLaCodeAndEstablishment()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var tpsCsvExtractId = Guid.NewGuid();
        var laCode1 = "322";
        var establishmentNumber1 = "4322";
        var postcode1 = Faker.Address.UkPostCode();
        var laCode2 = "323";
        var establishmentNumber2 = "4323";
        var postcode2 = Faker.Address.UkPostCode();
        var nonHigherEducationEstablishment1 = await TestData.CreateEstablishment(laCode1, establishmentNumber: establishmentNumber1, postcode: postcode1);
        var higherEductionEstablishment1 = await TestData.CreateEstablishment(laCode1, establishmentNumber: establishmentNumber1, postcode: postcode1, isHigherEducationInstitution: true);
        var higherEductionEstablishment2 = await TestData.CreateEstablishment(laCode2, postcode: postcode2, isHigherEducationInstitution: true);
        await TestData.CreateTpsCsvExtract(
            b => b.WithTpsCsvExtractId(tpsCsvExtractId)
                .WithItem(person!.Trn!, laCode1, establishmentNumber1, postcode1, new DateOnly(2023, 02, 03))
                .WithItem(person!.Trn!, laCode2, establishmentNumber2, postcode2, new DateOnly(2023, 02, 03)));

        // Act
        var processor = new TpsCsvExtractProcessor(
            TestData.DbContextFactory,
            TestData.Clock);
        await processor.ProcessNewEmploymentHistory(tpsCsvExtractId, CancellationToken.None);

        // Assert
        using var dbContext = TestData.DbContextFactory.CreateDbContext();
        var items = await dbContext.TpsCsvExtractItems.Where(i => i.TpsCsvExtractId == tpsCsvExtractId).ToListAsync();
        Assert.All(items, i => Assert.Equal(TpsCsvExtractItemResult.ValidDataAdded, i.Result));
        var employmentHistory = await dbContext.PersonEmployments.Where(e => e.PersonId == person.PersonId).ToListAsync();
        Assert.Equal(2, employmentHistory.Count);
        Assert.Contains(nonHigherEducationEstablishment1.EstablishmentId, employmentHistory.Select(pe => pe.EstablishmentId));
        Assert.Contains(higherEductionEstablishment2.EstablishmentId, employmentHistory.Select(pe => pe.EstablishmentId));
    }

    [Fact]
    public async Task ProcessUpdatedEmploymentHistory_WhenCalledWithUpdatedEmploymentHistory_UpdatesPersonEmploymentRecord()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var tpsCsvExtractId = Guid.NewGuid();
        var establishment1 = await TestData.CreateEstablishment(localAuthorityCode: "126", establishmentNumber: "1237");
        var existingPersonEmployment = await TestData.CreatePersonEmployment(person.PersonId, establishment1.EstablishmentId, new DateOnly(2023, 02, 02), EmploymentType.FullTime);
        var updatedEndDate = new DateOnly(2024, 03, 06);
        await TestData.CreateTpsCsvExtract(b => b.WithTpsCsvExtractId(tpsCsvExtractId).WithItem(person!.Trn!, establishment1.LaCode, establishment1.EstablishmentNumber, establishment1.Postcode!, new DateOnly(2023, 02, 02), updatedEndDate));

        // Act
        var processor = new TpsCsvExtractProcessor(
            TestData.DbContextFactory,
            TestData.Clock);
        await processor.ProcessUpdatedEmploymentHistory(tpsCsvExtractId, CancellationToken.None);

        // Assert
        using var dbContext = TestData.DbContextFactory.CreateDbContext();
        var items = await dbContext.TpsCsvExtractItems.Where(i => i.TpsCsvExtractId == tpsCsvExtractId).ToListAsync();
        Assert.All(items, i => Assert.Equal(TpsCsvExtractItemResult.ValidDataUpdated, i.Result));
        var updatedPersonEmployment = await dbContext.PersonEmployments.SingleAsync(e => e.PersonEmploymentId == existingPersonEmployment.PersonEmploymentId);
        Assert.Equal(updatedEndDate, updatedPersonEmployment.EndDate);
    }

    [Fact]
    public async Task ProcessUpdatedEmploymentHistory_WhenCalledWithUpdatedEmploymentHistoryWithNoChanges_SetsResultToValidNoChanges()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var tpsCsvExtractId = Guid.NewGuid();
        var establishment1 = await TestData.CreateEstablishment(localAuthorityCode: "126", establishmentNumber: "1237");
        var existingPersonEmployment = await TestData.CreatePersonEmployment(person.PersonId, establishment1.EstablishmentId, new DateOnly(2023, 02, 02), EmploymentType.FullTime);
        var updatedEndDate = new DateOnly(2024, 03, 06);
        await TestData.CreateTpsCsvExtract(b => b.WithTpsCsvExtractId(tpsCsvExtractId).WithItem(person!.Trn!, establishment1.LaCode, establishment1.EstablishmentNumber, establishment1.Postcode!, existingPersonEmployment.StartDate, existingPersonEmployment.EndDate, "FT"));

        // Act
        var processor = new TpsCsvExtractProcessor(
            TestData.DbContextFactory,
            TestData.Clock);
        await processor.ProcessUpdatedEmploymentHistory(tpsCsvExtractId, CancellationToken.None);

        // Assert
        using var dbContext = TestData.DbContextFactory.CreateDbContext();
        var items = await dbContext.TpsCsvExtractItems.Where(i => i.TpsCsvExtractId == tpsCsvExtractId).ToListAsync();
        Assert.All(items, i => Assert.Equal(TpsCsvExtractItemResult.ValidNoChange, i.Result));
    }

    private DbFixture DbFixture { get; }

    private TestData TestData { get; }

    private TestableClock Clock { get; }

    public TrsDataSyncHelper Helper { get; }
}
