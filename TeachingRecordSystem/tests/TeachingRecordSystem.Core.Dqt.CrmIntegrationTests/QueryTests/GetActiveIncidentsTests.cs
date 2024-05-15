namespace TeachingRecordSystem.Core.Dqt.CrmIntegrationTests.QueryTests;

public class GetActiveIncidentsTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly CrmQueryDispatcher _crmQueryDispatcher;

    public GetActiveIncidentsTests(CrmClientFixture crmClientFixture)
    {
        _dataScope = crmClientFixture.CreateTestDataScope();
        _crmQueryDispatcher = crmClientFixture.CreateQueryDispatcher();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _dataScope.DisposeAsync();

    [Fact]
    public async Task ReturnsActiveIncidentsOnly()
    {
        // Arrange
        var createPersonResult = await _dataScope.TestData.CreatePerson();
        var cancelledCreateNameChangeIncidentResult = await _dataScope.TestData.CreateNameChangeIncident(b => b.WithCustomerId(createPersonResult.ContactId).WithCanceledStatus());
        var activeCreateNameChangeIncidentResult = await _dataScope.TestData.CreateNameChangeIncident(b => b.WithCustomerId(createPersonResult.ContactId));
        var rejectedCreateNameChangeIncidentResult = await _dataScope.TestData.CreateNameChangeIncident(b => b.WithCustomerId(createPersonResult.ContactId).WithRejectedStatus());
        var activeCreateDateOfBirthChangeIncidentResult = await _dataScope.TestData.CreateDateOfBirthChangeIncident(b => b.WithCustomerId(createPersonResult.ContactId));
        var approvedCreateDateOfBirthChangeIncidentResult = await _dataScope.TestData.CreateDateOfBirthChangeIncident(b => b.WithCustomerId(createPersonResult.ContactId).WithApprovedStatus());

        // Act
        var result = await _crmQueryDispatcher.ExecuteQuery(new GetActiveIncidentsQuery(1, 50));

        // Assert        
        Assert.Contains(result.Incidents, i => i.Id == activeCreateNameChangeIncidentResult.IncidentId);
        Assert.Contains(result.Incidents, i => i.Id == activeCreateDateOfBirthChangeIncidentResult.IncidentId);
        Assert.DoesNotContain(result.Incidents, i => i.Id == cancelledCreateNameChangeIncidentResult.IncidentId);
        Assert.DoesNotContain(result.Incidents, i => i.Id == rejectedCreateNameChangeIncidentResult.IncidentId);
        Assert.DoesNotContain(result.Incidents, i => i.Id == approvedCreateDateOfBirthChangeIncidentResult.IncidentId);
    }
}
