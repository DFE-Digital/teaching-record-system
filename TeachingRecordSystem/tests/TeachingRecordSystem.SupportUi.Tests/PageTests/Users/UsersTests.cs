namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Users;

[Collection(nameof(DisableParallelization))]
public class UsersTests : TestBase, IAsyncLifetime
{
    private const string RequestPath = "/users";

    public UsersTests(HostFixture hostFixture) : base(hostFixture)
    {
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        await WithDbContext(dbContext => dbContext.Users.ExecuteDeleteAsync());
        TestUsers.ClearCache();
    }

    [Fact]
    public async Task Get_UserWithoutAccessManagerRole_ReturnsForbidden()
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(role: null));

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, RequestPath);
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

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, RequestPath);
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_ValidRequestAndUsersFound_RendersUsers()
    {
        // Arrange
        var user = await TestData.CreateUserAsync();

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

    [Fact]
    public async Task Get_ValidRequestAndUsersFound_PaginatesUsersAndSortsByFirstName()
    {
        await TestData.CreateMultipleUsersAsync(21, i => new() { Name = $"FirstName{(char)('A' + i)} {Faker.Name.Last()}" });

        // Act
        var page1Request = new HttpRequestMessage(HttpMethod.Get, $"{RequestPath}?page=1");
        var page1Response = await HttpClient.SendAsync(page1Request);

        var page2Request = new HttpRequestMessage(HttpMethod.Get, $"{RequestPath}?page=2");
        var page2Response = await HttpClient.SendAsync(page2Request);

        var page3Request = new HttpRequestMessage(HttpMethod.Get, $"{RequestPath}?page=3");
        var page3Response = await HttpClient.SendAsync(page3Request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)page1Response.StatusCode);
        Assert.Equal(StatusCodes.Status200OK, (int)page2Response.StatusCode);
        Assert.Equal(StatusCodes.Status200OK, (int)page3Response.StatusCode);

        var page1 = await AssertEx.HtmlResponseAsync(page1Response);
        var page1Users = page1.GetElementsByClassName("trs-user");

        var page2 = await AssertEx.HtmlResponseAsync(page2Response);
        var page2Users = page2.GetElementsByClassName("trs-user");

        var page3 = await AssertEx.HtmlResponseAsync(page3Response);
        var page3Users = page3.GetElementsByClassName("trs-user");

        Assert.Equal(10, page1Users.Length);
        Assert.Contains("FirstNameA", page1Users[0].InnerHtml);
        Assert.Contains("FirstNameJ", page1Users[9].InnerHtml);

        Assert.Equal(10, page2Users.Length);
        Assert.Contains("FirstNameK", page2Users[0].InnerHtml);
        Assert.Contains("FirstNameT", page2Users[9].InnerHtml);

        // Admin user also appears on last page
        Assert.Equal(2, page3Users.Length);
        Assert.Contains("FirstNameU", page3Users[0].InnerHtml);
        Assert.Contains("Test User 1", page3Users[1].InnerHtml);
    }
}
