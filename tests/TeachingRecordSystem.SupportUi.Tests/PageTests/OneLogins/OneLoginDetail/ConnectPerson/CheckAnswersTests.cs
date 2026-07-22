using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Optional;
using TeachingRecordSystem.Core.Events.ChangeReasons;
using TeachingRecordSystem.SupportUi.Pages.OneLogins.OneLoginDetail.ConnectPerson;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.OneLogins.OneLoginDetail.ConnectPerson;

public class CheckAnswersTests(HostFixture hostFixture) : ConnectPersonTestBase(hostFixture)
{
    [Fact]
    public async Task Get_WithoutJourneyInstance_ReturnsBadRequest()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(personId: null);
        var request = new HttpRequestMessage(HttpMethod.Get, $"/one-logins/{oneLoginUser.Subject}/connect-person/check-answers");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
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
                personId: null,
                email: Option.Some<string?>("test@example.com"),
                verifiedInfo: null);

        var journeyInstance = await CreateJourneyInstanceAsync(
            oneLoginUser.Subject,
            new ConnectPersonState
            {
                PersonId = person.PersonId,
                PersonTrn = person.Trn,
                ConnectReason = ConnectPersonReason.DataLossOrIncompleteInformation
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/one-logins/{oneLoginUser.Subject}/connect-person/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var summaryList = doc.GetElementByTestId("summary");
        Assert.NotNull(summaryList);
        Assert.Equal("test@example.com", summaryList.GetSummaryListValueByKey("GOV.UK One Login email address"));
        Assert.Equal(person.Trn, summaryList.GetSummaryListValueByKey("TRN"));
        Assert.Equal("Data loss or incomplete information", summaryList.GetSummaryListValueByKey("Reason"));

        var checkAnswersUrl = $"/one-logins/{oneLoginUser.Subject}/connect-person/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}";
        var expectedChangeLink = $"/one-logins/{oneLoginUser.Subject}/connect-person/reason?returnUrl={Uri.EscapeDataString(checkAnswersUrl)}&{journeyInstance.GetUniqueIdQueryParameter()}";
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

        var journeyInstance = await CreateJourneyInstanceAsync(
            oneLoginUser.Subject,
            new ConnectPersonState
            {
                PersonId = person.PersonId,
                PersonTrn = person.Trn,
                ConnectReason = ConnectPersonReason.AnotherReason,
                ReasonDetail = "Custom connection reason"
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/one-logins/{oneLoginUser.Subject}/connect-person/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

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
            personId: null,
            email: Option.Some<string?>("test@example.com"),
            verifiedInfo: null);

        var journeyInstance = await CreateJourneyInstanceAsync(
            oneLoginUser.Subject,
            new ConnectPersonState
            {
                PersonId = person.PersonId,
                PersonTrn = person.Trn,
                ConnectReason = ConnectPersonReason.DataLossOrIncompleteInformation
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/one-logins/{oneLoginUser.Subject}/connect-person/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "IdentityConfirmed", "false" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "IdentityConfirmed", "Confirm you’ve completed the required identity checks");
    }

    [Fact]
    public async Task Post_WithConfirmation_CompletesJourneyAndRedirectsToPersonDetail()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(
            personId: null,
            email: Option.Some<string?>("test@example.com"),
            verifiedInfo: null);

        var journeyInstance = await CreateJourneyInstanceAsync(
            oneLoginUser.Subject,
            new ConnectPersonState
            {
                PersonId = person.PersonId,
                PersonTrn = person.Trn,
                ConnectReason = ConnectPersonReason.DataLossOrIncompleteInformation
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/one-logins/{oneLoginUser.Subject}/connect-person/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "IdentityConfirmed", "true" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/persons/{person.PersonId}", response.Headers.Location?.OriginalString);

        await WithDbContextAsync(async dbContext =>
        {
            var updatedOneLoginUser = await dbContext.OneLoginUsers.SingleAsync(o => o.Subject == oneLoginUser.Subject);
            Assert.Equal(TimeProvider.UtcNow, updatedOneLoginUser.VerifiedOn);
            Assert.Equal(OneLoginUserVerificationRoute.Support, updatedOneLoginUser.VerificationRoute);
            Assert.Equal(person.PersonId, updatedOneLoginUser.PersonId);
            Assert.Equal(TimeProvider.UtcNow, updatedOneLoginUser.MatchedOn);
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

        Events.AssertProcessesCreated(p =>
        {
            Assert.Equal(ProcessType.OneLoginUserPersonConnecting, p.ProcessContext.ProcessType);
            Assert.NotNull(p.ProcessContext.Process.ChangeReason);
            var changeReason = Assert.IsType<ChangeReasonWithDetailsAndEvidence>(p.ProcessContext.Process.ChangeReason);
            Assert.Equal("Data loss or incomplete information", changeReason.Reason);
            Assert.Null(changeReason.Details);
        });

        var nextPage = await response.FollowRedirectAsync(HttpClient);
        var nextPageDoc = await nextPage.GetDocumentAsync();
        var expectedFlashMessage = $"Record connected to {string.JoinNonEmpty(' ', person.FirstName, person.MiddleName, person.LastName)}’s GOV.UK One Login";
        AssertEx.HtmlDocumentHasFlashNotificationBanner(nextPageDoc, expectedFlashMessage);

        Assert.Null(GetJourneyInstanceState(journeyInstance));
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

        var journeyInstance = await CreateJourneyInstanceAsync(
            oneLoginUser.Subject,
            new ConnectPersonState
            {
                PersonId = person.PersonId,
                PersonTrn = person.Trn,
                ConnectReason = ConnectPersonReason.DataLossOrIncompleteInformation
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/one-logins/{oneLoginUser.Subject}/connect-person/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
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
            Assert.Equal(TimeProvider.UtcNow, updatedOneLoginUser.MatchedOn);
            Assert.Equal(OneLoginUserMatchRoute.SupportUi, updatedOneLoginUser.MatchRoute);
        });

        Events.AssertProcessesCreated(p =>
        {
            Assert.Equal(ProcessType.OneLoginUserPersonConnecting, p.ProcessContext.ProcessType);
            Assert.NotNull(p.ProcessContext.Process.ChangeReason);
            var changeReason = Assert.IsType<ChangeReasonWithDetailsAndEvidence>(p.ProcessContext.Process.ChangeReason);
            Assert.Equal("Data loss or incomplete information", changeReason.Reason);
            Assert.Null(changeReason.Details);
        });

        var nextPage = await response.FollowRedirectAsync(HttpClient);
        var nextPageDoc = await nextPage.GetDocumentAsync();
        var expectedFlashMessage = $"Record connected to {string.JoinNonEmpty(' ', person.FirstName, person.MiddleName, person.LastName)}’s GOV.UK One Login";
        AssertEx.HtmlDocumentHasFlashNotificationBanner(nextPageDoc, expectedFlashMessage);

        Assert.Null(GetJourneyInstanceState(journeyInstance));
    }

    [Fact]
    public async Task Post_WithAnotherReasonAndDetail_SavesReasonInProcessContext()
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
                ReasonDetail = "Custom connection reason details"
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/one-logins/{oneLoginUser.Subject}/connect-person/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/persons/{person.PersonId}", response.Headers.Location?.OriginalString);

        Events.AssertProcessesCreated(p =>
        {
            Assert.Equal(ProcessType.OneLoginUserPersonConnecting, p.ProcessContext.ProcessType);
            Assert.NotNull(p.ProcessContext.Process.ChangeReason);
            var changeReason = Assert.IsType<ChangeReasonWithDetailsAndEvidence>(p.ProcessContext.Process.ChangeReason);
            Assert.Equal("Another reason", changeReason.Reason);
            Assert.Equal("Custom connection reason details", changeReason.Details);
        });

        Assert.Null(GetJourneyInstanceState(journeyInstance));
    }
}
