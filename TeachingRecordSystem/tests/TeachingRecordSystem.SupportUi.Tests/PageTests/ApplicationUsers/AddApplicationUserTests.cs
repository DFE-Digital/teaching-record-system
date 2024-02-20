using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.ApplicationUsers;

public class AddApplicationUserTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_UserDoesNotHavePermission_ReturnsForbidden()
    {
        // Arrange
        SetCurrentUser(TestUsers.NoRoles);

        var request = new HttpRequestMessage(HttpMethod.Get, "/application-users/add");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_ValidRequest_ReturnsExpectedContent()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/application-users/add");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponse(response);
    }

    [Fact]
    public async Task Post_UserDoesNotHavePermission_ReturnsForbidden()
    {
        // Arrange
        SetCurrentUser(TestUsers.NoRoles);

        var name = TestData.GenerateApplicationUserName();

        var request = new HttpRequestMessage(HttpMethod.Post, "/application-users/add")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "Name", name }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_NameNotSpecified_RendersError()
    {
        // Arrange
        var name = "";

        var request = new HttpRequestMessage(HttpMethod.Post, "/application-users/add")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "Name", name }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "Name", "Enter a name");
    }

    [Fact]
    public async Task Post_NameTooLong_RendersError()
    {
        // Arrange
        var name = new string('a', UserBase.NameMaxLength + 1);

        var request = new HttpRequestMessage(HttpMethod.Post, "/application-users/add")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "Name", name }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "Name", "Name must be 200 characters or less");
    }

    [Fact]
    public async Task Post_ValidRequest_CreatesApplicationUserCreatesEventAndRedirectsWithFlashMessage()
    {
        // Arrange
        var name = TestData.GenerateApplicationUserName();

        var request = new HttpRequestMessage(HttpMethod.Post, "/application-users/add")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "Name", name }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var applicationUser = await WithDbContext(async dbContext =>
        {
            var applicationUser = await dbContext.ApplicationUsers.SingleAsync(a => a.Name == name);
            Assert.NotNull(applicationUser);

            return applicationUser;
        });

        Assert.Equal($"/application-users/{applicationUser.UserId}", response.Headers.Location?.OriginalString);

        EventObserver.AssertEventsSaved(
            e =>
            {
                var applicationUserCreatedEvent = Assert.IsType<ApplicationUserCreatedEvent>(e);
                Assert.Equal(Clock.UtcNow, applicationUserCreatedEvent.CreatedUtc);
                Assert.Equal(GetCurrentUserId(), applicationUserCreatedEvent.RaisedBy.UserId);
                Assert.Equal(name, applicationUserCreatedEvent.ApplicationUser.Name);
            });

        var redirectResponse = await response.FollowRedirect(HttpClient);
        var redirectDoc = await redirectResponse.GetDocument();
        AssertEx.HtmlDocumentHasFlashSuccess(redirectDoc, "Application user added");
    }
}
