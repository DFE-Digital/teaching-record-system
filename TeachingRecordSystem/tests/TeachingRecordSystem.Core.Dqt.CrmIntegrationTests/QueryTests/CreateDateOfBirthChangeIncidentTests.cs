using System.Text;

namespace TeachingRecordSystem.Core.Dqt.CrmIntegrationTests.QueryTests;

public class CreateDateOfBirthChangeIncidentTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly CrmQueryDispatcher _crmQueryDispatcher;

    public CreateDateOfBirthChangeIncidentTests(CrmClientFixture crmClientFixture)
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

        var newDateOfBirth = _dataScope.TestData.GenerateChangedDateOfBirth(createPersonResult.DateOfBirth);
        var evidenceFileName = "evidence.txt";
        var evidenceFileContent = new MemoryStream(Encoding.UTF8.GetBytes("Test file"));
        var evidenceFileMimeType = "text/plain";

        var query = new CreateDateOfBirthChangeIncidentQuery()
        {
            ContactId = createPersonResult.ContactId,
            DateOfBirth = newDateOfBirth,
            EvidenceFileName = evidenceFileName,
            EvidenceFileContent = evidenceFileContent,
            EvidenceFileMimeType = evidenceFileMimeType,
            FromIdentity = true
        };

        // Act
        var (incidentId, ticketNumber) = await _crmQueryDispatcher.ExecuteQuery(query);

        // Assert
        using var ctx = new DqtCrmServiceContext(_dataScope.OrganizationService);

        var createdIncident = ctx.IncidentSet.SingleOrDefault(i => i.GetAttributeValue<Guid>(Incident.PrimaryIdAttribute) == incidentId);
        Assert.NotNull(createdIncident);
        Assert.NotNull(ticketNumber);
        Assert.Equal(createPersonResult.ContactId, createdIncident.CustomerId.Id);
        Assert.Equal("Request to change date of birth", createdIncident.Title);
        Assert.Equal(newDateOfBirth, DateOnly.FromDateTime(createdIncident.dfeta_NewDateofBirth!.Value));
        Assert.Equal(query.FromIdentity, createdIncident.dfeta_FromIdentity);
    }
}
