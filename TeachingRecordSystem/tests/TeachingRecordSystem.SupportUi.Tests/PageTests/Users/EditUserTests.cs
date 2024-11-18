namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Users;

public class EditUserTests : TestBase
{
    public EditUserTests(HostFixture hostFixture) : base(hostFixture)
    {
    }

    [Fact]
    public async Task Get_UserWithOutAdministratorRole_ReturnsForbidden()
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(roles: []));
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
        var userId = Guid.NewGuid();

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(userId));

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
        string[]? dqtRoles = hasDqtRoles ? [.. Faker.Lorem.Words(2)] : null;
        Guid? azureAdUserId = hasCrmAccount ? Guid.NewGuid() : null;
        var user = await TestData.CreateUserAsync(azureAdUserId: azureAdUserId);

        if (hasCrmAccount)
        {
            await TestData.CreateCrmUserAsync(azureAdUserId: azureAdUserId!.Value, hasDisabledCrmAccount: hasDisabledCrmAccount, dqtRoles: dqtRoles);
        }

        var request = new HttpRequestMessage(HttpMethod.Get, GetRequestPath(user.UserId));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

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

        Assert.Equal(user.Name, doc.GetElementById("Name")?.GetAttribute("value"));
        Assert.Equal(user.Email, doc.GetElementById("Email")?.GetAttribute("value"));
    }

    [Fact]
    public async Task Post_UserWithoutAdministratorRole_ReturnsForbidden()
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(roles: []));

        var user = await TestData.CreateUserAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(user.UserId))
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "Name", user.Name },
                { "Roles", user.Roles }
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
        var user = await TestData.CreateUserAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(Guid.NewGuid()))
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "Name", user.Name },
                { "Roles", user.Roles }
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
        var user = await TestData.CreateUserAsync();
        const string role = UserRoles.Administrator;

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(user.UserId))
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "Roles", role },
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "Name", "Enter a name");
    }

    [Theory]
    [InlineData(true, false, true, UserUpdatedEventChanges.Name)]
    [InlineData(false, true, true, UserUpdatedEventChanges.Roles)]
    [InlineData(true, true, true, UserUpdatedEventChanges.Name | UserUpdatedEventChanges.Roles)]
    public async Task Post_ValidRequest_CreatesUserEmitsEventAndRedirectsWithFlashMessage(
        bool changeName,
        bool changeRoles,
        bool expectedEvent,
        UserUpdatedEventChanges expectedChanges)
    {
        // Arrange
        var currentUser = await TestData.CreateUserAsync(roles: new[] { UserRoles.Helpdesk });
        var newName = changeName ? TestData.GenerateChangedName(currentUser.Name) : currentUser.Name;
        var roles = changeRoles ? new[] { UserRoles.Administrator } : currentUser.Roles;

        var request = new HttpRequestMessage(HttpMethod.Post, GetRequestPath(currentUser.UserId))
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "Name", newName },
                { "Roles", roles }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var updatedUser = await WithDbContext(dbContext =>
            dbContext.Users.SingleOrDefaultAsync(u => u.UserId == currentUser.UserId));
        Assert.NotNull(updatedUser);

        Assert.Equal(UserType.Person, updatedUser.UserType);
        Assert.Equal(newName, updatedUser.Name);
        Assert.Equal(currentUser.Email, updatedUser.Email);
        Assert.Equal(currentUser.AzureAdUserId, updatedUser.AzureAdUserId);
        Assert.True(updatedUser.Roles.SequenceEqual(roles));

        if (expectedEvent)
        {
            EventPublisher.AssertEventsSaved(e =>
            {
                var userCreatedEvent = Assert.IsType<UserUpdatedEvent>(e);
                Assert.Equal(Clock.UtcNow, userCreatedEvent.CreatedUtc);
                Assert.Equal(userCreatedEvent.RaisedBy.UserId, GetCurrentUserId());
                Assert.Equal(newName, userCreatedEvent.User.Name);
                Assert.Equal(updatedUser.Email, userCreatedEvent.User.Email);
                Assert.Equal(updatedUser.AzureAdUserId, userCreatedEvent.User.AzureAdUserId);
                Assert.Equal(expectedChanges, userCreatedEvent.Changes);
                Assert.True(userCreatedEvent.User.Roles.SequenceEqual(roles));
            });
        }

        var redirectResponse = await response.FollowRedirectAsync(HttpClient);
        var redirectDoc = await redirectResponse.GetDocumentAsync();
        AssertEx.HtmlDocumentHasFlashSuccess(redirectDoc, "User updated");
    }

    [Fact]
    public async Task Post_UserExistsButIsAlreadyDeactivated_ReturnsBadRequest()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(active: false);

        var request = new HttpRequestMessage(HttpMethod.Post, $"{GetRequestPath(user.UserId)}/deactivate");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_ValidRequest_DeactivatesUsersEmitsEventAndRedirectsWithFlashMessage()
    {
        // Arrange
        var currentUser = await TestData.CreateUserAsync();
        var request = new HttpRequestMessage(HttpMethod.Post, $"{GetRequestPath(currentUser.UserId)}/deactivate");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var updatedUser = await WithDbContext(dbContext =>
            dbContext.Users.SingleOrDefaultAsync(u => u.UserId == currentUser.UserId));
        Assert.NotNull(updatedUser);

        Assert.False(updatedUser.Active);

        EventPublisher.AssertEventsSaved(e =>
        {
            var userCreatedEvent = Assert.IsType<UserDeactivatedEvent>(e);
            Assert.Equal(Clock.UtcNow, userCreatedEvent.CreatedUtc);
            Assert.Equal(userCreatedEvent.RaisedBy.UserId, GetCurrentUserId());
        });

        var redirectResponse = await response.FollowRedirectAsync(HttpClient);
        var redirectDoc = await redirectResponse.GetDocumentAsync();
        AssertEx.HtmlDocumentHasFlashSuccess(redirectDoc, "User deactivated");
    }

    [Fact]
    public async Task Post_UserExistsButIsAlreadyActive_ReturnsBadRequest()
    {
        // Arrange
        var user = await TestData.CreateUserAsync();

        var request = new HttpRequestMessage(HttpMethod.Post, $"{GetRequestPath(user.UserId)}/activate");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_ValidRequest_ActivatesUsersEmitsEventAndRedirectsWithFlashMessage()
    {
        // Arrange
        var currentUser = await TestData.CreateUserAsync(active: false);
        var request = new HttpRequestMessage(HttpMethod.Post, $"{GetRequestPath(currentUser.UserId)}/activate");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var updatedUser = await WithDbContext(dbContext =>
            dbContext.Users.SingleOrDefaultAsync(u => u.UserId == currentUser.UserId));
        Assert.NotNull(updatedUser);

        Assert.True(updatedUser.Active);

        EventPublisher.AssertEventsSaved(e =>
        {
            var userCreatedEvent = Assert.IsType<UserActivatedEvent>(e);
            Assert.Equal(Clock.UtcNow, userCreatedEvent.CreatedUtc);
            Assert.Equal(userCreatedEvent.RaisedBy.UserId, GetCurrentUserId());
        });

        var redirectResponse = await response.FollowRedirectAsync(HttpClient);
        var redirectDoc = await redirectResponse.GetDocumentAsync();
        AssertEx.HtmlDocumentHasFlashSuccess(redirectDoc, "User reactivated");
    }

    private static string GetRequestPath(Guid userId) => $"/users/{userId}";
}
