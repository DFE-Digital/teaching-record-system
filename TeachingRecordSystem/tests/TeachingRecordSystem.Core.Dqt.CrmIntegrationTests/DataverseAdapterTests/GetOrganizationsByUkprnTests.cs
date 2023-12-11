#nullable disable

namespace TeachingRecordSystem.Core.Dqt.CrmIntegrationTests.DataverseAdapterTests;

public class GetOrganizationsByUkprnTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly DataverseAdapter _dataverseAdapter;

    public GetOrganizationsByUkprnTests(CrmClientFixture crmClientFixture)
    {
        _dataScope = crmClientFixture.CreateTestDataScope();
        _dataverseAdapter = _dataScope.CreateDataverseAdapter();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _dataScope.DisposeAsync();

    [Fact]
    public async Task Given_valid_ukprn_returns_account()
    {
        // Arrange
        var ukprn = "10044534";

        // Act
        var result = await _dataverseAdapter.GetOrganizationsByUkprn(ukprn, columnNames: new[] { Account.Fields.dfeta_UKPRN });

        // Assert
        Assert.Collection(
            result,
            record => Assert.Equal(ukprn, record.dfeta_UKPRN));
    }

    [Fact]
    public async Task Given_invalid_ukprn_returns_null()
    {
        // Arrange
        var ukprn = "xxx";

        // Act
        var result = await _dataverseAdapter.GetOrganizationsByUkprn(ukprn, columnNames: new[] { Account.Fields.dfeta_UKPRN });

        // Assert
        Assert.Empty(result);
    }
}
