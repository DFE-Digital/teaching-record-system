namespace TeachingRecordSystem.Core.Dqt.Tests.QueryTests;

public class UpdateContactNameTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly CrmQueryDispatcher _crmQueryDispatcher;

    public UpdateContactNameTests(CrmClientFixture crmClientFixture)
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
        var person = await _dataScope.TestData.CreatePerson();

        var newFirstName = _dataScope.TestData.GenerateChangedFirstName(person.FirstName);
        var newMiddleName = _dataScope.TestData.GenerateChangedMiddleName(person.MiddleName);
        var newLastName = _dataScope.TestData.GenerateChangedLastName(person.LastName);

        var query = new UpdateContactNameQuery(
            person.ContactId,
            newFirstName,
            newMiddleName,
            newLastName);

        // Act
        await _crmQueryDispatcher.ExecuteQuery(query);

        // Assert
        using var ctx = new DqtCrmServiceContext(_dataScope.OrganizationService);

        var updatedContact = ctx.ContactSet.SingleOrDefault(c => c.GetAttributeValue<Guid>(Contact.PrimaryIdAttribute) == person.ContactId);
        Assert.NotNull(updatedContact);
        Assert.Equal(newFirstName, updatedContact.FirstName);
        Assert.Equal(newMiddleName, updatedContact.MiddleName);
        Assert.Equal(newLastName, updatedContact.LastName);
    }
}
