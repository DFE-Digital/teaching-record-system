namespace TeachingRecordSystem.Core.Dqt.Tests.QueryTests;

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
    public async Task WhenCalled_WithTicketNumberForNonExistentIncident_ReturnsTupleWithNullProperties()
    {
        // Arrange
        var nonExistentTicketNumber = Guid.NewGuid().ToString();

        // Act
        var (incident, documents) = await _crmQueryDispatcher.ExecuteQuery(new GetIncidentByTicketNumberQuery(nonExistentTicketNumber));

        // Assert
        Assert.Null(incident);
        Assert.Null(documents);
    }

    [Fact]
    public async Task WhenCalled_ForIncidentWithSingleDocument_ReturnsSingleIncidentAndSingleDocument()
    {
        // Arrange
        var createPersonResult = await _dataScope.TestData.CreatePerson();
        var createNameChangeIncidentResult = await _dataScope.TestData.CreateNameChangeIncident(b => b.WithCustomerId(createPersonResult.ContactId));

        // Act
        var (incident, documents) = await _crmQueryDispatcher.ExecuteQuery(new GetIncidentByTicketNumberQuery(createNameChangeIncidentResult.TicketNumber));

        // Assert
        Assert.NotNull(incident);
        Assert.NotNull(documents);
        Assert.Single(documents);
    }

    [Fact]
    public async Task WhenCalled_ForIncidentWithMultipleDocument_ReturnsSingleIncidentAndMultipleDocuments()
    {
        // Arrange
        var createPersonResult = await _dataScope.TestData.CreatePerson();
        var createNameChangeIncidentResult = await _dataScope.TestData.CreateNameChangeIncident(b => b.WithCustomerId(createPersonResult.ContactId).WithMultipleEvidenceFiles());

        // Act
        var (incident, documents) = await _crmQueryDispatcher.ExecuteQuery(new GetIncidentByTicketNumberQuery(createNameChangeIncidentResult.TicketNumber));

        // Assert
        Assert.NotNull(incident);
        Assert.NotNull(documents);
        Assert.Equal(2, documents.Length);
    }
}
