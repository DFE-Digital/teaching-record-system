using System.Security.Cryptography;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.ApplicationUsers.EditApplicationUser;

public class IndexTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Test]
    public async Task Get_UserDoesNotHavePermission_ReturnsForbidden()
    {
        // Arrange
        SetCurrentUser(await TestData.CreateUserAsync(role: null));

        var applicationUser = await TestData.CreateApplicationUserAsync(apiRoles: []);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/application-users/{applicationUser.UserId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Test]
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

    [Test]
    public async Task Get_ValidRequest_RendersExpectedContent()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync(apiRoles: [ApiRoles.GetPerson, ApiRoles.UpdatePerson], isOidcClient: true);
        var apiKeyUnexpired = await TestData.CreateApiKeyAsync(applicationUser.UserId, expired: false);
        var apiKeyExpired = await TestData.CreateApiKeyAsync(applicationUser.UserId, expired: true);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/application-users/{applicationUser.UserId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.NotNull(doc.GetElementByLabel(ApiRoles.GetPerson)?.GetAttribute("checked"));
        Assert.NotNull(doc.GetElementByLabel(ApiRoles.UpdatePerson)?.GetAttribute("checked"));

        var apiKeysTable = doc.GetElementByTestId("ApiKeysTable")!;
        var keyRows = apiKeysTable.GetElementsByTagName("tbody").Single().GetElementsByTagName("tr");

        Assert.Collection(
            keyRows,
            row =>
            {
                var expiry = row.GetElementByTestId("Expiry")?.TrimmedText();
                Assert.Equal("No expiration", expiry);
            },
            row =>
            {
                var expiry = row.GetElementByTestId("Expiry")?.TrimmedText();
                Assert.Equal(apiKeyExpired.Expires!.Value.ToString("dd/MM/yyyy HH:mm"), expiry);
            });

        Assert.Equal(applicationUser.OneLoginClientId, doc.GetElementById("OneLoginClientId")?.GetAttribute("value"));
        Assert.Equal(applicationUser.OneLoginPrivateKeyPem, doc.GetElementById("OneLoginPrivateKeyPem")?.TrimmedText()?.Trim());
    }

    [Test]
    public async Task Post_UserDoesNotHavePermission_ReturnsForbidden()
    {
        // Arrange
        SetCurrentUser(await TestData.CreateUserAsync(role: null));

        var applicationUser = await TestData.CreateApplicationUserAsync(apiRoles: []);
        var originalName = applicationUser.Name;
        var newName = TestData.GenerateChangedApplicationUserName(originalName);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/application-users/{applicationUser.UserId}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Name", newName }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Test]
    public async Task Post_UserDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var applicationUserId = Guid.NewGuid();
        var newName = "name";

        var request = new HttpRequestMessage(HttpMethod.Post, $"/application-users/{applicationUserId}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Name", newName }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Test]
    public async Task Post_NameNotProvided_RendersError()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync(apiRoles: []);
        var newName = "";

        var request = new HttpRequestMessage(HttpMethod.Post, $"/application-users/{applicationUser.UserId}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Name", newName }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "Name", "Enter a name");
    }

    [Test]
    public async Task Post_NameTooLong_RendersError()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync(apiRoles: []);
        var newName = new string('x', UserBase.NameMaxLength + 1);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/application-users/{applicationUser.UserId}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Name", newName }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "Name", "Name must be 200 characters or less");
    }

    [Test]
    [MethodDataSource(nameof(GetInvalidOidcDetailsData))]
    public async Task Post_WithOidcClientButInvalidDetails_RendersExpectedError(
        string clientId,
        string clientSecret,
        string redirectUris,
        string postLogoutRedirectUris,
        string oneLoginClientId,
        string oneLoginClientKeyPem,
        string oneLoginAuthenticationSchemeName,
        string oneLoginRedirectUriPath,
        string oneLoginPostLogoutRedirectUriPath,
        string expectedErrorField,
        string expectedErrorMessage)
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, $"/application-users/{applicationUser.UserId}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Name", applicationUser.Name },
                { "ApiRoles", applicationUser.ApiRoles ?? [] },
                { "IsOidcClient", bool.TrueString },
                { "ClientId", clientId },
                { "ClientSecret", clientSecret },
                { "RedirectUris", redirectUris },
                { "PostLogoutRedirectUris", postLogoutRedirectUris },
                { "OneLoginClientId", oneLoginClientId },
                { "OneLoginClientKeyPem", oneLoginClientKeyPem },
                { "OneLoginAuthenticationSchemeName", oneLoginAuthenticationSchemeName },
                { "OneLoginRedirectUriPath", oneLoginRedirectUriPath },
                { "OneLoginPostLogoutRedirectUriPath", oneLoginPostLogoutRedirectUriPath }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, expectedErrorField, expectedErrorMessage);
    }

    [Test]
    public async Task Post_ValidRequest_UpdatesNameAndRolesAndOneLoginSettingsAndCreatesEventAndRedirectsWithFlashMessage()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync(shortName: "", apiRoles: [], isOidcClient: false);
        var originalName = applicationUser.Name;
        var newName = TestData.GenerateChangedApplicationUserName(originalName);
        var newShortName = TestData.GenerateApplicationUserShortName();
        var newRoles = new[] { ApiRoles.GetPerson, ApiRoles.UpdatePerson };
        var clientId = "client-id";
        var clientSecret = "Secret0123456789";
        var redirectUris = "http://localhost/callback";
        var postLogoutRedirectUris = "http://localhost/logout-callback";
        var oneLoginClientId = Guid.NewGuid().ToString();
        var oneLoginPrivateKeyPem = TestData.GeneratePrivateKeyPem();
        var oneLoginAuthenticationSchemeName = Guid.NewGuid().ToString();
        var oneLoginRedirectUriPath = $"/_onelogin/{oneLoginAuthenticationSchemeName}/callback";
        var oneLoginPostLogoutRedirectUriPath = $"/_onelogin/{oneLoginAuthenticationSchemeName}/logout-callback";

        var request = new HttpRequestMessage(HttpMethod.Post, $"/application-users/{applicationUser.UserId}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Name", newName },
                { "ShortName", newShortName },
                { "ApiRoles", newRoles },
                { "IsOidcClient", bool.TrueString },
                { "ClientId", clientId },
                { "ClientSecret", clientSecret },
                { "RedirectUris", redirectUris },
                { "PostLogoutRedirectUris", postLogoutRedirectUris },
                { "OneLoginClientId", oneLoginClientId },
                { "OneLoginPrivateKeyPem", oneLoginPrivateKeyPem },
                { "OneLoginAuthenticationSchemeName", oneLoginAuthenticationSchemeName },
                { "OneLoginRedirectUriPath", oneLoginRedirectUriPath },
                { "OneLoginPostLogoutRedirectUriPath", oneLoginPostLogoutRedirectUriPath }
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
            Assert.True(new HashSet<string>(applicationUser.ApiRoles ?? []).SetEquals(new HashSet<string>(newRoles)));
        });

        EventObserver.AssertEventsSaved(
            e =>
            {
                var applicationUserUpdatedEvent = Assert.IsType<ApplicationUserUpdatedEvent>(e);
                Assert.Equal(Clock.UtcNow, applicationUserUpdatedEvent.CreatedUtc);
                Assert.Equal(GetCurrentUserId(), applicationUserUpdatedEvent.RaisedBy.UserId);
                Assert.Equal(originalName, applicationUserUpdatedEvent.OldApplicationUser.Name);
                Assert.Equal(newName, applicationUserUpdatedEvent.ApplicationUser.Name);
                Assert.Equal(newShortName, applicationUserUpdatedEvent.ApplicationUser.ShortName);
                Assert.True((applicationUserUpdatedEvent.ApplicationUser.ApiRoles ?? []).SequenceEqual(newRoles));
                Assert.Empty(applicationUserUpdatedEvent.OldApplicationUser.ApiRoles ?? []);
                Assert.False(applicationUserUpdatedEvent.OldApplicationUser.IsOidcClient);
                Assert.Null(applicationUserUpdatedEvent.OldApplicationUser.ClientId);
                Assert.Null(applicationUserUpdatedEvent.OldApplicationUser.ClientSecret);
                Assert.Empty(applicationUserUpdatedEvent.OldApplicationUser.RedirectUris ?? []);
                Assert.Empty(applicationUserUpdatedEvent.OldApplicationUser.PostLogoutRedirectUris ?? []);
                Assert.Null(applicationUserUpdatedEvent.OldApplicationUser.OneLoginClientId);
                Assert.Null(applicationUserUpdatedEvent.OldApplicationUser.OneLoginPrivateKeyPem);
                Assert.Null(applicationUserUpdatedEvent.OldApplicationUser.OneLoginAuthenticationSchemeName);
                Assert.Null(applicationUserUpdatedEvent.OldApplicationUser.OneLoginRedirectUriPath);
                Assert.Null(applicationUserUpdatedEvent.OldApplicationUser.OneLoginPostLogoutRedirectUriPath);
                Assert.True(applicationUserUpdatedEvent.ApplicationUser.IsOidcClient);
                Assert.Equal(clientId, applicationUserUpdatedEvent.ApplicationUser.ClientId);
                Assert.Equal(clientSecret, applicationUserUpdatedEvent.ApplicationUser.ClientSecret);
                Assert.Collection(applicationUserUpdatedEvent.ApplicationUser.RedirectUris ?? [], uri => Assert.Equal(redirectUris, uri));
                Assert.Collection(applicationUserUpdatedEvent.ApplicationUser.PostLogoutRedirectUris ?? [], uri => Assert.Equal(postLogoutRedirectUris, uri));
                Assert.Equal(oneLoginClientId, applicationUserUpdatedEvent.ApplicationUser.OneLoginClientId);
                Assert.Equal(oneLoginPrivateKeyPem, applicationUserUpdatedEvent.ApplicationUser.OneLoginPrivateKeyPem);
                Assert.Equal(oneLoginAuthenticationSchemeName, applicationUserUpdatedEvent.ApplicationUser.OneLoginAuthenticationSchemeName);
                Assert.Equal(oneLoginRedirectUriPath, applicationUserUpdatedEvent.ApplicationUser.OneLoginRedirectUriPath);
                Assert.Equal(oneLoginPostLogoutRedirectUriPath, applicationUserUpdatedEvent.ApplicationUser.OneLoginPostLogoutRedirectUriPath);
                Assert.Equal(
                    ApplicationUserUpdatedEventChanges.ApiRoles |
                        ApplicationUserUpdatedEventChanges.Name |
                        ApplicationUserUpdatedEventChanges.ShortName |
                        ApplicationUserUpdatedEventChanges.IsOidcClient |
                        ApplicationUserUpdatedEventChanges.ClientId |
                        ApplicationUserUpdatedEventChanges.ClientSecret |
                        ApplicationUserUpdatedEventChanges.RedirectUris |
                        ApplicationUserUpdatedEventChanges.PostLogoutRedirectUris |
                        ApplicationUserUpdatedEventChanges.OneLoginClientId |
                        ApplicationUserUpdatedEventChanges.OneLoginPrivateKeyPem |
                        ApplicationUserUpdatedEventChanges.OneLoginAuthenticationSchemeName |
                        ApplicationUserUpdatedEventChanges.OneLoginRedirectUriPath |
                        ApplicationUserUpdatedEventChanges.OneLoginPostLogoutRedirectUriPath,
                    applicationUserUpdatedEvent.Changes);
            });

        var redirectResponse = await response.FollowRedirectAsync(HttpClient);
        var redirectDoc = await redirectResponse.GetDocumentAsync();
        AssertEx.HtmlDocumentHasFlashSuccess(redirectDoc, "Application user updated");
    }

    public static (string ClientId,
        string ClientSecret,
        string RedirectUris,
        string PostLogoutRedirectUris,
        string OneLoginClientId,
        string OneLoginClientKeyPem,
        string OneLoginAuthenticationSchemeName,
        string OneLoginRedirectUriPath,
        string OneLoginPostLogoutRedirectUriPath,
        string ExpectedErrorField,
        string ExpectedErrorMessage)[] GetInvalidOidcDetailsData() =>
    [
        (
            "client_id",
            "Secret0123456789",
            "https://localhost/callback",
            "https://localhost/logout-callback",
            "client_id",
            _privateKeyPem,
            "",  // OneLoginAuthenticationSchemeName
            $"/_onelogin/{Guid.NewGuid:N}/callback",
            $"/_onelogin/{Guid.NewGuid:N}/logout-callback",
            "OneLoginAuthenticationSchemeName",
            "Enter an authentication scheme name"
        ),
        (
            "client_id",
            "Secret0123456789",
            "https://localhost/callback",
            "https://localhost/logout-callback",
            "client_id",
            _privateKeyPem,
            new string('x', 51),  // OneLoginAuthenticationSchemeName
            $"/_onelogin/{Guid.NewGuid:N}/callback",
            $"/_onelogin/{Guid.NewGuid:N}/logout-callback",
            "OneLoginAuthenticationSchemeName",
            "Authentication scheme name must be 50 characters or less"
        ),
        (
            "client_id",
            "Secret0123456789",
            "https://localhost/callback",
            "https://localhost/logout-callback",
            "client_id",
            "",  // OneLoginPrivateKeyPem
            Guid.NewGuid().ToString(),
            $"/_onelogin/{Guid.NewGuid:N}/callback",
            $"/_onelogin/{Guid.NewGuid:N}/logout-callback",
            "OneLoginPrivateKeyPem",
            "Enter the One Login private key"
        ),
        (
            "client_id",
            "Secret0123456789",
            "https://localhost/callback",
            "https://localhost/logout-callback",
            "",  // OneLoginClientId
            _privateKeyPem,
            Guid.NewGuid().ToString(),
            $"/_onelogin/{Guid.NewGuid:N}/callback",
            $"/_onelogin/{Guid.NewGuid:N}/logout-callback",
            "OneLoginClientId",
            "Enter the One Login client ID"
        ),
        (
            "client_id",
            "Secret0123456789",
            "https://localhost/callback",
            "https://localhost/logout-callback",
            "client_id",
            _privateKeyPem,
            Guid.NewGuid().ToString(),
            "",  // OneLoginRedirectUriPath
            $"/_onelogin/{Guid.NewGuid:N}/logout-callback",
            "OneLoginRedirectUriPath",
            "Enter the One Login redirect URI"
        ),
        (
            "client_id",
            "Secret0123456789",
            "https://localhost/callback",
            "https://localhost/logout-callback",
            "client_id",
            _privateKeyPem,
            Guid.NewGuid().ToString(),
            $"/_onelogin/{Guid.NewGuid:N}/callback",
            "",  // OneLoginPostLogoutRedirectUriPath
            "OneLoginPostLogoutRedirectUriPath",
            "Enter the One Login post logout redirect URI"
        ),
        (
            "client_id",
            "Secret0123456789",
            "https://localhost/callback",
            "https://localhost/logout-callback",
            "client_id",
            _privateKeyPem,
            Guid.NewGuid().ToString(),
            new string('x', 101),  // OneLoginRedirectUriPath
            $"/_onelogin/{Guid.NewGuid:N}/logout-callback",
            "OneLoginRedirectUriPath",
            "One Login redirect URI must be 100 characters or less"
        ),
        (
            "client_id",
            "Secret0123456789",
            "https://localhost/callback",
            "https://localhost/logout-callback",
            "client_id",
            _privateKeyPem,
            Guid.NewGuid().ToString(),
            $"/_onelogin/{Guid.NewGuid:N}/callback",
            new string('x', 101),  // OneLoginPostLogoutRedirectUriPath
            "OneLoginPostLogoutRedirectUriPath",
            "One Login post logout redirect URI must be 100 characters or less"
        ),
        (
            "",
            "Secret0123456789",
            "https://localhost/callback",
            "https://localhost/logout-callback",
            "client_id",
            _privateKeyPem,
            Guid.NewGuid().ToString(),
            $"/_onelogin/{Guid.NewGuid:N}/callback",
            $"/_onelogin/{Guid.NewGuid:N}/logout-callback",
            "ClientId",
            "Enter a client ID"
        ),
        (
            new string('x', 51),
            "Secret0123456789",
            "https://localhost/callback",
            "https://localhost/logout-callback",
            "client_id",
            _privateKeyPem,
            Guid.NewGuid().ToString(),
            $"/_onelogin/{Guid.NewGuid:N}/callback",
            $"/_onelogin/{Guid.NewGuid:N}/logout-callback",
            "ClientId",
            "Client ID must be 50 characters or less"
        ),
        (
            "client_id",
            "",
            "https://localhost/callback",
            "https://localhost/logout-callback",
            "client_id",
            _privateKeyPem,
            Guid.NewGuid().ToString(),
            $"/_onelogin/{Guid.NewGuid:N}/callback",
            $"/_onelogin/{Guid.NewGuid:N}/logout-callback",
            "ClientSecret",
            "Enter a client secret"
        ),
        (
            "client_id",
            new string('x', 201),
            "https://localhost/callback",
            "https://localhost/logout-callback",
            "client_id",
            _privateKeyPem,
            Guid.NewGuid().ToString(),
            $"/_onelogin/{Guid.NewGuid:N}/callback",
            $"/_onelogin/{Guid.NewGuid:N}/logout-callback",
            "ClientSecret",
            "Client secret must be 200 characters or less"
        ),
        (
            "client_id",
            "S",
            "https://localhost/callback",
            "https://localhost/logout-callback",
            "client_id",
            _privateKeyPem,
            Guid.NewGuid().ToString(),
            $"/_onelogin/{Guid.NewGuid:N}/callback",
            $"/_onelogin/{Guid.NewGuid:N}/logout-callback",
            "ClientSecret",
            "Client secret must be at least 16 characters"
        ),
        (
            "client_id",
            "Secret0123456789",
            "foo",
            "https://localhost/logout-callback",
            "client_id",
            _privateKeyPem,
            Guid.NewGuid().ToString(),
            $"/_onelogin/{Guid.NewGuid:N}/callback",
            $"/_onelogin/{Guid.NewGuid:N}/logout-callback",
            "RedirectUris",
            "One or more redirect URIs are not valid"
        ),
        (
            "client_id",
            "Secret0123456789",
            "https://localhost/callback",
            "foo",
            "client_id",
            _privateKeyPem,
            Guid.NewGuid().ToString(),
            $"/_onelogin/{Guid.NewGuid:N}/callback",
            $"/_onelogin/{Guid.NewGuid:N}/logout-callback",
            "PostLogoutRedirectUris",
            "One or more post logout redirect URIs are not valid"
        )
    ];

    private static readonly string _privateKeyPem = RSA.Create().ExportPkcs8PrivateKeyPem();
}
