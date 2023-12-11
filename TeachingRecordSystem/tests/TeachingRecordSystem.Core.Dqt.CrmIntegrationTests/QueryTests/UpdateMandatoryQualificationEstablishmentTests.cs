namespace TeachingRecordSystem.Core.Dqt.CrmIntegrationTests.QueryTests;

public class UpdateMandatoryQualificationEstablishmentTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly CrmQueryDispatcher _crmQueryDispatcher;

    public UpdateMandatoryQualificationEstablishmentTests(CrmClientFixture crmClientFixture)
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
        var originalMqEstablishmentValue = "955";
        var newMqEstablishment = await _dataScope.TestData.ReferenceDataCache.GetMqEstablishmentByValue("959"); // University of Leeds

        var person = await _dataScope.TestData.CreatePerson(
            x => x.WithQts(qtsDate: new DateOnly(2021, 10, 5))
                    .WithMandatoryQualification(providerValue: originalMqEstablishmentValue));

        var qualification = person.MandatoryQualifications.First();

        // Act
        await _crmQueryDispatcher.ExecuteQuery(
            new UpdateMandatoryQualificationEstablishmentQuery(
                qualification.QualificationId,
                newMqEstablishment.Id));

        // Assert
        using var ctx = new DqtCrmServiceContext(_dataScope.OrganizationService);

        var updatedQualification = ctx.dfeta_qualificationSet.SingleOrDefault(q => q.GetAttributeValue<Guid>(dfeta_qualification.PrimaryIdAttribute) == qualification.QualificationId);
        Assert.NotNull(updatedQualification);
        Assert.Equal(newMqEstablishment.Id, updatedQualification.dfeta_MQ_MQEstablishmentId.Id);
    }
}
