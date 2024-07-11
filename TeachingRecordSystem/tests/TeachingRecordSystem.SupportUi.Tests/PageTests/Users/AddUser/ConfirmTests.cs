namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Users.AddUser;

public class ConfirmTests : TestBase
{
    public ConfirmTests(HostFixture hostFixture) : base(hostFixture)
    {
    }

    [Fact]
    public async Task Get_UserWithoutAdministratorRole_ReturnsForbidden()
    {
        // Arrange
        SetCurrentUser(TestUsers.NoRoles);

        var email = Faker.Internet.Email();
        var name = Faker.Name.FullName();
        var userId = Guid.NewGuid().ToString();

        ConfigureUserServiceMock(userId, new Services.AzureActiveDirectory.User()
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
        var userId = Guid.NewGuid().ToString();

        ConfigureUserServiceMock(userId, null);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/users/add/confirm?userId={userId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Theory]
    [InlineData(true, false, false)]
    [InlineData(true, false, true)]
    [InlineData(true, true, false)]
    [InlineData(true, true, true)]
    [InlineData(false, false, false)]
    public async Task Get_ValidRequest_RendersExpectedContent(
        bool hasCrmAccount,
        bool hasDisabledCrmAccount,
        bool hasDqtRoles)
    {
        // Arrange
        var email = Faker.Internet.Email();
        var name = Faker.Name.FullName();
        var azureAdUserId = Guid.NewGuid();
        string[]? dqtRoles = hasDqtRoles ? [.. Faker.Lorem.Words(2)] : null;

        if (hasCrmAccount)
        {
            await TestData.CreateCrmUser(azureAdUserId: azureAdUserId, hasDisabledCrmAccount: hasDisabledCrmAccount, dqtRoles: dqtRoles);
        }

        ConfigureUserServiceMock(azureAdUserId.ToString(), new Services.AzureActiveDirectory.User()
        {
            Email = email,
            Name = name,
            UserId = azureAdUserId.ToString()
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/users/add/confirm?userId={azureAdUserId}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);

        var noCrmAccountWarning = doc.GetElementByTestId("no-crm-account-warning");
        if (hasCrmAccount)
        {
            Assert.Null(noCrmAccountWarning);
        }
        else
        {
            Assert.NotNull(noCrmAccountWarning);
        }

        var disabledCrmAccountWarning = doc.GetElementByTestId("disabled-crm-account-warning");
        if (hasCrmAccount && hasDisabledCrmAccount)
        {
            Assert.NotNull(disabledCrmAccountWarning);
        }
        else
        {
            Assert.Null(disabledCrmAccountWarning);
        }

        var noDqtRolesWarning = doc.GetElementByTestId("no-dqt-roles-warning");
        if (hasCrmAccount && !hasDisabledCrmAccount && !hasDqtRoles)
        {
            Assert.NotNull(noDqtRolesWarning);
        }
        else
        {
            Assert.Null(noDqtRolesWarning);
        }

        var rolesList = doc.GetElementsByName("Roles");
        Assert.NotNull(rolesList);
        Assert.Equal(UserRoles.All.Count(), rolesList!.Count());

        var dqtRolesList = doc.GetElementByTestId("dqt-roles-list");
        if (hasDqtRoles)
        {
            Assert.NotNull(dqtRolesList);
            Assert.Equal(dqtRoles!.Length, dqtRolesList!.GetElementsByTagName("li").Count());
        }
        else
        {
            Assert.Null(dqtRolesList);
        }

        Assert.Equal(name, doc.GetElementById("Name")?.GetAttribute("value"));
        Assert.Equal(email, doc.GetElementById("Email")?.GetAttribute("value"));
    }

    [Fact]
    public async Task Post_UserWithoutAdministratorRole_ReturnsForbidden()
    {
        // Arrange
        SetCurrentUser(TestUsers.NoRoles);

        var email = Faker.Internet.Email();
        var name = Faker.Name.FullName();
        var userId = Guid.NewGuid().ToString();
        var newName = Faker.Name.FullName();
        var role = UserRoles.Administrator;

        ConfigureUserServiceMock(userId, new Services.AzureActiveDirectory.User()
        {
            Email = email,
            Name = name,
            UserId = userId
        });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/users/add/confirm?userId={userId}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "Name", newName },
                { "Roles", role }
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
        var userId = Guid.NewGuid().ToString();
        var newName = Faker.Name.FullName();
        var role = UserRoles.Administrator;

        ConfigureUserServiceMock(userId, null);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/users/add/confirm?userId={userId}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "Name", newName },
                { "Roles", role }
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
        var email = Faker.Internet.Email();
        var name = Faker.Name.FullName();
        var userId = Guid.NewGuid().ToString();
        var role = UserRoles.Administrator;

        ConfigureUserServiceMock(userId, new Services.AzureActiveDirectory.User()
        {
            Email = email,
            Name = name,
            UserId = userId
        });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/users/add/confirm?userId={userId}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "Roles", role },
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "Name", "Enter a name");
    }

    [Fact]
    public async Task Post_NoRolesSelected_RendersError()
    {
        // Arrange
        var email = Faker.Internet.Email();
        var name = Faker.Name.FullName();
        var userId = Guid.NewGuid().ToString();
        var newName = Faker.Name.FullName();

        ConfigureUserServiceMock(userId, new Services.AzureActiveDirectory.User()
        {
            Email = email,
            Name = name,
            UserId = userId
        });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/users/add/confirm?userId={userId}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "Name", newName },
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "Roles", "Select at least one role");
    }

    [Fact]
    public async Task Post_ValidRequest_CreatesUserEmitsEventAndRedirectsWithFlashMessage()
    {
        // Arrange
        var email = Faker.Internet.Email();
        var name = Faker.Name.FullName();
        var userId = Guid.NewGuid().ToString();
        var newName = Faker.Name.FullName();
        var role = UserRoles.Administrator;

        ConfigureUserServiceMock(userId, new Services.AzureActiveDirectory.User()
        {
            Email = email,
            Name = name,
            UserId = userId
        });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/users/add/confirm?userId={userId}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "Name", newName },
                { "Roles", role }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var user = await WithDbContext(dbContext => dbContext.Users.SingleOrDefaultAsync(u => u.AzureAdUserId == userId));
        Assert.NotNull(user);

        Assert.Equal(UserType.Person, user.UserType);
        Assert.Equal(newName, user.Name);
        Assert.Equal(email, user.Email);
        Assert.Equal(userId, user.AzureAdUserId);
        Assert.Collection(user.Roles, r => Assert.Equal(role, r));

        EventPublisher.AssertEventsSaved(e =>
        {
            var userCreatedEvent = Assert.IsType<UserAddedEvent>(e);
            Assert.Equal(Clock.UtcNow, userCreatedEvent.CreatedUtc);
            Assert.Equal(userCreatedEvent.RaisedBy.UserId, GetCurrentUserId());
            Assert.Equal(newName, userCreatedEvent.User.Name);
            Assert.Equal(email, userCreatedEvent.User.Email);
            Assert.Equal(userId, userCreatedEvent.User.AzureAdUserId);
            Assert.Collection(userCreatedEvent.User.Roles, r => Assert.Equal(role, r));
        });

        var redirectResponse = await response.FollowRedirect(HttpClient);
        var redirectDoc = await redirectResponse.GetDocument();
        AssertEx.HtmlDocumentHasFlashSuccess(redirectDoc, "User added");
    }

    private void ConfigureUserServiceMock(string userId, Services.AzureActiveDirectory.User? user) =>
        AzureActiveDirectoryUserServiceMock
            .Setup(mock => mock.GetUserById(userId))
            .ReturnsAsync(user);
}
