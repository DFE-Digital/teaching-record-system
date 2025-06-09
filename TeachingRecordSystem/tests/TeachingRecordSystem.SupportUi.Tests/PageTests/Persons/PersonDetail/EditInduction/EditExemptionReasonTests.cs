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

    [Fact]
    public async Task Get_PersonHasRouteWithInductionExemption_ShowsExemptionReasonsList()
    {
        // Arrange
        FeatureProvider.Features.Add(FeatureNames.RoutesToProfessionalStatus);
        var allGuidsToDisplay = ExemptionReasonCategories.ExemptionReasonIds;
        var route = await ReferenceDataCache.GetRouteToProfessionalStatusTypeByIdAsync(RouteToProfessionalStatusType.ApplyForQtsId);
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
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(ProfessionalStatusStatus.Awarded)
                .WithAwardedDate(awardedDate)
                .WithInductionExemption(true)));
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

    public static IEnumerable<object[]> SpecificInductionExemptedRoutesRequiringFilteredExemptionReasonsData()
    {
        yield return new object[] { RouteToProfessionalStatusType.ScotlandRId };
        yield return new object[] { RouteToProfessionalStatusType.NiRId };
    }
    [Theory]
    [MemberData(nameof(SpecificInductionExemptedRoutesRequiringFilteredExemptionReasonsData))]
    public async Task Get_RoutesFeatureFlagOn_PersonHasSomeSpecificRoutes_ShowsFilteredExceptionReasonsAndEditOnRouteMessage(Guid routeId)
    {
        // Arrange
        var allGuidsToDisplay = ExemptionReasonCategories.ExemptionReasonIds;
        var route = await ReferenceDataCache.GetRouteToProfessionalStatusTypeByIdAsync(routeId);
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
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(ProfessionalStatusStatus.Awarded)
                .WithAwardedDate(awardedDate)
                .WithInductionExemption(true)));
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
        var expectedMessage2 = $"To add/remove the induction exemption reason of: \"{route.InductionExemptionReason?.Name}\" please modify the \"{route.Name}\" route.";
        var messagesDisplayed = doc.GetElementsByClassName("govuk-inset-text").ToArray();
        Assert.Equal(expectedMessage1, messagesDisplayed[0].TextContent.Trim());
        Assert.Equal(expectedMessage2, messagesDisplayed[1].TextContent.Trim());
    }

    [Theory]
    [MemberData(nameof(SpecificInductionExemptedRoutesRequiringFilteredExemptionReasonsData))]
    public async Task Get_RoutesFeatureFlagOff_PersonHasSomeSpecificRoutes_ShowsAllExceptionReasonsAndNoMessage(Guid routeId)
    {
        // Arrange
        FeatureProvider.Features.Remove(FeatureNames.RoutesToProfessionalStatus);
        var allGuidsToDisplay = ExemptionReasonCategories.ExemptionReasonIds;
        var route = await ReferenceDataCache.GetRouteToProfessionalStatusTypeByIdAsync(routeId);
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
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(ProfessionalStatusStatus.Awarded)
                .WithAwardedDate(awardedDate)
                .WithInductionExemption(true)));
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

        Assert.Empty(doc.GetElementsByClassName("govuk-inset-text"));
    }

    public static IEnumerable<object[]> InductionExemptedRoutesRequiringRouteExemptionMessageData()
    {
        yield return new object[] { RouteToProfessionalStatusType.ScotlandRId, true };
        yield return new object[] { RouteToProfessionalStatusType.NiRId, true };
        yield return new object[] { RouteToProfessionalStatusType.QtlsAndSetMembershipId, true };
        yield return new object[] { RouteToProfessionalStatusType.ScotlandRId, false };
        yield return new object[] { RouteToProfessionalStatusType.NiRId, false };
        yield return new object[] { RouteToProfessionalStatusType.QtlsAndSetMembershipId, false };
    }
    [Theory]
    [MemberData(nameof(InductionExemptedRoutesRequiringRouteExemptionMessageData))]
    public async Task Get_RoutesFeatureFlagOn_PersonHasInductionExemptionFromRoute_ShowsExpectedMessageContent(Guid routeId, bool hasExemption)
    {
        // Arrange
        var allGuidsToDisplay = ExemptionReasonCategories.ExemptionReasonIds;
        var route = await ReferenceDataCache.GetRouteToProfessionalStatusTypeByIdAsync(routeId);
        var awardedDate = Clock.Today;

        var person = await TestData.CreatePersonAsync(p => p
            .WithQts()
            .WithProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(ProfessionalStatusStatus.Awarded)
                .WithAwardedDate(awardedDate)
                .WithInductionExemption(hasExemption)));
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

        if (hasExemption)
        {
            var expectedMessage = $"This person has an induction exemption \"{route.InductionExemptionReason?.Name}\" on the \"{route.Name}\" route.";
            var messageDisplayed = doc.GetElementsByClassName("govuk-inset-text").Where(e => e.TextContent.Contains("This person has an induction exemption")).Single();
            Assert.Equal(expectedMessage, messageDisplayed.TextContent.Trim());
        }
        else
        {
            Assert.Empty(doc.GetElementsByClassName("govuk-inset-text").Where(e => e.TextContent.Contains("This person has an induction exemption")));
        }
    }

    [Theory]
    [MemberData(nameof(InductionExemptedRoutesRequiringRouteExemptionMessageData))]
    public async Task Get_RoutesFeatureFlagOff_PersonHasInductionExemptionFromRoute_DoesntShowMessage(Guid routeId, bool hasExemption)
    {
        // Arrange
        FeatureProvider.Features.Remove(FeatureNames.RoutesToProfessionalStatus);
        var allGuidsToDisplay = ExemptionReasonCategories.ExemptionReasonIds;
        var route = await ReferenceDataCache.GetRouteToProfessionalStatusTypeByIdAsync(routeId);
        var awardedDate = Clock.Today;

        var person = await TestData.CreatePersonAsync(p => p
            .WithQts()
            .WithProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(ProfessionalStatusStatus.Awarded)
                .WithAwardedDate(awardedDate)
                .WithInductionExemption(hasExemption)));
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

        Assert.Empty(doc.GetElementsByClassName("govuk-inset-text"));
    }

    [Fact]
    public async Task Get_WithExemptionReasonsSelected_ShowsExpected()
    {
        var selectedExemptionReasonIds = ExemptionReasonCategories.ExemptionReasonIds
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
        var exemptionReasonIds = ExemptionReasonCategories.ExemptionReasonIds
            .RandomSelection(2)
            .ToArray();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionStateBuilder()
                .WithInitializedState(InductionStatus.Exempt, InductionJourneyPage.Status)
                .Build());

        var postRequest = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/edit-induction/exemption-reasons?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new EditInductionPostRequestContentBuilder()
                .WithExemptionReasonIds(exemptionReasonIds)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(exemptionReasonIds, journeyInstance.State.ExemptionReasonIds);
    }

    private Task<JourneyInstance<EditInductionState>> CreateJourneyInstanceAsync(Guid personId, EditInductionState? state = null) =>
    CreateJourneyInstance(
        JourneyNames.EditInduction,
        state ?? new EditInductionState(),
        new KeyValuePair<string, object>("personId", personId));
}
