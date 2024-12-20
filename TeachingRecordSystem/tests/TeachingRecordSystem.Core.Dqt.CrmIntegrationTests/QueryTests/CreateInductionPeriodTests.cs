namespace TeachingRecordSystem.Core.Dqt.CrmIntegrationTests.QueryTests;

public class CreateInductionPeriodTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly CrmQueryDispatcher _crmQueryDispatcher;
    private readonly CrmClientFixture _fixture;

    public CreateInductionPeriodTests(CrmClientFixture crmClientFixture)
    {
        _fixture = crmClientFixture;
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
        var org = await _dataScope.TestData.CreateAccountAsync(x =>
        {
            x.WithName("Testing");
        });
        var contact = await _dataScope.TestData.CreatePersonAsync(x =>
        {
            x.WithQts(new DateOnly(2024, 01, 01));
        });
        var startDate = new DateTime(2024, 01, 01);
        var completionDate = new DateTime(2024, 02, 01);
        var inductionStatus = dfeta_InductionStatus.InProgress;
        var inductionStartDate = new DateTime(2024, 01, 01);
        var inductionEndDate = new DateTime(2024, 02, 01);
        var inductionId = Guid.NewGuid();
        var inductionPeriodId = Guid.NewGuid();
        var queryInduction = new CreateInductionTransactionalQuery()
        {
            Id = inductionId,
            ContactId = contact.PersonId,
            StartDate = startDate,
            CompletionDate = completionDate,
            InductionStatus = inductionStatus,
        };
        var queryInductionPeriod = new CreateInductionPeriodTransactionalQuery()
        {
            Id = inductionPeriodId,
            InductionId = inductionId,
            AppropriateBodyId = org.Id,
            InductionStartDate = inductionStartDate,
            InductionEndDate = inductionEndDate,
        };
        txn.AppendQuery(queryInduction);
        txn.AppendQuery(queryInductionPeriod);

        // Act
        await txn.ExecuteAsync();

        // Assert
        using var ctx = new DqtCrmServiceContext(_dataScope.OrganizationService);
        var induction = ctx.dfeta_inductionSet.SingleOrDefault(i => i.GetAttributeValue<Guid>(dfeta_induction.PrimaryIdAttribute) == inductionId);
        var inductionPeriod = ctx.dfeta_inductionperiodSet.SingleOrDefault(i => i.GetAttributeValue<Guid>(dfeta_inductionperiod.PrimaryIdAttribute) == inductionPeriodId);
        Assert.NotNull(induction);
        Assert.NotNull(inductionPeriod);
        Assert.Equal(inductionPeriodId, inductionPeriod.Id);
        Assert.Equal(inductionId, inductionPeriod.dfeta_InductionId.Id);
        Assert.Equal(org.Id, inductionPeriod.dfeta_AppropriateBodyId.Id);
        Assert.Equal(startDate, inductionPeriod.dfeta_StartDate);
        Assert.Equal(inductionEndDate, inductionPeriod.dfeta_EndDate);
    }
}
