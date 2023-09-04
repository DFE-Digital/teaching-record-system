namespace TeachingRecordSystem.Core.Dqt.Tests.QueryTests;

public class ApproveIncidentTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly CrmQueryDispatcher _crmQueryDispatcher;

    public ApproveIncidentTests(CrmClientFixture crmClientFixture)
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
        var createPersonResult = await _dataScope.TestData.CreatePerson();
        var createNameChangeIncidentResult = await _dataScope.TestData.CreateDateOfBirthChangeIncident(b => b.WithCustomerId(createPersonResult.ContactId));

        // Act
        _ = await _crmQueryDispatcher.ExecuteQuery(new ApproveIncidentQuery(createNameChangeIncidentResult.IncidentId));

        // Assert
        using var ctx = new DqtCrmServiceContext(_dataScope.OrganizationService);

        var approvedIncident = ctx.IncidentSet.SingleOrDefault(i => i.GetAttributeValue<Guid>(Incident.PrimaryIdAttribute) == createNameChangeIncidentResult.IncidentId);
        Assert.NotNull(approvedIncident);
        Assert.Equal(IncidentState.Resolved, approvedIncident.StateCode);
        Assert.Equal(Incident_StatusCode.Approved, approvedIncident.StatusCode);
    }
}
