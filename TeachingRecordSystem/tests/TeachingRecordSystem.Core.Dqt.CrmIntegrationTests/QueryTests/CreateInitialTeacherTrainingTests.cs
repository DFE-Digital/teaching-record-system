namespace TeachingRecordSystem.Core.Dqt.CrmIntegrationTests.QueryTests;

public class CreateInitialTeacherTrainingTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly CrmQueryDispatcher _crmQueryDispatcher;
    private readonly CrmClientFixture _fixture;

    public CreateInitialTeacherTrainingTests(CrmClientFixture crmClientFixture)
    {
        _fixture = crmClientFixture;
        _dataScope = crmClientFixture.CreateTestDataScope();
        _crmQueryDispatcher = _dataScope.CreateQueryDispatcher();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _dataScope.DisposeAsync();

    [Fact]
    public async Task QueryExecutesSuccessfully()
    {
        // Arrange
        using var txn = _crmQueryDispatcher.CreateTransactionRequestBuilder();
        var countries = await _crmQueryDispatcher.ExecuteQueryAsync(new GetAllCountriesQuery());
        var country = countries.FirstOrDefault(x => x.dfeta_Value == "XK");
        var qualificationId = Guid.NewGuid();
        var ittId = Guid.NewGuid();
        var contact = await _dataScope.TestData.CreatePersonAsync(x =>
        {
            x.WithTrn();
            x.WithQts(new DateOnly(2024, 01, 01));
            x.WithQualification(qualificationId, dfeta_qualification_dfeta_Type.NPQH);
        });
        var query = new CreateInitialTeacherTrainingTransactionalQuery()
        {
            Id = ittId,
            ContactId = contact.PersonId,
            ITTQualificationId = null,
            CountryId = country!.Id,
            Result = dfeta_ITTResult.Pass
        };
        txn.AppendQuery(query);

        // Act
        await txn.ExecuteAsync();

        // Assert
        using var ctx = new DqtCrmServiceContext(_dataScope.OrganizationService);
        var itt = ctx.dfeta_initialteachertrainingSet.SingleOrDefault(i => i.GetAttributeValue<Guid>(dfeta_initialteachertraining.PrimaryIdAttribute) == ittId);
        Assert.NotNull(itt);
        Assert.Equal(dfeta_ITTResult.Pass, itt.dfeta_Result);
    }
}
