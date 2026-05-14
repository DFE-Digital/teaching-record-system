namespace TeachingRecordSystem.AuthorizeAccess.Tests.PageTests;

public class CookiesTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_WithJourney_RendersExpectedContent()
    {
        // Arrange
        await WithJourneyCoordinatorAsync(
            CreateSignInJourneyState,
            async coordinator =>
            {
                // Arrange
                AddUrlToPath(coordinator, coordinator.Links.Cookies());
                var request = new HttpRequestMessage(HttpMethod.Get, coordinator.Links.Cookies());

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                var doc = await AssertEx.HtmlResponseAsync(response);
                Assert.Equal("Cookies - Test Service", doc.Title);

                // Check the page contains the expected cookie names and descriptions
                var content = doc.Body!.TextContent;

                Assert.Contains("sess", content);
                Assert.Contains("Stores your session information", content);
                Assert.Contains("2 hours", content);

                Assert.Contains("af", content);
                Assert.Contains("Cross-Site Request Forgery", content);
                Assert.Contains("close your browser", content);

                Assert.Contains("onelogin-correlation", content);
                Assert.Contains("Security token", content);
                Assert.Contains("15 minutes", content);

                Assert.Contains("onelogin-nonce", content);
                Assert.Contains("validate the authentication response", content);
            });
    }
}
