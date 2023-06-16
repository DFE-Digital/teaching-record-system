#nullable disable
using TeachingRecordSystem.Api.DataStore.Crm;
using Xunit;

namespace TeachingRecordSystem.Api.Tests.DataverseIntegration;

public class GetCountryTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly DataverseAdapter _dataverseAdapter;

    public GetCountryTests(CrmClientFixture crmClientFixture)
    {
        _dataScope = crmClientFixture.CreateTestDataScope();
        _dataverseAdapter = _dataScope.CreateDataverseAdapter();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _dataScope.DisposeAsync();

    [Fact]
    public async Task Given_valid_country_code_returns_country()
    {
        // Arrange
        var countryCode = "XK";

        // Act
        var result = await _dataverseAdapter.GetCountry(countryCode);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(countryCode, result.dfeta_Value);
    }

    [Fact]
    public async Task Given_invalid_country_code_returns_null()
    {
        // Arrange
        var countryCode = "XXXX";

        // Act
        var result = await _dataverseAdapter.GetCountry(countryCode);

        // Assert
        Assert.Null(result);
    }
}
