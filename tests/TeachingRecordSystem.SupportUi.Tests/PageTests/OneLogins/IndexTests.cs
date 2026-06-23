using Optional;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.OneLogins;

[ClearDbBeforeTest, Collection(nameof(DisableParallelization))]
public class IndexTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_WithNoCriteria_RedirectsToIndex()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/one-logins");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal("/?selectedTab=one-logins", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_WithSearchQueryParameterWithNoMatches_DisplaysNoMatches()
    {
        // Arrange
        var uniqueSuffix = Guid.NewGuid().ToString();
        var search = $"smith{uniqueSuffix}";
        var request = new HttpRequestMessage(HttpMethod.Get, $"/one-logins?search={search}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var searchInput = doc.GetElementById("Search");
        Assert.NotNull(searchInput);
        Assert.Equal(search, searchInput.GetAttribute("value"));

        var noMatches = doc.GetElementByTestId("no-matches");
        Assert.NotNull(noMatches);
    }

    [Fact]
    public async Task Get_WithResult_DisplaysExpectedDataInResultsTable()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithFirstName("Test")
            .WithLastName("User")
            .WithDateOfBirth(new DateOnly(1990, 5, 15)));
        var emailAddress = Faker.Internet.Email();

        var oneLoginUser = await TestData.CreateOneLoginUserAsync(
            person,
            email: Option.Some<string?>(emailAddress));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/one-logins?search={emailAddress}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Null(doc.GetElementByTestId("no-matches"));

        var emailResults = doc.GetAllElementsByTestId("email");
        Assert.Single(emailResults);
        Assert.Equal(emailAddress, emailResults.Single().TrimmedText());

        var nameResults = doc.GetAllElementsByTestId("name");
        Assert.Single(nameResults);
        Assert.Equal("Test User", nameResults.Single().TrimmedText());

        var dobResults = doc.GetAllElementsByTestId("date-of-birth");
        Assert.Single(dobResults);
        Assert.Equal("15 May 1990", dobResults.Single().TrimmedText());

        var trnResults = doc.GetAllElementsByTestId("trn");
        Assert.Single(trnResults);
        Assert.Equal(person.Trn, trnResults.Single().TrimmedText());
    }

    [Fact]
    public async Task Get_WithOrphanedOneLoginUser_DisplaysNotProvidedForTrn()
    {
        // Arrange
        var emailAddress = Faker.Internet.Email();
        var name = new[] { "Test", "User" };
        var dateOfBirth = new DateOnly(1990, 5, 15);

        var oneLoginUser = await TestData.CreateOneLoginUserAsync(
            personId: null,
            email: Option.Some<string?>(emailAddress),
            verifiedInfo: (name, dateOfBirth));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/one-logins?search={emailAddress}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var trnResults = doc.GetAllElementsByTestId("trn");
        Assert.Single(trnResults);
        Assert.Equal(WebConstants.EmptyFallbackContent, trnResults.Single().TrimmedText());
    }

    [Fact]
    public async Task Get_WithSearchThatLooksLikeAnEmailAddress_DisplaysMatch()
    {
        // Arrange
        var emailAddress = Faker.Internet.Email();
        var name = new[] { Faker.Name.First(), Faker.Name.Last() };
        var dateOfBirth = new DateOnly(1990, 1, 1);
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(
            personId: null,
            email: Option.Some<string?>(emailAddress),
            verifiedInfo: (name, dateOfBirth));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/one-logins?search={emailAddress}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var emailResults = doc.GetAllElementsByTestId("email");
        Assert.Single(emailResults);
    }

    [Fact]
    public async Task Get_WithMultiplePages_DisplaysPaginationControls()
    {
        // Arrange
        var pageSize = 20;
        var page = 2;

        await Enumerable.Range(1, (pageSize * page) + 1)
            .ToAsyncEnumerable()
            .Select(async (int i, CancellationToken _) =>
            {
                var person = await TestData.CreatePersonAsync(p => p
                    .WithFirstName($"User{i}")
                    .WithLastName("Test"));
                return await TestData.CreateOneLoginUserAsync(
                    person,
                    email: Option.Some<string?>($"user{i}@example.com"));
            })
            .ToArrayAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, "/one-logins?search=test");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var pagination = doc.QuerySelector(".govuk-pagination");
        Assert.NotNull(pagination);

        var emailResults = doc.GetAllElementsByTestId("email");
        Assert.Equal(pageSize, emailResults.Count);
    }
}
