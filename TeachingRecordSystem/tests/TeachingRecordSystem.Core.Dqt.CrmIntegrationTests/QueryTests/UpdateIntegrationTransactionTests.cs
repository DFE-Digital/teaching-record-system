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
        using var txn = _crmQueryDispatcher.CreateTransactionRequestBuilder();
        var totalCount = 10;
        var successCount = 5;
        var duplicateCount = 1;
        var failureCount = 4;
        var failureMessage = "fail";
        var fileName = "fileName.csv";

        var startDate = new DateTime(2011, 01, 1);
        var endDate = new DateTime(2011, 01, 2);
        var typeId = dfeta_IntegrationInterface.GTCWalesImport;
        var query = new CreateIntegrationTransactionQuery()
        {
            TypeId = (int)typeId,
            StartDate = startDate,
            FileName = fileName
        };

        // Act
        var id = await _crmQueryDispatcher.ExecuteQueryAsync(query);
        var updateQuery = new UpdateIntegrationTransactionTransactionalQuery()
        {
            IntegrationTransactionId = id,
            EndDate = endDate,
            TotalCount = totalCount,
            SuccessCount = successCount,
            DuplicateCount = duplicateCount,
            FailureCount = failureCount,
            FailureMessage = failureMessage
        };
        txn.AppendQuery(updateQuery);
        await txn.ExecuteAsync();

        // Assert
        using var ctx = new DqtCrmServiceContext(_dataScope.OrganizationService);
        var integrationTransaction = ctx.dfeta_integrationtransactionSet.SingleOrDefault(i => i.GetAttributeValue<Guid>(dfeta_integrationtransaction.PrimaryIdAttribute) == id);
        Assert.Equal(totalCount, integrationTransaction!.dfeta_TotalCount);
        Assert.Equal(successCount, integrationTransaction!.dfeta_SuccessCount);
        Assert.Equal(duplicateCount, integrationTransaction!.dfeta_DuplicateCount);
        Assert.Equal(failureCount, integrationTransaction!.dfeta_FailureCount);
        Assert.Equal(failureMessage, integrationTransaction!.dfeta_FailureMessage);
        Assert.Equal(endDate, integrationTransaction!.dfeta_EndDate);
        Assert.Equal(fileName, integrationTransaction!.dfeta_Filename);
    }
}
