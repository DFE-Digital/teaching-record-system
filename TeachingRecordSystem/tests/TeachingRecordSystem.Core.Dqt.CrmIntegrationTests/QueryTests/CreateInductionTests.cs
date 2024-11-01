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
        var contact = await _dataScope.TestData.CreatePerson(x=> x.WithQts(new DateOnly(2024,01,01)));
        var startDate = new DateTime(2024, 01, 01);
        var completionDate = new DateTime(2024, 02, 01);
        var inductionStatus = dfeta_InductionStatus.InProgress;

        var query = new CreateInductionQuery()
        {
            PersonId = contact.PersonId,
            StartDate = startDate,
            CompletionDate = completionDate,
            InductionStatus = inductionStatus,
        };

        // Act
        var createdInductionId = await _crmQueryDispatcher.ExecuteQuery(query);

        // Assert
        using var ctx = new DqtCrmServiceContext(_dataScope.OrganizationService);
        var induction = ctx.dfeta_inductionSet.SingleOrDefault(i => i.GetAttributeValue<Guid>(dfeta_induction.PrimaryIdAttribute) == createdInductionId);
        Assert.NotNull(induction);
        Assert.Equal(induction.dfeta_PersonId.Id, contact.PersonId);
        Assert.Equal(induction.dfeta_StartDate, startDate);
        Assert.Equal(induction.dfeta_CompletionDate, completionDate);
        Assert.Equal(induction.dfeta_InductionStatus, inductionStatus);
    }
}
