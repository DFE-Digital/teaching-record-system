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
        var createPersonResult = await _dataScope.TestData.CreatePersonAsync();
        var email = _dataScope.TestData.GenerateUniqueEmail();
        var newDateOfBirth = _dataScope.TestData.GenerateChangedDateOfBirth(createPersonResult.DateOfBirth);
        var uniqueId = Guid.NewGuid();
        var evidenceFileName = $"evidence-{uniqueId}.jpg";
        var evidenceFileContent = new MemoryStream(TestCommon.TestData.JpegImage);
        var evidenceFileMimeType = "image/jpeg";

        var query = new CreateDateOfBirthChangeIncidentQuery()
        {
            ContactId = createPersonResult.ContactId,
            DateOfBirth = newDateOfBirth,
            EvidenceFileName = evidenceFileName,
            EvidenceFileContent = evidenceFileContent,
            EvidenceFileMimeType = evidenceFileMimeType,
            FromIdentity = true,
            EmailAddress = email
        };

        // Act
        var (incidentId, ticketNumber) = await _crmQueryDispatcher.ExecuteQueryAsync(query);

        // Assert
        using var ctx = new DqtCrmServiceContext(_dataScope.OrganizationService);

        var createdIncident = ctx.IncidentSet.SingleOrDefault(i => i.GetAttributeValue<Guid>(Incident.PrimaryIdAttribute) == incidentId);
        Assert.NotNull(createdIncident);
        Assert.NotNull(ticketNumber);
        Assert.Equal(createPersonResult.ContactId, createdIncident.CustomerId.Id);
        Assert.Equal("Request to change date of birth", createdIncident.Title);
        Assert.Equal(newDateOfBirth, DateOnly.FromDateTime(createdIncident.dfeta_NewDateofBirth!.Value));
        Assert.Equal(query.FromIdentity, createdIncident.dfeta_FromIdentity);
        Assert.Equal(query.EmailAddress, createdIncident.dfeta_emailaddress);

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
