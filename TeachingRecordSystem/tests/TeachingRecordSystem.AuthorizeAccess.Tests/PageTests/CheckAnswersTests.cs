using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTasks;
using static TeachingRecordSystem.AuthorizeAccess.SignInJourneyCoordinator.Vtrs;

namespace TeachingRecordSystem.AuthorizeAccess.Tests.PageTests;

public class CheckAnswersTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public Task Get_NotAuthenticatedWithOneLogin_ReturnsBadRequest() =>
        WithJourneyInstanceAsync(
            CreateNewState(),
            async journeyInstance =>
            {
                // Arrange
                var request = new HttpRequestMessage(HttpMethod.Get, JourneyUrls.CheckAnswers(journeyInstance.InstanceId));

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
            });

    [Fact]
    public Task Get_NotVerifiedWithOneLogin_ReturnsBadRequest() =>
        WithJourneyInstanceAsync(
            CreateNewState(),
            async journeyInstance =>
            {
                // Arrange
                var ticket = CreateOneLoginAuthenticationTicket(vtr: AuthenticationOnly, createCoreIdentityVc: false);
                await journeyInstance.OnOneLoginCallbackAsync(ticket);

                var request = new HttpRequestMessage(HttpMethod.Get, JourneyUrls.CheckAnswers(journeyInstance.InstanceId));

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
            });

    [Fact]
    public Task Get_NationalInsuranceNumberNotSpecified_RedirectsToNationalInsuranceNumberPage() =>
        WithJourneyInstanceAsync(
            CreateNewState(),
            async journeyInstance =>
            {
                // Arrange
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);

                var ticket = CreateOneLoginAuthenticationTicket(vtr: AuthenticationOnly, oneLoginUser);
                await journeyInstance.OnOneLoginCallbackAsync(ticket);

                var request = new HttpRequestMessage(HttpMethod.Get, JourneyUrls.CheckAnswers(journeyInstance.InstanceId));

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
                Assert.Equal(JourneyUrls.NationalInsuranceNumber(journeyInstance.InstanceId), response.Headers.Location?.OriginalString);
            });

    [Fact]
    public Task Get_TrnNotSpecified_RedirectsToTrnPage() =>
        WithJourneyInstanceAsync(
            CreateNewState(),
            async journeyInstance =>
            {
                // Arrange
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);

                var ticket = CreateOneLoginAuthenticationTicket(vtr: AuthenticationOnly, oneLoginUser);
                await journeyInstance.OnOneLoginCallbackAsync(ticket);

                journeyInstance.UpdateState(state => state.SetNationalInsuranceNumber(true, Faker.Identification.UkNationalInsuranceNumber()));

                var request = new HttpRequestMessage(HttpMethod.Get, JourneyUrls.CheckAnswers(journeyInstance.InstanceId));

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
                Assert.Equal(JourneyUrls.Trn(journeyInstance.InstanceId), response.Headers.Location?.OriginalString);
            });

    [Fact]
    public Task Get_AlreadyAuthenticated_RedirectsToStateRedirectUri() =>
        WithJourneyInstanceAsync(
            CreateNewState(),
            async journeyInstance =>
            {
                // Arrange
                var person = await TestData.CreatePersonAsync();
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(person);

                var ticket = CreateOneLoginAuthenticationTicket(vtr: AuthenticationOnly, oneLoginUser);
                await journeyInstance.OnOneLoginCallbackAsync(ticket);

                var request = new HttpRequestMessage(HttpMethod.Get, JourneyUrls.CheckAnswers(journeyInstance.InstanceId));

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
                Assert.Equal(journeyInstance.State.RedirectUri, response.Headers.Location?.OriginalString);
            });

    [Fact]
    public Task Get_ValidRequest_RendersExpectedContent() =>
        WithJourneyInstanceAsync(
            CreateNewState(),
            async journeyInstance =>
            {
                // Arrange
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);

                await SetupInstanceStateAsync(journeyInstance, oneLoginUser);

                var request = new HttpRequestMessage(HttpMethod.Get, JourneyUrls.CheckAnswers(journeyInstance.InstanceId));

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                var doc = await AssertEx.HtmlResponseAsync(response);
                var nationalInsuranceNumber = journeyInstance.State.NationalInsuranceNumber!;
                var trn = journeyInstance.State.Trn!;
                Assert.Equal(nationalInsuranceNumber, doc.GetSummaryListValueByKey("National Insurance number"));
                Assert.Equal(trn, doc.GetSummaryListValueByKey("Teacher reference number"));
            });

    [Fact]
    public Task Post_NotAuthenticatedWithOneLogin_ReturnsBadRequest() =>
        WithJourneyInstanceAsync(
            CreateNewState(),
            async journeyInstance =>
            {
                // Arrange
                var request = new HttpRequestMessage(HttpMethod.Post, JourneyUrls.CheckAnswers(journeyInstance.InstanceId));

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
            });

    [Fact]
    public Task Post_NotVerifiedWithOneLogin_ReturnsBadRequest() =>
        WithJourneyInstanceAsync(
            CreateNewState(),
            async journeyInstance =>
            {
                // Arrange
                var ticket = CreateOneLoginAuthenticationTicket(vtr: AuthenticationOnly, createCoreIdentityVc: false);
                await journeyInstance.OnOneLoginCallbackAsync(ticket);

                var request = new HttpRequestMessage(HttpMethod.Post, JourneyUrls.CheckAnswers(journeyInstance.InstanceId));

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
            });

    [Fact]
    public Task Post_NationalInsuranceNumberNotSpecified_RedirectsToNationalInsuranceNumberPage() =>
        WithJourneyInstanceAsync(
            CreateNewState(),
            async journeyInstance =>
            {
                // Arrange
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);

                var ticket = CreateOneLoginAuthenticationTicket(vtr: AuthenticationOnly, oneLoginUser);
                await journeyInstance.OnOneLoginCallbackAsync(ticket);

                var request = new HttpRequestMessage(HttpMethod.Post, JourneyUrls.CheckAnswers(journeyInstance.InstanceId));

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
                Assert.Equal(JourneyUrls.NationalInsuranceNumber(journeyInstance.InstanceId), response.Headers.Location?.OriginalString);
            });

    [Fact]
    public Task Post_TrnNotSpecified_RedirectsToTrnPage() =>
        WithJourneyInstanceAsync(
            CreateNewState(),
            async journeyInstance =>
            {
                // Arrange
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);

                var ticket = CreateOneLoginAuthenticationTicket(vtr: AuthenticationOnly, oneLoginUser);
                await journeyInstance.OnOneLoginCallbackAsync(ticket);

                journeyInstance.UpdateState(state => state.SetNationalInsuranceNumber(true, Faker.Identification.UkNationalInsuranceNumber()));

                var request = new HttpRequestMessage(HttpMethod.Post, JourneyUrls.CheckAnswers(journeyInstance.InstanceId));

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
                Assert.Equal(JourneyUrls.Trn(journeyInstance.InstanceId), response.Headers.Location?.OriginalString);
            });

    [Fact]
    public Task Post_AlreadyAuthenticated_RedirectsToStateRedirectUri() =>
        WithJourneyInstanceAsync(
            CreateNewState(),
            async journeyInstance =>
            {
                // Arrange
                var person = await TestData.CreatePersonAsync();
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(person);

                var ticket = CreateOneLoginAuthenticationTicket(vtr: AuthenticationOnly, oneLoginUser);
                await journeyInstance.OnOneLoginCallbackAsync(ticket);

                var request = new HttpRequestMessage(HttpMethod.Post, JourneyUrls.CheckAnswers(journeyInstance.InstanceId));

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
                Assert.Equal(journeyInstance.State.RedirectUri, response.Headers.Location?.OriginalString);
            });

    [Fact]
    public Task Post_ValidRequest_CreatesSupportTicketAndRedirectsToSupportRequestedSubmitted() =>
        WithJourneyInstanceAsync(
            CreateNewState(),
            async journeyInstance =>
            {
                // Arrange
                var person = await TestData.CreatePersonAsync();
                var trnToken = await CreateTrnTokenAsync(person.Trn);
                var applicationUser = await TestData.CreateApplicationUserAsync(isOidcClient: true);
                
                // Create a new state with the trnToken and applicationUser
                var stateWithToken = CreateNewState(trnToken, clientApplicationUserId: applicationUser.UserId);
                
                // Create a new journey instance with the token
                await WithJourneyInstanceAsync(
                    stateWithToken,
                    async innerJourneyInstance =>
                    {
                        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);

                        await SetupInstanceStateAsync(innerJourneyInstance, oneLoginUser);

                        LegacyEventPublisher.Clear();

                        var request = new HttpRequestMessage(HttpMethod.Post, JourneyUrls.CheckAnswers(innerJourneyInstance.InstanceId));

                        // Act
                        var response = await HttpClient.SendAsync(request);

                        // Assert
                        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
                        Assert.Equal(JourneyUrls.SupportRequestSubmitted(innerJourneyInstance.InstanceId), response.Headers.Location?.OriginalString);

                        var supportTask = await WithDbContextAsync(dbContext => dbContext.SupportTasks.SingleAsync(t => t.OneLoginUserSubject == oneLoginUser.Subject));
                        Assert.NotNull(supportTask);
                        Assert.Equal(Clock.UtcNow, supportTask.CreatedOn);
                        Assert.Equal(Clock.UtcNow, supportTask.UpdatedOn);
                        Assert.Equal(SupportTaskType.ConnectOneLoginUser, supportTask.SupportTaskType);
                        Assert.Equal(SupportTaskStatus.Open, supportTask.Status);
                        Assert.Equal(oneLoginUser.Subject, supportTask.OneLoginUserSubject);
                        var data = Assert.IsType<ConnectOneLoginUserData>(supportTask.Data);
                        Assert.True(data.Verified);
                        Assert.Equal(oneLoginUser.Subject, data.OneLoginUserSubject);
                        Assert.Equal(oneLoginUser.EmailAddress, data.OneLoginUserEmail);
                        Assert.Equal(oneLoginUser.VerifiedNames, data.VerifiedNames);
                        Assert.Equal(oneLoginUser.VerifiedDatesOfBirth, data.VerifiedDatesOfBirth);
                        Assert.Equal(innerJourneyInstance.State.NationalInsuranceNumber, data.StatedNationalInsuranceNumber);
                        Assert.Equal(innerJourneyInstance.State.Trn, data.StatedTrn);
                        Assert.Equal(trnToken.Trn, data.TrnTokenTrn);
                        Assert.Equal(applicationUser.UserId, data.ClientApplicationUserId);

                        LegacyEventPublisher.AssertEventsSaved(e =>
                        {
                            var supportTaskCreatedEvent = Assert.IsType<LegacyEvents.SupportTaskCreatedEvent>(e);
                            Assert.Equal(Clock.UtcNow, supportTaskCreatedEvent.CreatedUtc);
                            Assert.Equal(supportTaskCreatedEvent.RaisedBy.UserId, SystemUser.SystemUserId);
                            Assert.Equal(supportTask.SupportTaskReference, supportTaskCreatedEvent.SupportTask.SupportTaskReference);
                            Assert.Equal(SupportTaskType.ConnectOneLoginUser, supportTaskCreatedEvent.SupportTask.SupportTaskType);
                            Assert.Equal(SupportTaskStatus.Open, supportTaskCreatedEvent.SupportTask.Status);
                            Assert.Equal(oneLoginUser.Subject, supportTaskCreatedEvent.SupportTask.OneLoginUserSubject);
                            var eventData = Assert.IsType<ConnectOneLoginUserData>(supportTask.Data);
                            Assert.True(eventData.Verified);
                            Assert.Equal(oneLoginUser.Subject, eventData.OneLoginUserSubject);
                            Assert.Equal(oneLoginUser.EmailAddress, eventData.OneLoginUserEmail);
                            Assert.Equal(oneLoginUser.VerifiedNames, eventData.VerifiedNames);
                            Assert.Equal(oneLoginUser.VerifiedDatesOfBirth, eventData.VerifiedDatesOfBirth);
                            Assert.Equal(innerJourneyInstance.State.NationalInsuranceNumber, eventData.StatedNationalInsuranceNumber);
                            Assert.Equal(innerJourneyInstance.State.Trn, eventData.StatedTrn);
                            Assert.Equal(trnToken.Trn, eventData.TrnTokenTrn);
                            Assert.Equal(applicationUser.UserId, eventData.ClientApplicationUserId);
                        });

                        Events.AssertProcessesCreated(p =>
                        {
                            Assert.Equal(ProcessType.ConnectOneLoginUserSupportTaskCreating, p.ProcessContext.ProcessType);
                            p.AssertProcessHasEvent<SupportTaskCreatedEvent>();
                        });
                    });
            });

    private async Task SetupInstanceStateAsync(SignInJourneyCoordinator journeyInstance, OneLoginUser oneLoginUser)
    {
        var ticket = CreateOneLoginAuthenticationTicket(vtr: AuthenticationOnly, oneLoginUser);
        await journeyInstance.OnOneLoginCallbackAsync(ticket);
        var nationalInsuranceNumber = Faker.Identification.UkNationalInsuranceNumber();
        var trn = await TestData.GenerateTrnAsync();
        journeyInstance.UpdateState(state =>
        {
            state.SetNationalInsuranceNumber(true, nationalInsuranceNumber);
            state.SetTrn(true, trn);
        });
        AddUrlToPath(journeyInstance, StepUrls.CheckAnswers);
    }
}
