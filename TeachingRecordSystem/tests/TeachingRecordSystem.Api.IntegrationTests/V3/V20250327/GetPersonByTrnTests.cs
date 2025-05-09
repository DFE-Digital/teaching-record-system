namespace TeachingRecordSystem.Api.IntegrationTests.V3.V20250327;

public class GetPersonByTrnTests : TestBase
{
    public GetPersonByTrnTests(HostFixture hostFixture)
        : base(hostFixture)
    {
        SetCurrentApiClient(roles: [ApiRoles.GetPerson]);
    }

    [Fact]
    public async Task Get_PersonWithQtlsAndQtsViaAnotherRoute_ReturnsExpectedAwardedOrApprovedCount()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithTrn()
            .WithQts()
            .WithQtlsDate(Clock.Today));

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/v3/persons/{person.Trn}");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        var responseJson = await AssertEx.JsonResponseAsync(response);
        var awardedOrApprovedCount = responseJson.RootElement.GetProperty("qts").GetProperty("awardedOrApprovedCount").GetInt32();
        Assert.Equal(2, awardedOrApprovedCount);
    }

    [Fact]
    public async Task ValidRequest_ForInactiveContact_ReturnsNotFound()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithTrn().WithContactState(ContactState.Inactive));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/v3/persons/{person.Trn}");

        // Act
        var response = await GetHttpClientWithApiKey().SendAsync(request);

        // Assert
        await AssertEx.JsonResponseIsErrorAsync(response, ApiError.ErrorCodes.PersonInactive, StatusCodes.Status404NotFound);
    }
}
