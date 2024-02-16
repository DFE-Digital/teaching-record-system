namespace TeachingRecordSystem.Core.Dqt.CrmIntegrationTests.QueryTests;
public class CreateReviewTaskTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly CrmQueryDispatcher _crmQueryDispatcher;

    public CreateReviewTaskTests(CrmClientFixture crmClientFixture)
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
        var createPersonResult = await _dataScope.TestData.CreatePerson();
        var category = "Category";
        var description = "Description";
        var subject = "Subject";

        // Act
        var crmTaskId = await _crmQueryDispatcher.ExecuteQuery(new CreateReviewTaskQuery()
        {
            ContactId = createPersonResult.PersonId,
            Category = category,
            Subject = subject,
            Description = description
        });

        // Assert
        using var ctx = new DqtCrmServiceContext(_dataScope.OrganizationService);

        var createdTask = ctx.TaskSet.SingleOrDefault(s => s.GetAttributeValue<Guid>(Models.Task.Fields.Id) == crmTaskId);
        Assert.NotNull(createdTask);
        Assert.Equal(category, createdTask.Category);
        Assert.Equal(subject, createdTask.Subject);
        Assert.Equal(description, createdTask.Description);
    }
}
