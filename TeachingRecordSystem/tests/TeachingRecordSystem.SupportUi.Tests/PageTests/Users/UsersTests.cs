namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Users;

public class UsersTests : TestBase
{
    private const string RequestPath = "/users";

    public UsersTests(HostFixture hostFixture) : base(hostFixture)
    {
    }

    [Fact]
    public async Task Get_UserWithOutAdministratorRole_ReturnsForbidden()
    {
        // Arrange
        SetCurrentUser(TestUsers.NoRoles);

        var request = new HttpRequestMessage(HttpMethod.Get, RequestPath);

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert

        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_UserWithAdministratorRole_ReturnsOk()
    {
        // Arrange
        var user = await TestData.CreateUser(roles: new[] { UserRoles.Administrator });
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
        var user = await TestData.CreateUser(roles: new[] { UserRoles.Administrator });
        SetCurrentUser(TestUsers.Administrator);

        var request = new HttpRequestMessage(HttpMethod.Get, RequestPath);

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocument();
        var element = doc.GetElementByTestId($"user-{user.UserId}")!.InnerHtml;

        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        Assert.NotNull(element);
        Assert.Contains(user.Name, element);
        Assert.Contains(user.Email!, element);
    }
}
