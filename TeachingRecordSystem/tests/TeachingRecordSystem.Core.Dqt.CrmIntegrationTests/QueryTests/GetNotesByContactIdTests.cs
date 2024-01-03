namespace TeachingRecordSystem.Core.Dqt.CrmIntegrationTests.QueryTests;

public class GetNotesByContactIdTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly CrmQueryDispatcher _crmQueryDispatcher;

    public GetNotesByContactIdTests(CrmClientFixture crmClientFixture)
    {
        _dataScope = crmClientFixture.CreateTestDataScope();
        _crmQueryDispatcher = crmClientFixture.CreateQueryDispatcher();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _dataScope.DisposeAsync();

    [Fact]
    public async Task WhenCalled_WithContactIdForNonExistentContact_ReturnsResultWithEmptyProperties()
    {
        // Arrange
        var nonExistentContactId = Guid.NewGuid();

        // Act
        var result = await _crmQueryDispatcher.ExecuteQuery(new GetNotesByContactIdQuery(nonExistentContactId));

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Annotations);
        Assert.Empty(result.Tasks);
        Assert.Empty(result.IncidentResolutions);
    }

    [Theory]
    [InlineData(false, false, false)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(false, false, true)]
    [InlineData(true, true, false)]
    [InlineData(true, false, true)]
    [InlineData(false, true, true)]
    [InlineData(true, true, true)]
    public async Task WhenCalled_WithContactIdForContactWithNoNotes_ReturnsResultWithNotes(bool hasAnnotations, bool hasTasks, bool hasIncidentResolutions)
    {
        // Arrange
        var createPersonResult = await _dataScope.TestData.CreatePerson();
        if (hasAnnotations)
        {
            await _dataScope.TestData.CreateNote(b => b.WithPersonId(createPersonResult.ContactId));
            await _dataScope.TestData.CreateNote(b => b.WithPersonId(createPersonResult.ContactId));
        }

        if (hasTasks)
        {
            await _dataScope.TestData.CreateCrmTask(b => b.WithPersonId(createPersonResult.ContactId));
            await _dataScope.TestData.CreateCrmTask(b => b.WithPersonId(createPersonResult.ContactId));
        }

        if (hasIncidentResolutions)
        {
            await _dataScope.TestData.CreateNameChangeIncident(b => b.WithCustomerId(createPersonResult.ContactId).WithRejectedStatus());
            await _dataScope.TestData.CreateDateOfBirthChangeIncident(b => b.WithCustomerId(createPersonResult.ContactId).WithApprovedStatus());
        }

        // Act
        var result = await _crmQueryDispatcher.ExecuteQuery(new GetNotesByContactIdQuery(createPersonResult.ContactId));

        // Assert
        Assert.NotNull(result);
        if (hasAnnotations)
        {
            Assert.Equal(2, result.Annotations.Length);
        }
        else
        {
            Assert.Empty(result.Annotations);
        }

        if (hasTasks)
        {
            Assert.Equal(2, result.Tasks.Length);
        }
        else
        {
            Assert.Empty(result.Tasks);
        }

        if (hasIncidentResolutions)
        {
            Assert.Equal(2, result.IncidentResolutions!.Length);
        }
        else
        {
            Assert.Empty(result.IncidentResolutions);
        }
    }
}
