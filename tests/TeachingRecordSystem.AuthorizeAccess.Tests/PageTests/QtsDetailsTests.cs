using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using static TeachingRecordSystem.AuthorizeAccess.SignInJourneyCoordinator.Vtrs;

namespace TeachingRecordSystem.AuthorizeAccess.Tests.PageTests;

public class QtsDetailsTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public Task Get_ValidRequest_RendersExpectedContent(bool haveExistingValueInState) =>
        WithJourneyCoordinatorAsync(
            CreateSignInJourneyState,
            async coordinator =>
            {
                // Arrange
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);
                await SetupInstanceStateAsync(coordinator, oneLoginUser);

                var existingYear = haveExistingValueInState ? "2023" : null;
                Guid? existingTrainingProviderId = null;
                Guid? existingSubjectId = null;

                if (haveExistingValueInState)
                {
                    var trainingProvider = await GetTrainingProviderAsync();
                    var subject = await GetTrainingSubjectAsync();

                    existingTrainingProviderId = trainingProvider.TrainingProviderId;
                    existingSubjectId = subject.TrainingSubjectId;

                    coordinator.UpdateState(s => s.SetQtsDetails(existingYear!, existingTrainingProviderId.Value, existingSubjectId.Value));
                }

                var request = new HttpRequestMessage(HttpMethod.Get, JourneyUrls.QtsDetails(coordinator.InstanceId));

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                var doc = await AssertEx.HtmlResponseAsync(response);
                Assert.Equal("Enter your QTS details (optional)", doc.QuerySelector("h1")?.TextContent.Trim());
                Assert.Equal(existingYear, doc.GetElementById("YearQtsReceived")?.GetAttribute("value"));
                Assert.Equal(existingTrainingProviderId?.ToString(), doc.QuerySelector("#TrainingProviderId option:checked")?.GetAttribute("value"));
                Assert.Equal(existingSubjectId?.ToString(), doc.QuerySelector("#SubjectId option:checked")?.GetAttribute("value"));
                Assert.Equal("Why are we asking this?", doc.QuerySelector(".govuk-details__summary-text")?.TextContent.Trim());
            });

    [Fact]
    public Task Post_ContinueEmptyRequest_UpdatesStateNotRequiredAndRedirectsToCheckAnswersPage() =>
        WithJourneyCoordinatorAsync(
            CreateSignInJourneyState,
            async coordinator =>
            {
                // Arrange
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);
                await SetupInstanceStateAsync(coordinator, oneLoginUser);

                var request = new HttpRequestMessage(HttpMethod.Post, JourneyUrls.QtsDetails(coordinator.InstanceId))
                {
                    Content = new FormUrlEncodedContentBuilder { }
                };

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
                Assert.Equal(JourneyUrls.CheckAnswers(coordinator.InstanceId), response.Headers.Location?.OriginalString);

                var state = coordinator.State;
                Assert.Null(state.YearQtsReceived);
                Assert.Null(state.QtsTrainingProviderId);
                Assert.Null(state.QtsSubjectId);
            });

    [Fact]
    public Task Post_ContinueYearContainsText_RendersError() =>
        WithJourneyCoordinatorAsync(
            CreateSignInJourneyState,
            async coordinator =>
            {
                // Arrange
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);
                await SetupInstanceStateAsync(coordinator, oneLoginUser);

                var request = new HttpRequestMessage(HttpMethod.Post, JourneyUrls.QtsDetails(coordinator.InstanceId))
                {
                    Content = new FormUrlEncodedContentBuilder
                    {
                        { "YearQtsReceived", "abcd" },
                        { "TrainingProviderId", (await GetTrainingProviderAsync()).TrainingProviderId.ToString() },
                        { "SubjectId", (await GetTrainingSubjectAsync()).TrainingSubjectId.ToString() }
                    }
                };

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                await AssertEx.HtmlResponseHasErrorAsync(response, "YearQtsReceived", "Year QTS was received must be 4 digits");
            });

    [Fact]
    public Task Post_ContinueYearIsInFuture_RendersError() =>
        WithJourneyCoordinatorAsync(
            CreateSignInJourneyState,
            async coordinator =>
            {
                // Arrange
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);
                await SetupInstanceStateAsync(coordinator, oneLoginUser);

                var futureYear = (TimeProvider.UtcNow.Year + 1).ToString();
                var trainingProvider = await GetTrainingProviderAsync();
                var subject = await GetTrainingSubjectAsync();

                var request = new HttpRequestMessage(HttpMethod.Post, JourneyUrls.QtsDetails(coordinator.InstanceId))
                {
                    Content = new FormUrlEncodedContentBuilder
                    {
                        { "YearQtsReceived", futureYear },
                        { "TrainingProviderId", trainingProvider.TrainingProviderId.ToString() },
                        { "SubjectId", subject.TrainingSubjectId.ToString() }
                    }
                };

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                await AssertEx.HtmlResponseHasErrorAsync(response, "YearQtsReceived", "Year QTS was received cannot be in the future");
            });

    [Fact]
    public Task Post_ContinueTrainingProviderNotAnswered_UpdatesStateAndRedirectsToCheckAnswersPage() =>
        WithJourneyCoordinatorAsync(
            CreateSignInJourneyState,
            async coordinator =>
            {
                // Arrange
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);
                await SetupInstanceStateAsync(coordinator, oneLoginUser);
                var qtsReceived = (TimeProvider.UtcNow.AddDays(-10)).ToString("yyyy");

                var subject = await GetTrainingSubjectAsync();

                var request = new HttpRequestMessage(HttpMethod.Post, JourneyUrls.QtsDetails(coordinator.InstanceId))
                {
                    Content = new FormUrlEncodedContentBuilder
                    {
                        { "YearQtsReceived", qtsReceived },
                        { "TrainingProviderId", "" },
                        { "SubjectId", subject.TrainingSubjectId.ToString() }
                    }
                };

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
                Assert.Equal(JourneyUrls.CheckAnswers(coordinator.InstanceId), response.Headers.Location?.OriginalString);

                var state = coordinator.State;
                Assert.Equal(qtsReceived, state.YearQtsReceived);
                Assert.Null(state.QtsTrainingProviderId);
                Assert.Equal(subject.TrainingSubjectId, state.QtsSubjectId);
            });

    [Fact]
    public Task Post_ContinueSubjectNotAnswered_UpdatesStateAndRedirectsToCheckAnswersPage() =>
        WithJourneyCoordinatorAsync(
            CreateSignInJourneyState,
            async coordinator =>
            {
                // Arrange
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);
                await SetupInstanceStateAsync(coordinator, oneLoginUser);
                var qtsReceived = (TimeProvider.UtcNow.AddDays(-10)).ToString("yyyy");

                var trainingProvider = await GetTrainingProviderAsync();

                var request = new HttpRequestMessage(HttpMethod.Post, JourneyUrls.QtsDetails(coordinator.InstanceId))
                {
                    Content = new FormUrlEncodedContentBuilder
                    {
                        { "YearQtsReceived", qtsReceived },
                        { "TrainingProviderId", trainingProvider.TrainingProviderId.ToString() },
                        { "SubjectId", "" }
                    }
                };

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
                Assert.Equal(JourneyUrls.CheckAnswers(coordinator.InstanceId), response.Headers.Location?.OriginalString);

                var state = coordinator.State;
                Assert.Equal(qtsReceived, state.YearQtsReceived);
                Assert.Equal(trainingProvider.TrainingProviderId, state.QtsTrainingProviderId);
                Assert.Null(state.QtsSubjectId);
            });

    [Fact]
    public Task Post_ContinueUnknownTrainingProvider_RendersError() =>
        WithJourneyCoordinatorAsync(
            CreateSignInJourneyState,
            async coordinator =>
            {
                // Arrange
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);
                await SetupInstanceStateAsync(coordinator, oneLoginUser);
                var qtsReceived = (TimeProvider.UtcNow.AddDays(-10)).ToString("yyyy");
                var subject = await GetTrainingSubjectAsync();

                var request = new HttpRequestMessage(HttpMethod.Post, JourneyUrls.QtsDetails(coordinator.InstanceId))
                {
                    Content = new FormUrlEncodedContentBuilder
                    {
                        { "YearQtsReceived", qtsReceived },
                        { "TrainingProviderId", Guid.NewGuid().ToString() },
                        { "SubjectId", subject.TrainingSubjectId.ToString() }
                    }
                };

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                await AssertEx.HtmlResponseHasErrorAsync(response, "TrainingProviderId", "Select a training provider");
            });

    [Fact]
    public Task Post_ContinueUnknownSubject_RendersError() =>
        WithJourneyCoordinatorAsync(
            CreateSignInJourneyState,
            async coordinator =>
            {
                // Arrange
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);
                await SetupInstanceStateAsync(coordinator, oneLoginUser);
                var qtsReceived = (TimeProvider.UtcNow.AddDays(-10)).ToString("yyyy");
                var trainingProvider = await GetTrainingProviderAsync();

                var request = new HttpRequestMessage(HttpMethod.Post, JourneyUrls.QtsDetails(coordinator.InstanceId))
                {
                    Content = new FormUrlEncodedContentBuilder
                    {
                        { "YearQtsReceived", qtsReceived },
                        { "TrainingProviderId", trainingProvider.TrainingProviderId.ToString() },
                        { "SubjectId", Guid.NewGuid().ToString() }
                    }
                };

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                await AssertEx.HtmlResponseHasErrorAsync(response, "SubjectId", "Select a subject");
            });

    [Fact]
    public Task Post_ContinueValidRequest_UpdatesStateAndRedirectsToCheckAnswersPage() =>
        WithJourneyCoordinatorAsync(
            CreateSignInJourneyState,
            async coordinator =>
            {
                // Arrange
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);
                await SetupInstanceStateAsync(coordinator, oneLoginUser);

                var trainingProvider = await GetTrainingProviderAsync();
                var subject = await GetTrainingSubjectAsync();

                var request = new HttpRequestMessage(HttpMethod.Post, JourneyUrls.QtsDetails(coordinator.InstanceId))
                {
                    Content = new FormUrlEncodedContentBuilder
                    {
                        { "YearQtsReceived", TimeProvider.UtcNow.Year.ToString() },
                        { "TrainingProviderId", trainingProvider.TrainingProviderId.ToString() },
                        { "SubjectId", subject.TrainingSubjectId.ToString() }
                    }
                };

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
                Assert.Equal(JourneyUrls.CheckAnswers(coordinator.InstanceId), response.Headers.Location?.OriginalString);

                var state = coordinator.State;
                Assert.Equal(TimeProvider.UtcNow.Year.ToString(), state.YearQtsReceived);
                Assert.Equal(trainingProvider.TrainingProviderId, state.QtsTrainingProviderId);
                Assert.Equal(subject.TrainingSubjectId, state.QtsSubjectId);
            });

    [Fact]
    public Task Post_Skip_UpdatesStateNotRequiredAndRedirectsToCheckAnswersPage() =>
        WithJourneyCoordinatorAsync(
            CreateSignInJourneyState,
            async coordinator =>
            {
                // Arrange
                var oneLoginUser = await TestData.CreateOneLoginUserAsync(verified: true);
                await SetupInstanceStateAsync(coordinator, oneLoginUser);

                var request = new HttpRequestMessage(HttpMethod.Post, JourneyUrls.QtsDetails(coordinator.InstanceId))
                {
                    Content = new FormUrlEncodedContentBuilder
                    {
                        { "Skip", bool.TrueString }
                    }
                };

                // Act
                var response = await HttpClient.SendAsync(request);

                // Assert
                Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
                Assert.Equal(JourneyUrls.CheckAnswers(coordinator.InstanceId), response.Headers.Location?.OriginalString);

                var state = coordinator.State;
                Assert.Null(state.YearQtsReceived);
                Assert.Null(state.QtsTrainingProviderId);
                Assert.Null(state.QtsSubjectId);
            });

    private async Task SetupInstanceStateAsync(SignInJourneyCoordinator coordinator, OneLoginUser oneLoginUser)
    {
        var ticket = CreateOneLoginAuthenticationTicket(vtr: AuthenticationOnly, oneLoginUser);
        await coordinator.OnOneLoginCallbackAsync(ticket);
        AddUrlToPath(coordinator, StepUrls.NotFound);
        AddUrlToPath(coordinator, StepUrls.QtsStatus);
        AddUrlToPath(coordinator, StepUrls.QtsDetails);
    }

    private async Task<TrainingProvider> GetTrainingProviderAsync() =>
        (await TestData.ReferenceDataCache.GetTrainingProvidersAsync()).Where(x => !x.Name.Contains('\'')).First();

    private async Task<TrainingSubject> GetTrainingSubjectAsync() =>
        (await TestData.ReferenceDataCache.GetTrainingSubjectsAsync()).Where(x => !x.Name.Contains('\'')).First();
}
