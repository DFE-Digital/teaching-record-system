using TeachingRecordSystem.Core.Services.PersonMatching;

namespace TeachingRecordSystem.Core.Tests.Services.PersonMatching;

[Collection(nameof(DisableParallelization))]
public partial class PersonMatchingServiceTests : IAsyncLifetime
{
    public PersonMatchingServiceTests(
        DbFixture dbFixture,
        ReferenceDataCache referenceDataCache)
    {
        DbFixture = dbFixture;
        Clock = new();

        TestData = new TestData(
            dbFixture.DbContextFactory,
            referenceDataCache,
            Clock);
    }

    private DbFixture DbFixture { get; }

    private TestData TestData { get; }

    private TestableClock Clock { get; }

    public Task InitializeAsync() => DbFixture.DbHelper.ClearDataAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public Task GetMatchedAttributesAsync_ReturnsExpectedResults(bool usePersonNino) =>
        DbFixture.WithDbContextAsync(async dbContext =>
        {
            // Arrange
            var firstName = TestData.GenerateFirstName();
            var lastName = TestData.GenerateLastName();
            var dateOfBirth = TestData.GenerateDateOfBirth();
            var nationalInsuranceNumber = TestData.GenerateNationalInsuranceNumber();
            var alternativeNationalInsuranceNumber = TestData.GenerateChangedNationalInsuranceNumber(nationalInsuranceNumber);

            var person = await TestData.CreatePersonAsync(p => p.WithFirstName(firstName).WithLastName(lastName).WithDateOfBirth(dateOfBirth).WithNationalInsuranceNumber(usePersonNino ? nationalInsuranceNumber : alternativeNationalInsuranceNumber));
            var establishment = await TestData.CreateEstablishmentAsync(localAuthorityCode: "321", establishmentNumber: "4321", establishmentStatusCode: 1);
            var personEmployment = await TestData.CreateTpsEmploymentAsync(person, establishment, new DateOnly(2023, 08, 03), new DateOnly(2024, 05, 25), EmploymentType.FullTime, new DateOnly(2024, 05, 25), usePersonNino ? alternativeNationalInsuranceNumber : nationalInsuranceNumber);

            string[][] names = [[firstName, lastName]];
            DateOnly[] datesOfBirth = [dateOfBirth];

            var service = new PersonMatchingService(dbContext);

            // Act
            var result = await service.GetMatchedAttributesAsync(new(names, datesOfBirth, nationalInsuranceNumber, person.Trn!, TrnTokenTrnHint: null), person.PersonId);

            // Assert
            Assert.Collection(
                result,
                m => AssertAttributeMatch(PersonMatchedAttribute.LastName, lastName, m),
                m => AssertAttributeMatch(PersonMatchedAttribute.DateOfBirth, dateOfBirth.ToString("yyyy-MM-dd"), m),
                m => AssertAttributeMatch(PersonMatchedAttribute.NationalInsuranceNumber, nationalInsuranceNumber, m),
                m => AssertAttributeMatch(PersonMatchedAttribute.Trn, person.Trn!, m),
                m => AssertAttributeMatch(PersonMatchedAttribute.FirstName, firstName, m));

            static void AssertAttributeMatch(PersonMatchedAttribute expectedAttribute, string expectedValue, KeyValuePair<PersonMatchedAttribute, string> actual)
            {
                Assert.Equal(expectedAttribute, actual.Key);
                Assert.Equal(expectedValue, actual.Value);
            }
        });
}
