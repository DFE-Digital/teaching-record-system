using TeachingRecordSystem.Core.DataStore.Postgres;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.WorkforceData;

namespace TeachingRecordSystem.Core.Tests.Services.WorkforceData;

[Collection(nameof(WorkforceDataTestCollection))]
public class TpsCsvExtractProcessorTests(CoreFixture fixture) : IAsyncLifetime
{
    public IDbContextFactory<TrsDbContext> DbContextFactory => fixture.DbContextFactory;

    public TestData TestData => fixture.TestData;

    public TestableClock Clock => fixture.Clock;

    [Fact]
    public async Task ProcessNonMatchingTrns_WhenCalledWithTrnsNotMatchingPersonsInTrs_SetsResultToInvalidTrn()
    {
        // Arrange
        var establishment1 = await TestData.CreateEstablishmentAsync(localAuthorityCode: "123", establishmentNumber: "1234");

        var trn = await TestData.GenerateTrnAsync();
        var tpsCsvExtractId = Guid.NewGuid();
        var startDate = new DateOnly(2023, 02, 03);
        var endDate = new DateOnly(2024, 03, 30);
        var extractDate = new DateOnly(2024, 04, 25);
        await TestData.CreateTpsCsvExtractAsync(b => b.WithTpsCsvExtractId(tpsCsvExtractId).WithItem(trn, establishment1.LaCode, establishment1.EstablishmentNumber, establishment1.Postcode!, startDate, endDate, extractDate));

        // Act
        var processor = new TpsCsvExtractProcessor(
            TestData.DbContextFactory,
            TestData.Clock);
        await processor.ProcessNonMatchingTrnsAsync(tpsCsvExtractId, CancellationToken.None);

        // Assert
        using var dbContext = TestData.DbContextFactory.CreateDbContext();
        var items = await dbContext.TpsCsvExtractItems.Where(i => i.TpsCsvExtractId == tpsCsvExtractId).ToListAsync();
        Assert.All(items, i => Assert.Equal(TpsCsvExtractItemResult.InvalidTrn, i.Result));
    }

    [Fact]
    public async Task ProcessNonMatchingEstablishments_WhenCalledWithEstablishmentsNotMatchingEstablishmentsInTrs_SetsResultToInvalidEstablishment()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var tpsCsvExtractId = Guid.NewGuid();
        var establishment1 = await TestData.CreateEstablishmentAsync(localAuthorityCode: "124", establishmentNumber: "1235");
        var nonExistentEstablishmentNumber = "4321";
        var startDate = new DateOnly(2023, 02, 03);
        var endDate = new DateOnly(2024, 03, 30);
        var extractDate = new DateOnly(2024, 04, 25);
        await TestData.CreateTpsCsvExtractAsync(b => b.WithTpsCsvExtractId(tpsCsvExtractId).WithItem(person.Trn!, establishment1.LaCode, nonExistentEstablishmentNumber, establishment1.Postcode!, startDate, endDate, extractDate));

        // Act
        var processor = new TpsCsvExtractProcessor(
            TestData.DbContextFactory,
            TestData.Clock);
        await processor.ProcessNonMatchingEstablishmentsAsync(tpsCsvExtractId, CancellationToken.None);

