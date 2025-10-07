using TeachingRecordSystem.Core.Events.Legacy;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Users.EditUser;

public class IndexTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Test]
    public async Task Get_UserWithoutAccessManagerRole_ReturnsForbidden()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(role: UserRoles.RecordManager);
        SetCurrentUser(user);

        var userId = Guid.NewGuid();

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(userId));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert

        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Test]
    public async Task Get_UserIdDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(role: UserRoles.AccessManager);
        SetCurrentUser(user);

        var userId = Guid.NewGuid();

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(userId));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Test]
    public async Task Get_UserWithoutAdministratorRole_CannotViewAdministratorInRoleOptions()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(role: UserRoles.AccessManager);
        SetCurrentUser(user);

        var existingUser = await TestData.CreateUserAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(existingUser.UserId));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        var html = await AssertEx.HtmlResponseAsync(response);
        var roleNames = html.QuerySelectorAll(".trs-user-roles input[type=radio]").Select(e => e.Attributes["value"]!.Value);

        Assert.DoesNotContain(UserRoles.Administrator, roleNames);
    }

    [Test]
    public async Task Get_UserWithAdministratorRole_CanViewAdministratorInRoleOptions()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(role: UserRoles.Administrator);
        SetCurrentUser(user);

        var existingUser = await TestData.CreateUserAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(existingUser.UserId));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        var html = await AssertEx.HtmlResponseAsync(response);
        var roleNames = html.QuerySelectorAll(".trs-user-roles input[type=radio]").Select(e => e.Attributes["value"]!.Value);

        Assert.Contains(UserRoles.Administrator, roleNames);
    }

    [Test]
    public async Task Post_UserWithoutAccessManagerRole_ReturnsForbidden()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(role: UserRoles.RecordManager);
        SetCurrentUser(user);

        var existingUser = await TestData.CreateUserAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(existingUser.UserId))
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Name", user.Name },
                { "Role", user.Role! }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Test]
    public async Task Post_UserIdDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(role: UserRoles.AccessManager);
        SetCurrentUser(user);

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(Guid.NewGuid()))
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Name", "Some Name" },
                { "Role", UserRoles.RecordManager }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Test]
    public async Task Post_NoName_RendersError()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(role: UserRoles.AccessManager);
        SetCurrentUser(user);

        var existingUser = await TestData.CreateUserAsync();
        const string role = UserRoles.AlertsManagerTra;

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(existingUser.UserId))
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Role", role }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
        await AssertEx.HtmlResponseHasErrorAsync(response, "Name", "Enter a name");
    }

    [Test]
    public async Task Post_NoRole_RendersError()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(role: UserRoles.AccessManager);
        SetCurrentUser(user);

        var existingUser = await TestData.CreateUserAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(existingUser.UserId))
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Name", "New Name" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
        await AssertEx.HtmlResponseHasErrorAsync(response, "Role", "Select a role");
    }

    [Test]
    public async Task Post_UserWithoutAdministratorRole_UpdatingUserWithNonExistentRole_ReturnsBadRequest()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(role: UserRoles.AccessManager);
        SetCurrentUser(user);

        var existingUser = await TestData.CreateUserAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(existingUser.UserId))
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Name", existingUser.Name },
                { "Role", "XXXXXX" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Test]
    public async Task Post_UserWithoutAdministratorRole_UpdatingUserWithAdministratorRole_ReturnsBadRequest()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(role: UserRoles.AccessManager);
        SetCurrentUser(user);

        var existingUser = await TestData.CreateUserAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(existingUser.UserId))
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Name", existingUser.Name },
                { "Role", UserRoles.Administrator }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Test]
    public async Task Get_UserWithAdministratorRole_UpdatingUserWithAdministratorRole_ReturnsFound()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(role: UserRoles.Administrator);
        SetCurrentUser(user);

        var existingUser = await TestData.CreateUserAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(existingUser.UserId))
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Name", existingUser.Name },
                { "Role", UserRoles.Administrator }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
    }

    [Test]
    [Arguments(true, false, true, UserUpdatedEventChanges.Name, "has been updated.")]
    [Arguments(false, true, true, UserUpdatedEventChanges.Roles, "has been changed to an alerts manager (TRA decisions)")]
    [Arguments(true, true, true, UserUpdatedEventChanges.Name | UserUpdatedEventChanges.Roles, "has been changed to an alerts manager (TRA decisions)")]
    [Arguments(false, false, false, UserUpdatedEventChanges.None, "has been updated.")]
    public async Task Post_ValidRequest_CreatesUserEmitsEventAndRedirectsWithFlashMessage(
        bool changeName,
        bool changeRole,
        bool expectedEvent,
        UserUpdatedEventChanges expectedChanges,
        string expectedFlashMessage)
    {
        var user = await TestData.CreateUserAsync(role: UserRoles.AccessManager);
        SetCurrentUser(user);

        // Arrange
        var existingUser = await TestData.CreateUserAsync(role: UserRoles.RecordManager);
        var newName = changeName ? TestData.GenerateChangedName(existingUser.Name) : existingUser.Name;
        var newRole = changeRole ? UserRoles.AlertsManagerTra : existingUser.Role;

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(existingUser.UserId))
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Name", newName },
                { "Role", newRole! }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var updatedUser = await WithDbContext(dbContext =>
            dbContext.Users.SingleOrDefaultAsync(u => u.UserId == existingUser.UserId));
        Assert.NotNull(updatedUser);

        Assert.Equal(UserType.Person, updatedUser.UserType);
        Assert.Equal(existingUser.Email, updatedUser.Email);
        Assert.Equal(existingUser.AzureAdUserId, updatedUser.AzureAdUserId);
        Assert.Equal(newName, updatedUser.Name);
        Assert.Equal(newRole, updatedUser.Role);

        if (expectedEvent)
        {
            EventObserver.AssertEventsSaved(e =>
            {
                var userCreatedEvent = Assert.IsType<UserUpdatedEvent>(e);
                Assert.Equal(Clock.UtcNow, userCreatedEvent.CreatedUtc);
                Assert.Equal(userCreatedEvent.RaisedBy.UserId, GetCurrentUserId());
                Assert.Equal(newName, userCreatedEvent.User.Name);
                Assert.Equal(updatedUser.Email, userCreatedEvent.User.Email);
                Assert.Equal(updatedUser.AzureAdUserId, userCreatedEvent.User.AzureAdUserId);
                Assert.Equal(expectedChanges, userCreatedEvent.Changes);
                Assert.Equal(newRole, userCreatedEvent.User.Role);
            });
        }
        else
        {
            EventObserver.AssertNoEventsSaved();
        }

        var redirectResponse = await response.FollowRedirectAsync(HttpClient);
        var redirectDoc = await redirectResponse.GetDocumentAsync();
        AssertEx.HtmlDocumentHasFlashSuccess(redirectDoc, $"{newName} {expectedFlashMessage}");
    }

    [Test]
    public async Task PostActivate_UserWithoutAccessManagerRole_ReturnsForbidden()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(role: UserRoles.RecordManager);
        SetCurrentUser(user);

        var existingUser = await TestData.CreateUserAsync(role: UserRoles.RecordManager);

        var request = new HttpRequestMessage(HttpMethod.Post, $"{GetRequestPath(existingUser.UserId)}/activate");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert

        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Test]
    public async Task PostActivate_UserIdDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(role: UserRoles.AccessManager);
        SetCurrentUser(user);

        var request = new HttpRequestMessage(HttpMethod.Post, $"{GetRequestPath(Guid.NewGuid())}/activate");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Test]
    public async Task PostActivate_UserExistsButIsAlreadyActive_ReturnsBadRequest()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(role: UserRoles.AccessManager);
        SetCurrentUser(user);

        var existingUser = await TestData.CreateUserAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, $"{GetRequestPath(existingUser.UserId)}/activate");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Test]
    public async Task PostActivate_UserWithoutAdministratorRole_ActivatingUserWithAdministratorRole_ReturnsBadRequest()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(role: UserRoles.AccessManager);
        SetCurrentUser(user);

        var existingUser = await TestData.CreateUserAsync(active: false, role: UserRoles.Administrator);

        var request = new HttpRequestMessage(HttpMethod.Post, $"{GetRequestPath(existingUser.UserId)}/activate");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Test]
    public async Task PostActivate_UserWithAdministratorRole_ActivatingUserWithAdministratorRole_ReturnsFound()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(role: UserRoles.Administrator);
        SetCurrentUser(user);

        var existingUser = await TestData.CreateUserAsync(active: false, role: UserRoles.Administrator);

        var request = new HttpRequestMessage(HttpMethod.Post, $"{GetRequestPath(existingUser.UserId)}/activate");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
    }

    [Test]
    public async Task PostActivate_ValidRequest_ActivatesUserEmitsEventAndRedirectsWithFlashMessage()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(role: UserRoles.AccessManager);
        SetCurrentUser(user);

        var existingUser = await TestData.CreateUserAsync(active: false);
        var request = new HttpRequestMessage(HttpMethod.Post, $"{GetRequestPath(existingUser.UserId)}/activate");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var updatedUser = await WithDbContext(dbContext =>
            dbContext.Users.SingleOrDefaultAsync(u => u.UserId == existingUser.UserId));
        Assert.NotNull(updatedUser);

        Assert.True(updatedUser.Active);

        EventObserver.AssertEventsSaved(e =>
        {
            var userCreatedEvent = Assert.IsType<UserActivatedEvent>(e);
            Assert.Equal(Clock.UtcNow, userCreatedEvent.CreatedUtc);
            Assert.Equal(userCreatedEvent.RaisedBy.UserId, GetCurrentUserId());
        });

        var redirectResponse = await response.FollowRedirectAsync(HttpClient);
        var redirectDoc = await redirectResponse.GetDocumentAsync();
        AssertEx.HtmlDocumentHasFlashSuccess(redirectDoc, $"{existingUser.Name}\u2019s account has been reactivated");
    }

    private static string GetRequestPath(Guid userId) => $"/users/{userId}";
}
