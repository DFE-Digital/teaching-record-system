using Microsoft.Xrm.Sdk.Query;

namespace TeachingRecordSystem.Core.Dqt.CrmIntegrationTests.QueryTests;
public class SetQtlsDateTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly CrmQueryDispatcher _crmQueryDispatcher;

    public SetQtlsDateTests(CrmClientFixture crmClientFixture)
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
        var qtlsDate = new DateOnly(2021, 01, 01);
        var createPersonResult = await _dataScope.TestData.CreatePerson();

        // Act
        await _crmQueryDispatcher.ExecuteQuery(new SetQtlsDateQuery(contactId: createPersonResult.ContactId, qtlsDate: qtlsDate));
        var contact = await _crmQueryDispatcher.ExecuteQuery(new GetActiveContactDetailByIdQuery(ContactId: createPersonResult.ContactId, ColumnSet: new ColumnSet(
            Contact.Fields.dfeta_qtlsdate)));

        // Assert
        Assert.Equal(qtlsDate, contact!.Contact.dfeta_qtlsdate.ToDateOnlyWithDqtBstFix(isLocalTime: false));

    }
}
