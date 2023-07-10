using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Api.DataStore.Crm;
using TeachingRecordSystem.Api.DataStore.Crm.Models;
using Xunit;

namespace TeachingRecordSystem.Api.Tests.DataverseIntegration;

public class GetSanctionsByContactIdsTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly DataverseAdapter _dataverseAdapter;
    private readonly IOrganizationServiceAsync _organizationService;

    public GetSanctionsByContactIdsTests(CrmClientFixture crmClientFixture)
    {
        _dataScope = crmClientFixture.CreateTestDataScope();
        _dataverseAdapter = _dataScope.CreateDataverseAdapter();
        _organizationService = _dataScope.OrganizationService;
    }

    [Fact]
    public async Task ReturnsSanctionsForEachContactIdSpecified()
    {
        // Arrange
        var sanctionCodeTypes = await GetSanctionCodes();

        // Create a single sanction for contact 1, two for contact 2 and none for contact 3
        var contact1Id = await CreateContact("G1");
        var contact2Id = await CreateContact("A1", "G1");
        var contact3Id = await CreateContact();

        // Act
        var sanctionCodesByContactId = await _dataverseAdapter.GetSanctionsByContactIds(new[] { contact1Id, contact2Id, contact3Id });

        // Assert
        Assert.Equal(3, sanctionCodesByContactId.Count);
        Assert.Collection(sanctionCodesByContactId[contact1Id], c => Assert.Equal("G1", c));
        Assert.Collection(sanctionCodesByContactId[contact2Id], c => Assert.Equal("A1", c), c => Assert.Equal("G1", c));
        Assert.Empty(sanctionCodesByContactId[contact3Id]);

        async Task<Guid> CreateContact(params string[] sanctionCodes)
        {
            var requestBuilder = _dataverseAdapter.CreateMultipleRequestBuilder();
            var contactId = Guid.NewGuid();

            requestBuilder.AddRequest(new CreateRequest()
            {
                Target = new Contact()
                {
                    Id = contactId,
                    FirstName = Faker.Name.First(),
                    LastName = Faker.Name.Last()
                }
            });

            foreach (var sanctionCode in sanctionCodes)
            {
                var sanctionCodeId = sanctionCodeTypes.First(s => s.SanctionCode == sanctionCode).SanctionCodeId;

                requestBuilder.AddRequest(new CreateRequest()
                {
                    Target = new dfeta_sanction()
                    {
                        dfeta_PersonId = new Microsoft.Xrm.Sdk.EntityReference(Contact.EntityLogicalName, contactId),
                        dfeta_SanctionCodeId = new Microsoft.Xrm.Sdk.EntityReference(dfeta_sanctioncode.EntityLogicalName, sanctionCodeId),
                    }
                });
            }

            await requestBuilder.Execute();

            return contactId;
        }

        async Task<(string SanctionCode, Guid SanctionCodeId)[]> GetSanctionCodes()
        {
            var query = new QueryExpression(dfeta_sanctioncode.EntityLogicalName)
            {
                ColumnSet = new ColumnSet(dfeta_sanctioncode.Fields.dfeta_Value)
            };

            query.Criteria.AddCondition(dfeta_sanctioncode.Fields.StateCode, ConditionOperator.Equal, (int)dfeta_sanctioncodeState.Active);

            var request = new RetrieveMultipleRequest()
            {
                Query = query
            };

            var response = (RetrieveMultipleResponse)await _organizationService.ExecuteAsync(request);

            return response.EntityCollection.Entities
                .Select(e => e.ToEntity<dfeta_sanctioncode>())
                .Select(sc => (sc.dfeta_Value, sc.Id))
                .ToArray();
        }
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _dataScope.DisposeAsync();
}
