#nullable disable
using TeachingRecordSystem.Api.DataStore.Crm;
using Xunit;

namespace TeachingRecordSystem.Api.Tests.DataverseIntegration;

public class GetTeacherStatusTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly DataverseAdapter _dataverseAdapter;

    public GetTeacherStatusTests(CrmClientFixture crmClientFixture)
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
        var value = "211";

        // Act
        var result = await _dataverseAdapter.GetTeacherStatus(value, null);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(value, result.dfeta_Value);
    }

    [Fact]
    public async Task Given_invalid_value_returns_null()
    {
        // Arrange
        var value = "XXXX";

        // Act
        var result = await _dataverseAdapter.GetTeacherStatus(value, null);

        // Assert
        Assert.Null(result);
    }
}
