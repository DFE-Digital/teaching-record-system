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
        var account1 = await _dataScope.TestData.CreateAccount(x =>
        {
            x.WithName(accountName);
            x.WithAccountNumber(accountNumber);
        });
        var query = new FindOrganisationsByOrgNumberQuery()
        {
            OrganisationNumber = accountNumber
        };

        // Act
        var results = await _crmQueryDispatcher.ExecuteQuery(query);

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
    public async Task FindByOrgNumber_ReturnsMultipleMatches()
    {
        // Arrange
        var accountNumber = "1234";
        var accountName1 = "testing";
        var account1 = await _dataScope.TestData.CreateAccount(x =>
        {
            x.WithName(accountName1);
            x.WithAccountNumber(accountNumber);
        });
        var accountName2 = "secondAccount";
        var account2 = await _dataScope.TestData.CreateAccount(x =>
        {
            x.WithName(accountName2);
            x.WithAccountNumber(accountNumber);
        });
        var query = new FindOrganisationsByOrgNumberQuery()
        {
            OrganisationNumber = accountNumber
        };

        // Act
        var results = await _crmQueryDispatcher.ExecuteQuery(query);

        // Assert
        Assert.Collection(
            results,
            account1 =>
            {
                Assert.Equal(account1.Id, account1.Id);
                Assert.Equal(account1.Name, accountName1);
                Assert.Equal(account1.AccountNumber, accountNumber);
            },
            account2 =>
            {
                Assert.Equal(account2.Id, account2.Id);
                Assert.Equal(account2.Name, accountName2);
                Assert.Equal(account2.AccountNumber, accountNumber);
            });
    }
}
