namespace TeachingRecordSystem.Core.Dqt.CrmIntegrationTests.QueryTests;

public class UpdateInductionTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly CrmQueryDispatcher _crmQueryDispatcher;

    public UpdateInductionTests(CrmClientFixture crmClientFixture)
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
        var startDate = new DateTime(2001, 05, 1);
        var completionDate = new DateTime(2011, 05, 1);
        var inductionStatus = dfeta_InductionStatus.Pass;
        var inductionId = Guid.NewGuid();
        var contact = await _dataScope.TestData.CreatePersonAsync(x =>
        {
            x.WithQts(new DateOnly(2024, 01, 01));
        });
        var queryInduction = new CreateInductionTransactionalQuery()
        {
            Id = inductionId,
            ContactId = contact.PersonId,
            StartDate = startDate,
            CompletionDate = null,
            InductionStatus = dfeta_InductionStatus.InProgress,
        };
        txn.AppendQuery(queryInduction);

        // Act
        var updateInductionQuery = new UpdateInductionTransactionalQuery()
        {
            InductionId = inductionId,
            CompletionDate = completionDate,
            InductionStatus = inductionStatus
        };
        txn.AppendQuery(updateInductionQuery);
        await txn.ExecuteAsync();

        // Assert
        using var ctx = new DqtCrmServiceContext(_dataScope.OrganizationService);
        var induction = ctx.dfeta_inductionSet.SingleOrDefault(i => i.GetAttributeValue<Guid>(dfeta_induction.PrimaryIdAttribute) == inductionId);
        Assert.NotNull(induction);
        Assert.Equal(startDate, induction.dfeta_StartDate);
        Assert.Equal(completionDate, induction.dfeta_CompletionDate);
        Assert.Equal(inductionStatus, induction.dfeta_InductionStatus);
    }
}
