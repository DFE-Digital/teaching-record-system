namespace TeachingRecordSystem.Core.Dqt.CrmIntegrationTests.QueryTests;

public class CreateTrnRequestTaskTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly CrmQueryDispatcher _crmQueryDispatcher;

    public CreateTrnRequestTaskTests(CrmClientFixture crmClientFixture)
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
        var description = "Test description";
        var uniqueId = Guid.NewGuid();
        var evidenceFileName = $"evidence-{uniqueId}.jpg";
        var evidenceFileContent = new MemoryStream(TestCommon.TestData.JpegImage);
        var evidenceFileMimeType = "image/jpeg";

        var query = new CreateTrnRequestTaskQuery()
        {
            Description = description,
            EvidenceFileName = evidenceFileName,
            EvidenceFileContent = evidenceFileContent,
            EvidenceFileMimeType = evidenceFileMimeType
        };

        // Act
        var crmTaskId = await _crmQueryDispatcher.ExecuteQuery(query);

        // Assert
        using var ctx = new DqtCrmServiceContext(_dataScope.OrganizationService);

        var createdCrmTask = ctx.TaskSet.SingleOrDefault(i => i.GetAttributeValue<Guid>(CrmTask.PrimaryIdAttribute) == crmTaskId);
        Assert.NotNull(createdCrmTask);
        Assert.Equal("TRN Request", createdCrmTask.Subject);
        Assert.Equal(description, createdCrmTask.Description);

        var createdAnnotation = ctx.AnnotationSet.SingleOrDefault(i => i.GetAttributeValue<string>(Annotation.Fields.FileName) == evidenceFileName);
        Assert.NotNull(createdAnnotation);
        Assert.Equal(createdCrmTask.Id, createdAnnotation.ObjectId.Id);
        Assert.Equal(CrmTask.EntityLogicalName, createdAnnotation.ObjectTypeCode);
    }
}
