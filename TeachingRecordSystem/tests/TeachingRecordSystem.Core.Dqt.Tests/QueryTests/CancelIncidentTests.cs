namespace TeachingRecordSystem.Core.Dqt.Tests.QueryTests;

public class CancelIncidentTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly CrmQueryDispatcher _crmQueryDispatcher;

    public CancelIncidentTests(CrmClientFixture crmClientFixture)
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
        _ = await _crmQueryDispatcher.ExecuteQuery(new CancelIncidentQuery(createNameChangeIncidentResult.IncidentId));

        // Assert
        using var ctx = new DqtCrmServiceContext(_dataScope.OrganizationService);

        var canceledIncident = ctx.IncidentSet.SingleOrDefault(i => i.GetAttributeValue<Guid>(Incident.PrimaryIdAttribute) == createNameChangeIncidentResult.IncidentId);
        Assert.NotNull(canceledIncident);
        Assert.Equal(IncidentState.Canceled, canceledIncident.StateCode);
        Assert.Equal(Incident_StatusCode.Canceled, canceledIncident.StatusCode);
    }
}
