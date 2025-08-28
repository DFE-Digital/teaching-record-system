#nullable disable
using TeachingRecordSystem.Api.Properties;

namespace TeachingRecordSystem.Api.IntegrationTests.V1.Operations;

[Collection(nameof(DisableParallelization))]
public class GetTeacherTests : TestBase
{
    public GetTeacherTests(HostFixture hostFixture)
        : base(hostFixture)
    {
    }

    public override Task InitializeAsync() => DbHelper.DeleteAllPersonsAsync();

    [Theory]
    [InlineData("123456")]
    [InlineData("12345678")]
    [InlineData("xxx")]
    public async Task Given_invalid_trn_returns_error(string trn)
    {
        // Arrange
        var birthDate = "1990-04-01";
        var request = new HttpRequestMessage(HttpMethod.Get, $"/v1/teachers/{trn}?birthdate={birthDate:yyyy-MM-dd}");

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
        var request = new HttpRequestMessage(HttpMethod.Get, $"/v1/teachers/{trn}?birthdate={birthDate:yyyy-MM-dd}");

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

        var request = new HttpRequestMessage(HttpMethod.Get, $"/v1/teachers/{trn}?birthdate={birthDate:yyyy-MM-dd}");

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

        var person = await TestData.CreatePersonAsync(p => p.WithDateOfBirth(birthDate));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/v1/teachers/{person.Trn}?birthdate={birthDate:yyyy-MM-dd}");

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

        var personWithMatchingTrn = await TestData.CreatePersonAsync(p => p.WithDateOfBirth(birthDate));

        var personWithMatchingNino = await TestData.CreatePersonAsync(p => p.WithDateOfBirth(birthDate).WithNationalInsuranceNumber());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/v1/teachers/{personWithMatchingTrn.Trn}?birthdate={birthDate:yyyy-MM-dd}&nino={personWithMatchingNino.NationalInsuranceNumber}");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        var responseJson = await AssertEx.JsonResponseAsync(response, expectedStatusCode: StatusCodes.Status200OK);
        Assert.Equal(personWithMatchingTrn.Trn, responseJson.RootElement.GetProperty("trn").GetString());
    }
}
