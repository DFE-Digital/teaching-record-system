namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Alerts.EditAlert.Link;

public class IndexTests : LinkTestBase
{
    private const string PreviousStep = JourneySteps.New;
    private const string ThisStep = JourneySteps.Index;

    public IndexTests(HostFixture hostFixture) : base(hostFixture)
    {
        SetCurrentUser(TestUsers.GetUser(UserRoles.AlertsManagerTraDbs));
    }

    [Theory]
    [RolesWithoutAlertWritePermissionData]
    public async Task Get_UserDoesNotHavePermission_ReturnsForbidden(string? role)
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(role));

        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateEmptyJourneyInstanceAsync(alert.AlertId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alert.AlertId}/link?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_WithAlertIdForNonExistentAlert_ReturnsNotFound()
    {
        // Arrange
        var alertId = Guid.NewGuid();
        var journeyInstance = await CreateEmptyJourneyInstanceAsync(alertId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alertId}/link?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_WithClosedAlert_ReturnsBadRequest()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithClosedAlert();
        var journeyInstance = await CreateEmptyJourneyInstanceAsync(alert.AlertId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alert.AlertId}/link?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_ValidRequestWithUninitializedJourneyState_PopulatesModelFromDatabase()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateEmptyJourneyInstanceAsync(alert.AlertId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alert.AlertId}/link?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(alert.ExternalLink, doc.GetElementById("Link")?.GetAttribute("value"));
    }

    [Fact]
    public async Task Get_ValidRequestWithInitializedJourneyState_PopulatesModelFromJourneyState()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(ThisStep, alert);


        var request = new HttpRequestMessage(HttpMethod.Get, $"/alerts/{alert.AlertId}/link?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(journeyInstance.State.Link, doc.GetElementById("Link")?.GetAttribute("value"));
    }

    [Theory]
    [RolesWithoutAlertWritePermissionData]
    public async Task Post_UserDoesNotHavePermission_ReturnsForbidden(string? role)
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(role));

        var (person, alert) = await CreatePersonWithOpenAlert();
        var newLink = TestData.GenerateUrl();
        var journeyInstance = await CreateJourneyInstanceForCompletedStepAsync(PreviousStep, alert);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/link?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = CreatePostContent(true, newLink)
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_WithAlertIdForNonExistentAlert_ReturnsNotFound()
    {
        // Arrange
        var alertId = Guid.NewGuid();
        var journeyInstance = await CreateEmptyJourneyInstanceAsync(alertId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alertId}/link?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_WithClosedAlert_ReturnsBadRequest()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithClosedAlert();
        var journeyInstance = await CreateEmptyJourneyInstanceAsync(alert.AlertId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/link?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = CreatePostContent(true, TestData.GenerateUrl())
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Post_NoAddLinkOptionSelected_ReturnsError(bool hasCurrentLink)
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert(populateOptional: hasCurrentLink);
        var journeyInstance = await CreateEmptyJourneyInstanceAsync(alert.AlertId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/link?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = CreatePostContent(null, null)
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        if (hasCurrentLink)
        {
            await AssertEx.HtmlResponseHasErrorAsync(response, "AddLink", "Select change link if you want to change the link to the panel outcome");
        }
        else
        {
            await AssertEx.HtmlResponseHasErrorAsync(response, "AddLink", "Select yes if you want to add a link to a panel outcome");
        }
    }

    [Theory]
    [InlineData("invalid url")]
    [InlineData(null)]
    public async Task Post_WithInvalidLinkUrl_ReturnsError(string? invalidLink)
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateEmptyJourneyInstanceAsync(alert.AlertId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/link?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = CreatePostContent(true, invalidLink)
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "Link", "Enter a valid URL");
    }

    [Fact]
    public async Task Post_WithUnchangedLink_ReturnsError()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert();
        var journeyInstance = await CreateEmptyJourneyInstanceAsync(alert.AlertId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/link?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = CreatePostContent(true, alert.ExternalLink)
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "Link", "Enter a different link");
    }

    [Fact]
    public async Task Post_WithNoCurrentLinkAndAddLinkOptionNoSelected_RedirectsToPersonAlertsPage()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert(populateOptional: false);
        var journeyInstance = await CreateEmptyJourneyInstanceAsync(alert.AlertId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/link?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = CreatePostContent(false, null)
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/persons/{person.PersonId}/alerts", response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Null(journeyInstance);
    }

    [Fact]
    public async Task Post_WithLink_UpdatesStateAndRedirectsToChangeReasonPage()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert(populateOptional: false);
        var journeyInstance = await CreateEmptyJourneyInstanceAsync(alert.AlertId);
        var newLink = TestData.GenerateUrl();

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/link?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = CreatePostContent(true, newLink)
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/{alert.AlertId}/link/change-reason", response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.True(journeyInstance.State.AddLink);
        Assert.Equal(newLink, journeyInstance.State.Link);
    }

    [Fact]
    public async Task Post_WithNoLink_UpdatesStateAndRedirectsToChangeReasonPage()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert(populateOptional: true);
        var journeyInstance = await CreateEmptyJourneyInstanceAsync(alert.AlertId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/link?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = CreatePostContent(false, null)
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/alerts/{alert.AlertId}/link/change-reason", response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.False(journeyInstance.State.AddLink);
        Assert.Null(journeyInstance.State.Link);
    }

    [Fact]
    public async Task Post_Cancel_DeletesJourneyAndRedirects()
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert(populateOptional: true);
        var journeyInstance = await CreateEmptyJourneyInstanceAsync(alert.AlertId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/alerts/{alert.AlertId}/link/cancel?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/persons/{person.PersonId}/alerts", response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Null(journeyInstance);
    }

    [Theory]
    [MemberData(nameof(HttpMethods), TestHttpMethods.GetAndPost)]
    public async Task PersonIsDeactivated_ReturnsBadRequest(HttpMethod httpMethod)
    {
        // Arrange
        var (person, alert) = await CreatePersonWithOpenAlert(populateOptional: true);
        await WithDbContext(async dbContext =>
        {
            dbContext.Attach(person.Person);
            person.Person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });
        var journeyInstance = await CreateEmptyJourneyInstanceAsync(alert.AlertId);

        var request = new HttpRequestMessage(httpMethod, $"/alerts/{alert.AlertId}/link?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    private static FormUrlEncodedContentBuilder CreatePostContent(bool? addLink, string? newLink)
    {
        var builder = new FormUrlEncodedContentBuilder();

        if (addLink is not null)
        {
            builder.Add("AddLink", addLink.Value.ToString());
        }

        if (newLink is not null)
        {
            builder.Add("Link", newLink);
        }

        return builder;
    }
}
