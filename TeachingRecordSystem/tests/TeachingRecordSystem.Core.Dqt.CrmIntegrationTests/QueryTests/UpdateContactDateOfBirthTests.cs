namespace TeachingRecordSystem.Core.Dqt.CrmIntegrationTests.QueryTests;

public class UpdateContactDateOfBirthTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly CrmQueryDispatcher _crmQueryDispatcher;

    public UpdateContactDateOfBirthTests(CrmClientFixture crmClientFixture)
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
        var person = await _dataScope.TestData.CreatePersonAsync();
        var newDateOfBirth = _dataScope.TestData.GenerateChangedDateOfBirth(person.DateOfBirth);

        var query = new UpdateContactDateOfBirthQuery(
            person.ContactId,
            newDateOfBirth);

        // Act
        await _crmQueryDispatcher.ExecuteQueryAsync(query);

        // Assert
        using var ctx = new DqtCrmServiceContext(_dataScope.OrganizationService);

        var updatedContact = ctx.ContactSet.SingleOrDefault(c => c.GetAttributeValue<Guid>(Contact.PrimaryIdAttribute) == person.ContactId);
        Assert.NotNull(updatedContact);
        Assert.Equal(newDateOfBirth, updatedContact.BirthDate.ToDateOnlyWithDqtBstFix(isLocalTime: false));
    }
}
