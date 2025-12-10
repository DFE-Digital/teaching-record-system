using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.OneLoginUserIdVerification.Resolve;

public class ConfirmConnectTests(HostFixture hostFixture) : ResolveOneLoginUserIdVerificationTestBase(hostFixture)
{
    [Fact]
    public async Task Get_UserIsNotVerified_RedirectsToIndex()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);

        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask,
            state => state.Verified = false);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/confirm-connect?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_NoPersonSelected_RedirectsToMatches()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);

        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask,
            state => state.Verified = true);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/confirm-connect?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_ValidRequest_ShowsExpectedData()
    {
        // Arrange
        var (oneLoginUser, supportTask, matchedPerson) = await CreateUserSupportTaskAndMatchingPerson();

        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask,
            state =>
            {
                state.Verified = true;
                state.MatchedPersonId = matchedPerson.PersonId;
            });

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/confirm-connect?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var oneLoginUserSummaryList = doc.GetElementByTestId("one-login-user-details");
        Assert.NotNull(oneLoginUserSummaryList);
        Assert.Equal(oneLoginUser.EmailAddress, oneLoginUserSummaryList.GetSummaryListValueByKey("Email address"));

        var matchedPersonSummaryList = doc.GetElementByTestId("matched-person-details");
        Assert.NotNull(matchedPersonSummaryList);
        Assert.Equal($"{matchedPerson.FirstName} {matchedPerson.MiddleName} {matchedPerson.LastName}", matchedPersonSummaryList.GetSummaryListValueByKey("Name"));
        Assert.Equal(matchedPerson.EmailAddress, matchedPersonSummaryList.GetSummaryListValueByKey("Email address"));
        Assert.Equal(matchedPerson.DateOfBirth?.ToString(UiDefaults.DateOnlyDisplayFormat), matchedPersonSummaryList.GetSummaryListValueByKey("Date of birth"));
        Assert.Equal(matchedPerson.Trn, matchedPersonSummaryList.GetSummaryListValueByKey("TRN"));
        Assert.Equal(matchedPerson.NationalInsuranceNumber, matchedPersonSummaryList.GetSummaryListValueByKey("NI number"));
    }

    [Fact]
    public async Task Post_MarksUserVerifiedAndMatchedClosesSupportTaskAndRedirectsToListPageWithFlashMessage()
    {
        // Arrange
        var (oneLoginUser, supportTask, matchedPerson) = await CreateUserSupportTaskAndMatchingPerson();
        var supportTaskData = supportTask.GetData<OneLoginUserIdVerificationData>();

        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask,
            state =>
            {
                state.Verified = true;
                state.MatchedPersonId = matchedPerson.PersonId;
            });

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/confirm-connect?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var updatedSupportTask = await
                dbContext.SupportTasks.SingleAsync(t => t.SupportTaskReference == supportTask.SupportTaskReference);
            Assert.Equal(SupportTaskStatus.Closed, updatedSupportTask.Status);
            Assert.Equal(Clock.UtcNow, updatedSupportTask.UpdatedOn);
            var updatedSupportTaskData = updatedSupportTask.GetData<OneLoginUserIdVerificationData>();
            Assert.True(updatedSupportTaskData.Verified);
            Assert.Equal(OneLoginUserIdVerificationOutcome.VerifiedAndConnected, updatedSupportTaskData.Outcome);

            var updatedOneLoginUser = await dbContext.OneLoginUsers.SingleAsync(o => o.Subject == oneLoginUser.Subject);
            Assert.Equal(Clock.UtcNow, updatedOneLoginUser.VerifiedOn);
            Assert.Equal(OneLoginUserVerificationRoute.Support, updatedOneLoginUser.VerificationRoute);
            Assert.NotNull(updatedOneLoginUser.VerifiedDatesOfBirth);
            Assert.Collection(updatedOneLoginUser.VerifiedDatesOfBirth, dob => Assert.Equal(supportTaskData.StatedDateOfBirth, dob));
            Assert.NotNull(updatedOneLoginUser.VerifiedNames);
            Assert.Collection(
                updatedOneLoginUser.VerifiedNames,
                names => Assert.Equivalent(new[] { supportTaskData.StatedFirstName, supportTaskData.StatedLastName }, names));
            Assert.Equal(Clock.UtcNow, updatedOneLoginUser.MatchedOn);
            Assert.Equal(OneLoginUserMatchRoute.Support, updatedOneLoginUser.MatchRoute);
            Assert.Equal(matchedPerson.PersonId, updatedOneLoginUser.PersonId);
            Assert.NotNull(updatedOneLoginUser.MatchedAttributes);
            Assert.Contains(updatedOneLoginUser.MatchedAttributes, a => a.Key == PersonMatchedAttribute.FirstName && a.Value == supportTaskData.StatedFirstName);
            Assert.Contains(updatedOneLoginUser.MatchedAttributes, a => a.Key == PersonMatchedAttribute.LastName && a.Value == supportTaskData.StatedLastName);
            Assert.Contains(updatedOneLoginUser.MatchedAttributes, a => a.Key == PersonMatchedAttribute.DateOfBirth && a.Value == supportTaskData.StatedDateOfBirth.ToString("yyyy-MM-dd"));
            Assert.Contains(updatedOneLoginUser.MatchedAttributes, a => a.Key == PersonMatchedAttribute.Trn && a.Value == supportTaskData.StatedTrn);
        });

        Events.AssertProcessesCreated(p =>
        {
            Assert.Equal(ProcessType.OneLoginUserIdVerificationSupportTaskCompleting, p.ProcessContext.ProcessType);
            p.AssertProcessHasEvents</*EmailSentEvent, */SupportTaskUpdatedEvent>();
        });

        var nextPage = await response.FollowRedirectAsync(HttpClient);
        var nextPageDoc = await nextPage.GetDocumentAsync();
        AssertEx.HtmlDocumentHasFlashSuccess(
            nextPageDoc,
            $"GOV.UK One Login account connected to {matchedPerson.FirstName} {matchedPerson.MiddleName} {matchedPerson.LastName}’s record");

        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal("/support-tasks/one-login-user-id-verification", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_Cancel_DeletesJourneyAndRedirectToListPage()
    {
        // Arrange
        var (oneLoginUser, supportTask, matchedPerson) = await CreateUserSupportTaskAndMatchingPerson();

        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask,
            state =>
            {
                state.Verified = true;
                state.MatchedPersonId = matchedPerson.PersonId;
            });

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/one-login-user-id-verification/{supportTask.SupportTaskReference}/resolve/confirm-connect?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder { { "Cancel", "true" } }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/support-tasks/one-login-user-id-verification", response.Headers.Location?.OriginalString);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Null(journeyInstance);
    }

    private async Task<(OneLoginUser User, SupportTask SupportTask, Person MatchedPerson)> CreateUserSupportTaskAndMatchingPerson()
    {
        var matchedPerson = await TestData.CreatePersonAsync(p => p.WithNationalInsuranceNumber().WithEmailAddress());

        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);

        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(
            oneLoginUser.Subject, t => t
                .WithStatedFirstName(matchedPerson.FirstName)
                .WithStatedLastName(matchedPerson.LastName)
                .WithStatedDateOfBirth(matchedPerson.DateOfBirth)
                .WithStatedTrn(matchedPerson.Trn!));

        return (oneLoginUser, supportTask, matchedPerson.Person);
    }
}
