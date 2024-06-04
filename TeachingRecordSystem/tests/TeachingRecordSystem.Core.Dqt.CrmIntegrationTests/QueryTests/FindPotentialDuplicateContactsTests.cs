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
        var firstName = "Rob";
        var firstNameSynonym = "Robert";
        var middleName = Faker.Name.Middle();
        var lastName = Faker.Name.Last();
        var dob = new DateTime(1987, 01, 01);

        var person = await _dataScope.TestData.CreatePerson(b => b
            .WithFirstName(firstNameSynonym)
            .WithLastName(lastName)
            .WithMiddleName(middleName)
            .WithDateOfBirth(dob.ToDateOnlyWithDqtBstFix(isLocalTime: false))
            .WithEmail(email));

        var query = new FindPotentialDuplicateContactsQuery()
        {
            FirstNames = [firstName, firstNameSynonym],
            MiddleName = middleName,
            LastName = lastName,
            DateOfBirth = dob.ToDateOnlyWithDqtBstFix(isLocalTime: false),
            EmailAddresses = [email]
        };

        // Act
        var result = await _crmQueryDispatcher.ExecuteQuery(query);

        // Assert
        var queryResult = Assert.Single(result, r => r.ContactId == person.PersonId);

        Assert.Collection(
            queryResult.MatchedAttributes,
            a => Assert.Equal(Contact.Fields.FirstName, a),
            a => Assert.Equal(Contact.Fields.MiddleName, a),
            a => Assert.Equal(Contact.Fields.LastName, a),
            a => Assert.Equal(Contact.Fields.BirthDate, a),
            a => Assert.Equal(Contact.Fields.EMailAddress1, a));
    }
}
