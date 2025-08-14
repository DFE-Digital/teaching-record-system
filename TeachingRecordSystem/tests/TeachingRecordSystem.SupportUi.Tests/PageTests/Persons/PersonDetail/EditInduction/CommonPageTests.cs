using AngleSharp.Html.Dom;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.EditInduction;

public class CommonPageTests : TestBase
{
    public CommonPageTests(HostFixture hostFixture) : base(hostFixture)
    {
        FileServiceMock.Invocations.Clear();
    }

    [Theory]
    [InlineData("edit-induction/status", InductionStatus.Exempt, "edit-induction/exemption-reasons")]
    [InlineData("edit-induction/status", InductionStatus.InProgress, "edit-induction/start-date")]
    [InlineData("edit-induction/status", InductionStatus.Failed, "edit-induction/start-date")]
    [InlineData("edit-induction/status", InductionStatus.FailedInWales, "edit-induction/start-date")]
    [InlineData("edit-induction/status", InductionStatus.Passed, "edit-induction/start-date")]
    [InlineData("edit-induction/status", InductionStatus.RequiredToComplete, "edit-induction/change-reason")]
    [InlineData("edit-induction/exemption-reasons", InductionStatus.Exempt, "edit-induction/change-reason")]
    [InlineData("edit-induction/start-date", InductionStatus.InProgress, "edit-induction/change-reason")]
    [InlineData("edit-induction/start-date", InductionStatus.Failed, "edit-induction/date-completed")]
    [InlineData("edit-induction/start-date", InductionStatus.FailedInWales, "edit-induction/date-completed")]
    [InlineData("edit-induction/start-date", InductionStatus.Passed, "edit-induction/date-completed")]
    [InlineData("edit-induction/date-completed", InductionStatus.Failed, "edit-induction/change-reason")]
    [InlineData("edit-induction/date-completed", InductionStatus.FailedInWales, "edit-induction/change-reason")]
    [InlineData("edit-induction/date-completed", InductionStatus.Passed, "edit-induction/change-reason")]
    [InlineData("edit-induction/change-reason", InductionStatus.Exempt, "edit-induction/check-answers")]
    [InlineData("edit-induction/change-reason", InductionStatus.InProgress, "edit-induction/check-answers")]
    [InlineData("edit-induction/change-reason", InductionStatus.Failed, "edit-induction/check-answers")]
    [InlineData("edit-induction/change-reason", InductionStatus.FailedInWales, "edit-induction/check-answers")]
    [InlineData("edit-induction/change-reason", InductionStatus.Passed, "edit-induction/check-answers")]
    [InlineData("edit-induction/change-reason", InductionStatus.RequiredToComplete, "edit-induction/check-answers")]
    public async Task Post_RedirectsToExpectedPage(string fromPage, InductionStatus inductionStatus, string expectedNextPageUrl)
    {
        // Arrange
        var exemptionReasonIds = new Guid[] { InductionExemptionReason.ExemptId };
        var person = await TestData.CreatePersonAsync(
            p => p
                .WithQts()
                .WithInductionStatus(i => i
                    .WithStatus(inductionStatus)));

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionStateBuilder()
                .WithInitializedState(inductionStatus, InductionJourneyPage.Status)
                .WithStartDate(Clock.Today.AddYears(-2))
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/{fromPage}?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new EditInductionPostRequestContentBuilder()
                .WithInductionStatus(inductionStatus)
                .WithExemptionReasonIds(exemptionReasonIds)
                .WithStartDate(Clock.Today.AddDays(-1))
                .WithCompletedDate(Clock.Today)
                .WithChangeReason(InductionChangeReasonOption.IncompleteDetails)
                .WithChangeReasonDetailSelections(false)
                .WithEvidence(false)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        var location = response.Headers.Location?.OriginalString;
        var expectedUrl = expectedNextPageUrl == "induction"
            ? $"/persons/{person.PersonId}/{expectedNextPageUrl}"
            : $"/persons/{person.PersonId}/{expectedNextPageUrl}?{journeyInstance.GetUniqueIdQueryParameter()}";
        Assert.Equal(expectedUrl, location);
    }

