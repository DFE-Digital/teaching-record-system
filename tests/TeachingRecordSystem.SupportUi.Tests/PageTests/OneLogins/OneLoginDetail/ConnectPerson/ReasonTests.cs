using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Optional;
using TeachingRecordSystem.SupportUi.Pages.OneLogins.OneLoginDetail.ConnectPerson;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.OneLogins.OneLoginDetail.ConnectPerson;

public class ReasonTests(HostFixture hostFixture) : ConnectPersonTestBase(hostFixture)
{
    [Fact]
    public async Task Get_WithoutJourneyInstance_ReturnsBadRequest()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null);
        var request = new HttpRequestMessage(HttpMethod.Get, $"/one-logins/{oneLoginUser.Subject}/connect-person/reason");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
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

        var journeyInstance = await CreateJourneyInstanceAsync(
            oneLoginUser.Subject,
            new ConnectPersonState
            {
                PersonId = person.PersonId,
                PersonTrn = person.Trn
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/one-logins/{oneLoginUser.Subject}/connect-person/reason?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var radioButtons = doc.GetElementByTestId("reason-options")!
            .QuerySelectorAll<IHtmlInputElement>("input[type='radio']");
        Assert.Equal(3, radioButtons.Count());

        var hintText = doc.QuerySelector(".govuk-inset-text");
        Assert.NotNull(hintText);
        Assert.Contains(person.Trn, hintText.TextContent);
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

        var journeyInstance = await CreateJourneyInstanceAsync(
            oneLoginUser.Subject,
            new ConnectPersonState
            {
                PersonId = person.PersonId,
                PersonTrn = person.Trn,
                ConnectReason = ConnectPersonReason.AnotherReason,
                ReasonDetail = "Test reason detail"
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/one-logins/{oneLoginUser.Subject}/connect-person/reason?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var selectedRadio = doc.GetElementByTestId("reason-options")!
            .QuerySelectorAll<IHtmlInputElement>("input[type='radio']")
            .Single(i => i.IsChecked);
        Assert.Equal(ConnectPersonReason.AnotherReason.ToString(), selectedRadio.Value);

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

        var journeyInstance = await CreateJourneyInstanceAsync(
            oneLoginUser.Subject,
            new ConnectPersonState
            {
                PersonId = person.PersonId,
                PersonTrn = person.Trn
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/one-logins/{oneLoginUser.Subject}/connect-person/reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
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

        var journeyInstance = await CreateJourneyInstanceAsync(
            oneLoginUser.Subject,
            new ConnectPersonState
            {
                PersonId = person.PersonId,
                PersonTrn = person.Trn
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/one-logins/{oneLoginUser.Subject}/connect-person/reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "ConnectReason", ConnectPersonReason.AnotherReason.ToString() }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "ReasonDetail", "Enter details");
    }

    [Fact]
    public async Task Post_WithValidReason_UpdatesStateAndRedirectsToCheckAnswers()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(
            personId: null,
            email: Option.Some<string?>("test@example.com"),
            verifiedInfo: (["John", "Doe"], new DateOnly(1990, 1, 15)));

        var journeyInstance = await CreateJourneyInstanceAsync(
            oneLoginUser.Subject,
            new ConnectPersonState
            {
                PersonId = person.PersonId,
                PersonTrn = person.Trn
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/one-logins/{oneLoginUser.Subject}/connect-person/reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "ConnectReason", ConnectPersonReason.DataLossOrIncompleteInformation.ToString() }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/one-logins/{oneLoginUser.Subject}/connect-person/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);

        var journeyState = GetJourneyInstanceState(journeyInstance);
        Assert.Equal(ConnectPersonReason.DataLossOrIncompleteInformation, journeyState!.ConnectReason);
    }

    [Fact]
    public async Task Post_WithAnotherReasonAndDetail_UpdatesStateAndRedirectsToCheckAnswers()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(
            personId: null,
            email: Option.Some<string?>("test@example.com"),
            verifiedInfo: (["John", "Doe"], new DateOnly(1990, 1, 15)));

        var journeyInstance = await CreateJourneyInstanceAsync(
            oneLoginUser.Subject,
            new ConnectPersonState
            {
                PersonId = person.PersonId,
                PersonTrn = person.Trn
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/one-logins/{oneLoginUser.Subject}/connect-person/reason?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "ConnectReason", ConnectPersonReason.AnotherReason.ToString() },
                { "ReasonDetail", "Custom connection reason" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/one-logins/{oneLoginUser.Subject}/connect-person/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);

        var journeyState = GetJourneyInstanceState(journeyInstance);
        Assert.Equal(ConnectPersonReason.AnotherReason, journeyState!.ConnectReason);
        Assert.Equal("Custom connection reason", journeyState!.ReasonDetail);
    }

    [Fact]
    public async Task Post_FromCheckAnswers_RedirectsBackToCheckAnswers()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(
            personId: null,
            email: Option.Some<string?>("test@example.com"),
            verifiedInfo: (["John", "Doe"], new DateOnly(1990, 1, 15)));

        var journeyInstance = await CreateJourneyInstanceAsync(
            oneLoginUser.Subject,
            new ConnectPersonState
            {
                PersonId = person.PersonId,
                PersonTrn = person.Trn,
                ConnectReason = ConnectPersonReason.DataLossOrIncompleteInformation
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/one-logins/{oneLoginUser.Subject}/connect-person/reason?fromCheckAnswers=True&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "ConnectReason", ConnectPersonReason.NewInformationReceived.ToString() }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/one-logins/{oneLoginUser.Subject}/connect-person/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }
}
