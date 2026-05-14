namespace TeachingRecordSystem.AuthorizeAccess.Tests.PageTests;

public class CookiesTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_WithJourney_RendersPage()
    {
        // Arrange
        await WithJourneyCoordinatorAsync(
            CreateSignInJourneyState,
            async coordinator =>
            {
                var request = new HttpRequestMessage(HttpMethod.Get, coordinator.Links.Cookies());

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                await AssertEx.HtmlResponseAsync(response);
            });
    }
}
