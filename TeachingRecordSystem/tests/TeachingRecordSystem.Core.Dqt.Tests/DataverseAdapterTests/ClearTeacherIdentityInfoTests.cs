using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk.Query;

namespace TeachingRecordSystem.Core.Dqt.Tests.DataverseAdapterTests;

public class ClearTeacherIdentityInfoTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly DataverseAdapter _dataverseAdapter;
    private readonly IOrganizationServiceAsync _organizationService;

    public ClearTeacherIdentityInfoTests(CrmClientFixture crmClientFixture)
    {
        _dataScope = crmClientFixture.CreateTestDataScope();
        _dataverseAdapter = _dataScope.CreateDataverseAdapter();
        _organizationService = _dataScope.OrganizationService;
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _dataScope.DisposeAsync();

    [Fact]
    public async Task ContactExistsWithIdentityUserId_ClearsTsPersonId()
    {
        // Arrange
        var identityUserId = Guid.NewGuid();
        var trn = await _dataverseAdapter.GenerateTrn();
        var updateTimeUtc = DateTime.UtcNow;

        var contactId = await _organizationService.CreateAsync(new Contact()
        {
            dfeta_TSPersonID = identityUserId.ToString(),
            FirstName = Faker.Name.First(),
            LastName = Faker.Name.Last(),
            dfeta_TRN = trn
        });

        // Act
        await _dataverseAdapter.ClearTeacherIdentityInfo(identityUserId, updateTimeUtc);

        // Assert
        var contact = (await _organizationService.RetrieveAsync(
            Contact.EntityLogicalName,
            contactId,
            new ColumnSet(Contact.Fields.dfeta_TSPersonID, Contact.Fields.dfeta_LastIdentityUpdate))).ToEntity<Contact>();

        Assert.Null(contact.dfeta_TSPersonID);
        Assert.Equal(updateTimeUtc, contact.dfeta_LastIdentityUpdate!.Value, precision: TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task NoContactWithTsPersonId_Succeeds()
    {
        // Arrange
        var identityUserId = Guid.NewGuid();
        var updateTimeUtc = DateTime.UtcNow;

        // Act
        await _dataverseAdapter.ClearTeacherIdentityInfo(identityUserId, updateTimeUtc);

        // Assert
    }
}
