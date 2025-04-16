using TeachingRecordSystem.Core.Services.DqtOutbox.Messages;

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
            ReviewTasks = [],
            ApplicationUserName = "Tests",
            TrnRequestMetadataMessage = new TrnRequestMetadataMessage
            {
                ApplicationUserId = Guid.NewGuid(),
                RequestId = Guid.NewGuid().ToString(),
                CreatedOn = DateTime.UtcNow,
                IdentityVerified = null,
                EmailAddress = email,
                OneLoginUserSubject = null,
                Name = new string[]
                {
                    firstName,
                    middleName,
                    lastName
                },
                FirstName = firstName,
                MiddleName = middleName,
                LastName = lastName,
                DateOfBirth = dateOfBirth,
                PotentialDuplicate = false,
                NationalInsuranceNumber = nino,
                Gender = null,
                AddressLine1 = null,
                AddressLine2 = null,
                AddressLine3 = null,
                City = null,
                Postcode = null,
                Country = null,
                TrnToken = null
            },
            AllowPiiUpdates = true
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
        Assert.Equal(gender, contact.GenderCode);
        Assert.True(contact.dfeta_AllowPiiUpdatesFromRegister);
    }
}
