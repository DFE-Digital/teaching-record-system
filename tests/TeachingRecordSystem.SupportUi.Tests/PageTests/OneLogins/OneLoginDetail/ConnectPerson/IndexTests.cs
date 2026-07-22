using TeachingRecordSystem.SupportUi.Pages.OneLogins.OneLoginDetail.ConnectPerson;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.OneLogins.OneLoginDetail.ConnectPerson;

public class IndexTests(HostFixture hostFixture) : ConnectPersonTestBase(hostFixture)
{
    [Fact]
    public async Task Get_WithValidOneLoginUserSubject_RedirectsWithJourneyInstanceId()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null);
        var request = new HttpRequestMessage(HttpMethod.Get, $"/one-logins/{oneLoginUser.Subject}/connect-person");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/one-logins/{oneLoginUser.Subject}/connect-person?_jid", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_WithJourneyInstanceId_RendersExpectedContent()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null);

        var journeyInstance = await CreateJourneyInstanceAsync(
            oneLoginUser.Subject,
            new ConnectPersonState());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/one-logins/{oneLoginUser.Subject}/connect-person?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.NotNull(doc.GetElementById("Trn"));
    }

    [Fact]
    public async Task Get_WithInvalidOneLoginUserSubject_ReturnsNotFound()
    {
        // Arrange
        var oneLoginUserSubject = "invalid-subject";

        var journeyInstance = await CreateJourneyInstanceAsync(
            oneLoginUserSubject,
            new ConnectPersonState());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/one-logins/{oneLoginUserSubject}/connect-person?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_WithEmptyTrn_ReturnsError()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null);

        var journeyInstance = await CreateJourneyInstanceAsync(
            oneLoginUser.Subject,
            new ConnectPersonState());

        var request = new HttpRequestMessage(HttpMethod.Post, $"/one-logins/{oneLoginUser.Subject}/connect-person?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Trn", "" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "Trn", "Enter a TRN");
    }

    [Fact]
    public async Task Post_WithNonNumericTrn_ReturnsError()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null);

        var journeyInstance = await CreateJourneyInstanceAsync(
            oneLoginUser.Subject,
            new ConnectPersonState());

        var request = new HttpRequestMessage(HttpMethod.Post, $"/one-logins/{oneLoginUser.Subject}/connect-person?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Trn", "ABC1234" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "Trn", "TRN must be a number");
    }

    [Fact]
    public async Task Post_WithInvalidLengthTrn_ReturnsError()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null);

        var journeyInstance = await CreateJourneyInstanceAsync(
            oneLoginUser.Subject,
            new ConnectPersonState());

        var request = new HttpRequestMessage(HttpMethod.Post, $"/one-logins/{oneLoginUser.Subject}/connect-person?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Trn", "12345" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "Trn", "TRN must be 7 digits long");
    }

    [Fact]
    public async Task Post_WithValidTrnButNoMatchingPerson_ReturnsError()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null);
        var trn = "1234567";

        var journeyInstance = await CreateJourneyInstanceAsync(
            oneLoginUser.Subject,
            new ConnectPersonState());

        var request = new HttpRequestMessage(HttpMethod.Post, $"/one-logins/{oneLoginUser.Subject}/connect-person?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Trn", trn }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "Trn", "The TRN you entered does not exist");
    }

    [Fact]
    public async Task Post_WithOneLoginUserAlreadyConnectedToThisPerson_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(person);

        var journeyInstance = await CreateJourneyInstanceAsync(
            oneLoginUser.Subject,
            new ConnectPersonState());

        var request = new HttpRequestMessage(HttpMethod.Post, $"/one-logins/{oneLoginUser.Subject}/connect-person?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Trn", person.Trn }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "Trn", "This GOV.UK One Login is already connected to this record");
    }

    [Fact]
    public async Task Post_WithValidTrnAndUnconnectedOneLoginUser_CreatesJourneyAndRedirectsToMatchPage()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null);

        var journeyInstance = await CreateJourneyInstanceAsync(
            oneLoginUser.Subject,
            new ConnectPersonState());

        var request = new HttpRequestMessage(HttpMethod.Post, $"/one-logins/{oneLoginUser.Subject}/connect-person?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Trn", person.Trn }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/one-logins/{oneLoginUser.Subject}/connect-person/match?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);

        var journeyState = GetJourneyInstanceState(journeyInstance);
        Assert.Equal(person.PersonId, journeyState!.PersonId);
        Assert.Equal(person.Trn, journeyState!.PersonTrn);
    }

    [Fact]
    public async Task Post_WithOneLoginUserConnectedToAnotherPerson_AllowsConnection()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var otherPerson = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(otherPerson);

        var journeyInstance = await CreateJourneyInstanceAsync(
            oneLoginUser.Subject,
            new ConnectPersonState());

        var request = new HttpRequestMessage(HttpMethod.Post, $"/one-logins/{oneLoginUser.Subject}/connect-person?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Trn", person.Trn }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/one-logins/{oneLoginUser.Subject}/connect-person/match?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);

        var journeyState = GetJourneyInstanceState(journeyInstance);
        Assert.Equal(person.PersonId, journeyState!.PersonId);
        Assert.Equal(person.Trn, journeyState!.PersonTrn);
    }

    [Fact]
    public async Task Post_Cancel_DeletesJourneyAndRedirectsToOneLoginDetailIndex()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null);

        var journeyInstance = await CreateJourneyInstanceAsync(
            oneLoginUser.Subject,
            new ConnectPersonState());

        var request = new HttpRequestMessage(HttpMethod.Post, $"/one-logins/{oneLoginUser.Subject}/connect-person?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder { { "Cancel", "True" } }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/one-logins/{oneLoginUser.Subject}", response.Headers.Location?.OriginalString);

        Assert.Null(GetJourneyInstanceState(journeyInstance));
    }
}
