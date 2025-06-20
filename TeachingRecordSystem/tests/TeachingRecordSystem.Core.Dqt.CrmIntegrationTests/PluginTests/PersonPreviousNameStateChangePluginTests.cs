using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.TestCommon;

namespace TeachingRecordSystem.Core.Dqt.CrmIntegrationTests.PluginTests;

public class PersonPreviousNameStateChangePluginTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly CrmQueryDispatcher _crmQueryDispatcher;
    private DbFixture DbFixture;

    public PersonPreviousNameStateChangePluginTests(CrmClientFixture crmClientFixture, DbFixture fixture)
    {
        _dataScope = crmClientFixture.CreateTestDataScope();
        _crmQueryDispatcher = crmClientFixture.CreateQueryDispatcher();
        DbFixture = fixture;
    }

    public async Task DisposeAsync() => await _dataScope.DisposeAsync();

    public Task InitializeAsync() => DbFixture.DbHelper.EnsureSchemaAsync();

    [Fact]
    public async Task Put_MutipleUpdatesContactLastName_PluginUpdatesPreviousName()
    {
        // Arrange
        var person = await _dataScope.TestData.CreatePersonAsync();
        var newFirstName = _dataScope.TestData.GenerateChangedFirstName(person.FirstName);
        var newMiddleName = _dataScope.TestData.GenerateChangedMiddleName(person.MiddleName);
        var newNationalInsuranceNumber = _dataScope.TestData.GenerateChangedNationalInsuranceNumber(person.NationalInsuranceNumber!);
        var newGender = Contact_GenderCode.Female;
        var newEmailAddress = _dataScope.TestData.GenerateUniqueEmail();
        var newDob = _dataScope.TestData.GenerateDateOfBirth();

        var updatedLastName1 = _dataScope.TestData.GenerateChangedLastName(person.LastName);
        var updatedLastName2 = _dataScope.TestData.GenerateChangedLastName(person.LastName);
        var updatedLastName3 = _dataScope.TestData.GenerateChangedLastName(person.LastName);
        var updates = new List<(string updatedLastName, string expectedPreviousName)>()
        {
            (updatedLastName1, person.LastName),
            (updatedLastName2, updatedLastName1),
            (updatedLastName3, updatedLastName2),
        };


        foreach (var update in updates)
        {
            var query = new UpdateContactPiiQuery(
                ContactId: person.ContactId,
                FirstName: newFirstName,
                MiddleName: newMiddleName,
                LastName: update.updatedLastName,
                DateOfBirth: newDob,
                NationalInsuranceNumber: newNationalInsuranceNumber,
                Gender: newGender,
                EmailAddress: newEmailAddress
            );

            // Act
            await _crmQueryDispatcher.ExecuteQueryAsync(query);

            // Assert
            using var ctx = new DqtCrmServiceContext(_dataScope.OrganizationService);
            var updatedContact = ctx.ContactSet.SingleOrDefault(c => c.GetAttributeValue<Guid>(Contact.PrimaryIdAttribute) == person.ContactId);
            Assert.NotNull(updatedContact);
            Assert.Equal(update.updatedLastName, updatedContact.LastName);
            Assert.Equal(update.expectedPreviousName, updatedContact.dfeta_PreviousLastName);
        }
    }

    [Fact]
    public async Task DeactivatePreviousNameWithNoPreviousName_PluginClearsPreviousName()
    {
        // Arrange
        var person = await _dataScope.TestData.CreatePersonAsync();
        var newFirstName = _dataScope.TestData.GenerateChangedFirstName(person.FirstName);
        var newMiddleName = _dataScope.TestData.GenerateChangedMiddleName(person.MiddleName);
        var newNationalInsuranceNumber = _dataScope.TestData.GenerateChangedNationalInsuranceNumber(person.NationalInsuranceNumber!);
        var newGender = Contact_GenderCode.Female;
        var newEmailAddress = _dataScope.TestData.GenerateUniqueEmail();
        var newDob = _dataScope.TestData.GenerateDateOfBirth();
        var updatedLastName = _dataScope.TestData.GenerateChangedLastName(person.LastName);
        var query = new UpdateContactPiiQuery(
            ContactId: person.ContactId,
            FirstName: newFirstName,
            MiddleName: newMiddleName,
            LastName: updatedLastName,
            DateOfBirth: newDob,
            NationalInsuranceNumber: newNationalInsuranceNumber,
            Gender: newGender,
            EmailAddress: newEmailAddress
        );

        // Act
        await _crmQueryDispatcher.ExecuteQueryAsync(query);
        using var ctx = new DqtCrmServiceContext(_dataScope.OrganizationService);
        var crmQuery = new QueryByAttribute(dfeta_previousname.EntityLogicalName);
        crmQuery.AddAttributeValue(dfeta_previousname.Fields.dfeta_PersonId, person.ContactId);
        crmQuery.AddAttributeValue(dfeta_previousname.Fields.dfeta_Type, (int)dfeta_NameType.LastName);
        crmQuery.Orders.Add(new OrderExpression("createdon", OrderType.Descending));
        var previousNames = _dataScope.OrganizationService.RetrieveMultiple(crmQuery);

        var d1 = new dfeta_previousname();
        d1.Id = previousNames.Entities[0].Id;
        d1.StateCode = dfeta_previousnameState.Inactive;
        await _dataScope.OrganizationService.UpdateAsync(d1);
        var updatedContact = _dataScope.OrganizationService.Retrieve(Contact.EntityLogicalName, person.ContactId, new ColumnSet() { AllColumns = true }).ToEntity<Contact>();

        //Assert
        Assert.NotNull(updatedContact);
        Assert.Null(updatedContact.dfeta_PreviousLastName);
    }

    [Fact]
    public async Task DeactivatePreviousNameWithAPreviousName_PluginSetsToMostRecentPreviousName()
    {
        // Arrange
        var person = await _dataScope.TestData.CreatePersonAsync();
        var newFirstName = _dataScope.TestData.GenerateChangedFirstName(person.FirstName);
        var newMiddleName = _dataScope.TestData.GenerateChangedMiddleName(person.MiddleName);
        var newNationalInsuranceNumber = _dataScope.TestData.GenerateChangedNationalInsuranceNumber(person.NationalInsuranceNumber!);
        var newGender = Contact_GenderCode.Female;
        var newEmailAddress = _dataScope.TestData.GenerateUniqueEmail();
        var newDob = _dataScope.TestData.GenerateDateOfBirth();
        var updatedLastName1 = _dataScope.TestData.GenerateChangedLastName(person.LastName);
        var updatedLastName2 = _dataScope.TestData.GenerateChangedLastName(person.LastName);
        var query1 = new UpdateContactPiiQuery(
            ContactId: person.ContactId,
            FirstName: newFirstName,
            MiddleName: newMiddleName,
            LastName: updatedLastName1,
            DateOfBirth: newDob,
            NationalInsuranceNumber: newNationalInsuranceNumber,
            Gender: newGender,
            EmailAddress: newEmailAddress
        );
        var query2 = new UpdateContactPiiQuery(
            ContactId: person.ContactId,
            FirstName: newFirstName,
            MiddleName: newMiddleName,
            LastName: updatedLastName2,
            DateOfBirth: newDob,
            NationalInsuranceNumber: newNationalInsuranceNumber,
            Gender: newGender,
            EmailAddress: newEmailAddress
        );


        // Act
        await _crmQueryDispatcher.ExecuteQueryAsync(query1);
        await _crmQueryDispatcher.ExecuteQueryAsync(query2);
        using var ctx = new DqtCrmServiceContext(_dataScope.OrganizationService);
        var crmQuery = new QueryByAttribute(dfeta_previousname.EntityLogicalName);
        crmQuery.AddAttributeValue(dfeta_previousname.Fields.dfeta_PersonId, person.ContactId);
        crmQuery.AddAttributeValue(dfeta_previousname.Fields.dfeta_Type, (int)dfeta_NameType.LastName);
        crmQuery.AddAttributeValue(dfeta_previousname.Fields.StateCode, (int)dfeta_previousnameState.Active);
        crmQuery.Orders.Add(new OrderExpression("createdon", OrderType.Descending));
        var previousNames = _dataScope.OrganizationService.RetrieveMultiple(crmQuery);

        var d1 = new dfeta_previousname();
        d1.Id = previousNames.Entities[0].Id;
        d1.StateCode = dfeta_previousnameState.Inactive;
        await _dataScope.OrganizationService.UpdateAsync(d1);
        var updatedContact = _dataScope.OrganizationService.Retrieve(Contact.EntityLogicalName, person.ContactId, new ColumnSet() { AllColumns = true }).ToEntity<Contact>();

        //Assert
        Assert.NotNull(updatedContact);
        Assert.Equal(query2.LastName, updatedContact.LastName);
        Assert.Equal(person.LastName, updatedContact.dfeta_PreviousLastName);
    }
}