        // Assert
        using var dbContext = TestData.DbContextFactory.CreateDbContext();
        var items = await dbContext.TpsCsvExtractItems.Where(i => i.TpsCsvExtractId == tpsCsvExtractId).ToListAsync();
        Assert.All(items, i => Assert.Equal(TpsCsvExtractItemResult.InvalidEstablishment, i.Result));
    }

    [Fact]
    public async Task ProcessNewEmploymentHistory_WhenCalledWithNewEmploymentHistory_InsertsNewPersonEmploymentRecord()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var tpsCsvExtractId = Guid.NewGuid();
        var establishment = await TestData.CreateEstablishmentAsync(localAuthorityCode: "125", establishmentNumber: "1236");
        var startDate = new DateOnly(2023, 02, 03);
        var endDate = new DateOnly(2024, 03, 30);
        var extractDate = new DateOnly(2024, 04, 25);
        await TestData.CreateTpsCsvExtractAsync(b => b.WithTpsCsvExtractId(tpsCsvExtractId).WithItem(person!.Trn!, establishment.LaCode, establishment.EstablishmentNumber, establishment.Postcode!, startDate, endDate, extractDate));

        // Act
        var processor = new TpsCsvExtractProcessor(
            TestData.DbContextFactory,
            TestData.Clock);
        await processor.ProcessNewEmploymentHistoryAsync(tpsCsvExtractId, CancellationToken.None);

        // Assert
        using var dbContext = TestData.DbContextFactory.CreateDbContext();
        var items = await dbContext.TpsCsvExtractItems.Where(i => i.TpsCsvExtractId == tpsCsvExtractId).ToListAsync();
        Assert.All(items, i => Assert.Equal(TpsCsvExtractItemResult.ValidDataAdded, i.Result));
        var employmentHistory = await dbContext.TpsEmployments.Where(e => e.PersonId == person.PersonId).ToListAsync();
        Assert.Single(employmentHistory);
        var personEmployment = employmentHistory.Single();
        Assert.Equal(establishment.EstablishmentId, personEmployment.EstablishmentId);
        Assert.Null(personEmployment.EndDate);
    }

    [Fact]
    public async Task ProcessNewEmploymentHistory_WhenCalledWithEndDateInTheFuture_SetsLastKnownEmployedDateToExtractDate()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var tpsCsvExtractId = Guid.NewGuid();
        var establishment = await TestData.CreateEstablishmentAsync(localAuthorityCode: "125", establishmentNumber: "1236");
        var startDate = new DateOnly(2023, 02, 03);
        var endDate = new DateOnly(2024, 10, 30);
        var extractDate = new DateOnly(2024, 04, 25);
        await TestData.CreateTpsCsvExtractAsync(b => b.WithTpsCsvExtractId(tpsCsvExtractId).WithItem(person!.Trn!, establishment.LaCode, establishment.EstablishmentNumber, establishment.Postcode!, startDate, endDate, extractDate));

        // Act
        var processor = new TpsCsvExtractProcessor(
            TestData.DbContextFactory,
            TestData.Clock);
        await processor.ProcessNewEmploymentHistoryAsync(tpsCsvExtractId, CancellationToken.None);

        // Assert
        using var dbContext = TestData.DbContextFactory.CreateDbContext();
        var items = await dbContext.TpsCsvExtractItems.Where(i => i.TpsCsvExtractId == tpsCsvExtractId).ToListAsync();
        Assert.All(items, i => Assert.Equal(TpsCsvExtractItemResult.ValidDataAdded, i.Result));
        var employmentHistory = await dbContext.TpsEmployments.Where(e => e.PersonId == person.PersonId).ToListAsync();
        Assert.Single(employmentHistory);
        var personEmployment = employmentHistory.Single();
        Assert.Equal(establishment.EstablishmentId, personEmployment.EstablishmentId);
        Assert.Equal(extractDate, personEmployment.LastKnownTpsEmployedDate);
    }

    [Fact]
    public async Task ProcessNewEmploymentHistory_WhenCalledWithWithdrawalIndicatorSet_SetsEndDate()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var tpsCsvExtractId = Guid.NewGuid();
        var establishment = await TestData.CreateEstablishmentAsync(localAuthorityCode: "125", establishmentNumber: "1236");
        var startDate = new DateOnly(2023, 02, 03);
        var endDate = new DateOnly(2024, 03, 30);
        var extractDate = new DateOnly(2024, 04, 25);
        await TestData.CreateTpsCsvExtractAsync(b => b.WithTpsCsvExtractId(tpsCsvExtractId).WithItem(person!.Trn!, establishment.LaCode, establishment.EstablishmentNumber, establishment.Postcode!, startDate, endDate, extractDate, withdrawalIndicator: "W"));

        // Act
        var processor = new TpsCsvExtractProcessor(
            TestData.DbContextFactory,
            TestData.Clock);
        await processor.ProcessNewEmploymentHistoryAsync(tpsCsvExtractId, CancellationToken.None);

        // Assert
        using var dbContext = TestData.DbContextFactory.CreateDbContext();
        var items = await dbContext.TpsCsvExtractItems.Where(i => i.TpsCsvExtractId == tpsCsvExtractId).ToListAsync();
        Assert.All(items, i => Assert.Equal(TpsCsvExtractItemResult.ValidDataAdded, i.Result));
        var employmentHistory = await dbContext.TpsEmployments.Where(e => e.PersonId == person.PersonId).ToListAsync();
        Assert.Single(employmentHistory);
        var personEmployment = employmentHistory.Single();
        Assert.Equal(establishment.EstablishmentId, personEmployment.EstablishmentId);
        Assert.Equal(endDate, personEmployment.EndDate);
    }

    [Fact]
    public async Task ProcessNewEmploymentHistory_WhenCalledWithLastKnownEmployedDateOlderThanFiveMonths_SetsEndDate()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var tpsCsvExtractId = Guid.NewGuid();
        var establishment = await TestData.CreateEstablishmentAsync(localAuthorityCode: "125", establishmentNumber: "1236");
        var startDate = new DateOnly(2023, 02, 03);
        var endDate = new DateOnly(2023, 10, 30);
        var extractDate = new DateOnly(2024, 04, 25);
        await TestData.CreateTpsCsvExtractAsync(b => b.WithTpsCsvExtractId(tpsCsvExtractId).WithItem(person!.Trn!, establishment.LaCode, establishment.EstablishmentNumber, establishment.Postcode!, startDate, endDate, extractDate));

        // Act
        var processor = new TpsCsvExtractProcessor(
            TestData.DbContextFactory,
            TestData.Clock);
        await processor.ProcessNewEmploymentHistoryAsync(tpsCsvExtractId, CancellationToken.None);

        // Assert
        using var dbContext = TestData.DbContextFactory.CreateDbContext();
        var items = await dbContext.TpsCsvExtractItems.Where(i => i.TpsCsvExtractId == tpsCsvExtractId).ToListAsync();
        Assert.All(items, i => Assert.Equal(TpsCsvExtractItemResult.ValidDataAdded, i.Result));
        var employmentHistory = await dbContext.TpsEmployments.Where(e => e.PersonId == person.PersonId).ToListAsync();
        Assert.Single(employmentHistory);
        var personEmployment = employmentHistory.Single();
        Assert.Equal(establishment.EstablishmentId, personEmployment.EstablishmentId);
        Assert.Equal(endDate, personEmployment.EndDate);
    }


    [Fact]
    public async Task ProcessNewEmploymentHistory_ForLaCodeAndEstablishmentNumberWithMultipleEstablishmentEntries_MatchesToTheMostOpenEstablishment()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var tpsCsvExtractId = Guid.NewGuid();
        var laCode = "321";
        var establishmentNumber = "4321";
        var postcode = Faker.Address.UkPostCode();
        var closedEstablishment = await TestData.CreateEstablishmentAsync(laCode, establishmentNumber: establishmentNumber, establishmentStatusCode: 2, postcode: postcode);
        var openEstablishment = await TestData.CreateEstablishmentAsync(laCode, establishmentNumber: establishmentNumber, establishmentStatusCode: 1, postcode: postcode);
        var proposedToOpenEstablishment = await TestData.CreateEstablishmentAsync(laCode, establishmentNumber: establishmentNumber, establishmentStatusCode: 4, postcode: postcode);
        var openButProposedToCloseEstablishment = await TestData.CreateEstablishmentAsync(laCode, establishmentNumber: establishmentNumber, establishmentStatusCode: 3, postcode: postcode);
        var startDate = new DateOnly(2023, 02, 03);
        var endDate = new DateOnly(2024, 03, 30);
        var extractDate = new DateOnly(2024, 04, 25);
        await TestData.CreateTpsCsvExtractAsync(b => b.WithTpsCsvExtractId(tpsCsvExtractId).WithItem(person!.Trn!, laCode, establishmentNumber, postcode, startDate, endDate, extractDate));

        // Act
        var processor = new TpsCsvExtractProcessor(
            TestData.DbContextFactory,
            TestData.Clock);
        await processor.ProcessNewEmploymentHistoryAsync(tpsCsvExtractId, CancellationToken.None);

        // Assert
        using var dbContext = TestData.DbContextFactory.CreateDbContext();
        var items = await dbContext.TpsCsvExtractItems.Where(i => i.TpsCsvExtractId == tpsCsvExtractId).ToListAsync();
        Assert.All(items, i => Assert.Equal(TpsCsvExtractItemResult.ValidDataAdded, i.Result));
        var employmentHistory = await dbContext.TpsEmployments.Where(e => e.PersonId == person.PersonId).ToListAsync();
        Assert.Single(employmentHistory);
        var personEmployment = employmentHistory.Single();
        Assert.Equal(openEstablishment.EstablishmentId, personEmployment.EstablishmentId);
    }

    [Fact]
    public async Task ProcessNewEmploymentHistory_WithValidData_OnlyMatchesToLaCodeAndPostCodeForHigherEducationIfNoMatchOnLaCodeAndEstablishment()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var tpsCsvExtractId = Guid.NewGuid();
        var laCode1 = "322";
        var establishmentNumber1 = "4322";
        var postcode1 = Faker.Address.UkPostCode();
        var establishmentNumber2 = "4323";
        var postcode2 = Faker.Address.UkPostCode();
        var nonHigherEducationEstablishment = await TestData.CreateEstablishmentAsync(laCode1, establishmentNumber: establishmentNumber1, postcode: postcode1);
        var higherEductionEstablishment = await TestData.CreateEstablishmentAsync(laCode1, postcode: postcode2, isHigherEducationInstitution: true);
        await TestData.CreateTpsCsvExtractAsync(
            b => b.WithTpsCsvExtractId(tpsCsvExtractId)
                .WithItem(person!.Trn!, laCode1, establishmentNumber1, postcode1, new DateOnly(2023, 02, 03), new DateOnly(2024, 03, 30), new DateOnly(2024, 04, 25))
                .WithItem(person!.Trn!, laCode1, establishmentNumber2, postcode2, new DateOnly(2023, 04, 05), new DateOnly(2024, 03, 30), new DateOnly(2024, 04, 25)));

        // Act
        var processor = new TpsCsvExtractProcessor(
            TestData.DbContextFactory,
            TestData.Clock);
        await processor.ProcessNewEmploymentHistoryAsync(tpsCsvExtractId, CancellationToken.None);

        // Assert
        using var dbContext = TestData.DbContextFactory.CreateDbContext();
        var items = await dbContext.TpsCsvExtractItems.Where(i => i.TpsCsvExtractId == tpsCsvExtractId).ToListAsync();
        Assert.All(items, i => Assert.Equal(TpsCsvExtractItemResult.ValidDataAdded, i.Result));
        var employmentHistory = await dbContext.TpsEmployments.Where(e => e.PersonId == person.PersonId).ToListAsync();
        Assert.Equal(2, employmentHistory.Count);
        Assert.Contains(nonHigherEducationEstablishment.EstablishmentId, employmentHistory.Select(pe => pe.EstablishmentId));
        Assert.Contains(higherEductionEstablishment.EstablishmentId, employmentHistory.Select(pe => pe.EstablishmentId));
    }

    [Fact]
    public async Task ProcessUpdatedEmploymentHistory_WhenCalledWithUpdatedEmploymentHistory_UpdatesPersonEmploymentRecord()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var tpsCsvExtractId = Guid.NewGuid();
        var establishment1 = await TestData.CreateEstablishmentAsync(localAuthorityCode: "126", establishmentNumber: "1237");
        var nationalInsuranceNumber = TestData.GenerateNationalInsuranceNumber();
        var personPostcode = Faker.Address.UkPostCode();
        var personEmailAddress = "original@email.com";
        var existingPersonEmployment = await TestData.CreateTpsEmploymentAsync(person, establishment1, new DateOnly(2023, 02, 02), new DateOnly(2024, 02, 29), EmploymentType.FullTime, new DateOnly(2024, 03, 25), nationalInsuranceNumber, personPostcode, personEmailAddress: personEmailAddress);
        var updatedEndDate = new DateOnly(2024, 03, 30);
        var updatedLastExtractDate = new DateOnly(2024, 04, 25);
        var updatedMemberEmailAddress = "updated@email.com";
        await TestData.CreateTpsCsvExtractAsync(b => b.WithTpsCsvExtractId(tpsCsvExtractId).WithItem(person!.Trn!, establishment1.LaCode, establishment1.EstablishmentNumber, establishment1.Postcode!, new DateOnly(2023, 02, 02), updatedEndDate, updatedLastExtractDate, memberEmailAddress: updatedMemberEmailAddress));

        // Act
        var processor = new TpsCsvExtractProcessor(
            TestData.DbContextFactory,
            TestData.Clock);
        await processor.ProcessUpdatedEmploymentHistoryAsync(tpsCsvExtractId, CancellationToken.None);

        // Assert
        using var dbContext = TestData.DbContextFactory.CreateDbContext();
        var items = await dbContext.TpsCsvExtractItems.Where(i => i.TpsCsvExtractId == tpsCsvExtractId).ToListAsync();
        Assert.All(items, i => Assert.Equal(TpsCsvExtractItemResult.ValidDataUpdated, i.Result));
        var updatedPersonEmployment = await dbContext.TpsEmployments.SingleAsync(e => e.TpsEmploymentId == existingPersonEmployment.TpsEmploymentId);
        Assert.Equal(updatedEndDate, updatedPersonEmployment.LastKnownTpsEmployedDate);
        Assert.Equal(updatedLastExtractDate, updatedPersonEmployment.LastExtractDate);
        Assert.Equal(updatedMemberEmailAddress, updatedPersonEmployment.PersonEmailAddress);
        Assert.Null(updatedPersonEmployment.EndDate);
    }

    [Fact]
    public async Task ProcessUpdatedEmploymentHistory_WhenCalledWithEndDateInTheFuture_SetsLastKnownEmployedDateToExtractDate()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var tpsCsvExtractId = Guid.NewGuid();
        var establishment1 = await TestData.CreateEstablishmentAsync(localAuthorityCode: "126", establishmentNumber: "1237");
        var nationalInsuranceNumber = TestData.GenerateNationalInsuranceNumber();
        var personPostcode = Faker.Address.UkPostCode();
        var existingPersonEmployment = await TestData.CreateTpsEmploymentAsync(person, establishment1, new DateOnly(2023, 02, 02), new DateOnly(2024, 02, 29), EmploymentType.FullTime, new DateOnly(2024, 03, 25), nationalInsuranceNumber, personPostcode);
        var updatedEndDate = new DateOnly(2024, 10, 30);
        var updatedLastExtractDate = new DateOnly(2024, 04, 25);
        await TestData.CreateTpsCsvExtractAsync(b => b.WithTpsCsvExtractId(tpsCsvExtractId).WithItem(person!.Trn!, establishment1.LaCode, establishment1.EstablishmentNumber, establishment1.Postcode!, new DateOnly(2023, 02, 02), updatedEndDate, updatedLastExtractDate));

        // Act
        var processor = new TpsCsvExtractProcessor(
            TestData.DbContextFactory,
            TestData.Clock);
        await processor.ProcessUpdatedEmploymentHistoryAsync(tpsCsvExtractId, CancellationToken.None);

        // Assert
        using var dbContext = TestData.DbContextFactory.CreateDbContext();
        var items = await dbContext.TpsCsvExtractItems.Where(i => i.TpsCsvExtractId == tpsCsvExtractId).ToListAsync();
        Assert.All(items, i => Assert.Equal(TpsCsvExtractItemResult.ValidDataUpdated, i.Result));
        var updatedPersonEmployment = await dbContext.TpsEmployments.SingleAsync(e => e.TpsEmploymentId == existingPersonEmployment.TpsEmploymentId);
        Assert.Equal(updatedLastExtractDate, updatedPersonEmployment.LastKnownTpsEmployedDate);
    }

    [Fact]
    public async Task ProcessUpdatedEmploymentHistory_WhenCalledWithWithdrawalIndicatorSet_SetsEndDate()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var tpsCsvExtractId = Guid.NewGuid();
        var establishment1 = await TestData.CreateEstablishmentAsync(localAuthorityCode: "126", establishmentNumber: "1237");
        var nationalInsuranceNumber = TestData.GenerateNationalInsuranceNumber();
        var personPostcode = Faker.Address.UkPostCode();
        var existingPersonEmployment = await TestData.CreateTpsEmploymentAsync(person, establishment1, new DateOnly(2023, 02, 02), new DateOnly(2024, 02, 29), EmploymentType.FullTime, new DateOnly(2024, 03, 25), nationalInsuranceNumber, personPostcode);
        var updatedEndDate = new DateOnly(2024, 03, 30);
        var updatedWithdrawalIndicator = "W";
        var updatedLastExtractDate = new DateOnly(2024, 04, 25);
        await TestData.CreateTpsCsvExtractAsync(b => b.WithTpsCsvExtractId(tpsCsvExtractId).WithItem(person!.Trn!, establishment1.LaCode, establishment1.EstablishmentNumber, establishment1.Postcode!, new DateOnly(2023, 02, 02), updatedEndDate, updatedLastExtractDate, withdrawalIndicator: updatedWithdrawalIndicator));

        // Act
        var processor = new TpsCsvExtractProcessor(
            TestData.DbContextFactory,
            TestData.Clock);
        await processor.ProcessUpdatedEmploymentHistoryAsync(tpsCsvExtractId, CancellationToken.None);

        // Assert
        using var dbContext = TestData.DbContextFactory.CreateDbContext();
        var items = await dbContext.TpsCsvExtractItems.Where(i => i.TpsCsvExtractId == tpsCsvExtractId).ToListAsync();
        Assert.All(items, i => Assert.Equal(TpsCsvExtractItemResult.ValidDataUpdated, i.Result));
        var updatedPersonEmployment = await dbContext.TpsEmployments.SingleAsync(e => e.TpsEmploymentId == existingPersonEmployment.TpsEmploymentId);
        Assert.Equal(updatedEndDate, updatedPersonEmployment.LastKnownTpsEmployedDate);
        Assert.Equal(updatedLastExtractDate, updatedPersonEmployment.LastExtractDate);
        Assert.True(updatedPersonEmployment.WithdrawalConfirmed);
        Assert.Equal(updatedEndDate, updatedPersonEmployment.EndDate);
    }

    [Fact]
    public async Task ProcessUpdatedEmploymentHistory_WhenCalledWithLastKnownEmployedDateOlderThanFiveMonths_SetsEndDate()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var tpsCsvExtractId = Guid.NewGuid();
        var establishment1 = await TestData.CreateEstablishmentAsync(localAuthorityCode: "126", establishmentNumber: "1237");
        var nationalInsuranceNumber = TestData.GenerateNationalInsuranceNumber();
        var personPostcode = Faker.Address.UkPostCode();
        var existingPersonEmployment = await TestData.CreateTpsEmploymentAsync(person, establishment1, new DateOnly(2023, 02, 02), new DateOnly(2023, 10, 29), EmploymentType.FullTime, new DateOnly(2024, 03, 25), nationalInsuranceNumber, personPostcode);
        var updatedLastExtractDate = new DateOnly(2024, 04, 25);
        await TestData.CreateTpsCsvExtractAsync(b => b.WithTpsCsvExtractId(tpsCsvExtractId).WithItem(person!.Trn!, establishment1.LaCode, establishment1.EstablishmentNumber, establishment1.Postcode!, new DateOnly(2023, 02, 02), existingPersonEmployment.LastKnownTpsEmployedDate, updatedLastExtractDate));

        // Act
        var processor = new TpsCsvExtractProcessor(
            TestData.DbContextFactory,
            TestData.Clock);
        await processor.ProcessUpdatedEmploymentHistoryAsync(tpsCsvExtractId, CancellationToken.None);

        // Assert
        using var dbContext = TestData.DbContextFactory.CreateDbContext();
        var items = await dbContext.TpsCsvExtractItems.Where(i => i.TpsCsvExtractId == tpsCsvExtractId).ToListAsync();
        Assert.All(items, i => Assert.Equal(TpsCsvExtractItemResult.ValidDataUpdated, i.Result));
        var updatedPersonEmployment = await dbContext.TpsEmployments.SingleAsync(e => e.TpsEmploymentId == existingPersonEmployment.TpsEmploymentId);
        Assert.Equal(updatedLastExtractDate, updatedPersonEmployment.LastExtractDate);
        Assert.Equal(updatedPersonEmployment.LastKnownTpsEmployedDate, updatedPersonEmployment.EndDate);
    }

    [Fact]
    public async Task ProcessUpdatedEmploymentHistory_WhenCalledWithWithdrawalIndicatorNowRemoved_ResetsEndDate()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var tpsCsvExtractId = Guid.NewGuid();
        var establishment1 = await TestData.CreateEstablishmentAsync(localAuthorityCode: "126", establishmentNumber: "1237");
        var nationalInsuranceNumber = TestData.GenerateNationalInsuranceNumber();
        var personPostcode = Faker.Address.UkPostCode();
        var existingPersonEmployment = await TestData.CreateTpsEmploymentAsync(person, establishment1, new DateOnly(2023, 02, 02), new DateOnly(2024, 02, 29), EmploymentType.FullTime, new DateOnly(2024, 03, 25), nationalInsuranceNumber, personPostcode, withdrawalConfirmed: true);
        var updatedLastExtractDate = new DateOnly(2024, 04, 25);
        await TestData.CreateTpsCsvExtractAsync(b => b.WithTpsCsvExtractId(tpsCsvExtractId).WithItem(person!.Trn!, establishment1.LaCode, establishment1.EstablishmentNumber, establishment1.Postcode!, new DateOnly(2023, 02, 02), existingPersonEmployment.LastKnownTpsEmployedDate, updatedLastExtractDate));

        // Act
        var processor = new TpsCsvExtractProcessor(
            TestData.DbContextFactory,
            TestData.Clock);
        await processor.ProcessUpdatedEmploymentHistoryAsync(tpsCsvExtractId, CancellationToken.None);

        // Assert
        using var dbContext = TestData.DbContextFactory.CreateDbContext();
        var items = await dbContext.TpsCsvExtractItems.Where(i => i.TpsCsvExtractId == tpsCsvExtractId).ToListAsync();
        Assert.All(items, i => Assert.Equal(TpsCsvExtractItemResult.ValidDataUpdated, i.Result));
        var updatedPersonEmployment = await dbContext.TpsEmployments.SingleAsync(e => e.TpsEmploymentId == existingPersonEmployment.TpsEmploymentId);
        Assert.Equal(updatedLastExtractDate, updatedPersonEmployment.LastExtractDate);
        Assert.Null(updatedPersonEmployment.EndDate);
        Assert.False(updatedPersonEmployment.WithdrawalConfirmed);
    }

    [Fact]
    public async Task ProcessUpdatedEmploymentHistory_WhenCalledWithUpdatedEmploymentHistoryWithNoChanges_SetsResultToValidNoChanges()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var tpsCsvExtractId = Guid.NewGuid();
        var establishment1 = await TestData.CreateEstablishmentAsync(localAuthorityCode: "126", establishmentNumber: "1237");
        var nationalInsuranceNumber = TestData.GenerateNationalInsuranceNumber();
        var personPostcode = Faker.Address.UkPostCode();
        var existingPersonEmployment = await TestData.CreateTpsEmploymentAsync(person, establishment1, new DateOnly(2023, 02, 02), new DateOnly(2024, 02, 29), EmploymentType.FullTime, new DateOnly(2024, 03, 25), nationalInsuranceNumber, personPostcode);
        await TestData.CreateTpsCsvExtractAsync(b => b.WithTpsCsvExtractId(tpsCsvExtractId).WithItem(person!.Trn!, establishment1.LaCode, establishment1.EstablishmentNumber, establishment1.Postcode!, existingPersonEmployment.StartDate, existingPersonEmployment.LastKnownTpsEmployedDate, existingPersonEmployment.LastExtractDate, "FT"));

        // Act
        var processor = new TpsCsvExtractProcessor(
            TestData.DbContextFactory,
            TestData.Clock);
        await processor.ProcessUpdatedEmploymentHistoryAsync(tpsCsvExtractId, CancellationToken.None);

        // Assert
        using var dbContext = TestData.DbContextFactory.CreateDbContext();
        var items = await dbContext.TpsCsvExtractItems.Where(i => i.TpsCsvExtractId == tpsCsvExtractId).ToListAsync();
        Assert.All(items, i => Assert.Equal(TpsCsvExtractItemResult.ValidNoChange, i.Result));
    }

    [Fact]
    public async Task UpdateLatestEstablishmentVersions_WithEstablishmentChangingUrn_UpdatesPersonEmploymentRecord()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var establishment1 = await TestData.CreateEstablishmentAsync(localAuthorityCode: "127", establishmentNumber: "1238", establishmentStatusCode: 2); // Closed
        var establishment2 = await TestData.CreateEstablishmentAsync(localAuthorityCode: "127", establishmentNumber: "1238", establishmentStatusCode: 1); // Open
        var nationalInsuranceNumber = TestData.GenerateNationalInsuranceNumber();
        var personPostcode = Faker.Address.UkPostCode();
        var existingPersonEmployment = await TestData.CreateTpsEmploymentAsync(person, establishment1, new DateOnly(2023, 02, 02), new DateOnly(2024, 02, 29), EmploymentType.FullTime, new DateOnly(2024, 03, 25), nationalInsuranceNumber, personPostcode);

        // Act
        var processor = new TpsCsvExtractProcessor(
            TestData.DbContextFactory,
            TestData.Clock);
        await processor.UpdateLatestEstablishmentVersionsAsync(CancellationToken.None);

        // Assert
        using var dbContext = TestData.DbContextFactory.CreateDbContext();
        var updatedPersonEmployment = await dbContext.TpsEmployments.SingleAsync(e => e.TpsEmploymentId == existingPersonEmployment.TpsEmploymentId);
        Assert.Equal(establishment2.EstablishmentId, updatedPersonEmployment.EstablishmentId);
    }

    [Fact]
    public async Task ProcessEndedEmployments_WithLastKnownEmployedDateGreaterThanThreeMonthsBeforeLastExtractDate_SetsEndDateOnPersonEmploymentRecord()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var establishment1 = await TestData.CreateEstablishmentAsync(localAuthorityCode: "128", establishmentNumber: "1239");
        var establishment2 = await TestData.CreateEstablishmentAsync(localAuthorityCode: "128", establishmentNumber: "1240");
        var extractDate = new DateOnly(2024, 04, 25);
        var lastKnownEmployedDateWithinThreeMonthsOfExtractDate = new DateOnly(2024, 02, 29);
        var lastKnownEmployedDateOutsideThreeMonthsOfExtractDate = new DateOnly(2023, 09, 30);
        var nationalInsuranceNumber = TestData.GenerateNationalInsuranceNumber();
        var personPostcode = Faker.Address.UkPostCode();
        var personEmploymentWhichHasEnded = await TestData.CreateTpsEmploymentAsync(person, establishment1, new DateOnly(2023, 02, 02), lastKnownEmployedDateOutsideThreeMonthsOfExtractDate, EmploymentType.FullTime, extractDate, nationalInsuranceNumber, personPostcode);
        var personEmploymentWhichHasNotEnded = await TestData.CreateTpsEmploymentAsync(person, establishment2, new DateOnly(2023, 02, 02), lastKnownEmployedDateWithinThreeMonthsOfExtractDate, EmploymentType.FullTime, extractDate, nationalInsuranceNumber, personPostcode);

        // Act
        var processor = new TpsCsvExtractProcessor(
            TestData.DbContextFactory,
            TestData.Clock);
        await processor.ProcessEndedEmploymentsAsync(CancellationToken.None);

        // Assert
        using var dbContext = TestData.DbContextFactory.CreateDbContext();
        var updatedPersonEmploymentWhichShouldHaveEndDateSet = await dbContext.TpsEmployments.SingleAsync(e => e.TpsEmploymentId == personEmploymentWhichHasEnded.TpsEmploymentId);
        var updatedPersonEmploymentWhichShouldNotHaveEndDateSet = await dbContext.TpsEmployments.SingleAsync(e => e.TpsEmploymentId == personEmploymentWhichHasNotEnded.TpsEmploymentId);
        Assert.Equal(lastKnownEmployedDateOutsideThreeMonthsOfExtractDate, updatedPersonEmploymentWhichShouldHaveEndDateSet.EndDate);
        Assert.Null(updatedPersonEmploymentWhichShouldNotHaveEndDateSet.EndDate);
    }

    [Fact]
    public async Task BackfillEmployerEmailAddressInEmploymentHistory_WhenCalledWithTpsEmploymentRecordsWithoutEmployerEmailAddress_SetsEmployerEmailAddress()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var tpsCsvExtractId = Guid.NewGuid();
        var establishment = await TestData.CreateEstablishmentAsync(localAuthorityCode: "129", establishmentNumber: "1241");
        var nationalInsuranceNumber = TestData.GenerateNationalInsuranceNumber();
        var memberPostcode = Faker.Address.UkPostCode();
        var employerEmailAddress = Faker.Internet.Email();
        var personEmploymentWithoutEmployerEmailAddress = await TestData.CreateTpsEmploymentAsync(person, establishment, new DateOnly(2023, 02, 02), new DateOnly(2024, 02, 29), EmploymentType.FullTime, new DateOnly(2024, 03, 25), nationalInsuranceNumber, memberPostcode);
        await TestData.CreateTpsCsvExtractAsync(b => b.WithTpsCsvExtractId(tpsCsvExtractId).WithItem(person!.Trn!, establishment.LaCode, establishment.EstablishmentNumber, establishment.Postcode!, personEmploymentWithoutEmployerEmailAddress.StartDate, personEmploymentWithoutEmployerEmailAddress.LastKnownTpsEmployedDate, personEmploymentWithoutEmployerEmailAddress.LastExtractDate, "FT", personEmploymentWithoutEmployerEmailAddress.NationalInsuranceNumber, memberPostcode: personEmploymentWithoutEmployerEmailAddress.PersonPostcode, employerEmailAddress: employerEmailAddress));

        // Act
        var processor = new TpsCsvExtractProcessor(
            TestData.DbContextFactory,
            TestData.Clock);
        await processor.BackfillEmployerEmailAddressInEmploymentHistoryAsync(CancellationToken.None);

        // Assert
        using var dbContext = TestData.DbContextFactory.CreateDbContext();
        var updatedTpsEmployment = await dbContext.TpsEmployments.SingleAsync(e => e.TpsEmploymentId == personEmploymentWithoutEmployerEmailAddress.TpsEmploymentId);
        Assert.Equal(employerEmailAddress, updatedTpsEmployment.EmployerEmailAddress);
    }

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public async ValueTask DisposeAsync() => await fixture.DbHelper.ClearDataAsync();
}
