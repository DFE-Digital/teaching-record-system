namespace TeachingRecordSystem.Core.Dqt.CrmIntegrationTests.QueryTests;

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
        var uniqueId = Guid.NewGuid();
        var evidenceFileName = $"evidence-{uniqueId}.jpg";
        var evidenceFileContent = new MemoryStream(TestCommon.TestData.JpegImage);
        var evidenceFileMimeType = "image/jpeg";

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
        var (incidentId, ticketNumber) = await _crmQueryDispatcher.ExecuteQuery(query);

        // Assert
        using var ctx = new DqtCrmServiceContext(_dataScope.OrganizationService);

        var createdIncident = ctx.IncidentSet.SingleOrDefault(i => i.GetAttributeValue<Guid>(Incident.PrimaryIdAttribute) == incidentId);
        Assert.NotNull(createdIncident);
        Assert.NotNull(ticketNumber);
        Assert.Equal(createPersonResult.ContactId, createdIncident.CustomerId.Id);
        Assert.Equal("Request to change name", createdIncident.Title);
        Assert.Equal(newFirstName, createdIncident.dfeta_NewFirstName);
        Assert.Equal(newMiddleName, createdIncident.dfeta_NewMiddleName);
        Assert.Equal(newLastName, createdIncident.dfeta_NewLastName);
        Assert.Equal(newFirstName, createdIncident.dfeta_StatedFirstName);
        Assert.Equal(newMiddleName, createdIncident.dfeta_StatedMiddleName);
        Assert.Equal(newLastName, createdIncident.dfeta_StatedLastName);
        Assert.Equal(query.FromIdentity, createdIncident.dfeta_FromIdentity);

        var createdDocument = ctx.dfeta_documentSet.SingleOrDefault(i => i.GetAttributeValue<string>(dfeta_document.Fields.dfeta_name) == evidenceFileName);
        Assert.NotNull(createdDocument);
        Assert.Equal(createPersonResult.ContactId, createdDocument.dfeta_PersonId.Id);
        Assert.Equal(createdIncident.Id, createdDocument.dfeta_CaseId.Id);
        Assert.Equal(dfeta_DocumentType.ChangeofNameDOBEvidence, createdDocument.dfeta_Type);

        var createdAnnotation = ctx.AnnotationSet.SingleOrDefault(i => i.GetAttributeValue<string>(Annotation.Fields.FileName) == evidenceFileName);
        Assert.NotNull(createdAnnotation);
        Assert.Equal(createdDocument.Id, createdAnnotation.ObjectId.Id);
        Assert.Equal(dfeta_document.EntityLogicalName, createdAnnotation.ObjectTypeCode);

        var contact = ctx.ContactSet.Single(c => c.Id == createPersonResult.ContactId);
        Assert.False(contact.dfeta_AllowPiiUpdatesFromRegister);
    }
}
