using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.Persons;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.EditInduction;

public class EditInductionStatusTests(HostFixture hostFixture) : EditInductionTestBase(hostFixture)
{
    [Fact]
    public async Task Get_PageLegend_Expected()
    {
        // Arrange
        InductionStatus inductionStatus = InductionStatus.Passed;
        var person = await TestData.CreatePersonAsync(p => p
            .WithQts()
            .WithFirstName("Alfred")
            .WithMiddleName("The")
            .WithLastName("Great"));
        var expectedCaption = "Edit induction details - Alfred The Great";

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionState
            {
                InductionStatus = inductionStatus,
                CurrentInductionStatus = inductionStatus
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/edit-induction/status?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var caption = doc.GetElementByTestId("induction-status-caption");
        Assert.Equal(expectedCaption, caption!.TrimmedText());
    }

    [Fact]
    public async Task Get_ContinueAndCancelButtons_ExistOnPage()
    {
        // Arrange
        InductionStatus inductionStatus = InductionStatus.Passed;
        var person = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionState
            {
                InductionStatus = inductionStatus,
                CurrentInductionStatus = inductionStatus
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/edit-induction/status?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var form = doc.GetElementByTestId("submit-form") as IHtmlFormElement;
        Assert.NotNull(form);
        var buttons = form.GetElementsByTagName("button").Select(button => button as IHtmlButtonElement);
        Assert.Equal(2, buttons.Count());
        Assert.Equal("Continue", buttons.ElementAt(0)!.TrimmedText());
        Assert.Equal("Cancel and return to record", buttons.ElementAt(1)!.TrimmedText());
    }

    [Fact]
    public async Task Get_InductionNotManagedByCpd_ExpectedRadioButtonsExistOnPage()
    {
        // Arrange
        InductionStatus currentInductionStatus = InductionStatus.InProgress;
        var expectedStatuses = new InductionStatus[] { InductionStatus.RequiredToComplete, InductionStatus.Exempt, InductionStatus.InProgress, InductionStatus.Passed, InductionStatus.Failed, InductionStatus.FailedInWales };
        var expectedChoices = expectedStatuses.Select(s => s.ToString());

        var person = await TestData.CreatePersonAsync(p => p.WithQts());

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionState
            {
                InductionStatus = currentInductionStatus,
                CurrentInductionStatus = currentInductionStatus
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/edit-induction/status?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var statusChoices = doc.QuerySelectorAll<IHtmlInputElement>("[type=radio]").Select(r => r.Value);
        var statusChoicesLegend = doc.GetElementByTestId("status-choices-legend");
        Assert.Equal("Select an induction status", statusChoicesLegend!.TrimmedText());
        Assert.Equal(expectedChoices, statusChoices);
    }

    [Theory]
    [InlineData(InductionStatus.Passed)]
    [InlineData(InductionStatus.InProgress)]
    [InlineData(InductionStatus.RequiredToComplete)]
    [InlineData(InductionStatus.Failed)]
    public async Task Get_InductionManagedByCpd_ExpectedRadioButtonsExistOnPage(InductionStatus currentInductionStatus)
    {
        // Arrange
        InductionStatus[] expectedStatuses = new List<InductionStatus> { InductionStatus.Exempt, InductionStatus.FailedInWales, currentInductionStatus }.OrderBy(i => i).ToArray();
        var expectedChoices = expectedStatuses.Select(s => s.ToString());
        var lessThanSevenYearsAgo = TimeProvider.Today.AddYears(-1);
        var person = await TestData.CreatePersonAsync(p => p.WithQts());
        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(person);
            person.SetCpdInductionStatus(
                InductionStatus.Passed,
                startDate: lessThanSevenYearsAgo.AddYears(-1),
                completedDate: lessThanSevenYearsAgo,
                cpdModifiedOn: TimeProvider.UtcNow,
                updatedBy: SystemUser.SystemUserId,
                now: TimeProvider.UtcNow,
                out _);
            await dbContext.SaveChangesAsync();
        });

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionState
            {
                InductionStatus = currentInductionStatus,
                CurrentInductionStatus = currentInductionStatus
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/edit-induction/status?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var statusChoices = doc.QuerySelectorAll<IHtmlInputElement>("[type=radio]").Select(r => r.Value);
        var statusChoicesLegend = doc.GetElementByTestId("status-choices-legend");
        Assert.Equal("Select an induction status", statusChoicesLegend!.TrimmedText());
        Assert.Equal(expectedChoices, statusChoices);
    }

    [Theory]
    [InlineData(InductionStatus.Exempt)]
    [InlineData(InductionStatus.FailedInWales)]
    public async Task Get_InductionManagedByCpd_StatusExemptOrFailedInWales_ExpectedRadioButtonsExistOnPage(InductionStatus status)
    {
        // Arrange
        var expectedStatuses = new InductionStatus[] { InductionStatus.RequiredToComplete, InductionStatus.Exempt, InductionStatus.InProgress, InductionStatus.Passed, InductionStatus.Failed, InductionStatus.FailedInWales };
        var expectedChoices = expectedStatuses.Select(s => s.ToString());

        // test setup here is convoluted because I need to set up a person,
        // then call SetCpdInductionstatus to set the CpdInductionModifiedOn date,
        // then set the induction status to the one being tested
        var person = await TestData.CreatePersonAsync(
            p => p.WithQts());
        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(person);
            person.SetCpdInductionStatus(
                InductionStatus.RequiredToComplete, // CPD induction status can't be Exempt or FailedInWales
                startDate: null,
                completedDate: null,
                cpdModifiedOn: TimeProvider.UtcNow,
                updatedBy: SystemUser.SystemUserId,
                now: TimeProvider.UtcNow,
                out _);
            person.SetInductionStatus(
                status,
                startDate: null,
                completedDate: null,
                exemptionReasonIds: [],
                changeReason: null,
                changeReasonDetail: null,
                evidenceFile: null,
                updatedBy: SystemUser.SystemUserId,
                now: TimeProvider.UtcNow,
                additionalInformation: null,
                out _);
            await dbContext.SaveChangesAsync();
        });

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionState
            {
                InductionStatus = status,
                CurrentInductionStatus = status
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/edit-induction/status?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var statusChoices = doc.QuerySelectorAll<IHtmlInputElement>("[type=radio]").Select(r => r.Value);
        Assert.Equal(expectedChoices, statusChoices);
    }

    [Fact]
    public async Task Get_InductionStatus_ShowsSelectedRadioButton()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithQts());
        var currentInductionStatus = InductionStatus.RequiredToComplete;
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionState
            {
                InductionStatus = currentInductionStatus,
                CurrentInductionStatus = currentInductionStatus
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/edit-induction/status?returnUrl={Uri.EscapeDataString($"/persons/{person.PersonId}/edit-induction/check-answers")}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var selectedStatus = doc.QuerySelectorAll<IHtmlInputElement>("[type=radio]").Single(r => r.IsChecked);
        Assert.Equal(currentInductionStatus.ToString(), selectedStatus.Value);
    }

