namespace TeachingRecordSystem.Core.Dqt.CrmIntegrationTests.QueryTests;

public class DeleteAnnotationTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly CrmQueryDispatcher _crmQueryDispatcher;

    public DeleteAnnotationTests(CrmClientFixture crmClientFixture)
    {
        _dataScope = crmClientFixture.CreateTestDataScope();
        _crmQueryDispatcher = crmClientFixture.CreateQueryDispatcher();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _dataScope.DisposeAsync();

    [Fact]
    public async Task QueryExecutesSuccessfully()
    {
        // Arrange
        var createPersonResult = await _dataScope.TestData.CreatePersonAsync();
        var createIncidentResult = await _dataScope.TestData.CreateDateOfBirthChangeIncidentAsync(c => c.WithCustomerId(createPersonResult.ContactId));

        var annotationId = createIncidentResult.AnnotationId;
        var @event = new DqtAnnotationDeletedEvent()
        {
            AnnotationId = annotationId,
            CreatedUtc = DateTime.UtcNow,
            EventId = Guid.NewGuid(),
            RaisedBy = DataStore.Postgres.Models.SystemUser.SystemUserId
        };

        // Act
        await _crmQueryDispatcher.ExecuteQueryAsync(new DeleteAnnotationQuery(annotationId, EventInfo.Create(@event)));

        // Assert
        using var ctx = new DqtCrmServiceContext(_dataScope.OrganizationService);

        var annotation = ctx.AnnotationSet.SingleOrDefault(i => i.Id == annotationId);
        Assert.Null(annotation);
        var persistedEvent = ctx.dfeta_TRSEventSet.SingleOrDefault(e => e.dfeta_TRSEventId == @event.EventId);
        Assert.NotNull(persistedEvent);
        Assert.Equivalent(@event, EventInfo.Deserialize(persistedEvent.dfeta_Payload).Event);
    }
}
