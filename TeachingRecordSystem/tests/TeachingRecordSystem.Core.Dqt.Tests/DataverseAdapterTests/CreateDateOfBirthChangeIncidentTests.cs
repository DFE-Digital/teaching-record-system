using System.Text;

namespace TeachingRecordSystem.Core.Dqt.Tests.DataverseAdapterTests;

public class CreateDateOfBirthChangeIncidentTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly DataverseAdapter _dataverseAdapter;

    public CreateDateOfBirthChangeIncidentTests(CrmClientFixture crmClientFixture)
    {
        _dataScope = crmClientFixture.CreateTestDataScope();
        _dataverseAdapter = _dataScope.CreateDataverseAdapter();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _dataScope.DisposeAsync();

    [Fact]
    public async Task CreateDateOfBirthChangeIncident_ExecutesSuccessfully()
    {
        // Arrange
        var createPersonResult = await _dataScope.CreateTestDataHelper().CreatePerson();

        var newDateOfBirth = DateOnly.FromDateTime(Faker.Identification.DateOfBirth());
        var evidenceFileName = "evidence.txt";
        var evidenceFileContent = new MemoryStream(Encoding.UTF8.GetBytes("Test file"));
        var evidenceFileMimeType = "text/plain";

        var command = new CreateDateOfBirthChangeIncidentCommand()
        {
            ContactId = createPersonResult.TeacherId,
            Trn = createPersonResult.Trn,
            DateOfBirth = newDateOfBirth,
            EvidenceFileName = evidenceFileName,
            EvidenceFileContent = evidenceFileContent,
            EvidenceFileMimeType = evidenceFileMimeType,
            FromIdentity = true
        };

        // Act
        var incidentId = await _dataverseAdapter.CreateDateOfBirthChangeIncident(command);

        // Assert
        using var ctx = new DqtCrmServiceContext(_dataScope.OrganizationService);

        var createdIncident = ctx.IncidentSet.SingleOrDefault(i => i.GetAttributeValue<Guid>(Incident.PrimaryIdAttribute) == incidentId);
        Assert.NotNull(createdIncident);
        Assert.Equal(createPersonResult.TeacherId, createdIncident.CustomerId.Id);
        Assert.Equal("Request to change date of birth", createdIncident.Title);
        Assert.Equal(newDateOfBirth, DateOnly.FromDateTime(createdIncident.dfeta_NewDateofBirth!.Value));
        Assert.Equal(command.FromIdentity, createdIncident.dfeta_FromIdentity);
    }
}
