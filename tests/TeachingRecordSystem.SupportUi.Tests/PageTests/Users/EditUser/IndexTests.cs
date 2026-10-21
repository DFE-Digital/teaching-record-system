namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Users.EditUser;

public class IndexTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
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

    [Fact]
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

    [Fact]
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

    [Fact]
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

    [Fact]
    public async Task Get_ValidRequest_RendersTitleAndTeamOptionsSortedByNameWithCurrentTeamSelected()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(role: UserRoles.AccessManager);
        SetCurrentUser(user);

        var prefix = Guid.NewGuid().ToString("N");
        var teamB = await TestData.CreateTeamAsync($"{prefix} B");
        var teamA = await TestData.CreateTeamAsync($"{prefix} A");

        var existingUser = await TestData.CreateUserAsync(teamId: teamB.TeamId);

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(existingUser.UserId));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        var html = await AssertEx.HtmlResponseAsync(response);

        Assert.Equal($"Change {existingUser.Name}\u2019s details", html.QuerySelector("h1")!.TextContent.Trim());

        var teamOptions = html.QuerySelectorAll("select[name=TeamId] option").ToArray();

        Assert.Equal("", teamOptions.First().Attributes["value"]!.Value);
        Assert.Equal("", teamOptions.First().TextContent.Trim());
        Assert.Equal(
            [teamA.TeamId.ToString(), teamB.TeamId.ToString()],
            teamOptions
                .Select(e => e.Attributes["value"]!.Value)
                .Where(v => v == teamA.TeamId.ToString() || v == teamB.TeamId.ToString()));

        var selectedOption = Assert.Single(teamOptions, e => e.HasAttribute("selected"));
        Assert.Equal(teamB.TeamId.ToString(), selectedOption.Attributes["value"]!.Value);
    }

    [Fact]
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

    [Fact]
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

    [Fact]
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

    [Fact]
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

    [Fact]
    public async Task Post_InvalidActiveUser_RendersEditableFormWithSaveButton()
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
        var html = await AssertEx.HtmlResponseAsync(response, StatusCodes.Status400BadRequest);
        Assert.False(html.QuerySelector("input[name=Name]")!.HasAttribute("disabled"));
        Assert.False(html.QuerySelector("select[name=TeamId]")!.HasAttribute("disabled"));
        Assert.All(html.QuerySelectorAll(".trs-user-roles input[type=radio]"), e => Assert.False(e.HasAttribute("disabled")));
        Assert.Contains(html.QuerySelectorAll("button"), e => e.TrimmedText() == "Save changes");
    }

    [Fact]
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

    [Fact]
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

    [Fact]
    public async Task Post_NonExistentTeam_ReturnsBadRequest()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(role: UserRoles.AccessManager);
        SetCurrentUser(user);

        var existingUser = await TestData.CreateUserAsync(role: UserRoles.RecordManager);

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(existingUser.UserId))
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Name", existingUser.Name },
                { "Role", existingUser.Role! },
                { "TeamId", Guid.NewGuid() }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
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

    [Theory]
    [InlineData(true, false, false, true, UserUpdatedEventChanges.Name, "has been updated.")]
    [InlineData(false, true, false, true, UserUpdatedEventChanges.Roles, "has been changed to an alerts manager (TRA decisions)")]
    [InlineData(false, false, true, true, UserUpdatedEventChanges.Team, "has been updated.")]
    [InlineData(true, true, true, true, UserUpdatedEventChanges.Name | UserUpdatedEventChanges.Roles | UserUpdatedEventChanges.Team, "has been changed to an alerts manager (TRA decisions)")]
    [InlineData(false, false, false, false, UserUpdatedEventChanges.None, "has been updated.")]
    public async Task Post_ValidRequest_CreatesUserEmitsEventAndRedirectsWithFlashMessage(
        bool changeName,
        bool changeRole,
        bool changeTeam,
        bool expectedEvent,
        UserUpdatedEventChanges expectedChanges,
        string expectedFlashMessage)
    {
        var user = await TestData.CreateUserAsync(role: UserRoles.AccessManager);
        SetCurrentUser(user);

        // Arrange
        var existingTeam = await TestData.CreateTeamAsync();
        var existingUser = await TestData.CreateUserAsync(role: UserRoles.RecordManager, teamId: existingTeam.TeamId);
        var newName = changeName ? TestData.GenerateChangedName(existingUser.Name) : existingUser.Name;
        var newRole = changeRole ? UserRoles.AlertsManagerTra : existingUser.Role;
        var newTeamId = changeTeam ? (await TestData.CreateTeamAsync()).TeamId : existingTeam.TeamId;

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(existingUser.UserId))
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Name", newName },
                { "Role", newRole! },
                { "TeamId", newTeamId }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var updatedUser = await WithDbContextAsync(dbContext =>
            dbContext.Users.SingleOrDefaultAsync(u => u.UserId == existingUser.UserId));
        Assert.NotNull(updatedUser);

        Assert.Equal(UserType.Person, updatedUser.UserType);
        Assert.Equal(existingUser.Email, updatedUser.Email);
        Assert.Equal(existingUser.AzureAdUserId, updatedUser.AzureAdUserId);
        Assert.Equal(newName, updatedUser.Name);
        Assert.Equal(newRole, updatedUser.Role);
        Assert.Equal(newTeamId, updatedUser.TeamId);

        if (expectedEvent)
        {
            Events.AssertProcessesCreated(p =>
            {
                Assert.Equal(ProcessType.UserUpdating, p.ProcessContext.ProcessType);
                Assert.Equal(TimeProvider.UtcNow, p.ProcessContext.Process.CreatedOn);
                Assert.Equal(GetCurrentUserId(), p.ProcessContext.Process.UserId);
                p.AssertProcessHasEvents<UserUpdatedEvent>(userUpdatedEvent =>
                {
                    Assert.Equal(newName, userUpdatedEvent.User.Name);
                    Assert.Equal(updatedUser.Email, userUpdatedEvent.User.Email);
                    Assert.Equal(updatedUser.AzureAdUserId, userUpdatedEvent.User.AzureAdUserId);
                    Assert.Equal(expectedChanges, userUpdatedEvent.Changes);
                    Assert.Equal(newRole, userUpdatedEvent.User.Role);
                    Assert.Equal(newTeamId, userUpdatedEvent.User.TeamId);
                });
            });
        }
        else
        {
            Events.AssertNoEventsPublished();
        }

        var redirectResponse = await response.FollowRedirectAsync(HttpClient);
        var redirectDoc = await redirectResponse.GetDocumentAsync();
        AssertEx.HtmlDocumentHasFlashNotificationBanner(redirectDoc, $"{newName} {expectedFlashMessage}");
    }

    [Fact]
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

    [Fact]
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

    [Fact]
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

    [Fact]
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

    [Fact]
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

    [Fact]
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

        var updatedUser = await WithDbContextAsync(dbContext =>
            dbContext.Users.SingleOrDefaultAsync(u => u.UserId == existingUser.UserId));
        Assert.NotNull(updatedUser);

        Assert.True(updatedUser.Active);

        Events.AssertProcessesCreated(p =>
        {
            Assert.Equal(ProcessType.UserActivating, p.ProcessContext.ProcessType);
            Assert.Equal(TimeProvider.UtcNow, p.ProcessContext.Process.CreatedOn);
            Assert.Equal(GetCurrentUserId(), p.ProcessContext.Process.UserId);
            p.AssertProcessHasEvents<UserActivatedEvent>();
        });

        var redirectResponse = await response.FollowRedirectAsync(HttpClient);
        var redirectDoc = await redirectResponse.GetDocumentAsync();
        AssertEx.HtmlDocumentHasFlashNotificationBanner(redirectDoc, $"{existingUser.Name}\u2019s account has been reactivated");
    }

    private static string GetRequestPath(Guid userId) => $"/users/{userId}";
}
