namespace TeachingRecordSystem.Core.Dqt.CrmIntegrationTests.QueryTests;

public class GetQualificationsByContactIdTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly CrmQueryDispatcher _crmQueryDispatcher;

    public GetQualificationsByContactIdTests(CrmClientFixture crmClientFixture)
    {
        _dataScope = crmClientFixture.CreateTestDataScope();
        _crmQueryDispatcher = crmClientFixture.CreateQueryDispatcher();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _dataScope.DisposeAsync();

    [Fact]
    public async Task WhenCalled_ForContactWithoutQualifications_ReturnsEmptyArray()
    {
        // Arrange
        var person = await _dataScope.TestData.CreatePerson();

        // Act
        var qualifications = await _crmQueryDispatcher.ExecuteQuery(new GetQualificationsByContactIdQuery(person.ContactId));

        // Assert
        Assert.Empty(qualifications);
    }

    [Fact]
    public async Task WhenCalled_ForContactWithQualifications_ReturnsQualificationsAsExpected()
    {
        // Arrange
        var person = await _dataScope.TestData.CreatePerson(x => x
            .WithQts(qtsDate: new DateOnly(2021, 10, 5), "213", new DateTime(2021, 10, 5))
            .WithMandatoryQualification()
            .WithMandatoryQualification(q => q.WithDqtMqEstablishmentValue("959").WithSpecialism(MandatoryQualificationSpecialism.Visual)));

        // Act
        var qualifications = await _crmQueryDispatcher.ExecuteQuery(new GetQualificationsByContactIdQuery(person.ContactId));

        // Assert
        Assert.Equal(2, qualifications.Length);
    }
}
