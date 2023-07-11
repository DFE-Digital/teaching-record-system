#nullable disable

namespace TeachingRecordSystem.Dqt.Tests.DataverseAdapterTests;

public class GetIttProvidersTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly DataverseAdapter _dataverseAdapter;

    public GetIttProvidersTests(CrmClientFixture crmClientFixture)
    {
        _dataScope = crmClientFixture.CreateTestDataScope();
        _dataverseAdapter = _dataScope.CreateDataverseAdapter();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _dataScope.DisposeAsync();

    [Fact]
    public async Task Returns_providers()
    {
        // Arrange

        // Act
        var result = await _dataverseAdapter.GetIttProviders(false);

        // Assert
        Assert.NotEmpty(result);
    }
}
