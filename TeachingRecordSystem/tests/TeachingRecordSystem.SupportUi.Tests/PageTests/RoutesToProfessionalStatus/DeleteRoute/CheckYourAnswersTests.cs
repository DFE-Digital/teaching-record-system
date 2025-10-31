using AngleSharp.Html.Dom;
using TeachingRecordSystem.Core.Events.Legacy;
using TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.DeleteRoute;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.RoutesToProfessionalStatus.DeleteRoute;

public class CheckYourAnswersTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Cancel_DeletesJourneyAndRedirectsToExpectedPage()
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).Where(r => r.Name == "Northern Irish Recognition").Single();
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(RouteToProfessionalStatusStatus.Deferred)));
        var qualificationid = person.ProfessionalStatuses.First().QualificationId;
        var deleteRouteState = new DeleteRouteState
        {
            ChangeReason = ChangeReasonOption.RemovedQtlsStatus,
            ChangeReasonDetail = new ChangeReasonStateBuilder()
                .WithValidChangeReasonDetail()
                .Build()
        };

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationid,
            deleteRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Get, $"/routes/{qualificationid}/delete/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

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
        Assert.Equal($"/persons/{person.PersonId}/qualifications", location);
        Assert.Null(await ReloadJourneyInstance(journeyInstance));
    }

    [Fact]
    public async Task Post_RedirectsToExpectedPage()
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).Where(r => r.Name == "Northern Irish Recognition").Single();
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(RouteToProfessionalStatusStatus.Deferred)));
        var qualificationid = person.ProfessionalStatuses.First().QualificationId;
        var deleteRouteState = new DeleteRouteState
        {
            ChangeReason = ChangeReasonOption.RemovedQtlsStatus,
            ChangeReasonDetail = new ChangeReasonStateBuilder()
                .WithValidChangeReasonDetail()
                .Build()
        };

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationid,
            deleteRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Post, $"/routes/{qualificationid}/delete/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        var location = response.Headers.Location?.OriginalString;
        Assert.Equal($"/persons/{person.PersonId}/qualifications", location);
    }

    [Fact]
    public async Task Get_ShowsAnswers_AsExpected()
    {
        // Arrange
        var startDate = Clock.Today.AddYears(-1);
        var endDate = Clock.Today.AddDays(-1);
        var holdsFrom = endDate.AddDays(1);
        var route = await ReferenceDataCache.GetRouteWhereAllFieldsApplyAsync();
        var status = TestDataHelper.GetRouteStatusWhereAllFieldsApply();
        var subjects = (await ReferenceDataCache.GetTrainingSubjectsAsync()).Where(s => !s.Name.Contains('\'')).Take(1);
        var trainingProvider = (await ReferenceDataCache.GetTrainingProvidersAsync()).Where(s => !s.Name.Contains('\'')).SingleRandom();
        var degreeType = (await ReferenceDataCache.GetDegreeTypesAsync()).SingleRandom();
        var country = (await ReferenceDataCache.GetTrainingCountriesAsync()).SingleRandom();
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(status)
                .WithTrainingStartDate(startDate)
                .WithTrainingEndDate(endDate)
                .WithHoldsFrom(holdsFrom)
                .WithTrainingProviderId(trainingProvider.TrainingProviderId)
                .WithTrainingCountryId(country.CountryId)
                .WithTrainingSubjectIds(subjects.Select(s => s.TrainingSubjectId).ToArray())
                .WithTrainingAgeSpecialismType(TrainingAgeSpecialismType.FoundationStage)
                .WithDegreeTypeId(degreeType.DegreeTypeId)
                .WithInductionExemption(true)));

        var qualificationid = person.ProfessionalStatuses.First().QualificationId;
        var deleteRouteState = new DeleteRouteState
        {
            ChangeReason = ChangeReasonOption.RemovedQtlsStatus,
            ChangeReasonDetail = new ChangeReasonStateBuilder()
                .WithValidChangeReasonDetail()
                .Build()
        };

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationid,
            deleteRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Get, $"/routes/{qualificationid}/delete/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertSummaryListRowValueContentMatches("Route", route.Name);
        doc.AssertSummaryListRowValueContentMatches("Status", status.GetTitle());
        doc.AssertSummaryListRowValueContentMatches("Start date", startDate.ToString(UiDefaults.DateOnlyDisplayFormat));
        doc.AssertSummaryListRowValueContentMatches("End date", endDate.ToString(UiDefaults.DateOnlyDisplayFormat));
        doc.AssertSummaryListRowValueContentMatches("Held since", holdsFrom.ToString(UiDefaults.DateOnlyDisplayFormat));
        doc.AssertSummaryListRowValueContentMatches("Training provider", trainingProvider.Name);
        doc.AssertSummaryListRowValueContentMatches("Degree type", degreeType.Name);
        doc.AssertSummaryListRowValueContentMatches("Country of training", country.Name);
        doc.AssertSummaryListRowValueContentMatches("Age range", "Foundation stage");
        doc.AssertSummaryListRowValueContentMatches("Subjects", subjects.Select(s => $"{s.Reference} - {s.Name}"));
    }

    [Fact]
    public async Task Get_ShowsChangeReasonAnswers_AsExpected()
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).Where(r => r.Name == "Northern Irish Recognition").Single();
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(RouteToProfessionalStatusStatus.Deferred)));
        var qualificationid = person.ProfessionalStatuses.First().QualificationId;
        var deleteRouteState = new DeleteRouteState
        {
            ChangeReason = ChangeReasonOption.RemovedQtlsStatus,
            ChangeReasonDetail = new ChangeReasonStateBuilder()
                .WithValidChangeReasonDetail()
                .Build()
        };

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationid,
            deleteRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Get, $"/routes/{qualificationid}/delete/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertSummaryListRowValueContentMatches("Reason", deleteRouteState.ChangeReason!.GetDisplayName()!);
        doc.AssertSummaryListRowValueContentMatches("More details", deleteRouteState.ChangeReasonDetail!.ChangeReasonDetail!);
        doc.AssertSummaryListRowValueContentMatches("Evidence", "Not provided");
    }

    [Fact]
    public async Task Post_Confirm_DeletesRecordCreatesEventCompletesJourneyAndRedirectsWithFlashMessage()
    {
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).SingleRandom();

        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(RouteToProfessionalStatusStatus.InTraining)));
        EventObserver.Clear();

        var qualificationId = person.ProfessionalStatuses.First().QualificationId;
        var deleteRouteState = new DeleteRouteState
        {
            ChangeReason = ChangeReasonOption.RemovedQtlsStatus,
            ChangeReasonDetail = new ChangeReasonStateBuilder()
                .WithValidChangeReasonDetail()
                .Build()
        };

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            deleteRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Post, $"/routes/{qualificationId}/delete/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var redirectResponse = await response.FollowRedirectAsync(HttpClient);
        var redirectDoc = await redirectResponse.GetDocumentAsync();
        AssertEx.HtmlDocumentHasFlashSuccess(redirectDoc, "Route to professional status deleted");

        await WithDbContextAsync(async dbContext => Assert.Null(await dbContext.RouteToProfessionalStatuses.FirstOrDefaultAsync(p => p.QualificationId == qualificationId)));

        var RaisedBy = GetCurrentUserId();

        EventObserver.AssertEventsSaved(e =>
        {
            var deletedEvent = Assert.IsType<RouteToProfessionalStatusDeletedEvent>(e);

            Assert.Equal(Clock.UtcNow, deletedEvent.CreatedUtc);
            Assert.Equal(person.PersonId, deletedEvent.PersonId);
            Assert.Equal(journeyInstance.State.ChangeReason!.GetDisplayName(), deletedEvent.DeletionReason);
            Assert.Equal(journeyInstance.State.ChangeReasonDetail.ChangeReasonDetail, deletedEvent.DeletionReasonDetail);
            Assert.Null(deletedEvent.EvidenceFile);
            Assert.Equal(RouteToProfessionalStatusDeletedEventChanges.None, deletedEvent.Changes);
        });

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.True(journeyInstance.Completed);
    }

    [Fact]
    public async Task Post_Confirm_WithHoldsQtsRouteTypeUpdatesPersonQtsDateAndHasChangesInEvent()
    {
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.ProfessionalStatusType == ProfessionalStatusType.QualifiedTeacherStatus)
            .SingleRandom();
        var status = RouteToProfessionalStatusStatus.Holds;
        var qtsDate = Clock.Today.AddYears(-1);
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(status)
                .WithHoldsFrom(qtsDate)));
        EventObserver.Clear();

        var qualificationId = person.ProfessionalStatuses.First().QualificationId;
        var deleteRouteState = new DeleteRouteState
        {
            ChangeReason = ChangeReasonOption.RemovedQtlsStatus,
            ChangeReasonDetail = new ChangeReasonStateBuilder()
                .WithValidChangeReasonDetail()
                .Build()
        };

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            deleteRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Post, $"/routes/{qualificationId}/delete/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var updatedPerson = await WithDbContextAsync(dbContext => dbContext.Persons.SingleAsync(p => p.PersonId == person.PersonId));
        Assert.Null(updatedPerson.QtsDate);

        var RaisedByUserId = GetCurrentUserId();

        EventObserver.AssertEventsSaved(e =>
        {
            var deletedEvent = Assert.IsType<RouteToProfessionalStatusDeletedEvent>(e);
            Assert.Equal(RaisedByUserId, deletedEvent.RaisedBy.UserId);
            Assert.Equal(Clock.UtcNow, deletedEvent.CreatedUtc);
            Assert.Equal(person.PersonId, deletedEvent.PersonId);
            Assert.Equal(qtsDate, deletedEvent.OldPersonAttributes.QtsDate);
            Assert.Null(deletedEvent.PersonAttributes.QtsDate);
            Assert.True(deletedEvent.Changes.HasFlag(RouteToProfessionalStatusDeletedEventChanges.PersonQtsDate));
        });

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.True(journeyInstance.Completed);
    }

    [Fact]
    public async Task Post_Confirm_WithHoldsQtsRouteType_UpdatesPersonQtsDateWithOlderRouteDateAndHasChangesInEvent()
    {
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.ProfessionalStatusType == ProfessionalStatusType.QualifiedTeacherStatus)
            .SingleRandom();
        var holdsFromEarliest = Clock.Today.AddYears(-1);
        var holdsFromLatest = holdsFromEarliest.AddMonths(1);
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(RouteToProfessionalStatusStatus.Holds)
                .WithHoldsFrom(holdsFromEarliest))
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(RouteToProfessionalStatusStatus.Holds)
                .WithHoldsFrom(holdsFromLatest)));
        EventObserver.Clear();

        var qualificationIdEarliestDate = person.ProfessionalStatuses.Single(p => p.HoldsFrom == holdsFromEarliest).QualificationId;
        var qualificationIdLatestDate = person.ProfessionalStatuses.Single(p => p.HoldsFrom == holdsFromLatest).QualificationId;
        var deleteRouteState = new DeleteRouteState
        {
            ChangeReason = ChangeReasonOption.RemovedQtlsStatus,
            ChangeReasonDetail = new ChangeReasonStateBuilder()
                .WithValidChangeReasonDetail()
                .Build()
        };

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationIdEarliestDate,
            deleteRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Post, $"/routes/{qualificationIdEarliestDate}/delete/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var updatedPerson = await WithDbContextAsync(dbContext => dbContext.Persons.SingleAsync(p => p.PersonId == person.PersonId));
        Assert.Equal(holdsFromLatest, updatedPerson.QtsDate);

        EventObserver.AssertEventsSaved(e =>
        {
            var deletedEvent = Assert.IsType<RouteToProfessionalStatusDeletedEvent>(e);
            Assert.Equal(Clock.UtcNow, deletedEvent.CreatedUtc);
            Assert.Equal(person.PersonId, deletedEvent.PersonId);
            Assert.Equal(holdsFromEarliest, deletedEvent.OldPersonAttributes.QtsDate);
            Assert.Equal(holdsFromLatest, deletedEvent.PersonAttributes.QtsDate);
            Assert.Equal(RouteToProfessionalStatusDeletedEventChanges.PersonQtsDate, deletedEvent.Changes);
        });

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.True(journeyInstance.Completed);
    }

    [Theory]
    [HttpMethods(TestHttpMethods.GetAndPost)]
    public async Task PersonIsDeactivated_ReturnsBadRequest(HttpMethod httpMethod)
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).Where(r => r.Name == "Northern Irish Recognition").Single();
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(RouteToProfessionalStatusStatus.Deferred)));
        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(person.Person);
            person.Person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });
        var qualificationid = person.ProfessionalStatuses.First().QualificationId;
        var deleteRouteState = new DeleteRouteState
        {
            ChangeReason = ChangeReasonOption.RemovedQtlsStatus,
            ChangeReasonDetail = new ChangeReasonStateBuilder()
                .WithValidChangeReasonDetail()
                .Build()
        };

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationid,
            deleteRouteState
            );

        var request = new HttpRequestMessage(httpMethod, $"/routes/{qualificationid}/delete/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    private Task<JourneyInstance<DeleteRouteState>> CreateJourneyInstanceAsync(Guid qualificationId, DeleteRouteState? state = null) =>
        CreateJourneyInstance(
            JourneyNames.DeleteRouteToProfessionalStatus,
            state ?? new DeleteRouteState(),
            new KeyValuePair<string, object>("qualificationId", qualificationId));
}
