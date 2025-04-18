namespace TeachingRecordSystem.SupportUi.Tests.PageTests.ApiKeys;

public class EditApiKeyTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_UserDoesNotHavePermission_ReturnsForbidden()
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(role: null));

        var applicationUser = await TestData.CreateApplicationUserAsync();
        var apiKey = await TestData.CreateApiKeyAsync(applicationUser.UserId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/api-keys/{apiKey.Key}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_ApiKeyDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var apiKeyId = Guid.NewGuid();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/api-keys/{apiKeyId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_ValidRequest_RendersExpectedContent()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var apiKey = await TestData.CreateApiKeyAsync(applicationUser.UserId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/api-keys/{apiKey.ApiKeyId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(apiKey.Key, doc.GetElementById("Key")?.GetAttribute("value"));
    }

    [Fact]
    public async Task Get_KeyIsExpired_HasDisabledExpireButton()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var apiKey = await TestData.CreateApiKeyAsync(applicationUser.UserId, expired: true);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/api-keys/{apiKey.ApiKeyId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.NotNull(doc.GetElementByTestId("ExpireButton")?.GetAttribute("disabled"));
    }

    [Fact]
    public async Task Get_KeyIsNotExpired_HasNonDisabledExpireButton()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var apiKey = await TestData.CreateApiKeyAsync(applicationUser.UserId, expired: false);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/api-keys/{apiKey.ApiKeyId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Null(doc.GetElementByTestId("ExpireButton")?.GetAttribute("disabled"));
    }

    [Fact]
    public async Task PostExpire_UserDoesNotHavePermission_ReturnsForbidden()
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(role: null));

        var applicationUser = await TestData.CreateApplicationUserAsync();
        var apiKey = await TestData.CreateApiKeyAsync(applicationUser.UserId, expired: false);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/api-keys/{apiKey.ApiKeyId}/Expire");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Fact]
    public async Task PostExpire_KeyDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var apiKeyId = Guid.NewGuid();

        var request = new HttpRequestMessage(HttpMethod.Post, $"/api-keys/{apiKeyId}/Expire");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    //
    [Fact]
    public async Task PostExpire_KeyIsAlreadyExpired_ReturnsBadRequest()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var apiKey = await TestData.CreateApiKeyAsync(applicationUser.UserId, expired: true);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/api-keys/{apiKey.ApiKeyId}/Expire");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task PostExpire_ValidRequest_SetsExpiresOnApiKeyCreatesEventAndRedirectsToApplicationUserWithFlashMessage()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        var apiKey = await TestData.CreateApiKeyAsync(applicationUser.UserId, expired: false);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/api-keys/{apiKey.ApiKeyId}/Expire");

        EventPublisher.Clear();

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/application-users/{applicationUser.UserId}", response.Headers.Location?.OriginalString);

        await WithDbContext(async dbContext =>
        {
            apiKey = await dbContext.ApiKeys.SingleAsync(k => k.ApiKeyId == apiKey.ApiKeyId);
            Assert.Equal(Clock.UtcNow, apiKey.Expires);
        });

        EventPublisher.AssertEventsSaved(
            e =>
            {
                var apiKeyUpdatedEvent = Assert.IsType<ApiKeyUpdatedEvent>(e);
                Assert.Equal(Clock.UtcNow, apiKeyUpdatedEvent.CreatedUtc);
                Assert.Equal(GetCurrentUserId(), apiKeyUpdatedEvent.RaisedBy.UserId);
                Assert.Equal(ApiKeyUpdatedEventChanges.Expires, apiKeyUpdatedEvent.Changes);
                Assert.Null(apiKeyUpdatedEvent.OldApiKey.Expires);
                Assert.Equal(Clock.UtcNow, apiKeyUpdatedEvent.ApiKey.Expires);
            });

        var redirectResponse = await response.FollowRedirectAsync(HttpClient);
        var redirectDoc = await redirectResponse.GetDocumentAsync();
        AssertEx.HtmlDocumentHasFlashSuccess(redirectDoc, "API key expired");
    }
}
