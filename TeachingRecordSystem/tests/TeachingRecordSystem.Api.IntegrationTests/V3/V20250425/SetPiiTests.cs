using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using TeachingRecordSystem.Api.V3.Implementation.Dtos;
using TeachingRecordSystem.Core.Dqt;

namespace TeachingRecordSystem.Api.IntegrationTests.V3.V20250425;

public class SetPiiTests : TestBase
{
    public SetPiiTests(HostFixture hostFixture) : base(hostFixture)
    {
        SetCurrentApiClient([ApiRoles.UpdatePerson]);
    }

    [Theory, RoleNamesData(except: ApiRoles.UpdatePerson)]
    public async Task Put_UserDoesNotHavePermission_ReturnsForbidden(string[] roles)
    {
        // Arrange
        SetCurrentApiClient(roles);

        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn());

        var request = new HttpRequestMessage(HttpMethod.Put, $"/v3/persons/{person.Trn}")
        {
            Content = CreateJsonContent(new
            {
                firstName = Faker.Name.First(),
                middleName = Faker.Name.Middle(),
                lastName = Faker.Name.Last(),
                dateOfBirth = Faker.Identification.DateOfBirth().ToShortDateString(),
                emailAddress = Faker.Internet.Email(),
                nationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber(),
                gender = Gender.Male
            })
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Fact]
    public async Task Put_PersonDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var content = CreateJsonContent(new
        {
            firstName = Faker.Name.First(),
            middleName = Faker.Name.Middle(),
            lastName = Faker.Name.Last(),
            dateOfBirth = Faker.Identification.DateOfBirth().ToDateOnlyWithDqtBstFix(isLocalTime: false),
            emailAddress = Faker.Internet.Email(),
            nationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber(),
            gender = Gender.Male
        });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/0000001/set-pii", content);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Put_PiiUpdatesNotPermitted_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn());

        XrmFakedContext.UpdateEntity(new Contact()
        {
            ContactId = person.ContactId,
            dfeta_AllowPiiUpdatesFromRegister = false
        });

        var content = CreateJsonContent(new
        {
            firstName = Faker.Name.First(),
            middleName = Faker.Name.Middle(),
            lastName = Faker.Name.Last(),
            dateOfBirth = Faker.Identification.DateOfBirth().ToDateOnlyWithDqtBstFix(isLocalTime: false),
            emailAddress = Faker.Internet.Email(),
            nationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber(),
            gender = Gender.Male
        });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}", content);

        // Assert
        await AssertEx.JsonResponseIsErrorAsync(response, ApiError.ErrorCodes.PiiUpdatesForbidden, StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task Put_PersontHasQts_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithQts());

        var content = CreateJsonContent(new
        {
            firstName = Faker.Name.First(),
            middleName = Faker.Name.Middle(),
            lastName = Faker.Name.Last(),
            dateOfBirth = Faker.Identification.DateOfBirth().ToDateOnlyWithDqtBstFix(isLocalTime: false),
            emailAddress = Faker.Internet.Email(),
            nationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber(),
            gender = Gender.Male
        });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}", content);

        // Assert
        await AssertEx.JsonResponseIsErrorAsync(response, ApiError.ErrorCodes.PiiUpdatesForbiddenPersonHasQts, StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task Put_ValidRequest_ReturnsNoContent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithGender(Contact_GenderCode.Male));

        var updatedFirstName = Faker.Name.First();
        var updatedMiddleName = Faker.Name.Middle();
        var updatedLastName = Faker.Name.Last();
        var updatdDateOfBirth = Faker.Identification.DateOfBirth().ToDateOnlyWithDqtBstFix(isLocalTime: false);
        var expectedEmailAddress = Faker.Internet.Email();
        var updatedNationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();
        var updatedGender = Gender.Female;
        var expectedGenderOptionSetValue = new OptionSetValue((int)updatedGender);

        var content = CreateJsonContent(new
        {
            firstName = updatedFirstName,
            middleName = updatedMiddleName,
            lastName = updatedLastName,
            dateOfBirth = updatdDateOfBirth,
            emailAddress = expectedEmailAddress,
            nationalInsuranceNumber = updatedNationalInsuranceNumber,
            gender = updatedGender
        });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}", content);

        // Assert
        Assert.Equal(StatusCodes.Status204NoContent, (int)response.StatusCode);
        var updatedContact = TestData.OrganizationService.Retrieve(Contact.EntityLogicalName, person.ContactId, new ColumnSet() { AllColumns = true });
        var actualDateOfBirth = ((DateTime)updatedContact[Contact.Fields.BirthDate]).ToDateOnlyWithDqtBstFix(isLocalTime: false);
        var actualGender = ((OptionSetValue)updatedContact[Contact.Fields.GenderCode]);
        Assert.NotNull(updatedContact);
        Assert.Equal(updatedFirstName, updatedContact[Contact.Fields.FirstName]);
        Assert.Equal(updatedMiddleName, updatedContact[Contact.Fields.MiddleName]);
        Assert.Equal(updatedLastName, updatedContact[Contact.Fields.LastName]);
        Assert.Equal(updatdDateOfBirth, actualDateOfBirth);
        Assert.Equal(expectedEmailAddress, updatedContact[Contact.Fields.EMailAddress1]);
        Assert.Equal(updatedNationalInsuranceNumber, updatedContact[Contact.Fields.dfeta_NINumber]);
        Assert.Equal(expectedGenderOptionSetValue.Value, actualGender.Value);
    }

    [Fact]
    public async Task Put_OptionalFieldsCanBeSetToNull_ReturnsNoContent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithGender(Contact_GenderCode.Male)
            .WithEmail(Faker.Internet.Email())
            .WithNationalInsuranceNumber(Faker.Identification.UkNationalInsuranceNumber()));

        var updatedFirstName = Faker.Name.First();
        var updatedMiddleName = Faker.Name.Middle();
        var updatedLastName = Faker.Name.Last();
        var updatdDateOfBirth = Faker.Identification.DateOfBirth().ToDateOnlyWithDqtBstFix(isLocalTime: false);
        var expectedEmailAddress = default(string?);
        var updatedNationalInsuranceNumber = default(string?);
        var updatedGender = default(Gender?);

        var content = CreateJsonContent(new
        {
            firstName = updatedFirstName,
            middleName = updatedMiddleName,
            lastName = updatedLastName,
            dateOfBirth = updatdDateOfBirth,
            nationalInsuranceNumber = updatedNationalInsuranceNumber,
            emailAddress = expectedEmailAddress,
            gender = updatedGender
        });

        // Act
        var response = await GetHttpClientWithApiKey().PutAsync($"/v3/persons/{person.Trn}", content);

        // Assert
        Assert.Equal(StatusCodes.Status204NoContent, (int)response.StatusCode);
        var updatedContact = TestData.OrganizationService.Retrieve(Contact.EntityLogicalName, person.ContactId, new ColumnSet() { AllColumns = true }).ToEntity<Contact>();
        Assert.NotNull(updatedContact);
        Assert.Equal(updatedFirstName, updatedContact.FirstName);
        Assert.Equal(updatedMiddleName, updatedContact.MiddleName);
        Assert.Equal(updatedLastName, updatedContact.LastName);
        Assert.Equal(updatdDateOfBirth, updatedContact.BirthDate!.Value.ToDateOnlyWithDqtBstFix(isLocalTime: false));
        Assert.Null(updatedContact.EMailAddress1);
        Assert.Null(updatedContact.dfeta_NINumber);
        Assert.Null(updatedContact.GenderCode);
    }
}
