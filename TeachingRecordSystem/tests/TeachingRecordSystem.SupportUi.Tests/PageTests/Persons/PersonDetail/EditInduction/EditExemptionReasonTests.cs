using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.EditInduction;

public class EditExemptionReasonTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_WithExemptionReasons_ShowsExemptionReasons()
    {
        // note - not testing that inactive reasons are not shown as there aren't any in the reference data cache
        //      - is there any value in the cache returning non-active reasons?
        // Arrange
        var exemptionReasons = await TestData.ReferenceDataCache.GetInductionExemptionReasonsAsync();
        var inductionStatus = InductionStatus.InProgress;
        var person = await TestData.CreatePersonAsync(p => p.WithQts());
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionStateBuilder()
                .WithInitialisedState(inductionStatus, InductionJourneyPage.Status)
                .Create());

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/edit-induction/exemption-reasons?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var exemptionReasonsElement = doc.QuerySelectorAll<IHtmlInputElement>("[type=checkbox]");
        Assert.Equal(exemptionReasons.Length, exemptionReasonsElement.Count());

        for (var i = 0; i < exemptionReasons.Length; i++)
        {
            var checkbox = exemptionReasonsElement.ElementAt(i);
            var label = checkbox.ParentElement!.QuerySelector<IHtmlLabelElement>($"label[for='{checkbox.Id}']");

            Assert.Equal(exemptionReasons[i].InductionExemptionReasonId.ToString(), checkbox.Value);
            Assert.Equal(exemptionReasons[i].Name, label!.TextContent.Trim());
        }
    }

    [Fact]
    public async Task Post_NoExemptionReasonsSet_ShowsError()
    {
        // Arrange
        var dateValid = Clock.Today;
        var inductionStatus = InductionStatus.InProgress;
        var person = await TestData.CreatePersonAsync(p => p.WithQts());
        var random = new Random();
        var exemptionReasons = (await TestData.ReferenceDataCache.GetInductionExemptionReasonsAsync()).Where(e => e.IsActive).Select(e => e.InductionExemptionReasonId);
        var randomExemptionReasonIds = exemptionReasons.OrderBy(x => random.Next()).Take(2).ToArray();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionStateBuilder()
                .WithInitialisedState(inductionStatus, InductionJourneyPage.Status)
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
        var dateValid = Clock.Today;
        var inductionStatus = InductionStatus.InProgress;
        var person = await TestData.CreatePersonAsync(p => p.WithQts());
        var random = new Random();
        var exemptionReasons = (await TestData.ReferenceDataCache.GetInductionExemptionReasonsAsync()).Where(e => e.IsActive).Select(e => e.InductionExemptionReasonId);
        var randomExemptionReasonIds = exemptionReasons.OrderBy(x => random.Next()).Take(2).ToArray();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionStateBuilder()
                .WithInitialisedState(inductionStatus, InductionJourneyPage.Status)
                .Create());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/edit-induction/exemption-reasons?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(
                new EditInductionPostRequestBuilder()
                    .WithCompletedDate(dateValid)
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
