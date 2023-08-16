using Microsoft.EntityFrameworkCore;
using TeachingRecordSystem.Core.Events;
using User = TeachingRecordSystem.Core.DataStore.Postgres.Models.User;

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
        SetCurrentUser(TestUsers.NoRoles);
        var userId = Guid.NewGuid();

        var request = new HttpRequestMessage(HttpMethod.Get, UrlSegmentPath(userId));

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

        var request = new HttpRequestMessage(HttpMethod.Get, UrlSegmentPath(userId));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_ValidRequest_RendersExpectedContent()
    {
        // Arrange
        var user = await CreateUser();

        var request = new HttpRequestMessage(HttpMethod.Get, UrlSegmentPath(user.UserId));

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocument();
        Assert.Equal(user.Name, doc.GetElementById("Name")?.GetAttribute("value"));
        Assert.Equal(user.Email, doc.GetElementById("Email")?.GetAttribute("value"));
    }

    [Fact]
    public async Task Post_UserWithoutAdministratorRole_ReturnsForbidden()
    {
        // Arrange
        SetCurrentUser(TestUsers.NoRoles);

        var user = await CreateUser();

        var request = new HttpRequestMessage(HttpMethod.Post, UrlSegmentPath(user.UserId))
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
        var user = await CreateUser();

        var request = new HttpRequestMessage(HttpMethod.Post, UrlSegmentPath(Guid.NewGuid()))
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
        var user = await CreateUser();
        const string role = UserRoles.Administrator;

        var request = new HttpRequestMessage(HttpMethod.Post, UrlSegmentPath(user.UserId))
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
        var user = await CreateUser();
        var newName = Faker.Name.FullName();

        var request = new HttpRequestMessage(HttpMethod.Post, UrlSegmentPath(user.UserId))
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
        var currentUser = await CreateUser();
        var newName = changeName ? Faker.Name.FullName() : currentUser.Name;
        var roles = changeRoles ? new[] { UserRoles.Administrator, "Super Admin" } : currentUser.Roles;

        var request = new HttpRequestMessage(HttpMethod.Post, UrlSegmentPath(currentUser.UserId))
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
            EventObserver.AssertEventsSaved(e =>
            {
                var userCreatedEvent = Assert.IsType<UserUpdatedEvent>(e);
                Assert.Equal(Clock.UtcNow, userCreatedEvent.CreatedUtc);
                Assert.Equal(userCreatedEvent.UpdatedByUserId, GetCurrentUserId());
                Assert.Equal(UserType.Person, userCreatedEvent.User.UserType);
                Assert.Equal(newName, userCreatedEvent.User.Name);
                Assert.Equal(updatedUser.Email, userCreatedEvent.User.Email);
                Assert.Equal(updatedUser.AzureAdUserId, userCreatedEvent.User.AzureAdUserId);
                Assert.Equal(expectedChanges, userCreatedEvent.Changes);
                Assert.True(userCreatedEvent.User.Roles.SequenceEqual(roles));
            });
        }

        var redirectResponse = await response.FollowRedirect(HttpClient);
        var redirectDoc = await redirectResponse.GetDocument();
        AssertEx.HtmlDocumentHasFlashSuccess(redirectDoc, "User updated");
    }

    private static string UrlSegmentPath(Guid userId) => $"/users/{userId}";

    private async Task<User> CreateUser() =>
        await WithDbContext<User>(async dbContext =>
        {
            var user = TestUsers.CreateUser();
            dbContext.Users.Add(user);

            await dbContext.SaveChangesAsync();

            return user;
        });
}
