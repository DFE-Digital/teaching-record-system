using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using QualifiedTeachersApi.DataStore.Crm;
using QualifiedTeachersApi.DataStore.Crm.Models;
using Xunit;

namespace QualifiedTeachersApi.Tests.DataverseIntegration;

public class GetInitialTeacherTrainingByTeacherTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly DataverseAdapter _dataverseAdapter;
    private readonly IOrganizationServiceAsync _organizationService;

    public GetInitialTeacherTrainingByTeacherTests(CrmClientFixture crmClientFixture)
    {
        _dataScope = crmClientFixture.CreateTestDataScope();
        _dataverseAdapter = _dataScope.CreateDataverseAdapter();
        _organizationService = _dataScope.OrganizationService;
    }

    [Fact]
    public async Task Given_inactive_itt_record_not_returned_by_default()
    {
        // Arrange
        var firstName = "Joe";
        var teacherId = await _organizationService.CreateAsync(new Contact() { FirstName = firstName });
        var ittId = await _organizationService.CreateAsync(new dfeta_initialteachertraining() { dfeta_PersonId = new EntityReference(Contact.EntityLogicalName, teacherId) });
        await _organizationService.ExecuteAsync(new UpdateRequest()
        {
            Target = new dfeta_initialteachertraining()
            {
                Id = ittId,
                StateCode = dfeta_initialteachertrainingState.Inactive
            }
        });

        // Act
        var result = await _dataverseAdapter.GetTeacher(teacherId, new[] { Contact.Fields.FirstName });
        var ittRecords = await _dataverseAdapter.GetInitialTeacherTrainingByTeacher(teacherId, columnNames: new[]
            {
                dfeta_initialteachertraining.Fields.dfeta_PersonId,
                dfeta_initialteachertraining.Fields.StateCode
            });

        // Assert
        Assert.Empty(ittRecords);
    }

    [Fact]
    public async Task Given_active_and_inactive_itt_records_are_returned_when_passing_includeInactive()
    {
        // Arrange
        var firstName = "Joe";
        var teacherId = await _organizationService.CreateAsync(new Contact() { FirstName = firstName });
        var aciveittId = await _organizationService.CreateAsync(new dfeta_initialteachertraining() { dfeta_PersonId = new EntityReference(Contact.EntityLogicalName, teacherId) });
        var inActiveittId = await _organizationService.CreateAsync(new dfeta_initialteachertraining() { dfeta_PersonId = new EntityReference(Contact.EntityLogicalName, teacherId) });
        await _organizationService.ExecuteAsync(new UpdateRequest()
        {
            Target = new dfeta_initialteachertraining()
            {
                Id = inActiveittId,
                StateCode = dfeta_initialteachertrainingState.Inactive
            }
        });

        // Act
        var result = await _dataverseAdapter.GetTeacher(teacherId, new[] { Contact.Fields.FirstName });
        var ittRecords = await _dataverseAdapter.GetInitialTeacherTrainingByTeacher(teacherId, columnNames: new[]
            {
                dfeta_initialteachertraining.Fields.dfeta_PersonId,
                dfeta_initialteachertraining.Fields.StateCode
            },
            activeOnly: false);

        // Assert
        Assert.Collection(
                    ittRecords,
                    item1 =>
                    {
                        Assert.Equal(aciveittId, item1.Id);
                    },
                    item2 =>
                    {
                        Assert.Equal(inActiveittId, item2.Id);
                    }
                );
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _dataScope.DisposeAsync();
}
