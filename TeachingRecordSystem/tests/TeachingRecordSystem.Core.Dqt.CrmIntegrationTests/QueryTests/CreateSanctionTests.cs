namespace TeachingRecordSystem.Core.Dqt.CrmIntegrationTests.QueryTests;

public class CreateSanctionTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly CrmQueryDispatcher _crmQueryDispatcher;

    public CreateSanctionTests(CrmClientFixture crmClientFixture)
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
        var sanctionCode = "G1";
        var sanctionCodeId = (await _dataScope.TestData.ReferenceDataCache.GetSanctionCodeByValue(sanctionCode)).dfeta_sanctioncodeId;
        var startDate = new DateOnly(2020, 01, 01);
        var createPersonResult = await _dataScope.TestData.CreatePerson();

        // Act
        var sanctionId = await _crmQueryDispatcher.ExecuteQuery(new CreateSanctionQuery()
        {
            ContactId = createPersonResult.PersonId,
            SanctionCodeId = sanctionCodeId!.Value,
            Details = "These are some test details",
            Link = "http://www.gov.uk",
            StartDate = startDate,
        });

        // Assert
        using var ctx = new DqtCrmServiceContext(_dataScope.OrganizationService);

        var createdSanction = ctx.dfeta_sanctionSet.SingleOrDefault(s => s.GetAttributeValue<Guid>(dfeta_sanction.PrimaryIdAttribute) == sanctionId);
        Assert.NotNull(createdSanction);
        Assert.Equal(dfeta_sanctionState.Active, createdSanction.StateCode);
        Assert.Equal(createPersonResult.PersonId, createdSanction.dfeta_PersonId.Id);
        Assert.Equal(sanctionCodeId, createdSanction.dfeta_SanctionCodeId.Id);
        Assert.Equal("These are some test details", createdSanction.dfeta_SanctionDetails);
        Assert.Equal("http://www.gov.uk", createdSanction.dfeta_DetailsLink);
        Assert.Equal(startDate.FromDateOnlyWithDqtBstFix(isLocalTime: true), createdSanction.dfeta_StartDate);
    }
}
