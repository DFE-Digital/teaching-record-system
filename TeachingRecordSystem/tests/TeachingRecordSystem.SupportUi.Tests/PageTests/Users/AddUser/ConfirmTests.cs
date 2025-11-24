using TeachingRecordSystem.Core.Events.Legacy;
using TeachingRecordSystem.SupportUi.Services.AzureActiveDirectory;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Users.AddUser;

public class ConfirmTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_UserWithoutAccessManagerRole_ReturnsForbidden()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(role: UserRoles.RecordManager);
        SetCurrentUser(user);

        var email = Faker.Internet.Email();
        var name = TestData.GenerateName();
        var userId = Guid.NewGuid().ToString();

        ConfigureUserServiceMock(userId, new User
        {
            Email = email,
            Name = name,
            UserId = userId
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/users/add/confirm?userId={userId}");

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

        var userId = Guid.NewGuid().ToString();

        ConfigureUserServiceMock(userId, null);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/users/add/confirm?userId={userId}");

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

        var email = Faker.Internet.Email();
        var name = TestData.GenerateName();
        var userId = Guid.NewGuid().ToString();

        ConfigureUserServiceMock(userId, new User
        {
            Email = email,
            Name = name,
            UserId = userId
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/users/add/confirm?userId={userId}");

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

        var email = Faker.Internet.Email();
        var name = TestData.GenerateName();
        var userId = Guid.NewGuid().ToString();

        ConfigureUserServiceMock(userId, new User
        {
            Email = email,
            Name = name,
            UserId = userId
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/users/add/confirm?userId={userId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        var html = await AssertEx.HtmlResponseAsync(response);
        var roleNames = html.QuerySelectorAll(".trs-user-roles input[type=radio]").Select(e => e.Attributes["value"]!.Value);

        Assert.Contains(UserRoles.Administrator, roleNames);
    }

    [Fact]
    public async Task Post_UserWithoutAccessManagerRole_ReturnsForbidden()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(role: UserRoles.RecordManager);
        SetCurrentUser(user);

        var email = Faker.Internet.Email();
        var name = TestData.GenerateName();
        var userId = Guid.NewGuid().ToString();
        var newName = TestData.GenerateName();
        var role = UserRoles.RecordManager;

        ConfigureUserServiceMock(userId, new User
        {
            Email = email,
            Name = name,
            UserId = userId
        });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/users/add/confirm?userId={userId}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Name", newName },
                { "Role", role }
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

        var userId = Guid.NewGuid().ToString();
        var newName = TestData.GenerateName();
        var role = UserRoles.RecordManager;

        ConfigureUserServiceMock(userId, null);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/users/add/confirm?userId={userId}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Name", newName },
                { "Role", role }
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

        var email = Faker.Internet.Email();
        var name = TestData.GenerateName();
        var userId = Guid.NewGuid().ToString();
        var role = UserRoles.RecordManager;

        ConfigureUserServiceMock(userId, new User
        {
            Email = email,
            Name = name,
            UserId = userId
        });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/users/add/confirm?userId={userId}")
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

        var email = Faker.Internet.Email();
        var name = TestData.GenerateName();
        var userId = Guid.NewGuid().ToString();

        ConfigureUserServiceMock(userId, new User
        {
            Email = email,
            Name = name,
            UserId = userId
        });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/users/add/confirm?userId={userId}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Name", name }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
        await AssertEx.HtmlResponseHasErrorAsync(response, "Role", "Select a role");
    }

    [Fact]
    public async Task Post_UserWithoutAdministratorRole_AddingUserWithNonExistentRole_ReturnsBadRequest()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(role: UserRoles.AccessManager);
        SetCurrentUser(user);

        var email = Faker.Internet.Email();
        var name = TestData.GenerateName();
        var userId = Guid.NewGuid().ToString();

        ConfigureUserServiceMock(userId, new User
        {
            Email = email,
            Name = name,
            UserId = userId
        });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/users/add/confirm?userId={userId}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Name", name },
                { "Role", "XXXXXX" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_UserWithoutAdministratorRole_AddingUserWithAdministratorRole_ReturnsBadRequest()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(role: UserRoles.AccessManager);
        SetCurrentUser(user);

        var email = Faker.Internet.Email();
        var name = TestData.GenerateName();
        var userId = Guid.NewGuid().ToString();

        ConfigureUserServiceMock(userId, new User
        {
            Email = email,
            Name = name,
            UserId = userId
        });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/users/add/confirm?userId={userId}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Name", name },
                { "Role", UserRoles.Administrator }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_UserWithAdministratorRole_AddingUserWithAdministratorRole_ReturnsFound()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(role: UserRoles.Administrator);
        SetCurrentUser(user);

        var email = Faker.Internet.Email();
        var name = TestData.GenerateName();
        var userId = Guid.NewGuid().ToString();

        ConfigureUserServiceMock(userId, new User
        {
            Email = email,
            Name = name,
            UserId = userId
        });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/users/add/confirm?userId={userId}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Name", name },
                { "Role", UserRoles.Administrator }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_ValidRequest_CreatesUserEmitsEventAndRedirectsWithFlashMessage()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(role: UserRoles.AccessManager);
        SetCurrentUser(user);

        var email = Faker.Internet.Email();
        var name = TestData.GenerateName();
        var userId = Guid.NewGuid().ToString();
        var newName = TestData.GenerateName();
        var role = UserRoles.RecordManager;

        ConfigureUserServiceMock(userId, new User
        {
            Email = email,
            Name = name,
            UserId = userId
        });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/users/add/confirm?userId={userId}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Name", newName },
                { "Role", role }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var newUser = await WithDbContextAsync(dbContext => dbContext.Users.SingleOrDefaultAsync(u => u.AzureAdUserId == userId));
        Assert.NotNull(newUser);

        Assert.Equal(UserType.Person, newUser.UserType);
        Assert.Equal(newName, newUser.Name);
        Assert.Equal(email, newUser.Email);
        Assert.Equal(userId, newUser.AzureAdUserId);
        Assert.Equal(role, newUser.Role);

        EventObserver.AssertEventsSaved(e =>
        {
            var userCreatedEvent = Assert.IsType<UserAddedEvent>(e);
            Assert.Equal(Clock.UtcNow, userCreatedEvent.CreatedUtc);
            Assert.Equal(userCreatedEvent.RaisedBy.UserId, GetCurrentUserId());
            Assert.Equal(newName, userCreatedEvent.User.Name);
            Assert.Equal(email, userCreatedEvent.User.Email);
            Assert.Equal(userId, userCreatedEvent.User.AzureAdUserId);
            Assert.Equal(role, userCreatedEvent.User.Role);
        });

        var redirectResponse = await response.FollowRedirectAsync(HttpClient);
        var redirectDoc = await redirectResponse.GetDocumentAsync();
        AssertEx.HtmlDocumentHasFlashSuccess(redirectDoc, $"{newName} has been added as a record manager.");
    }

    private void ConfigureUserServiceMock(string userId, User? user) =>
        AzureActiveDirectoryUserServiceMock
            .Setup(mock => mock.GetUserByIdAsync(userId))
            .ReturnsAsync(user);
}
