
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
        var inductionStartDate = new DateTime(2024, 01, 01);
        var inductionEndDate = new DateTime(2024, 02, 01);
        var inductionStatus = dfeta_InductionStatus.InProgress;
        var inductionPeriodStartDate = new DateTime(2024, 01, 01);
        var inductionPeriodEndDate = new DateTime(2024, 02, 01);
        var updatedInductionPeriodStartDate = new DateTime(2024, 06, 28);
        var updatedInductionPeriodEndDate = new DateTime(2024, 08, 01);
        var org = await _dataScope.TestData.CreateAccount(x =>
        {
            x.WithName("Testing");
        });
        var contact = await _dataScope.TestData.CreatePerson(x =>
        {
            x.WithQts(new DateOnly(2024, 01, 01));
        });

        var queryInduction = new CreateInductionQuery()
        {
            PersonId = contact.PersonId,
            StartDate = inductionEndDate,
            CompletionDate = inductionEndDate,
            InductionStatus = inductionStatus,
        };
        var createdInductionId = await _crmQueryDispatcher.ExecuteQuery(queryInduction);
        var queryInductionPeriod = new CreateInductionPeriodQuery()
        {
            InductionID = createdInductionId,
            AppropriateBodyID = org.Id,
            InductionStartDate = inductionPeriodStartDate,
            InductionEndDate = inductionPeriodEndDate,
        };
        var inductionPeriodId = await _crmQueryDispatcher.ExecuteQuery(queryInductionPeriod);
        var updatedInductionPeriodQuery = new UpdateInductionPeriodQuery()
        {
            InductionPeriodID = inductionPeriodId,
            InductionID = createdInductionId,
            AppropriateBodyID = org.Id,
            InductionStartDate = updatedInductionPeriodStartDate,
            InductionEndDate = updatedInductionPeriodEndDate,
        };

        // Act
        var updatedInductionPeriod = await _crmQueryDispatcher.ExecuteQuery(updatedInductionPeriodQuery);

        // Assert
        using var ctx = new DqtCrmServiceContext(_dataScope.OrganizationService);
        var induction = ctx.dfeta_inductionSet.SingleOrDefault(i => i.GetAttributeValue<Guid>(dfeta_induction.PrimaryIdAttribute) == createdInductionId);
        var inductionPeriod = ctx.dfeta_inductionperiodSet.SingleOrDefault(i => i.GetAttributeValue<Guid>(dfeta_inductionperiod.PrimaryIdAttribute) == inductionPeriodId);
        Assert.NotNull(induction);
        Assert.NotNull(inductionPeriod);
        Assert.Equal(inductionPeriod.Id, inductionPeriodId);
        Assert.Equal(inductionPeriod.dfeta_InductionId.Id, createdInductionId);
        Assert.Equal(inductionPeriod.dfeta_AppropriateBodyId.Id, org.Id);
        Assert.Equal(inductionPeriod.dfeta_StartDate, updatedInductionPeriodStartDate);
        Assert.Equal(inductionPeriod.dfeta_EndDate, updatedInductionPeriodEndDate);
    }
}
