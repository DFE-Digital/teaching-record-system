using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.EditInduction;

public class EditExemptionReasonTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_ShowsExemptionReasonsList()
    {
        // note - not testing that inactive reasons are not shown as there aren't any in the reference data cache
        // Arrange
        var exemptionReasons = (await TestData.ReferenceDataCache.GetInductionExemptionReasonsAsync()).Where(e => e.IsActive).ToArray();
        var person = await TestData.CreatePersonAsync(p => p.WithQts());
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionStateBuilder()
                .WithInitialisedState(InductionStatus.Exempt, InductionJourneyPage.Status)
                .Create());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/edit-induction/exemption-reasons?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var exemptionReasonsElement = doc.QuerySelectorAll<IHtmlInputElement>("[type=checkbox]");
        Assert.Equal(exemptionReasons.Length, exemptionReasonsElement.Count());
        Assert.All(exemptionReasonsElement, checkbox =>
        {
            Assert.Contains(checkbox.Value, exemptionReasons.Select(e => e.InductionExemptionReasonId.ToString()));
            Assert.Contains(checkbox.ParentElement!.QuerySelector<IHtmlLabelElement>($"label[for='{checkbox.Id}']")!.TextContent.Trim(), exemptionReasons.Select(e => e.Name));
        });
    }

    [Fact]
    public async Task Get_WithExemptionReasonsSelected_ShowsExpected()
    {
        var exemptionReasons = (await TestData.ReferenceDataCache.GetInductionExemptionReasonsAsync())
            .Where(e => e.IsActive)
            .ToArray();
        var selectedExemptionReasonIds = exemptionReasons
            .Select(e => e.InductionExemptionReasonId)
            .RandomSelection(2)
            .ToArray();
        var person = await TestData.CreatePersonAsync(p => p.WithQts());
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionStateBuilder()
                .WithInitialisedState(InductionStatus.Exempt, InductionJourneyPage.Status)
                .WithExemptionReasonIds(selectedExemptionReasonIds)
                .Create());

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
                .WithInitialisedState(InductionStatus.Exempt, InductionJourneyPage.Status)
                .Create());

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
        var exemptionReasons = (await TestData.ReferenceDataCache.GetInductionExemptionReasonsAsync())
            .Where(e => e.IsActive)
            .Select(e => e.InductionExemptionReasonId);
        var randomExemptionReasonIds = exemptionReasons
            .RandomSelection(2)
            .ToArray();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionStateBuilder()
                .WithInitialisedState(InductionStatus.Exempt, InductionJourneyPage.Status)
                .Create());

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
