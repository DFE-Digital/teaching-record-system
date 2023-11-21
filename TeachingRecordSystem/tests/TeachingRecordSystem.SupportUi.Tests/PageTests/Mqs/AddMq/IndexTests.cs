namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Mqs.AddMq;

public class IndexTests : TestBase
{
    public IndexTests(HostFixture hostFixture)
        : base(hostFixture)
    {
    }

    [Fact]
    public async Task Get_RedirectsToMqAddProvider()
    {
        // Arrange
        var person = await TestData.CreatePerson();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/add?personId={person.PersonId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/mqs/add/provider?personId={person.PersonId}", response.Headers.Location?.OriginalString);
    }
}
