namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Teams;

[ClearDbBeforeTest, Collection(nameof(DisableParallelization))]
public class TeamTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Theory]
    [InlineData(null, StatusCodes.Status403Forbidden)]
    [InlineData(UserRoles.Viewer, StatusCodes.Status403Forbidden)]
    [InlineData(UserRoles.RecordManager, StatusCodes.Status403Forbidden)]
    [InlineData(UserRoles.AlertsManagerTra, StatusCodes.Status403Forbidden)]
    [InlineData(UserRoles.AlertsManagerTraDbs, StatusCodes.Status403Forbidden)]
    [InlineData(UserRoles.AccessManager, StatusCodes.Status200OK)]
    [InlineData(UserRoles.Administrator, StatusCodes.Status200OK)]
    public async Task Get_TeamPages_OnlyUserManagementRolesCanView(string? role, int expectedStatusCode)
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
    public async Task Post_WithoutUserManagementRole_CannotCreateTeam(string? role)
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
        SetCurrentUser(await TestData.CreateUserAsync(role: UserRoles.RecordManager));

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
                Assert.Equal(team.TeamId, e.TeamId);
                Assert.Equal(name, e.Name);
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
}
