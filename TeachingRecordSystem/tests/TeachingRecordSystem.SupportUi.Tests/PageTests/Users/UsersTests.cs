using AngleSharp.Dom;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Users;

public class UsersTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    private const string RequestPath = "/users";

    [Before(Test)]
    public Task DeleteUsersAsync() => WithDbContextAsync(dbContext => dbContext.Users.ExecuteDeleteAsync());

    [Test]
    public async Task Get_UserWithoutAccessManagerRole_ReturnsForbidden()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(role: UserRoles.RecordManager);
        SetCurrentUser(user);

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, RequestPath);
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Test]
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

    [Test]
    public async Task Get_ValidRequestAndUsersFound_RendersUsers()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(role: UserRoles.AccessManager);
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

    [Test]
    [Arguments("?pageNumber=1", 10, "Auser", "Juser")]
    [Arguments("?pageNumber=2", 10, "Kuser", "Tuser")]
    [Arguments("?pageNumber=3", 2, "Uuser", "Vuser")]
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

    [Test]
    [Arguments("?keywords=user%20laSTName1", new string[] { "Auser", "Buser", "Cuser", "Duser" })]
    [Arguments("?keywords=USER%40org2", new string[] { "Duser", "Euser", "Fuser" })]
    [Arguments("?role=AccessManager", new string[] { "Auser", "Buser" })]
    [Arguments("?role=RecordManager", new string[] { "Cuser", "Duser", "Euser" })]
    [Arguments("?role=Viewer", new string[] { "Fuser", "Guser", "Huser", "Iuser" })]
    [Arguments("?role=AccessManager&role=RecordManager", new string[] { "Auser", "Buser", "Cuser", "Duser", "Euser" })]
    [Arguments("?status=active", new string[] { "Auser", "Cuser", "Euser", "Guser", "Iuser" })]
    [Arguments("?status=deactivated", new string[] { "Buser", "Duser", "Fuser", "Huser" })]
    [Arguments("?role=AccessManager&status=active", new string[] { "Auser" })]
    [Arguments("?role=Viewer&status=deactivated", new string[] { "Fuser", "Huser" })]
    [Arguments("?keywords=org1&role=AccessManager&role=RecordManager&status=active&status=deactivated", new string[] { "Auser", "Buser", "Cuser" })]
    public async Task Get_ValidRequestAndUsersFound_FiltersUsersByKeywordsRoleAndStatus(string query, string[] expectedUserFirstNames)
    {
        // Arrange
        var users = await TestData.CreateMultipleUsersAsync(
            new() { Name = $"Auser Lastname1", Email = "auser@org1.com", Role = UserRoles.AccessManager, Active = true },
            new() { Name = $"Buser Lastname1", Email = "buser@org1.com", Role = UserRoles.AccessManager, Active = false },
            new() { Name = $"Cuser Lastname1", Email = "cuser@org1.com", Role = UserRoles.RecordManager, Active = true },
            new() { Name = $"Duser Lastname1", Email = "duser@org2.com", Role = UserRoles.RecordManager, Active = false },
            new() { Name = $"Euser Lastname2", Email = "euser@org2.com", Role = UserRoles.RecordManager, Active = true },
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

    [Test]
    [Arguments("?role=AccessManager&pageNumber=1", 10, "Auser", "Juser")]
    [Arguments("?role=AccessManager&pageNumber=2", 2, "Kuser", "Luser")]
    [Arguments("?role=RecordManager&pageNumber=1", 10, "Muser", "Vuser")]
    [Arguments("?role=RecordManager&pageNumber=2", 4, "Wuser", "Zuser")]
    [Arguments("?role=AccessManager&role=RecordManager&status=active&pageNumber=1", 10, "Auser", "Suser")]
    [Arguments("?role=AccessManager&role=RecordManager&status=active&pageNumber=2", 3, "Uuser", "Yuser")]
    public async Task Get_ValidRequestAndUsersFound_PaginatesFilteredUsers(string query, int expectedUserCount, string expectedFirstUserOnPage, string expectedLastUserOnPage)
    {
        // Arrange
        var users = await TestData.CreateMultipleUsersAsync(26, i => new()
        {
            Name = $"{(char)('A' + i)}user {Faker.Name.Last()}",
            Role = i < 12 ? UserRoles.AccessManager : UserRoles.RecordManager,
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

    [Test]
    [Arguments("?", new string[] {
        "Viewer (4)",
        "Record manager (3)",
        "Alerts manager (TRA decisions) (0)",
        "Alerts manager (TRA and DBS decisions) (0)",
        "Access manager (2)"
    }, new string[] {
        "Active (5)",
        "Deactivated (4)"
    })]
    [Arguments("?status=active", new string[] {
        "Viewer (2)",
        "Record manager (2)",
        "Alerts manager (TRA decisions) (0)",
        "Alerts manager (TRA and DBS decisions) (0)",
        "Access manager (1)"
    }, new string[] {
        "Active (5)",
        "Deactivated (0)"
    })]
    [Arguments("?role=AccessManager&role=RecordManager&status=deactivated", new string[] {
        "Viewer (0)",
        "Record manager (1)",
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
            new() { Name = $"Cuser {Faker.Name.Last()}", Role = UserRoles.RecordManager, Active = true },
            new() { Name = $"Duser {Faker.Name.Last()}", Role = UserRoles.RecordManager, Active = false },
            new() { Name = $"Euser {Faker.Name.Last()}", Role = UserRoles.RecordManager, Active = true },
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
        var roleLabels = html.QuerySelectorAll(@".moj-filter input[name=""Role""] + label");
        var statusLabels = html.QuerySelectorAll(@".moj-filter input[name=""Status""] + label");

        AssertElementsInnerTextContains(roleLabels, expectedRoleLabels);
        AssertElementsInnerTextContains(statusLabels, expectedStatusLabels);
    }

    [Test]
    [Arguments("?", new string[] { "Auser", "Buser", "Cuser" })]
    [Arguments("?role=Administrator", new string[0])]
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

    [Test]
    [Arguments("?", new string[] { "Auser", "Buser", "Cuser", "Duser", "Euser", "Fuser" })]
    [Arguments("?role=Administrator", new string[] { "Duser", "Euser", "Fuser" })]
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

    [Test]
    [Arguments("?", new string[] {
        "Viewer (0)",
        "Record manager (0)",
        "Alerts manager (TRA decisions) (0)",
        "Alerts manager (TRA and DBS decisions) (0)",
        "Access manager (2)",
        "Administrator (3)"
    }, new string[] {
        "Active (3)",
        "Deactivated (2)"
    })]
    [Arguments("?role=Administrator&status=active", new string[] {
        "Viewer (0)",
        "Record manager (0)",
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
        var roleLabels = html.QuerySelectorAll(@".moj-filter input[name=""Role""] + label");
        var statusLabels = html.QuerySelectorAll(@".moj-filter input[name=""Status""] + label");

        AssertElementsInnerTextContains(roleLabels, expectedRoleLabels);
        AssertElementsInnerTextContains(statusLabels, expectedStatusLabels);
    }

    private void AssertElementsInnerTextContains(IHtmlCollection<IElement> elements, string[] expectedValues)
    {
        Assert.Collection(elements, [.. expectedValues.Select(name => (Action<IElement>)(e => Assert.Contains(name, e.InnerHtml)))]);
    }
}
