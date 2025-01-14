namespace TeachingRecordSystem.Core.Dqt.CrmIntegrationTests.QueryTests;

public class FindOrganisationsByLaSchoolCodeTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly CrmQueryDispatcher _crmQueryDispatcher;

    public FindOrganisationsByLaSchoolCodeTests(CrmClientFixture crmClientFixture)
    {
        _dataScope = crmClientFixture.CreateTestDataScope();
        _crmQueryDispatcher = _dataScope.CreateQueryDispatcher();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _dataScope.DisposeAsync();

    [Fact]
    public async Task FindByLaSchoolCode_ReturnsSingleMatch()
    {
        // Arrange
        var accountNumber = "1234";
        var laschoolcode = "999";
        var accountName = "testing";
        var account1 = await _dataScope.TestData.CreateAccountAsync(x =>
        {
            x.WithName(accountName);
            x.WithAccountNumber(accountNumber);
            x.WithLaSchoolCode(laschoolcode);
        });
        var query = new FindActiveOrganisationsByLaSchoolCodeQuery(laschoolcode);

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
                Assert.Equal(account.dfeta_LASchoolCode, laschoolcode);
            });
    }

    [Fact]
    public async Task FindOrganisationsByLaSchoolCode_ReturnsMultipleMatches()
    {
        // Arrange
        var laschoolcode = "999";
        var accountNumber1 = "95556";
        var accountName1 = "testing";
        var account1 = await _dataScope.TestData.CreateAccountAsync(x =>
        {
            x.WithName(accountName1);
            x.WithAccountNumber(accountNumber1);
            x.WithLaSchoolCode(laschoolcode);

        });
        var accountName2 = "secondAccount";
        var accountNumber2 = "7777";
        var account2 = await _dataScope.TestData.CreateAccountAsync(x =>
        {
            x.WithName(accountName2);
            x.WithAccountNumber(accountNumber2);
            x.WithLaSchoolCode(laschoolcode);
        });
        var query = new FindActiveOrganisationsByLaSchoolCodeQuery(laschoolcode);

        // Act
        var results = await _crmQueryDispatcher.ExecuteQueryAsync(query);

        // Assert
        Assert.Contains(results, account =>
            account.Id == account1.Id &&
            account.Name == accountName1 &&
            account.AccountNumber == accountNumber1 &&
            account.dfeta_LASchoolCode == laschoolcode
        );

        Assert.Contains(results, account =>
            account.Id == account2.Id &&
            account.Name == accountName2 &&
            account.AccountNumber == accountNumber2 &&
            account.dfeta_LASchoolCode == laschoolcode
        );
    }
}
