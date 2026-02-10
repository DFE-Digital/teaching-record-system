using Optional;

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

    [Fact]
    public async Task Get_WithSearchThatLooksLikeAnEmailAddress_DisplaysMatchOnOneLoginUserEmail()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var emailAddress = Faker.Internet.Email();
        await TestData.CreateOneLoginUserAsync(person, email: Option.Some<string?>(emailAddress));
        var search = emailAddress;

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons?search={search}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var personResults = doc.GetAllElementsByTestId($"person-{person.PersonId}");
        Assert.Single(personResults);

        var emailResults = doc.GetAllElementsByTestId("one-login-emails");
        Assert.Single(emailResults);
        Assert.Contains(emailAddress, emailResults.Single().TextContent);
    }

    [Fact]
    public async Task Get_WithIncludeActiveFilter_DisplaysOnlyActivePersons()
    {
        // Arrange
        var searchName = $"FilterTest{Guid.NewGuid()}";
        var activePerson1 = await TestData.CreatePersonAsync(b => b.WithFirstName(searchName));
        var activePerson2 = await TestData.CreatePersonAsync(b => b.WithFirstName(searchName));
        var deactivatedPerson = await TestData.CreatePersonAsync(b => b.WithFirstName(searchName));

        await WithDbContextAsync(dbContext =>
            dbContext.Persons
                .Where(p => p.PersonId == deactivatedPerson.PersonId)
                .ExecuteUpdateAsync(u => u.SetProperty(p => p.Status, _ => PersonStatus.Deactivated)));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons?search={searchName}&includeActive=true");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.NotNull(doc.GetElementByTestId($"person-{activePerson1.PersonId}"));
        Assert.NotNull(doc.GetElementByTestId($"person-{activePerson2.PersonId}"));
        Assert.Null(doc.GetElementByTestId($"person-{deactivatedPerson.PersonId}"));

        var statusResults = doc.GetAllElementsByTestId("status");
        Assert.All(statusResults.Select(r => r.TrimmedText()), t => Assert.Equal("Active", t));
    }

    [Fact]
    public async Task Get_WithIncludeDeactivatedFilter_DisplaysOnlyDeactivatedPersons()
    {
        // Arrange
        var searchName = $"FilterTest{Guid.NewGuid()}";
        var activePerson = await TestData.CreatePersonAsync(b => b.WithFirstName(searchName));
        var deactivatedPerson1 = await TestData.CreatePersonAsync(b => b.WithFirstName(searchName));
        var deactivatedPerson2 = await TestData.CreatePersonAsync(b => b.WithFirstName(searchName));

        await WithDbContextAsync(dbContext =>
            dbContext.Persons
                .Where(p => p.PersonId == deactivatedPerson1.PersonId)
                .ExecuteUpdateAsync(u => u.SetProperty(p => p.Status, _ => PersonStatus.Deactivated)));
        await WithDbContextAsync(dbContext =>
            dbContext.Persons
                .Where(p => p.PersonId == deactivatedPerson2.PersonId)
                .ExecuteUpdateAsync(u => u.SetProperty(p => p.Status, _ => PersonStatus.Deactivated)));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons?search={searchName}&includeDeactivated=true");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.NotNull(doc.GetElementByTestId($"person-{deactivatedPerson1.PersonId}"));
        Assert.NotNull(doc.GetElementByTestId($"person-{deactivatedPerson2.PersonId}"));
        Assert.Null(doc.GetElementByTestId($"person-{activePerson.PersonId}"));

        var statusResults = doc.GetAllElementsByTestId("status");
        Assert.All(statusResults.Select(r => r.TrimmedText()), t => Assert.Equal("Deactivated", t));
    }

    [Fact]
    public async Task Get_WithIncludeOneLoginUserFilter_DisplaysOnlyPersonsWithOneLoginUser()
    {
        // Arrange
        var searchName = $"FilterTest{Guid.NewGuid()}";
        var personWithOneLogin1 = await TestData.CreatePersonAsync(b => b.WithFirstName(searchName));
        var personWithOneLogin2 = await TestData.CreatePersonAsync(b => b.WithFirstName(searchName));
        var personWithoutOneLogin = await TestData.CreatePersonAsync(b => b.WithFirstName(searchName));

        await TestData.CreateOneLoginUserAsync(personWithOneLogin1, email: Option.Some<string?>(Faker.Internet.Email()));
        await TestData.CreateOneLoginUserAsync(personWithOneLogin2, email: Option.Some<string?>(Faker.Internet.Email()));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons?search={searchName}&includeOneLoginUser=true");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.NotNull(doc.GetElementByTestId($"person-{personWithOneLogin1.PersonId}"));
        Assert.NotNull(doc.GetElementByTestId($"person-{personWithOneLogin2.PersonId}"));
        Assert.Null(doc.GetElementByTestId($"person-{personWithoutOneLogin.PersonId}"));
    }

    [Fact]
    public async Task Get_WithCombinedFilters_DisplaysOnlyMatchingPersons()
    {
        // Arrange
        var searchName = $"FilterTest{Guid.NewGuid()}";
        var activePersonWithOneLogin = await TestData.CreatePersonAsync(b => b.WithFirstName(searchName));
        var activePersonWithoutOneLogin = await TestData.CreatePersonAsync(b => b.WithFirstName(searchName));
        var deactivatedPersonWithOneLogin = await TestData.CreatePersonAsync(b => b.WithFirstName(searchName));
        var deactivatedPersonWithoutOneLogin = await TestData.CreatePersonAsync(b => b.WithFirstName(searchName));

        await TestData.CreateOneLoginUserAsync(activePersonWithOneLogin, email: Option.Some<string?>(Faker.Internet.Email()));
        await TestData.CreateOneLoginUserAsync(deactivatedPersonWithOneLogin, email: Option.Some<string?>(Faker.Internet.Email()));

        await WithDbContextAsync(dbContext =>
            dbContext.Persons
                .Where(p => p.PersonId == deactivatedPersonWithOneLogin.PersonId)
                .ExecuteUpdateAsync(u => u.SetProperty(p => p.Status, _ => PersonStatus.Deactivated)));
        await WithDbContextAsync(dbContext =>
            dbContext.Persons
                .Where(p => p.PersonId == deactivatedPersonWithoutOneLogin.PersonId)
                .ExecuteUpdateAsync(u => u.SetProperty(p => p.Status, _ => PersonStatus.Deactivated)));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons?search={searchName}&includeActive=true&includeOneLoginUser=true");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.NotNull(doc.GetElementByTestId($"person-{activePersonWithOneLogin.PersonId}"));
        Assert.Null(doc.GetElementByTestId($"person-{activePersonWithoutOneLogin.PersonId}"));
        Assert.Null(doc.GetElementByTestId($"person-{deactivatedPersonWithOneLogin.PersonId}"));
        Assert.Null(doc.GetElementByTestId($"person-{deactivatedPersonWithoutOneLogin.PersonId}"));
    }
}
