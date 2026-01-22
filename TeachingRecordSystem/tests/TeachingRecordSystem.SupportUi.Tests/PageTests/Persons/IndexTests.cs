namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons;

public class IndexTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_WithNoCriteria_RedirectsToIndex()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/persons");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal("/", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_WithSearchQueryParameterWithNoMatches_DisplaysNoMatches()
    {
        // Arrange
        var uniqueSuffix = Guid.NewGuid().ToString();
        var search = $"smith{uniqueSuffix}";
        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons?search={search}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var searchInput = doc.GetElementByLabel("Search");
        Assert.NotNull(searchInput);
        Assert.Equal(search, searchInput.GetAttribute("value"));

        var noMatches = doc.GetElementByTestId("no-matches");
        Assert.NotNull(noMatches);
    }

    [Fact]
    public async Task Get_WithSearchThatLooksLikeADate_DisplaysMatchesOnDateOfBirth()
    {
        // Arrange
        var dateOfBirth = new DateOnly(1990, 1, 1);
        var person1 = await TestData.CreatePersonAsync(b => b.WithDateOfBirth(dateOfBirth));
        var person2 = await TestData.CreatePersonAsync(b => b.WithDateOfBirth(dateOfBirth));
        var person3 = await TestData.CreatePersonAsync(b => b.WithDateOfBirth(dateOfBirth));
        var search = dateOfBirth.ToString(WebConstants.DateOnlyDisplayFormat);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons?search={search}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var dateOfBirthResults = doc.GetAllElementsByTestId("date-of-birth");
        Assert.NotNull(dateOfBirthResults);
        Assert.All(dateOfBirthResults.Select(r => r.TrimmedText()), t => Assert.Equal(search, t));
    }

    [Fact]
    public async Task Get_WithSearchThatLooksLikeAName_DisplaysMatchesOnName()
    {
        // Arrange
        var name = "Andrew";
        var person1 = await TestData.CreatePersonAsync(b => b.WithFirstName(name));
        var person2 = await TestData.CreatePersonAsync(b => b.WithMiddleName(name));
        var person3 = await TestData.CreatePersonAsync(b => b.WithLastName(name));
        var search = "andrew";

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons?search={search}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var nameResults = doc.GetAllElementsByTestId("name");
        Assert.NotNull(nameResults);
        Assert.All(nameResults.Select(r => r.TrimmedText()), t => Assert.Contains(search, t, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Get_WithSearchThatLooksLikeATrn_DisplaysMatchOnTrn()
    {
        // Arrange
        var person1 = await TestData.CreatePersonAsync();
        var person2 = await TestData.CreatePersonAsync();
        var person3 = await TestData.CreatePersonAsync();
        var search = person1.Trn;

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons?search={search}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var nameResults = doc.GetAllElementsByTestId("trn");
        Assert.Single(nameResults);
        Assert.Contains(search!, nameResults.Single().TrimmedText());
    }
}
