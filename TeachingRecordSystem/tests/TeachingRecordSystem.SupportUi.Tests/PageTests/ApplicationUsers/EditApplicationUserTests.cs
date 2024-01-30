using Microsoft.EntityFrameworkCore;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.ApplicationUsers;

public class EditApplicationUserTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_UserDoesNotHavePermission_ReturnsForbidden()
    {
        // Arrange
        SetCurrentUser(TestUsers.NoRoles);

        var applicationUser = await TestData.CreateApplicationUser(apiRoles: []);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/application-users/{applicationUser.UserId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_UserDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var applicationUserId = Guid.NewGuid();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/application-users/{applicationUserId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_ValidRequest_RendersExpectedContent()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUser(apiRoles: [ApiRoles.GetPerson, ApiRoles.UpdatePerson], hasOneLoginSettings: true);
        var apiKeyUnexpired = await TestData.CreateApiKey(applicationUser.UserId, expired: false);
        var apiKeyExpired = await TestData.CreateApiKey(applicationUser.UserId, expired: true);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/application-users/{applicationUser.UserId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        var doc = await response.GetDocument();

        Assert.NotNull(doc.GetElementByLabel(ApiRoles.GetPerson)?.GetAttribute("checked"));
        Assert.NotNull(doc.GetElementByLabel(ApiRoles.UpdatePerson)?.GetAttribute("checked"));

        var apiKeysTable = doc.GetElementByTestId("ApiKeysTable")!;
        var keyRows = apiKeysTable.GetElementsByTagName("tbody").Single().GetElementsByTagName("tr");

        Assert.Collection(
            keyRows,
            row =>
            {
                var expiry = row.GetElementByTestId("Expiry")?.TextContent?.Trim();
                Assert.Equal("No expiration", expiry);
            },
            row =>
            {
                var expiry = row.GetElementByTestId("Expiry")?.TextContent?.Trim();
                Assert.Equal(apiKeyExpired.Expires!.Value.ToString("dd/MM/yyyy HH:mm"), expiry);
            });

        Assert.Equal(applicationUser.OneLoginClientId, doc.GetElementById("OneLoginClientId")?.GetAttribute("value"));
        Assert.Equal(applicationUser.OneLoginPrivateKeyPem, doc.GetElementById("OneLoginPrivateKeyPem")?.TextContent?.Trim());
    }

    [Fact]
    public async Task Post_UserDoesNotHavePermission_ReturnsForbidden()
    {
        // Arrange
        SetCurrentUser(TestUsers.NoRoles);

        var applicationUser = await TestData.CreateApplicationUser(apiRoles: []);
        var originalName = applicationUser.Name;
        var newName = TestData.GenerateChangedApplicationUserName(originalName);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/application-users/{applicationUser.UserId}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "Name", newName }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_UserDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var applicationUserId = Guid.NewGuid();
        var newName = "name";

        var request = new HttpRequestMessage(HttpMethod.Post, $"/application-users/{applicationUserId}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "Name", newName }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_NameNotProvided_RendersError()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUser(apiRoles: []);
        var originalName = applicationUser.Name;
        var newName = "";

        var request = new HttpRequestMessage(HttpMethod.Post, $"/application-users/{applicationUser.UserId}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "Name", newName }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "Name", "Enter a name");
    }

    [Fact]
    public async Task Post_NameTooLong_RendersError()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUser(apiRoles: []);
        var originalName = applicationUser.Name;
        var newName = new string('x', UserBase.NameMaxLength + 1);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/application-users/{applicationUser.UserId}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "Name", newName }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "Name", "Name must be 200 characters or less");
    }

    [Fact]
    public async Task Post_OneLoginClientIdButNoPem_RendersError()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUser(apiRoles: []);
        var oneLoginClientId = Guid.NewGuid().ToString();

        var request = new HttpRequestMessage(HttpMethod.Post, $"/application-users/{applicationUser.UserId}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "Name", applicationUser.Name },
                { "OneLoginClientId", oneLoginClientId }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "OneLoginPrivateKeyPem", "One Login Private Key PEM is required if One Login Client ID is set");
    }

    [Fact]
    public async Task Post_OneLoginClientIdTooLong_RendersError()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUser(apiRoles: []);
        var oneLoginClientId = new string('x', ApplicationUser.OneLoginClientIdMaxLength + 1);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/application-users/{applicationUser.UserId}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "Name", applicationUser.Name },
                { "OneLoginClientId", oneLoginClientId }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "OneLoginClientId", "One Login Client ID must be 50 characters or less");
    }

    [Fact]
    public async Task Post_OneLoginPrivateKeyPemButNoClientId_RendersError()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUser(apiRoles: []);
        var oneLoginPrivateKeyPem = TestData.GeneratePrivateKeyPem();

        var request = new HttpRequestMessage(HttpMethod.Post, $"/application-users/{applicationUser.UserId}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "Name", applicationUser.Name },
                { "OneLoginPrivateKeyPem", oneLoginPrivateKeyPem! }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "OneLoginClientId", "One Login Client ID is required if One Login Private Key PEM is set");
    }

    [Fact]
    public async Task Post_ValidRequest_UpdatesNameAndRolesAndOneLoginSettinsAndCreatesEventAndRedirectsWithFlashMessage()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUser(apiRoles: [], hasOneLoginSettings: false);
        var originalName = applicationUser.Name;
        var newName = TestData.GenerateChangedApplicationUserName(originalName);
        var newRoles = new[] { ApiRoles.GetPerson, ApiRoles.UpdatePerson };
        var oneLoginClientId = Guid.NewGuid().ToString();
        var oneLoginPrivateKeyPem = TestData.GeneratePrivateKeyPem();

        var request = new HttpRequestMessage(HttpMethod.Post, $"/application-users/{applicationUser.UserId}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "Name", newName },
                { "ApiRoles", newRoles },
                { "OneLoginClientId", oneLoginClientId },
                { "OneLoginPrivateKeyPem", oneLoginPrivateKeyPem! }
            }
        };

        EventObserver.Clear();

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal("/application-users", response.Headers.Location?.OriginalString);

        await WithDbContext(async dbContext =>
        {
            applicationUser = await dbContext.ApplicationUsers.SingleAsync(u => u.UserId == applicationUser.UserId);
            Assert.True(new HashSet<string>(applicationUser.ApiRoles).SetEquals(new HashSet<string>(newRoles)));
        });

        EventObserver.AssertEventsSaved(
            e =>
            {
                var applicationUserUpdatedEvent = Assert.IsType<ApplicationUserUpdatedEvent>(e);
                Assert.Equal(Clock.UtcNow, applicationUserUpdatedEvent.CreatedUtc);
                Assert.Equal(GetCurrentUserId(), applicationUserUpdatedEvent.RaisedBy.UserId);
                Assert.Equal(originalName, applicationUserUpdatedEvent.OldApplicationUser.Name);
                Assert.Equal(newName, applicationUserUpdatedEvent.ApplicationUser.Name);
                Assert.True(applicationUserUpdatedEvent.ApplicationUser.ApiRoles.SequenceEqual(newRoles));
                Assert.Empty(applicationUserUpdatedEvent.OldApplicationUser.ApiRoles);
                Assert.Null(applicationUserUpdatedEvent.OldApplicationUser.OneLoginClientId);
                Assert.Null(applicationUserUpdatedEvent.OldApplicationUser.OneLoginPrivateKeyPem);
                Assert.Equal(oneLoginClientId, applicationUserUpdatedEvent.ApplicationUser.OneLoginClientId);
                Assert.Equal(oneLoginPrivateKeyPem, applicationUserUpdatedEvent.ApplicationUser.OneLoginPrivateKeyPem);
                Assert.Equal(ApplicationUserUpdatedEventChanges.ApiRoles | ApplicationUserUpdatedEventChanges.Name | ApplicationUserUpdatedEventChanges.OneLoginClientId | ApplicationUserUpdatedEventChanges.OneLoginPrivateKeyPem, applicationUserUpdatedEvent.Changes);
            });

        var redirectResponse = await response.FollowRedirect(HttpClient);
        var redirectDoc = await redirectResponse.GetDocument();
        AssertEx.HtmlDocumentHasFlashSuccess(redirectDoc, "Application user updated");
    }
}
