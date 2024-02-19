namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons;

public class IndexTests : TestBase
{
    public IndexTests(HostFixture hostFixture)
        : base(hostFixture)
    {
    }

    [Fact]
    public async Task Get_WithNoQueryParameters_DisplaysSearchFormOnly()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);

        var searchForm = doc.GetElementByTestId("search-form");
        Assert.NotNull(searchForm);
        var searchResults = doc.GetElementByTestId("search-results");
        Assert.Null(searchResults);
        var sortByForm = doc.GetElementByTestId("search-sortby-form");
        Assert.Null(searchResults);
    }

    [Fact]
    public async Task Get_WithEmptySearchParameter_DisplaysSearchFormOnly()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons?search=");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);

        var searchForm = doc.GetElementByTestId("search-form");
        Assert.NotNull(searchForm);
        var searchResults = doc.GetElementByTestId("search-results");
        Assert.Null(searchResults);
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
        var doc = await AssertEx.HtmlResponse(response);

        var searchForm = doc.GetElementByTestId("search-form");
        Assert.NotNull(searchForm);

        var searchInput = doc.GetElementByLabel("Search");
        Assert.NotNull(searchInput);
        Assert.Equal(search, searchInput!.GetAttribute("value"));

        var searchResults = doc.GetElementByTestId("search-results");
        Assert.NotNull(searchResults);

        var noMatches = searchResults!.GetElementByTestId("no-matches");
        Assert.NotNull(noMatches);
    }

    [Fact]
    public async Task Get_WithSearchThatLooksLikeADate_DisplaysMatchesOnDateOfBirth()
    {
        // Arrange
        var dateOfBirth = new DateOnly(1990, 1, 1);
        var person1 = await TestData.CreatePerson(b => b.WithDateOfBirth(dateOfBirth));
        var person2 = await TestData.CreatePerson(b => b.WithDateOfBirth(dateOfBirth));
        var person3 = await TestData.CreatePerson(b => b.WithDateOfBirth(dateOfBirth));
        var search = dateOfBirth.ToString("dd/MM/yyyy");

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons?search={search}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);

        var searchResults = doc.GetElementByTestId("search-results");
        Assert.NotNull(searchResults);

        var dateOfBirthResults = searchResults!.GetAllElementsByTestId("date-of-birth");
        Assert.NotNull(dateOfBirthResults);
        Assert.All(dateOfBirthResults.Select(r => r.TextContent), t => Assert.Equal(search, t));
    }

    [Fact]
    public async Task Get_WithSearchThatLooksLikeAName_DisplaysMatchesOnName()
    {
        // Arrange
        var name = "Andrew";
        var person1 = await TestData.CreatePerson(b => b.WithFirstName(name));
        var person2 = await TestData.CreatePerson(b => b.WithMiddleName(name));
        var person3 = await TestData.CreatePerson(b => b.WithLastName(name));
        var search = "andrew";

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons?search={search}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);

        var searchResults = doc.GetElementByTestId("search-results");
        Assert.NotNull(searchResults);

        var nameResults = searchResults!.GetAllElementsByTestId("name");
        Assert.NotNull(nameResults);
        Assert.All(nameResults.Select(r => r.TextContent), t => Assert.Contains(search, t.ToLower()));
    }

    [Fact]
    public async Task Get_WithSearchThatLooksLikeATrn_DisplaysMatchOnTrn()
    {
        // Arrange                
        var person1 = await TestData.CreatePerson(b => b.WithTrn());
        var person2 = await TestData.CreatePerson(b => b.WithTrn());
        var person3 = await TestData.CreatePerson(b => b.WithTrn());
        var search = person1.Trn;

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons?search={search}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);

        var searchResults = doc.GetElementByTestId("search-results");
        Assert.NotNull(searchResults);

        var nameResults = searchResults!.GetAllElementsByTestId("trn");
        Assert.Single(nameResults);
        Assert.Contains(search!, nameResults.Single().TextContent);
    }
}
