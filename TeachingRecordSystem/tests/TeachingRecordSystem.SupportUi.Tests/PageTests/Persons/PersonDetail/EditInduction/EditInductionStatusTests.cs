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
            new EditInductionState()
            {
                Initialized = true,
                InductionStatus = inductionStatus
            });
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
            new EditInductionState()
            {
                Initialized = true,
                InductionStatus = inductionStatus
            });
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

    [Theory]
    [InlineData(InductionStatus.RequiredToComplete, new InductionStatus[] { InductionStatus.Exempt, InductionStatus.InProgress, InductionStatus.Passed, InductionStatus.Failed, InductionStatus.FailedInWales })]
    [InlineData(InductionStatus.Exempt, new InductionStatus[] { InductionStatus.RequiredToComplete, InductionStatus.InProgress, InductionStatus.Passed, InductionStatus.Failed, InductionStatus.FailedInWales })]
    [InlineData(InductionStatus.InProgress, new InductionStatus[] { InductionStatus.RequiredToComplete, InductionStatus.Exempt, InductionStatus.Passed, InductionStatus.Failed, InductionStatus.FailedInWales })]
    [InlineData(InductionStatus.Passed, new InductionStatus[] { InductionStatus.RequiredToComplete, InductionStatus.Exempt, InductionStatus.InProgress, InductionStatus.Failed, InductionStatus.FailedInWales })]
    [InlineData(InductionStatus.Failed, new InductionStatus[] { InductionStatus.RequiredToComplete, InductionStatus.Exempt, InductionStatus.InProgress, InductionStatus.Passed, InductionStatus.FailedInWales })]
    [InlineData(InductionStatus.FailedInWales, new InductionStatus[] { InductionStatus.RequiredToComplete, InductionStatus.Exempt, InductionStatus.InProgress, InductionStatus.Passed, InductionStatus.Failed })]
    public async Task Get_InductionNotManagedByCpd_ExpectedRadioButtonsExistOnPage(InductionStatus initialInductionStatus, InductionStatus[] expectedStatuses)
    {
        // Arrange
        var expectedChoices = expectedStatuses.Select(s => s.ToString());

        var person = await TestData.CreatePersonAsync(p => p.WithQts());

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionState()
            {
                Initialized = true,
                InitialInductionStatus = initialInductionStatus
            });
        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/edit-induction/status?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var statusChoices = doc.QuerySelectorAll<IHtmlInputElement>("[type=radio]").Select(r => r.Value);
        var statusChoicesLegend = doc.GetElementByTestId("status-choices-legend");
        Assert.Equal("Select a status", statusChoicesLegend!.TextContent);
        Assert.Equal(expectedChoices, statusChoices);
    }

    [Theory]
    [InlineData(InductionStatus.Passed, new InductionStatus[] { InductionStatus.RequiredToComplete, InductionStatus.Exempt, InductionStatus.FailedInWales })]
    [InlineData(InductionStatus.Failed, new InductionStatus[] { InductionStatus.RequiredToComplete, InductionStatus.Exempt, InductionStatus.FailedInWales })]
    [InlineData(InductionStatus.FailedInWales, new InductionStatus[] { InductionStatus.RequiredToComplete, InductionStatus.Exempt })]
    public async Task Get_InductionManagedByCpd_ExpectedRadioButtonsExistOnPage(InductionStatus initialInductionStatus, InductionStatus[] expectedStatuses)
    {
        // Arrange
        var expectedChoices = expectedStatuses.Select(s => s.ToString());
        var overSevenYearsAgo = Clock.Today.AddYears(-7).AddDays(-1);
        var person = await TestData.CreatePersonAsync(p => p.WithQts());
        await WithDbContext(async dbContext =>
        {
            dbContext.Attach(person.Person);
            person.Person.SetCpdInductionStatus(
                InductionStatus.Passed,
                startDate: Clock.Today.AddYears(-7).AddMonths(-6),
                completedDate: overSevenYearsAgo,
                cpdModifiedOn: Clock.UtcNow,
                updatedBy: SystemUser.SystemUserId,
                now: Clock.UtcNow,
                out _);
            await dbContext.SaveChangesAsync();
        });

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionState()
            {
                Initialized = true,
                InitialInductionStatus = initialInductionStatus
            });
        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/edit-induction/status?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var statusChoices = doc.QuerySelectorAll<IHtmlInputElement>("[type=radio]").Select(r => r.Value);
        var statusChoicesLegend = doc.GetElementByTestId("status-choices-legend");
        Assert.Equal("Select a status", statusChoicesLegend!.TextContent);
        Assert.Equal(expectedChoices, statusChoices);
    }

    [Fact]
    public async Task Post_SelectedStatus_PersistsStatus()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithQts());

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionState()
            {
                Initialized = true,
                InitialInductionStatus = InductionStatus.Passed
            });
        var postRequest = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/edit-induction/status?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["InductionStatus"] = "Exempt"
            })
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal("Exempt", journeyInstance.State.InductionStatus.GetTitle());
    }

    [Theory]
    [InlineData(InductionStatus.RequiredToComplete, new InductionStatus[] { InductionStatus.Exempt, InductionStatus.InProgress, InductionStatus.Passed, InductionStatus.Failed, InductionStatus.FailedInWales })]
    [InlineData(InductionStatus.Exempt, new InductionStatus[] { InductionStatus.RequiredToComplete, InductionStatus.InProgress, InductionStatus.Passed, InductionStatus.Failed, InductionStatus.FailedInWales })]
    [InlineData(InductionStatus.InProgress, new InductionStatus[] { InductionStatus.RequiredToComplete, InductionStatus.Exempt, InductionStatus.Passed, InductionStatus.Failed, InductionStatus.FailedInWales })]
    [InlineData(InductionStatus.Passed, new InductionStatus[] { InductionStatus.RequiredToComplete, InductionStatus.Exempt, InductionStatus.InProgress, InductionStatus.Failed, InductionStatus.FailedInWales })]
    [InlineData(InductionStatus.Failed, new InductionStatus[] { InductionStatus.RequiredToComplete, InductionStatus.Exempt, InductionStatus.InProgress, InductionStatus.Passed, InductionStatus.FailedInWales })]
    [InlineData(InductionStatus.FailedInWales, new InductionStatus[] { InductionStatus.RequiredToComplete, InductionStatus.Exempt, InductionStatus.InProgress, InductionStatus.Passed, InductionStatus.Failed })]

    public async Task Post_NoSelectedStatus_ShowsPageError(InductionStatus initialInductionStatus, InductionStatus[] expectedStatusChoices)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithQts());

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionState()
            {
                Initialized = true,
                InitialInductionStatus = initialInductionStatus
            });
        var postRequest = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/edit-induction/status?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["InductionStatus"] = InductionStatus.None.ToString()
            })
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(StatusModel.InductionStatus), "Select a status");
        var doc = await response.GetDocumentAsync();
        var statusChoices = doc.QuerySelectorAll<IHtmlInputElement>("[type=radio]").Select(r => r.Value);
        var statusChoicesLegend = doc.GetElementByTestId("status-choices-legend");
        Assert.Equal("Select a status", statusChoicesLegend!.TextContent);
        Assert.Equal(expectedStatusChoices.Select(c => c.ToString()), statusChoices);
    }

    [Theory]
    [InlineData(InductionStatus.Passed, new InductionStatus[] { InductionStatus.RequiredToComplete, InductionStatus.Exempt, InductionStatus.FailedInWales })]
    [InlineData(InductionStatus.Failed, new InductionStatus[] { InductionStatus.RequiredToComplete, InductionStatus.Exempt, InductionStatus.FailedInWales })]
    [InlineData(InductionStatus.FailedInWales, new InductionStatus[] { InductionStatus.RequiredToComplete, InductionStatus.Exempt })]
    public async Task Post_PersonManagedByCpd_NoSelectedStatus_ShowsPageError(InductionStatus initialInductionStatus, InductionStatus[] expectedChoices)
    {
        var overSevenYearsAgo = Clock.Today.AddYears(-7).AddDays(-1);
        var person = await TestData.CreatePersonAsync(p => p.WithQts());
        await WithDbContext(async dbContext =>
        {
            dbContext.Attach(person.Person);
            person.Person.SetCpdInductionStatus(
                InductionStatus.Passed,
                startDate: Clock.Today.AddYears(-7).AddMonths(-6),
                completedDate: overSevenYearsAgo,
                cpdModifiedOn: Clock.UtcNow,
                updatedBy: SystemUser.SystemUserId,
                now: Clock.UtcNow,
                out _);
            await dbContext.SaveChangesAsync();
        });
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionState()
            {
                Initialized = true,
                InitialInductionStatus = initialInductionStatus
            });
        var postRequest = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/edit-induction/status?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["InductionStatus"] = InductionStatus.None.ToString()
            })
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(StatusModel.InductionStatus), "Select a status");
        var doc = await response.GetDocumentAsync();
        var statusChoices = doc.QuerySelectorAll<IHtmlInputElement>("[type=radio]").Select(r => r.Value);
        var statusChoicesLegend = doc.GetElementByTestId("status-choices-legend");
        Assert.Equal("Select a status", statusChoicesLegend!.TextContent);
        Assert.Equal(expectedChoices.Select(c => c.ToString()), statusChoices);
    }

    [Fact]
    public async Task Get_ForPersonWithInductionStatusManagedByCPD_ShowsWarning()
    {
        //Arrange
        var expectedWarning = "To change this teacher’s induction status ";
        var overSevenYearsAgo = Clock.Today.AddYears(-7).AddDays(-1);

        var person = await TestData.CreatePersonAsync();
        await WithDbContext(async dbContext =>
        {
            dbContext.Attach(person.Person);
            person.Person.SetCpdInductionStatus(
                InductionStatus.Passed,
                startDate: Clock.Today.AddYears(-7).AddMonths(-6),
                completedDate: overSevenYearsAgo,
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
        Assert.Contains(expectedWarning, doc!.GetElementByTestId("induction-status-warning")!.Children[1].TextContent);
    }

    private Task<JourneyInstance<EditInductionState>> CreateJourneyInstanceAsync(Guid personId, EditInductionState? state = null) =>
        CreateJourneyInstance(
            JourneyNames.EditInduction,
            state ?? new EditInductionState(),
            new KeyValuePair<string, object>("personId", personId));
}
