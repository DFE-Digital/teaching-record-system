using Microsoft.Xrm.Sdk.Query;

namespace TeachingRecordSystem.Core.Dqt.Tests.QueryTests;

public class GetContactsByNameTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly CrmQueryDispatcher _crmQueryDispatcher;

    public GetContactsByNameTests(CrmClientFixture crmClientFixture)
    {
        _dataScope = crmClientFixture.CreateTestDataScope();
        _crmQueryDispatcher = crmClientFixture.CreateQueryDispatcher();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _dataScope.DisposeAsync();

    [Fact]
    public async Task ReturnsMatchingContactsFromCrm()
    {
        // Arrange
        var name = "smith";
        var maxRecordCount = 4; // pretty safe bet there will always be at least 4 smiths in the dev CRM database

        // Act
        var results = await _crmQueryDispatcher.ExecuteQuery(new GetContactsByNameQuery(name, maxRecordCount, new ColumnSet()));

        // Assert
        Assert.NotNull(results);
        Assert.Equal(maxRecordCount, results.Length);
    }
}
