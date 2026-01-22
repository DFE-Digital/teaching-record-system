using System.Diagnostics;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;
using static TeachingRecordSystem.AuthorizeAccess.SignInJourneyCoordinator.Vtrs;

namespace TeachingRecordSystem.AuthorizeAccess.Tests.PageTests;

public class CheckAnswersTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public Task Get_ValidRequestForVerifiedUser_RendersExpectedContent() =>
        WithJourneyCoordinatorAsync(
            CreateNewState,
            async coordinator =>
            {
                // Arrange
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);

                var nationalInsuranceNumber = TestData.GenerateNationalInsuranceNumber();
                var trn = await TestData.GenerateTrnAsync();

                await SetupInstanceStateForVerifiedUserAsync(coordinator, oneLoginUser, nationalInsuranceNumber, trn);

                var request = new HttpRequestMessage(HttpMethod.Get, JourneyUrls.CheckAnswers(coordinator.InstanceId));

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                var doc = await AssertEx.HtmlResponseAsync(response);
                Assert.Equal(nationalInsuranceNumber, doc.GetSummaryListValueByKey("National Insurance number"));
                Assert.Equal(trn, doc.GetSummaryListValueByKey("Teacher reference number"));
            });

    [Fact]
    public Task Get_ValidRequestForUnverifiedUser_RendersExpectedContent() =>
        WithJourneyCoordinatorAsync(
            CreateNewState,
            async coordinator =>
            {
                // Arrange
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);

                var firstName = TestData.GenerateFirstName();
                var lastName = TestData.GenerateLastName();
                var dateOfBirth = TestData.GenerateDateOfBirth();
                var nationalInsuranceNumber = TestData.GenerateNationalInsuranceNumber();
                var trn = await TestData.GenerateTrnAsync();

                await SetupInstanceStateForUnverifiedUserAsync(
                    coordinator,
                    oneLoginUser,
                    firstName,
                    lastName,
                    dateOfBirth,
                    nationalInsuranceNumber,
                    trn);

                var request = new HttpRequestMessage(HttpMethod.Get, JourneyUrls.CheckAnswers(coordinator.InstanceId));

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                var doc = await AssertEx.HtmlResponseAsync(response);
                Assert.Equal($"{firstName} {lastName}", doc.GetSummaryListValueByKey("Name"));
                Assert.Equal(dateOfBirth.ToString(WebConstants.DateOnlyDisplayFormat), doc.GetSummaryListValueByKey("Date of birth"));
                Assert.Equal(nationalInsuranceNumber, doc.GetSummaryListValueByKey("National Insurance number"));
                Assert.Equal(trn, doc.GetSummaryListValueByKey("Teacher reference number"));
            });

    [Fact]
    public async Task Post_ValidRequestForVerifiedUser_CreatesSupportTicketAndRedirectsToSupportRequestedSubmitted()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var trnToken = await CreateTrnTokenAsync(person.Trn);
        var applicationUser = await TestData.CreateApplicationUserAsync(isOidcClient: true);

        await WithJourneyCoordinatorAsync(
            id => CreateNewState(id, trnToken: trnToken, clientApplicationUserId: applicationUser.UserId),
            async coordinator =>
            {
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);

                var nationalInsuranceNumber = TestData.GenerateNationalInsuranceNumber();
                var trn = await TestData.GenerateTrnAsync();

                await SetupInstanceStateForVerifiedUserAsync(coordinator, oneLoginUser, nationalInsuranceNumber, trn);

                LegacyEventPublisher.Clear();

                var request = new HttpRequestMessage(HttpMethod.Post, JourneyUrls.CheckAnswers(coordinator.InstanceId));

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
                Assert.Equal(JourneyUrls.RequestSubmitted(coordinator.InstanceId), response.Headers.Location?.OriginalString);

                Assert.Collection(
                    coordinator.Path.Steps,
                    s => Assert.Equal(StepUrls.RequestSubmitted, s.NormalizedUrl));

                var supportTask = await WithDbContextAsync(dbContext => dbContext.SupportTasks.SingleAsync(t => t.OneLoginUserSubject == oneLoginUser.Subject));
                Assert.NotNull(supportTask);
                Assert.Equal(Clock.UtcNow, supportTask.CreatedOn);
                Assert.Equal(Clock.UtcNow, supportTask.UpdatedOn);
                Assert.Equal(SupportTaskType.OneLoginUserRecordMatching, supportTask.SupportTaskType);
                Assert.Equal(SupportTaskStatus.Open, supportTask.Status);
                Assert.Equal(oneLoginUser.Subject, supportTask.OneLoginUserSubject);
                var data = Assert.IsType<OneLoginUserRecordMatchingData>(supportTask.Data);
                Assert.True(data.Verified);
                Assert.Equal(oneLoginUser.Subject, data.OneLoginUserSubject);
                Assert.Equal(oneLoginUser.EmailAddress, data.OneLoginUserEmail);
                Assert.Equal(oneLoginUser.VerifiedNames, data.VerifiedNames);
                Assert.Equal(oneLoginUser.VerifiedDatesOfBirth, data.VerifiedDatesOfBirth);
                Assert.Equal(nationalInsuranceNumber, data.StatedNationalInsuranceNumber);
                Assert.Equal(trn, data.StatedTrn);
                Assert.Equal(trnToken.Trn, data.TrnTokenTrn);
                Assert.Equal(applicationUser.UserId, data.ClientApplicationUserId);

                LegacyEventPublisher.AssertEventsSaved(e =>
                {
                    var supportTaskCreatedEvent = Assert.IsType<LegacyEvents.SupportTaskCreatedEvent>(e);
                    Assert.Equal(Clock.UtcNow, supportTaskCreatedEvent.CreatedUtc);
                    Assert.Equal(supportTaskCreatedEvent.RaisedBy.UserId, SystemUser.SystemUserId);
                    Assert.Equal(supportTask.SupportTaskReference, supportTaskCreatedEvent.SupportTask.SupportTaskReference);
                    Assert.Equal(SupportTaskType.OneLoginUserRecordMatching, supportTaskCreatedEvent.SupportTask.SupportTaskType);
                    Assert.Equal(SupportTaskStatus.Open, supportTaskCreatedEvent.SupportTask.Status);
                    Assert.Equal(oneLoginUser.Subject, supportTaskCreatedEvent.SupportTask.OneLoginUserSubject);
                    var eventData = Assert.IsType<OneLoginUserRecordMatchingData>(supportTask.Data);
                    Assert.True(eventData.Verified);
                    Assert.Equal(oneLoginUser.Subject, eventData.OneLoginUserSubject);
                    Assert.Equal(oneLoginUser.EmailAddress, eventData.OneLoginUserEmail);
                    Assert.Equal(oneLoginUser.VerifiedNames, eventData.VerifiedNames);
                    Assert.Equal(oneLoginUser.VerifiedDatesOfBirth, eventData.VerifiedDatesOfBirth);
                    Assert.Equal(nationalInsuranceNumber, eventData.StatedNationalInsuranceNumber);
                    Assert.Equal(trn, eventData.StatedTrn);
                    Assert.Equal(trnToken.Trn, eventData.TrnTokenTrn);
                    Assert.Equal(applicationUser.UserId, eventData.ClientApplicationUserId);
                });

                Events.AssertProcessesCreated(p =>
                {
                    Assert.Equal(ProcessType.ConnectOneLoginUserSupportTaskCreating, p.ProcessContext.ProcessType);
                    p.AssertProcessHasEvent<SupportTaskCreatedEvent>();
                });
            });
    }

    [Fact]
    public async Task Post_ValidRequestForUnverifiedUser_CreatesSupportTicketAndRedirectsToSupportRequestedSubmitted()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var trnToken = await CreateTrnTokenAsync(person.Trn);
        var applicationUser = await TestData.CreateApplicationUserAsync(isOidcClient: true);

        await WithJourneyCoordinatorAsync(
            id => CreateNewState(id, trnToken: trnToken, clientApplicationUserId: applicationUser.UserId),
            async coordinator =>
            {
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);

                var firstName = TestData.GenerateFirstName();
                var lastName = TestData.GenerateLastName();
                var dateOfBirth = TestData.GenerateDateOfBirth();
                var nationalInsuranceNumber = TestData.GenerateNationalInsuranceNumber();
                var trn = await TestData.GenerateTrnAsync();

                await SetupInstanceStateForUnverifiedUserAsync(
                    coordinator,
                    oneLoginUser,
                    firstName,
                    lastName,
                    dateOfBirth,
                    nationalInsuranceNumber,
                    trn);

                LegacyEventPublisher.Clear();

                var request = new HttpRequestMessage(HttpMethod.Post, JourneyUrls.CheckAnswers(coordinator.InstanceId));

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
                Assert.Equal(JourneyUrls.RequestSubmitted(coordinator.InstanceId), response.Headers.Location?.OriginalString);

                Assert.Collection(
                    coordinator.Path.Steps,
                    s => Assert.Equal(StepUrls.RequestSubmitted, s.NormalizedUrl));

                var supportTask = await WithDbContextAsync(dbContext => dbContext.SupportTasks.SingleAsync(t => t.OneLoginUserSubject == oneLoginUser.Subject));
                Assert.NotNull(supportTask);
                Assert.Equal(Clock.UtcNow, supportTask.CreatedOn);
                Assert.Equal(Clock.UtcNow, supportTask.UpdatedOn);
                Assert.Equal(SupportTaskType.OneLoginUserIdVerification, supportTask.SupportTaskType);
                Assert.Equal(SupportTaskStatus.Open, supportTask.Status);
                Assert.Equal(oneLoginUser.Subject, supportTask.OneLoginUserSubject);
                var data = Assert.IsType<OneLoginUserIdVerificationData>(supportTask.Data);
                Assert.Null(data.Verified);
                Assert.Equal(oneLoginUser.Subject, data.OneLoginUserSubject);
                Assert.Equal(firstName, data.StatedFirstName);
                Assert.Equal(lastName, data.StatedLastName);
                Assert.Equal(dateOfBirth, data.StatedDateOfBirth);
                Assert.Equal(nationalInsuranceNumber, data.StatedNationalInsuranceNumber);
                Assert.Equal(trn, data.StatedTrn);
                Assert.Equal(trnToken.Trn, data.TrnTokenTrn);
                Assert.Equal(applicationUser.UserId, data.ClientApplicationUserId);

                LegacyEventPublisher.AssertEventsSaved(e =>
                {
                    var supportTaskCreatedEvent = Assert.IsType<LegacyEvents.SupportTaskCreatedEvent>(e);
                    Assert.Equal(Clock.UtcNow, supportTaskCreatedEvent.CreatedUtc);
                    Assert.Equal(supportTaskCreatedEvent.RaisedBy.UserId, SystemUser.SystemUserId);
                    Assert.Equal(supportTask.SupportTaskReference, supportTaskCreatedEvent.SupportTask.SupportTaskReference);
                    Assert.Equal(SupportTaskType.OneLoginUserIdVerification, supportTaskCreatedEvent.SupportTask.SupportTaskType);
                    Assert.Equal(SupportTaskStatus.Open, supportTaskCreatedEvent.SupportTask.Status);
                    Assert.Equal(oneLoginUser.Subject, supportTaskCreatedEvent.SupportTask.OneLoginUserSubject);
                    var eventData = Assert.IsType<OneLoginUserIdVerificationData>(supportTask.Data);
                    Assert.Null(eventData.Verified);
                    Assert.Equal(oneLoginUser.Subject, eventData.OneLoginUserSubject);
                    Assert.Equal(firstName, eventData.StatedFirstName);
                    Assert.Equal(lastName, eventData.StatedLastName);
                    Assert.Equal(dateOfBirth, eventData.StatedDateOfBirth);
                    Assert.Equal(nationalInsuranceNumber, eventData.StatedNationalInsuranceNumber);
                    Assert.Equal(trn, eventData.StatedTrn);
                    Assert.Equal(trnToken.Trn, eventData.TrnTokenTrn);
                    Assert.Equal(applicationUser.UserId, eventData.ClientApplicationUserId);
                });

                Events.AssertProcessesCreated(p =>
                {
                    Assert.Equal(ProcessType.OneLoginUserIdVerificationSupportTaskCreating, p.ProcessContext.ProcessType);
                    p.AssertProcessHasEvent<SupportTaskCreatedEvent>();
                });
            });
    }

    private async Task SetupInstanceStateForVerifiedUserAsync(
        SignInJourneyCoordinator coordinator,
        OneLoginUser oneLoginUser,
        string? nationalInsuranceNumber,
        string? trn)
    {
        var ticket = CreateOneLoginAuthenticationTicket(vtr: AuthenticationOnly, oneLoginUser);
        await coordinator.OnOneLoginCallbackAsync(ticket);
        Debug.Assert(coordinator.State.IdentityVerified);
        AddUrlToPath(coordinator, StepUrls.NationalInsuranceNumber);
        coordinator.UpdateState(s => s.SetNationalInsuranceNumber(true, nationalInsuranceNumber));
        AddUrlToPath(coordinator, StepUrls.Trn);
        coordinator.UpdateState(s => s.SetTrn(true, trn));
        AddUrlToPath(coordinator, StepUrls.NotFound);
        AddUrlToPath(coordinator, StepUrls.CheckAnswers);
    }

    private async Task SetupInstanceStateForUnverifiedUserAsync(
        SignInJourneyCoordinator coordinator,
        OneLoginUser oneLoginUser,
        string? firstName,
        string? lastName,
        DateOnly? dateOfBirth,
        string? nationalInsuranceNumber,
        string? trn)
    {
        var ticket = CreateOneLoginAuthenticationTicket(vtr: AuthenticationOnly, oneLoginUser);
        await coordinator.OnOneLoginCallbackAsync(ticket);
        Debug.Assert(!coordinator.State.IdentityVerified);
        AddUrlToPath(coordinator, StepUrls.Name);
        coordinator.UpdateState(s => s.SetName(firstName ?? TestData.GenerateFirstName(), lastName ?? TestData.GenerateLastName()));
        AddUrlToPath(coordinator, StepUrls.DateOfBirth);
        coordinator.UpdateState(s => s.SetDateOfBirth(dateOfBirth ?? TestData.GenerateDateOfBirth()));
        AddUrlToPath(coordinator, StepUrls.NationalInsuranceNumber);
        coordinator.UpdateState(s => s.SetNationalInsuranceNumber(true, nationalInsuranceNumber));
        AddUrlToPath(coordinator, StepUrls.Trn);
        coordinator.UpdateState(s => s.SetTrn(true, trn));
        AddUrlToPath(coordinator, StepUrls.NotFound);
        AddUrlToPath(coordinator, StepUrls.CheckAnswers);
    }
}
