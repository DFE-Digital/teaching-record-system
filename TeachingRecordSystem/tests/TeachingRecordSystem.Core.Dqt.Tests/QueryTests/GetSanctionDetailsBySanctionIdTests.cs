namespace TeachingRecordSystem.Core.Dqt.Tests.QueryTests;

public class GetSanctionDetailsBySanctionIdTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly CrmQueryDispatcher _crmQueryDispatcher;

    public GetSanctionDetailsBySanctionIdTests(CrmClientFixture crmClientFixture)
    {
        _dataScope = crmClientFixture.CreateTestDataScope();
        _crmQueryDispatcher = crmClientFixture.CreateQueryDispatcher();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _dataScope.DisposeAsync();

    [Fact]
    public async Task WhenCalled_ForSanction_ReturnsSanctionAsExpected()
    {
        // Arrange
        var sanctionCode = "G1";
        var sanctionCodeName = (await _dataScope.TestData.ReferenceDataCache.GetSanctionCodeByValue(sanctionCode)).dfeta_name;
        var person = await _dataScope.TestData.CreatePerson(x => x.WithSanction(sanctionCode));

        // Act
        var sanction = await _crmQueryDispatcher.ExecuteQuery(new GetSanctionDetailsBySanctionIdQuery(person.Sanctions.Single().SanctionId));

        // Assert
        Assert.NotNull(sanction);
        Assert.Equal(sanctionCodeName, sanction.Description);
    }
}
