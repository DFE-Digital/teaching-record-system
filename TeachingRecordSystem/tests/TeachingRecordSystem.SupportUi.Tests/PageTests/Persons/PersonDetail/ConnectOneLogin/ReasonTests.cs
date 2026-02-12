using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Optional;
using TeachingRecordSystem.Core.Services.OneLogin;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.ConnectOneLogin;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.ConnectOneLogin;

public class ReasonTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_WithoutJourneyInstance_ReturnsNotFound()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/connect-one-login/reason");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_WithValidJourney_RendersExpectedContent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(
            personId: null,
            email: Option.Some<string?>("test@example.com"),
            verifiedInfo: (["John", "Doe"], new DateOnly(1990, 1, 15)));

        var journeyInstance = await CreateJourneyInstance(
            JourneyNames.ConnectOneLogin,
            new ConnectOneLoginState
            {
                Subject = oneLoginUser.Subject,
                OneLoginEmailAddress = oneLoginUser.EmailAddress,
                MatchedPerson = new MatchPersonResult(person.PersonId, person.Trn, [])
            },
            new KeyValuePair<string, object>("personId", person.PersonId));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/connect-one-login/reason?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var radioButtons = doc.GetElementByTestId("reason-options")!
            .QuerySelectorAll<IHtmlInputElement>("input[type='radio']");
        Assert.Equal(3, radioButtons.Count());
    }

    [Fact]
    public async Task Get_WithExistingStateData_PopulatesForm()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(
            personId: null,
            email: Option.Some<string?>("test@example.com"),
            verifiedInfo: (["John", "Doe"], new DateOnly(1990, 1, 15)));

        var journeyInstance = await CreateJourneyInstance(
            JourneyNames.ConnectOneLogin,
            new ConnectOneLoginState
            {
                Subject = oneLoginUser.Subject,
                OneLoginEmailAddress = oneLoginUser.EmailAddress,
                MatchedPerson = new MatchPersonResult(person.PersonId, person.Trn, []),
                ConnectReason = ConnectOneLoginReason.AnotherReason,
                ReasonDetail = "Test reason detail"
            },
            new KeyValuePair<string, object>("personId", person.PersonId));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/connect-one-login/reason?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var selectedRadio = doc.GetElementByTestId("reason-options")!
            .QuerySelectorAll<IHtmlInputElement>("input[type='radio']")
            .Single(i => i.IsChecked);
        Assert.Equal(ConnectOneLoginReason.AnotherReason.ToString(), selectedRadio.Value);

        var reasonDetailTextArea = doc.QuerySelector("textarea[name='ReasonDetail']");
        Assert.NotNull(reasonDetailTextArea);
        Assert.Equal("Test reason detail", reasonDetailTextArea.TextContent);
    }

    [Fact]
    public async Task Post_WithoutReason_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(
            personId: null,
            email: Option.Some<string?>("test@example.com"),
            verifiedInfo: (["John", "Doe"], new DateOnly(1990, 1, 15)));

        var journeyInstance = await CreateJourneyInstance(
            JourneyNames.ConnectOneLogin,
            new ConnectOneLoginState
            {
                Subject = oneLoginUser.Subject,
                OneLoginEmailAddress = oneLoginUser.EmailAddress,
                MatchedPerson = new MatchPersonResult(person.PersonId, person.Trn, [])
            },
            new KeyValuePair<string, object>("personId", person.PersonId));

        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/connect-one-login/reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>())
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "ConnectReason", "Select a reason");
    }

    [Fact]
    public async Task Post_WithAnotherReasonButNoDetail_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(
            personId: null,
            email: Option.Some<string?>("test@example.com"),
            verifiedInfo: (["John", "Doe"], new DateOnly(1990, 1, 15)));

        var journeyInstance = await CreateJourneyInstance(
            JourneyNames.ConnectOneLogin,
            new ConnectOneLoginState
            {
                Subject = oneLoginUser.Subject,
                OneLoginEmailAddress = oneLoginUser.EmailAddress,
                MatchedPerson = new MatchPersonResult(person.PersonId, person.Trn, [])
            },
            new KeyValuePair<string, object>("personId", person.PersonId));

        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/connect-one-login/reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["ConnectReason"] = $"{ConnectOneLoginReason.AnotherReason}"
            })
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "ReasonDetail", "Enter details");
    }

    [Fact]
    public async Task Post_WithAnotherReasonAndDetailTooLong_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(
            personId: null,
            email: Option.Some<string?>("test@example.com"),
            verifiedInfo: (["John", "Doe"], new DateOnly(1990, 1, 15)));

        var journeyInstance = await CreateJourneyInstance(
            JourneyNames.ConnectOneLogin,
            new ConnectOneLoginState
            {
                Subject = oneLoginUser.Subject,
                OneLoginEmailAddress = oneLoginUser.EmailAddress,
                MatchedPerson = new MatchPersonResult(person.PersonId, person.Trn, [])
            },
            new KeyValuePair<string, object>("personId", person.PersonId));

        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/connect-one-login/reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["ConnectReason"] = ((int)ConnectOneLoginReason.AnotherReason).ToString(),
                ["ReasonDetail"] = new string('x', 4001)
            })
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "ReasonDetail", "Details must be 4000 characters or less");
    }

    [Fact]
    public async Task Post_WithSystemCouldNotMatch_UpdatesStateAndRedirects()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(
            personId: null,
            email: Option.Some<string?>("test@example.com"),
            verifiedInfo: (["John", "Doe"], new DateOnly(1990, 1, 15)));

        var journeyInstance = await CreateJourneyInstance(
            JourneyNames.ConnectOneLogin,
            new ConnectOneLoginState
            {
                Subject = oneLoginUser.Subject,
                OneLoginEmailAddress = oneLoginUser.EmailAddress,
                MatchedPerson = new MatchPersonResult(person.PersonId, person.Trn, [])
            },
            new KeyValuePair<string, object>("personId", person.PersonId));

        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/connect-one-login/reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["ConnectReason"] = ((int)ConnectOneLoginReason.SystemCouldNotMatch).ToString()
            })
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/persons/{person.PersonId}/connect-one-login/check-answers", response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(ConnectOneLoginReason.SystemCouldNotMatch, journeyInstance!.State.ConnectReason);
        Assert.Null(journeyInstance.State.ReasonDetail);
    }

    [Fact]
    public async Task Post_WithConnectedToWrongOneLogin_UpdatesStateAndRedirects()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(
            personId: null,
            email: Option.Some<string?>("test@example.com"),
            verifiedInfo: (["John", "Doe"], new DateOnly(1990, 1, 15)));

        var journeyInstance = await CreateJourneyInstance(
            JourneyNames.ConnectOneLogin,
            new ConnectOneLoginState
            {
                Subject = oneLoginUser.Subject,
                OneLoginEmailAddress = oneLoginUser.EmailAddress,
                MatchedPerson = new MatchPersonResult(person.PersonId, person.Trn, [])
            },
            new KeyValuePair<string, object>("personId", person.PersonId));

        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/connect-one-login/reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["ConnectReason"] = ((int)ConnectOneLoginReason.ConnectedToWrongOneLogin).ToString()
            })
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/persons/{person.PersonId}/connect-one-login/check-answers", response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(ConnectOneLoginReason.ConnectedToWrongOneLogin, journeyInstance!.State.ConnectReason);
        Assert.Null(journeyInstance.State.ReasonDetail);
    }

    [Fact]
    public async Task Post_WithAnotherReasonAndDetail_UpdatesStateAndRedirects()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(
            personId: null,
            email: Option.Some<string?>("test@example.com"),
            verifiedInfo: (["John", "Doe"], new DateOnly(1990, 1, 15)));

        var journeyInstance = await CreateJourneyInstance(
            JourneyNames.ConnectOneLogin,
            new ConnectOneLoginState
            {
                Subject = oneLoginUser.Subject,
                OneLoginEmailAddress = oneLoginUser.EmailAddress,
                MatchedPerson = new MatchPersonResult(person.PersonId, person.Trn, [])
            },
            new KeyValuePair<string, object>("personId", person.PersonId));

        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/connect-one-login/reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["ConnectReason"] = ((int)ConnectOneLoginReason.AnotherReason).ToString(),
                ["ReasonDetail"] = "Custom reason for connection"
            })
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/persons/{person.PersonId}/connect-one-login/check-answers", response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(ConnectOneLoginReason.AnotherReason, journeyInstance!.State.ConnectReason);
        Assert.Equal("Custom reason for connection", journeyInstance.State.ReasonDetail);
    }

    [Fact]
    public async Task Post_Cancel_DeletesJourneyAndRedirectsToPersonDetail()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(
            personId: null,
            email: Option.Some<string?>("test@example.com"),
            verifiedInfo: (["John", "Doe"], new DateOnly(1990, 1, 15)));

        var journeyInstance = await CreateJourneyInstance(
            JourneyNames.ConnectOneLogin,
            new ConnectOneLoginState
            {
                Subject = oneLoginUser.Subject,
                OneLoginEmailAddress = oneLoginUser.EmailAddress,
                MatchedPerson = new MatchPersonResult(person.PersonId, person.Trn, [])
            },
            new KeyValuePair<string, object>("personId", person.PersonId));

        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/connect-one-login/reason?handler=Cancel&{journeyInstance.GetUniqueIdQueryParameter()}")
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
