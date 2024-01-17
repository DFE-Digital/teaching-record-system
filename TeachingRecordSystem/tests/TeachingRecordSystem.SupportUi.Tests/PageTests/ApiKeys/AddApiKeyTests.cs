using Microsoft.EntityFrameworkCore;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.ApiKeys;

public class AddApiKeyTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_UserDoesNotHavePermission_ReturnsForbidden()
    {
        // Arrange
        SetCurrentUser(TestUsers.NoRoles);

        var applicationUser = await TestData.CreateApplicationUser();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/api-keys/add?applicationUserId={applicationUser.UserId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_ApplicationUserDoesNotExist_ReturnsBadRequest()
    {
        // Arrange
        var applicationUserId = await TestData.CreateApplicationUser();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/api-keys/add?applicationUserId={applicationUserId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
        await response.GetDocument();
    }

    [Fact]
    public async Task Get_ValidRequest_RendersExpectedContent()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUser();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/api-keys/add?applicationUserId={applicationUser.UserId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        await response.GetDocument();
    }

    [Fact]
    public async Task Post_UserDoesNotHavePermission_ReturnsForbidden()
    {
        // Arrange
        SetCurrentUser(TestUsers.NoRoles);

        var applicationUser = await TestData.CreateApplicationUser();

        var key = new string('a', 20);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/api-keys/add?applicationUserId={applicationUser.UserId}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "Key", key }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_ApplicationUserDoesNotExist_ReturnsBadRequest()
    {
        // Arrange
        var applicationUserId = Guid.NewGuid();

        var key = new string('a', 20);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/api-keys/add?applicationUserId={applicationUserId}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "Key", key }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_KeyIsTooLong_RendersErrorMessage()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUser();

        var key = new string('a', ApiKey.KeyMaxLength + 1);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/api-keys/add?applicationUserId={applicationUser.UserId}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "Key", key }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "Key", "Key must be 100 characters or less");
    }

    [Fact]
    public async Task Post_KeyIsTooShort_RendersErrorMessage()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUser();

        var key = new string('a', ApiKey.KeyMinLength - 1);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/api-keys/add?applicationUserId={applicationUser.UserId}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "Key", key }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "Key", "Key must be at least 20 characters");
    }

    [Fact]
    public async Task Post_KeyAlreadyExists_RendersErrorMessage()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUser();
        var anotherKey = await TestData.CreateApiKey(applicationUser.UserId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/api-keys/add?applicationUserId={applicationUser.UserId}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "Key", anotherKey.Key }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "Key", "Key is already in use");
    }

    [Fact]
    public async Task Post_ValidRequest_CreatesApiKeyCreatesEventAndRedirectsToApplicationUserWithFlashMessage()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUser();

        var key = new string('a', 20);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/api-keys/add?applicationUserId={applicationUser.UserId}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "Key", key }
            }
        };

        EventObserver.Clear();

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/application-users/{applicationUser.UserId}", response.Headers.Location?.OriginalString);

        var apiKey = await WithDbContext(async dbContext =>
        {
            var apiKey = await dbContext.ApiKeys.Where(k => k.ApplicationUserId == applicationUser.UserId).SingleOrDefaultAsync();
            Assert.NotNull(apiKey);
            Assert.Equal(Clock.UtcNow, apiKey.CreatedOn);
            Assert.Equal(Clock.UtcNow, apiKey.UpdatedOn);
            Assert.Equal(key, apiKey.Key);

            return apiKey;
        });

        EventObserver.AssertEventsSaved(
            e =>
            {
                var apiKeyCreatedEvent = Assert.IsType<ApiKeyCreatedEvent>(e);
                Assert.Equal(Clock.UtcNow, apiKeyCreatedEvent.CreatedUtc);
                Assert.Equal(GetCurrentUserId(), apiKeyCreatedEvent.RaisedBy.UserId);
                Assert.Equal(apiKey.ApiKeyId, apiKeyCreatedEvent.ApiKey.ApiKeyId);
                Assert.Equal(apiKey.ApplicationUserId, applicationUser.UserId);
                Assert.Equal(apiKey.Key, apiKeyCreatedEvent.ApiKey.Key);
                Assert.Equal(apiKey.Expires, apiKeyCreatedEvent.ApiKey.Expires);
            });

        var redirectResponse = await response.FollowRedirect(HttpClient);
        var redirectDoc = await redirectResponse.GetDocument();
        AssertEx.HtmlDocumentHasFlashSuccess(redirectDoc, "API key added");
    }
}
