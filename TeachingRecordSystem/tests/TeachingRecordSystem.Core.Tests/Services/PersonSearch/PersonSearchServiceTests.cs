using Microsoft.PowerPlatform.Dataverse.Client;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt;
using TeachingRecordSystem.Core.Services.PersonSearch;
using TeachingRecordSystem.Core.Services.TrsDataSync;

namespace TeachingRecordSystem.Core.Tests.Services.PersonSearch;

public class PersonSearchServiceTests
{
    public PersonSearchServiceTests(
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

    [Theory]
    [InlineData(true, false, false, false)]
    [InlineData(false, true, false, false)]
    [InlineData(false, false, true, false)]
    [InlineData(false, false, false, true)]
    [InlineData(true, true, false, false)]
    [InlineData(true, false, true, false)]
    [InlineData(true, false, false, true)]
    [InlineData(false, true, true, false)]
    [InlineData(false, true, false, true)]
    [InlineData(false, false, true, true)]
    [InlineData(true, true, true, false)]
    [InlineData(true, true, false, true)]
    [InlineData(true, false, true, true)]
    [InlineData(false, true, true, true)]
    public Task Search_WithMissingParameters_ReturnsEmptyArray(bool hasNames, bool hasDatesOfBirth, bool hasNino, bool hasTrn) =>
        DbFixture.WithDbContext(async dbContext =>
        {
            // Arrange
            var names = hasNames ? [["John", "Doe"]] : Array.Empty<string[]>();
            var datesOfBirth = hasDatesOfBirth ? new[] { new DateOnly(1980, 1, 1) } : Array.Empty<DateOnly>();
            var nino = hasNino ? Faker.Identification.UkNationalInsuranceNumber() : null;
            var trn = hasTrn ? await TestData.GenerateTrn() : null;

            // Act
            var personSearchService = new PersonSearchService(dbContext);
            var results = await personSearchService.Search(names, datesOfBirth, nino, trn);

            // Assert
            Assert.Empty(results);
        });

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public Task Search_WithMatch_ReturnsResults(bool matchOnSynonym, bool matchOnNino) =>
        DbFixture.WithDbContext(async dbContext =>
        {
            // Arrange
            var firstName = "John";
            var nickName = "Johnny";
            dbContext.NameSynonyms.Add(new NameSynonyms
            {
                Name = firstName,
                Synonyms = new[] { nickName },
            });
            await dbContext.SaveChangesAsync();

            var person = await TestData.CreatePerson(b => b.WithFirstName(firstName).WithNationalInsuranceNumber());
            var name = new[] { new[] { matchOnSynonym ? nickName : firstName, person.LastName } };
            var dateOfBirth = new[] { person.DateOfBirth };
            var nino = matchOnNino ? person.NationalInsuranceNumber : null;
            var trn = matchOnNino ? null : person.Trn;

            // Act
            var personSearchService = new PersonSearchService(dbContext);
            var results = await personSearchService.Search(name, dateOfBirth, nino, trn);

            // Assert
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.PersonId == person.PersonId);
            await dbContext.NameSynonyms.Where(ns => ns.Name == firstName).ExecuteDeleteAsync();
        });

    [Fact]
    public Task Search_WithMatchForMultipleNames_ReturnsMultipleResults() =>
        DbFixture.WithDbContext(async dbContext =>
        {
            // Arrange
            var nationalInsuranceNumber = TestData.GenerateNationalInsuranceNumber();
            var person1 = await TestData.CreatePerson(b => b.WithFirstName("John").WithLastName("Doe").WithNationalInsuranceNumber());
            var person2 = await TestData.CreatePerson(b => b.WithFirstName("Jane").WithLastName("Doe"));
            var name = new[] { new[] { "John", "Doe" }, new[] { "Jane", "Doe" } };
            var dateOfBirth = new[] { person1.DateOfBirth, person2.DateOfBirth };

            // Act
            var personSearchService = new PersonSearchService(dbContext);
            var results = await personSearchService.Search(name, dateOfBirth, person1.NationalInsuranceNumber, person2.Trn);

            // Assert
            Assert.NotEmpty(results);
            Assert.Contains(results, r => r.PersonId == person1.PersonId);
            Assert.Contains(results, r => r.PersonId == person2.PersonId);
        });

    private DbFixture DbFixture { get; }

    private TestData TestData { get; }

    private TestableClock Clock { get; }

    public TrsDataSyncHelper Helper { get; }
}
