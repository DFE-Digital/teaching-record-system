using System.Text;
using TeachingRecordSystem.Api.DataStore.Crm;
using TeachingRecordSystem.Api.DataStore.Crm.Models;
using Xunit;

namespace TeachingRecordSystem.Api.Tests.DataverseIntegration;

public class CreateNameChangeIncidentTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly DataverseAdapter _dataverseAdapter;

    public CreateNameChangeIncidentTests(CrmClientFixture crmClientFixture)
    {
        _dataScope = crmClientFixture.CreateTestDataScope();
        _dataverseAdapter = _dataScope.CreateDataverseAdapter();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _dataScope.DisposeAsync();

    [Fact]
    public async Task CreateNameChangeIncident_ExecutesSuccessfully()
    {
        // Arrange
        var createPersonResult = await _dataScope.CreateTestDataHelper().CreatePerson();

        var newFirstName1 = Faker.Name.First();
        var newFirstName2 = Faker.Name.First();
        var newMiddleName = Faker.Name.Middle();
        var newLastName = Faker.Name.Last();
        var evidenceFileName = "evidence.txt";
        var evidenceFileContent = new MemoryStream(Encoding.UTF8.GetBytes("Test file"));
        var evidenceFileMimeType = "text/plain";

        var command = new CreateNameChangeIncidentCommand()
        {
            ContactId = createPersonResult.TeacherId,
            Trn = createPersonResult.Trn,
            FirstName = newFirstName1,
            MiddleName = $"{newFirstName2} {newMiddleName}",
            LastName = newLastName,
            StatedFirstName = $"{newFirstName1} {newFirstName2}",
            StatedMiddleName = newMiddleName,
            StatedLastName = newLastName,
            EvidenceFileName = evidenceFileName,
            EvidenceFileContent = evidenceFileContent,
            EvidenceFileMimeType = evidenceFileMimeType,
            FromIdentity = true
        };

        // Act
        var incidentId = await _dataverseAdapter.CreateNameChangeIncident(command);

        // Assert
        using var ctx = new DqtCrmServiceContext(_dataScope.OrganizationService);

        var createdIncident = ctx.IncidentSet.SingleOrDefault(i => i.GetAttributeValue<Guid>(Incident.PrimaryIdAttribute) == incidentId);
        Assert.NotNull(createdIncident);
        Assert.Equal(createPersonResult.TeacherId, createdIncident.CustomerId.Id);
        Assert.Equal("Request to change name", createdIncident.Title);
        Assert.Equal(command.FirstName, createdIncident.dfeta_NewFirstName);
        Assert.Equal(command.MiddleName, createdIncident.dfeta_NewMiddleName);
        Assert.Equal(command.LastName, createdIncident.dfeta_NewLastName);
        Assert.Equal(command.StatedFirstName, createdIncident.dfeta_StatedFirstName);
        Assert.Equal(command.StatedMiddleName, createdIncident.dfeta_StatedMiddleName);
        Assert.Equal(command.StatedLastName, createdIncident.dfeta_StatedLastName);
        Assert.Equal(command.FromIdentity, createdIncident.dfeta_FromIdentity);
    }
}
