namespace TeachingRecordSystem.Core.Dqt.CrmIntegrationTests.QueryTests;

public class GetIncidentByTicketNumberTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly CrmQueryDispatcher _crmQueryDispatcher;

    public GetIncidentByTicketNumberTests(CrmClientFixture crmClientFixture)
    {
        _dataScope = crmClientFixture.CreateTestDataScope();
        _crmQueryDispatcher = crmClientFixture.CreateQueryDispatcher();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _dataScope.DisposeAsync();

    [Fact]
    public async Task WhenCalled_WithTicketNumberForNonExistentIncident_ReturnsNull()
    {
        // Arrange
        var nonExistentTicketNumber = Guid.NewGuid().ToString();

        // Act
        var incidentDetail = await _crmQueryDispatcher.ExecuteQueryAsync(new GetIncidentByTicketNumberQuery(nonExistentTicketNumber));

        // Assert
        Assert.Null(incidentDetail);
    }

    [Fact]
    public async Task WhenCalled_ForIncidentWithSingleDocument_ReturnsSingleIncidentAndSingleDocument()
    {
        // Arrange
        var createPersonResult = await _dataScope.TestData.CreatePersonAsync();
        var createNameChangeIncidentResult = await _dataScope.TestData.CreateNameChangeIncidentAsync(b => b.WithCustomerId(createPersonResult.ContactId));

        // Act
        var incidentDetail = await _crmQueryDispatcher.ExecuteQueryAsync(new GetIncidentByTicketNumberQuery(createNameChangeIncidentResult.TicketNumber));

        // Assert
        Assert.NotNull(incidentDetail);
        Assert.Single(incidentDetail.IncidentDocuments);
    }

    [Fact]
    public async Task WhenCalled_ForIncidentWithMultipleDocument_ReturnsSingleIncidentAndMultipleDocuments()
    {
        // Arrange
        var createPersonResult = await _dataScope.TestData.CreatePersonAsync();
        var createNameChangeIncidentResult = await _dataScope.TestData.CreateNameChangeIncidentAsync(b => b.WithCustomerId(createPersonResult.ContactId).WithMultipleEvidenceFiles());

        // Act
        var incidentDetail = await _crmQueryDispatcher.ExecuteQueryAsync(new GetIncidentByTicketNumberQuery(createNameChangeIncidentResult.TicketNumber));

        // Assert
        Assert.NotNull(incidentDetail);
        Assert.Equal(2, incidentDetail.IncidentDocuments.Length);
    }
}
