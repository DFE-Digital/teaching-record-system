using System.Net;
using TeachingRecordSystem.Api.Properties;
using TeachingRecordSystem.Api.V3.VNext.Requests;
using TeachingRecordSystem.Core.Dqt;

namespace TeachingRecordSystem.Api.Tests.V3.VNext;

public class SetQTLSDateRequestTests : TestBase
{
    public SetQTLSDateRequestTests(HostFixture hostFixture)
        : base(hostFixture)
    {
        SetCurrentApiClient(new[] { ApiRoles.UpdatePerson });
    }

    [Theory, RoleNamesData(except: ApiRoles.UpdatePerson)]
    public async Task Put_ClientDoesNotHavePermission_ReturnsForbidden(string[] roles)
    {
        // Arrange
        SetCurrentApiClient(roles);

        var requestId = Guid.NewGuid().ToString();
        var firstName = Faker.Name.First();
        var middleName = Faker.Name.Middle();
        var lastName = Faker.Name.Last();
        var dateOfBirth = new DateOnly(1990, 01, 01);
        var email = Faker.Internet.Email();
        var nationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();

        var existingContact = await TestData.CreatePerson(p => p
            .WithTrn(hasTrn: true)
            .WithFirstName(firstName)
            .WithMiddleName(middleName)
            .WithLastName(lastName)
            .WithDateOfBirth(dateOfBirth)
            .WithEmail(email)
            .WithNationalInsuranceNumber(nationalInsuranceNumber: nationalInsuranceNumber));

        var requestBody = CreateJsonContent(new { AwardedDate = new DateOnly(1990,01,01) });
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/{existingContact.Trn}/qtls")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Theory]
    [InlineData("123456")]
    [InlineData("12345678")]
    [InlineData("xxx")]
    public async Task Put_InvalidTrn_ReturnsErrror(string trn)
    {
        // Arrange
        var requestBody = CreateJsonContent(new { AwardedDate = new DateOnly(1990, 01, 01) });
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/{trn}/qtls")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(response, "trn", expectedError: StringResources.ErrorMessages_TRNMustBe7Digits);
    }

    [Fact]
    public async Task Put_AwardedDateInFuture_ReturnsErrror()
    {
        // Arrange
        var futureDate = Clock.UtcNow.AddDays(1);
        var requestId = Guid.NewGuid().ToString();
        var firstName = Faker.Name.First();
        var middleName = Faker.Name.Middle();
        var lastName = Faker.Name.Last();
        var dateOfBirth = new DateOnly(1990, 01, 01);
        var email = Faker.Internet.Email();
        var nationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();

        var existingContact = await TestData.CreatePerson(p => p
            .WithTrn(hasTrn: true)
            .WithFirstName(firstName)
            .WithMiddleName(middleName)
            .WithLastName(lastName)
            .WithDateOfBirth(dateOfBirth)
            .WithEmail(email)
            .WithNationalInsuranceNumber(nationalInsuranceNumber: nationalInsuranceNumber));

        var requestBody = CreateJsonContent(new { AwardedDate = futureDate.ToDateOnlyWithDqtBstFix(isLocalTime:true) });
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/{existingContact.Trn}/qtls")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(
            response,
            propertyName: nameof(SetQTLSRequest.AwardedDate),
            expectedError: "Awarded date cannot be in the future.");
    }

    [Fact]
    public async Task Put_TrnNotFound_ReturnsNotFound()
    {
        // Arrange
        var futureDate = Clock.UtcNow.AddDays(1);
        var requestId = Guid.NewGuid().ToString();
        var firstName = Faker.Name.First();
        var middleName = Faker.Name.Middle();
        var lastName = Faker.Name.Last();
        var dateOfBirth = new DateOnly(1990, 01, 01);
        var email = Faker.Internet.Email();
        var nationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();
        var nonExistentTrn = "1234567";

        var existingContact = await TestData.CreatePerson(p => p
            .WithTrn(hasTrn: true)
            .WithFirstName(firstName)
            .WithMiddleName(middleName)
            .WithLastName(lastName)
            .WithDateOfBirth(dateOfBirth)
            .WithEmail(email)
            .WithNationalInsuranceNumber(nationalInsuranceNumber: nationalInsuranceNumber));

        var requestBody = CreateJsonContent(new { AwardedDate = new DateOnly(1990,01,01) });
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/{nonExistentTrn}/qtls")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Put_ValidQTLSDate_ReturnsExpectedResult()
    {
        // Arrange
        var qtlsDate = new DateOnly(2020, 01, 01);
        var requestId = Guid.NewGuid().ToString();
        var firstName = Faker.Name.First();
        var middleName = Faker.Name.Middle();
        var lastName = Faker.Name.Last();
        var dateOfBirth = new DateOnly(1990, 01, 01);
        var email = Faker.Internet.Email();
        var nationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();

        var existingContact = await TestData.CreatePerson(p => p
            .WithTrn(hasTrn: true)
            .WithFirstName(firstName)
            .WithMiddleName(middleName)
            .WithLastName(lastName)
            .WithDateOfBirth(dateOfBirth)
            .WithEmail(email)
            .WithNationalInsuranceNumber(nationalInsuranceNumber: nationalInsuranceNumber));

        var requestBody = CreateJsonContent(new { AwardedDate = qtlsDate });
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/{existingContact.Trn}/qtls")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        await AssertEx.JsonResponseEquals(
            response,
            expected: new
            {
                Trn = existingContact.Trn,
                AwardedDate = qtlsDate

            },
            expectedStatusCode: 200);
    }

    [Fact]
    public async Task Put_ClearExistingQTLSDate_ReturnsExpectedResult()
    {
        // Arrange
        var qtlsDate = default(DateOnly?);
        var requestId = Guid.NewGuid().ToString();
        var firstName = Faker.Name.First();
        var middleName = Faker.Name.Middle();
        var lastName = Faker.Name.Last();
        var dateOfBirth = new DateOnly(1990, 01, 01);
        var email = Faker.Internet.Email();
        var nationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();

        var existingContact = await TestData.CreatePerson(p => p
            .WithTrn(hasTrn: true)
            .WithFirstName(firstName)
            .WithMiddleName(middleName)
            .WithLastName(lastName)
            .WithDateOfBirth(dateOfBirth)
            .WithEmail(email)
            .WithNationalInsuranceNumber(nationalInsuranceNumber: nationalInsuranceNumber)
            .WithQTLSDate(new DateOnly(2020,01,01));

        var requestBody = CreateJsonContent(new { AwardedDate = qtlsDate });
        var request = new HttpRequestMessage(HttpMethod.Put, $"v3/persons/{existingContact.Trn}/qtls")
        {
            Content = requestBody
        };

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        await AssertEx.JsonResponseEquals(
            response,
            expected: new
            {
                Trn = existingContact.Trn,
                AwardedDate = qtlsDate

            },
            expectedStatusCode: 200);
    }
}
