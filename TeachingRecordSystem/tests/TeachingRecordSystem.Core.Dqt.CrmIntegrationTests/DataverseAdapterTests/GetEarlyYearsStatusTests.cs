#nullable disable

namespace TeachingRecordSystem.Core.Dqt.CrmIntegrationTests.DataverseAdapterTests;

public class GetEarlyYearsStatusTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly DataverseAdapter _dataverseAdapter;

    public GetEarlyYearsStatusTests(CrmClientFixture crmClientFixture)
    {
        _dataScope = crmClientFixture.CreateTestDataScope();
        _dataverseAdapter = _dataScope.CreateDataverseAdapter();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _dataScope.DisposeAsync();

    [Fact]
    public async Task Given_valid_value_returns_entity()
    {
        // Arrange
        var value = "220";

        // Act
        var result = await _dataverseAdapter.GetEarlyYearsStatus(value, null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(value, result.dfeta_Value);
    }

    [Fact]
    public async Task Given_invalid_value_returns_null()
    {
        // Arrange
        var countryCode = "XXXX";

        // Act
        var result = await _dataverseAdapter.GetEarlyYearsStatus(countryCode, null);

        // Assert
        Assert.Null(result);
    }
}
