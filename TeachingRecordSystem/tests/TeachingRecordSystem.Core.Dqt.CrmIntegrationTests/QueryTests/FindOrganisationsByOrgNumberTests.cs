namespace TeachingRecordSystem.Core.Dqt.CrmIntegrationTests.QueryTests;

public class FindOrganisationsByOrgNumberTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly CrmQueryDispatcher _crmQueryDispatcher;

    public FindOrganisationsByOrgNumberTests(CrmClientFixture crmClientFixture)
    {
        _dataScope = crmClientFixture.CreateTestDataScope();
        _crmQueryDispatcher = _dataScope.CreateQueryDispatcher();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _dataScope.DisposeAsync();

    [Fact]
    public async Task FindByOrgNumber_ReturnsSingleMatch()
    {
        // Arrange
        var accountNumber = "1234";
        var accountName = "testing";
        var account1 = await _dataScope.TestData.CreateAccountAsync(x =>
        {
            x.WithName(accountName);
            x.WithAccountNumber(accountNumber);
        });
        var query = new FindActiveOrganisationsByAccountNumberQuery(accountNumber);

        // Act
        var results = await _crmQueryDispatcher.ExecuteQueryAsync(query);

        // Assert
        Assert.Collection(
            results,
            account =>
            {
                Assert.Equal(account.Id, account1.Id);
                Assert.Equal(account.Name, accountName);
                Assert.Equal(account.AccountNumber, accountNumber);
            });
    }

    [Fact]
    public async Task FindOrganisationsByAccountNumber_ReturnsMultipleMatches()
    {
        // Arrange
        var accountNumber = "95556";
        var accountName1 = "testing";
        var account1 = await _dataScope.TestData.CreateAccountAsync(x =>
        {
            x.WithName(accountName1);
            x.WithAccountNumber(accountNumber);
        });
        var accountName2 = "secondAccount";
        var account2 = await _dataScope.TestData.CreateAccountAsync(x =>
        {
            x.WithName(accountName2);
            x.WithAccountNumber(accountNumber);
        });
        var query = new FindActiveOrganisationsByAccountNumberQuery(accountNumber);

        // Act
        var results = await _crmQueryDispatcher.ExecuteQueryAsync(query);

        // Assert
        Assert.Contains(results, account =>
            account.Id == account1.Id &&
            account.Name == accountName1 &&
            account.AccountNumber == accountNumber
        );

        Assert.Contains(results, account =>
            account.Id == account2.Id &&
            account.Name == accountName2 &&
            account.AccountNumber == accountNumber
        );
    }
}