    [Fact]
    public async Task Post_SelectedStatus_PersistsStatus()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithQts());

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionState
            {
                InductionStatus = InductionStatus.Passed,
                CurrentInductionStatus = InductionStatus.Passed
            });

        var postRequest = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/edit-induction/status?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new EditInductionPostRequestContentBuilder()
                .WithInductionStatus(InductionStatus.Exempt)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        var state = GetJourneyInstanceState(journeyInstance)!;
        Assert.Equal("Exempt", state.InductionStatus.GetTitle());
    }

    [Fact]
    public async Task Post_NoSelectedStatus_ShowsPageError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithQts());

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionState
            {
                InductionStatus = InductionStatus.RequiredToComplete,
                CurrentInductionStatus = InductionStatus.RequiredToComplete
            });

        var postRequest = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/edit-induction/status?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new EditInductionPostRequestContentBuilder()
                .WithInductionStatus(InductionStatus.None)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(StatusModel.InductionStatus), "Select a status");
    }

    [Fact]
    public async Task Post_PersonManagedByCpd_NoSelectedStatus_ShowsPageError()
    {
        var lessThanSevenYearsAgo = TimeProvider.Today.AddYears(-1);
        var person = await TestData.CreatePersonAsync(p => p.WithQts());
        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(person);
            person.SetCpdInductionStatus(
                InductionStatus.Passed,
                startDate: lessThanSevenYearsAgo.AddYears(-1),
                completedDate: lessThanSevenYearsAgo,
                cpdModifiedOn: TimeProvider.UtcNow,
                updatedBy: SystemUser.SystemUserId,
                now: TimeProvider.UtcNow,
                out _);
            await dbContext.SaveChangesAsync();
        });
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionState
            {
                InductionStatus = InductionStatus.Passed,
                CurrentInductionStatus = InductionStatus.Passed
            });

        var postRequest = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/edit-induction/status?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new EditInductionPostRequestContentBuilder()
                .WithInductionStatus(InductionStatus.None)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(StatusModel.InductionStatus), "Select a status");
    }


    [Theory]
    [InlineData(InductionStatus.RequiredToComplete, "passed, failed, or in progress")]
    [InlineData(InductionStatus.InProgress, "required to complete, passed, or failed")]
    [InlineData(InductionStatus.Passed, "required to complete, failed, or in progress")]
    [InlineData(InductionStatus.Failed, "required to complete, passed, or in progress")]
    public async Task Get_ForPersonWithInductionStatusManagedByCPD_ShowsWarning(InductionStatus status, string statusSpecificText)
    {
        //Arrange
        var lessThanSevenYearsAgo = TimeProvider.Today.AddYears(-1);

        // test setup here is convoluted because I need to set up a person,
        // then call SetCpdInductionstatus to set the CpdInductionModifiedOn date,
        // then set the induction status to the one being tested
        var person = await TestData.CreatePersonAsync(
            p => p.WithQts());

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(person);

            // Force status to `None` so that the SetCpdInductionStatus() call below always has a change to status
            person.UnsafeSetInductionStatus(
                InductionStatus.None,
                InductionStatus.None,
                startDate: null,
                completedDate: null,
                exemptionReasonIds: []);

            person.SetCpdInductionStatus(
                InductionStatus.RequiredToComplete, // CPD induction status can't be Exempt or FailedInWales
                startDate: null,
                completedDate: null,
                cpdModifiedOn: TimeProvider.UtcNow,
                updatedBy: SystemUser.SystemUserId,
                now: TimeProvider.UtcNow,
                out _);

            person.SetInductionStatus(
                status,
                startDate: null,
                completedDate: null,
                exemptionReasonIds: [],
                changeReason: null,
                changeReasonDetail: null,
                evidenceFile: null,
                updatedBy: SystemUser.SystemUserId,
                now: TimeProvider.UtcNow,
                additionalInformation: null,
                out _);

            await dbContext.SaveChangesAsync();
        });

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionState
            {
                InductionStatus = status,
                CurrentInductionStatus = status
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/edit-induction/status?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Contains(statusSpecificText, doc!.GetElementByTestId("induction-status-warning")!.Children[1].TrimmedText());
    }

    [Theory]
    [InlineData(InductionStatus.FailedInWales)]
    [InlineData(InductionStatus.Exempt)]
    public async Task Get_ForPersonWithInductionStatusManagedByCPD_StatusExemptOrFailedInWales_NoWarning(InductionStatus status)
    {
        //Arrange
        var lessThanSevenYearsAgo = TimeProvider.Today.AddYears(-1);

        // test setup here is convoluted because I need to set up a person,
        // then call SetCpdInductionstatus to set the CpdInductionModifiedOn date,
        // then set the induction status to the one being tested
        var person = await TestData.CreatePersonAsync(
            p => p.WithQts());
        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(person);
            person.SetCpdInductionStatus(
                InductionStatus.RequiredToComplete, // CPD induction status can't be Exempt or FailedInWales
                startDate: null,
                completedDate: null,
                cpdModifiedOn: TimeProvider.UtcNow,
                updatedBy: SystemUser.SystemUserId,
                now: TimeProvider.UtcNow,
                out _);
            person.SetInductionStatus(
                status,
                startDate: null,
                completedDate: null,
                exemptionReasonIds: [],
                changeReason: null,
                changeReasonDetail: null,
                evidenceFile: null,
                updatedBy: SystemUser.SystemUserId,
                now: TimeProvider.UtcNow,
                additionalInformation: null,
                out _);
            await dbContext.SaveChangesAsync();
        });
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionState
            {
                InductionStatus = status,
                CurrentInductionStatus = status
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/edit-induction/status?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Null(doc!.GetElementByTestId("induction-status-warning"));
    }

    [Theory]
    [InlineData(InductionStatus.Exempt, "edit-induction/exemption-reasons")]
    [InlineData(InductionStatus.InProgress, "edit-induction/start-date")]
    [InlineData(InductionStatus.Failed, "edit-induction/start-date")]
    [InlineData(InductionStatus.FailedInWales, "edit-induction/start-date")]
    [InlineData(InductionStatus.Passed, "edit-induction/start-date")]
    [InlineData(InductionStatus.RequiredToComplete, "edit-induction/reason")]
    public async Task Post_RedirectsToExpectedPage(InductionStatus inductionStatus, string expectedNextPageUrl)
    {
        // Arrange
        var exemptionReasonIds = new Guid[] { InductionExemptionReason.ExemptId };
        var person = await TestData.CreatePersonAsync(p => p
            .WithQts()
            .WithInductionStatus(i => i.WithStatus(inductionStatus)));

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionState
            {
                InductionStatus = inductionStatus,
                CurrentInductionStatus = inductionStatus,
                StartDate = TimeProvider.Today.AddYears(-2)
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/edit-induction/status?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new EditInductionPostRequestContentBuilder()
                .WithInductionStatus(inductionStatus)
                .WithExemptionReasonIds(exemptionReasonIds)
                .WithStartDate(TimeProvider.Today.AddDays(-1))
                .WithCompletedDate(TimeProvider.Today)
                .WithChangeReason(PersonInductionChangeReason.IncompleteDetails)
                .WithProvideAdditionalInformation(false)
                .WithUploadEvidence(false)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/persons/{person.PersonId}/{expectedNextPageUrl}?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_Cancel_DeletesJourneyAndRedirectsToInduction()
    {
        // Arrange
        var inductionStatus = InductionStatus.InProgress;
        var person = await TestData.CreatePersonAsync(p => p
            .WithQts()
            .WithInductionStatus(s => s.WithStatus(inductionStatus)));

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionState
            {
                InductionStatus = inductionStatus,
                CurrentInductionStatus = inductionStatus,
                StartDate = TimeProvider.Today.AddYears(-2),
                CompletedDate = TimeProvider.Today,
                ChangeReason = PersonInductionChangeReason.AnotherReason,
                ProvideAdditionalInformation = true,
                AdditionalInformation = "Details",
                Evidence = CreateEvidence(false)
            });

        // Act
        // Cancelling is a field on the form rather than a handler of its own: a distinct URL would be
        // an invalid step for the journey.
        var response = await HttpClient.SendAsync(new HttpRequestMessage(
            HttpMethod.Post,
            $"/persons/{person.PersonId}/edit-induction/status?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder().Add("Cancel", bool.TrueString)
        });

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/persons/{person.PersonId}/induction", response.Headers.Location?.OriginalString);
        Assert.Null(GetJourneyInstanceState(journeyInstance));
    }

    [Fact]
    public async Task Post_Cancel_EvidenceFilePreviouslyUploaded_DeletesPreviouslyUploadedFile()
    {
        // Arrange
        var inductionStatus = InductionStatus.InProgress;
        var evidenceFileId = Guid.NewGuid();
        var person = await TestData.CreatePersonAsync(p => p
            .WithQts()
            .WithInductionStatus(s => s.WithStatus(inductionStatus)));

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionState
            {
                InductionStatus = inductionStatus,
                CurrentInductionStatus = inductionStatus,
                StartDate = TimeProvider.Today.AddYears(-2),
                CompletedDate = TimeProvider.Today,
                ChangeReason = PersonInductionChangeReason.AnotherReason,
                ProvideAdditionalInformation = true,
                AdditionalInformation = "Details",
                Evidence = CreateEvidence(true, evidenceFileId)
            });

        // Act
        var response = await HttpClient.SendAsync(new HttpRequestMessage(
            HttpMethod.Post,
            $"/persons/{person.PersonId}/edit-induction/status?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder().Add("Cancel", bool.TrueString)
        });

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        FileServiceMock.AssertFileWasDeleted(evidenceFileId);
    }

    [Theory]
    [InlineData(StartPage.Status, InductionStatus.Exempt, "induction")]
    [InlineData(StartPage.Status, InductionStatus.RequiredToComplete, "induction")]
    public async Task Get_BackLinkContainsExpected(StartPage startPage, InductionStatus inductionStatus, string expectedBackPage)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithQts());

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionState
            {
                InductionStatus = inductionStatus,
                CurrentInductionStatus = inductionStatus,
                StartDate = TimeProvider.Today.AddYears(-2),
                CompletedDate = TimeProvider.Today
            },
            startPage);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/edit-induction/status?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var document = await response.GetDocumentAsync();
        var backlink = document.GetElementByTestId("back-link") as IHtmlAnchorElement;
        Assert.Contains($"/persons/{person.PersonId}/{expectedBackPage}", backlink!.Href);
    }

    [Fact]
    public async Task Get_FromCheckAnswers_BackLinkReturnsToCheckAnswers()
    {
        // Arrange
        var inductionStatus = InductionStatus.Exempt;
        var exemptionReasonIds = new Guid[] { InductionExemptionReason.ExemptId };
        var person = await TestData.CreatePersonAsync(p => p.WithQts());

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionState
            {
                InductionStatus = inductionStatus,
                CurrentInductionStatus = inductionStatus,
                ExemptionReasonIds = exemptionReasonIds,
                StartDate = new DateOnly(2000, 2, 2),
                CompletedDate = new DateOnly(2002, 2, 2),
                ChangeReason = PersonInductionChangeReason.AnotherReason
            });

        var checkAnswersUrl = $"/persons/{person.PersonId}/edit-induction/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}";

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/persons/{person.PersonId}/edit-induction/status?returnUrl={Uri.EscapeDataString(checkAnswersUrl)}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var document = await response.GetDocumentAsync();
        var backlink = document.GetElementByTestId("back-link") as IHtmlAnchorElement;
        Assert.Contains($"/persons/{person.PersonId}/edit-induction/check-answers", backlink!.Href);
    }

    [Theory]
    // The status is the exception: the answers that follow depend on it, so it always walks the user
    // forward through them rather than back to check answers.
    [InlineData(InductionStatus.Exempt, "edit-induction/exemption-reasons")]
    public async Task Post_FromCheckAnswers_RedirectsToExpectedPage(InductionStatus inductionStatus, string expectedNextPageUrl)
    {
        // Arrange
        var startDate = new DateOnly(2000, 2, 1);
        var completedDate = new DateOnly(2002, 2, 2);
        var exemptionReasonIds = new Guid[] { InductionExemptionReason.ExemptId };

        var person = await TestData.CreatePersonAsync(p => p
            .WithQts()
            .WithInductionStatus(i => i.WithStatus(inductionStatus)));

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionState
            {
                InductionStatus = inductionStatus,
                CurrentInductionStatus = inductionStatus,
                ExemptionReasonIds = exemptionReasonIds,
                StartDate = startDate,
                CompletedDate = completedDate,
                ChangeReason = PersonInductionChangeReason.AnotherReason
            });

        var checkAnswersUrl = $"/persons/{person.PersonId}/edit-induction/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}";

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/persons/{person.PersonId}/edit-induction/status?returnUrl={Uri.EscapeDataString(checkAnswersUrl)}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new EditInductionPostRequestContentBuilder()
                .WithInductionStatus(inductionStatus)
                .WithExemptionReasonIds(exemptionReasonIds)
                .WithStartDate(startDate)
                .WithCompletedDate(completedDate)
                .WithChangeReason(PersonInductionChangeReason.IncompleteDetails)
                .WithProvideAdditionalInformation(false)
                .WithUploadEvidence(false)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/persons/{person.PersonId}/{expectedNextPageUrl}?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_Status_FromCheckAnswers_WhenNewStatusChangesTheQuestionsAsked_WalksForwardToTheNextQuestion()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithQts()
            .WithInductionStatus(s => s.WithStatus(InductionStatus.RequiredToComplete)));

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionState
            {
                InductionStatus = InductionStatus.RequiredToComplete,
                CurrentInductionStatus = InductionStatus.RequiredToComplete,
                ChangeReason = PersonInductionChangeReason.AnotherReason,
                ChangeReasonDetail = "A reason",
                ProvideAdditionalInformation = false,
                Evidence = CreateEvidence(false)
            });

        var checkAnswersUrl = $"/persons/{person.PersonId}/edit-induction/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}";

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/persons/{person.PersonId}/edit-induction/status?returnUrl={Uri.EscapeDataString(checkAnswersUrl)}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new EditInductionPostRequestContentBuilder()
                .WithInductionStatus(InductionStatus.Exempt)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        // Exempt asks a question the journey hasn't been through, so the user is sent forward to it
        // rather than back to check answers.
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith(
            $"/persons/{person.PersonId}/edit-induction/exemption-reasons",
            response.Headers.Location?.OriginalString);
    }
}
