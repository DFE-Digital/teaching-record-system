namespace TeachingRecordSystem.Core.Dqt.CrmIntegrationTests.QueryTests;

public class UpdateMandatoryQualificationSpecialismTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly CrmQueryDispatcher _crmQueryDispatcher;

    public UpdateMandatoryQualificationSpecialismTests(CrmClientFixture crmClientFixture)
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
        var originalSpecialism = MandatoryQualificationSpecialism.Visual;
        var newSpecialism = await _dataScope.TestData.ReferenceDataCache.GetMqSpecialismByValue(MandatoryQualificationSpecialism.Hearing.GetDqtValue());

        var person = await _dataScope.TestData.CreatePerson(x => x
            .WithMandatoryQualification(q => q.WithSpecialism(originalSpecialism)));

        var qualification = person.MandatoryQualifications.First();

        // Act
        await _crmQueryDispatcher.ExecuteQuery(
            new UpdateMandatoryQualificationSpecialismQuery(
                qualification.QualificationId,
                newSpecialism.Id,
                DummyEvent.Create()));

        // Assert
        using var ctx = new DqtCrmServiceContext(_dataScope.OrganizationService);

        var updatedQualification = ctx.dfeta_qualificationSet.SingleOrDefault(q => q.GetAttributeValue<Guid>(dfeta_qualification.PrimaryIdAttribute) == qualification.QualificationId);
        Assert.NotNull(updatedQualification);
        Assert.Equal(newSpecialism.Id, updatedQualification.dfeta_MQ_SpecialismId.Id);
    }
}
