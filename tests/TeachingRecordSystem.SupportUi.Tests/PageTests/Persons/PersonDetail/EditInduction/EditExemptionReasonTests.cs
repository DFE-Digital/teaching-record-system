using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Services.InductionExemptions;
using TeachingRecordSystem.Core.Services.Persons;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.EditInduction;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail.EditInduction;

public class EditExemptionReasonTests(HostFixture hostFixture) : EditInductionTestBase(hostFixture)
{
    [Fact]
    public async Task Get_InvalidInductionStatusForPage_RedirectToStart()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p.WithQts());
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionState
            {
                InductionStatus = InductionStatus.InProgress,
                CurrentInductionStatus = InductionStatus.InProgress
            });

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
            new EditInductionState
            {
                InductionStatus = InductionStatus.Exempt,
                CurrentInductionStatus = InductionStatus.Exempt
            });

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
            new EditInductionState
            {
                InductionStatus = InductionStatus.Exempt,
                CurrentInductionStatus = InductionStatus.Exempt
            });

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
        var holdsFromDate = TimeProvider.Today;
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
            new EditInductionState
            {
                InductionStatus = InductionStatus.Exempt,
                CurrentInductionStatus = InductionStatus.Exempt
            });

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
        var holdsFromDate = TimeProvider.Today;
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
            new EditInductionState
            {
                InductionStatus = InductionStatus.Exempt,
                CurrentInductionStatus = InductionStatus.Exempt
            });

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
        var holdsFromDate = TimeProvider.Today;

        var person = await TestData.CreatePersonAsync(p => p
            .WithQts()
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(RouteToProfessionalStatusStatus.Holds)
                .WithHoldsFrom(holdsFromDate)
                .WithInductionExemption(hasExemption)));
        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new EditInductionState
            {
                InductionStatus = InductionStatus.Exempt,
                CurrentInductionStatus = InductionStatus.Exempt
            });

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
            new EditInductionState
            {
                InductionStatus = InductionStatus.Exempt,
                CurrentInductionStatus = InductionStatus.Exempt,
                ExemptionReasonIds = selectedExemptionReasonIds
            });

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
            new EditInductionState
            {
                InductionStatus = InductionStatus.Exempt,
                CurrentInductionStatus = InductionStatus.Exempt
            });

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
            new EditInductionState
            {
                InductionStatus = InductionStatus.Exempt,
                CurrentInductionStatus = InductionStatus.Exempt
            });

        var postRequest = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/edit-induction/exemption-reasons?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new EditInductionPostRequestContentBuilder()
                .WithExemptionReasonIds(exemptionReasonIds)
                .BuildFormUrlEncoded()
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        var state = GetJourneyInstanceState(journeyInstance)!;
        Assert.Equal(exemptionReasonIds, state.ExemptionReasonIds);
    }

    [Theory]
    [InlineData(InductionStatus.Exempt, "edit-induction/reason")]
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

        var request = new HttpRequestMessage(HttpMethod.Post, $"/persons/{person.PersonId}/edit-induction/exemption-reasons?{journeyInstance.GetUniqueIdQueryParameter()}")
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
        var inductionStatus = InductionStatus.Exempt;
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
            $"/persons/{person.PersonId}/edit-induction/exemption-reasons?{journeyInstance.GetUniqueIdQueryParameter()}")
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
        var inductionStatus = InductionStatus.Exempt;
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
            $"/persons/{person.PersonId}/edit-induction/exemption-reasons?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder().Add("Cancel", bool.TrueString)
        });

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        FileServiceMock.AssertFileWasDeleted(evidenceFileId);
    }

    [Theory]
    [InlineData(StartPage.Status, InductionStatus.Exempt, "edit-induction/status")]
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

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/edit-induction/exemption-reasons?{journeyInstance.GetUniqueIdQueryParameter()}");

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
            $"/persons/{person.PersonId}/edit-induction/exemption-reasons?returnUrl={Uri.EscapeDataString(checkAnswersUrl)}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var document = await response.GetDocumentAsync();
        var backlink = document.GetElementByTestId("back-link") as IHtmlAnchorElement;
        Assert.Contains($"/persons/{person.PersonId}/edit-induction/check-answers", backlink!.Href);
    }

    [Theory]
    [InlineData(InductionStatus.Exempt, "edit-induction/check-answers")]
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
            $"/persons/{person.PersonId}/edit-induction/exemption-reasons?returnUrl={Uri.EscapeDataString(checkAnswersUrl)}&{journeyInstance.GetUniqueIdQueryParameter()}")
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
}
