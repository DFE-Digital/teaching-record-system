using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.OneLogin;
using TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserMatching.Resolve;
using CoreNationalInsuranceNumber = TeachingRecordSystem.Core.NationalInsuranceNumber;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.OneLoginUserMatching.Resolve;

[ClearDbBeforeTest, Collection(nameof(DisableParallelization))]
public class VerifyTests(HostFixture hostFixture) : ResolveOneLoginUserMatchingTestBase(hostFixture)
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Get_ValidRequest_RendersExpectedContent(bool evidenceIsPdf)
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(
            oneLoginUser.Subject,
            b => b.WithEvidenceFileName(evidenceIsPdf ? "evidence.pdf" : "evidence.jpg"));
        var requestData = supportTask.GetData<OneLoginUserIdVerificationData>();

        var journeyInstance = await CreateJourneyInstanceAsync(supportTask);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/verify?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal($"{requestData.StatedFirstName} {requestData.StatedLastName}", doc.GetSummaryListValueByKey("Name"));
        Assert.Equal(requestData.StatedDateOfBirth.ToString(WebConstants.DateDisplayFormat), doc.GetSummaryListValueByKey("Date of birth"));
        Assert.Equal(oneLoginUser.EmailAddress, doc.GetSummaryListValueByKey("Email address"));
        Assert.Equal(TrnHelper.NormalizeTrn(requestData.StatedTrn), doc.GetSummaryListValueByKey("TRN"));
        Assert.Equal(CoreNationalInsuranceNumber.Normalize(requestData.StatedNationalInsuranceNumber), doc.GetSummaryListValueByKey("National Insurance number"));
        Assert.Contains("Yes, verify and find a matching record (if applicable)", doc.Body!.TextContent);
        if (evidenceIsPdf)
        {
            Assert.NotNull(doc.GetElementByTestId($"pdf-{requestData.EvidenceFileId}"));
            Assert.Null(doc.GetElementByTestId($"image-{requestData.EvidenceFileId}"));
        }
        else
        {
            Assert.NotNull(doc.GetElementByTestId($"image-{requestData.EvidenceFileId}"));
            Assert.Null(doc.GetElementByTestId($"pdf-{requestData.EvidenceFileId}"));
        }
    }

    [Fact]
    public async Task Get_ValidRequest_WithNonNormalizedTrnAndNino_RendersNormalizedValues()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var nonNormalizedTrn = "01/23456";
        var normalizedTrn = "0123456";
        var nonNormalizedNino = "ab 12 34 56 c";
        var normalizedNino = "AB123456C";
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(
            oneLoginUser.Subject,
            b => b
                .WithStatedTrn(nonNormalizedTrn)
                .WithStatedNationalInsuranceNumber(nonNormalizedNino));

        var journeyInstance = await CreateJourneyInstanceAsync(supportTask);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/verify?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(normalizedTrn, doc.GetSummaryListValueByKey("TRN"));
        Assert.Equal(normalizedNino, doc.GetSummaryListValueByKey("National Insurance number"));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Get_WhereStateIsPopulated_SetsInputFields(bool verified)
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);

        var journeyInstance = await CreateJourneyInstanceAsync(supportTask, state => state.Verified = verified);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/verify?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        var doc = await AssertEx.HtmlResponseAsync(response);
        AssertCheckedRadioOption("Verified", verified.ToString());

        void AssertCheckedRadioOption(string name, string expectedCheckedValue)
        {
            var selectedOption = doc.GetElementsByName(name).SingleOrDefault(r => r.HasAttribute("checked"));
            Assert.Equal(expectedCheckedValue, selectedOption?.GetAttribute("value"));
        }
    }

    [Fact]
    public async Task Post_WhenNoVerifiedOptionIsSelected_ReturnsError()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);

        var journeyInstance = await CreateJourneyInstanceAsync(supportTask);

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/verify?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "Verified", "Select yes if you can verify this person’s identity");
    }

    [Fact]
    public async Task Post_VerifiedIsFalse_UpdatesStateAndRedirectsToRejectPage()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);

        var journeyInstance = await CreateJourneyInstanceAsync(supportTask);

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/verify?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Verified", "False" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/reject?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);

        var journeyState = GetJourneyInstanceState(journeyInstance);
        Assert.False(journeyState!.Verified);
    }

    [Fact]
    public async Task Post_VerifiedIsTrueAndNoMatches_UpdatesStateAndRedirectsToNoMatchesPage()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);

        var journeyInstance = await CreateJourneyInstanceWithMatchedPersonsAsync(supportTask);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/verify?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Verified", "True" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/no-matches?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);

        var journeyState = GetJourneyInstanceState(journeyInstance);
        Assert.True(journeyState!.Verified);
    }

    [Fact]
    public async Task Post_VerifiedIsTrueAndNoTrnProvided_UpdatesStateAndRedirectsToNoMatchesPage()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(
            oneLoginUser.Subject,
            t => t.WithStatedTrn(null));

        var journeyInstance = await CreateJourneyInstanceAsync(supportTask);

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/verify?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Verified", "True" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/no-matches?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);

        var journeyState = GetJourneyInstanceState(journeyInstance);
        Assert.True(journeyState!.Verified);
        Assert.Empty(journeyState!.MatchedPersons);
    }

    [Fact]
    public async Task Post_VerifiedIsTrueAndOneDefiniteMatch_UpdatesStateAndRedirectsToConfirmConnectPage()
    {
        // Arrange
        var matchedPerson = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(
            oneLoginUser.Subject, t => t
                .WithStatedFirstName(matchedPerson.FirstName)
                .WithStatedLastName(matchedPerson.LastName)
                .WithStatedDateOfBirth(matchedPerson.DateOfBirth!.Value)
                .WithStatedTrn(matchedPerson.Trn!));

        var journeyInstance = await CreateJourneyInstanceWithMatchedPersonsAsync(
            supportTask,
            state => state.DefiniteMatch = true,
            new MatchPersonResult(
                matchedPerson.PersonId,
                matchedPerson.Trn,
                [
                    KeyValuePair.Create(PersonMatchedAttribute.FirstName, matchedPerson.FirstName),
                    KeyValuePair.Create(PersonMatchedAttribute.LastName, matchedPerson.LastName),
                    KeyValuePair.Create(PersonMatchedAttribute.DateOfBirth, matchedPerson.DateOfBirth!.Value.ToString("yyyy-MM-dd")),
                    KeyValuePair.Create(PersonMatchedAttribute.Trn, matchedPerson.Trn)
                ]));

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/verify?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Verified", "True" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/confirm-connect?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);

        var journeyState = GetJourneyInstanceState(journeyInstance);
        Assert.True(journeyState!.Verified);
        Assert.True(journeyState.DefiniteMatch);
        Assert.Equal(matchedPerson.PersonId, journeyState.MatchedPersonId);
    }

    [Fact]
    public async Task Post_VerifiedIsTrueAndOneNonDefiniteMatch_UpdatesStateAndRedirectsToMatchesPage()
    {
        // Arrange
        var matchedPerson = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(
            oneLoginUser.Subject, t => t
                .WithStatedFirstName(matchedPerson.FirstName)
                .WithStatedLastName(matchedPerson.LastName)
                .WithStatedDateOfBirth(matchedPerson.DateOfBirth!.Value)
                .WithStatedTrn(matchedPerson.Trn!));

        var journeyInstance = await CreateJourneyInstanceWithMatchedPersonsAsync(
            supportTask,
            configureState: null,
            new MatchPersonResult(
                matchedPerson.PersonId,
                matchedPerson.Trn,
                [
                    KeyValuePair.Create(PersonMatchedAttribute.FirstName, matchedPerson.FirstName),
                    KeyValuePair.Create(PersonMatchedAttribute.LastName, matchedPerson.LastName),
                    KeyValuePair.Create(PersonMatchedAttribute.DateOfBirth, matchedPerson.DateOfBirth!.Value.ToString("yyyy-MM-dd"))
                ]));

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/verify?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Verified", "True" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);

        var journeyState = GetJourneyInstanceState(journeyInstance);
        Assert.True(journeyState!.Verified);
        Assert.False(journeyState.DefiniteMatch);
        Assert.Null(journeyState.MatchedPersonId);
    }

    [Fact]
    public async Task Post_VerifiedIsTrueAndMultipleMatches_UpdatesStateAndRedirectsToMatchesPage()
    {
        // Arrange
        var matchedPerson1 = await TestData.CreatePersonAsync();
        var matchedPerson2 = await TestData.CreatePersonAsync();
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(
            oneLoginUser.Subject, t => t
                .WithStatedFirstName(matchedPerson1.FirstName)
                .WithStatedLastName(matchedPerson1.LastName)
                .WithStatedDateOfBirth(matchedPerson1.DateOfBirth!.Value)
                .WithStatedTrn(matchedPerson1.Trn!));

        var journeyInstance = await CreateJourneyInstanceWithMatchedPersonsAsync(
            supportTask,
            configureState: null,
            new MatchPersonResult(
                matchedPerson1.PersonId,
                matchedPerson1.Trn,
                [
                    KeyValuePair.Create(PersonMatchedAttribute.FirstName, matchedPerson1.FirstName),
                    KeyValuePair.Create(PersonMatchedAttribute.LastName, matchedPerson1.LastName),
                    KeyValuePair.Create(PersonMatchedAttribute.DateOfBirth, matchedPerson1.DateOfBirth!.Value.ToString("yyyy-MM-dd")),
                    KeyValuePair.Create(PersonMatchedAttribute.Trn, matchedPerson1.Trn)
                ]),
            new MatchPersonResult(
                matchedPerson2.PersonId,
                matchedPerson2.Trn,
                [
                    KeyValuePair.Create(PersonMatchedAttribute.FirstName, matchedPerson2.FirstName),
                    KeyValuePair.Create(PersonMatchedAttribute.LastName, matchedPerson2.LastName),
                    KeyValuePair.Create(PersonMatchedAttribute.DateOfBirth, matchedPerson2.DateOfBirth!.Value.ToString("yyyy-MM-dd")),
                    KeyValuePair.Create(PersonMatchedAttribute.Trn, matchedPerson2.Trn)
                ]));

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/verify?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Verified", "True" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);

        var journeyState = GetJourneyInstanceState(journeyInstance);
        Assert.True(journeyState!.Verified);
        Assert.Null(journeyState.MatchedPersonId);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Post_SaveAndComeBackLater_PersistsJourneyStateIntoTaskAndRedirectsToCorrectPage(bool supportTaskDashboardEnabled)
    {
        // Arrange
        // Recreate AdminUser since [ClearDbBeforeTest] removes it
        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Users.Add(HostFixture.AdminUser);
            await dbContext.SaveChangesAsync();
        });

        FeatureProvider.Features.Clear();
        if (supportTaskDashboardEnabled)
        {
            FeatureProvider.Features.Add("SupportTaskDashboard");
        }

        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);

        var verified = true;
        var journeyInstance = await CreateJourneyInstanceAsync(supportTask);

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/verify?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Action", "SaveAndComeBackLater" },
                { "Verified", verified.ToString() }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        if (supportTaskDashboardEnabled)
        {
            Assert.Equal($"/support-tasks/{supportTask.SupportTaskReference}", response.Headers.Location?.OriginalString);
        }
        else
        {
            Assert.Equal($"/support-tasks/one-login-user-matching/id-verification", response.Headers.Location?.OriginalString);
        }

        await WithDbContextAsync(async dbContext =>
        {
            supportTask = (await dbContext.SupportTasks.FindAsync(supportTask.SupportTaskReference))!;
            Assert.NotNull(supportTask.ResolveJourneySavedState);

            Assert.Equal("VerifyModel", supportTask.ResolveJourneySavedState.PageName);

            Assert.Collection(
                supportTask.ResolveJourneySavedState.ModelStateValues,
                kvp =>
                {
                    Assert.Equal("Verified", kvp.Key);
                    Assert.Equal(verified.ToString(), kvp.Value);
                });

            var savedState = supportTask.ResolveJourneySavedState.GetState<ResolveOneLoginUserMatchingState>();
            Assert.NotNull(savedState);
        });

        Assert.Null(GetJourneyInstanceState(journeyInstance));

        Events.AssertProcessesCreated(p => Assert.Equal(
            ProcessType.OneLoginUserIdVerificationSupportTaskSaving,
            p.ProcessContext.ProcessType));
    }

    [Fact]
    public async Task Post_Cancel_DeletesJourneyAndRedirectsToListPage()
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);

        var journeyInstance = await CreateJourneyInstanceAsync(supportTask);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/verify?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Action", "Cancel" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/support-tasks/one-login-user-matching/id-verification", response.Headers.Location?.OriginalString);

        Assert.Null(GetJourneyInstanceState(journeyInstance));
    }

    [Fact]
    public async Task Get_JourneyStartedWithReturnUrl_RendersBackLinkToReturnUrl()
    {
        // Arrange
        var returnUrl = "/support-tasks/active?keyword=test";
        var verifyUrl = await StartJourneyAsync(returnUrl);

        var request = new HttpRequestMessage(HttpMethod.Get, verifyUrl);

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(returnUrl, doc.GetElementsByClassName("govuk-back-link").Single().GetAttribute("href"));
    }

    [Fact]
    public async Task Post_Cancel_JourneyStartedWithReturnUrl_RedirectsToReturnUrl()
    {
        // Arrange
        var returnUrl = "/support-tasks/active?keyword=test";
        var verifyUrl = await StartJourneyAsync(returnUrl);

        var request = new HttpRequestMessage(HttpMethod.Post, verifyUrl)
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "action", "Cancel" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(returnUrl, response.Headers.Location?.OriginalString);
    }

    /// <summary>
    /// Starts the journey the way the "View task" link does and returns the URL of this page.
    /// </summary>
    private async Task<string> StartJourneyAsync(string returnUrl)
    {
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve?returnUrl={Uri.EscapeDataString(returnUrl)}");

        var response = await HttpClient.SendAsync(request);  // Initializes journey
        response = await response.FollowRedirectAsync(HttpClient);

        return response.Headers.Location!.OriginalString;
    }
}
