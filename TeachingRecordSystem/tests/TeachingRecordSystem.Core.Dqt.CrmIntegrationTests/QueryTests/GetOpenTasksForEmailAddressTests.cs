namespace TeachingRecordSystem.Core.Dqt.CrmIntegrationTests.QueryTests;

public class GetOpenTasksForEmailAddressTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly CrmQueryDispatcher _crmQueryDispatcher;

    public GetOpenTasksForEmailAddressTests(CrmClientFixture crmClientFixture)
    {
        _dataScope = crmClientFixture.CreateTestDataScope();
        _crmQueryDispatcher = crmClientFixture.CreateQueryDispatcher();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _dataScope.DisposeAsync();

    [Fact]
    public async Task WhenCalled_WithEmailWithNoOpenTasks_ReturnsEmpty()
    {
        // Arrange
        var emailWithNoOpenTasks = Faker.Internet.Email();

        // Act
        var result = await _crmQueryDispatcher.ExecuteQuery(new GetOpenTasksForEmailAddressQuery(emailWithNoOpenTasks));

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task WhenCalled_WithEmailWithOpenTasks_ReturnsTasks()
    {
        // Arrange
        var person = await _dataScope.TestData.CreatePerson();
        var emailWithOpenTasks = Faker.Internet.Email();
        await _dataScope.TestData.CreateCrmTask(x =>
        {
            x.WithPersonId(person.ContactId);
            x.WithEmailAddress(emailWithOpenTasks);
        });

        // Act
        var result = await _crmQueryDispatcher.ExecuteQuery(new GetOpenTasksForEmailAddressQuery(emailWithOpenTasks));

        // Assert
        Assert.NotEmpty(result);
        Assert.Collection(result, item1 =>
        {
            Assert.Equal(emailWithOpenTasks, item1.dfeta_EmailAddress);
        });
    }

    [Fact]
    public async Task WhenCalled_WithEmailWithCompletedTasks_ReturnsEmpty()
    {
        // Arrange
        var person = await _dataScope.TestData.CreatePerson();
        var emailWithOpenTasks = Faker.Internet.Email();
        await _dataScope.TestData.CreateCrmTask(x =>
        {
            x.WithPersonId(person.ContactId);
            x.WithEmailAddress(emailWithOpenTasks);
            x.WithCompletedStatus();
        });

        // Act
        var result = await _crmQueryDispatcher.ExecuteQuery(new GetOpenTasksForEmailAddressQuery(emailWithOpenTasks));

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task WhenCalled_WithEmailWithCancelledTasks_ReturnsEmpty()
    {
        // Arrange
        var person = await _dataScope.TestData.CreatePerson();
        var emailWithOpenTasks = Faker.Internet.Email();
        await _dataScope.TestData.CreateCrmTask(x =>
        {
            x.WithPersonId(person.ContactId);
            x.WithEmailAddress(emailWithOpenTasks);
            x.WithCanceledStatus();
        });

        // Act
        var result = await _crmQueryDispatcher.ExecuteQuery(new GetOpenTasksForEmailAddressQuery(emailWithOpenTasks));

        // Assert
        Assert.Empty(result);
    }
}
