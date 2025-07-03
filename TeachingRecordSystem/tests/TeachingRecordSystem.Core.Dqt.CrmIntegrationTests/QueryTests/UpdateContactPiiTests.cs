namespace TeachingRecordSystem.Core.Dqt.CrmIntegrationTests.QueryTests;

public class UpdateContactPiiTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly CrmQueryDispatcher _crmQueryDispatcher;

    public UpdateContactPiiTests(CrmClientFixture crmClientFixture)
    {
        _dataScope = crmClientFixture.CreateTestDataScope();
        _crmQueryDispatcher = _dataScope.CreateQueryDispatcher();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _dataScope.DisposeAsync();

    [Fact]
    public async Task QueryExecutesSuccessfully()
    {
        // Arrange
        var person = await _dataScope.TestData.CreatePersonAsync(x =>
        {
            x.WithNationalInsuranceNumber();
            x.WithGender(Gender.Male);
        });

        var newFirstName = _dataScope.TestData.GenerateChangedFirstName(person.FirstName);
        var newMiddleName = _dataScope.TestData.GenerateChangedMiddleName(person.MiddleName);
        var newLastName = _dataScope.TestData.GenerateChangedLastName(person.LastName);
        var newNationalInsuranceNumber = _dataScope.TestData.GenerateChangedNationalInsuranceNumber(person.NationalInsuranceNumber!);
        var newGender = Contact_GenderCode.Female;
        var newEmailAddress = _dataScope.TestData.GenerateUniqueEmail();
        var newDob = _dataScope.TestData.GenerateDateOfBirth();

        var query = new UpdateContactPiiQuery(
            ContactId: person.ContactId,
            FirstName: newFirstName,
            MiddleName: newMiddleName,
            LastName: newLastName,
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
        Assert.Equal(newFirstName, updatedContact.FirstName);
        Assert.Equal(newMiddleName, updatedContact.MiddleName);
        Assert.Equal(newLastName, updatedContact.LastName);
        Assert.Equal(newDob, updatedContact.BirthDate.ToDateOnlyWithDqtBstFix(isLocalTime: false));
        Assert.Equal(newNationalInsuranceNumber, updatedContact.dfeta_NINumber);
        Assert.Equal(newGender, updatedContact.GenderCode);
        Assert.Equal(newEmailAddress, updatedContact.EMailAddress1);
    }
}
