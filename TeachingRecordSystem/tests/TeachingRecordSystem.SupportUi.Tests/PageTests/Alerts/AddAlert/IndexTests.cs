namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Alerts.AddAlert;

public class IndexTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_RedirectsToAlertAddType()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/add?personId={person.PersonId}");

        // Act
        var response = await HttpClient.SendAsync(request);  // Initializes journey
        response = await response.FollowRedirectAsync(HttpClient);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/add/type?personId={person.PersonId}", response.Headers.Location?.OriginalString);
    }
}
