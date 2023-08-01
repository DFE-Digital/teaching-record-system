#nullable disable
using Microsoft.Xrm.Sdk.Messages;

namespace TeachingRecordSystem.Core.Dqt.Tests.DataverseAdapterTests;

public class GetTeacherByHusIdTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly DataverseAdapter _dataverseAdapter;
    private readonly ITrackedEntityOrganizationService _organizationService;
    private readonly TestableClock _clock;

    public GetTeacherByHusIdTests(CrmClientFixture crmClientFixture)
    {
        _dataScope = crmClientFixture.CreateTestDataScope();
        _dataverseAdapter = _dataScope.CreateDataverseAdapter();
        _organizationService = _dataScope.OrganizationService;
        _clock = crmClientFixture.Clock;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _dataScope.DisposeAsync();


    [Fact]
    public async Task Given_existing_teacher_matches_on_husid_return_teacher()
    {
        // Arrange
        var husId = new Random().NextInt64(2000000000000, 2999999999999).ToString();
        var (_, _) = await CreateTeacher(husId);

        // Act
        var result = await _dataverseAdapter.GetTeachersByHusId(husId, columnNames: new[] { Contact.Fields.dfeta_HUSID });

        // Assert
        Assert.NotEmpty(result);
        Assert.Equal(husId, result[0].dfeta_HUSID);
    }

    [Fact]
    public async Task Given_teacher_for_husid_does_not_exist_return_empty_array()
    {
        // Arrange
        var husId = new Random().NextInt64(2000000000000, 2999999999999).ToString();
        var (_, _) = await CreateTeacher(husId);

        // Act
        var result = await _dataverseAdapter.GetTeachersByHusId("SOME_NONE_EXISTENT_HUSID", columnNames: new[] { Contact.Fields.dfeta_HUSID });

        // Assert
        Assert.Empty(result);
    }

    private async Task<(Guid TeacherId, string Trn)> CreateTeacher(string husId)
    {
        var teacherId = Guid.NewGuid();
        var birthDate = new DateOnly(1980, 01, 01);

        var request = new ExecuteTransactionRequest()
        {
            Requests = new Microsoft.Xrm.Sdk.OrganizationRequestCollection()
            {
                new CreateRequest()
                {
                    Target = new Contact()
                    {
                        Id = teacherId,
                        BirthDate = birthDate.ToDateTime(),
                        dfeta_HUSID = husId
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
