namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Mqs.AddMq;

public class IndexTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Test]
    public async Task Get_RedirectsToMqAddProvider()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/add?personId={person.PersonId}");

        // Act
        var response = await HttpClient.SendAsync(request);  // Initializes journey
        response = await response.FollowRedirectAsync(HttpClient);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/mqs/add/provider?personId={person.PersonId}", response.Headers.Location?.OriginalString);
    }
}
