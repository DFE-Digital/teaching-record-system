namespace TeachingRecordSystem.Core.Dqt.CrmIntegrationTests.QueryTests;

public class DeleteIntegrationTransactionTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly CrmQueryDispatcher _crmQueryDispatcher;

    public DeleteIntegrationTransactionTests(CrmClientFixture crmClientFixture)
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
        var startDate = new DateTime(2011, 01, 1);
        var typeId = dfeta_IntegrationInterface.GTCWalesImport;
        var createQuery = new CreateIntegrationTransactionQuery()
        {
            TypeId = (int)typeId,
            StartDate = startDate
        };

        // Act
        var id = await _crmQueryDispatcher.ExecuteQuery(createQuery);
        var deleteQuery = new DeleteIntegrationTransactionQuery()
        {
            IntegrationTransactionId = id
        };
        var deleted = await _crmQueryDispatcher.ExecuteQuery(deleteQuery);

        // Assert
        using var ctx = new DqtCrmServiceContext(_dataScope.OrganizationService);
        var createdIntegrationTransaction = ctx.dfeta_integrationtransactionSet.SingleOrDefault(i => i.GetAttributeValue<Guid>(dfeta_integrationtransaction.PrimaryIdAttribute) == id);
        Assert.Null(createdIntegrationTransaction);
    }
}
