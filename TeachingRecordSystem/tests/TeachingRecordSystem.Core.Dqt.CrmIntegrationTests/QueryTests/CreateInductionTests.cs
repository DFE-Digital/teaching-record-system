namespace TeachingRecordSystem.Core.Dqt.CrmIntegrationTests.QueryTests;

public class CreateInductionTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly CrmQueryDispatcher _crmQueryDispatcher;
    private readonly CrmClientFixture _fixture;

    public CreateInductionTests(CrmClientFixture crmClientFixture)
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
        var contact = await _dataScope.TestData.CreatePersonAsync(x => x.WithQts(new DateOnly(2024, 01, 01)));
        var startDate = new DateTime(2024, 01, 01);
        var completionDate = new DateTime(2024, 02, 01);
        var inductionStatus = dfeta_InductionStatus.InProgress;
        var inductionId = Guid.NewGuid();

        var query = new CreateInductionTransactionalQuery()
        {
            Id = inductionId,
            ContactId = contact.PersonId,
            StartDate = startDate,
            CompletionDate = completionDate,
            InductionStatus = inductionStatus,
        };
        txn.AppendQuery(query);

        // Act
        await txn.ExecuteAsync();

        // Assert
        using var ctx = new DqtCrmServiceContext(_dataScope.OrganizationService);
        var induction = ctx.dfeta_inductionSet.SingleOrDefault(i => i.GetAttributeValue<Guid>(dfeta_induction.PrimaryIdAttribute) == inductionId);
        Assert.NotNull(induction);
        Assert.Equal(contact.PersonId, induction.dfeta_PersonId.Id);
        Assert.Equal(startDate, induction.dfeta_StartDate);
        Assert.Equal(completionDate, induction.dfeta_CompletionDate);
        Assert.Equal(inductionStatus, induction.dfeta_InductionStatus);
    }
}
