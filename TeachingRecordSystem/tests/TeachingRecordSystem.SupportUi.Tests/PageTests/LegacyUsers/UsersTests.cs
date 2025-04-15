namespace TeachingRecordSystem.SupportUi.Tests.PageTests.LegacyUsers;

public class UsersTests : TestBase
{
    private const string RequestPath = "/legacy-users";

    public UsersTests(HostFixture hostFixture) : base(hostFixture)
    {
    }

    [Fact]
    public async Task Get_UserWithOutAccessManagerRole_ReturnsForbidden()
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(role: null));

        var request = new HttpRequestMessage(HttpMethod.Get, RequestPath);

        // Act
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

        var request = new HttpRequestMessage(HttpMethod.Get, RequestPath);

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert

        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_ValidRequestAndUsersFound_RendersUsers()
    {
        // Arrange
        var user = await TestData.CreateUserAsync(role: UserRoles.AccessManager);

        var request = new HttpRequestMessage(HttpMethod.Get, RequestPath);

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var element = doc.GetElementByTestId($"user-{user.UserId}")!.InnerHtml;

        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        Assert.NotNull(element);
        Assert.Contains(user.Name, element);
        Assert.Contains(user.Email!, element);
    }
}
