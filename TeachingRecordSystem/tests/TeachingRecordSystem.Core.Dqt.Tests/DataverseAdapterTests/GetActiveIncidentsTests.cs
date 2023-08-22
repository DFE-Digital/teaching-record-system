namespace TeachingRecordSystem.Core.Dqt.Tests.DataverseAdapterTests;

public class GetActiveIncidentsTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly DataverseAdapter _dataverseAdapter;

    public GetActiveIncidentsTests(CrmClientFixture crmClientFixture)
    {
        _dataScope = crmClientFixture.CreateTestDataScope();
        _dataverseAdapter = _dataScope.CreateDataverseAdapter();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _dataScope.DisposeAsync();

    [Fact]
    public async Task WhenCalled_ReturnsActiveIncidentsOnly()
    {
        // Arrange
        var cancelledCreateNameChangeIncidentResult = await _dataScope.TestData.CreateNameChangeIncident((builder) => builder.WithCanceledStatus());
        var activeCreateNameChangeIncidentResult = await _dataScope.TestData.CreateNameChangeIncident();
        var rejectedCreateNameChangeIncidentResult = await _dataScope.TestData.CreateNameChangeIncident((builder) => builder.WithRejectedStatus());
        var activeCreateDateOfBirthChangeIncidentResult = await _dataScope.TestData.CreateDateOfBirthChangeIncident();
        var approvedCreateDateOfBirthChangeIncidentResult = await _dataScope.TestData.CreateDateOfBirthChangeIncident((builder) => builder.WithApprovedStatus());

        // Act
        var incidents = await _dataverseAdapter.GetActiveIncidents();

        // Assert
        Assert.Contains(incidents, i => i.Id == activeCreateNameChangeIncidentResult.IncidentId);
        Assert.Contains(incidents, i => i.Id == activeCreateDateOfBirthChangeIncidentResult.IncidentId);
        Assert.DoesNotContain(incidents, i => i.Id == cancelledCreateNameChangeIncidentResult.IncidentId);
        Assert.DoesNotContain(incidents, i => i.Id == rejectedCreateNameChangeIncidentResult.IncidentId);
        Assert.DoesNotContain(incidents, i => i.Id == approvedCreateDateOfBirthChangeIncidentResult.IncidentId);
    }
}
