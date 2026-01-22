using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.OneLoginUserMatching.Resolve;

public class ConfirmConnectTests(HostFixture hostFixture) : ResolveOneLoginUserMatchingTestBase(hostFixture)
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
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/confirm-connect?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Get_NoPersonSelected_RedirectsToMatches(bool isRecordMatchingOnlySupportTask)
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = isRecordMatchingOnlySupportTask ?
            await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(oneLoginUser.Subject) :
            await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);

        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask,
            state => state.Verified = true);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/confirm-connect?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Get_ValidRequest_ShowsExpectedData(bool isRecordMatchingOnlySupportTask)
    {
        // Arrange
        var (oneLoginUser, supportTask, matchedPerson) = await CreateUserSupportTaskAndMatchingPerson(isRecordMatchingOnlySupportTask);

        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask,
            state =>
            {
                state.Verified = true;
                state.MatchedPersonId = matchedPerson.PersonId;
            });

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/confirm-connect?{journeyInstance.GetUniqueIdQueryParameter()}");

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
        Assert.Equal(matchedPerson.DateOfBirth?.ToString(WebConstants.DateOnlyDisplayFormat), matchedPersonSummaryList.GetSummaryListValueByKey("Date of birth"));
        Assert.Equal(matchedPerson.Trn, matchedPersonSummaryList.GetSummaryListValueByKey("TRN"));
        Assert.Equal(matchedPerson.NationalInsuranceNumber, matchedPersonSummaryList.GetSummaryListValueByKey("NI number"));
    }

    [Fact]
    public async Task Post_IdVerificationSupportTaskMarksUserVerifiedAndMatchedClosesSupportTaskAndRedirectsToListPageWithFlashMessage()
    {
        // Arrange
        var (oneLoginUser, supportTask, matchedPerson) = await CreateUserSupportTaskAndMatchingPerson(isRecordMatchingOnlySupportTask: false);

        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask,
            state =>
            {
                state.Verified = true;
                state.MatchedPersonId = matchedPerson.PersonId;
            });

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/confirm-connect?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var updatedSupportTask = await
                dbContext.SupportTasks.SingleAsync(t => t.SupportTaskReference == supportTask.SupportTaskReference);
            Assert.Equal(SupportTaskStatus.Closed, updatedSupportTask.Status);
            var updatedSupportTaskData = updatedSupportTask.GetData<OneLoginUserIdVerificationData>();
            Assert.True(updatedSupportTaskData.Verified);
            Assert.Equal(OneLoginUserIdVerificationOutcome.VerifiedAndConnected, updatedSupportTaskData.Outcome);

            var updatedOneLoginUser = await dbContext.OneLoginUsers.SingleAsync(o => o.Subject == oneLoginUser.Subject);
            Assert.Equal(Clock.UtcNow, updatedOneLoginUser.VerifiedOn);
            Assert.Equal(matchedPerson.PersonId, updatedOneLoginUser.PersonId);
        });

        Events.AssertProcessesCreated(p => Assert.Equal(ProcessType.OneLoginUserIdVerificationSupportTaskCompleting, p.ProcessContext.ProcessType));

        var nextPage = await response.FollowRedirectAsync(HttpClient);
        var nextPageDoc = await nextPage.GetDocumentAsync();
        AssertEx.HtmlDocumentHasFlashSuccess(
            nextPageDoc,
            $"GOV.UK One Login connected to {matchedPerson.FirstName} {matchedPerson.MiddleName} {matchedPerson.LastName}’s record");

        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal("/support-tasks/one-login-user-matching/id-verification", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_RecordMatchingSupportTaskMarksUserVerifiedAndMatchedClosesSupportTaskAndRedirectsToListPageWithFlashMessage()
    {
        // Arrange
        var (oneLoginUser, supportTask, matchedPerson) = await CreateUserSupportTaskAndMatchingPerson(isRecordMatchingOnlySupportTask: true);

        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask,
            state =>
            {
                state.Verified = true;
                state.MatchedPersonId = matchedPerson.PersonId;
            });

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/confirm-connect?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var updatedSupportTask = await
                dbContext.SupportTasks.SingleAsync(t => t.SupportTaskReference == supportTask.SupportTaskReference);
            Assert.Equal(SupportTaskStatus.Closed, updatedSupportTask.Status);
            var updatedSupportTaskData = updatedSupportTask.GetData<OneLoginUserRecordMatchingData>();
            Assert.Equal(OneLoginUserRecordMatchingOutcome.Connected, updatedSupportTaskData.Outcome);

            var updatedOneLoginUser = await dbContext.OneLoginUsers.SingleAsync(o => o.Subject == oneLoginUser.Subject);
            Assert.Equal(matchedPerson.PersonId, updatedOneLoginUser.PersonId);
        });

        Events.AssertProcessesCreated(p => Assert.Equal(ProcessType.OneLoginUserRecordMatchingSupportTaskCompleting, p.ProcessContext.ProcessType));

        var nextPage = await response.FollowRedirectAsync(HttpClient);
        var nextPageDoc = await nextPage.GetDocumentAsync();
        AssertEx.HtmlDocumentHasFlashSuccess(
            nextPageDoc,
            $"GOV.UK One Login connected to {matchedPerson.FirstName} {matchedPerson.MiddleName} {matchedPerson.LastName}’s record");

        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal("/support-tasks/one-login-user-matching/record-matching", response.Headers.Location?.OriginalString);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Post_Cancel_DeletesJourneyAndRedirectToListPage(bool isRecordMatchingOnlySupportTask)
    {
        // Arrange
        var (oneLoginUser, supportTask, matchedPerson) = await CreateUserSupportTaskAndMatchingPerson(isRecordMatchingOnlySupportTask);

        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask,
            state =>
            {
                state.Verified = true;
                state.MatchedPersonId = matchedPerson.PersonId;
            });

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/confirm-connect?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder { { "Cancel", "true" } }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        if (isRecordMatchingOnlySupportTask)
        {
            Assert.Equal("/support-tasks/one-login-user-matching/record-matching", response.Headers.Location?.OriginalString);
        }
        else
        {
            Assert.Equal("/support-tasks/one-login-user-matching/id-verification", response.Headers.Location?.OriginalString);
        }

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Null(journeyInstance);
    }

    private async Task<(OneLoginUser User, SupportTask SupportTask, Person MatchedPerson)> CreateUserSupportTaskAndMatchingPerson(bool isRecordMatchingOnlySupportTask)
    {
        var matchedPerson = await TestData.CreatePersonAsync(p => p.WithNationalInsuranceNumber().WithEmailAddress());

        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: isRecordMatchingOnlySupportTask);

        var supportTask = isRecordMatchingOnlySupportTask ?
            await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(
                oneLoginUser.Subject, t => t
                    .WithVerifiedNames([matchedPerson.FirstName, matchedPerson.LastName])
                    .WithVerifiedDateOfBirth(matchedPerson.DateOfBirth)
                    .WithStatedTrn(matchedPerson.Trn!)) :
            await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(
                oneLoginUser.Subject, t => t
                    .WithStatedFirstName(matchedPerson.FirstName)
                    .WithStatedLastName(matchedPerson.LastName)
                    .WithStatedDateOfBirth(matchedPerson.DateOfBirth)
                    .WithStatedTrn(matchedPerson.Trn!));

        return (oneLoginUser, supportTask, matchedPerson.Person);
    }
}
