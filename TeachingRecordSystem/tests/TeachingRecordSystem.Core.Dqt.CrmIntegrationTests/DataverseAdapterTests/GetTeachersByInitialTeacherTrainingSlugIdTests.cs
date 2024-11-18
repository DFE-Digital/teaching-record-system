using Microsoft.Xrm.Sdk;

namespace TeachingRecordSystem.Core.Dqt.CrmIntegrationTests.DataverseAdapterTests;

public class GetTeachersByInitialTeacherTrainingSlugIdTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly DataverseAdapter _dataverseAdapter;
    private readonly ITrackedEntityOrganizationService _organizationService;
    private readonly IClock _clock;

    public GetTeachersByInitialTeacherTrainingSlugIdTests(CrmClientFixture crmClientFixture)
    {
        _dataScope = crmClientFixture.CreateTestDataScope();
        _dataverseAdapter = _dataScope.CreateDataverseAdapter();
        _organizationService = _dataScope.OrganizationService;
        _clock = crmClientFixture.Clock;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _dataScope.DisposeAsync();


    [Fact]
    public async Task Given_no_itt_records_exist_with_slugid_return_empty_collection()
    {
        // Arrange
        var slugId = Guid.NewGuid().ToString();

        // Act
        var result = await _dataverseAdapter.GetTeachersByInitialTeacherTrainingSlugIdAsync(slugId, columnNames: new[] { Contact.Fields.dfeta_TRN }, null);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task Given_multiple_itt_records_exist_with_slugid_return_associated_teachers()
    {
        // Arrange
        var slugId = Guid.NewGuid().ToString();
        var teacher1Id = await _organizationService.CreateAsync(new Contact() { FirstName = "test" });
        await _organizationService.CreateAsync(new dfeta_initialteachertraining() { dfeta_PersonId = new EntityReference(Contact.EntityLogicalName, teacher1Id), dfeta_SlugId = slugId });
        var teacher2Id = await _organizationService.CreateAsync(new Contact() { FirstName = "testing" });
        await _organizationService.CreateAsync(new dfeta_initialteachertraining() { dfeta_PersonId = new EntityReference(Contact.EntityLogicalName, teacher2Id), dfeta_SlugId = slugId });

        // Act
        var result = await _dataverseAdapter.GetTeachersByInitialTeacherTrainingSlugIdAsync(slugId, columnNames: new[] { Contact.Fields.dfeta_TRN }, null);

        // Assert
        Assert.NotEmpty(result);
        Assert.Contains(result, item => item.Id == teacher1Id);
        Assert.Contains(result, item => item.Id == teacher2Id);
    }
}
