using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.EditInduction;

public class EditExemptionReasonTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_InvalidInductionStatusForPage_RedirectToStart()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithQts());
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionStateBuilder()
                .WithInitializedState(InductionStatus.InProgress, InductionJourneyPage.Status)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/edit-induction/exemption-reasons?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/persons/{person.PersonId}/edit-induction/status?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_ShowsExemptionReasonsList()
    {
        // Arrange
        var allGuidsToDisplay = ExemptionReasonCategories.ExemptionReasonIds;
        var exemptionReasons = (await TestData.ReferenceDataCache.GetInductionExemptionReasonsAsync(activeOnly: true))
            .ToArray();
        var exemptionReasonsForDisplay = allGuidsToDisplay.Join(
            exemptionReasons,
            guid => guid,
            exemption => exemption.InductionExemptionReasonId,
            (guid, exemption) => new { guid, exemption.Name })
            .ToArray();
        var person = await TestData.CreatePersonAsync(p => p
            .WithQts()
            .WithInductionStatus(i => i.
                WithStatus(InductionStatus.Exempt)));
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionStateBuilder()
                .WithInitializedState(InductionStatus.Exempt, InductionJourneyPage.Status)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/edit-induction/exemption-reasons?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var exemptionReasonsElement = doc.QuerySelectorAll<IHtmlInputElement>("[type=checkbox]");
        Assert.Equal(exemptionReasonsForDisplay.Length, exemptionReasonsElement.Count());
        Assert.All(exemptionReasonsElement, checkbox =>
        {
            Assert.Contains(checkbox.Value, exemptionReasonsForDisplay.Select(e => e.guid.ToString()));
            Assert.Contains(checkbox.ParentElement!.QuerySelector<IHtmlLabelElement>($"label[for='{checkbox.Id}']")!.TextContent.Trim(), exemptionReasonsForDisplay.Select(e => e.Name));
        });
    }

    public static IEnumerable<object[]> SpecificInductionExemptedRoutesRequiringMessagesData()
    {
        yield return new object[] { RouteToProfessionalStatus.ScotlandRId };
        yield return new object[] { RouteToProfessionalStatus.NiRId };
    }
    [Theory]
    [MemberData(nameof(SpecificInductionExemptedRoutesRequiringMessagesData))]
    public async Task Get_PersonHasInductionExemptionFromSomeSpecificRoutes_ShowsExpectedContent(Guid routeId)
    {
        // Arrange
        var allGuidsToDisplay = ExemptionReasonCategories.ExemptionReasonIds;
        var route = await ReferenceDataCache.GetRouteToProfessionalStatusByIdAsync(routeId);
        var awardedDate = Clock.Today;
        var exemptionReasons = (await TestData.ReferenceDataCache.GetInductionExemptionReasonsAsync(activeOnly: true))
            .ToArray();
        var exemptionReasonsForDisplay = allGuidsToDisplay
            .Where(guid => guid != route.InductionExemptionReason?.InductionExemptionReasonId) // exclude the awarded route exemption reason 
            .Join(exemptionReasons,
                guid => guid,
                exemption => exemption.InductionExemptionReasonId,
                (guid, exemption) => new { guid, exemption.Name })
            .ToArray();
        var person = await TestData.CreatePersonAsync(p => p
            .WithQts()
            .WithProfessionalStatus(r => r
                .WithRoute(route.RouteToProfessionalStatusId)
                .WithStatus(ProfessionalStatusStatus.Awarded)
                .WithAwardedDate(awardedDate)));
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionStateBuilder()
                .WithInitializedState(InductionStatus.Exempt, InductionJourneyPage.Status)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/edit-induction/exemption-reasons?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var exemptionReasonsElements = doc.QuerySelectorAll<IHtmlInputElement>("[type=checkbox]");
        Assert.Equal(exemptionReasonsForDisplay.Length, exemptionReasonsElements.Count());
        Assert.All(exemptionReasonsElements, checkbox =>
        {
            Assert.Contains(checkbox.Value, exemptionReasonsForDisplay.Select(e => e.guid.ToString()));
            Assert.Contains(checkbox.ParentElement!.QuerySelector<IHtmlLabelElement>($"label[for='{checkbox.Id}']")!.TextContent.Trim(), exemptionReasonsForDisplay.Select(e => e.Name));
        });

        var expectedMessage1 = $"This person has an induction exemption \"{route.InductionExemptionReason?.Name}\" on the \"{route.Name}\" route.";
        var expectedMessage2 = $"To add/remove the Induction exemption reason of: \"{route.InductionExemptionReason?.Name}\" please modify the \"{route.Name}\" route.";
        var messagesDisplayed = doc.GetElementsByClassName("govuk-inset-text").ToArray();
        Assert.Equal(expectedMessage1, messagesDisplayed[0].TextContent.Trim());
        Assert.Equal(expectedMessage2, messagesDisplayed[1].TextContent.Trim());
    }

    [Fact]
    public async Task Get_PersonHasInductionExemptionFromRoute_ShowsExpectedContent()
    {
        // Arrange
        var allGuidsToDisplay = ExemptionReasonCategories.ExemptionReasonIds;
        var route = (await ReferenceDataCache.GetRoutesToProfessionalStatusAsync())
            .Where(r => r.InductionExemptionReasonId is not null && r.RouteToProfessionalStatusId != RouteToProfessionalStatus.ScotlandRId && r.RouteToProfessionalStatusId != RouteToProfessionalStatus.NiRId)
            .RandomOne();
        var awardedDate = Clock.Today;
        var exemptionReasons = (await TestData.ReferenceDataCache.GetInductionExemptionReasonsAsync(activeOnly: true))
            .ToArray();
        var exemptionReasonsForDisplay = allGuidsToDisplay
            .Join(exemptionReasons,
                guid => guid,
                exemption => exemption.InductionExemptionReasonId,
                (guid, exemption) => new { guid, exemption.Name })
            .ToArray();
        var person = await TestData.CreatePersonAsync(p => p
            .WithQts()
            .WithProfessionalStatus(r => r
                .WithRoute(route.RouteToProfessionalStatusId)
                .WithStatus(ProfessionalStatusStatus.Awarded)
                .WithAwardedDate(awardedDate)));
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionStateBuilder()
                .WithInitializedState(InductionStatus.Exempt, InductionJourneyPage.Status)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/edit-induction/exemption-reasons?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var exemptionReasonsElements = doc.QuerySelectorAll<IHtmlInputElement>("[type=checkbox]");
        Assert.Equal(exemptionReasonsForDisplay.Length, exemptionReasonsElements.Count());
        Assert.All(exemptionReasonsElements, checkbox =>
        {
            Assert.Contains(checkbox.Value, exemptionReasonsForDisplay.Select(e => e.guid.ToString()));
            Assert.Contains(checkbox.ParentElement!.QuerySelector<IHtmlLabelElement>($"label[for='{checkbox.Id}']")!.TextContent.Trim(), exemptionReasonsForDisplay.Select(e => e.Name));
        });

        var expectedMessage = $"This person has an induction exemption \"{route.InductionExemptionReason?.Name}\" on the \"{route.Name}\" route.";
        var messageDisplayed = doc.GetElementsByClassName("govuk-inset-text").Single();
        Assert.Equal(expectedMessage, messageDisplayed.TextContent.Trim());
    }

    [Fact]
    public async Task Get_WithExemptionReasonsSelected_ShowsExpected()
    {
        var exemptionReasons = (await TestData.ReferenceDataCache.GetInductionExemptionReasonsAsync(activeOnly: true)).ToArray();
        var selectedExemptionReasonIds = exemptionReasons
            .Select(e => e.InductionExemptionReasonId)
            .RandomSelection(2)
            .ToArray();
        var person = await TestData.CreatePersonAsync(p => p.WithQts());
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionStateBuilder()
                .WithInitializedState(InductionStatus.Exempt, InductionJourneyPage.Status)
                .WithExemptionReasonIds(selectedExemptionReasonIds)
                .Build());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/edit-induction/exemption-reasons?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var selectedExemptionReasonsCheckboxes = doc.QuerySelectorAll<IHtmlInputElement>("[type=checkbox]").Where(c => c.IsChecked).Select(c => c.Value);
        Assert.All(selectedExemptionReasonsCheckboxes, checkboxValue =>
            {
                Assert.Contains(checkboxValue, selectedExemptionReasonIds.Select(id => id.ToString()));
            });
    }

    [Fact]
    public async Task Post_NoExemptionReasonsSelected_ShowsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithQts());
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionStateBuilder()
                .WithInitializedState(InductionStatus.Exempt, InductionJourneyPage.Status)
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/edit-induction/exemption-reasons?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "ExemptionReasonIds", "Select the reason for a teacher’s exemption to induction");
    }

    [Fact]
    public async Task Post_SetExemptionReasons_PersistsExemptionReasons()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithQts());
        var exemptionReasons = (await TestData.ReferenceDataCache.GetInductionExemptionReasonsAsync(activeOnly: true))
            .Select(e => e.InductionExemptionReasonId);
        var randomExemptionReasonIds = exemptionReasons
            .RandomSelection(2)
            .ToArray();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionStateBuilder()
                .WithInitializedState(InductionStatus.Exempt, InductionJourneyPage.Status)
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/edit-induction/exemption-reasons?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(
                new EditInductionPostRequestBuilder()
                    .WithExemptionReasonIds(randomExemptionReasonIds)
                    .Build())
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(randomExemptionReasonIds, journeyInstance.State.ExemptionReasonIds);
    }

    private Task<JourneyInstance<EditInductionState>> CreateJourneyInstanceAsync(Guid personId, EditInductionState? state = null) =>
    CreateJourneyInstance(
        JourneyNames.EditInduction,
        state ?? new EditInductionState(),
        new KeyValuePair<string, object>("personId", personId));
}
