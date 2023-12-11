namespace TeachingRecordSystem.Core.Dqt.CrmIntegrationTests.QueryTests;

public class CreateMandatoryQualificationTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly CrmQueryDispatcher _crmQueryDispatcher;

    public CreateMandatoryQualificationTests(CrmClientFixture crmClientFixture)
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
        var person = await _dataScope.TestData.CreatePerson(b => b.WithQts(qtsDate: new DateOnly(2021, 10, 5)));
        var mqEstablishment = await _dataScope.TestData.ReferenceDataCache.GetMqEstablishmentByValue("955"); // University of Birmingham
        var specialism = await _dataScope.TestData.ReferenceDataCache.GetMqSpecialismByValue("Hearing");
        var startDate = new DateOnly(2023, 01, 5);
        var endDate = new DateOnly(2023, 07, 10);

        var query = new CreateMandatoryQualificationQuery()
        {
            ContactId = person.ContactId,
            MqEstablishmentId = mqEstablishment.Id,
            SpecialismId = specialism.Id,
            StartDate = new DateOnly(2023, 01, 5),
            Result = dfeta_qualification_dfeta_MQ_Status.Passed,
            EndDate = new DateOnly(2023, 07, 10),
        };

        // Act
        var qualificationId = await _crmQueryDispatcher.ExecuteQuery(query);

        // Assert
        using var ctx = new DqtCrmServiceContext(_dataScope.OrganizationService);

        var createdQualification = ctx.dfeta_qualificationSet.SingleOrDefault(q => q.GetAttributeValue<Guid>(dfeta_qualification.PrimaryIdAttribute) == qualificationId);
        Assert.NotNull(createdQualification);
        Assert.Equal(person.ContactId, createdQualification.dfeta_PersonId.Id);
        Assert.Equal(mqEstablishment.Id, createdQualification.dfeta_MQ_MQEstablishmentId.Id);
        Assert.Equal(specialism.Id, createdQualification.dfeta_MQ_SpecialismId.Id);
        Assert.Equal(startDate.FromDateOnlyWithDqtBstFix(isLocalTime: true), createdQualification.dfeta_MQStartDate);
        Assert.Equal(dfeta_qualification_dfeta_MQ_Status.Passed, createdQualification.dfeta_MQ_Status);
        Assert.Equal(endDate.FromDateOnlyWithDqtBstFix(isLocalTime: true), createdQualification.dfeta_MQ_Date);
    }
}
