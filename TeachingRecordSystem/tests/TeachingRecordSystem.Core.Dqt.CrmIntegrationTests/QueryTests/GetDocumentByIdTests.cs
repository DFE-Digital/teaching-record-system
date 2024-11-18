namespace TeachingRecordSystem.Core.Dqt.CrmIntegrationTests.QueryTests;

public class GetDocumentByIdTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly CrmQueryDispatcher _crmQueryDispatcher;

    public GetDocumentByIdTests(CrmClientFixture crmClientFixture)
    {
        _dataScope = crmClientFixture.CreateTestDataScope();
        _crmQueryDispatcher = crmClientFixture.CreateQueryDispatcher();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _dataScope.DisposeAsync();

    [Fact]
    public async Task WhenCalled_WithDocumentIdForNonExistentDocument_ReturnsNull()
    {
        // Arrange
        var nonExistentDocumentId = Guid.NewGuid();

        // Act
        var document = await _crmQueryDispatcher.ExecuteQueryAsync(new GetDocumentByIdQuery(nonExistentDocumentId));

        // Assert
        Assert.Null(document);
    }

    [Fact]
    public async Task WhenCalled_WithDocumentIdForExistingDocument_ReturnsDocument()
    {
        // Arrange
        var createPersonResult = await _dataScope.TestData.CreatePersonAsync();
        var createNameChangeIncidentResult = await _dataScope.TestData.CreateNameChangeIncidentAsync(b => b.WithCustomerId(createPersonResult.ContactId));
        var documentId = createNameChangeIncidentResult.Evidence[0].DocumentId;

        // Act
        var document = await _crmQueryDispatcher.ExecuteQueryAsync(new GetDocumentByIdQuery(documentId));

        // Assert
        Assert.NotNull(document);
    }
}
