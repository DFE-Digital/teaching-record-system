using System.Text;

namespace TeachingRecordSystem.Core.Dqt.Tests.QueryTests;

public class CreateNameChangeIncidentTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly CrmQueryDispatcher _crmQueryDispatcher;

    public CreateNameChangeIncidentTests(CrmClientFixture crmClientFixture)
    {
        _dataScope = crmClientFixture.CreateTestDataScope();
        _crmQueryDispatcher = _dataScope.CreateQueryDispatcher();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _dataScope.DisposeAsync();

    [Fact]
    public async Task QueryExecutesSuccessfully()
    {
        // Arrange
        var createPersonResult = await _dataScope.TestData.CreatePerson();

        var newFirstName = _dataScope.TestData.GenerateFirstName();
        var newMiddleName = _dataScope.TestData.GenerateMiddleName();
        var newLastName = _dataScope.TestData.GenerateLastName();
        var evidenceFileName = "evidence.txt";
        var evidenceFileContent = new MemoryStream(Encoding.UTF8.GetBytes("Test file"));
        var evidenceFileMimeType = "text/plain";

        var query = new CreateNameChangeIncidentQuery()
        {
            ContactId = createPersonResult.ContactId,
            FirstName = newFirstName,
            MiddleName = newMiddleName,
            LastName = newLastName,
            StatedFirstName = newFirstName,
            StatedMiddleName = newMiddleName,
            StatedLastName = newLastName,
            EvidenceFileName = evidenceFileName,
            EvidenceFileContent = evidenceFileContent,
            EvidenceFileMimeType = evidenceFileMimeType,
            FromIdentity = true
        };

        // Act
        var incidentId = await _crmQueryDispatcher.ExecuteQuery(query);

        // Assert
        using var ctx = new DqtCrmServiceContext(_dataScope.OrganizationService);

        var createdIncident = ctx.IncidentSet.SingleOrDefault(i => i.GetAttributeValue<Guid>(Incident.PrimaryIdAttribute) == incidentId);
        Assert.NotNull(createdIncident);
        Assert.Equal(createPersonResult.ContactId, createdIncident.CustomerId.Id);
        Assert.Equal("Request to change name", createdIncident.Title);
        Assert.Equal(newFirstName, createdIncident.dfeta_NewFirstName);
        Assert.Equal(newMiddleName, createdIncident.dfeta_NewMiddleName);
        Assert.Equal(newLastName, createdIncident.dfeta_NewLastName);
        Assert.Equal(newFirstName, createdIncident.dfeta_StatedFirstName);
        Assert.Equal(newMiddleName, createdIncident.dfeta_StatedMiddleName);
        Assert.Equal(newLastName, createdIncident.dfeta_StatedLastName);
        Assert.Equal(query.FromIdentity, createdIncident.dfeta_FromIdentity);
    }
}
