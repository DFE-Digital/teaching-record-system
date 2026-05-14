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

                // Check each cookie row contains the expected information
                var sessCookieRow = doc.GetElementByTestId("cookie-sess");
                Assert.NotNull(sessCookieRow);
                Assert.Contains("sess", sessCookieRow.TextContent);
                Assert.Contains("Stores your session information", sessCookieRow.TextContent);
                Assert.Contains("close your browser", sessCookieRow.TextContent);

                var afCookieRow = doc.GetElementByTestId("cookie-af");
                Assert.NotNull(afCookieRow);
                Assert.Contains("af", afCookieRow.TextContent);
                Assert.Contains("Cross-Site Request Forgery", afCookieRow.TextContent);
                Assert.Contains("close your browser", afCookieRow.TextContent);

                var correlationCookieRow = doc.GetElementByTestId("cookie-onelogin-correlation");
                Assert.NotNull(correlationCookieRow);
                Assert.Contains("onelogin-correlation", correlationCookieRow.TextContent);
                Assert.Contains("Security token", correlationCookieRow.TextContent);
                Assert.Contains("prevent unauthorized access", correlationCookieRow.TextContent);
                Assert.Contains("close your browser", correlationCookieRow.TextContent);

                var nonceCookieRow = doc.GetElementByTestId("cookie-onelogin-nonce");
                Assert.NotNull(nonceCookieRow);
                Assert.Contains("onelogin-nonce", nonceCookieRow.TextContent);
                Assert.Contains("Security token", nonceCookieRow.TextContent);
                Assert.Contains("validate the authentication response", nonceCookieRow.TextContent);
                Assert.Contains("close your browser", nonceCookieRow.TextContent);
            });
    }
}
