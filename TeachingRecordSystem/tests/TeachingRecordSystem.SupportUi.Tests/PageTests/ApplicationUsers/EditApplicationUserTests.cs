using System.Security.Cryptography;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

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
        var doc = await AssertEx.HtmlResponse(response);

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

    [Theory]
    [MemberData(nameof(InvalidOneLoginDetailsData))]
    public async Task Post_WithOidcClientButInvalidDetails_RendersExpectedError(
        string oneLoginClientId,
        string oneLoginClientKeyPem,
        string oneLoginAuthenticationSchemeName,
        string oneLoginRedirectUriPath,
        string oneLoginPostLogoutRedirectUriPath,
        string expectedErrorField,
        string expectedErrorMessage)
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUser();

        var request = new HttpRequestMessage(HttpMethod.Post, $"/application-users/{applicationUser.UserId}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "Name", applicationUser.Name },
                { "ApiRoles", applicationUser.ApiRoles },
                { "IsOidcClient", bool.TrueString },
                { "OneLoginClientId", oneLoginClientId },
                { "OneLoginClientKeyPem", oneLoginClientKeyPem },
                { "OneLoginAuthenticationSchemeName", oneLoginAuthenticationSchemeName },
                { "OneLoginRedirectUriPath", oneLoginRedirectUriPath },
                { "OneLoginPostLogoutRedirectUriPath", oneLoginPostLogoutRedirectUriPath },
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, expectedErrorField, expectedErrorMessage);
    }

    [Fact]
    public async Task Post_ValidRequest_UpdatesNameAndRolesAndOneLoginSettingsAndCreatesEventAndRedirectsWithFlashMessage()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUser(apiRoles: [], hasOneLoginSettings: false);
        var originalName = applicationUser.Name;
        var newName = TestData.GenerateChangedApplicationUserName(originalName);
        var newRoles = new[] { ApiRoles.GetPerson, ApiRoles.UpdatePerson };
        var oneLoginClientId = Guid.NewGuid().ToString();
        var oneLoginPrivateKeyPem = TestData.GeneratePrivateKeyPem();
        var oneLoginAuthenticationSchemeName = Guid.NewGuid().ToString();
        var oneLoginRedirectUriPath = $"/_onelogin/{oneLoginAuthenticationSchemeName}/callback";
        var oneLoginPostLogoutRedirectUriPath = $"/_onelogin/{oneLoginAuthenticationSchemeName}/logout-callback";

        var request = new HttpRequestMessage(HttpMethod.Post, $"/application-users/{applicationUser.UserId}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "Name", newName },
                { "ApiRoles", newRoles },
                { "IsOidcClient", bool.TrueString },
                { "OneLoginClientId", oneLoginClientId },
                { "OneLoginPrivateKeyPem", oneLoginPrivateKeyPem },
                { "OneLoginAuthenticationSchemeName", oneLoginAuthenticationSchemeName },
                { "OneLoginRedirectUriPath", oneLoginRedirectUriPath },
                { "OneLoginPostLogoutRedirectUriPath", oneLoginPostLogoutRedirectUriPath },
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
                Assert.False(applicationUserUpdatedEvent.OldApplicationUser.IsOidcClient);
                Assert.Null(applicationUserUpdatedEvent.OldApplicationUser.OneLoginClientId);
                Assert.Null(applicationUserUpdatedEvent.OldApplicationUser.OneLoginPrivateKeyPem);
                Assert.Null(applicationUserUpdatedEvent.OldApplicationUser.OneLoginAuthenticationSchemeName);
                Assert.Null(applicationUserUpdatedEvent.OldApplicationUser.OneLoginRedirectUriPath);
                Assert.Null(applicationUserUpdatedEvent.OldApplicationUser.OneLoginPostLogoutRedirectUriPath);
                Assert.True(applicationUserUpdatedEvent.ApplicationUser.IsOidcClient);
                Assert.Equal(oneLoginClientId, applicationUserUpdatedEvent.ApplicationUser.OneLoginClientId);
                Assert.Equal(oneLoginPrivateKeyPem, applicationUserUpdatedEvent.ApplicationUser.OneLoginPrivateKeyPem);
                Assert.Equal(oneLoginAuthenticationSchemeName, applicationUserUpdatedEvent.ApplicationUser.OneLoginAuthenticationSchemeName);
                Assert.Equal(oneLoginRedirectUriPath, applicationUserUpdatedEvent.ApplicationUser.OneLoginRedirectUriPath);
                Assert.Equal(oneLoginPostLogoutRedirectUriPath, applicationUserUpdatedEvent.ApplicationUser.OneLoginPostLogoutRedirectUriPath);
                Assert.Equal(
                    ApplicationUserUpdatedEventChanges.ApiRoles |
                        ApplicationUserUpdatedEventChanges.Name |
                        ApplicationUserUpdatedEventChanges.IsOidcClient |
                        ApplicationUserUpdatedEventChanges.OneLoginClientId |
                        ApplicationUserUpdatedEventChanges.OneLoginPrivateKeyPem |
                        ApplicationUserUpdatedEventChanges.OneLoginAuthenticationSchemeName |
                        ApplicationUserUpdatedEventChanges.OneLoginRedirectUriPath |
                        ApplicationUserUpdatedEventChanges.OneLoginPostLogoutRedirectUriPath,
                    applicationUserUpdatedEvent.Changes);
            });

        var redirectResponse = await response.FollowRedirect(HttpClient);
        var redirectDoc = await redirectResponse.GetDocument();
        AssertEx.HtmlDocumentHasFlashSuccess(redirectDoc, "Application user updated");
    }

    public static TheoryData<string, string, string, string, string, string, string> InvalidOneLoginDetailsData => new()
    {
        {
            "client_id",
            _privateKeyPem,
            "",  // OneLoginAuthenticationSchemeName
            $"/_onelogin/{Guid.NewGuid:N}/callback",
            $"/_onelogin/{Guid.NewGuid:N}/logout-callback",
            "OneLoginAuthenticationSchemeName",
            "Enter an authentication scheme name"
        },
        {
            "client_id",
            _privateKeyPem,
            new string('x', 51),  // OneLoginAuthenticationSchemeName
            $"/_onelogin/{Guid.NewGuid:N}/callback",
            $"/_onelogin/{Guid.NewGuid:N}/logout-callback",
            "OneLoginAuthenticationSchemeName",
            "Authentication scheme name must be 50 characters or less"
        },
        {
            "client_id",
            "",  // OneLoginPrivateKeyPem
            Guid.NewGuid().ToString(),
            $"/_onelogin/{Guid.NewGuid:N}/callback",
            $"/_onelogin/{Guid.NewGuid:N}/logout-callback",
            "OneLoginPrivateKeyPem",
            "Enter the One Login private key"
        },

        {
            "",  // OneLoginClientId
            _privateKeyPem,
            Guid.NewGuid().ToString(),
            $"/_onelogin/{Guid.NewGuid:N}/callback",
            $"/_onelogin/{Guid.NewGuid:N}/logout-callback",
            "OneLoginClientId",
            "Enter the One Login client ID"
        },
        {
            "client_id",
            _privateKeyPem,
            Guid.NewGuid().ToString(),
            "",  // OneLoginRedirectUriPath
            $"/_onelogin/{Guid.NewGuid:N}/logout-callback",
            "OneLoginRedirectUriPath",
            "Enter the One Login redirect URI"
        },
        {
            "client_id",
            _privateKeyPem,
            Guid.NewGuid().ToString(),
            $"/_onelogin/{Guid.NewGuid:N}/callback",
            "",  // OneLoginPostLogoutRedirectUriPath
            "OneLoginPostLogoutRedirectUriPath",
            "Enter the One Login post logout redirect URI"
        },
        {
            "client_id",
            _privateKeyPem,
            Guid.NewGuid().ToString(),
            new string('x', 101),  // OneLoginRedirectUriPath
            $"/_onelogin/{Guid.NewGuid:N}/logout-callback",
            "OneLoginRedirectUriPath",
            "One Login redirect URI must be 100 characters or less"
        },
        {
            "client_id",
            _privateKeyPem,
            Guid.NewGuid().ToString(),
            $"/_onelogin/{Guid.NewGuid:N}/callback",
            new string('x', 101),  // OneLoginPostLogoutRedirectUriPath
            "OneLoginPostLogoutRedirectUriPath",
            "One Login post logout redirect URI must be 100 characters or less"
        }
    };

    private static readonly string _privateKeyPem = RSA.Create().ExportPkcs8PrivateKeyPem();
}
