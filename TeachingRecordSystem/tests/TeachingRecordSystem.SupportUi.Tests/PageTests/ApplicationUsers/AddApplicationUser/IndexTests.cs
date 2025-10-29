using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.Legacy;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.ApplicationUsers.AddApplicationUser;

public class IndexTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Test]
    public async Task Get_UserDoesNotHavePermission_ReturnsForbidden()
    {
        // Arrange
        SetCurrentUser(await TestData.CreateUserAsync(role: null));

        var request = new HttpRequestMessage(HttpMethod.Get, "/application-users/add");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Test]
    public async Task Get_ValidRequest_ReturnsExpectedContent()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/application-users/add");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseAsync(response);
    }

    [Test]
    public async Task Post_UserDoesNotHavePermission_ReturnsForbidden()
    {
        // Arrange
        SetCurrentUser(await TestData.CreateUserAsync(role: null));

        var name = TestData.GenerateApplicationUserName();

        var request = new HttpRequestMessage(HttpMethod.Post, "/application-users/add")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Name", name }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Test]
    public async Task Post_NameNotSpecified_RendersError()
    {
        // Arrange
        var name = "";

        var request = new HttpRequestMessage(HttpMethod.Post, "/application-users/add")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Name", name }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "Name", "Enter a name");
    }

    [Test]
    public async Task Post_NameTooLong_RendersError()
    {
        // Arrange
        var name = new string('a', UserBase.NameMaxLength + 1);

        var request = new HttpRequestMessage(HttpMethod.Post, "/application-users/add")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Name", name }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "Name", "Name must be 200 characters or less");
    }

    [Test]
    public async Task Post_ValidRequest_CreatesApplicationUserCreatesEventAndRedirectsWithFlashMessage()
    {
        // Arrange
        var name = TestData.GenerateApplicationUserName();
        var shortName = TestData.GenerateApplicationUserShortName();

        var request = new HttpRequestMessage(HttpMethod.Post, "/application-users/add")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Name", name },
                { "ShortName", shortName }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var applicationUser = await WithDbContextAsync(async dbContext =>
        {
            var applicationUser = await dbContext.ApplicationUsers.SingleAsync(a => a.Name == name);
            Assert.NotNull(applicationUser);
            Assert.Equal(shortName, applicationUser.ShortName);

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
                Assert.Equal(shortName, applicationUserCreatedEvent.ApplicationUser.ShortName);
            });

        var redirectResponse = await response.FollowRedirectAsync(HttpClient);
        var redirectDoc = await redirectResponse.GetDocumentAsync();
        AssertEx.HtmlDocumentHasFlashSuccess(redirectDoc, "Application user added");
    }
}
