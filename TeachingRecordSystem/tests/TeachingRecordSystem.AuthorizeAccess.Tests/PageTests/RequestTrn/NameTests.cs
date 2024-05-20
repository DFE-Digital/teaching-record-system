namespace TeachingRecordSystem.AuthorizeAccess.Tests.PageTests.RequestTrn;

public class NameTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_EmailMissingFromState_RedirectsToEmail()
    {
        // Arrange
        var state = CreateNewState();
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/request-trn/name?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/email?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_ValidRequestWithPopulatedDataInJourneyState_PopulatesModelFromJourneyState()
    {
        // Arrange
        var state = CreateNewState();
        state.Email = Faker.Internet.Email();
        state.Name = Faker.Name.FullName();
        state.PreviousName = Faker.Name.FullName();
        state.HasPreviousName = true;
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/request-trn/name?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);
        Assert.Equal(state.Name, doc.GetElementById("Name")?.GetAttribute("value"));
        var radioButtons = doc.GetElementsByName("HasPreviousName");
        var selectedRadioButton = radioButtons.Single(r => r.HasAttribute("checked"));
        Assert.Equal("True", selectedRadioButton.GetAttribute("value"));
        Assert.Equal(state.PreviousName, doc.GetElementById("PreviousName")?.GetAttribute("value"));
    }

    [Fact]
    public async Task Post_EmptyNameEntered_ReturnsError()
    {
        // Arrange
        var state = CreateNewState();
        state.Email = Faker.Internet.Email();
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/request-trn/name?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["HasPreviousName"] = "False",
            }),
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "Name", "Enter your name");
    }

    [Fact]
    public async Task Post_WhenHasPreviousNameHasNoSelection_ReturnsError()
    {
        // Arrange
        var state = CreateNewState();
        state.Email = Faker.Internet.Email();
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/request-trn/name?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["Name"] = Faker.Name.FullName(),
            }),
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "HasPreviousName", "Tell us if you have a previous name");
    }

    [Fact]
    public async Task Post_WhenHasPreviousNameIsTrueAndPreviousNameIsEmpty_ReturnsError()
    {
        // Arrange
        var state = CreateNewState();
        state.Email = Faker.Internet.Email();
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/request-trn/name?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["Name"] = Faker.Name.FullName(),
                ["HasPreviousName"] = "True",
            }),
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "PreviousName", "Tell us your previous name");
    }

    [Fact]
    public async Task Post_EmailMissingFromState_RedirectsToEmail()
    {
        // Arrange
        var state = CreateNewState();
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/request-trn/name?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["Name"] = Faker.Name.FullName(),
                ["HasPreviousName"] = "False",
            }),
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/email?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_ValidRequest_UpdatesStateAndRedirectsToNextPage()
    {
        // Arrange
        var state = CreateNewState();
        state.Email = Faker.Internet.Email();
        var journeyInstance = await CreateJourneyInstance(state);

        var name = Faker.Name.FullName();
        var previousName = Faker.Name.FullName();
        var request = new HttpRequestMessage(HttpMethod.Post, $"/request-trn/name?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["Name"] = name,
                ["HasPreviousName"] = "True",
                ["PreviousName"] = previousName,
            }),
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/date-of-birth?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);

        var reloadedJourneyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(name, reloadedJourneyInstance.State.Name);
        Assert.Equal(previousName, reloadedJourneyInstance.State.PreviousName);
        Assert.True(reloadedJourneyInstance.State.HasPreviousName);
    }
}
