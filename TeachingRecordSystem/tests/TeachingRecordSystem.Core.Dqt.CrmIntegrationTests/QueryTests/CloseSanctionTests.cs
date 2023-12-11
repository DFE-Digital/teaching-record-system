namespace TeachingRecordSystem.Core.Dqt.CrmIntegrationTests.QueryTests;

public class CloseSanctionTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly CrmQueryDispatcher _crmQueryDispatcher;

    public CloseSanctionTests(CrmClientFixture crmClientFixture)
    {
        _dataScope = crmClientFixture.CreateTestDataScope();
        _crmQueryDispatcher = crmClientFixture.CreateQueryDispatcher();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _dataScope.DisposeAsync();

    [Fact]
    public async Task QueryExecutesSuccessfully()
    {
        // Arrange
        var sanctionCode = "G1";
        var startDate = new DateOnly(2020, 01, 01);
        var endDate = new DateOnly(2021, 03, 09);
        var createPersonResult = await _dataScope.TestData.CreatePerson(x => x.WithSanction(sanctionCode, startDate: startDate));
        var sanction = createPersonResult.Sanctions.Single();

        // Act
        _ = await _crmQueryDispatcher.ExecuteQuery(new CloseSanctionQuery(sanction.SanctionId, endDate));

        // Assert
        using var ctx = new DqtCrmServiceContext(_dataScope.OrganizationService);

        var closedSanction = ctx.dfeta_sanctionSet.SingleOrDefault(s => s.GetAttributeValue<Guid>(dfeta_sanction.PrimaryIdAttribute) == sanction.SanctionId);
        Assert.NotNull(closedSanction);
        Assert.Equal(dfeta_sanctionState.Active, closedSanction.StateCode);
        Assert.Equal(endDate.FromDateOnlyWithDqtBstFix(isLocalTime: true), closedSanction.dfeta_EndDate);
        Assert.True(closedSanction.dfeta_Spent);
    }
}
