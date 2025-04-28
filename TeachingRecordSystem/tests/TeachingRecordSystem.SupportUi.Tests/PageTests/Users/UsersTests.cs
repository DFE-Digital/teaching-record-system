using AngleSharp.Dom;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Users;

[Collection(nameof(DisableParallelization))]
public class UsersTests : TestBase, IAsyncLifetime
{
    private const string RequestPath = "/users";

    public UsersTests(HostFixture hostFixture) : base(hostFixture)
    {
        TestScopedServices.GetCurrent().FeatureProvider.Features.Add(FeatureNames.NewUserRoles);
    }

    public async Task InitializeAsync()
    {
        await WithDbContext(dbContext => dbContext.Users.ExecuteDeleteAsync());
        TestUsers.ClearCache();
    }

    public Task DisposeAsync()
    {
        TestScopedServices.GetCurrent().FeatureProvider.Features.Remove(FeatureNames.NewUserRoles);
        return Task.CompletedTask;
    }

    [Fact]
    public async Task Get_UserWithoutAccessManagerRole_ReturnsForbidden()
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(role: null));

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, RequestPath);
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_UserWithAccessManagerRole_ReturnsOk()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(role: UserRoles.AccessManager);
        SetCurrentUser(user);

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, RequestPath);
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_ValidRequestAndUsersFound_RendersUsers()
    {
        // Arrange
        var user = await TestData.CreateUserAsync();
        SetCurrentUser(user);

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, RequestPath);
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        var doc = await AssertEx.HtmlResponseAsync(response);
        var element = doc.GetElementByTestId($"user-{user.UserId}")!.InnerHtml;
        Assert.NotNull(element);
        Assert.Contains(user.Name, element);
        Assert.Contains(user.Email!, element);
    }

    [Theory]
    [InlineData("?page=1", 10, "Auser", "Juser")]
    [InlineData("?page=2", 10, "Kuser", "Tuser")]
    [InlineData("?page=3", 2, "Uuser", "Vuser")]
    public async Task Get_ValidRequestAndUsersFound_PaginatesUsersAndSortsByFirstName(string query, int expectedUserCount, string expectedFirstUserOnPage, string expectedLastUserOnPage)
    {
        // Arrange
        var users = await TestData.CreateMultipleUsersAsync(22, i =>
            new() { Name = $"{(char)('A' + i)}user {Faker.Name.Last()}", Role = UserRoles.AccessManager });
        SetCurrentUser(users[0]);

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, $"{RequestPath}{query}");
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        var html = await AssertEx.HtmlResponseAsync(response);
        var userElements = html.GetElementsByClassName("trs-user");

        Assert.Equal(expectedUserCount, userElements.Length);
        Assert.Contains(expectedFirstUserOnPage, userElements[0].InnerHtml);
        Assert.Contains(expectedLastUserOnPage, userElements[expectedUserCount - 1].InnerHtml);
    }

    [Theory]
    [InlineData("?keywords=user%20laSTName1", new string[] { "Auser", "Buser", "Cuser", "Duser" })]
    [InlineData("?keywords=USER%40org2", new string[] { "Duser", "Euser", "Fuser" })]
    [InlineData("?role=AccessManager", new string[] { "Auser", "Buser" })]
    [InlineData("?role=SupportOfficer", new string[] { "Cuser", "Duser", "Euser" })]
    [InlineData("?role=Viewer", new string[] { "Fuser", "Guser", "Huser", "Iuser" })]
    [InlineData("?role=AccessManager&role=SupportOfficer", new string[] { "Auser", "Buser", "Cuser", "Duser", "Euser" })]
    [InlineData("?status=active", new string[] { "Auser", "Cuser", "Euser", "Guser", "Iuser" })]
    [InlineData("?status=deactivated", new string[] { "Buser", "Duser", "Fuser", "Huser" })]
    [InlineData("?role=AccessManager&status=active", new string[] { "Auser" })]
    [InlineData("?role=Viewer&status=deactivated", new string[] { "Fuser", "Huser" })]
    [InlineData("?keywords=org1&role=AccessManager&role=SupportOfficer&status=active&status=deactivated", new string[] { "Auser", "Buser", "Cuser" })]
    public async Task Get_ValidRequestAndUsersFound_FiltersUsersByKeywordsRoleAndStatus(string query, string[] expectedUserFirstNames)
    {
        // Arrange
        var users = await TestData.CreateMultipleUsersAsync(
            new() { Name = $"Auser Lastname1", Email = "auser@org1.com", Role = UserRoles.AccessManager, Active = true },
            new() { Name = $"Buser Lastname1", Email = "buser@org1.com", Role = UserRoles.AccessManager, Active = false },
            new() { Name = $"Cuser Lastname1", Email = "cuser@org1.com", Role = UserRoles.SupportOfficer, Active = true },
            new() { Name = $"Duser Lastname1", Email = "duser@org2.com", Role = UserRoles.SupportOfficer, Active = false },
            new() { Name = $"Euser Lastname2", Email = "euser@org2.com", Role = UserRoles.SupportOfficer, Active = true },
            new() { Name = $"Fuser Lastname2", Email = "fuser@org2.com", Role = UserRoles.Viewer, Active = false },
            new() { Name = $"Guser Lastname2", Email = "guser@org3.com", Role = UserRoles.Viewer, Active = true },
            new() { Name = $"Huser Lastname2", Email = "huser@org3.com", Role = UserRoles.Viewer, Active = false },
            new() { Name = $"Iuser Lastname2", Email = "iuser@org3.com", Role = UserRoles.Viewer, Active = true }
        );
        SetCurrentUser(users.First(u => u.Role == UserRoles.AccessManager && u.Active));

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, $"{RequestPath}{query}");
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        var html = await AssertEx.HtmlResponseAsync(response);
        var userElements = html.GetElementsByClassName("trs-user");

        AssertElementsInnerTextContains(userElements, expectedUserFirstNames);
    }

    [Theory]
    [InlineData("?role=AccessManager&page=1", 10, "Auser", "Juser")]
    [InlineData("?role=AccessManager&page=2", 2, "Kuser", "Luser")]
    [InlineData("?role=SupportOfficer&page=1", 10, "Muser", "Vuser")]
    [InlineData("?role=SupportOfficer&page=2", 4, "Wuser", "Zuser")]
    [InlineData("?role=AccessManager&role=SupportOfficer&status=active&page=1", 10, "Auser", "Suser")]
    [InlineData("?role=AccessManager&role=SupportOfficer&status=active&page=2", 3, "Uuser", "Yuser")]
    public async Task Get_ValidRequestAndUsersFound_PaginatesFilteredUsers(string query, int expectedUserCount, string expectedFirstUserOnPage, string expectedLastUserOnPage)
    {
        // Arrange
        var users = await TestData.CreateMultipleUsersAsync(26, i => new()
        {
            Name = $"{(char)('A' + i)}user {Faker.Name.Last()}",
            Role = i < 12 ? UserRoles.AccessManager : UserRoles.SupportOfficer,
            Active = i % 2 == 0
        });
        SetCurrentUser(users.First(u => u.Role == UserRoles.AccessManager && u.Active));

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, $"{RequestPath}{query}");
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        var html = await AssertEx.HtmlResponseAsync(response);
        var userElements = html.GetElementsByClassName("trs-user");

        Assert.Equal(expectedUserCount, userElements.Length);
        Assert.Contains(expectedFirstUserOnPage, userElements[0].InnerHtml);
        Assert.Contains(expectedLastUserOnPage, userElements[expectedUserCount - 1].InnerHtml);
    }

    [Theory]
    [InlineData("?", new string[] {
        "Viewer (4)",
        "Support officer (3)",
        "Alerts manager (TRA decisions) (0)",
        "Alerts manager (TRA and DBS decisions) (0)",
        "Access manager (2)"
    }, new string[] {
        "Active (5)",
        "Deactivated (4)"
    })]
    [InlineData("?status=active", new string[] {
        "Viewer (2)",
        "Support officer (2)",
        "Alerts manager (TRA decisions) (0)",
        "Alerts manager (TRA and DBS decisions) (0)",
        "Access manager (1)"
    }, new string[] {
        "Active (5)",
        "Deactivated (0)"
    })]
    [InlineData("?role=AccessManager&role=SupportOfficer&status=deactivated", new string[] {
        "Viewer (0)",
        "Support officer (1)",
        "Alerts manager (TRA decisions) (0)",
        "Alerts manager (TRA and DBS decisions) (0)",
        "Access manager (1)"
    }, new string[] {
        "Active (0)",
        "Deactivated (2)"
    })]
    public async Task Get_ValidRequestAndUsersFound_ShowsFilterCounts(string query, string[] expectedRoleLabels, string[] expectedStatusLabels)
    {
        // Arrange
        var users = await TestData.CreateMultipleUsersAsync(
            new() { Name = $"Auser {Faker.Name.Last()}", Role = UserRoles.AccessManager, Active = true },
            new() { Name = $"Buser {Faker.Name.Last()}", Role = UserRoles.AccessManager, Active = false },
            new() { Name = $"Cuser {Faker.Name.Last()}", Role = UserRoles.SupportOfficer, Active = true },
            new() { Name = $"Duser {Faker.Name.Last()}", Role = UserRoles.SupportOfficer, Active = false },
            new() { Name = $"Euser {Faker.Name.Last()}", Role = UserRoles.SupportOfficer, Active = true },
            new() { Name = $"Fuser {Faker.Name.Last()}", Role = UserRoles.Viewer, Active = false },
            new() { Name = $"Guser {Faker.Name.Last()}", Role = UserRoles.Viewer, Active = true },
            new() { Name = $"Huser {Faker.Name.Last()}", Role = UserRoles.Viewer, Active = false },
            new() { Name = $"Iuser {Faker.Name.Last()}", Role = UserRoles.Viewer, Active = true }
        );
        SetCurrentUser(users.First(u => u.Role == UserRoles.AccessManager && u.Active));

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, $"{RequestPath}{query}");
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        var html = await AssertEx.HtmlResponseAsync(response);
        var roleLabels = html.QuerySelectorAll(@".moj-filter input[name=""role""] + label");
        var statusLabels = html.QuerySelectorAll(@".moj-filter input[name=""status""] + label");

        AssertElementsInnerTextContains(roleLabels, expectedRoleLabels);
        AssertElementsInnerTextContains(statusLabels, expectedStatusLabels);
    }

    [Theory]
    [InlineData("?", new string[] { "Auser", "Buser", "Cuser" })]
    [InlineData("?role=Administrator", new string[0])]
    public async Task Get_NonAdministratorUser_DoesNotShowAdministratorUsers(string query, string[] expectedUserFirstNames)
    {
        // Arrange
        var users = await TestData.CreateMultipleUsersAsync(6, i => new()
        {
            Name = $"{(char)('A' + i)}user {Faker.Name.Last()}",
            Role = i < 3 ? UserRoles.AccessManager : UserRoles.Administrator
        });
        SetCurrentUser(users.First(u => u.Role == UserRoles.AccessManager && u.Active));

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, $"{RequestPath}{query}");
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        var html = await AssertEx.HtmlResponseAsync(response);
        var userElements = html.GetElementsByClassName("trs-user");

        AssertElementsInnerTextContains(userElements, expectedUserFirstNames);
    }

    [Theory]
    [InlineData("?", new string[] { "Auser", "Buser", "Cuser", "Duser", "Euser", "Fuser" })]
    [InlineData("?role=Administrator", new string[] { "Duser", "Euser", "Fuser" })]
    public async Task Get_AdministratorUser_ShowsAdministratorUsers(string query, string[] expectedUserFirstNames)
    {
        // Arrange
        var users = await TestData.CreateMultipleUsersAsync(6, i => new()
        {
            Name = $"{(char)('A' + i)}user {Faker.Name.Last()}",
            Role = i < 3 ? UserRoles.AccessManager : UserRoles.Administrator
        });
        SetCurrentUser(users.First(u => u.Role == UserRoles.Administrator && u.Active));

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, $"{RequestPath}{query}");
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        var html = await AssertEx.HtmlResponseAsync(response);
        var userElements = html.GetElementsByClassName("trs-user");

        AssertElementsInnerTextContains(userElements, expectedUserFirstNames);
    }

    [Theory]
    [InlineData("?", new string[] {
        "Viewer (0)",
        "Support officer (0)",
        "Alerts manager (TRA decisions) (0)",
        "Alerts manager (TRA and DBS decisions) (0)",
        "Access manager (2)",
        "Administrator (3)"
    }, new string[] {
        "Active (3)",
        "Deactivated (2)"
    })]
    [InlineData("?role=Administrator&status=active", new string[] {
        "Viewer (0)",
        "Support officer (0)",
        "Alerts manager (TRA decisions) (0)",
        "Alerts manager (TRA and DBS decisions) (0)",
        "Access manager (0)",
        "Administrator (2)"
    }, new string[] {
        "Active (2)",
        "Deactivated (0)"
    })]
    public async Task Get_AdministratorUser_ShowsAdministratorFilterCounts(string query, string[] expectedRoleLabels, string[] expectedStatusLabels)
    {
        // Arrange
        var users = await TestData.CreateMultipleUsersAsync(
            new() { Name = $"Auser {Faker.Name.Last()}", Role = UserRoles.AccessManager, Active = true },
            new() { Name = $"Buser {Faker.Name.Last()}", Role = UserRoles.AccessManager, Active = false },
            new() { Name = $"Cuser {Faker.Name.Last()}", Role = UserRoles.Administrator, Active = true },
            new() { Name = $"Duser {Faker.Name.Last()}", Role = UserRoles.Administrator, Active = false },
            new() { Name = $"Euser {Faker.Name.Last()}", Role = UserRoles.Administrator, Active = true }
        );
        SetCurrentUser(users.First(u => u.Role == UserRoles.Administrator && u.Active));

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, $"{RequestPath}{query}");
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        var html = await AssertEx.HtmlResponseAsync(response);
        var roleLabels = html.QuerySelectorAll(@".moj-filter input[name=""role""] + label");
        var statusLabels = html.QuerySelectorAll(@".moj-filter input[name=""status""] + label");

        AssertElementsInnerTextContains(roleLabels, expectedRoleLabels);
        AssertElementsInnerTextContains(statusLabels, expectedStatusLabels);
    }

    private void AssertElementsInnerTextContains(IHtmlCollection<IElement> elements, string[] expectedValues)
    {
        Assert.Collection(elements, [.. expectedValues.Select(name => (Action<IElement>)(e => Assert.Contains(name, e.InnerHtml)))]);
    }
}
