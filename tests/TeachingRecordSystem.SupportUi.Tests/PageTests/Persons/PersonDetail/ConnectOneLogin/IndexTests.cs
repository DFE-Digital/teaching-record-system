using Optional;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.ConnectOneLogin;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.ConnectOneLogin;

public class IndexTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_WithValidPersonId_RedirectsWithJourneyInstanceId()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/connect-one-login");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/persons/{person.PersonId}/connect-one-login?{Constants.UniqueKeyQueryParameterName}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_WithJourneyInstanceId_RendersExpectedContent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstance(
            JourneyNames.ConnectOneLogin,
            new ConnectOneLoginState(),
            new KeyValuePair<string, object>("personId", person.PersonId));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/connect-one-login?{journeyInstance.GetUniqueIdQueryParameter()}");

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

        var journeyInstance = await CreateJourneyInstance(
            JourneyNames.ConnectOneLogin,
            new ConnectOneLoginState(),
            new KeyValuePair<string, object>("personId", person.PersonId));

        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/connect-one-login?{journeyInstance.GetUniqueIdQueryParameter()}")
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

        var journeyInstance = await CreateJourneyInstance(
            JourneyNames.ConnectOneLogin,
            new ConnectOneLoginState(),
            new KeyValuePair<string, object>("personId", person.PersonId));

        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/connect-one-login?{journeyInstance.GetUniqueIdQueryParameter()}")
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

        var journeyInstance = await CreateJourneyInstance(
            JourneyNames.ConnectOneLogin,
            new ConnectOneLoginState(),
            new KeyValuePair<string, object>("personId", person.PersonId));

        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/connect-one-login?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["EmailAddress"] = emailAddress
            })
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "EmailAddress", "The email address you entered is not linked to a GOV.UK One Login record");
    }

    [Fact]
    public async Task Post_WithOneLoginUserAlreadyConnectedToThisRecord_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var emailAddress = Faker.Internet.Email();
        await TestData.CreateOneLoginUserAsync(person, email: Option.Some<string?>(emailAddress));

        var journeyInstance = await CreateJourneyInstance(
            JourneyNames.ConnectOneLogin,
            new ConnectOneLoginState(),
            new KeyValuePair<string, object>("personId", person.PersonId));

        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/connect-one-login?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["EmailAddress"] = emailAddress
            })
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "EmailAddress", "The email address you entered is already connected to this record");
    }

    [Fact]
    public async Task Post_WithOneLoginUserAlreadyConnectedToAnotherRecord_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var otherPerson = await TestData.CreatePersonAsync();
        var emailAddress = Faker.Internet.Email();
        await TestData.CreateOneLoginUserAsync(otherPerson, email: Option.Some<string?>(emailAddress));

        var journeyInstance = await CreateJourneyInstance(
            JourneyNames.ConnectOneLogin,
            new ConnectOneLoginState(),
            new KeyValuePair<string, object>("personId", person.PersonId));

        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/connect-one-login?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["EmailAddress"] = emailAddress
            })
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "EmailAddress", "The email address you entered is already connected to another record");
    }

    [Fact]
    public async Task Post_WithValidEmailAndUnmatchedOneLoginUser_CreatesJourneyAndRedirectsToMatchPage()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var emailAddress = Faker.Internet.Email();
        _ = await TestData.CreateOneLoginUserAsync(
            personId: null,
            email: Option.Some<string?>(emailAddress),
            verifiedInfo: ([person.FirstName, person.LastName], person.DateOfBirth));

        var journeyInstance = await CreateJourneyInstance(
            JourneyNames.ConnectOneLogin,
            new ConnectOneLoginState(),
            new KeyValuePair<string, object>("personId", person.PersonId));

        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/connect-one-login?{journeyInstance.GetUniqueIdQueryParameter()}")
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
        Assert.Equal($"/persons/{person.PersonId}/connect-one-login/match?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_Cancel_DeletesJourneyAndRedirectsToPersonDetail()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstance(
            JourneyNames.ConnectOneLogin,
            new ConnectOneLoginState(),
            new KeyValuePair<string, object>("personId", person.PersonId));

        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/connect-one-login?handler=Cancel&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>())
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/persons/{person.PersonId}", response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Null(journeyInstance);
    }
}
