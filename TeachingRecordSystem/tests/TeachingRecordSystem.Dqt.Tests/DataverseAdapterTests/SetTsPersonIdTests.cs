#nullable disable
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Query;

namespace TeachingRecordSystem.Dqt.Tests.DataverseAdapterTests;

public class SetTsPersonIdTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly DataverseAdapter _dataverseAdapter;
    private readonly IOrganizationServiceAsync _organizationService;
    private readonly TestDataHelper _testDataHelper;
    private readonly TestableClock _clock;

    public SetTsPersonIdTests(CrmClientFixture crmClientFixture)
    {
        _dataScope = crmClientFixture.CreateTestDataScope();
        _dataverseAdapter = _dataScope.CreateDataverseAdapter();
        _organizationService = _dataScope.OrganizationService;
        _testDataHelper = _dataScope.CreateTestDataHelper();
        _clock = crmClientFixture.Clock;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _dataScope.DisposeAsync();

    [Fact]
    public async Task Given_valid_id_updates_tspersonid()
    {
        // Arrange
        var createPersonResult = await _testDataHelper.CreatePerson();
        var tsPersonId = Guid.NewGuid().ToString();

        // Act
        await _dataverseAdapter.SetTsPersonId(createPersonResult.TeacherId, tsPersonId);

        // Assert
        var teacher = (await _organizationService.RetrieveAsync(
            Contact.EntityLogicalName,
            createPersonResult.TeacherId,
            new ColumnSet(Contact.Fields.dfeta_TSPersonID))).ToEntity<Contact>();

        Assert.Equal(tsPersonId, teacher.dfeta_TSPersonID);
    }
}
