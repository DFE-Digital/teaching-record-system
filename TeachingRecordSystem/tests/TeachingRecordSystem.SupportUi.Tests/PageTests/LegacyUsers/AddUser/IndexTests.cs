namespace TeachingRecordSystem.SupportUi.Tests.PageTests.LegacyUsers.AddUser;

public class IndexTests : TestBase
{
    public IndexTests(HostFixture hostFixture) : base(hostFixture)
    {
    }

    [Fact]
    public async Task Get_UserWithoutAdministratorRole_ReturnsForbidden()
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(role: null));
        var request = new HttpRequestMessage(HttpMethod.Get, "/legacy-users/add");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_WithAdministratorUser_ReturnsOk()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/legacy-users/add");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_UserWithoutAdministratorRole_ReturnsForbidden()
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(role: null));
        var request = new HttpRequestMessage(HttpMethod.Post, "/legacy-users/add");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_NoEmailEntered_RendersErrorMessage()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Post, "/legacy-users/add")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "Email", "" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "Email", "Enter an email address");
    }


    [Fact]
    public async Task Post_UserNotFound_RendersErrorMessage()
    {
        // Arrange
        var email = Faker.Internet.Email();

        ConfigureUserServiceMock(email, null);

        var request = new HttpRequestMessage(HttpMethod.Post, "/legacy-users/add")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "Email", email }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "Email", "User does not exist");
    }

    [Fact]
    public async Task Post_EmailDoesNotHaveSuffix_AppendsEducationSuffixBeforeSearching()
    {
        // Arrange
        var email = "an.email";

        var request = new HttpRequestMessage(HttpMethod.Post, "/legacy-users/add")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "Email", email }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        AzureActiveDirectoryUserServiceMock.Verify(mock => mock.GetUserByEmailAsync(email + "@education.gov.uk"));
    }

    [Fact]
    public async Task Post_UserFound_RedirectsToConfirmPage()
    {
        // Arrange
        var email = Faker.Internet.Email();
        var name = TestData.GenerateName();
        var userId = Guid.NewGuid().ToString();

        ConfigureUserServiceMock(email, new Services.AzureActiveDirectory.User()
        {
            Email = email,
            Name = name,
            UserId = userId
        });

        var request = new HttpRequestMessage(HttpMethod.Post, "/legacy-users/add")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "Email", email }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/legacy-users/add/confirm?userId={userId}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_UserFound_ButAlreadyExistsInTrs_RedirectsToEditPage()
    {
        // Arrange
        var email = Faker.Internet.Email();
        var name = TestData.GenerateName();
        var userId = Guid.NewGuid();
        var user = await TestData.CreateUserAsync(name: name, email: email, azureAdUserId: userId);

        ConfigureUserServiceMock(email, new Services.AzureActiveDirectory.User()
        {
            Email = email,
            Name = name,
            UserId = userId.ToString()
        });

        var request = new HttpRequestMessage(HttpMethod.Post, "/legacy-users/add")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "Email", email }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/legacy-users/{user.UserId}", response.Headers.Location?.OriginalString);
    }


    private void ConfigureUserServiceMock(string email, Services.AzureActiveDirectory.User? user) =>
        AzureActiveDirectoryUserServiceMock
            .Setup(mock => mock.GetUserByEmailAsync(email))
            .ReturnsAsync(user);
}
