namespace TeachingRecordSystem.Core.Dqt.CrmIntegrationTests.QueryTests;

public class CreateContactTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly CrmQueryDispatcher _crmQueryDispatcher;
    private readonly CrmClientFixture _fixture;

    public CreateContactTests(CrmClientFixture crmClientFixture)
    {
        _fixture = crmClientFixture;
        _dataScope = crmClientFixture.CreateTestDataScope();
        _crmQueryDispatcher = _dataScope.CreateQueryDispatcher();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _dataScope.DisposeAsync();

    [Fact]
    public async Task QueryExecutesSuccessfully()
    {
        // Arrange
        var firstName = _dataScope.TestData.GenerateFirstName();
        var middleName = _dataScope.TestData.GenerateMiddleName();
        var lastName = _dataScope.TestData.GenerateLastName();
        var email = _dataScope.TestData.GenerateUniqueEmail();
        var nino = _dataScope.TestData.GenerateNationalInsuranceNumber();
        var address1 = Faker.Address.StreetName();
        var address2 = Faker.Address.StreetName();
        var address3 = Faker.Address.StreetName();
        var city = Faker.Address.City().ToUpper();
        var country = "England";
        var postCode = Faker.Address.UkPostCode();
        var gender = Contact_GenderCode.Notavailable;
        var dateOfBirth = _dataScope.TestData.GenerateDateOfBirth();
        var trn = await _dataScope.TestData.GenerateTrnAsync();

        var query = new CreateContactQuery()
        {
            TrnRequestId = null,
            FirstName = firstName,
            MiddleName = middleName,
            LastName = lastName,
            StatedFirstName = firstName,
            StatedMiddleName = middleName,
            StatedLastName = lastName,
            EmailAddress = email,
            NationalInsuranceNumber = nino,
            DateOfBirth = dateOfBirth,
            Gender = gender,
            Trn = trn,
            PotentialDuplicates = [],
            ApplicationUserName = "Tests",
            OutboxMessages = [],
            Address1Line1 = address1,
            Address1Line2 = address2,
            Address1Line3 = address3,
            Address1City = city,
            Address1Country = country,
            Address1PostalCode = postCode
        };

        // Act
        var createdTeacherId = await _crmQueryDispatcher.ExecuteQueryAsync(query);

        // Assert
        using var ctx = new DqtCrmServiceContext(_dataScope.OrganizationService);
        var contact = ctx.ContactSet.SingleOrDefault(i => i.GetAttributeValue<Guid>(Contact.PrimaryIdAttribute) == createdTeacherId);
        Assert.NotNull(contact);
        Assert.NotNull(contact.dfeta_TRN);
        Assert.Equal(firstName, contact.FirstName);
        Assert.Equal(middleName, contact.MiddleName);
        Assert.Equal(lastName, contact.LastName);
        Assert.Equal(email, contact.EMailAddress1);
        Assert.False(contact.dfeta_AllowPiiUpdatesFromRegister);
        Assert.Equal(nino, contact.dfeta_NINumber);
        Assert.Equal(dateOfBirth, contact.BirthDate.ToDateOnlyWithDqtBstFix(true));
        Assert.Equal(address1, contact.Address1_Line1);
        Assert.Equal(address2, contact.Address1_Line2);
        Assert.Equal(address3, contact.Address1_Line3);
        Assert.Equal(city, contact.Address1_City);
        Assert.Equal(postCode, contact.Address1_PostalCode);
        Assert.Equal(country, contact.Address1_Country);
        Assert.Equal(gender, contact.GenderCode);
    }

    [Fact]
    public async Task QueryWithMatchedDuplicateContactName_ExecutesSuccessfully_CreatesTask()
    {
        // Arrange
        var firstName = $"{_dataScope.TestData.GenerateFirstName()}";
        var middleName = $"{_dataScope.TestData.GenerateMiddleName()}";
        var lastName = $"{_dataScope.TestData.GenerateLastName()}";
        var email = _dataScope.TestData.GenerateUniqueEmail();
        var nino = _dataScope.TestData.GenerateNationalInsuranceNumber();
        var dateOfBirth = _dataScope.TestData.GenerateDateOfBirth();

        var existingContactId = Guid.NewGuid();
        var existingContactTrn = await _dataScope.TestData.GenerateTrnAsync();
        await _dataScope.OrganizationService.CreateAsync(new Contact()
        {
            Id = existingContactId,
            FirstName = firstName,
            MiddleName = middleName,
            LastName = lastName,
            BirthDate = dateOfBirth.ToDateTimeWithDqtBstFix(isLocalTime: false),
            dfeta_TRN = existingContactTrn
        });

        // Act
        var query = new CreateContactQuery()
        {
            TrnRequestId = null,
            FirstName = firstName,
            MiddleName = middleName,
            LastName = lastName,
            StatedFirstName = firstName,
            StatedMiddleName = middleName,
            StatedLastName = lastName,
            EmailAddress = email,
            NationalInsuranceNumber = nino,
            DateOfBirth = dateOfBirth,
            Gender = Contact_GenderCode.Notavailable,
            Trn = null,
            PotentialDuplicates =
            [
                (Duplicate: new FindPotentialDuplicateContactsResult()
                {
                    ContactId = existingContactId,
                    Trn = existingContactTrn,
                    MatchedAttributes = [Contact.Fields.FirstName, Contact.Fields.MiddleName, Contact.Fields.LastName],
                    HasEytsDate = false,
                    HasQtsDate = false,
                    FirstName = firstName,
                    MiddleName = middleName,
                    LastName = lastName,
                    StatedFirstName = firstName,
                    StatedMiddleName = middleName,
                    StatedLastName = lastName,
                    PreviousLastName = null,
                    DateOfBirth = dateOfBirth,
                    EmailAddress = email,
                    NationalInsuranceNumber = nino
                },
                HasActiveAlert: false)
            ],
            ApplicationUserName = "Tests",
            OutboxMessages = [],
            Address1Line1 = null,
            Address1Line2 = null,
            Address1Line3 = null,
            Address1City = null,
            Address1Country = null,
            Address1PostalCode = null
        };
        var createdTeacherId2 = await _crmQueryDispatcher.ExecuteQueryAsync(query);
        using var ctx = new DqtCrmServiceContext(_dataScope.OrganizationService);
        var contact = ctx.ContactSet.SingleOrDefault(i => i.GetAttributeValue<Guid>(Contact.PrimaryIdAttribute) == createdTeacherId2);
        var potentialDuplicateTask = ctx.TaskSet.FirstOrDefault(x => x.RegardingObjectId == createdTeacherId2.ToEntityReference(Contact.EntityLogicalName));

        // Assert
        Assert.NotNull(contact);
        Assert.Null(contact.dfeta_TRN);
        Assert.NotNull(potentialDuplicateTask);
        Assert.Contains("Potential duplicate", potentialDuplicateTask.Description);
        Assert.Equal("TRN request from Tests", potentialDuplicateTask.Category);
    }
}
