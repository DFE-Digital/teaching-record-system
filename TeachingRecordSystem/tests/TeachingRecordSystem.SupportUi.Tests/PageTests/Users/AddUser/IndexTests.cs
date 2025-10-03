namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Users.AddUser;

[NotInParallel]
public class IndexTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Before(Test)]
    public async Task DeleteUsersAsync()
    {
        await WithDbContext(async dbContext =>
        {
            await dbContext.Notes.ExecuteDeleteAsync();
            await dbContext.SupportTasks.ExecuteDeleteAsync();
            await dbContext.Users.ExecuteDeleteAsync();
        });

        TestUsers.ClearCache();
    }

    [Test]
    public async Task Get_UserWithoutAccessManagerRole_ReturnsForbidden()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(role: UserRoles.RecordManager);
        SetCurrentUser(user);

        var request = new HttpRequestMessage(HttpMethod.Get, "/users/add");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Test]
    public async Task Get_WithAccessManagerUser_ReturnsOk()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(role: UserRoles.AccessManager);
        SetCurrentUser(user);

        var request = new HttpRequestMessage(HttpMethod.Get, "/users/add");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
    }

    [Test]
    public async Task Post_UserWithoutAccessManagerRole_ReturnsForbidden()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(role: UserRoles.RecordManager);
        SetCurrentUser(user);

        var request = new HttpRequestMessage(HttpMethod.Post, "/users/add");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Test]
    public async Task Post_NoEmailEntered_RendersErrorMessage()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(role: UserRoles.AccessManager);
        SetCurrentUser(user);

        var request = new HttpRequestMessage(HttpMethod.Post, "/users/add")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Email", "" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
        await AssertEx.HtmlResponseHasErrorAsync(response, "Email", "Enter an email address");
    }


    [Test]
    public async Task Post_UserNotFound_RendersErrorMessage()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(role: UserRoles.AccessManager);
        SetCurrentUser(user);

        var email = Faker.Internet.Email();

        ConfigureUserServiceMock(email, null);

        var request = new HttpRequestMessage(HttpMethod.Post, "/users/add")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Email", email }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
        await AssertEx.HtmlResponseHasErrorAsync(response, "Email", "User does not exist");
    }

    [Test]
    public async Task Post_EmailDoesNotHaveSuffix_AppendsEducationSuffixBeforeSearching()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(role: UserRoles.AccessManager);
        SetCurrentUser(user);

        var email = "an.email";

        var request = new HttpRequestMessage(HttpMethod.Post, "/users/add")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Email", email }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        AzureActiveDirectoryUserServiceMock.Verify(mock => mock.GetUserByEmailAsync(email + "@education.gov.uk"));
    }

    [Test]
    public async Task Post_UserFound_RedirectsToConfirmPage()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(role: UserRoles.AccessManager);
        SetCurrentUser(user);

        var email = Faker.Internet.Email();
        var name = TestData.GenerateName();
        var userId = Guid.NewGuid().ToString();

        ConfigureUserServiceMock(email, new Services.AzureActiveDirectory.User
        {
            Email = email,
            Name = name,
            UserId = userId
        });

        var request = new HttpRequestMessage(HttpMethod.Post, "/users/add")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Email", email }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/users/add/confirm?userId={userId}", response.Headers.Location?.OriginalString);
    }

    [Test]
    public async Task Post_UserFound_ButAlreadyExistsInTrs_RedirectsToEditPage()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(role: UserRoles.AccessManager);
        SetCurrentUser(user);

        var email = Faker.Internet.Email();
        var name = TestData.GenerateName();
        var userId = Guid.NewGuid();
        var existingUser = await TestData.CreateUserAsync(name: name, email: email, azureAdUserId: userId);

        ConfigureUserServiceMock(email, new Services.AzureActiveDirectory.User
        {
            Email = email,
            Name = name,
            UserId = userId.ToString()
        });

        var request = new HttpRequestMessage(HttpMethod.Post, "/users/add")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Email", email }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/users/{existingUser.UserId}", response.Headers.Location?.OriginalString);
    }


    private void ConfigureUserServiceMock(string email, Services.AzureActiveDirectory.User? user) =>
        AzureActiveDirectoryUserServiceMock
            .Setup(mock => mock.GetUserByEmailAsync(email))
            .ReturnsAsync(user);
}
