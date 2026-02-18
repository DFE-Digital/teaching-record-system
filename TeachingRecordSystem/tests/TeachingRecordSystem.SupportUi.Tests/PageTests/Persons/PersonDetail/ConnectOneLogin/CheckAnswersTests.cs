using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Optional;
using TeachingRecordSystem.Core.Services.OneLogin;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.ConnectOneLogin;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.ConnectOneLogin;

public class CheckAnswersTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_WithoutJourneyInstance_ReturnsNotFound()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/connect-one-login/check-answers");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_WithoutReason_RedirectsToReasonPage()
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

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/connect-one-login/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/persons/{person.PersonId}/connect-one-login/reason", response.Headers.Location?.OriginalString);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Get_WithValidJourney_RendersExpectedContent(bool isVerified)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = isVerified
            ? await TestData.CreateOneLoginUserAsync(
                personId: null,
                email: Option.Some<string?>("test@example.com"),
                verifiedInfo: (["John", "Doe"], new DateOnly(1990, 1, 15)))
            : await TestData.CreateOneLoginUserAsync(
                email: Option.Some<string?>("test@example.com"),
                verified: false);

        var journeyInstance = await CreateJourneyInstance(
            JourneyNames.ConnectOneLogin,
            new ConnectOneLoginState
            {
                Subject = oneLoginUser.Subject,
                OneLoginEmailAddress = oneLoginUser.EmailAddress,
                MatchedPerson = new MatchPersonResult(person.PersonId, person.Trn, []),
                ConnectReason = ConnectOneLoginReason.SystemCouldNotMatch
            },
            new KeyValuePair<string, object>("personId", person.PersonId));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/connect-one-login/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var summaryList = doc.GetElementByTestId("summary");
        Assert.NotNull(summaryList);
        Assert.Equal("test@example.com", summaryList.GetSummaryListValueByKey("GOV.UK One Login email address"));
        Assert.Equal("The system could not find a match automatically", summaryList.GetSummaryListValueByKey("Reason"));

        var expectedChangeLink = $"/persons/{person.PersonId}/connect-one-login/reason?fromCheckAnswers=True&{journeyInstance.GetUniqueIdQueryParameter()}";
        Assert.Equal(expectedChangeLink, doc.GetElementByTestId("change-reason-link")?.GetAttribute("href"));

        var checkbox = doc.QuerySelector<IHtmlInputElement>("input[name='IdentityConfirmed'][type='checkbox']");
        if (isVerified)
        {
            Assert.Null(checkbox);
        }
        else
        {
            Assert.NotNull(checkbox);
        }
    }

    [Fact]
    public async Task Get_WithAnotherReasonAndDetail_DisplaysReasonWithDetail()
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
                ReasonDetail = "Custom connection reason"
            },
            new KeyValuePair<string, object>("personId", person.PersonId));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/connect-one-login/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var summaryList = doc.GetElementByTestId("summary");
        Assert.NotNull(summaryList);
        Assert.Equal("Another reason: Custom connection reason", summaryList.GetSummaryListValueByKey("Reason"));
    }

    [Fact]
    public async Task Post_WithoutConfirmation_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(
            email: Option.Some<string?>("test@example.com"),
            verified: false);

        var journeyInstance = await CreateJourneyInstance(
            JourneyNames.ConnectOneLogin,
            new ConnectOneLoginState
            {
                Subject = oneLoginUser.Subject,
                OneLoginEmailAddress = oneLoginUser.EmailAddress,
                MatchedPerson = new MatchPersonResult(person.PersonId, person.Trn, []),
                ConnectReason = ConnectOneLoginReason.SystemCouldNotMatch
            },
            new KeyValuePair<string, object>("personId", person.PersonId));

        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/connect-one-login/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["IdentityConfirmed"] = "false"
            })
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "IdentityConfirmed", "Confirm you’ve completed the required identity checks");
    }

    [Fact]
    public async Task Post_WithConfirmation_CompletesJourneyAndRedirects()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(
            email: Option.Some<string?>("test@example.com"),
            verified: false);

        var journeyInstance = await CreateJourneyInstance(
            JourneyNames.ConnectOneLogin,
            new ConnectOneLoginState
            {
                Subject = oneLoginUser.Subject,
                OneLoginEmailAddress = oneLoginUser.EmailAddress,
                MatchedPerson = new MatchPersonResult(person.PersonId, person.Trn, []),
                ConnectReason = ConnectOneLoginReason.SystemCouldNotMatch
            },
            new KeyValuePair<string, object>("personId", person.PersonId));

        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/connect-one-login/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["IdentityConfirmed"] = "true"
            })
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/persons/{person.PersonId}", response.Headers.Location?.OriginalString);

        await WithDbContextAsync(async dbContext =>
        {
            var updatedOneLoginUser = await dbContext.OneLoginUsers.SingleAsync(o => o.Subject == oneLoginUser.Subject);
            Assert.Equal(Clock.UtcNow, updatedOneLoginUser.VerifiedOn);
            Assert.Equal(OneLoginUserVerificationRoute.Support, updatedOneLoginUser.VerificationRoute);
            Assert.Equal(person.PersonId, updatedOneLoginUser.PersonId);
            Assert.Equal(Clock.UtcNow, updatedOneLoginUser.MatchedOn);
            Assert.Equal(OneLoginUserMatchRoute.SupportUi, updatedOneLoginUser.MatchRoute);
            Assert.NotNull(updatedOneLoginUser.VerifiedNames);
            Assert.Single(updatedOneLoginUser.VerifiedNames);
            var expectedNames = new[] { person.FirstName, person.MiddleName, person.LastName }
                .Where(n => !string.IsNullOrEmpty(n))
                .ToArray();
            Assert.Equal(expectedNames, updatedOneLoginUser.VerifiedNames[0]);
            Assert.NotNull(updatedOneLoginUser.VerifiedDatesOfBirth);
            Assert.Single(updatedOneLoginUser.VerifiedDatesOfBirth);
            Assert.Equal(person.DateOfBirth, updatedOneLoginUser.VerifiedDatesOfBirth[0]);
        });

        Events.AssertProcessesCreated(p => Assert.Equal(ProcessType.PersonOneLoginUserConnecting, p.ProcessContext.ProcessType));

        var nextPage = await response.FollowRedirectAsync(HttpClient);
        var nextPageDoc = await nextPage.GetDocumentAsync();
        var expectedFlashMessage = $"Record connected to {StringHelper.JoinNonEmpty(' ', person.FirstName, person.MiddleName, person.LastName)}’s GOV.UK One Login";
        AssertEx.HtmlDocumentHasFlashSuccess(nextPageDoc, expectedFlashMessage);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.NotNull(journeyInstance);
        Assert.True(journeyInstance.Completed);
    }

    [Fact]
    public async Task Post_WithVerifiedOneLoginUser_CanProceedWithoutConfirmation()
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
                ConnectReason = ConnectOneLoginReason.SystemCouldNotMatch
            },
            new KeyValuePair<string, object>("personId", person.PersonId));

        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/connect-one-login/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>())
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/persons/{person.PersonId}", response.Headers.Location?.OriginalString);

        await WithDbContextAsync(async dbContext =>
        {
            var updatedOneLoginUser = await dbContext.OneLoginUsers.SingleAsync(o => o.Subject == oneLoginUser.Subject);
            Assert.NotNull(updatedOneLoginUser.VerifiedOn);
            Assert.Equal(OneLoginUserVerificationRoute.OneLogin, updatedOneLoginUser.VerificationRoute);
            Assert.Equal(person.PersonId, updatedOneLoginUser.PersonId);
            Assert.Equal(Clock.UtcNow, updatedOneLoginUser.MatchedOn);
            Assert.Equal(OneLoginUserMatchRoute.SupportUi, updatedOneLoginUser.MatchRoute);
        });

        Events.AssertProcessesCreated(p => Assert.Equal(ProcessType.PersonOneLoginUserConnecting, p.ProcessContext.ProcessType));

        var nextPage = await response.FollowRedirectAsync(HttpClient);
        var nextPageDoc = await nextPage.GetDocumentAsync();
        var expectedFlashMessage = $"Record connected to {StringHelper.JoinNonEmpty(' ', person.FirstName, person.MiddleName, person.LastName)}’s GOV.UK One Login";
        AssertEx.HtmlDocumentHasFlashSuccess(nextPageDoc, expectedFlashMessage);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.NotNull(journeyInstance);
        Assert.True(journeyInstance.Completed);
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
                MatchedPerson = new MatchPersonResult(person.PersonId, person.Trn, []),
                ConnectReason = ConnectOneLoginReason.SystemCouldNotMatch
            },
            new KeyValuePair<string, object>("personId", person.PersonId));

        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/connect-one-login/check-answers?handler=Cancel&{journeyInstance.GetUniqueIdQueryParameter()}")
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
