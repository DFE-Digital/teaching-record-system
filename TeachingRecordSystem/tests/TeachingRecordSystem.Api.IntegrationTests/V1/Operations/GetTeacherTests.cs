#nullable disable
using TeachingRecordSystem.Api.Properties;

namespace TeachingRecordSystem.Api.IntegrationTests.V1.Operations;

public class GetTeacherTests : TestBase
{
    public GetTeacherTests(HostFixture hostFixture)
        : base(hostFixture)
    {
    }

    [Theory]
    [InlineData("123456")]
    [InlineData("12345678")]
    [InlineData("xxx")]
    public async Task Given_invalid_trn_returns_error(string trn)
    {
        // Arrange
        var birthDate = "1990-04-01";
        var request = new HttpRequestMessage(HttpMethod.Get, $"/v1/teachers/{trn}?birthdate={birthDate}");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(response, "trn", expectedError: StringResources.ErrorMessages_TRNMustBe7Digits);
    }

    [Theory]
    [InlineData("xxx")]
    public async Task Given_invalid_birthdate_returns_error(string birthDate)
    {
        // Arrange
        var trn = "1234567";
        var request = new HttpRequestMessage(HttpMethod.Get, $"/v1/teachers/{trn}?birthdate={birthDate}");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForPropertyAsync(response, "birthdate", expectedError: $"The value '{birthDate}' is not valid for BirthDate.");
    }

    [Fact]
    public async Task Given_no_match_found_returns_notfound()
    {
        // Arrange
        var trn = "1234567";
        var birthDate = "1990-04-01";

        DataverseAdapterMock
            .Setup(mock => mock.FindTeachersAsync(It.IsAny<FindTeachersByTrnBirthDateAndNinoQuery>()))
            .ReturnsAsync(Array.Empty<Contact>());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/v1/teachers/{trn}?birthdate={birthDate}");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Given_match_returns_ok()
    {
        // Arrange
        var birthDate = new DateOnly(1990, 4, 1);

        var person = await TestData.CreatePersonAsync(p => p.WithTrn().WithDateOfBirth(birthDate));

        var contact = new Contact()
        {
            Id = person.ContactId,
            BirthDate = birthDate.ToDateTime(),
            dfeta_TRN = person.Trn,
            StateCode = ContactState.Active,
            FormattedValues =
            {
                { Contact.Fields.StateCode, "Active" }
            }
        };

        DataverseAdapterMock
            .Setup(mock => mock.FindTeachersAsync(It.IsAny<FindTeachersByTrnBirthDateAndNinoQuery>()))
            .ReturnsAsync([contact]);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/v1/teachers/{person.Trn}?birthdate={birthDate}");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
    }

    [Fact]
    public async Task Given_multiple_matches_returns_match_on_TRN()
    {
        // Arrange
        var birthDate = new DateOnly(1990, 4, 1);

        var personWithMatchingTrn = await TestData.CreatePersonAsync(p => p.WithTrn().WithDateOfBirth(birthDate));

        var contactWithMatchingTrn = new Contact()
        {
            Id = personWithMatchingTrn.ContactId,
            BirthDate = birthDate.ToDateTime(),
            dfeta_TRN = personWithMatchingTrn.Trn,
            StateCode = ContactState.Active,
            FormattedValues =
            {
                { Contact.Fields.StateCode, "Active" }
            }
        };

        var personWithMatchingNino = await TestData.CreatePersonAsync(p => p.WithTrn().WithDateOfBirth(birthDate).WithNationalInsuranceNumber());

        var contactWithMatchingNino = new Contact()
        {
            Id = personWithMatchingNino.ContactId,
            BirthDate = birthDate.ToDateTime(),
            dfeta_TRN = personWithMatchingNino.Trn,
            dfeta_NINumber = personWithMatchingNino.NationalInsuranceNumber,
            StateCode = ContactState.Active,
            FormattedValues =
            {
                { Contact.Fields.StateCode, "Active" }
            }
        };

        DataverseAdapterMock
            .Setup(mock => mock.FindTeachersAsync(It.IsAny<FindTeachersByTrnBirthDateAndNinoQuery>()))
            .ReturnsAsync(new[] { contactWithMatchingTrn, contactWithMatchingNino });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/v1/teachers/{personWithMatchingTrn.Trn}?birthdate={birthDate}&nino={personWithMatchingNino.NationalInsuranceNumber}");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        var responseJson = await AssertEx.JsonResponseAsync(response, expectedStatusCode: StatusCodes.Status200OK);
        Assert.Equal(personWithMatchingTrn.Trn, responseJson.RootElement.GetProperty("trn").GetString());
    }
}
