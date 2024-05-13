using System.Diagnostics;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Models.SupportTaskData;

namespace TeachingRecordSystem.AuthorizeAccess.Tests.PageTests;

public class CheckAnswersTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_NotAuthenticatedWithOneLogin_ReturnsBadRequest()
    {
        // Arrange
        var state = CreateNewState();
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_NotVerifiedWithOneLogin_ReturnsBadRequest()
    {
        // Arrange
        var state = CreateNewState();
        var journeyInstance = await CreateJourneyInstance(state);

        var ticket = CreateOneLoginAuthenticationTicket(vtr: SignInJourneyHelper.AuthenticationOnlyVtr, createCoreIdentityVc: false);
        await GetSignInJourneyHelper().OnOneLoginCallback(journeyInstance, ticket);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_NationalInsuranceNumberNotSpecified_RedirectsToNationalInsuranceNumberPage()
    {
        // Arrange
        var state = CreateNewState();
        var journeyInstance = await CreateJourneyInstance(state);

        var oneLoginUser = await TestData.CreateOneLoginUser(verified: true);

        var ticket = CreateOneLoginAuthenticationTicket(vtr: SignInJourneyHelper.AuthenticationOnlyVtr, oneLoginUser);
        await GetSignInJourneyHelper().OnOneLoginCallback(journeyInstance, ticket);

        Debug.Assert(state.NationalInsuranceNumber is null);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/national-insurance-number?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_TrnNotSpecified_RedirectsToTrnPage()
    {
        // Arrange
        var state = CreateNewState();
        var journeyInstance = await CreateJourneyInstance(state);

        var oneLoginUser = await TestData.CreateOneLoginUser(verified: true);

        var ticket = CreateOneLoginAuthenticationTicket(vtr: SignInJourneyHelper.AuthenticationOnlyVtr, oneLoginUser);
        await GetSignInJourneyHelper().OnOneLoginCallback(journeyInstance, ticket);

        Debug.Assert(state.NationalInsuranceNumber is null);
        await journeyInstance.UpdateStateAsync(state => state.SetNationalInsuranceNumber(true, TestData.GenerateNationalInsuranceNumber()));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/trn?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_AlreadyAuthenticated_RedirectsToStateRedirectUri()
    {
        // Arrange
        var state = CreateNewState();
        var journeyInstance = await CreateJourneyInstance(state);

        var person = await TestData.CreatePerson(b => b.WithTrn());
        var oneLoginUser = await TestData.CreateOneLoginUser(person);

        var ticket = CreateOneLoginAuthenticationTicket(vtr: SignInJourneyHelper.AuthenticationOnlyVtr, oneLoginUser);
        await GetSignInJourneyHelper().OnOneLoginCallback(journeyInstance, ticket);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"{state.RedirectUri}?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_ValidRequest_RendersExpectedContent()
    {
        // Arrange
        var state = CreateNewState();
        var journeyInstance = await CreateJourneyInstance(state);

        var oneLoginUser = await TestData.CreateOneLoginUser(verified: true);

        var ticket = CreateOneLoginAuthenticationTicket(vtr: SignInJourneyHelper.AuthenticationOnlyVtr, oneLoginUser);
        await GetSignInJourneyHelper().OnOneLoginCallback(journeyInstance, ticket);

        var nationalInsuranceNumber = TestData.GenerateNationalInsuranceNumber();
        var trn = await TestData.GenerateTrn();

        await journeyInstance.UpdateStateAsync(state =>
        {
            state.SetNationalInsuranceNumber(true, nationalInsuranceNumber);
            state.SetTrn(true, trn);
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);
        Assert.Equal(nationalInsuranceNumber, doc.GetSummaryListValueForKey("National Insurance number"));
        Assert.Equal(trn, doc.GetSummaryListValueForKey("Teacher reference number"));
    }

    [Fact]
    public async Task Post_NotAuthenticatedWithOneLogin_ReturnsBadRequest()
    {
        // Arrange
        var state = CreateNewState();
        var journeyInstance = await CreateJourneyInstance(state);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_NotVerifiedWithOneLogin_ReturnsBadRequest()
    {
        // Arrange
        var state = CreateNewState();
        var journeyInstance = await CreateJourneyInstance(state);

        var ticket = CreateOneLoginAuthenticationTicket(vtr: SignInJourneyHelper.AuthenticationOnlyVtr, createCoreIdentityVc: false);
        await GetSignInJourneyHelper().OnOneLoginCallback(journeyInstance, ticket);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_NationalInsuranceNumberNotSpecified_RedirectsToNationalInsuranceNumberPage()
    {
        // Arrange
        var state = CreateNewState();
        var journeyInstance = await CreateJourneyInstance(state);

        var oneLoginUser = await TestData.CreateOneLoginUser(verified: true);

        var ticket = CreateOneLoginAuthenticationTicket(vtr: SignInJourneyHelper.AuthenticationOnlyVtr, oneLoginUser);
        await GetSignInJourneyHelper().OnOneLoginCallback(journeyInstance, ticket);

        Debug.Assert(state.NationalInsuranceNumber is null);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/national-insurance-number?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_TrnNotSpecified_RedirectsToTrnPage()
    {
        // Arrange
        var state = CreateNewState();
        var journeyInstance = await CreateJourneyInstance(state);

        var oneLoginUser = await TestData.CreateOneLoginUser(verified: true);

        var ticket = CreateOneLoginAuthenticationTicket(vtr: SignInJourneyHelper.AuthenticationOnlyVtr, oneLoginUser);
        await GetSignInJourneyHelper().OnOneLoginCallback(journeyInstance, ticket);

        Debug.Assert(state.NationalInsuranceNumber is null);
        await journeyInstance.UpdateStateAsync(state => state.SetNationalInsuranceNumber(true, TestData.GenerateNationalInsuranceNumber()));

        var request = new HttpRequestMessage(HttpMethod.Post, $"/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/trn?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_AlreadyAuthenticated_RedirectsToStateRedirectUri()
    {
        // Arrange
        var state = CreateNewState();
        var journeyInstance = await CreateJourneyInstance(state);

        var person = await TestData.CreatePerson(b => b.WithTrn());
        var oneLoginUser = await TestData.CreateOneLoginUser(person);

        var ticket = CreateOneLoginAuthenticationTicket(vtr: SignInJourneyHelper.AuthenticationOnlyVtr, oneLoginUser);
        await GetSignInJourneyHelper().OnOneLoginCallback(journeyInstance, ticket);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"{state.RedirectUri}?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_ValidRequest_CreatesSupportTicketAndRedirectsToSupportRequestedSubmitted()
    {
        // Arrange
        var person = await TestData.CreatePerson(b => b.WithTrn());
        var trnToken = await CreateTrnToken(person.Trn!);
        var applicationUser = await TestData.CreateApplicationUser(isOidcClient: true);
        var state = CreateNewState(clientApplicationUserId: applicationUser.UserId, trnToken: trnToken);
        var journeyInstance = await CreateJourneyInstance(state);

        var oneLoginUser = await TestData.CreateOneLoginUser(verified: true);

        var ticket = CreateOneLoginAuthenticationTicket(vtr: SignInJourneyHelper.AuthenticationOnlyVtr, oneLoginUser);
        await GetSignInJourneyHelper().OnUserAuthenticated(journeyInstance, ticket);

        var nationalInsuranceNumber = TestData.GenerateNationalInsuranceNumber();
        var trn = await TestData.GenerateTrn();

        await journeyInstance.UpdateStateAsync(state =>
        {
            state.SetNationalInsuranceNumber(true, nationalInsuranceNumber);
            state.SetTrn(true, trn);
        });

        EventObserver.Clear();

        var request = new HttpRequestMessage(HttpMethod.Post, $"/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/request-submitted?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);

        var supportTask = await WithDbContext(dbContext => dbContext.SupportTasks.SingleAsync(t => t.OneLoginUserSubject == oneLoginUser.Subject));
        Assert.NotNull(supportTask);
        Assert.Equal(Clock.UtcNow, supportTask.CreatedOn);
        Assert.Equal(Clock.UtcNow, supportTask.UpdatedOn);
        Assert.Equal(SupportTaskType.ConnectOneLoginUser, supportTask.SupportTaskType);
        Assert.Equal(SupportTaskStatus.Open, supportTask.Status);
        Assert.Equal(oneLoginUser.Subject, supportTask.OneLoginUserSubject);
        var data = Assert.IsType<ConnectOneLoginUserData>(supportTask.Data);
        Assert.True(data.Verified);
        Assert.Equal(oneLoginUser.Subject, data.OneLoginUserSubject);
        Assert.Equal(oneLoginUser.Email, data.OneLoginUserEmail);
        Assert.Equal(oneLoginUser.VerifiedNames, data.VerifiedNames);
        Assert.Equal(oneLoginUser.VerifiedDatesOfBirth, data.VerifiedDatesOfBirth);
        Assert.Equal(nationalInsuranceNumber, data.StatedNationalInsuranceNumber);
        Assert.Equal(trn, data.StatedTrn);
        Assert.Equal(trnToken.Trn, data.TrnTokenTrn);
        Assert.Equal(applicationUser.UserId, data.ClientApplicationUserId);

        EventObserver.AssertEventsSaved(e =>
        {
            var supportTaskCreatedEvent = Assert.IsType<SupportTaskCreatedEvent>(e);
            Assert.Equal(Clock.UtcNow, supportTaskCreatedEvent.CreatedUtc);
            Assert.Equal(supportTaskCreatedEvent.RaisedBy.UserId, SystemUser.SystemUserId);
            Assert.Equal(supportTask.SupportTaskReference, supportTaskCreatedEvent.SupportTask.SupportTaskReference);
            Assert.Equal(SupportTaskType.ConnectOneLoginUser, supportTaskCreatedEvent.SupportTask.SupportTaskType);
            Assert.Equal(SupportTaskStatus.Open, supportTaskCreatedEvent.SupportTask.Status);
            Assert.Equal(oneLoginUser.Subject, supportTaskCreatedEvent.SupportTask.OneLoginUserSubject);
            var eventData = Assert.IsType<ConnectOneLoginUserData>(supportTask.Data);
            Assert.True(eventData.Verified);
            Assert.Equal(oneLoginUser.Subject, eventData.OneLoginUserSubject);
            Assert.Equal(oneLoginUser.Email, eventData.OneLoginUserEmail);
            Assert.Equal(oneLoginUser.VerifiedNames, eventData.VerifiedNames);
            Assert.Equal(oneLoginUser.VerifiedDatesOfBirth, eventData.VerifiedDatesOfBirth);
            Assert.Equal(nationalInsuranceNumber, eventData.StatedNationalInsuranceNumber);
            Assert.Equal(trn, eventData.StatedTrn);
            Assert.Equal(trnToken.Trn, eventData.TrnTokenTrn);
            Assert.Equal(applicationUser.UserId, eventData.ClientApplicationUserId);
        });
    }
}
