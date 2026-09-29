using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.ChangeReasons;
using TeachingRecordSystem.Core.Services.Persons;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.EditInduction;

public class CheckYourAnswersTests(HostFixture hostFixture) : EditInductionTestBase(hostFixture)
{
    private const string ChangeReasonDetails = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat.";
    private const string AdditionalInformation = "Lorem ipsum dolor sit amet, consectetur adipiscing elit";

    public static IEnumerable<object[]> GetInductionStatusData()
    {
        yield return
        [
            new EditInductionState
            {
                InductionStatus = InductionStatus.InProgress,
                CurrentInductionStatus = InductionStatus.InProgress,
                CompletedDate = DateOnly.Parse("2024-12-31"),
                ChangeReason = PersonInductionChangeReason.AnotherReason
            }
        ];
        yield return
        [
            new EditInductionState
            {
                InductionStatus = InductionStatus.Passed,
                CurrentInductionStatus = InductionStatus.Passed,
                StartDate = DateOnly.Parse("2024-12-31"),
                ChangeReason = PersonInductionChangeReason.AnotherReason
            }
        ];
        yield return
        [
            new EditInductionState
            {
                InductionStatus = InductionStatus.RequiredToComplete,
                CurrentInductionStatus = InductionStatus.RequiredToComplete,
                StartDate = DateOnly.Parse("2024-12-31"),
                ChangeReason = PersonInductionChangeReason.AnotherReason
            }
        ];
        yield return
        [
            new EditInductionState
            {
                InductionStatus = InductionStatus.InProgress,
                CurrentInductionStatus = InductionStatus.InProgress,
                StartDate = DateOnly.Parse("2024-12-31")
            }
        ];
    }

