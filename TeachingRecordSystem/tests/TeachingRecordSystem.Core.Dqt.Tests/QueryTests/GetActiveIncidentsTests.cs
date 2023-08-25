namespace TeachingRecordSystem.Core.Dqt.Tests.QueryTests;

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
        var incidents = await _crmQueryDispatcher.ExecuteQuery(new GetActiveIncidentsQuery());

        // Assert
        Assert.Contains(incidents, i => i.Id == activeCreateNameChangeIncidentResult.IncidentId);
        Assert.Contains(incidents, i => i.Id == activeCreateDateOfBirthChangeIncidentResult.IncidentId);
        Assert.DoesNotContain(incidents, i => i.Id == cancelledCreateNameChangeIncidentResult.IncidentId);
        Assert.DoesNotContain(incidents, i => i.Id == rejectedCreateNameChangeIncidentResult.IncidentId);
        Assert.DoesNotContain(incidents, i => i.Id == approvedCreateDateOfBirthChangeIncidentResult.IncidentId);
    }
}