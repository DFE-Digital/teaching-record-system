using TeachingRecordSystem.Core.Models.SupportTasks;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.OneLoginUserMatching.Resolve;

public class NoMatchesTests(HostFixture hostFixture) : ResolveOneLoginUserMatchingTestBase(hostFixture)
{
    [Fact]
    public async Task Post_IdVerificationSupportTaskEmailsUserMarksUserVerifiedClosesSupportTaskAndRedirectsToListPageWithFlashMessage()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);
        var supportTaskData = supportTask.GetData<OneLoginUserIdVerificationData>();

        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask.SupportTaskReference,
            state => state.Verified = true,
            matchedPersons: []);

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/no-matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var updatedSupportTask = await
                dbContext.SupportTasks.SingleAsync(t => t.SupportTaskReference == supportTask.SupportTaskReference);
            Assert.Equal(SupportTaskStatus.Closed, updatedSupportTask.Status);
            var updatedSupportTaskData = updatedSupportTask.GetData<OneLoginUserIdVerificationData>();
            Assert.Equal(OneLoginUserIdVerificationOutcome.VerifiedOnlyWithoutMatches, updatedSupportTaskData.Outcome);

            var updatedOneLoginUser = await dbContext.OneLoginUsers.SingleAsync(o => o.Subject == oneLoginUser.Subject);
            Assert.Equal(TimeProvider.UtcNow, updatedOneLoginUser.VerifiedOn);
        });

        Events.AssertProcessesCreated(p => Assert.Equal(ProcessType.OneLoginUserIdVerificationSupportTaskCompleting, p.ProcessContext.ProcessType));

        var nextPage = await response.FollowRedirectAsync(HttpClient);
        var nextPageDoc = await nextPage.GetDocumentAsync();
        AssertEx.HtmlDocumentHasFlashNotificationBanner(
            nextPageDoc,
            "Email sent",
            $"Request closed for {supportTaskData.StatedFirstName} {supportTaskData.StatedLastName}. We’ve sent them an email confirming we could not find a teaching record matching their GOV.UK One Login.");

        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal("/support-tasks/one-login-user-matching/id-verification", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_RecordMatchingSupportTaskEmailsUserSetsOutcomeClosesSupportTaskAndRedirectsToListPageWithFlashMessage()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);
        var firstName = TestData.GenerateFirstName();
        var lastName = TestData.GenerateLastName();

        // An email is only sent for a record matching task when the application user has a Required
        // record matching policy and a "cannot find record" email template configured.
        var applicationUser = await TestData.CreateApplicationUserAsync(
            recordMatchingPolicy: RecordMatchingPolicy.Required,
            appContent: new AppContent { OneLoginCannotFindRecordEmailTemplateId = Guid.NewGuid().ToString() });

        var supportTask = await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(
            oneLoginUser.Subject, t => t
                .WithVerifiedNames([firstName, lastName])
                .WithClientApplicationUserId(applicationUser.UserId));
        var supportTaskData = supportTask.GetData<OneLoginUserRecordMatchingData>();

        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask.SupportTaskReference,
            state => state.Verified = true,
            matchedPersons: []);

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/no-matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await WithDbContextAsync(async dbContext =>
        {
            var updatedSupportTask = await
                dbContext.SupportTasks.SingleAsync(t => t.SupportTaskReference == supportTask.SupportTaskReference);
            Assert.Equal(SupportTaskStatus.Closed, updatedSupportTask.Status);
            var updatedSupportTaskData = updatedSupportTask.GetData<OneLoginUserRecordMatchingData>();
            Assert.Equal(OneLoginUserRecordMatchingOutcome.NoMatches, updatedSupportTaskData.Outcome);
        });

        Events.AssertProcessesCreated(p => Assert.Equal(ProcessType.OneLoginUserRecordMatchingSupportTaskCompleting, p.ProcessContext.ProcessType));

        var nextPage = await response.FollowRedirectAsync(HttpClient);
        var nextPageDoc = await nextPage.GetDocumentAsync();
        AssertEx.HtmlDocumentHasFlashNotificationBanner(
            nextPageDoc,
            "Email sent",
            $"Request closed for {firstName} {lastName}. We’ve sent them an email confirming we could not find a teaching record matching their GOV.UK One Login.");

        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal("/support-tasks/one-login-user-matching/record-matching", response.Headers.Location?.OriginalString);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Post_Cancel_DeletesJourneyAndRedirectsToListPage(bool isRecordMatchingOnlySupportTask)
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: isRecordMatchingOnlySupportTask);
        var supportTask = isRecordMatchingOnlySupportTask ?
            await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(oneLoginUser.Subject) :
            await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);

        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask.SupportTaskReference,
            state => state.Verified = true,
            matchedPersons: []);

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/no-matches?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "cancel", "true" }
            }
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

        Assert.Null(GetJourneyInstanceState(journeyInstance));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Get_WithCustomAppContent_DisplaysCustomPageContent(bool isRecordMatchingOnlySupportTask)
    {
        // Arrange
        var customEmailTemplateId = Guid.NewGuid().ToString();
        var customPageContent = "<p class='govuk-body'>This is custom HTML content for this service.</p><p class='govuk-body'>Please contact us if you need help.</p>";

        var applicationUser = await TestData.CreateApplicationUserAsync(
            isOidcClient: true,
            appContent: new AppContent
            {
                OneLoginCannotFindRecordEmailTemplateId = customEmailTemplateId,
                OneLoginNoMatchesPageContentHtml = customPageContent
            });

        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: !isRecordMatchingOnlySupportTask);
        var firstName = TestData.GenerateFirstName();
        var lastName = TestData.GenerateLastName();
        var supportTask = isRecordMatchingOnlySupportTask ?
            await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(
                oneLoginUser.Subject,
                t => t.WithVerifiedNames([firstName, lastName])
                      .WithClientApplicationUserId(applicationUser.UserId)) :
            await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(
                oneLoginUser.Subject,
                t => t.WithClientApplicationUserId(applicationUser.UserId));

        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask,
            state => state.Verified = true);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/no-matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();

        Assert.Contains("This is custom HTML content for this service.", doc.Body!.TextContent);
        Assert.Contains("Please contact us if you need help.", doc.Body!.TextContent);
        Assert.DoesNotContain("We’ll send them an email confirming we could not find a teaching record matching their GOV.UK One Login and asking them to check their details.", doc.Body!.TextContent);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Get_WithoutCustomAppContent_DisplaysDefaultPageContent(bool isRecordMatchingOnlySupportTask)
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: !isRecordMatchingOnlySupportTask);
        var firstName = TestData.GenerateFirstName();
        var lastName = TestData.GenerateLastName();
        var supportTask = isRecordMatchingOnlySupportTask ?
            await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(
                oneLoginUser.Subject,
                t => t.WithVerifiedNames([firstName, lastName])) :
            await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);

        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask,
            state => state.Verified = true);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/no-matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await response.GetDocumentAsync();

        Assert.Contains("send them an email", doc.Body!.TextContent);
        Assert.Contains("could not find a teaching record", doc.Body!.TextContent);
    }

    [Fact]
    public async Task Post_IdVerificationSupportTaskWithCustomFlashMessage_UsesCustomFlashMessage()
    {
        // Arrange
        var customFlashMessage = "Request closed for {0}. We’ve sent them an email with a link to continue their national professional qualification (NPQ) registration.";
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);

        var applicationUser = await TestData.CreateApplicationUserAsync(
            appContent: new AppContent
            {
                OneLoginCannotFindRecordEmailTemplateId = Guid.NewGuid().ToString(),
                OneLoginNoMatchesEmailSentFlashMessage = customFlashMessage
            });

        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(
            oneLoginUser.Subject, t => t.WithClientApplicationUserId(applicationUser.UserId));
        var supportTaskData = supportTask.GetData<OneLoginUserIdVerificationData>();

        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask.SupportTaskReference,
            state => state.Verified = true,
            matchedPersons: []);

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/no-matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var nextPage = await response.FollowRedirectAsync(HttpClient);
        var nextPageDoc = await nextPage.GetDocumentAsync();
        AssertEx.HtmlDocumentHasFlashNotificationBanner(
            nextPageDoc,
            "Email sent",
            string.Format(customFlashMessage, $"{supportTaskData.StatedFirstName} {supportTaskData.StatedLastName}"));
    }

    [Fact]
    public async Task Post_IdVerificationSupportTaskWithoutAppContent_UsesDefaultTemplateAndSendsEmail()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);
        var supportTaskData = supportTask.GetData<OneLoginUserIdVerificationData>();

        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask.SupportTaskReference,
            state => state.Verified = true,
            matchedPersons: []);

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/no-matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var nextPage = await response.FollowRedirectAsync(HttpClient);
        var nextPageDoc = await nextPage.GetDocumentAsync();
        AssertEx.HtmlDocumentHasFlashNotificationBanner(
            nextPageDoc,
            "Email sent",
            $"Request closed for {supportTaskData.StatedFirstName} {supportTaskData.StatedLastName}. We’ve sent them an email confirming we could not find a teaching record matching their GOV.UK One Login.");
    }

    [Fact]
    public async Task Post_RecordMatchingSupportTaskWithoutAppContent_DoesNotSendEmailAndUsesRequestClosedFlashMessage()
    {
        // Arrange
        // The default application user has a Deferred record matching policy and no app content, so no
        // "cannot find record" email is sent for a record matching task.
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);
        var firstName = TestData.GenerateFirstName();
        var lastName = TestData.GenerateLastName();
        var supportTask = await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(
            oneLoginUser.Subject, t => t
                .WithVerifiedNames([firstName, lastName]));

        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask.SupportTaskReference,
            state => state.Verified = true,
            matchedPersons: []);

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/no-matches?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var nextPage = await response.FollowRedirectAsync(HttpClient);
        var nextPageDoc = await nextPage.GetDocumentAsync();
        AssertEx.HtmlDocumentHasFlashNotificationBanner(
            nextPageDoc,
            "Request closed",
            $"Request closed for {firstName} {lastName}.");
    }
}
