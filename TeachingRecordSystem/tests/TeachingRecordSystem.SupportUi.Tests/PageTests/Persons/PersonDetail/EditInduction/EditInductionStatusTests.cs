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
        Assert.Equal(expectedCaption, caption!.TextContent);
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
        Assert.Equal("Continue", buttons.ElementAt(0)!.TextContent);
        Assert.Equal("Cancel and return to record", buttons.ElementAt(1)!.TextContent);
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
        Assert.Equal("What is their induction status?", statusChoicesLegend!.TextContent);
        Assert.Equal(expectedChoices, statusChoices);
    }

    [Fact]
    public async Task Get_InductionManagedByCpd_ExpectedRadioButtonsExistOnPage()
    {
        // Arrange
        var currentInductionStatus = InductionStatus.Passed;
        InductionStatus[] expectedStatuses = { InductionStatus.RequiredToComplete, InductionStatus.Exempt, InductionStatus.FailedInWales };
        var expectedChoices = expectedStatuses.Select(s => s.ToString());
        var lessThanSevenYearsAgo = Clock.Today.AddYears(-1);
        var person = await TestData.CreatePersonAsync(p => p.WithQts());
        await WithDbContext(async dbContext =>
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
        Assert.Equal("What is their induction status?", statusChoicesLegend!.TextContent);
        Assert.Equal(expectedChoices, statusChoices);
    }

    [Fact]
    public async Task Get_InductionStatus_ShowsAllRadioButtonsNotSelected()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithQts());
        var currentInductionStatus = InductionStatus.RequiredToComplete;
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
        var selectedStatus = doc.QuerySelectorAll<IHtmlInputElement>("[type=radio]").Where(r => r.IsChecked == true);
        Assert.Empty(selectedStatus);
    }

    [Fact]
    public async Task Get_InductionStatusFromCya_ShowsSelectedRadioButton()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithQts());
        var currentInductionStatus = InductionStatus.RequiredToComplete;
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionStateBuilder()
                .WithInitializedState(currentInductionStatus, InductionJourneyPage.Status)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/edit-induction/status?fromCheckAnswers={JourneyFromCheckYourAnswersPage.CheckYourAnswers}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var selectedStatus = doc.QuerySelectorAll<IHtmlInputElement>("[type=radio]").Single(r => r.IsChecked == true);
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
            Content = new FormUrlEncodedContent(
                new EditInductionPostRequestBuilder()
                    .WithInductionStatus(InductionStatus.Exempt)
                    .Build())
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
            Content = new FormUrlEncodedContent(new EditInductionPostRequestBuilder()
                    .WithInductionStatus(InductionStatus.None)
                    .Build())
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
        await WithDbContext(async dbContext =>
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
            Content = new FormUrlEncodedContent(new EditInductionPostRequestBuilder()
                    .WithInductionStatus(InductionStatus.None)
                    .Build())
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

        var person = await TestData.CreatePersonAsync(p => p.WithQts());
        await WithDbContext(async dbContext =>
        {
            dbContext.Attach(person.Person);
            person.Person.SetCpdInductionStatus(
                status,
                startDate: status.RequiresStartDate() ? lessThanSevenYearsAgo.AddYears(-1) : null,
                completedDate: status.RequiresCompletedDate() ? lessThanSevenYearsAgo : null,
                cpdModifiedOn: Clock.UtcNow,
                updatedBy: SystemUser.SystemUserId,
                now: Clock.UtcNow,
                out _);
            await dbContext.SaveChangesAsync();
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/induction");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Contains(statusSpecificText, doc!.GetElementByTestId("induction-status-warning")!.Children[1].TextContent);
    }

    private Task<JourneyInstance<EditInductionState>> CreateJourneyInstanceAsync(Guid personId, EditInductionState? state = null) =>
        CreateJourneyInstance(
            JourneyNames.EditInduction,
            state ?? new EditInductionState(),
            new KeyValuePair<string, object>("personId", personId));
}
