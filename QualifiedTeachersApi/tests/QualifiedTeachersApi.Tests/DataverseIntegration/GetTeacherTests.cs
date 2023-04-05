#nullable disable
using System;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using QualifiedTeachersApi.DataStore.Crm;
using QualifiedTeachersApi.DataStore.Crm.Models;
using Xunit;

namespace QualifiedTeachersApi.Tests.DataverseIntegration;

public class GetTeacherTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly DataverseAdapter _dataverseAdapter;
    private readonly IOrganizationServiceAsync _organizationService;

    public GetTeacherTests(CrmClientFixture crmClientFixture)
    {
        _dataScope = crmClientFixture.CreateTestDataScope();
        _dataverseAdapter = _dataScope.CreateDataverseAdapter();
        _organizationService = _dataScope.OrganizationService;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _dataScope.DisposeAsync();

    [Fact]
    public async Task Given_teacher_that_does_not_exist_returns_null()
    {
        // Arrange
        var teacherId = Guid.NewGuid();

        // Act
        var result = await _dataverseAdapter.GetTeacher(teacherId, new[] { Contact.Fields.StateCode }, resolveMerges: false);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Given_teacher_that_exists_returns_teacher()
    {
        // Arrange
        var teacherId = await _organizationService.CreateAsync(new Contact());

        // Act
        var result = await _dataverseAdapter.GetTeacher(teacherId, new[] { Contact.Fields.StateCode }, resolveMerges: false);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(teacherId, result.Id);
    }

    [Fact]
    public async Task Given_merged_teacher_and_resolveMerges_true_return_master()
    {
        // Arrange
        var firstName = "Joe";
        var masterTeacherId = await _organizationService.CreateAsync(new Contact() { FirstName = firstName });
        var teacherId = await _organizationService.CreateAsync(new Contact() { FirstName = firstName });

        await _organizationService.ExecuteAsync(new MergeRequest()
        {
            Target = new EntityReference(Contact.EntityLogicalName, masterTeacherId),
            SubordinateId = teacherId,
            PerformParentingChecks = false,
            UpdateContent = new Entity(Contact.EntityLogicalName)
        });

        // Act
        var result = await _dataverseAdapter.GetTeacher(teacherId, new[] { Contact.Fields.FirstName }, resolveMerges: true);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(masterTeacherId, result.Id);
        Assert.Equal(firstName, result.FirstName);
    }

    [Fact]
    public async Task Given_merged_teacher_and_resolveMerges_false_return_merged_record()
    {
        // Arrange
        var masterTeacherId = await _organizationService.CreateAsync(new Contact());
        var teacherId = await _organizationService.CreateAsync(new Contact());

        await _organizationService.ExecuteAsync(new MergeRequest()
        {
            Target = new EntityReference(Contact.EntityLogicalName, masterTeacherId),
            SubordinateId = teacherId,
            PerformParentingChecks = false,
            UpdateContent = new Entity(Contact.EntityLogicalName)
        });

        // Act
        var result = await _dataverseAdapter.GetTeacher(teacherId, new[] { Contact.Fields.StateCode }, resolveMerges: false);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(teacherId, result.Id);
        Assert.Equal(ContactState.Inactive, result.StateCode);
    }
}
