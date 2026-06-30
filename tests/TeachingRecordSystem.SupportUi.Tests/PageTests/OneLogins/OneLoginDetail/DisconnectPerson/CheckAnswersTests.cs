using TeachingRecordSystem.Core.Services.OneLogin;
using TeachingRecordSystem.SupportUi.Pages.OneLogins.OneLoginDetail.DisconnectPerson;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.OneLogins.OneLoginDetail.DisconnectPerson;

public class CheckAnswersTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Theory]
    [InlineData(null, null)]
    [InlineData(DisconnectPersonStayVerified.Yes, null)]
    [InlineData(null, DisconnectPersonReason.NewInformation)]
    public async Task Get_WithMissingOptions_ReturnsToIndex(DisconnectPersonStayVerified? stayVerified,
        DisconnectPersonReason? reason)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLogin = await TestData.CreateOneLoginUserAsync(person);
        var journeyInstance = await CreateJourneyInstanceAsync(
            oneLogin.Subject,
            new DisconnectPersonState() { DisconnectReason = reason, StayVerified = stayVerified });

        var request = new HttpRequestMessage(HttpMethod.Get,
            $"/one-logins/{oneLogin.Subject}/disconnect-person/{person.PersonId}/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Contains($"/one-logins/{oneLogin.Subject}/disconnect-person/{person.PersonId}",
            response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_RemovingVerification_RemovesPersonAndVerification()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLogin = await TestData.CreateOneLoginUserAsync(person);
        var journeyInstance = await CreateJourneyInstanceAsync(
            oneLogin.Subject,
            new DisconnectPersonState() { DisconnectReason = DisconnectPersonReason.NewInformation, StayVerified = DisconnectPersonStayVerified.No });
        var request = new HttpRequestMessage(HttpMethod.Post, $"/one-logins/{oneLogin.Subject}/disconnect-person/{person.PersonId}/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>())
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Contains($"/one-logins/{oneLogin.Subject}", response.Headers.Location?.OriginalString);

        await WithDbContextAsync(async dbContext =>
        {
            var oneLoginUpdated = await dbContext.OneLoginUsers.SingleAsync(x => x.Subject == oneLogin.Subject);
            Assert.NotNull(oneLoginUpdated);
            Assert.Null(oneLoginUpdated.PersonId);
            Assert.Null(oneLoginUpdated.VerifiedOn);
            Assert.Null(oneLoginUpdated.VerificationRoute);
            Assert.Null(oneLoginUpdated.VerifiedByApplicationUserId);
            Assert.Null(oneLoginUpdated.VerifiedNames);
            Assert.Null(oneLoginUpdated.VerifiedDatesOfBirth);
            Assert.Null(oneLoginUpdated.LastCoreIdentityVc);
            Assert.Null(oneLoginUpdated.MatchedOn);
            Assert.Null(oneLoginUpdated.MatchRoute);
            Assert.Null(oneLoginUpdated.MatchedAttributes);
        });
    }

    [Fact]
    public async Task Post_KeepingVerification_RemovesPersonAndKeepsVerification()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLogin = await TestData.CreateOneLoginUserAsync(person);
        var journeyInstance = await CreateJourneyInstanceAsync(
            oneLogin.Subject,
            new DisconnectPersonState() { DisconnectReason = DisconnectPersonReason.NewInformation, StayVerified = DisconnectPersonStayVerified.Yes });
        var request = new HttpRequestMessage(HttpMethod.Post, $"/one-logins/{oneLogin.Subject}/disconnect-person/{person.PersonId}/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>())
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Contains($"/one-logins/{oneLogin.Subject}", response.Headers.Location?.OriginalString);

        await WithDbContextAsync(async dbContext =>
        {
            var oneLoginUpdated = await dbContext.OneLoginUsers.SingleAsync(x => x.Subject == oneLogin.Subject);
            Assert.NotNull(oneLoginUpdated);
            Assert.Null(oneLoginUpdated.PersonId);
            Assert.Null(oneLoginUpdated.MatchedOn);
            Assert.Null(oneLoginUpdated.MatchRoute);
            Assert.Null(oneLoginUpdated.MatchedAttributes);
            Assert.NotNull(oneLoginUpdated.VerifiedNames);
            Assert.NotNull(oneLoginUpdated.VerifiedDatesOfBirth);
        });
    }

    [Fact]
    public async Task Post_CreatesProcessWithCorrectType()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLogin = await TestData.CreateOneLoginUserAsync(person);
        var journeyInstance = await CreateJourneyInstanceAsync(
            oneLogin.Subject,
            new DisconnectPersonState() { DisconnectReason = DisconnectPersonReason.NewInformation, StayVerified = DisconnectPersonStayVerified.Yes });
        var request = new HttpRequestMessage(HttpMethod.Post, $"/one-logins/{oneLogin.Subject}/disconnect-person/{person.PersonId}/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>())
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        Events.AssertProcessesCreated(p =>
        {
            Assert.Equal(ProcessType.OneLoginUserPersonDisconnecting, p.ProcessContext.ProcessType);
        });
    }

    [Theory]
    [InlineData(DisconnectPersonStayVerified.Yes, DisconnectPersonReason.NewInformation, null)]
    public async Task Get_RendersCorrectDetails(DisconnectPersonStayVerified? stayVerified,
        DisconnectPersonReason? reason, string? details)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLogin = await TestData.CreateOneLoginUserAsync(person);
        var journeyInstance = await CreateJourneyInstanceAsync(
            oneLogin.Subject,
            new DisconnectPersonState() { DisconnectReason = reason, StayVerified = stayVerified, Detail = details });

        var request = new HttpRequestMessage(HttpMethod.Get,
            $"/one-logins/{oneLogin.Subject}/disconnect-person/{person.PersonId}/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        doc.AssertSummaryListRowValue("Why are you disconnecting this record from GOV.UK One Login?", v => Assert.Contains(reason!.GetDisplayName()!, v.TrimmedText()));
        doc.AssertSummaryListRowValue("Should this GOV.UK One Login stay verified?", v => Assert.Contains(stayVerified!.GetDisplayName()!, v.TrimmedText()));
    }

    [Theory]
    [InlineData(DisconnectPersonStayVerified.Yes, DisconnectPersonReason.AnotherReason, "this is a test record")]
    public async Task Get_RendersOtherReasonDetails(DisconnectPersonStayVerified? stayVerified,
        DisconnectPersonReason? reason, string? details)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oneLogin = await TestData.CreateOneLoginUserAsync(person);
        var journeyInstance = await CreateJourneyInstanceAsync(
            oneLogin.Subject,
            new DisconnectPersonState() { DisconnectReason = reason, StayVerified = stayVerified, Detail = details });

        var request = new HttpRequestMessage(HttpMethod.Get,
            $"/one-logins/{oneLogin.Subject}/disconnect-person/{person.PersonId}/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        doc.GetElementByTestId("disconnect-reason-detail");
    }

    private Task<JourneyInstance<DisconnectPersonState>> CreateJourneyInstanceAsync(string oneLoginUserSubject, DisconnectPersonState? state = null) =>
        CreateJourneyInstance(
            JourneyNames.DisconnectPerson,
            state ?? new DisconnectPersonState(),
            new KeyValuePair<string, object>("oneLoginUserSubject", oneLoginUserSubject));
}
