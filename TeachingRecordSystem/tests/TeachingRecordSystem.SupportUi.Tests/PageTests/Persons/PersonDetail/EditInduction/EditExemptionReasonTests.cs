using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.InductionExemptions;
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
    public async Task Get_ReferenceDataExemptionsValuesNotInExemptionReasonCategories_NotShown()
    {
        // Arrange
        var allGuidsToDisplay = InductionExemptionService.ExemptionReasonIds;
        var referenceDataExemptionReasons = await TestData.ReferenceDataCache.GetPersonLevelInductionExemptionReasonsAsync(activeOnly: true);

        referenceDataExemptionReasons.ToList().Add(new()
        {
            InductionExemptionReasonId = new Guid(),
            IsActive = true,
            Name = "An exemption reason not allowed for in the page",
            RouteImplicitExemption = false,
            RouteOnlyExemption = false
        });

        var exemptionReasonsForDisplay = allGuidsToDisplay.Join(
            referenceDataExemptionReasons,
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
    public async Task Get_ShowsExemptionReasonsList()
    {
        // Arrange
        var allGuidsToDisplay = InductionExemptionService.ExemptionReasonIds;
        var exemptionReasons = (await TestData.ReferenceDataCache.GetPersonLevelInductionExemptionReasonsAsync(activeOnly: true))
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
            Assert.Contains(checkbox.Value, exemptionReasons.Select(e => e.InductionExemptionReasonId.ToString()));
            Assert.Contains(checkbox.ParentElement!.QuerySelector<IHtmlLabelElement>($"label[for='{checkbox.Id}']")!.TrimmedText(), exemptionReasons.Select(e => e.Name));
        });
    }

    [Fact]
    public async Task Get_PersonHasRouteWithInductionExemption_ShowsExemptionReasonsList()
    {
        // Arrange
        var allGuidsToDisplay = InductionExemptionService.RouteFeatureExemptionReasonIds;
        var route = await ReferenceDataCache.GetRouteToProfessionalStatusTypeByIdAsync(RouteToProfessionalStatusType.ApplyForQtsId);
        var holdsFromDate = Clock.Today;
        var exemptionReasons = (await TestData.ReferenceDataCache.GetPersonLevelInductionExemptionReasonsAsync(activeOnly: true))
            .ToArray();
        var exemptionReasonsForDisplay = allGuidsToDisplay
            .Join(exemptionReasons,
                guid => guid,
                exemption => exemption.InductionExemptionReasonId,
                (guid, exemption) => new { guid, exemption.Name })
            .ToArray();
        var person = await TestData.CreatePersonAsync(p => p
            .WithQts()
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(RouteToProfessionalStatusStatus.Holds)
                .WithHoldsFrom(holdsFromDate)
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
        yield return [RouteToProfessionalStatusType.ScotlandRId];
        yield return [RouteToProfessionalStatusType.NiRId];
    }
    [Theory]
    [MemberData(nameof(SpecificInductionExemptedRoutesRequiringFilteredExemptionReasonsData))]
    public async Task Get_PersonHasSomeSpecificRoutes_ShowsFilteredExceptionReasonsAndEditOnRouteMessage(Guid routeId)
    {
        // Arrange
        var allGuidsToDisplay = InductionExemptionService.ExemptionReasonIds;
        var route = await ReferenceDataCache.GetRouteToProfessionalStatusTypeByIdAsync(routeId);
        var holdsFromDate = Clock.Today;
        var exemptionReasons = (await TestData.ReferenceDataCache.GetPersonLevelInductionExemptionReasonsAsync(activeOnly: true))
            .ToArray();
        var exemptionReasonsForDisplay = allGuidsToDisplay
            .Where(guid => guid != route.InductionExemptionReason?.InductionExemptionReasonId) // exclude the holds route exemption reason
            .Join(exemptionReasons,
                guid => guid,
                exemption => exemption.InductionExemptionReasonId,
                (guid, exemption) => new { guid, exemption.Name })
            .ToArray();
        var person = await TestData.CreatePersonAsync(p => p
            .WithQts()
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(RouteToProfessionalStatusStatus.Holds)
                .WithHoldsFrom(holdsFromDate)
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

    public static IEnumerable<object[]> InductionExemptedRoutesRequiringRouteExemptionMessageData()
    {
        yield return [RouteToProfessionalStatusType.ScotlandRId, true];
        yield return [RouteToProfessionalStatusType.NiRId, true];
        yield return [RouteToProfessionalStatusType.QtlsAndSetMembershipId, true];
        yield return [RouteToProfessionalStatusType.ScotlandRId, false];
        yield return [RouteToProfessionalStatusType.NiRId, false];
        yield return [RouteToProfessionalStatusType.QtlsAndSetMembershipId, false];
    }

    [Theory]
    [MemberData(nameof(InductionExemptedRoutesRequiringRouteExemptionMessageData))]
    public async Task Get_PersonHasInductionExemptionFromRoute_ShowsExpectedMessageContent(Guid routeId, bool hasExemption)
    {
        // Arrange
        var allGuidsToDisplay = InductionExemptionService.ExemptionReasonIds;
        var route = await ReferenceDataCache.GetRouteToProfessionalStatusTypeByIdAsync(routeId);
        var holdsFromDate = Clock.Today;

        var person = await TestData.CreatePersonAsync(p => p
            .WithQts()
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(RouteToProfessionalStatusStatus.Holds)
                .WithHoldsFrom(holdsFromDate)
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
            var messageDisplayed = doc.GetElementsByClassName("govuk-inset-text").Single(e => e.TextContent.Contains("This person has an induction exemption"));
            Assert.Equal(expectedMessage, messageDisplayed.TextContent.Trim());
        }
        else
        {
            Assert.DoesNotContain(doc.GetElementsByClassName("govuk-inset-text"), e => e.TextContent.Contains("This person has an induction exemption"));
        }
    }

    [Fact]
    public async Task Get_WithExemptionReasonsSelected_ShowsExpected()
    {
        var selectedExemptionReasonIds = InductionExemptionService.ExemptionReasonIds
            .TakeRandom(2)
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
        var exemptionReasonIds = InductionExemptionService.ExemptionReasonIds
            .TakeRandom(2)
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
