namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Teams;

[ClearDbBeforeTest, Collection(nameof(DisableParallelization))]
public class TeamTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_NoTeams_ShowsEmptyState()
    {
        // Arrange
        SetCurrentUser(await TestData.CreateUserAsync(role: UserRoles.Administrator));

        // Act
        var response = await HttpClient.GetAsync("/teams");

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Contains("No teams have been added yet.", doc.Body!.TextContent);
        Assert.Null(doc.QuerySelector("table"));
        Assert.NotNull(doc.QuerySelector("a[href='/teams/add']"));
    }

    [Theory]
    [InlineData(null, StatusCodes.Status403Forbidden)]
    [InlineData(UserRoles.Viewer, StatusCodes.Status403Forbidden)]
    [InlineData(UserRoles.RecordManager, StatusCodes.Status403Forbidden)]
    [InlineData(UserRoles.AlertsManagerTra, StatusCodes.Status403Forbidden)]
    [InlineData(UserRoles.AlertsManagerTraDbs, StatusCodes.Status403Forbidden)]
    [InlineData(UserRoles.AccessManager, StatusCodes.Status200OK)]
    [InlineData(UserRoles.Administrator, StatusCodes.Status200OK)]
    public async Task Get_TeamPages_OnlyTeamManagementRolesCanView(string? role, int expectedStatusCode)
    {
        // Arrange
        SetCurrentUser(await TestData.CreateUserAsync(role: role));

        // Act
        var listResponse = await HttpClient.GetAsync("/teams");
        var addResponse = await HttpClient.GetAsync("/teams/add");

        // Assert
        Assert.Equal(expectedStatusCode, (int)listResponse.StatusCode);
        Assert.Equal(expectedStatusCode, (int)addResponse.StatusCode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(UserRoles.Viewer)]
    [InlineData(UserRoles.RecordManager)]
    [InlineData(UserRoles.AlertsManagerTra)]
    [InlineData(UserRoles.AlertsManagerTraDbs)]
    public async Task Post_WithoutTeamManagementRole_CannotCreateTeam(string? role)
    {
        // Arrange
        SetCurrentUser(await TestData.CreateUserAsync(role: role));
        var request = new HttpRequestMessage(HttpMethod.Post, "/teams/add")
        {
            Content = new FormUrlEncodedContentBuilder { { "Name", $"Team {Guid.NewGuid()}" } }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Theory]
    [InlineData("/teams")]
    [InlineData("/teams/add")]
    [InlineData("/teams/9b48932e-d497-4ea2-a6c3-384e2e3b913c")]
    [InlineData("/teams/9b48932e-d497-4ea2-a6c3-384e2e3b913c/delete")]
    public async Task Get_WithoutPermission_ReturnsForbidden(string path)
    {
        // Arrange
        SetCurrentUser(await TestData.CreateUserAsync(role: UserRoles.Viewer));

        // Act
        var response = await HttpClient.GetAsync(path);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Theory]
    [InlineData(UserRoles.AccessManager)]
    [InlineData(UserRoles.Administrator)]
    public async Task Post_ValidName_CreatesTeamAndEvent(string role)
    {
        // Arrange
        SetCurrentUser(await TestData.CreateUserAsync(role: role));
        var name = $"Team {Guid.NewGuid()}";
        var request = new HttpRequestMessage(HttpMethod.Post, "/teams/add")
        {
            Content = new FormUrlEncodedContentBuilder { { "Name", name } }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal("/teams", response.Headers.Location?.OriginalString);

        var team = await WithDbContextAsync(dbContext => dbContext.Teams.SingleAsync(t => t.Name == name));
        Assert.NotEqual(Guid.Empty, team.TeamId);

        Events.AssertProcessesCreated(p =>
        {
            Assert.Equal(ProcessType.TeamCreating, p.ProcessContext.ProcessType);
            p.AssertProcessHasEvents<TeamCreatedEvent>(e =>
            {
                Assert.Equal(team.TeamId, e.Team.TeamId);
                Assert.Equal(name, e.Team.Name);
            });
        });

        var listResponse = await response.FollowRedirectAsync(HttpClient);
        var listHtml = await AssertEx.HtmlResponseAsync(listResponse);
        Assert.Contains($"{name} team added", listHtml.Body!.TextContent);
        Assert.Contains($"/teams/{team.TeamId}", listHtml.Body.InnerHtml);
        Assert.Contains($"/teams/{team.TeamId}/delete", listHtml.Body.InnerHtml);
    }

    [Fact]
    public async Task Post_EmptyName_ShowsValidationError()
    {
        // Arrange
        SetCurrentUser(await TestData.CreateUserAsync(role: UserRoles.Administrator));
        var request = new HttpRequestMessage(HttpMethod.Post, "/teams/add")
        {
            Content = new FormUrlEncodedContentBuilder { { "Name", "" } }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "Name", "Enter a team name");
    }

    [Fact]
    public async Task Get_DeleteTeam_ShowsTeamMembersInNameOrder()
    {
        // Arrange
        var team = await TestData.CreateTeamAsync("Example team");
        var otherTeam = await TestData.CreateTeamAsync();
        var users = await TestData.CreateMultipleUsersAsync(
            new() { Name = "Anna Zebra", TeamId = team.TeamId },
            new() { Name = "Zoe Adams", TeamId = team.TeamId },
            new() { Name = "Una Other", TeamId = otherTeam.TeamId });
        SetCurrentUser(await TestData.CreateUserAsync(role: UserRoles.AccessManager));

        // Act
        var response = await HttpClient.GetAsync($"/teams/{team.TeamId}/delete");

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal("Delete a team", doc.QuerySelector("h1")?.TextContent.Trim());
        Assert.Contains("Are you sure you want to delete the team Example team?", doc.Body!.TextContent);
        Assert.Contains("Doing this will mean team members will be automatically unassigned from this team.", doc.Body.TextContent);
        Assert.Contains("Members on this team", doc.QuerySelector("details summary")?.TextContent);
        Assert.Equal(new[] { "Anna Zebra", "Zoe Adams" }, doc.QuerySelectorAll("details li").Select(li => li.TextContent.Trim()).ToArray());
        Assert.DoesNotContain(users.Single(u => u.TeamId == otherTeam.TeamId).Name, doc.Body.TextContent);
        Assert.Contains("Yes, delete this team", doc.QuerySelector("button.govuk-button--warning")?.TextContent);
        Assert.Equal("/teams", doc.QuerySelector("a.govuk-button--secondary")?.GetAttribute("href"));

        var cancelResponse = await HttpClient.GetAsync(doc.QuerySelector("a.govuk-button--secondary")!.GetAttribute("href"));
        var teamList = await AssertEx.HtmlResponseAsync(cancelResponse);
        Assert.Contains(team.Name, teamList.Body!.TextContent);
        Assert.Null(await WithDbContextAsync(dbContext => dbContext.Teams.Where(t => t.TeamId == team.TeamId).Select(t => t.DeletedOn).SingleAsync()));
    }

    [Theory]
    [InlineData(UserRoles.AccessManager)]
    [InlineData(UserRoles.Administrator)]
    public async Task Post_DeleteTeam_UnassignsMembersAndHidesTeam(string role)
    {
        // Arrange
        var team = await TestData.CreateTeamAsync("Example team");
        var otherTeam = await TestData.CreateTeamAsync("Other team");
        var users = await TestData.CreateMultipleUsersAsync(
            new() { Name = "Anna Zebra", TeamId = team.TeamId },
            new() { Name = "Zoe Adams", TeamId = team.TeamId },
            new() { Name = "Una Other", TeamId = otherTeam.TeamId });
        var memberIds = users.Where(u => u.TeamId == team.TeamId).Select(u => u.UserId).ToArray();
        var otherUserId = users.Single(u => u.TeamId == otherTeam.TeamId).UserId;
        SetCurrentUser(await TestData.CreateUserAsync(role: role));

        // Act
        var response = await HttpClient.PostAsync($"/teams/{team.TeamId}/delete", new FormUrlEncodedContent([]));

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal("/teams", response.Headers.Location?.OriginalString);
        var deletedTeam = await WithDbContextAsync(dbContext => dbContext.Teams.IgnoreQueryFilters().SingleAsync(t => t.TeamId == team.TeamId));
        Assert.Equal(TimeProvider.UtcNow, deletedTeam.DeletedOn);
        var savedMembers = await WithDbContextAsync(dbContext => dbContext.Users.Where(u => memberIds.Contains(u.UserId)).ToArrayAsync());
        Assert.Equal(2, savedMembers.Length);
        Assert.All(savedMembers, u => Assert.Null(u.TeamId));
        Assert.Equal(otherTeam.TeamId, await WithDbContextAsync(dbContext => dbContext.Users.Where(u => u.UserId == otherUserId).Select(u => u.TeamId).SingleAsync()));

        Events.AssertProcessesCreated(p =>
        {
            Assert.Equal(ProcessType.TeamDeleting, p.ProcessContext.ProcessType);
            Assert.Equal(3, p.Events.Count);
            var deletedEvent = Assert.Single(p.Events.OfType<TeamDeletedEvent>());
            Assert.Equal(team.TeamId, deletedEvent.Team.TeamId);
            Assert.Equal(team.Name, deletedEvent.Team.Name);

            var userEvents = p.Events.OfType<UserUpdatedEvent>().ToArray();
            Assert.Equal(memberIds.Order(), userEvents.Select(e => e.User.UserId).Order());
            Assert.All(userEvents, e =>
            {
                Assert.Null(e.User.TeamId);
                Assert.Equal(UserUpdatedEventChanges.Team, e.Changes);
            });
        });

        var listResponse = await response.FollowRedirectAsync(HttpClient);
        var doc = await AssertEx.HtmlResponseAsync(listResponse);
        Assert.Contains("Example team team deleted", doc.Body!.TextContent);
        Assert.DoesNotContain($"/teams/{team.TeamId}/delete", doc.Body.InnerHtml);
    }

    [Fact]
    public async Task Post_DeleteMissingTeam_ReturnsNotFound()
    {
        // Arrange
        SetCurrentUser(await TestData.CreateUserAsync(role: UserRoles.Administrator));

        // Act
        var response = await HttpClient.PostAsync($"/teams/{Guid.NewGuid()}/delete", new FormUrlEncodedContent([]));

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_DeleteMissingTeam_ReturnsNotFound()
    {
        // Arrange
        SetCurrentUser(await TestData.CreateUserAsync(role: UserRoles.Administrator));

        // Act
        var response = await HttpClient.GetAsync($"/teams/{Guid.NewGuid()}/delete");

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_DeleteTeamWithoutPermission_ReturnsForbidden()
    {
        // Arrange
        var team = await TestData.CreateTeamAsync();
        SetCurrentUser(await TestData.CreateUserAsync(role: UserRoles.Viewer));

        // Act
        var response = await HttpClient.PostAsync($"/teams/{team.TeamId}/delete", new FormUrlEncodedContent([]));

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
        Assert.Null(await WithDbContextAsync(dbContext => dbContext.Teams.Where(t => t.TeamId == team.TeamId).Select(t => t.DeletedOn).SingleAsync()));
    }
}
