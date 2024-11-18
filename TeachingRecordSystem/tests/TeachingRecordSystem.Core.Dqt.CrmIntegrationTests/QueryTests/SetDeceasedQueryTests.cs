using Microsoft.Xrm.Sdk.Query;

namespace TeachingRecordSystem.Core.Dqt.CrmIntegrationTests.QueryTests;

public class SetDeceasedQueryTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly CrmQueryDispatcher _crmQueryDispatcher;

    public SetDeceasedQueryTests(CrmClientFixture crmClientFixture)
    {
        _dataScope = crmClientFixture.CreateTestDataScope();
        _crmQueryDispatcher = crmClientFixture.CreateQueryDispatcher();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _dataScope.DisposeAsync();

    [Fact]
    public async Task QueryExecutesSuccessfully()
    {
        // Arrange
        var dateOfDeath = new DateOnly(2021, 01, 01);
        var createPersonResult = await _dataScope.TestData.CreatePersonAsync();

        // Act
        await _crmQueryDispatcher.ExecuteQueryAsync(new SetDeceasedQuery(ContactId: createPersonResult.ContactId, DateOfDeath: dateOfDeath));
        var contact = await _crmQueryDispatcher.ExecuteQueryAsync(new GetActiveContactDetailByIdQuery(ContactId: createPersonResult.ContactId, ColumnSet: new ColumnSet(
            Contact.Fields.dfeta_DateofDeath, Contact.Fields.StateCode)));

        // Assert
        Assert.Equal(dateOfDeath, contact!.Contact.dfeta_DateofDeath.ToDateOnlyWithDqtBstFix(isLocalTime: false));
    }
}
