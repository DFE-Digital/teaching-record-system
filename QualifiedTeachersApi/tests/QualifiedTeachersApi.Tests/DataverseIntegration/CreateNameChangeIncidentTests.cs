﻿#nullable disable
using System;
using System.IO;
using System.Linq;
using System.Text;
using QualifiedTeachersApi.DataStore.Crm;
using QualifiedTeachersApi.DataStore.Crm.Models;
using Xunit;

namespace QualifiedTeachersApi.Tests.DataverseIntegration;

public class CreateNameChangeIncidentTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly DataverseAdapter _dataverseAdapter;

    public CreateNameChangeIncidentTests(CrmClientFixture crmClientFixture)
    {
        _dataScope = crmClientFixture.CreateTestDataScope();
        _dataverseAdapter = _dataScope.CreateDataverseAdapter();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _dataScope.DisposeAsync();

    [Fact]
    public async Task CreateNameChangeIncident_ExecutesSuccessfully()
    {
        // Arrange
        var createPersonResult = await _dataScope.CreateTestDataHelper().CreatePerson();

        var newFirstName = Faker.Name.First();
        var newMiddleName = Faker.Name.Middle();
        var newLastName = Faker.Name.Last();
        var evidenceFileName = "evidence.txt";
        var evidenceFileContent = new MemoryStream(Encoding.UTF8.GetBytes("Test file"));
        var evidenceFileMimeType = "text/plain";

        var command = new CreateNameChangeIncidentCommand()
        {
            ContactId = createPersonResult.TeacherId,
            Trn = createPersonResult.Trn,
            FirstName = newFirstName,
            MiddleName = newMiddleName,
            LastName = newLastName,
            EvidenceFileName = evidenceFileName,
            EvidenceFileContent = evidenceFileContent,
            EvidenceFileMimeType = evidenceFileMimeType
        };

        // Act
        var incidentId = await _dataverseAdapter.CreateNameChangeIncident(command);

        // Assert
        using var ctx = new DqtCrmServiceContext(_dataScope.OrganizationService);

        var createdIncident = ctx.IncidentSet.SingleOrDefault(i => i.GetAttributeValue<Guid>(Incident.PrimaryIdAttribute) == incidentId);
        Assert.Equal(createPersonResult.TeacherId, createdIncident.CustomerId.Id);
        Assert.Equal("Request to change name", createdIncident.Title);
        Assert.Equal(newFirstName, createdIncident.dfeta_NewFirstName);
        Assert.Equal(newMiddleName, createdIncident.dfeta_NewMiddleName);
        Assert.Equal(newLastName, createdIncident.dfeta_NewLastName);
    }
}
