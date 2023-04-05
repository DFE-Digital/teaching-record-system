#nullable disable
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk.Messages;
using QualifiedTeachersApi.DataStore.Crm;
using QualifiedTeachersApi.DataStore.Crm.Models;
using Xunit;

namespace QualifiedTeachersApi.Tests.DataverseIntegration;

public class GetTeachersByTrnAndDobTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly DataverseAdapter _dataverseAdapter;
    private readonly ITrackedEntityOrganizationService _organizationService;
    private readonly TestableClock _clock;

    public GetTeachersByTrnAndDobTests(CrmClientFixture crmClientFixture)
    {
        _dataScope = crmClientFixture.CreateTestDataScope();
        _dataverseAdapter = _dataScope.CreateDataverseAdapter();
        _organizationService = _dataScope.OrganizationService;
        _clock = crmClientFixture.Clock;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _dataScope.DisposeAsync();

    [Fact]
    public async Task Given_existing_teacher_matches_on_dob_and_trn_return_teacher()
    {
        // Arrange
        var dob = new DateOnly(1980, 01, 01);
        var (_, trn) = await CreateTeacher(dob);

        // Act
        var result = await _dataverseAdapter.GetTeachersByTrnAndDoB(trn, dob, columnNames: new[] { Contact.Fields.BirthDate });

        // Assert
        Assert.NotNull(result);
        Assert.Equal(dob, DateOnly.FromDateTime(result[0].BirthDate.Value));
    }

    [Fact]
    public async Task Given_existing_teacher_matches_only_trn_return_empty_collection()
    {
        // Arrange
        var dob = new DateOnly(1988, 01, 01);
        var (_, trn) = await CreateTeacher(dob);

        // Act
        var result = await _dataverseAdapter.GetTeachersByTrnAndDoB(trn, new DateOnly(2022, 1, 1), columnNames: new[] { Contact.Fields.BirthDate });

        // Assert
        Assert.Empty(result);
    }

    private async Task<(Guid TeacherId, string Trn)> CreateTeacher(DateOnly birthDate)
    {
        var teacherId = Guid.NewGuid();

        var request = new ExecuteTransactionRequest()
        {
            Requests = new Microsoft.Xrm.Sdk.OrganizationRequestCollection()
            {
                new CreateRequest()
                {
                    Target = new Contact()
                    {
                        Id = teacherId,
                        BirthDate = birthDate.ToDateTime()
                    }
                },
                new UpdateRequest()
                {
                    Target = new Contact()
                    {
                        Id = teacherId,
                        dfeta_TRNAllocateRequest = _clock.UtcNow
                    }
                },
                new RetrieveRequest()
                {
                    Target = new Microsoft.Xrm.Sdk.EntityReference(Contact.EntityLogicalName, teacherId),
                    ColumnSet = new(Contact.Fields.dfeta_TRN)
                }
            },
            ReturnResponses = true
        };

        var response = (ExecuteTransactionResponse)await _organizationService.ExecuteAsync(request);
        var retrieveResponse = (RetrieveResponse)response.Responses.Last();

        return (teacherId, retrieveResponse.Entity.ToEntity<Contact>().dfeta_TRN);
    }
}
