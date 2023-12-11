namespace TeachingRecordSystem.Core.Dqt.CrmIntegrationTests.QueryTests;

public class RejectIncidentTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly CrmQueryDispatcher _crmQueryDispatcher;

    public RejectIncidentTests(CrmClientFixture crmClientFixture)
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
        _ = await _crmQueryDispatcher.ExecuteQuery(new RejectIncidentQuery(createNameChangeIncidentResult.IncidentId, "Computer says no"));

        // Assert
        using var ctx = new DqtCrmServiceContext(_dataScope.OrganizationService);

        var rejectedIncident = ctx.IncidentSet.SingleOrDefault(i => i.GetAttributeValue<Guid>(Incident.PrimaryIdAttribute) == createNameChangeIncidentResult.IncidentId);
        Assert.NotNull(rejectedIncident);
        Assert.Equal(IncidentState.Resolved, rejectedIncident.StateCode);
        Assert.Equal(Incident_StatusCode.Rejected, rejectedIncident.StatusCode);
    }
}
