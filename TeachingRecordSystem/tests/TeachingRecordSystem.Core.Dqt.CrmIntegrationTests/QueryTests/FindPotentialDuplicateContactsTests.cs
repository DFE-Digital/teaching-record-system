namespace TeachingRecordSystem.Core.Dqt.CrmIntegrationTests.QueryTests;

public class FindPotentialDuplicateContactsTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly CrmQueryDispatcher _crmQueryDispatcher;

    public FindPotentialDuplicateContactsTests(CrmClientFixture crmClientFixture)
    {
        _dataScope = crmClientFixture.CreateTestDataScope();
        _crmQueryDispatcher = _dataScope.CreateQueryDispatcher();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _dataScope.DisposeAsync();

    [Fact]
    public async Task QueryExecutesSuccessfully()
    {
        // Arrange
        var email = $"{Guid.NewGuid()}@test.com";
        var firstName = Faker.Name.First();
        var middleName = Faker.Name.Middle();
        var lastName = Faker.Name.Last();
        var dob = new DateTime(1987, 01, 01);

        var person = await _dataScope.TestData.CreatePerson(b =>
        {
            b.WithFirstName(email);
            b.WithLastName(lastName);
            b.WithMiddleName(middleName);
            b.WithDateOfBirth(dob.ToDateOnlyWithDqtBstFix(isLocalTime: false));
        });

        var query = new FindPotentialDuplicateContactsQuery()
        {
            FirstName = firstName,
            MiddleName = middleName,
            LastName = lastName,
            DateOfBirth = dob.ToDateOnlyWithDqtBstFix(isLocalTime: false)
        };

        // Act
        var result = await _crmQueryDispatcher.ExecuteQuery(query);

        // Assert
        Assert.NotNull(result);
        Assert.Contains(person.PersonId, result!.Select(x => x.TeacherId));
    }
}
