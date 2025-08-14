namespace TeachingRecordSystem.AuthorizeAccess.Tests.PageTests.RequestTrn;

public class PreviousNameTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_HasPendingTrnRequestSetTrue_RedirectsToSubmitted()
    {
        // Arrange
        var state = CreateNewState();
        state.HasPendingTrnRequest = true;
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/request-trn/previous-name?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/submitted?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_NameMissingFromState_RedirectsToName()
    {
        // Arrange
        var state = CreateNewState();
        state.LastName = null;
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/request-trn/previous-name?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/name?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_ValidRequestWithPopulatedDataInJourneyState_PopulatesModelFromJourneyState()
    {
        // Arrange
        var state = CreateNewState();
        state.PreviousFirstName = TestData.GenerateFirstName();
        state.PreviousMiddleName = TestData.GenerateMiddleName();
        state.PreviousLastName = TestData.GenerateLastName();
        state.HasPreviousName = true;
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/request-trn/previous-name?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var radioButtons = doc.GetElementsByName("HasPreviousName");
        var selectedRadioButton = radioButtons.Single(r => r.HasAttribute("checked"));
        Assert.Equal("True", selectedRadioButton.GetAttribute("value"));
        Assert.Equal(state.PreviousFirstName, doc.GetElementById("FirstName")?.GetAttribute("value"));
        Assert.Equal(state.PreviousMiddleName, doc.GetElementById("MiddleName")?.GetAttribute("value"));
        Assert.Equal(state.PreviousLastName, doc.GetElementById("LastName")?.GetAttribute("value"));
    }

    [Fact]
    public async Task Post_HasPendingTrnRequestSetTrue_RedirectsToSubmitted()
    {
        // Arrange
        var state = CreateNewState();
        state.HasPendingTrnRequest = true;
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/request-trn/previous-name?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/submitted?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_NameMissingFromState_RedirectsToName()
    {
        // Arrange
        var state = CreateNewState();
        state.LastName = null;
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/request-trn/previous-name?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/name?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_WhenHasPreviousNameHasNoSelection_ReturnsError()
    {
        // Arrange
        var state = CreateNewState();

        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/request-trn/previous-name?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "HasPreviousName", "Select yes if you’ve ever changed your name");
    }

    [Fact]
    public async Task Post_WhenHasPreviousNameFalse_NamesAreEntered_NamesNotSaved()
    {
        // Arrange
        var state = CreateNewState();

        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/request-trn/previous-name?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["HasPreviousName"] = "False",
                ["FirstName"] = TestData.GenerateFirstName(),
                ["MiddleName"] = TestData.GenerateMiddleName(),
                ["LastName"] = TestData.GenerateLastName()
            }),
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        var reloadedJourneyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Null(reloadedJourneyInstance.State.PreviousFirstName);
        Assert.Null(reloadedJourneyInstance.State.PreviousMiddleName);
        Assert.Null(reloadedJourneyInstance.State.PreviousLastName);
    }

    [Fact]
    public async Task Post_WhenHasPreviousNameIsTrueAndPreviousNameIsEmpty_ReturnsError()
    {
        // Arrange
        var state = CreateNewState();

        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/request-trn/previous-name?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["HasPreviousName"] = "True",
            }),
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "FirstName", "Enter your previous first name");
        await AssertEx.HtmlResponseHasErrorAsync(response, "LastName", "Enter your previous last name");
    }

    [Fact]
    public async Task Post_WhenHasPreviousNameIsFalseAndPreviousNameIsEmpty_UpdatesStateAndRedirectsToNextPage()
    {
        // Arrange
        var state = CreateNewState();

        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/request-trn/previous-name?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["HasPreviousName"] = "False",
            }),
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/date-of-birth?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);

        var reloadedJourneyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.False(reloadedJourneyInstance.State.HasPreviousName);
    }

    [Fact]
    public async Task Post_ValidRequest_UpdatesStateAndRedirectsToNextPage()
    {
        var state = CreateNewState();

        var journeyInstance = await CreateJourneyInstance(state);

        var previousFirstName = TestData.GenerateFirstName();
        var previousMiddleName = TestData.GenerateMiddleName();
        var previousLastName = TestData.GenerateLastName();
        var request = new HttpRequestMessage(HttpMethod.Post, $"/request-trn/previous-name?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["HasPreviousName"] = "True",
                ["FirstName"] = previousFirstName,
                ["MiddleName"] = previousMiddleName,
                ["LastName"] = previousLastName
            }),
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-trn/date-of-birth?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);

        var reloadedJourneyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(previousFirstName, reloadedJourneyInstance.State.PreviousFirstName);
        Assert.Equal(previousMiddleName, reloadedJourneyInstance.State.PreviousMiddleName);
        Assert.Equal(previousLastName, reloadedJourneyInstance.State.PreviousLastName);
        Assert.True(reloadedJourneyInstance.State.HasPreviousName);
    }
}
