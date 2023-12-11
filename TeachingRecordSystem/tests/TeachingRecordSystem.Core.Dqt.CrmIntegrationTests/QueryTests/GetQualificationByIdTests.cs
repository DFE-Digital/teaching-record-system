namespace TeachingRecordSystem.Core.Dqt.CrmIntegrationTests.QueryTests;

public class GetQualificationByIdTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly CrmQueryDispatcher _crmQueryDispatcher;

    public GetQualificationByIdTests(CrmClientFixture crmClientFixture)
    {
        _dataScope = crmClientFixture.CreateTestDataScope();
        _crmQueryDispatcher = crmClientFixture.CreateQueryDispatcher();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _dataScope.DisposeAsync();

    [Fact]
    public async Task WhenCalled_WithQualificationIdForNonExistentQualification_ReturnsNull()
    {
        // Arrange
        var qualificationId = Guid.NewGuid();

        // Act
        var result = await _crmQueryDispatcher.ExecuteQuery(new GetQualificationByIdQuery(qualificationId));

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task WhenCalled_WithQualificationIdForExistingQualification_ReturnsQualification()
    {
        // Arrange
        var person = await _dataScope.TestData.CreatePerson(
            x => x.WithQts(qtsDate: new DateOnly(2021, 10, 5))
                    .WithMandatoryQualification());

        var qualification = person.MandatoryQualifications.First();

        // Act
        var results = await _crmQueryDispatcher.ExecuteQuery(new GetQualificationByIdQuery(qualification.QualificationId));

        // Assert
        Assert.NotNull(results);
        Assert.Equal(results.dfeta_qualificationId, qualification.QualificationId);
    }
}
