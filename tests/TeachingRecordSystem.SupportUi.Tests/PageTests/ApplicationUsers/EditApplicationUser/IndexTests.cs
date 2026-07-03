using System.Security.Cryptography;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.ApplicationUsers.EditApplicationUser;

public class IndexTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
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

    [Fact]
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

    [Fact]
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

    [Fact]
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

    [Fact]
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

    [Fact]
    public async Task Post_ShortNameTooLong_RendersError()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync(apiRoles: []);
        var newName = "xxxx";
        var newShortName = new string('x', ApplicationUser.ShortNameMaxLength + 1);


        var request = new HttpRequestMessage(HttpMethod.Post, $"/application-users/{applicationUser.UserId}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Name", newName },
                { "ShortName", newShortName }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "ShortName", "Short name must be 25 characters or less");
    }

    [Theory]
    [MemberData(nameof(GetInvalidOidcDetailsData))]
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
        string supportEmailAddressNotifyId,
        string supportEmailAddress,
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
                { "UseSharedOneLoginSigningKeys", "false" },
                { "OneLoginClientKeyPem", oneLoginClientKeyPem },
                { "OneLoginAuthenticationSchemeName", oneLoginAuthenticationSchemeName },
                { "OneLoginRedirectUriPath", oneLoginRedirectUriPath },
                { "OneLoginPostLogoutRedirectUriPath", oneLoginPostLogoutRedirectUriPath },
                { "SupportEmailAddressNotifyId", supportEmailAddressNotifyId },
                { "SupportEmailAddress", supportEmailAddress }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, expectedErrorField, expectedErrorMessage);
    }

    [Fact]
    public async Task Post_ValidRequest_UpdatesNameAndRolesAndOneLoginSettingsAndCreatesEventAndRedirectsWithFlashMessage()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync(apiRoles: [], isOidcClient: false);
        var originalName = applicationUser.Name;
        var newName = TestData.GenerateChangedApplicationUserName(originalName);
        var newShortName = TestData.GenerateApplicationUserShortName();
        var newRoles = new[] { ApiRoles.GetPerson, ApiRoles.UpdatePerson };
        var clientId = Guid.NewGuid().ToString();
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
                { "UseSharedOneLoginSigningKeys", "false" },
                { "UseSharedOneLoginSigningKeys", "false" },
                { "OneLoginPrivateKeyPem", oneLoginPrivateKeyPem },
                { "OneLoginAuthenticationSchemeName", oneLoginAuthenticationSchemeName },
                { "OneLoginRedirectUriPath", oneLoginRedirectUriPath },
                { "OneLoginPostLogoutRedirectUriPath", oneLoginPostLogoutRedirectUriPath },
                { "RecordMatchingPolicy", RecordMatchingPolicy.Deferred.ToString() }
            }
        };

        Events.Clear();

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal("/application-users", response.Headers.Location?.OriginalString);

        await WithDbContextAsync(async dbContext =>
        {
            applicationUser = await dbContext.ApplicationUsers.SingleAsync(u => u.UserId == applicationUser.UserId);
            Assert.True(new HashSet<string>(applicationUser.ApiRoles ?? []).SetEquals(new HashSet<string>(newRoles)));
        });

        Events.AssertProcessesCreated(p =>
        {
            Assert.Equal(ProcessType.ApplicationUserUpdating, p.ProcessContext.ProcessType);
            Assert.Equal(TimeProvider.UtcNow, p.ProcessContext.Process.CreatedOn);
            Assert.Equal(GetCurrentUserId(), p.ProcessContext.Process.UserId);
            p.AssertProcessHasEvents<ApplicationUserUpdatedEvent>(applicationUserUpdatedEvent =>
            {
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
                Assert.Null(applicationUserUpdatedEvent.OldApplicationUser.UseSharedOneLoginSigningKeys);
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
                Assert.False(applicationUserUpdatedEvent.ApplicationUser.UseSharedOneLoginSigningKeys);
                Assert.Equal(oneLoginPrivateKeyPem, applicationUserUpdatedEvent.ApplicationUser.OneLoginPrivateKeyPem);
                Assert.Equal(oneLoginAuthenticationSchemeName, applicationUserUpdatedEvent.ApplicationUser.OneLoginAuthenticationSchemeName);
                Assert.Equal(oneLoginRedirectUriPath, applicationUserUpdatedEvent.ApplicationUser.OneLoginRedirectUriPath);
                Assert.Equal(oneLoginPostLogoutRedirectUriPath, applicationUserUpdatedEvent.ApplicationUser.OneLoginPostLogoutRedirectUriPath);
                Assert.Equal(RecordMatchingPolicy.Required, applicationUserUpdatedEvent.OldApplicationUser.RecordMatchingPolicy);
                Assert.Equal(RecordMatchingPolicy.Deferred, applicationUserUpdatedEvent.ApplicationUser.RecordMatchingPolicy);
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
                        ApplicationUserUpdatedEventChanges.UseSharedOneLoginSigningKeys |
                        ApplicationUserUpdatedEventChanges.OneLoginPrivateKeyPem |
                        ApplicationUserUpdatedEventChanges.OneLoginAuthenticationSchemeName |
                        ApplicationUserUpdatedEventChanges.OneLoginRedirectUriPath |
                        ApplicationUserUpdatedEventChanges.OneLoginPostLogoutRedirectUriPath |
                        ApplicationUserUpdatedEventChanges.RecordMatchingPolicy,
                    applicationUserUpdatedEvent.Changes);
            });
        });

        var redirectResponse = await response.FollowRedirectAsync(HttpClient);
        var redirectDoc = await redirectResponse.GetDocumentAsync();
        AssertEx.HtmlDocumentHasFlashNotificationBanner(redirectDoc, "Application user updated");
    }

    [Fact]
    public async Task Post_ValidRequestWithAppContent_UpdatesAppContentAndCreatesEvent()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync(isOidcClient: true);
        var newName = applicationUser.Name;
        var emailTemplateId = Guid.NewGuid().ToString();
        var pageContent = "<p>Custom content for this app</p>";
        var flashMessage = "Request closed for {0}. We've sent them an email with a link to continue their NPQ registration.";
        var foundPageLinkText = "<p class=\"govuk-body\">You can return to the <a href=\"{0}\" class=\"govuk-link\">Register for a national professional qualification</a> service.</p>";
        var supportEmailAddressNotifyId = Guid.NewGuid().ToString();
        var supportEmailAddress = "support@example.com";

        var request = new HttpRequestMessage(HttpMethod.Post, $"/application-users/{applicationUser.UserId}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Name", newName },
                { "ApiRoles", applicationUser.ApiRoles ?? [] },
                { "IsOidcClient", bool.TrueString },
                { "ClientId", applicationUser.ClientId! },
                { "ClientSecret", applicationUser.ClientSecret! },
                { "RedirectUris", applicationUser.RedirectUris ?? [] },
                { "PostLogoutRedirectUris", applicationUser.PostLogoutRedirectUris ?? [] },
                { "OneLoginClientId", applicationUser.OneLoginClientId! },
                { "UseSharedOneLoginSigningKeys", applicationUser.UseSharedOneLoginSigningKeys!.Value.ToString() },
                { "OneLoginPrivateKeyPem", applicationUser.OneLoginPrivateKeyPem! },
                { "OneLoginAuthenticationSchemeName", applicationUser.OneLoginAuthenticationSchemeName! },
                { "OneLoginRedirectUriPath", applicationUser.OneLoginRedirectUriPath! },
                { "OneLoginPostLogoutRedirectUriPath", applicationUser.OneLoginPostLogoutRedirectUriPath! },
                { "RecordMatchingPolicy", applicationUser.RecordMatchingPolicy.ToString() },
                { "OneLoginCannotFindRecordEmailTemplateId", emailTemplateId },
                { "OneLoginNoMatchesPageContentHtml", pageContent },
                { "OneLoginNoMatchesEmailSentFlashMessage", flashMessage },
                { "OneLoginFoundPageLinkText", foundPageLinkText },
                { "SupportEmailAddressNotifyId", supportEmailAddressNotifyId },
                { "SupportEmailAddress", supportEmailAddress }
            }
        };

        Events.Clear();

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        await WithDbContextAsync(async dbContext =>
        {
            applicationUser = await dbContext.ApplicationUsers.SingleAsync(u => u.UserId == applicationUser.UserId);
            Assert.NotNull(applicationUser.AppContent);
            Assert.Equal(emailTemplateId, applicationUser.AppContent.OneLoginCannotFindRecordEmailTemplateId);
            Assert.Equal(pageContent, applicationUser.AppContent.OneLoginNoMatchesPageContentHtml);
            Assert.Equal(flashMessage, applicationUser.AppContent.OneLoginNoMatchesEmailSentFlashMessage);
            Assert.Equal(foundPageLinkText, applicationUser.AppContent.OneLoginFoundPageLinkText);
            Assert.Equal(supportEmailAddressNotifyId, applicationUser.AppContent.SupportEmailAddressNotifyId);
            Assert.Equal(supportEmailAddress, applicationUser.AppContent.SupportEmailAddress);
        });

        Events.AssertProcessesCreated(p =>
        {
            Assert.Equal(ProcessType.ApplicationUserUpdating, p.ProcessContext.ProcessType);
            p.AssertProcessHasEvents<ApplicationUserUpdatedEvent>(applicationUserUpdatedEvent =>
            {
                Assert.NotNull(applicationUserUpdatedEvent.ApplicationUser.AppContent);
                Assert.Equal(emailTemplateId, applicationUserUpdatedEvent.ApplicationUser.AppContent.OneLoginCannotFindRecordEmailTemplateId);
                Assert.Equal(pageContent, applicationUserUpdatedEvent.ApplicationUser.AppContent.OneLoginNoMatchesPageContentHtml);
                Assert.Equal(flashMessage, applicationUserUpdatedEvent.ApplicationUser.AppContent.OneLoginNoMatchesEmailSentFlashMessage);
                Assert.Equal(foundPageLinkText, applicationUserUpdatedEvent.ApplicationUser.AppContent.OneLoginFoundPageLinkText);
                Assert.Equal(supportEmailAddressNotifyId, applicationUserUpdatedEvent.ApplicationUser.AppContent.SupportEmailAddressNotifyId);
                Assert.Equal(supportEmailAddress, applicationUserUpdatedEvent.ApplicationUser.AppContent.SupportEmailAddress);
                Assert.Null(applicationUserUpdatedEvent.OldApplicationUser.AppContent);
                Assert.True(applicationUserUpdatedEvent.Changes.HasFlag(ApplicationUserUpdatedEventChanges.AppContent));
            });
        });
    }

    [Fact]
    public async Task Post_AppContentUnchanged_DoesNotSetAppContentChangeFlag()
    {
        // Arrange
        var originalName = "Original Name";
        var emailTemplateId = Guid.NewGuid().ToString();
        var pageContent = "<p>Existing content</p>";

        var applicationUser = await TestData.CreateApplicationUserAsync(
            name: originalName,
            isOidcClient: true,
            appContent: new AppContent
            {
                OneLoginCannotFindRecordEmailTemplateId = emailTemplateId,
                OneLoginNoMatchesPageContentHtml = pageContent
            });

        var newName = TestData.GenerateChangedApplicationUserName(originalName);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/application-users/{applicationUser.UserId}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Name", newName },
                { "ApiRoles", applicationUser.ApiRoles ?? [] },
                { "IsOidcClient", bool.TrueString },
                { "ClientId", applicationUser.ClientId! },
                { "ClientSecret", applicationUser.ClientSecret! },
                { "RedirectUris", applicationUser.RedirectUris ?? [] },
                { "PostLogoutRedirectUris", applicationUser.PostLogoutRedirectUris ?? [] },
                { "OneLoginClientId", applicationUser.OneLoginClientId! },
                { "UseSharedOneLoginSigningKeys", applicationUser.UseSharedOneLoginSigningKeys!.Value.ToString() },
                { "OneLoginPrivateKeyPem", applicationUser.OneLoginPrivateKeyPem! },
                { "OneLoginAuthenticationSchemeName", applicationUser.OneLoginAuthenticationSchemeName! },
                { "OneLoginRedirectUriPath", applicationUser.OneLoginRedirectUriPath! },
                { "OneLoginPostLogoutRedirectUriPath", applicationUser.OneLoginPostLogoutRedirectUriPath! },
                { "RecordMatchingPolicy", applicationUser.RecordMatchingPolicy.ToString() },
                { "OneLoginCannotFindRecordEmailTemplateId", emailTemplateId },
                { "OneLoginNoMatchesPageContentHtml", pageContent }
            }
        };

        Events.Clear();

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        Events.AssertProcessesCreated(p =>
        {
            Assert.Equal(ProcessType.ApplicationUserUpdating, p.ProcessContext.ProcessType);
            p.AssertProcessHasEvents<ApplicationUserUpdatedEvent>(applicationUserUpdatedEvent =>
            {
                Assert.Equal(ApplicationUserUpdatedEventChanges.Name, applicationUserUpdatedEvent.Changes); // Only Name changed
                Assert.False(applicationUserUpdatedEvent.Changes.HasFlag(ApplicationUserUpdatedEventChanges.AppContent)); // AppContent flag NOT set
                Assert.Equal(emailTemplateId, applicationUserUpdatedEvent.ApplicationUser.AppContent!.OneLoginCannotFindRecordEmailTemplateId);
                Assert.Equal(pageContent, applicationUserUpdatedEvent.ApplicationUser.AppContent.OneLoginNoMatchesPageContentHtml);
            });
        });
    }

    [Fact]
    public async Task Post_ClearingAppContent_UpdatesToNullAndCreatesEvent()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync(
            isOidcClient: true,
            appContent: new AppContent
            {
                OneLoginCannotFindRecordEmailTemplateId = Guid.NewGuid().ToString(),
                OneLoginNoMatchesPageContentHtml = "<p>Old content</p>"
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/application-users/{applicationUser.UserId}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Name", applicationUser.Name },
                { "ApiRoles", applicationUser.ApiRoles ?? [] },
                { "IsOidcClient", bool.TrueString },
                { "ClientId", applicationUser.ClientId! },
                { "ClientSecret", applicationUser.ClientSecret! },
                { "RedirectUris", applicationUser.RedirectUris ?? [] },
                { "PostLogoutRedirectUris", applicationUser.PostLogoutRedirectUris ?? [] },
                { "OneLoginClientId", applicationUser.OneLoginClientId! },
                { "UseSharedOneLoginSigningKeys", applicationUser.UseSharedOneLoginSigningKeys!.Value.ToString() },
                { "OneLoginPrivateKeyPem", applicationUser.OneLoginPrivateKeyPem! },
                { "OneLoginAuthenticationSchemeName", applicationUser.OneLoginAuthenticationSchemeName! },
                { "OneLoginRedirectUriPath", applicationUser.OneLoginRedirectUriPath! },
                { "OneLoginPostLogoutRedirectUriPath", applicationUser.OneLoginPostLogoutRedirectUriPath! },
                { "RecordMatchingPolicy", applicationUser.RecordMatchingPolicy.ToString() },
                { "OneLoginCannotFindRecordEmailTemplateId", "" },
                { "OneLoginNoMatchesPageContentHtml", "" }
            }
        };

        Events.Clear();

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        await WithDbContextAsync(async dbContext =>
        {
            applicationUser = await dbContext.ApplicationUsers.SingleAsync(u => u.UserId == applicationUser.UserId);
            Assert.NotNull(applicationUser.AppContent);
            Assert.Null(applicationUser.AppContent.OneLoginCannotFindRecordEmailTemplateId);
            Assert.Null(applicationUser.AppContent.OneLoginNoMatchesPageContentHtml);
        });

        Events.AssertProcessesCreated(p =>
        {
            Assert.Equal(ProcessType.ApplicationUserUpdating, p.ProcessContext.ProcessType);
            p.AssertProcessHasEvents<ApplicationUserUpdatedEvent>(applicationUserUpdatedEvent =>
            {
                Assert.True(applicationUserUpdatedEvent.Changes.HasFlag(ApplicationUserUpdatedEventChanges.AppContent));
            });
        });
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
        string SupportEmailAddressNotifyId,
        string SupportEmailAddress,
        string ExpectedErrorField,
        string ExpectedErrorMessage)[] GetInvalidOidcDetailsData() =>
    [
        (
            Guid.NewGuid().ToString(),
            "Secret0123456789",
            "https://localhost/callback",
            "https://localhost/logout-callback",
            "client_id",
            _privateKeyPem,
            "",  // OneLoginAuthenticationSchemeName
            $"/_onelogin/{Guid.NewGuid:N}/callback",
            $"/_onelogin/{Guid.NewGuid:N}/logout-callback",
            "",
            "",
            "OneLoginAuthenticationSchemeName",
            "Enter an authentication scheme name"
        ),
        (
            Guid.NewGuid().ToString(),
            "Secret0123456789",
            "https://localhost/callback",
            "https://localhost/logout-callback",
            "client_id",
            _privateKeyPem,
            new string('x', 51),  // OneLoginAuthenticationSchemeName
            $"/_onelogin/{Guid.NewGuid:N}/callback",
            $"/_onelogin/{Guid.NewGuid:N}/logout-callback",
            "",
            "",
            "OneLoginAuthenticationSchemeName",
            "Authentication scheme name must be 50 characters or less"
        ),
        (
            Guid.NewGuid().ToString(),
            "Secret0123456789",
            "https://localhost/callback",
            "https://localhost/logout-callback",
            "client_id",
            "",  // OneLoginPrivateKeyPem
            Guid.NewGuid().ToString(),
            $"/_onelogin/{Guid.NewGuid:N}/callback",
            $"/_onelogin/{Guid.NewGuid:N}/logout-callback",
            "",
            "",
            "OneLoginPrivateKeyPem",
            "Enter the One Login private key"
        ),
        (
            Guid.NewGuid().ToString(),
            "Secret0123456789",
            "https://localhost/callback",
            "https://localhost/logout-callback",
            "",  // OneLoginClientId
            _privateKeyPem,
            Guid.NewGuid().ToString(),
            $"/_onelogin/{Guid.NewGuid:N}/callback",
            $"/_onelogin/{Guid.NewGuid:N}/logout-callback",
            "",
            "",
            "OneLoginClientId",
            "Enter the One Login client ID"
        ),
        (
            Guid.NewGuid().ToString(),
            "Secret0123456789",
            "https://localhost/callback",
            "https://localhost/logout-callback",
            "client_id",
            _privateKeyPem,
            Guid.NewGuid().ToString(),
            "",  // OneLoginRedirectUriPath
            $"/_onelogin/{Guid.NewGuid:N}/logout-callback",
            "",
            "",
            "OneLoginRedirectUriPath",
            "Enter the One Login redirect URI"
        ),
        (
            Guid.NewGuid().ToString(),
            "Secret0123456789",
            "https://localhost/callback",
            "https://localhost/logout-callback",
            "client_id",
            _privateKeyPem,
            Guid.NewGuid().ToString(),
            $"/_onelogin/{Guid.NewGuid:N}/callback",
            "",  // OneLoginPostLogoutRedirectUriPath
            "",
            "",
            "OneLoginPostLogoutRedirectUriPath",
            "Enter the One Login post logout redirect URI"
        ),
        (
            Guid.NewGuid().ToString(),
            "Secret0123456789",
            "https://localhost/callback",
            "https://localhost/logout-callback",
            "client_id",
            _privateKeyPem,
            Guid.NewGuid().ToString(),
            new string('x', 151),  // OneLoginRedirectUriPath
            $"/_onelogin/{Guid.NewGuid:N}/logout-callback",
            "",
            "",
            "OneLoginRedirectUriPath",
            "One Login redirect URI must be 150 characters or less"
        ),
        (
            Guid.NewGuid().ToString(),
            "Secret0123456789",
            "https://localhost/callback",
            "https://localhost/logout-callback",
            "client_id",
            _privateKeyPem,
            Guid.NewGuid().ToString(),
            $"/_onelogin/{Guid.NewGuid:N}/callback",
            new string('x', 151),  // OneLoginPostLogoutRedirectUriPath
            "",
            "",
            "OneLoginPostLogoutRedirectUriPath",
            "One Login post logout redirect URI must be 150 characters or less"
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
            "",
            "",
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
            "",
            "",
            "ClientId",
            "Client ID must be 50 characters or less"
        ),
        (
            Guid.NewGuid().ToString(),
            "",
            "https://localhost/callback",
            "https://localhost/logout-callback",
            "client_id",
            _privateKeyPem,
            Guid.NewGuid().ToString(),
            $"/_onelogin/{Guid.NewGuid:N}/callback",
            $"/_onelogin/{Guid.NewGuid:N}/logout-callback",
            "",
            "",
            "ClientSecret",
            "Enter a client secret"
        ),
        (
            Guid.NewGuid().ToString(),
            new string('x', 201),
            "https://localhost/callback",
            "https://localhost/logout-callback",
            "client_id",
            _privateKeyPem,
            Guid.NewGuid().ToString(),
            $"/_onelogin/{Guid.NewGuid:N}/callback",
            $"/_onelogin/{Guid.NewGuid:N}/logout-callback",
            "",
            "",
            "ClientSecret",
            "Client secret must be 200 characters or less"
        ),
        (
            Guid.NewGuid().ToString(),
            "S",
            "https://localhost/callback",
            "https://localhost/logout-callback",
            "client_id",
            _privateKeyPem,
            Guid.NewGuid().ToString(),
            $"/_onelogin/{Guid.NewGuid:N}/callback",
            $"/_onelogin/{Guid.NewGuid:N}/logout-callback",
            "",
            "",
            "ClientSecret",
            "Client secret must be at least 16 characters"
        ),
        (
            Guid.NewGuid().ToString(),
            "Secret0123456789",
            "foo",
            "https://localhost/logout-callback",
            "client_id",
            _privateKeyPem,
            Guid.NewGuid().ToString(),
            $"/_onelogin/{Guid.NewGuid:N}/callback",
            $"/_onelogin/{Guid.NewGuid:N}/logout-callback",
            "",
            "",
            "RedirectUris",
            "One or more redirect URIs are not valid"
        ),
        (
            Guid.NewGuid().ToString(),
            "Secret0123456789",
            "https://localhost/callback",
            "foo",
            "client_id",
            _privateKeyPem,
            Guid.NewGuid().ToString(),
            $"/_onelogin/{Guid.NewGuid:N}/callback",
            $"/_onelogin/{Guid.NewGuid:N}/logout-callback",
            "",
            "",
            "PostLogoutRedirectUris",
            "One or more post logout redirect URIs are not valid"
        ),
        (
            Guid.NewGuid().ToString(),
            "Secret0123456789",
            "https://localhost/callback",
            "https://localhost/logout-callback",
            "client_id",
            _privateKeyPem,
            Guid.NewGuid().ToString(),
            $"/_onelogin/{Guid.NewGuid:N}/callback",
            $"/_onelogin/{Guid.NewGuid:N}/logout-callback",
            "not-a-valid-guid",  // SupportEmailAddressNotifyId
            "",
            "SupportEmailAddressNotifyId",
            "Support email address Notify ID must be a valid GUID"
        ),
        (
            Guid.NewGuid().ToString(),
            "Secret0123456789",
            "https://localhost/callback",
            "https://localhost/logout-callback",
            "client_id",
            _privateKeyPem,
            Guid.NewGuid().ToString(),
            $"/_onelogin/{Guid.NewGuid:N}/callback",
            $"/_onelogin/{Guid.NewGuid:N}/logout-callback",
            "",
            "not-a-valid-email",  // SupportEmailAddress
            "SupportEmailAddress",
            "Enter a valid email address"
        )
    ];

    private static readonly string _privateKeyPem = RSA.Create().ExportPkcs8PrivateKeyPem();
}
