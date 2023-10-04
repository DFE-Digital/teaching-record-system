namespace TeachingRecordSystem.Core.Dqt.Tests.QueryTests;

public class UpdateSanctionStateTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly CrmQueryDispatcher _crmQueryDispatcher;

    public UpdateSanctionStateTests(CrmClientFixture crmClientFixture)
    {
        _dataScope = crmClientFixture.CreateTestDataScope();
        _crmQueryDispatcher = crmClientFixture.CreateQueryDispatcher();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _dataScope.DisposeAsync();

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task QueryExecutesSuccessfully(bool setActive)
    {
        // Arrange
        var sanctionCode = "G1";
        var startDate = new DateOnly(2020, 01, 01);
        var createPersonResult = await _dataScope.TestData.CreatePerson(x => x.WithSanction(sanctionCode, startDate: startDate, isActive: !setActive));
        var sanction = createPersonResult.Sanctions.Single();

        // Act
        _ = await _crmQueryDispatcher.ExecuteQuery(new UpdateSanctionStateQuery(sanction.SanctionId, setActive ? dfeta_sanctionState.Active : dfeta_sanctionState.Inactive));

        // Assert
        using var ctx = new DqtCrmServiceContext(_dataScope.OrganizationService);
        var updatedSanction = ctx.dfeta_sanctionSet.SingleOrDefault(s => s.GetAttributeValue<Guid>(dfeta_sanction.PrimaryIdAttribute) == sanction.SanctionId);
        Assert.NotNull(updatedSanction);
        Assert.Equal(setActive ? dfeta_sanctionState.Active : dfeta_sanctionState.Inactive, updatedSanction.StateCode);
    }
}
