namespace TeachingRecordSystem.AuthorizeAccess.Tests.PageTests.RequestTrn;

public class IndexTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_ValidRequest_RendersExpectedContent()
    {
        // Arrange
        var accessToken = HostFixture.Services.GetRequiredService<IConfiguration>().GetValue<string>("RequestTrnAccessToken");
        var state = CreateNewState();
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/request-trn?{journeyInstance.GetUniqueIdQueryParameter()}&AccessToken={accessToken}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseAsync(response);
    }

    [Fact]
    public async Task Get_MissingAccessToken_ReturnsBadRequest()
    {
        // Arrange
        var state = CreateNewState();
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/request-trn?{journeyInstance.GetUniqueIdQueryParameter()}&AccessToken=");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }
}
