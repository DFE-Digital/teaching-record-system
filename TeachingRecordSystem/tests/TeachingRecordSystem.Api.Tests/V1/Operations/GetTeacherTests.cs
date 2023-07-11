#nullable disable
using TeachingRecordSystem.Api.Properties;
using TeachingRecordSystem.TestCommon;

namespace TeachingRecordSystem.Api.Tests.V1.Operations;

public class GetTeacherTests : ApiTestBase
{
    public GetTeacherTests(ApiFixture apiFixture)
        : base(apiFixture)
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
        var response = await HttpClientWithApiKey.SendAsync(request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(response, "trn", expectedError: StringResources.ErrorMessages_TRNMustBe7Digits);
    }

    [Theory]
    [InlineData("xxx")]
    public async Task Given_invalid_birthdate_returns_error(string birthDate)
    {
        // Arrange
        var trn = "1234567";
        var request = new HttpRequestMessage(HttpMethod.Get, $"/v1/teachers/{trn}?birthdate={birthDate}");

        // Act
        var response = await HttpClientWithApiKey.SendAsync(request);

        // Assert
        await AssertEx.JsonResponseHasValidationErrorForProperty(response, "birthdate", expectedError: $"The value '{birthDate}' is not valid for BirthDate.");
    }

    [Fact]
    public async Task Given_no_match_found_returns_notfound()
    {
        // Arrange
        var trn = "1234567";
        var birthDate = "1990-04-01";

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.FindTeachers(It.IsAny<FindTeachersByTrnBirthDateAndNinoQuery>()))
            .ReturnsAsync(Array.Empty<Contact>());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/v1/teachers/{trn}?birthdate={birthDate}");

        // Act
        var response = await HttpClientWithApiKey.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Given_match_returns_ok()
    {
        // Arrange
        var trn = "1234567";
        var birthDate = "1990-04-01";

        var contact = new Contact()
        {
            BirthDate = DateTime.Parse(birthDate),
            dfeta_TRN = trn,
            StateCode = ContactState.Active,
            FormattedValues =
            {
                { Contact.Fields.StateCode, "Active" }
            }
        };

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.FindTeachers(It.IsAny<FindTeachersByTrnBirthDateAndNinoQuery>()))
            .ReturnsAsync(new[] { contact });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/v1/teachers/{trn}?birthdate={birthDate}");

        // Act
        var response = await HttpClientWithApiKey.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
    }

    [Fact]
    public async Task Given_multiple_matches_returns_match_on_TRN()
    {
        // Arrange
        var matchingTrn = "1234567";
        var anotherTrn = "1234567";
        var birthDate = "1990-04-01";
        var nino = "AB012345C";

        var contactWithMatchingTrn = new Contact()
        {
            BirthDate = DateTime.Parse(birthDate),
            dfeta_TRN = matchingTrn,
            StateCode = ContactState.Active,
            FormattedValues =
            {
                { Contact.Fields.StateCode, "Active" }
            }
        };

        var contactWithMatchingNino = new Contact()
        {
            BirthDate = DateTime.Parse(birthDate),
            dfeta_TRN = anotherTrn,
            dfeta_NINumber = nino,
            StateCode = ContactState.Active,
            FormattedValues =
            {
                { Contact.Fields.StateCode, "Active" }
            }
        };

        ApiFixture.DataverseAdapter
            .Setup(mock => mock.FindTeachers(It.IsAny<FindTeachersByTrnBirthDateAndNinoQuery>()))
            .ReturnsAsync(new[] { contactWithMatchingTrn, contactWithMatchingNino });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/v1/teachers/{matchingTrn}?birthdate={birthDate}&nino={nino}");

        // Act
        var response = await HttpClientWithApiKey.SendAsync(request);

        // Assert
        var responseJson = await AssertEx.JsonResponse(response, expectedStatusCode: StatusCodes.Status200OK);
        Assert.Equal(matchingTrn, responseJson.RootElement.GetProperty("trn").GetString());
    }
}