    [Theory]
    [InlineData("edit-induction/status", InductionStatus.InProgress)]
    [InlineData("edit-induction/exemption-reasons", InductionStatus.Exempt)]
    [InlineData("edit-induction/start-date", InductionStatus.Passed)]
    [InlineData("edit-induction/date-completed", InductionStatus.Passed)]
    [InlineData("edit-induction/change-reason", InductionStatus.InProgress)]
    public async Task Post_Cancel_DeletesJourneyAndRedirectsToExpectedPage(string fromPage, InductionStatus inductionStatus)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithQts()
            .WithInductionStatus(s => s.
                WithStatus(inductionStatus)));

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionStateBuilder()
                .WithInitializedState(inductionStatus, InductionJourneyPage.Status)
                .WithStartDate(Clock.Today.AddYears(-2))
                .WithCompletedDate(Clock.Today)
                .WithReasonChoice(InductionChangeReasonOption.AnotherReason)
                .WithReasonDetailsChoice(true, "Details")
                .WithFileUploadChoice(false)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/{fromPage}?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var cancelButton = doc.GetElementByTestId("cancel-button") as IHtmlButtonElement;

        // Act
        var redirectRequest = new HttpRequestMessage(HttpMethod.Post, cancelButton!.FormAction);
        var redirectResponse = await HttpClient.SendAsync(redirectRequest);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)redirectResponse.StatusCode);
        var location = redirectResponse.Headers.Location?.OriginalString;
        Assert.Equal($"/persons/{person.PersonId}/induction", location);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Null(journeyInstance);
    }

    [Theory]
    [InlineData("edit-induction/status", InductionStatus.InProgress)]
    [InlineData("edit-induction/exemption-reasons", InductionStatus.Exempt)]
    [InlineData("edit-induction/start-date", InductionStatus.Passed)]
    [InlineData("edit-induction/date-completed", InductionStatus.Passed)]
    [InlineData("edit-induction/change-reason", InductionStatus.InProgress)]
    public async Task Post_Cancel_EvidenceFilePreviouslyUploaded_DeletesPreviouslyUploadedFile(string fromPage, InductionStatus inductionStatus)
    {
        // Arrange
        var evidenceFileId = Guid.NewGuid();

        var person = await TestData.CreatePersonAsync(p => p
            .WithQts()
            .WithInductionStatus(s => s.
                WithStatus(inductionStatus)));

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionStateBuilder()
                .WithInitializedState(inductionStatus, InductionJourneyPage.Status)
                .WithStartDate(Clock.Today.AddYears(-2))
                .WithCompletedDate(Clock.Today)
                .WithReasonChoice(InductionChangeReasonOption.AnotherReason)
                .WithReasonDetailsChoice(true, "Details")
                .WithFileUploadChoice(true, evidenceFileId)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/{fromPage}?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        var doc = await AssertEx.HtmlResponseAsync(response);
        var cancelButton = doc.GetElementByTestId("cancel-button") as IHtmlButtonElement;

        // Act
        var redirectRequest = new HttpRequestMessage(HttpMethod.Post, cancelButton!.FormAction);
        var redirectResponse = await HttpClient.SendAsync(redirectRequest);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)redirectResponse.StatusCode);
        var location = redirectResponse.Headers.Location?.OriginalString;
        Assert.Equal($"/persons/{person.PersonId}/induction", location);

        FileServiceMock.AssertFileWasDeleted(evidenceFileId);
    }

    [Theory]
    [InlineData("edit-induction/status", InductionJourneyPage.Status)]
    [InlineData("edit-induction/exemption-reasons", InductionJourneyPage.ExemptionReason)]
    [InlineData("edit-induction/start-date", InductionJourneyPage.StartDate)]
    [InlineData("edit-induction/date-completed", InductionJourneyPage.CompletedDate)]
    public async Task Post_PersistsStartPageInfo(string page, InductionJourneyPage pageName)
    {
        // Arrange
        InductionStatus inductionStatus = InductionStatus.Passed;
        var exemptionReasonIds = new Guid[] { InductionExemptionReason.ExemptId };
        var person = await TestData.CreatePersonAsync(
            p => p.WithQts()
                .WithInductionStatus(i => i.WithStatus(inductionStatus).WithStartDate(Clock.Today.AddYears(-2)).WithCompletedDate(Clock.Today)));

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionStateBuilder()
                .WithInitializedState(inductionStatus, pageName)
                .WithStartDate(Clock.Today.AddYears(-2))
                .WithCompletedDate(Clock.Today)
                .Build());
        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/{page}?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new EditInductionPostRequestContentBuilder()
                .WithInductionStatus(inductionStatus)
                .WithStartDate(Clock.Today.AddDays(-1))
                .WithCompletedDate(Clock.Today)
                .WithExemptionReasonIds(exemptionReasonIds)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(pageName, journeyInstance.State.JourneyStartPage);
    }

    [Theory]
    [InlineData(InductionJourneyPage.Status, "edit-induction/status", InductionStatus.Exempt, "induction")]
    [InlineData(InductionJourneyPage.Status, "edit-induction/status", InductionStatus.RequiredToComplete, "induction")]
    [InlineData(InductionJourneyPage.Status, "edit-induction/exemption-reasons", InductionStatus.Exempt, "edit-induction/status")]
    [InlineData(InductionJourneyPage.Status, "edit-induction/start-date", InductionStatus.InProgress, "edit-induction/status")]
    [InlineData(InductionJourneyPage.StartDate, "edit-induction/start-date", InductionStatus.InProgress, "induction")]
    [InlineData(InductionJourneyPage.CompletedDate, "edit-induction/date-completed", InductionStatus.Failed, "induction")]
    [InlineData(InductionJourneyPage.Status, "edit-induction/date-completed", InductionStatus.Failed, "edit-induction/start-date")]
    [InlineData(InductionJourneyPage.StartDate, "edit-induction/date-completed", InductionStatus.Failed, "edit-induction/start-date")]
    [InlineData(InductionJourneyPage.StartDate, "edit-induction/change-reason", InductionStatus.Exempt, "edit-induction/exemption-reasons")]
    [InlineData(InductionJourneyPage.StartDate, "edit-induction/change-reason", InductionStatus.InProgress, "edit-induction/start-date")]
    [InlineData(InductionJourneyPage.StartDate, "edit-induction/change-reason", InductionStatus.Failed, "edit-induction/date-completed")]
    [InlineData(InductionJourneyPage.StartDate, "edit-induction/change-reason", InductionStatus.FailedInWales, "edit-induction/date-completed")]
    [InlineData(InductionJourneyPage.StartDate, "edit-induction/change-reason", InductionStatus.Passed, "edit-induction/date-completed")]
    [InlineData(InductionJourneyPage.StartDate, "edit-induction/change-reason", InductionStatus.RequiredToComplete, "edit-induction/status")]
    public async Task Get_SubjectToStartPage_BacklinkContainsExpected(InductionJourneyPage startPage, string fromPage, InductionStatus inductionStatus, string expectedBackPage)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithQts());

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionStateBuilder()
                .WithInitializedState(inductionStatus, startPage)
                .WithStartDate(Clock.Today.AddYears(-2))
                .WithCompletedDate(Clock.Today)
                .Build());
        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/{fromPage}?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var document = await response.GetDocumentAsync();
        var backlink = document.GetElementByTestId("back-link") as IHtmlAnchorElement;
        Assert.Contains($"/persons/{person.PersonId}/{expectedBackPage}", backlink!.Href);
    }

    [Theory]
    [InlineData(InductionJourneyPage.Status, "edit-induction/status", InductionStatus.Exempt, "check-answers")]
    [InlineData(InductionJourneyPage.Status, "edit-induction/start-date", InductionStatus.Passed, "check-answers")]
    [InlineData(InductionJourneyPage.Status, "edit-induction/date-completed", InductionStatus.Passed, "check-answers")]
    [InlineData(InductionJourneyPage.Status, "edit-induction/change-reason", InductionStatus.Passed, "check-answers")]
    [InlineData(InductionJourneyPage.Status, "edit-induction/exemption-reasons", InductionStatus.Exempt, "check-answers")]
    public async Task Get_FromCheckYourAnswersPage_BacklinkContainsExpected(InductionJourneyPage startPage, string fromPage, InductionStatus inductionStatus, string expectedBackPage)
    {
        // Arrange
        var exemptionReasonIds = new Guid[] { InductionExemptionReason.ExemptId };

        var person = await TestData.CreatePersonAsync(p => p.WithQts());

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionStateBuilder()
                .WithInitializedState(inductionStatus, startPage)
                .WithExemptionReasonIds(exemptionReasonIds)
                .WithStartDate(new DateOnly(2000, 2, 2))
                .WithCompletedDate(new DateOnly(2002, 2, 2))
                .WithReasonChoice(InductionChangeReasonOption.AnotherReason)
                .Build());
        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/{fromPage}?FromCheckAnswers={JourneyFromCheckYourAnswersPage.CheckYourAnswers}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var document = await response.GetDocumentAsync();
        var backlink = document.GetElementByTestId("back-link") as IHtmlAnchorElement;
        Assert.Contains($"/persons/{person.PersonId}/edit-induction/{expectedBackPage}", backlink!.Href);
    }

    [Fact]
    public async Task Get_CompletedDate_FromStartDate_FromCya_BacklinkContainsExpected()
    {
        // Arrange
        var fromPage = "edit-induction/date-completed";
        var inductionStatus = InductionStatus.Passed;
        var expectedBackPage = "edit-induction/start-date";
        var person = await TestData.CreatePersonAsync(p => p.WithQts());

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionStateBuilder()
                .WithInitializedState(inductionStatus, InductionJourneyPage.StartDate)
                .WithStartDate(new DateOnly(2000, 2, 2))
                .WithCompletedDate(new DateOnly(2002, 2, 2))
                .WithReasonChoice(InductionChangeReasonOption.AnotherReason)
                .Build());
        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/{fromPage}?FromCheckAnswers={JourneyFromCheckYourAnswersPage.CheckYourAnswersToStartDate.ToString()}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var document = await response.GetDocumentAsync();
        var backlink = document.GetElementByTestId("back-link") as IHtmlAnchorElement;
        Assert.Contains($"/persons/{person.PersonId}/{expectedBackPage}", backlink!.Href);
    }

    [Theory]
    [InlineData("edit-induction/status", InductionStatus.Exempt, "edit-induction/exemption-reasons")]
    [InlineData("edit-induction/start-date", InductionStatus.Passed, "edit-induction/check-answers")]
    [InlineData("edit-induction/date-completed", InductionStatus.Passed, "edit-induction/check-answers")]
    [InlineData("edit-induction/exemption-reasons", InductionStatus.Exempt, "edit-induction/check-answers")]
    [InlineData("edit-induction/change-reason", InductionStatus.Passed, "edit-induction/check-answers")]
    public async Task Post_FromCya_ToPage_Post_RedirectsToExpectedPage(string page, InductionStatus inductionStatus, string expectedNextPageUrl)
    {
        // Arrange
        var startDate = new DateOnly(2000, 2, 1);
        var completedDate = new DateOnly(2002, 2, 2);
        var exemptionReasonIds = new Guid[] { InductionExemptionReason.ExemptId };

        var person = await TestData.CreatePersonAsync(
            p => p
                .WithQts()
                .WithInductionStatus(i => i
                    .WithStatus(inductionStatus)));

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionStateBuilder()
               .WithInitializedState(inductionStatus, InductionJourneyPage.Status)
               .WithExemptionReasonIds(exemptionReasonIds)
               .WithStartDate(startDate)
               .WithCompletedDate(completedDate)
               .WithReasonChoice(InductionChangeReasonOption.AnotherReason)
               .Build());

        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/{page}?FromCheckAnswers={JourneyFromCheckYourAnswersPage.CheckYourAnswers}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new EditInductionPostRequestContentBuilder()
                .WithInductionStatus(inductionStatus)
                .WithExemptionReasonIds(exemptionReasonIds)
                .WithStartDate(startDate)
                .WithCompletedDate(completedDate)
                .WithChangeReason(InductionChangeReasonOption.IncompleteDetails)
                .WithChangeReasonDetailSelections(false)
                .WithEvidence(false)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        var location = response.Headers.Location?.OriginalString;
        var expectedUrl = $"/persons/{person.PersonId}/{expectedNextPageUrl}?{journeyInstance.GetUniqueIdQueryParameter()}";
        Assert.Equal(expectedUrl, location);
    }

    [Theory]
    [InlineData(InductionStatus.Passed, "2000-02-01", "2002-02-02", "check-answers")] // Start date not within two years of completed date
    [InlineData(InductionStatus.Passed, "2003-02-02", "2002-02-02", "date-completed")] // Start date after completed date
    public async Task Post_FromCya_ToStartDate_Post_RedirectsToExpectedPage(InductionStatus inductionStatus, string startDateString, string completedDateString, string expectedNextPageUrl)
    {
        // Arrange
        var startDate = DateOnly.Parse(startDateString);
        var completedDate = DateOnly.Parse(completedDateString);
        var fromPage = "edit-induction/start-date";
        var person = await TestData.CreatePersonAsync(
            p => p
                .WithQts()
                .WithInductionStatus(i => i
                    .WithStatus(inductionStatus)));

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionStateBuilder()
                .WithInitializedState(inductionStatus, InductionJourneyPage.Status)
                .WithStartDate(startDate)
                .WithCompletedDate(completedDate)
                .WithReasonChoice(InductionChangeReasonOption.AnotherReason)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/{fromPage}?FromCheckAnswers={JourneyFromCheckYourAnswersPage.CheckYourAnswers}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new EditInductionPostRequestContentBuilder()
                .WithInductionStatus(inductionStatus)
                .WithStartDate(startDate)
                .WithCompletedDate(completedDate)
                .WithChangeReason(InductionChangeReasonOption.IncompleteDetails)
                .WithChangeReasonDetailSelections(false)
                .WithEvidence(false)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        var location = response.Headers.Location?.OriginalString;
        var expectedUrl = $"/persons/{person.PersonId}/edit-induction/{expectedNextPageUrl}";
        Assert.Contains(expectedUrl, location);
    }

    [Theory]
    [MemberData(nameof(GetPagesForUserWithoutInductionWriteRoleForAllHttpMethodsData))]
    public async Task UserDoesNotHavePermission_ReturnsForbidden(string page, string? role, InductionStatus inductionStatus, HttpMethod httpMethod)
    {
        // Arrange
        SetCurrentUser(TestUsers.GetUser(role));

        var person = await TestData.CreatePersonAsync(
            p => p
                .WithQts()
                .WithInductionStatus(i => i
                    .WithStatus(inductionStatus)));

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionStateBuilder()
                .WithInitializedState(inductionStatus, InductionJourneyPage.Status)
                .WithStartDate(Clock.Today.AddYears(-2))
                .Build());

        var request = new HttpRequestMessage(httpMethod,
            $"/persons/{person.PersonId}/{page}?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status403Forbidden, (int)response.StatusCode);
    }

    [Theory]
    [MemberData(nameof(GetPagesForAllHttpMethodsData))]
    public async Task PersonIsDeactivated_ReturnsBadRequest(string page, InductionStatus inductionStatus, HttpMethod httpMethod)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        await WithDbContext(async dbContext =>
        {
            dbContext.Attach(person.Person);
            person.Person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionStateBuilder()
                .WithInitializedState(inductionStatus, InductionJourneyPage.Status)
                .WithStartDate(Clock.Today.AddYears(-2))
                .Build());

        var request = new HttpRequestMessage(httpMethod,
            $"/persons/{person.PersonId}/{page}?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    public static TheoryData<string, InductionStatus, HttpMethod> GetPagesForAllHttpMethodsData()
    {
        var data = new TheoryData<string, InductionStatus, HttpMethod>();

        foreach (var (page, status) in _pagesAndValidStatuses)
        {
            data.Add(page, status, HttpMethod.Get);
            data.Add(page, status, HttpMethod.Post);
        }

        return data;
    }

    public static TheoryData<string, string?, InductionStatus> GetPagesForUserWithoutInductionWriteRoleData()
    {
        var data = new TheoryData<string, string?, InductionStatus>();

        foreach (var (page, status) in _pagesAndValidStatuses)
        {
            foreach (var role in _rolesWithoutWritePermission)
            {
                data.Add(page, role, status);
            }
        }

        return data;
    }

    public static TheoryData<string, string?, InductionStatus, HttpMethod> GetPagesForUserWithoutInductionWriteRoleForAllHttpMethodsData()
    {
        var data = new TheoryData<string, string?, InductionStatus, HttpMethod>();

        foreach (var (page, status) in _pagesAndValidStatuses)
        {
            foreach (var role in _rolesWithoutWritePermission)
            {
                data.Add(page, role, status, HttpMethod.Get);
                data.Add(page, role, status, HttpMethod.Post);
            }
        }

        return data;
    }

    private Task<JourneyInstance<EditInductionState>> CreateJourneyInstanceAsync(Guid personId, EditInductionState? state = null) =>
        CreateJourneyInstance(
            JourneyNames.EditInduction,
            state ?? new EditInductionState(),
            new KeyValuePair<string, object>("personId", personId));

    private static string?[] _rolesWithoutWritePermission = UserRoles.All
        .Except([UserRoles.RecordManager, UserRoles.AccessManager, UserRoles.Administrator])
        .Append(null)
        .ToArray();

    private static readonly (string, InductionStatus)[] _pagesAndValidStatuses = new[]
    {
        ("edit-induction/status", InductionStatus.Exempt),
        ("edit-induction/status", InductionStatus.InProgress),
        ("edit-induction/status", InductionStatus.Failed),
        ("edit-induction/status", InductionStatus.FailedInWales),
        ("edit-induction/status", InductionStatus.Passed),
        ("edit-induction/status", InductionStatus.RequiredToComplete),
        ("edit-induction/exemption-reasons", InductionStatus.Exempt),
        ("edit-induction/start-date", InductionStatus.InProgress),
        ("edit-induction/start-date", InductionStatus.Failed),
        ("edit-induction/start-date", InductionStatus.FailedInWales),
        ("edit-induction/start-date", InductionStatus.Passed),
        ("edit-induction/date-completed", InductionStatus.Failed),
        ("edit-induction/date-completed", InductionStatus.FailedInWales),
        ("edit-induction/date-completed", InductionStatus.Passed),
        ("edit-induction/change-reason", InductionStatus.Exempt),
        ("edit-induction/change-reason", InductionStatus.InProgress),
        ("edit-induction/change-reason", InductionStatus.Failed),
        ("edit-induction/change-reason", InductionStatus.FailedInWales),
        ("edit-induction/change-reason", InductionStatus.Passed),
        ("edit-induction/change-reason", InductionStatus.RequiredToComplete)
    };
}
