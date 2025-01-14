
namespace TeachingRecordSystem.Core.Dqt.CrmIntegrationTests.QueryTests;

public class UpdateInductionPeriodTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly CrmQueryDispatcher _crmQueryDispatcher;
    private readonly CrmClientFixture _fixture;

    public UpdateInductionPeriodTests(CrmClientFixture crmClientFixture)
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
        var inductionStartDate = new DateTime(2024, 01, 01);
        var inductionEndDate = new DateTime(2024, 02, 01);
        var inductionStatus = dfeta_InductionStatus.InProgress;
        var inductionPeriodStartDate = new DateTime(2024, 01, 01);
        var inductionPeriodEndDate = new DateTime(2024, 02, 01);
        var updatedInductionPeriodStartDate = new DateTime(2024, 06, 28);
        var updatedInductionPeriodEndDate = new DateTime(2024, 08, 01);
        var inductionId = Guid.NewGuid();
        var inductionPeriodId = Guid.NewGuid();
        var org = await _dataScope.TestData.CreateAccountAsync(x =>
        {
            x.WithName("Testing");
        });
        var contact = await _dataScope.TestData.CreatePersonAsync(x =>
        {
            x.WithQts(new DateOnly(2024, 01, 01));
        });

        var createInductionQuery = new CreateInductionTransactionalQuery()
        {
            Id = inductionId,
            ContactId = contact.PersonId,
            StartDate = inductionEndDate,
            CompletionDate = inductionEndDate,
            InductionStatus = inductionStatus,
        };
        var createInductionPeriodQuery = new CreateInductionPeriodTransactionalQuery()
        {
            Id = inductionPeriodId,
            InductionId = inductionId,
            AppropriateBodyId = org.Id,
            InductionStartDate = inductionPeriodStartDate,
            InductionEndDate = inductionPeriodEndDate,
        };
        var updatedInductionPeriodQuery = new UpdateInductionPeriodTransactionalQuery()
        {
            InductionPeriodId = inductionPeriodId,
            AppropriateBodyId = org.Id,
            InductionStartDate = updatedInductionPeriodStartDate,
            InductionEndDate = updatedInductionPeriodEndDate,
        };
        txn.AppendQuery(createInductionQuery);
        txn.AppendQuery(createInductionPeriodQuery);
        txn.AppendQuery(updatedInductionPeriodQuery);

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
        Assert.Equal(updatedInductionPeriodStartDate, inductionPeriod.dfeta_StartDate);
        Assert.Equal(updatedInductionPeriodEndDate, inductionPeriod.dfeta_EndDate);
    }
}
