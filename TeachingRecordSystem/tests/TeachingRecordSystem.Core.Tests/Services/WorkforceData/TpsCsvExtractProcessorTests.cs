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
        var startDate = new DateOnly(2023, 02, 03);
        var endDate = new DateOnly(2024, 03, 30);
        var extractDate = new DateOnly(2024, 04, 25);
        await TestData.CreateTpsCsvExtract(b => b.WithTpsCsvExtractId(tpsCsvExtractId).WithItem(trn, establishment1.LaCode, establishment1.EstablishmentNumber, establishment1.Postcode!, startDate, endDate, extractDate));

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
        var startDate = new DateOnly(2023, 02, 03);
        var endDate = new DateOnly(2024, 03, 30);
        var extractDate = new DateOnly(2024, 04, 25);
        await TestData.CreateTpsCsvExtract(b => b.WithTpsCsvExtractId(tpsCsvExtractId).WithItem(person.Trn!, establishment1.LaCode, nonExistentEstablishmentNumber, establishment1.Postcode!, startDate, endDate, extractDate));

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
        var startDate = new DateOnly(2023, 02, 03);
        var endDate = new DateOnly(2024, 03, 30);
        var extractDate = new DateOnly(2024, 04, 25);
        await TestData.CreateTpsCsvExtract(b => b.WithTpsCsvExtractId(tpsCsvExtractId).WithItem(person!.Trn!, establishment.LaCode, establishment.EstablishmentNumber, establishment.Postcode!, startDate, endDate, extractDate));

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
        var startDate = new DateOnly(2023, 02, 03);
        var endDate = new DateOnly(2024, 03, 30);
        var extractDate = new DateOnly(2024, 04, 25);
        await TestData.CreateTpsCsvExtract(b => b.WithTpsCsvExtractId(tpsCsvExtractId).WithItem(person!.Trn!, laCode, establishmentNumber, postcode, startDate, endDate, extractDate));

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
        var establishmentNumber2 = "4323";
        var postcode2 = Faker.Address.UkPostCode();
        var nonHigherEducationEstablishment = await TestData.CreateEstablishment(laCode1, establishmentNumber: establishmentNumber1, postcode: postcode1);
        var higherEductionEstablishment = await TestData.CreateEstablishment(laCode1, postcode: postcode2, isHigherEducationInstitution: true);
        await TestData.CreateTpsCsvExtract(
            b => b.WithTpsCsvExtractId(tpsCsvExtractId)
                .WithItem(person!.Trn!, laCode1, establishmentNumber1, postcode1, new DateOnly(2023, 02, 03), new DateOnly(2024, 03, 30), new DateOnly(2024, 04, 25))
                .WithItem(person!.Trn!, laCode1, establishmentNumber2, postcode2, new DateOnly(2023, 04, 05), new DateOnly(2024, 03, 30), new DateOnly(2024, 04, 25)));

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
        Assert.Contains(nonHigherEducationEstablishment.EstablishmentId, employmentHistory.Select(pe => pe.EstablishmentId));
        Assert.Contains(higherEductionEstablishment.EstablishmentId, employmentHistory.Select(pe => pe.EstablishmentId));
    }

    [Fact]
    public async Task ProcessUpdatedEmploymentHistory_WhenCalledWithUpdatedEmploymentHistory_UpdatesPersonEmploymentRecord()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var tpsCsvExtractId = Guid.NewGuid();
        var establishment1 = await TestData.CreateEstablishment(localAuthorityCode: "126", establishmentNumber: "1237");
        var nationalInsuranceNumber = TestData.GenerateNationalInsuranceNumber();
        var personPostcode = Faker.Address.UkPostCode();
        var existingPersonEmployment = await TestData.CreatePersonEmployment(person, establishment1, new DateOnly(2023, 02, 02), new DateOnly(2024, 02, 29), EmploymentType.FullTime, new DateOnly(2024, 03, 25), nationalInsuranceNumber, personPostcode);
        var updatedEndDate = new DateOnly(2024, 03, 30);
        var updatedLastExtractDate = new DateOnly(2024, 04, 25);
        await TestData.CreateTpsCsvExtract(b => b.WithTpsCsvExtractId(tpsCsvExtractId).WithItem(person!.Trn!, establishment1.LaCode, establishment1.EstablishmentNumber, establishment1.Postcode!, new DateOnly(2023, 02, 02), updatedEndDate, updatedLastExtractDate));

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
        Assert.Equal(updatedEndDate, updatedPersonEmployment.LastKnownEmployedDate);
        Assert.Equal(updatedLastExtractDate, updatedPersonEmployment.LastExtractDate);
    }

    [Fact]
    public async Task ProcessUpdatedEmploymentHistory_WhenCalledWithUpdatedEmploymentHistoryWithNoChanges_SetsResultToValidNoChanges()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var tpsCsvExtractId = Guid.NewGuid();
        var establishment1 = await TestData.CreateEstablishment(localAuthorityCode: "126", establishmentNumber: "1237");
        var nationalInsuranceNumber = TestData.GenerateNationalInsuranceNumber();
        var personPostcode = Faker.Address.UkPostCode();
        var existingPersonEmployment = await TestData.CreatePersonEmployment(person, establishment1, new DateOnly(2023, 02, 02), new DateOnly(2024, 02, 29), EmploymentType.FullTime, new DateOnly(2024, 03, 25), nationalInsuranceNumber, personPostcode);
        await TestData.CreateTpsCsvExtract(b => b.WithTpsCsvExtractId(tpsCsvExtractId).WithItem(person!.Trn!, establishment1.LaCode, establishment1.EstablishmentNumber, establishment1.Postcode!, existingPersonEmployment.StartDate, existingPersonEmployment.LastKnownEmployedDate, existingPersonEmployment.LastExtractDate, "FT"));

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

    [Fact]
    public async Task UpdateLatestEstablishmentVersions_WithEstablishmentChangingUrn_UpdatesPersonEmploymentRecord()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var establishment1 = await TestData.CreateEstablishment(localAuthorityCode: "127", establishmentNumber: "1238", establishmentStatusCode: 2); // Closed
        var establishment2 = await TestData.CreateEstablishment(localAuthorityCode: "127", establishmentNumber: "1238", establishmentStatusCode: 1); // Open
        var nationalInsuranceNumber = TestData.GenerateNationalInsuranceNumber();
        var personPostcode = Faker.Address.UkPostCode();
        var existingPersonEmployment = await TestData.CreatePersonEmployment(person, establishment1, new DateOnly(2023, 02, 02), new DateOnly(2024, 02, 29), EmploymentType.FullTime, new DateOnly(2024, 03, 25), nationalInsuranceNumber, personPostcode);

        // Act
        var processor = new TpsCsvExtractProcessor(
            TestData.DbContextFactory,
            TestData.Clock);
        await processor.UpdateLatestEstablishmentVersions(CancellationToken.None);

        // Assert
        using var dbContext = TestData.DbContextFactory.CreateDbContext();
        var updatedPersonEmployment = await dbContext.PersonEmployments.SingleAsync(e => e.PersonEmploymentId == existingPersonEmployment.PersonEmploymentId);
        Assert.Equal(establishment2.EstablishmentId, updatedPersonEmployment.EstablishmentId);
    }

    [Fact]
    public async Task ProcessEndedEmployments_WithLastKnownEmployedDateGreaterThanThreeMonthsBeforeLastExtractDate_SetsEndDateOnPersonEmploymentRecord()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var establishment1 = await TestData.CreateEstablishment(localAuthorityCode: "128", establishmentNumber: "1239");
        var establishment2 = await TestData.CreateEstablishment(localAuthorityCode: "128", establishmentNumber: "1240");
        var extractDate = new DateOnly(2024, 04, 25);
        var lastKnownEmployedDateWithinThreeMonthsOfExtractDate = new DateOnly(2024, 02, 29);
        var lastKnownEmployedDateOutsideThreeMonthsOfExtractDate = new DateOnly(2023, 09, 30);
        var nationalInsuranceNumber = TestData.GenerateNationalInsuranceNumber();
        var personPostcode = Faker.Address.UkPostCode();
        var personEmploymentWhichHasEnded = await TestData.CreatePersonEmployment(person, establishment1, new DateOnly(2023, 02, 02), lastKnownEmployedDateOutsideThreeMonthsOfExtractDate, EmploymentType.FullTime, extractDate, nationalInsuranceNumber, personPostcode);
        var personEmploymentWhichHasNotEnded = await TestData.CreatePersonEmployment(person, establishment2, new DateOnly(2023, 02, 02), lastKnownEmployedDateWithinThreeMonthsOfExtractDate, EmploymentType.FullTime, extractDate, nationalInsuranceNumber, personPostcode);

        // Act
        var processor = new TpsCsvExtractProcessor(
            TestData.DbContextFactory,
            TestData.Clock);
        await processor.ProcessEndedEmployments(CancellationToken.None);

        // Assert
        using var dbContext = TestData.DbContextFactory.CreateDbContext();
        var updatedPersonEmploymentWhichShouldHaveEndDateSet = await dbContext.PersonEmployments.SingleAsync(e => e.PersonEmploymentId == personEmploymentWhichHasEnded.PersonEmploymentId);
        var updatedPersonEmploymentWhichShouldNotHaveEndDateSet = await dbContext.PersonEmployments.SingleAsync(e => e.PersonEmploymentId == personEmploymentWhichHasNotEnded.PersonEmploymentId);
        Assert.Equal(lastKnownEmployedDateOutsideThreeMonthsOfExtractDate, updatedPersonEmploymentWhichShouldHaveEndDateSet.EndDate);
        Assert.Null(updatedPersonEmploymentWhichShouldNotHaveEndDateSet.EndDate);
    }

    [Fact]
    public async Task BackfillNinoAndPersonPostcodeInEmploymentHistory_WhenCalledWithPersonEmploymentRecordsWithoutNinoAndPersonPostcode_SetsNinoAndPersonPostcode()
    {
        // Arrange
        var person = await TestData.CreatePerson();
        var tpsCsvExtractId = Guid.NewGuid();
        var establishment = await TestData.CreateEstablishment(localAuthorityCode: "129", establishmentNumber: "1241");
        var nationalInsuranceNumber = TestData.GenerateNationalInsuranceNumber();
        var memberPostcode = Faker.Address.UkPostCode();
        var personEmploymentWithoutNinoAndPersonPostcode = await TestData.CreatePersonEmployment(person, establishment, new DateOnly(2023, 02, 02), new DateOnly(2024, 02, 29), EmploymentType.FullTime, new DateOnly(2024, 03, 25), null, null);
        await TestData.CreateTpsCsvExtract(b => b.WithTpsCsvExtractId(tpsCsvExtractId).WithItem(person!.Trn!, establishment.LaCode, establishment.EstablishmentNumber, establishment.Postcode!, personEmploymentWithoutNinoAndPersonPostcode.StartDate, personEmploymentWithoutNinoAndPersonPostcode.LastKnownEmployedDate, personEmploymentWithoutNinoAndPersonPostcode.LastExtractDate, "FT", nationalInsuranceNumber, memberPostcode: memberPostcode));

        // Act
        var processor = new TpsCsvExtractProcessor(
            TestData.DbContextFactory,
            TestData.Clock);
        await processor.BackfillNinoAndPersonPostcodeInEmploymentHistory(CancellationToken.None);

        // Assert
        using var dbContext = TestData.DbContextFactory.CreateDbContext();
        var updatedPersonEmployment = await dbContext.PersonEmployments.SingleAsync(e => e.PersonEmploymentId == personEmploymentWithoutNinoAndPersonPostcode.PersonEmploymentId);
        Assert.Equal(nationalInsuranceNumber, updatedPersonEmployment.NationalInsuranceNumber);
        Assert.Equal(memberPostcode, updatedPersonEmployment.PersonPostcode);
    }

    private DbFixture DbFixture { get; }

    private TestData TestData { get; }

    private TestableClock Clock { get; }

    public TrsDataSyncHelper Helper { get; }
}
