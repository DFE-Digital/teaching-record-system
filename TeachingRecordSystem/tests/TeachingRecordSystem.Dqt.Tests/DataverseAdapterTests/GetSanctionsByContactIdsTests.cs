using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;

namespace TeachingRecordSystem.Dqt.Tests.DataverseAdapterTests;

public class GetSanctionsByContactIdsTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly DataverseAdapter _dataverseAdapter;
    private readonly IOrganizationServiceAsync _organizationService;

    private (string SanctionCode, Guid SanctionCodeId)[]? _sanctionCodeTypes;

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
        // Create a single sanction for contact 1, two for contact 2 and none for contact 3
        var contact1Id = await CreateContact((SanctionCode: "G1", Spent: false, EndDate: null));
        var contact2Id = await CreateContact((SanctionCode: "A1", Spent: false, EndDate: null), (SanctionCode: "G1", Spent: false, EndDate: null));
        var contact3Id = await CreateContact();

        // Act
        var sanctionCodesByContactId = await _dataverseAdapter.GetSanctionsByContactIds(new[] { contact1Id, contact2Id, contact3Id });

        // Assert
        Assert.Equal(3, sanctionCodesByContactId.Count());
        Assert.Collection(sanctionCodesByContactId[contact1Id], c => Assert.Equal("G1", c));
        Assert.Collection(sanctionCodesByContactId[contact2Id], c => Assert.Equal("A1", c), c => Assert.Equal("G1", c));
        Assert.Empty(sanctionCodesByContactId[contact3Id]);
    }

    [Theory]
    [InlineData(true, true, false)]
    [InlineData(false, true, true)]
    [InlineData(true, false, true)]
    [InlineData(false, false, true)]
    public async Task SpentSanctionIsReturnedAsExpected(bool spent, bool liveOnly, bool expectSanctionReturned)
    {
        // Arrange
        var contactId = await CreateContact((SanctionCode: "G1", Spent: spent, EndDate: null));

        // Act
        var sanctionCodesByContactId = await _dataverseAdapter.GetSanctionsByContactIds(new[] { contactId }, liveOnly);

        // Assert
        if (expectSanctionReturned)
        {
            Assert.Collection(sanctionCodesByContactId[contactId], c => Assert.Equal("G1", c));
        }
        else
        {
            Assert.Empty(sanctionCodesByContactId[contactId]);
        }
    }

    [Theory]
    [InlineData(true, true, false)]
    [InlineData(false, true, true)]
    [InlineData(true, false, true)]
    [InlineData(false, false, true)]
    public async Task EndedSanctionIsReturnedAsExpected(bool ended, bool liveOnly, bool expectSanctionReturned)
    {
        // Arrange
        var contactId = await CreateContact((SanctionCode: "G1", Spent: false, EndDate: ended ? DateTime.Today.AddDays(-1) : null));

        // Act
        var sanctionCodesByContactId = await _dataverseAdapter.GetSanctionsByContactIds(new[] { contactId }, liveOnly);

        // Assert
        if (expectSanctionReturned)
        {
            Assert.Collection(sanctionCodesByContactId[contactId], c => Assert.Equal("G1", c));
        }
        else
        {
            Assert.Empty(sanctionCodesByContactId[contactId]);
        }
    }

    public async Task InitializeAsync() => _sanctionCodeTypes = await GetSanctionCodes();

    public async Task DisposeAsync() => await _dataScope.DisposeAsync();

    private async Task<Guid> CreateContact(params (string SanctionCode, bool Spent, DateTime? EndDate)[] sanctions)
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

        foreach (var sanction in sanctions)
        {
            var sanctionCodeId = _sanctionCodeTypes!.First(s => s.SanctionCode == sanction.SanctionCode).SanctionCodeId;

            requestBuilder.AddRequest(new CreateRequest()
            {
                Target = new dfeta_sanction()
                {
                    dfeta_PersonId = new Microsoft.Xrm.Sdk.EntityReference(Contact.EntityLogicalName, contactId),
                    dfeta_SanctionCodeId = new Microsoft.Xrm.Sdk.EntityReference(dfeta_sanctioncode.EntityLogicalName, sanctionCodeId),
                    dfeta_Spent = sanction.Spent,
                    dfeta_EndDate = sanction.EndDate
                }
            });
        }

        await requestBuilder.Execute();

        return contactId;
    }

    private async Task<(string SanctionCode, Guid SanctionCodeId)[]> GetSanctionCodes()
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
