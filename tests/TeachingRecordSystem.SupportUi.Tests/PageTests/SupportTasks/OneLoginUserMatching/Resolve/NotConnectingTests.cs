using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.SupportUi.Pages.SupportTasks.OneLoginUserMatching.Resolve;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.OneLoginUserMatching.Resolve;

public class NotConnectingTests(HostFixture hostFixture) : ResolveOneLoginUserMatchingTestBase(hostFixture)
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Get_ValidRequest_ReturnsOk(bool isRecordMatchingOnlySupportTask)
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = isRecordMatchingOnlySupportTask ?
            await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(oneLoginUser.Subject) :
            await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);

        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask,
            s =>
            {
                s.Verified = true;
                s.MatchedPersonId = ResolveOneLoginUserMatchingState.NotMatchedPersonIdSentinel;
            });

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/not-connecting?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseAsync(response);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Get_ValidRequestWithDataInState_PopulatesFields(bool isRecordMatchingOnlySupportTask)
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = isRecordMatchingOnlySupportTask ?
            await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(oneLoginUser.Subject) :
            await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);

        var additionalDetails = Faker.Lorem.Paragraph();
        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask,
            s =>
            {
                s.Verified = true;
                s.MatchedPersonId = ResolveOneLoginUserMatchingState.NotMatchedPersonIdSentinel;
                s.NotConnectingReason = OneLoginUserNotConnectingReason.AnotherReason;
                s.NotConnectingAdditionalDetails = additionalDetails;
            });

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/not-connecting?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.True(doc.GetElementsByName("Reason").SingleOrDefault(e => e.GetAttribute("value") == nameof(OneLoginIdVerificationRejectReason.AnotherReason))!.HasAttribute("checked"));
        Assert.Equal(additionalDetails, doc.GetElementsByName("AdditionalDetails").Single().TrimmedText());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Post_NoReasonChosen_ShowsError(bool isRecordMatchingOnlySupportTask)
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = isRecordMatchingOnlySupportTask ?
            await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(oneLoginUser.Subject) :
            await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);

        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask,
            s =>
            {
                s.Verified = true;
                s.MatchedPersonId = ResolveOneLoginUserMatchingState.NotMatchedPersonIdSentinel;
            });

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/not-connecting?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "Reason", "Select a reason");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Post_AnotherReasonChosenAndNoAdditionalDetails_ShowsError(bool isRecordMatchingOnlySupportTask)
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = isRecordMatchingOnlySupportTask ?
            await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(oneLoginUser.Subject) :
            await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);

        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask,
            s =>
            {
                s.Verified = true;
                s.MatchedPersonId = ResolveOneLoginUserMatchingState.NotMatchedPersonIdSentinel;
            });

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/not-connecting?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Reason", nameof(OneLoginUserNotConnectingReason.AnotherReason) }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "AdditionalDetails", "Enter additional detail");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Post_ValidRequest_UpdatesStateAndRedirects(bool isRecordMatchingOnlySupportTask)
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = isRecordMatchingOnlySupportTask ?
            await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(oneLoginUser.Subject) :
            await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);

        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask,
            s =>
            {
                s.Verified = true;
                s.MatchedPersonId = ResolveOneLoginUserMatchingState.NotMatchedPersonIdSentinel;
            });

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/not-connecting?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Reason", nameof(OneLoginUserNotConnectingReason.NoMatchingRecord) }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/confirm-not-connecting?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);

        var journeyState = GetJourneyInstanceState(journeyInstance);
        Assert.Equal(OneLoginUserNotConnectingReason.NoMatchingRecord, journeyState!.NotConnectingReason);
        Assert.Null(journeyState!.NotConnectingAdditionalDetails);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Post_ValidRequestWithAnotherReason_UpdatesStateAndRedirects(bool isRecordMatchingOnlySupportTask)
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = isRecordMatchingOnlySupportTask ?
            await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(oneLoginUser.Subject) :
            await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);

        var additionalDetails = Faker.Lorem.Paragraph();
        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask,
            s =>
            {
                s.Verified = true;
                s.MatchedPersonId = ResolveOneLoginUserMatchingState.NotMatchedPersonIdSentinel;
            });

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/not-connecting?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Reason", nameof(OneLoginUserNotConnectingReason.AnotherReason) },
                { "AdditionalDetails", additionalDetails }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/confirm-not-connecting?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);

        var journeyState = GetJourneyInstanceState(journeyInstance);
        Assert.Equal(OneLoginUserNotConnectingReason.AnotherReason, journeyState!.NotConnectingReason);
        Assert.Equal(additionalDetails, journeyState!.NotConnectingAdditionalDetails);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public async Task Post_SaveAndComeBackLater_PersistsJourneyStateIntoTaskAndRedirectsToCorrectPage(bool isRecordMatchingOnlySupportTask, bool supportTaskDashboardEnabled)
    {
        // Arrange
        FeatureProvider.Features.Clear();
        if (supportTaskDashboardEnabled)
        {
            FeatureProvider.Features.Add("SupportTaskDashboard");
        }

        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = isRecordMatchingOnlySupportTask ?
            await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(oneLoginUser.Subject) :
            await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);

        var reason = OneLoginUserNotConnectingReason.AnotherReason;
        var additionalDetails = Faker.Lorem.Paragraph();
        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask,
            s =>
            {
                s.Verified = true;
                s.MatchedPersonId = ResolveOneLoginUserMatchingState.NotMatchedPersonIdSentinel;
            });

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/not-connecting?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "Action", "SaveAndComeBackLater" },
                { "Reason", reason.ToString() },
                { "AdditionalDetails", additionalDetails }
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
        else if (isRecordMatchingOnlySupportTask)
        {
            Assert.Equal($"/support-tasks/one-login-user-matching/record-matching", response.Headers.Location?.OriginalString);
        }
        else
        {
            Assert.Equal($"/support-tasks/one-login-user-matching/id-verification", response.Headers.Location?.OriginalString);
        }

        await WithDbContextAsync(async dbContext =>
        {
            supportTask = (await dbContext.SupportTasks.FindAsync(supportTask.SupportTaskReference))!;
            Assert.NotNull(supportTask.ResolveJourneySavedState);

            Assert.Equal("NotConnecting", supportTask.ResolveJourneySavedState.PageName);

            Assert.Collection(
                supportTask.ResolveJourneySavedState.ModelStateValues,
                kvp =>
                {
                    Assert.Equal("Reason", kvp.Key);
                    Assert.Equal(reason.ToString(), kvp.Value);
                },
                kvp =>
                {
                    Assert.Equal("AdditionalDetails", kvp.Key);
                    Assert.Equal(additionalDetails, kvp.Value);
                });

            var savedState = supportTask.ResolveJourneySavedState.GetState<ResolveOneLoginUserMatchingState>();
            Assert.NotNull(savedState);
            Assert.True(savedState.Verified);
            Assert.Equal(ResolveOneLoginUserMatchingState.NotMatchedPersonIdSentinel, savedState.MatchedPersonId);
        });

        Assert.Null(GetJourneyInstanceState(journeyInstance));

        Events.AssertProcessesCreated(p => Assert.Equal(
            isRecordMatchingOnlySupportTask ?
                ProcessType.OneLoginUserRecordMatchingSupportTaskSaving :
                ProcessType.OneLoginUserIdVerificationSupportTaskSaving,
            p.ProcessContext.ProcessType));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Post_Cancel_DeletesJourneyAndRedirectToListPage(bool isRecordMatchingOnlySupportTask)
    {
        // Arrange
        var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: false);
        var supportTask = isRecordMatchingOnlySupportTask ?
            await TestData.CreateOneLoginUserRecordMatchingSupportTaskAsync(oneLoginUser.Subject) :
            await TestData.CreateOneLoginUserIdVerificationSupportTaskAsync(oneLoginUser.Subject);

        var journeyInstance = await CreateJourneyInstanceAsync(
            supportTask,
            s =>
            {
                s.Verified = true;
                s.MatchedPersonId = ResolveOneLoginUserMatchingState.NotMatchedPersonIdSentinel;
            });

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/one-login-user-matching/{supportTask.SupportTaskReference}/resolve/not-connecting?{journeyInstance.GetUniqueIdQueryParameter()}")
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
        if (isRecordMatchingOnlySupportTask)
        {
            Assert.Equal(
                "/support-tasks/one-login-user-matching/record-matching",
                response.Headers.Location?.OriginalString);
        }
        else
        {
            Assert.Equal(
                "/support-tasks/one-login-user-matching/id-verification",
                response.Headers.Location?.OriginalString);
        }

        Assert.Null(GetJourneyInstanceState(journeyInstance));
    }
}