    [Theory]
    [MemberData(nameof(GetInductionStatusData))]
    public async Task Get_WithInvalidJourneyState_RedirectToStart(EditInductionState editInductionState)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithQts());

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            editInductionState);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/edit-induction/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/persons/{person.PersonId}/edit-induction/status?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Theory]
    [InlineData(StartPage.Status, InductionStatus.InProgress, true)]
    [InlineData(StartPage.Status, InductionStatus.Passed, true)]
    [InlineData(StartPage.Status, InductionStatus.Failed, true)]
    [InlineData(StartPage.Status, InductionStatus.Exempt, true)]
    [InlineData(StartPage.Status, InductionStatus.FailedInWales, true)]
    [InlineData(StartPage.Status, InductionStatus.RequiredToComplete, true)]
    [InlineData(StartPage.StartDate, InductionStatus.InProgress, false)]
    [InlineData(StartPage.StartDate, InductionStatus.Passed, false)]
    [InlineData(StartPage.StartDate, InductionStatus.Failed, false)]
    [InlineData(StartPage.StartDate, InductionStatus.FailedInWales, false)]
    [InlineData(StartPage.CompletedDate, InductionStatus.Passed, false)]
    [InlineData(StartPage.CompletedDate, InductionStatus.Failed, false)]
    [InlineData(StartPage.CompletedDate, InductionStatus.FailedInWales, false)]
    [InlineData(StartPage.ExemptionReasons, InductionStatus.Exempt, false)]
    public async Task Get_ShowsInductionStatus_AsExpected(StartPage startPage, InductionStatus inductionStatus, bool showChangeLink)
    {
        // Arrange
        var labelContent = "Status";

        DateOnly? startDate = inductionStatus.RequiresStartDate() ? TimeProvider.Today.AddYears(-2) : null;
        DateOnly? completedDate = inductionStatus.RequiresCompletedDate() ? TimeProvider.Today : null;

        var exemptionReasonIds = inductionStatus is InductionStatus.Exempt
            ? (await TestData.ReferenceDataCache.GetInductionExemptionReasonsAsync(activeOnly: true))
                .TakeRandom(1)
                .Select(r => r.InductionExemptionReasonId)
                .ToArray()
            : [];

        var editInductionState = new EditInductionState
        {
            InductionStatus = inductionStatus,
            CurrentInductionStatus = inductionStatus,
            ExemptionReasonIds = exemptionReasonIds,
            StartDate = startDate,
            CompletedDate = completedDate,
            ChangeReason = PersonInductionChangeReason.AnotherReason,
            ChangeReasonDetail = ChangeReasonDetails,
            ProvideAdditionalInformation = false,
            AdditionalInformation = null,
            Evidence = CreateEvidence(false)
        };

        var person = await TestData.CreatePersonAsync(p => p.WithQts());

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            editInductionState,
            startPage);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/edit-induction/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var label = doc.QuerySelectorAll(".govuk-summary-list__key").Single(e => e.TrimmedText() == labelContent);
        Assert.NotNull(label);
        var value = label.NextElementSibling;
        Assert.NotNull(value);
        Assert.Equal(inductionStatus.GetTitle(), value.TrimmedText());
        if (showChangeLink)
        {
            Assert.NotNull(value.NextElementSibling!.GetElementsByTagName("a").First());
        }
        else
        {
            Assert.Null(value.NextElementSibling);
        }
    }

    [Theory]
    [InlineData(StartPage.Status, InductionStatus.InProgress, true, true)]
    [InlineData(StartPage.Status, InductionStatus.Passed, true, true)]
    [InlineData(StartPage.Status, InductionStatus.Failed, true, true)]
    [InlineData(StartPage.Status, InductionStatus.FailedInWales, true, true)]
    [InlineData(StartPage.Status, InductionStatus.Exempt, false, false)]
    [InlineData(StartPage.Status, InductionStatus.RequiredToComplete, false, false)]
    [InlineData(StartPage.StartDate, InductionStatus.InProgress, true, true)]
    [InlineData(StartPage.StartDate, InductionStatus.Passed, true, true)]
    [InlineData(StartPage.StartDate, InductionStatus.Failed, true, true)]
    [InlineData(StartPage.StartDate, InductionStatus.FailedInWales, true, true)]
    [InlineData(StartPage.CompletedDate, InductionStatus.Passed, true, false)]
    [InlineData(StartPage.CompletedDate, InductionStatus.Failed, true, false)]
    [InlineData(StartPage.CompletedDate, InductionStatus.FailedInWales, true, false)]
    [InlineData(StartPage.ExemptionReasons, InductionStatus.Exempt, false, false)]
    public async Task Get_ShowsStartDate_AsExpected(StartPage startPage, InductionStatus inductionStatus, bool showStartDateRow, bool showChangeLink)
    {
        // Arrange
        var labelContent = "Start date";

        DateOnly? startDate = inductionStatus.RequiresStartDate() ? TimeProvider.Today.AddYears(-2) : null;
        DateOnly? completedDate = inductionStatus.RequiresCompletedDate() ? TimeProvider.Today : null;

        var exemptionReasonIds = inductionStatus is InductionStatus.Exempt
            ? (await TestData.ReferenceDataCache.GetInductionExemptionReasonsAsync(activeOnly: true))
            .TakeRandom(1)
            .Select(r => r.InductionExemptionReasonId)
            .ToArray()
            : [];

        var editInductionState = new EditInductionState
        {
            InductionStatus = inductionStatus,
            CurrentInductionStatus = inductionStatus,
            ExemptionReasonIds = exemptionReasonIds,
            StartDate = startDate,
            CompletedDate = completedDate,
            ChangeReason = PersonInductionChangeReason.AnotherReason,
            ChangeReasonDetail = ChangeReasonDetails,
            ProvideAdditionalInformation = false,
            Evidence = CreateEvidence(false)
        };

        var person = await TestData.CreatePersonAsync(p => p.WithQts());

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            editInductionState,
            startPage);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/edit-induction/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        if (showStartDateRow)
        {
            var label = doc.QuerySelectorAll(".govuk-summary-list__key").Single(e => e.TrimmedText() == labelContent);
            Assert.NotNull(label.NextElementSibling);
            var value = label.NextElementSibling;
            Assert.Equal(startDate?.ToString(WebConstants.DateDisplayFormat), value!.TrimmedText());
            if (showChangeLink)
            {
                Assert.NotNull(value.NextElementSibling!.GetElementsByTagName("a").First());
            }
            else
            {
                Assert.Null(value.NextElementSibling);
            }
        }
        else
        {
            Assert.DoesNotContain(doc.QuerySelectorAll(".govuk-summary-list__key"), e => e.TrimmedText() == labelContent);
        }
    }

    [Theory]
    [InlineData(StartPage.Status, InductionStatus.InProgress, false)]
    [InlineData(StartPage.Status, InductionStatus.Passed, true)]
    [InlineData(StartPage.Status, InductionStatus.Failed, true)]
    [InlineData(StartPage.Status, InductionStatus.FailedInWales, true)]
    [InlineData(StartPage.Status, InductionStatus.Exempt, false)]
    [InlineData(StartPage.Status, InductionStatus.RequiredToComplete, false)]
    [InlineData(StartPage.StartDate, InductionStatus.InProgress, false)]
    [InlineData(StartPage.StartDate, InductionStatus.Passed, true)]
    [InlineData(StartPage.StartDate, InductionStatus.Failed, true)]
    [InlineData(StartPage.StartDate, InductionStatus.FailedInWales, true)]
    [InlineData(StartPage.CompletedDate, InductionStatus.Passed, true)]
    [InlineData(StartPage.CompletedDate, InductionStatus.Failed, true)]
    [InlineData(StartPage.CompletedDate, InductionStatus.FailedInWales, true)]
    [InlineData(StartPage.ExemptionReasons, InductionStatus.Exempt, false)]
    public async Task Get_ShowsCompletedDate_AsExpected(StartPage startPage, InductionStatus inductionStatus, bool ShowsCompletedDate)
    {
        // Arrange
        var labelContent = "Completion date";

        DateOnly? startDate = inductionStatus.RequiresStartDate() ? TimeProvider.Today.AddYears(-2) : null;
        DateOnly? completedDate = inductionStatus.RequiresCompletedDate() ? TimeProvider.Today : null;

        var exemptionReasonIds = inductionStatus is InductionStatus.Exempt
            ? (await TestData.ReferenceDataCache.GetInductionExemptionReasonsAsync(activeOnly: true))
            .TakeRandom(1)
            .Select(r => r.InductionExemptionReasonId)
            .ToArray()
            : [];

        var editInductionState = new EditInductionState
        {
            InductionStatus = inductionStatus,
            CurrentInductionStatus = inductionStatus,
            ExemptionReasonIds = exemptionReasonIds,
            StartDate = startDate,
            CompletedDate = completedDate,
            ChangeReason = PersonInductionChangeReason.AnotherReason,
            ChangeReasonDetail = ChangeReasonDetails,
            ProvideAdditionalInformation = false,
            Evidence = CreateEvidence(false)
        };

        var person = await TestData.CreatePersonAsync(p => p.WithQts());

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            editInductionState,
            startPage);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/edit-induction/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        if (ShowsCompletedDate)
        {
            var label = doc.QuerySelectorAll(".govuk-summary-list__key").Single(e => e.TrimmedText() == labelContent);
            Assert.NotNull(label);
            var value = label.NextElementSibling;
            Assert.Equal(completedDate?.ToString(WebConstants.DateDisplayFormat), value!.TrimmedText());
        }
        else
        {
            Assert.DoesNotContain(doc.QuerySelectorAll(".govuk-summary-list__key"), e => e.TrimmedText() == labelContent);
        }
    }

    [Theory]
    [InlineData(StartPage.Status, InductionStatus.InProgress, false)]
    [InlineData(StartPage.Status, InductionStatus.Passed, false)]
    [InlineData(StartPage.Status, InductionStatus.Failed, false)]
    [InlineData(StartPage.Status, InductionStatus.FailedInWales, false)]
    [InlineData(StartPage.Status, InductionStatus.Exempt, true)]
    [InlineData(StartPage.Status, InductionStatus.RequiredToComplete, false)]
    [InlineData(StartPage.StartDate, InductionStatus.InProgress, false)]
    [InlineData(StartPage.StartDate, InductionStatus.Passed, false)]
    [InlineData(StartPage.StartDate, InductionStatus.Failed, false)]
    [InlineData(StartPage.StartDate, InductionStatus.FailedInWales, false)]
    [InlineData(StartPage.CompletedDate, InductionStatus.Passed, false)]
    [InlineData(StartPage.CompletedDate, InductionStatus.Failed, false)]
    [InlineData(StartPage.CompletedDate, InductionStatus.FailedInWales, false)]
    [InlineData(StartPage.ExemptionReasons, InductionStatus.Exempt, true)]
    public async Task Get_ShowsExemptionReason_AsExpected(StartPage startPage, InductionStatus inductionStatus, bool ShowsExemptionReason)
    {
        // Arrange
        var labelContent = "Exemption reason";

        DateOnly? startDate = inductionStatus.RequiresStartDate() ? TimeProvider.Today.AddYears(-2) : null;
        DateOnly? completedDate = inductionStatus.RequiresCompletedDate() ? TimeProvider.Today : null;

        //var exemptionReasonIds = inductionStatus is InductionStatus.Exempt
        //    ? (await TestData.ReferenceDataCache.GetInductionExemptionReasonsAsync(activeOnly: true))
        //    .RandomSelection(2)
        //    .Select(r => r.InductionExemptionReasonId)
        //    .ToArray()
        //    : [];
        var exemptionReasonIds = inductionStatus is InductionStatus.Exempt
            ? new Guid[] { InductionExemptionReason.ExemptDataLossOrErrorCriteriaId, InductionExemptionReason.ExemptId }
            : [];

        var expectedReasons = (await TestData.ReferenceDataCache
            .GetInductionExemptionReasonsAsync(activeOnly: true))
            .Where(r => exemptionReasonIds.Contains(r.InductionExemptionReasonId))
            .Select(r => r.Name)
            .OrderByDescending(r => r)
            .ToArray();

        var editInductionState = new EditInductionState
        {
            InductionStatus = inductionStatus,
            CurrentInductionStatus = inductionStatus,
            ExemptionReasonIds = exemptionReasonIds,
            StartDate = startDate,
            CompletedDate = completedDate,
            ChangeReason = PersonInductionChangeReason.AnotherReason,
            ChangeReasonDetail = ChangeReasonDetails,
            ProvideAdditionalInformation = false,
            Evidence = CreateEvidence(false)
        };

        var person = await TestData.CreatePersonAsync(p => p.WithQts());

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            editInductionState,
            startPage);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/edit-induction/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        if (ShowsExemptionReason)
        {
            var label = doc.QuerySelectorAll(".govuk-summary-list__key").Single(e => e.TrimmedText() == labelContent);
            Assert.NotNull(label);
            var reasons = label.NextElementSibling!.QuerySelectorAll("li").Select(d => d.TrimmedText());
            Assert.NotEmpty(reasons);
            Assert.Equal(expectedReasons, reasons);
        }
        else
        {
            Assert.DoesNotContain(doc.QuerySelectorAll(".govuk-summary-list__key"), e => e.TrimmedText() == labelContent);
        }
    }

    [Fact]
    public async Task Cancel_RedirectsToExpectedPage()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithQts()
            .WithInductionStatus(s => s.
                WithStatus(InductionStatus.RequiredToComplete)));

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionState
            {
                InductionStatus = InductionStatus.RequiredToComplete,
                CurrentInductionStatus = InductionStatus.RequiredToComplete,
                ChangeReason = PersonInductionChangeReason.AnotherReason,
                ChangeReasonDetail = ChangeReasonDetails,
                ProvideAdditionalInformation = false,
                Evidence = CreateEvidence(false)
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/edit-induction/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseAsync(response);

        // Act
        // Cancelling is a field on the form rather than a handler of its own: a distinct URL
        // would be an invalid step for the journey.
        var redirectRequest = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/edit-induction/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder().Add("Cancel", bool.TrueString)
        };
        var redirectResponse = await HttpClient.SendAsync(redirectRequest);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)redirectResponse.StatusCode);
        var location = redirectResponse.Headers.Location?.OriginalString;
        Assert.Equal($"/persons/{person.PersonId}/induction", location);
    }

    [Fact]
    public async Task Get_ShowsChangeReason_AsExpected()
    {
        // Arrange
        var inductionStatus = InductionStatus.InProgress;
        var startDate = TimeProvider.Today.AddYears(-2);
        var completedDate = TimeProvider.Today;
        var exemptionReasonIds = Array.Empty<Guid>();

        var editInductionState = new EditInductionState
        {
            InductionStatus = inductionStatus,
            CurrentInductionStatus = inductionStatus,
            ExemptionReasonIds = exemptionReasonIds,
            StartDate = startDate,
            CompletedDate = completedDate,
            ChangeReason = PersonInductionChangeReason.AnotherReason,
            ChangeReasonDetail = ChangeReasonDetails,
            ProvideAdditionalInformation = true,
            AdditionalInformation = AdditionalInformation,
            Evidence = CreateEvidence(false)
        };

        var person = await TestData.CreatePersonAsync(p => p.WithQts());

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            editInductionState);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/edit-induction/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var label = doc.QuerySelectorAll(".govuk-summary-list__key").Single(e => e.TrimmedText() == "Reason for changing induction details");
        Assert.NotNull(label);
        var value = label.NextElementSibling;
        Assert.Equal(PersonInductionChangeReason.AnotherReason.GetDisplayName(), value!.TrimmedText());

        var labelDetails = doc.QuerySelectorAll(".govuk-summary-list__key").Single(e => e.TrimmedText() == "Reason details");
        Assert.NotNull(labelDetails);
        var valueDetails = labelDetails.NextElementSibling;
        Assert.Equal(ChangeReasonDetails, valueDetails!.TrimmedText());

        var reasonDetailsElement = doc.QuerySelectorAll(".govuk-summary-list__key").Single(e => e.TrimmedText() == "Additional information");
        Assert.NotNull(reasonDetailsElement);
        var reasonDetails = reasonDetailsElement.NextElementSibling;
        Assert.Equal(AdditionalInformation, reasonDetails!.TrimmedText());

        var labelFileUpload = doc.QuerySelectorAll(".govuk-summary-list__key").Single(e => e.TrimmedText() == "Evidence");
        Assert.NotNull(labelFileUpload);
        var valueFileUpload = labelFileUpload.NextElementSibling;
        Assert.Equal("Not provided", valueFileUpload!.TrimmedText());
    }

    [Fact]
    public async Task Post_InvalidCompletedDate_RedirectToCompletedDatePage()
    {
        // Arrange
        var inductionStatus = InductionStatus.RequiredToComplete;
        var startDate = TimeProvider.Today;
        var completedDate = startDate.AddYears(-2);
        var exemptionReasonIds = Array.Empty<Guid>();

        var person = await TestData.CreatePersonAsync(
            p => p
                .WithQts()
                .WithInductionStatus(i => i
                    .WithStatus(inductionStatus)));

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionState
            {
                InductionStatus = inductionStatus,
                CurrentInductionStatus = inductionStatus,
                ExemptionReasonIds = exemptionReasonIds,
                StartDate = startDate,
                CompletedDate = completedDate,
                ChangeReason = PersonInductionChangeReason.AnotherReason,
                ChangeReasonDetail = ChangeReasonDetails,
                ProvideAdditionalInformation = true,
                AdditionalInformation = AdditionalInformation,
                Evidence = CreateEvidence(false)
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/edit-induction/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        var location = response.Headers.Location?.OriginalString;

        Assert.StartsWith($"/persons/{person.PersonId}/edit-induction/date-completed?returnUrl=", location);
    }

    [Fact]
    public async Task Post_RedirectsToExpectedPage()
    {
        // Arrange
        var inductionStatus = InductionStatus.RequiredToComplete;
        var startDate = TimeProvider.Today.AddYears(-2);
        var completedDate = TimeProvider.Today;
        var exemptionReasonIds = (await TestData.ReferenceDataCache
            .GetInductionExemptionReasonsAsync(activeOnly: true))
            .TakeRandom(1)
            .Select(r => r.InductionExemptionReasonId)
            .ToArray();
        var person = await TestData.CreatePersonAsync(
            p => p
                .WithQts()
                .WithInductionStatus(i => i
                    .WithStatus(inductionStatus)));

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionState
            {
                InductionStatus = inductionStatus,
                CurrentInductionStatus = inductionStatus,
                ExemptionReasonIds = exemptionReasonIds,
                StartDate = startDate,
                CompletedDate = completedDate,
                ChangeReason = PersonInductionChangeReason.AnotherReason,
                ChangeReasonDetail = ChangeReasonDetails,
                ProvideAdditionalInformation = true,
                AdditionalInformation = AdditionalInformation,
                Evidence = CreateEvidence(false)
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/edit-induction/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        var location = response.Headers.Location?.OriginalString;

        Assert.Equal($"/persons/{person.PersonId}/induction", location);
    }

    [Fact]
    public async Task Post_Confirm_UpdatesPersonInductionCreatesEventCompletesJourneyAndRedirectsWithFlashMessage()
    {
        // Arrange
        var inductionStatus = InductionStatus.InProgress;

        DateOnly? startDate = inductionStatus.RequiresStartDate() ? TimeProvider.Today.AddYears(-2) : null;
        DateOnly? completedDate = inductionStatus.RequiresCompletedDate() ? TimeProvider.Today : null;

        var exemptionReasonIds = Array.Empty<Guid>();

        var person = await TestData.CreatePersonAsync(
            p => p
                .WithQts()
                .WithInductionStatus(i => i
                    .WithStatus(InductionStatus.RequiredToComplete)));

        // Confirming deletes the journey, so keep the answers it was seeded with to assert against.
        var state = new EditInductionState
        {
            InductionStatus = inductionStatus,
            CurrentInductionStatus = inductionStatus,
            ExemptionReasonIds = exemptionReasonIds,
            StartDate = startDate,
            CompletedDate = completedDate,
            ChangeReason = PersonInductionChangeReason.AnotherReason,
            ChangeReasonDetail = ChangeReasonDetails,
            ProvideAdditionalInformation = true,
            AdditionalInformation = AdditionalInformation,
            Evidence = CreateEvidence(true)
        };

        var journeyInstance = await CreateJourneyInstanceAsync(person.PersonId, state);

        EventObserver.Clear();

        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/edit-induction/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var redirectResponse = await response.FollowRedirectAsync(HttpClient);
        var redirectDoc = await redirectResponse.GetDocumentAsync();
        AssertEx.HtmlDocumentHasFlashNotificationBanner(redirectDoc, "Induction details have been updated");

        await WithDbContextAsync(async dbContext =>
        {
            var updatedPersonRecord = await dbContext.Persons.FirstOrDefaultAsync(p => p.PersonId == person.PersonId);
            Assert.Equal(state.InductionStatus, updatedPersonRecord!.InductionStatus);
            Assert.Equal(state.StartDate, updatedPersonRecord!.InductionStartDate);
            Assert.Equal(state.CompletedDate, updatedPersonRecord!.InductionCompletedDate);
            Assert.Equal(state.ExemptionReasonIds, updatedPersonRecord!.InductionExemptionReasonIds);
        });

        Events.AssertProcessesCreated(p =>
        {
            Assert.Equal(ProcessType.PersonInductionUpdating, p.ProcessContext.ProcessType);
            Assert.Equal(GetCurrentUserId(), p.ProcessContext.UserId);

            var changeReason = p.ProcessContext.Process.ChangeReason as ChangeReasonWithDetailsAndEvidence;
            Assert.Equal(state.ChangeReason!.GetDisplayName(), changeReason?.Reason);
            Assert.Equal(state.ChangeReasonDetail, changeReason?.Details);
            Assert.Equal(state.AdditionalInformation, changeReason?.AdditionalInformation);
            Assert.Equal(state.Evidence.UploadedEvidenceFile!.FileId, changeReason?.EvidenceFile?.FileId);
            Assert.Equal(state.Evidence.UploadedEvidenceFile!.FileName, changeReason?.EvidenceFile?.Name);

            p.AssertProcessHasEvents<PersonInductionUpdatedEvent>(actualInductionUpdatedEvent =>
            {
                Assert.Equal(person.PersonId, actualInductionUpdatedEvent.PersonId);
                Assert.Equal(state.InductionStatus, actualInductionUpdatedEvent.Induction.Status);
                Assert.Equal(state.StartDate, actualInductionUpdatedEvent.Induction.StartDate);
                Assert.Equal(state.CompletedDate, actualInductionUpdatedEvent.Induction.CompletedDate);
                Assert.Equal(state.ExemptionReasonIds, actualInductionUpdatedEvent.Induction.ExemptionReasonIds);
            });
        });

        Assert.Null(GetJourneyInstanceState(journeyInstance));
    }

    [Theory]
    [InlineData(PersonInductionChangeReason.AnotherReason, "this is a reason")]
    [InlineData(PersonInductionChangeReason.IncompleteDetails, null)]
    [InlineData(PersonInductionChangeReason.NewInformation, null)]
    public async Task Get_ShowReasonDetails_AsExpected(PersonInductionChangeReason reason, string? changeReasonDetail)
    {
        // Arrange
        var inductionStatus = InductionStatus.InProgress;
        var startDate = TimeProvider.Today.AddYears(-2);
        var completedDate = TimeProvider.Today;
        var exemptionReasonIds = Array.Empty<Guid>();

        var editInductionState = new EditInductionState
        {
            InductionStatus = inductionStatus,
            CurrentInductionStatus = inductionStatus,
            ExemptionReasonIds = exemptionReasonIds,
            StartDate = startDate,
            CompletedDate = completedDate,
            ChangeReason = reason,
            ChangeReasonDetail = changeReasonDetail,
            ProvideAdditionalInformation = true,
            AdditionalInformation = AdditionalInformation,
            Evidence = CreateEvidence(false)
        };

        var person = await TestData.CreatePersonAsync(p => p.WithQts());

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            editInductionState);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/edit-induction/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var label = doc.QuerySelectorAll(".govuk-summary-list__key").Single(e => e.TrimmedText() == "Reason");
        Assert.NotNull(label);

        var valueDetails = label.NextElementSibling;
        Assert.Equal(reason.GetDisplayName(), valueDetails!.TrimmedText());
    }

    [Theory]
    [InlineData(true, "this is some details", "this is some details")]
    [InlineData(false, null, "")]
    public async Task Get_ShowAdditionalInformation_AsExpected(bool addDetail, string? details, string? expectedDetails)
    {
        // Arrange
        var inductionStatus = InductionStatus.InProgress;
        var startDate = TimeProvider.Today.AddYears(-2);
        var completedDate = TimeProvider.Today;
        var exemptionReasonIds = Array.Empty<Guid>();

        var editInductionState = new EditInductionState
        {
            InductionStatus = inductionStatus,
            CurrentInductionStatus = inductionStatus,
            ExemptionReasonIds = exemptionReasonIds,
            StartDate = startDate,
            CompletedDate = completedDate,
            ChangeReason = PersonInductionChangeReason.AnotherReason,
            ChangeReasonDetail = ChangeReasonDetails,
            ProvideAdditionalInformation = addDetail,
            AdditionalInformation = details,
            Evidence = CreateEvidence(false)
        };

        var person = await TestData.CreatePersonAsync(p => p.WithQts());

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            editInductionState);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/edit-induction/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var label = doc.QuerySelectorAll(".govuk-summary-list__key").Single(e => e.TrimmedText() == "Additional information");
        Assert.NotNull(label);

        var valueDetails = label.NextElementSibling;
        Assert.Equal(expectedDetails, valueDetails!.TrimmedText());
    }

    [Theory]
    [InlineData(true, "evidence.jpeg (opens in new tab)")]
    [InlineData(false, "Not provided")]
    public async Task Get_ShowAttachment_AsExpected(bool uploadFile, string? expectedString)
    {
        // Arrange
        var evidenceFileId = uploadFile == true ? Guid.NewGuid() : default(Guid?);
        var inductionStatus = InductionStatus.InProgress;
        var startDate = TimeProvider.Today.AddYears(-2);
        var completedDate = TimeProvider.Today;
        var exemptionReasonIds = Array.Empty<Guid>();

        var editInductionState = new EditInductionState
        {
            InductionStatus = inductionStatus,
            CurrentInductionStatus = inductionStatus,
            ExemptionReasonIds = exemptionReasonIds,
            StartDate = startDate,
            CompletedDate = completedDate,
            ChangeReason = PersonInductionChangeReason.AnotherReason,
            ChangeReasonDetail = ChangeReasonDetails,
            ProvideAdditionalInformation = true,
            AdditionalInformation = ChangeReasonDetails,
            Evidence = CreateEvidence(uploadFile, evidenceFileId)
        };

        var person = await TestData.CreatePersonAsync(p => p.WithQts());

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            editInductionState);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/edit-induction/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var label = doc.QuerySelectorAll(".govuk-summary-list__key").Single(e => e.TrimmedText() == "Evidence");
        Assert.NotNull(label);

        var valueDetails = label.NextElementSibling;
        Assert.Equal(expectedString, valueDetails!.TrimmedText());
    }

}
