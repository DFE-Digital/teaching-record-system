namespace TeachingRecordSystem.Core.Dqt.CrmIntegrationTests.QueryTests;

public class UpdateIntegrationTransactionTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly CrmQueryDispatcher _crmQueryDispatcher;

    public UpdateIntegrationTransactionTests(CrmClientFixture crmClientFixture)
    {
        _dataScope = crmClientFixture.CreateTestDataScope();
        _crmQueryDispatcher = _dataScope.CreateQueryDispatcher();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _dataScope.DisposeAsync();

    [Fact]
    public async Task QueryExecutesSuccessfully()
    {
        // Arrange
        var totalCount = 10;
        var successCount = 5;
        var duplicateCount = 1;
        var failureCount = 4;
        var failureMessage = "fail";

        var startDate = new DateTime(2011, 01, 1);
        var endDate = new DateTime(2011, 01, 2);
        var typeId = dfeta_IntegrationInterface.GTCWalesImport;
        var query = new CreateIntegrationTransactionQuery()
        {
            TypeId = (int)typeId,
            StartDate = startDate
        };

        // Act
        var id = await _crmQueryDispatcher.ExecuteQuery(query);
        var updateQuery = new UpdateIntegrationTransactionQuery()
        {
            IntegrationTransactionId = id,
            EndDate = endDate,
            TotalCount = totalCount,
            SuccessCount = successCount,
            DuplicateCount = duplicateCount,
            FailureCount = failureCount,
            FailureMessage = failureMessage
        };
        var updateResult = await _crmQueryDispatcher.ExecuteQuery(updateQuery);

        // Assert
        using var ctx = new DqtCrmServiceContext(_dataScope.OrganizationService);
        var integrationTransaction = ctx.dfeta_integrationtransactionSet.SingleOrDefault(i => i.GetAttributeValue<Guid>(dfeta_integrationtransaction.PrimaryIdAttribute) == id);
        Assert.Equal(integrationTransaction!.dfeta_TotalCount, totalCount);
        Assert.Equal(integrationTransaction!.dfeta_SuccessCount, successCount);
        Assert.Equal(integrationTransaction!.dfeta_DuplicateCount, duplicateCount);
        Assert.Equal(integrationTransaction!.dfeta_FailureCount, failureCount);
        Assert.Equal(integrationTransaction!.dfeta_FailureMessage, failureMessage);
        Assert.Equal(integrationTransaction!.dfeta_EndDate, endDate);
    }
}
