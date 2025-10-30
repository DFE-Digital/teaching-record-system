using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.EditInduction;

public class EditInductionStatusTests(HostFixture hostFixture) : TestBase(hostFixture)
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
        var expectedCaption = "Induction - Alfred The Great";

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionStateBuilder()
                .WithInitializedState(inductionStatus, InductionJourneyPage.Status)
                .Build());

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
            new EditInductionStateBuilder()
                .WithInitializedState(inductionStatus, InductionJourneyPage.Status)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/edit-induction/status?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var form = doc.GetElementByTestId("submit-form") as IHtmlFormElement;
        Assert.NotNull(form);
        Assert.Contains($"/persons/{person.PersonId}/edit-induction/status", form.Action);
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
            new EditInductionStateBuilder()
                .WithInitializedState(currentInductionStatus, InductionJourneyPage.Status)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/edit-induction/status?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var statusChoices = doc.QuerySelectorAll<IHtmlInputElement>("[type=radio]").Select(r => r.Value);
        var statusChoicesLegend = doc.GetElementByTestId("status-choices-legend");
        Assert.Equal("What is their induction status?", statusChoicesLegend!.TrimmedText());
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
        var lessThanSevenYearsAgo = Clock.Today.AddYears(-1);
        var person = await TestData.CreatePersonAsync(p => p.WithQts());
        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(person.Person);
            person.Person.SetCpdInductionStatus(
                InductionStatus.Passed,
                startDate: lessThanSevenYearsAgo.AddYears(-1),
                completedDate: lessThanSevenYearsAgo,
                cpdModifiedOn: Clock.UtcNow,
                updatedBy: SystemUser.SystemUserId,
                now: Clock.UtcNow,
                out _);
            await dbContext.SaveChangesAsync();
        });

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionStateBuilder()
                .WithInitializedState(currentInductionStatus, InductionJourneyPage.Status)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/edit-induction/status?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var statusChoices = doc.QuerySelectorAll<IHtmlInputElement>("[type=radio]").Select(r => r.Value);
        var statusChoicesLegend = doc.GetElementByTestId("status-choices-legend");
        Assert.Equal("What is their induction status?", statusChoicesLegend!.TrimmedText());
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
            dbContext.Attach(person.Person);
            person.Person.SetCpdInductionStatus(
                InductionStatus.RequiredToComplete, // CPD induction status can't be Exempt or FailedInWales
                startDate: null,
                completedDate: null,
                cpdModifiedOn: Clock.UtcNow,
                updatedBy: SystemUser.SystemUserId,
                now: Clock.UtcNow,
                out _);
            person.Person.SetInductionStatus(
                status,
                startDate: null,
                completedDate: null,
                exemptionReasonIds: [],
                changeReason: null,
                changeReasonDetail: null,
                evidenceFile: null,
                updatedBy: SystemUser.SystemUserId,
                now: Clock.UtcNow,
                out _);
            await dbContext.SaveChangesAsync();
        });

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionStateBuilder()
                .WithInitializedState(status, InductionJourneyPage.Status)
                .Build());

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
            new EditInductionStateBuilder()
                .WithInitializedState(currentInductionStatus, InductionJourneyPage.Status)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/edit-induction/status?fromCheckAnswers={JourneyFromCheckAnswersPage.CheckAnswers}&{journeyInstance.GetUniqueIdQueryParameter()}");

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
            new EditInductionStateBuilder()
                .WithInitializedState(InductionStatus.Passed, InductionJourneyPage.Status)
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/edit-induction/status?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new EditInductionPostRequestContentBuilder()
                .WithInductionStatus(InductionStatus.Exempt)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal("Exempt", journeyInstance.State.InductionStatus.GetTitle());
    }

    [Fact]
    public async Task Post_NoSelectedStatus_ShowsPageError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithQts());

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionStateBuilder()
                .WithInitializedState(InductionStatus.RequiredToComplete, InductionJourneyPage.Status)
                .Build());

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
        var lessThanSevenYearsAgo = Clock.Today.AddYears(-1);
        var person = await TestData.CreatePersonAsync(p => p.WithQts());
        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(person.Person);
            person.Person.SetCpdInductionStatus(
                InductionStatus.Passed,
                startDate: lessThanSevenYearsAgo.AddYears(-1),
                completedDate: lessThanSevenYearsAgo,
                cpdModifiedOn: Clock.UtcNow,
                updatedBy: SystemUser.SystemUserId,
                now: Clock.UtcNow,
                out _);
            await dbContext.SaveChangesAsync();
        });
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionStateBuilder()
                .WithInitializedState(InductionStatus.Passed, InductionJourneyPage.Status)
                .Build());

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
        var lessThanSevenYearsAgo = Clock.Today.AddYears(-1);

        // test setup here is convoluted because I need to set up a person,
        // then call SetCpdInductionstatus to set the CpdInductionModifiedOn date,
        // then set the induction status to the one being tested
        var person = await TestData.CreatePersonAsync(
            p => p.WithQts());

        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(person.Person);

            // Force status to `None` so that the SetCpdInductionStatus() call below always has a change to status
            person.Person.UnsafeSetInductionStatus(
                InductionStatus.None,
                InductionStatus.None,
                startDate: null,
                completedDate: null,
                exemptionReasonIds: []);

            person.Person.SetCpdInductionStatus(
                InductionStatus.RequiredToComplete, // CPD induction status can't be Exempt or FailedInWales
                startDate: null,
                completedDate: null,
                cpdModifiedOn: Clock.UtcNow,
                updatedBy: SystemUser.SystemUserId,
                now: Clock.UtcNow,
                out _);

            person.Person.SetInductionStatus(
                status,
                startDate: null,
                completedDate: null,
                exemptionReasonIds: [],
                changeReason: null,
                changeReasonDetail: null,
                evidenceFile: null,
                updatedBy: SystemUser.SystemUserId,
                now: Clock.UtcNow,
                out _);

            await dbContext.SaveChangesAsync();
        });

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionStateBuilder()
                .WithInitializedState(status, InductionJourneyPage.Status)
                .Build());

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
        var lessThanSevenYearsAgo = Clock.Today.AddYears(-1);

        // test setup here is convoluted because I need to set up a person,
        // then call SetCpdInductionstatus to set the CpdInductionModifiedOn date,
        // then set the induction status to the one being tested
        var person = await TestData.CreatePersonAsync(
            p => p.WithQts());
        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(person.Person);
            person.Person.SetCpdInductionStatus(
                InductionStatus.RequiredToComplete, // CPD induction status can't be Exempt or FailedInWales
                startDate: null,
                completedDate: null,
                cpdModifiedOn: Clock.UtcNow,
                updatedBy: SystemUser.SystemUserId,
                now: Clock.UtcNow,
                out _);
            person.Person.SetInductionStatus(
                status,
                startDate: null,
                completedDate: null,
                exemptionReasonIds: [],
                changeReason: null,
                changeReasonDetail: null,
                evidenceFile: null,
                updatedBy: SystemUser.SystemUserId,
                now: Clock.UtcNow,
                out _);
            await dbContext.SaveChangesAsync();
        });
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionStateBuilder()
                .WithInitializedState(status, InductionJourneyPage.Status)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/edit-induction/status?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Null(doc!.GetElementByTestId("induction-status-warning"));
    }

    private Task<JourneyInstance<EditInductionState>> CreateJourneyInstanceAsync(Guid personId, EditInductionState? state = null) =>
        CreateJourneyInstance(
            JourneyNames.EditInduction,
            state ?? new EditInductionState(),
            new KeyValuePair<string, object>("personId", personId));
}
