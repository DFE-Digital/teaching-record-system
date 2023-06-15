#nullable disable
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Api.DataStore.Crm;
using TeachingRecordSystem.Api.DataStore.Crm.Models;
using Xunit;

namespace TeachingRecordSystem.Api.Tests.DataverseIntegration;

public class UnlockTeacherRecordTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly DataverseAdapter _dataverseAdapter;
    private readonly IOrganizationServiceAsync _organizationService;

    public UnlockTeacherRecordTests(CrmClientFixture crmClientFixture)
    {
        _dataScope = crmClientFixture.CreateTestDataScope();
        _dataverseAdapter = _dataScope.CreateDataverseAdapter();
        _organizationService = _dataScope.OrganizationService;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _dataScope.DisposeAsync();

    [Fact]
    public async Task Given_an_id_that_does_not_exist_returns_false()
    {
        // Arrange
        var teacherId = Guid.NewGuid();

        // Act
        var result = await _dataverseAdapter.UnlockTeacherRecord(teacherId);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(5)]
    public async Task Given_a_valid_id_sets_loginfailedcounter_to_0_and_returns_true(int? initialLoginFailedCounter)
    {
        // Arrange
        var teacherId = await _organizationService.CreateAsync(new Contact()
        {
            dfeta_loginfailedcounter = initialLoginFailedCounter
        });

        // Act
        var result = await _dataverseAdapter.UnlockTeacherRecord(teacherId);

        // Assert
        Assert.True(result);

        var teacher = (await _organizationService.RetrieveAsync(Contact.EntityLogicalName, teacherId, new ColumnSet() { AllColumns = true })).ToEntity<Contact>();
        Assert.Equal(0, teacher.dfeta_loginfailedcounter);
    }
}
