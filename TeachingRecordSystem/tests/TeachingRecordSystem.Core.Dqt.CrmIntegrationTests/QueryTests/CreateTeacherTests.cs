namespace TeachingRecordSystem.Core.Dqt.CrmIntegrationTests.QueryTests;

public class CreateTeacherTests : IAsyncLifetime
{
    private readonly CrmClientFixture.TestDataScope _dataScope;
    private readonly CrmQueryDispatcher _crmQueryDispatcher;
    private readonly CrmClientFixture _fixture;

    public CreateTeacherTests(CrmClientFixture crmClientFixture)
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
        var ni = _dataScope.TestData.GenerateNationalInsuranceNumber();
        var dob = _dataScope.TestData.GenerateDateOfBirth();
        var trn = await _dataScope.TestData.GenerateTrn();

        var query = new CreateContactQuery()
        {
            FirstName = firstName,
            MiddleName = middleName,
            LastName = lastName,
            Email = email,
            NationalInsuranceNumber = ni,
            DateOfBirth = dob,
            Trn = trn,
            ExistingTeacherResults = []
        };

        // Act
        var createdTeacherId = await _crmQueryDispatcher.ExecuteQuery(query);

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
        Assert.Equal(ni, contact.dfeta_NINumber);
        Assert.Equal(dob, contact.BirthDate.ToDateOnlyWithDqtBstFix(true));
    }

    [Theory]
    [InlineData("1", "", "", "First name contains a digit")]
    [InlineData("", "1", "", "Middle name contains a digit")]
    [InlineData("", "", "1", "Last name contains a digit")]
    public async Task QueryWithDigits_ExecutesSuccessfully_CreatesTask(string firstNameStr, string middleNameStr, string lastNameStr, string description)
    {
        // Arrange
        var firstName = $"{_dataScope.TestData.GenerateFirstName()}{firstNameStr}";
        var middleName = $"{_dataScope.TestData.GenerateMiddleName()}{middleNameStr}";
        var lastName = $"{_dataScope.TestData.GenerateLastName()}{lastNameStr}";
        var email = _dataScope.TestData.GenerateUniqueEmail();
        var ni = _dataScope.TestData.GenerateNationalInsuranceNumber();
        var dob = _dataScope.TestData.GenerateDateOfBirth();
        var trn = await _dataScope.TestData.GenerateTrn();

        var query = new CreateContactQuery()
        {
            FirstName = firstName,
            MiddleName = middleName,
            LastName = lastName,
            Email = email,
            NationalInsuranceNumber = ni,
            DateOfBirth = dob,
            Trn = trn,
            ExistingTeacherResults = []
        };

        // Act
        var createdTeacherId = await _crmQueryDispatcher.ExecuteQuery(query);

        // Assert
        using var ctx = new DqtCrmServiceContext(_dataScope.OrganizationService);
        var contact = ctx.ContactSet.SingleOrDefault(i => i.GetAttributeValue<Guid>(Contact.PrimaryIdAttribute) == createdTeacherId);
        var task = ctx.TaskSet.FirstOrDefault(x => x.RegardingObjectId == createdTeacherId.ToEntityReference(Contact.EntityLogicalName));
        Assert.NotNull(contact);
        Assert.NotNull(task);
        Assert.Equal(description, task.Description);
        Assert.Equal("DMSImportTrn", task.Category);
    }

    [Fact]
    public async Task QueryWithMatchedDuplicateContactName_ExecutesSuccessfully_CreatesTask()
    {
        // Arrange
        var firstName = $"{_dataScope.TestData.GenerateFirstName()}";
        var middleName = $"{_dataScope.TestData.GenerateMiddleName()}";
        var lastName = $"{_dataScope.TestData.GenerateLastName()}";
        var email = _dataScope.TestData.GenerateUniqueEmail();
        var ni = _dataScope.TestData.GenerateNationalInsuranceNumber();
        var dob = _dataScope.TestData.GenerateDateOfBirth();
        var trn1 = await _dataScope.TestData.GenerateTrn();
        var trn2 = await _dataScope.TestData.GenerateTrn();

        var query1 = new CreateContactQuery()
        {
            FirstName = firstName,
            MiddleName = middleName,
            LastName = lastName,
            Email = email,
            NationalInsuranceNumber = ni,
            DateOfBirth = dob,
            Trn = trn1,
            ExistingTeacherResults = []
        };

        // Act
        var createdTeacherId1 = await _crmQueryDispatcher.ExecuteQuery(query1);
        var query2 = new CreateContactQuery()
        {
            FirstName = firstName,
            MiddleName = middleName,
            LastName = lastName,
            Email = email,
            NationalInsuranceNumber = ni,
            DateOfBirth = dob,
            Trn = trn2,
            ExistingTeacherResults = new[] {
                new FindingExistingTeachersResult()
                {
                    TeacherId = createdTeacherId1,
                    MatchedAttributes = new[] { Contact.Fields.FirstName, Contact.Fields.MiddleName, Contact.Fields.LastName },
                    HasActiveSanctions = false,
                    HasEytsDate = false,
                    HasQtsDate = false
                }
            }
        };
        var createdTeacherId2 = await _crmQueryDispatcher.ExecuteQuery(query2);
        using var ctx = new DqtCrmServiceContext(_dataScope.OrganizationService);
        var contact1 = ctx.ContactSet.SingleOrDefault(i => i.GetAttributeValue<Guid>(Contact.PrimaryIdAttribute) == createdTeacherId1);
        var contact2 = ctx.ContactSet.SingleOrDefault(i => i.GetAttributeValue<Guid>(Contact.PrimaryIdAttribute) == createdTeacherId2);
        var potentialDuplicateTask = ctx.TaskSet.FirstOrDefault(x => x.RegardingObjectId == createdTeacherId2.ToEntityReference(Contact.EntityLogicalName));

        // Assert
        Assert.NotNull(contact1);
        Assert.NotNull(contact2);
        Assert.Null(contact2.dfeta_TRN);
        Assert.NotNull(potentialDuplicateTask);
        Assert.Contains("Potential duplicate", potentialDuplicateTask.Description);
        Assert.Equal("DMSImportTrn", potentialDuplicateTask.Category);
    }
}
