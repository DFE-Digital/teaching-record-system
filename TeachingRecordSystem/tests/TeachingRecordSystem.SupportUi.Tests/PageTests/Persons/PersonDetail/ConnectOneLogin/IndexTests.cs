using Optional;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.ConnectOneLogin;

public class IndexTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_WithValidPersonId_RendersExpectedContent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/connect-one-login");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Contains(person.Trn, doc.Body!.TextContent);
        Assert.NotNull(doc.GetElementById("EmailAddress"));
    }

    [Fact]
    public async Task Get_WithInvalidPersonId_ReturnsNotFound()
    {
        // Arrange
        var personId = Guid.NewGuid();
        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{personId}/connect-one-login");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_WithEmptyEmailAddress_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/connect-one-login")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["EmailAddress"] = ""
            })
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "EmailAddress", "Enter a GOV.UK One Login email address");
    }

    [Fact]
    public async Task Post_WithInvalidEmailFormat_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/connect-one-login")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["EmailAddress"] = "not-an-email"
            })
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "EmailAddress", "Email address is not valid.");
    }

    [Fact]
    public async Task Post_WithValidEmailButNoMatchingOneLoginUser_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var emailAddress = Faker.Internet.Email();
        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/connect-one-login")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["EmailAddress"] = emailAddress
            })
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "EmailAddress", "No GOV.UK One Login user found with this email address");
    }

    [Fact]
    public async Task Post_WithOneLoginUserAlreadyConnectedToThisRecord_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var emailAddress = Faker.Internet.Email();
        await TestData.CreateOneLoginUserAsync(person, email: Option.Some<string?>(emailAddress));

        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/connect-one-login")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["EmailAddress"] = emailAddress
            })
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "EmailAddress", "This GOV.UK One Login user is already connected to this record");
    }

    [Fact]
    public async Task Post_WithOneLoginUserAlreadyConnectedToAnotherRecord_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var otherPerson = await TestData.CreatePersonAsync();
        var emailAddress = Faker.Internet.Email();
        await TestData.CreateOneLoginUserAsync(otherPerson, email: Option.Some<string?>(emailAddress));

        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/connect-one-login")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["EmailAddress"] = emailAddress
            })
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "EmailAddress", "This GOV.UK One Login user is already connected to another record");
    }

    [Fact]
    public async Task Post_WithValidEmailAndUnmatchedOneLoginUser_RedirectsToMatchPage()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var emailAddress = Faker.Internet.Email();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(email: Option.Some<string?>(emailAddress));

        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/connect-one-login")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["EmailAddress"] = emailAddress
            })
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/persons/{person.PersonId}/connect-one-login/match?subject={oneLoginUser.Subject}", response.Headers.Location?.OriginalString);
    }
}
