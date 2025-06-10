using AngleSharp.Html.Dom;
using TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.DeleteRoute;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.RoutesToProfessionalStatus.DeleteRoute;

public class CheckYourAnswersTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Cancel_DeletesJourneyAndRedirectsToExpectedPage()
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).Where(r => r.Name == "NI R").Single();
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(RouteToProfessionalStatusStatus.Deferred)));
        var qualificationid = person.ProfessionalStatuses.First().QualificationId;
        var deleteRouteState = new DeleteRouteState()
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

        var request = new HttpRequestMessage(HttpMethod.Get, $"/route/{qualificationid}/delete/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

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
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).Where(r => r.Name == "NI R").Single();
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(RouteToProfessionalStatusStatus.Deferred)));
        var qualificationid = person.ProfessionalStatuses.First().QualificationId;
        var deleteRouteState = new DeleteRouteState()
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

        var request = new HttpRequestMessage(HttpMethod.Post, $"/route/{qualificationid}/delete/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

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
        var awardedDate = endDate.AddDays(1);
        var route = await ReferenceDataCache.GetRouteWhereAllFieldsApplyAsync();
        var status = TestDataHelper.GetRouteStatusWhereAllFieldsApply();
        var subjects = (await ReferenceDataCache.GetTrainingSubjectsAsync()).Take(1);
        var trainingProvider = (await ReferenceDataCache.GetTrainingProvidersAsync()).RandomOne();
        var degreeType = (await ReferenceDataCache.GetDegreeTypesAsync()).RandomOne();
        var country = (await ReferenceDataCache.GetTrainingCountriesAsync()).RandomOne();
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(status)
                .WithTrainingStartDate(startDate)
                .WithTrainingEndDate(endDate)
                .WithAwardedDate(awardedDate)
                .WithTrainingProviderId(trainingProvider.TrainingProviderId)
                .WithTrainingCountryId(country.CountryId)
                .WithTrainingSubjectIds(subjects.Select(s => s.TrainingSubjectId).ToArray())
                .WithTrainingAgeSpecialismType(TrainingAgeSpecialismType.FoundationStage)
                .WithDegreeTypeId(degreeType.DegreeTypeId)
                .WithInductionExemption(true)));

        var qualificationid = person.ProfessionalStatuses.First().QualificationId;
        var deleteRouteState = new DeleteRouteState()
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

        var request = new HttpRequestMessage(HttpMethod.Get, $"/route/{qualificationid}/delete/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertRowContentMatches("Route", route.Name);
        doc.AssertRowContentMatches("Status", status.GetTitle());
        doc.AssertRowContentMatches("Start date", startDate.ToString(UiDefaults.DateOnlyDisplayFormat));
        doc.AssertRowContentMatches("End date", endDate.ToString(UiDefaults.DateOnlyDisplayFormat));
        doc.AssertRowContentMatches("Award date", awardedDate.ToString(UiDefaults.DateOnlyDisplayFormat));
        doc.AssertRowContentMatches("Training provider", trainingProvider.Name);
        doc.AssertRowContentMatches("Degree type", degreeType.Name);
        doc.AssertRowContentMatches("Country of training", country.Name);
        doc.AssertRowContentMatches("Age range", "Foundation stage");
        doc.AssertRowContentMatches("Subjects", subjects.Select(s => s.Name));
    }

    [Fact]
    public async Task Get_ShowsChangeReasonAnswers_AsExpected()
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).Where(r => r.Name == "NI R").Single();
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(RouteToProfessionalStatusStatus.Deferred)));
        var qualificationid = person.ProfessionalStatuses.First().QualificationId;
        var deleteRouteState = new DeleteRouteState()
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

        var request = new HttpRequestMessage(HttpMethod.Get, $"/route/{qualificationid}/delete/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertRowContentMatches("Reason for change", deleteRouteState.ChangeReason!.GetDisplayName()!);
        doc.AssertRowContentMatches("Additional information", deleteRouteState.ChangeReasonDetail!.ChangeReasonDetail!);
        doc.AssertRowContentMatches("Evidence", "Not provided");
    }

    [Fact]
    public async Task Post_Confirm_DeletesRecordCreatesEventCompletesJourneyAndRedirectsWithFlashMessage()
    {
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).RandomOne();

        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(RouteToProfessionalStatusStatus.InTraining)));
        EventPublisher.Clear();

        var qualificationId = person.ProfessionalStatuses.First().QualificationId;
        var deleteRouteState = new DeleteRouteState()
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

        var request = new HttpRequestMessage(HttpMethod.Post, $"/route/{qualificationId}/delete/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var redirectResponse = await response.FollowRedirectAsync(HttpClient);
        var redirectDoc = await redirectResponse.GetDocumentAsync();
        AssertEx.HtmlDocumentHasFlashSuccess(redirectDoc, "Route to professional status deleted");

        await WithDbContext(async dbContext => Assert.Null(await dbContext.RouteToProfessionalStatuses.FirstOrDefaultAsync(p => p.QualificationId == qualificationId)));

        var RaisedBy = GetCurrentUserId();

        EventPublisher.AssertEventsSaved(e =>
        {
            var deletedEvent = Assert.IsType<RouteToProfessionalStatusDeletedEvent>(e);

            Assert.Equal(Clock.UtcNow, deletedEvent.CreatedUtc);
            Assert.Equal(person.PersonId, deletedEvent.PersonId);
            Assert.Equal(journeyInstance.State.ChangeReason!.GetDisplayName(), deletedEvent.DeletionReason);
            Assert.Equal(journeyInstance.State.ChangeReasonDetail.ChangeReasonDetail, deletedEvent.DeletionReasonDetail);
            Assert.Equal(journeyInstance.State.ChangeReasonDetail.EvidenceFileId, deletedEvent.EvidenceFile?.FileId);
            Assert.Equal(journeyInstance.State.ChangeReasonDetail.EvidenceFileName, deletedEvent.EvidenceFile?.Name);
            Assert.Equal(RouteToProfessionalStatusDeletedEventChanges.None, deletedEvent.Changes);
        });

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.True(journeyInstance.Completed);
    }

    [Fact]
    public async Task Post_Confirm_WithAwardedQtsRouteTypeUpdatesPersonQtsDateAndHasChangesInEvent()
    {
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.ProfessionalStatusType == ProfessionalStatusType.QualifiedTeacherStatus)
            .RandomOne();
        var status = RouteToProfessionalStatusStatus.Awarded;
        var qtsDate = Clock.Today.AddYears(-1);
        var person = await TestData.CreatePersonAsync(p => p
            .WithQts(qtsDate)
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(status)
                .WithAwardedDate(qtsDate)));
        EventPublisher.Clear();

        var qualificationId = person.ProfessionalStatuses.First().QualificationId;
        var deleteRouteState = new DeleteRouteState()
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

        var request = new HttpRequestMessage(HttpMethod.Post, $"/route/{qualificationId}/delete/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var updatedPerson = await WithDbContext(dbContext => dbContext.Persons.SingleAsync(p => p.PersonId == person.PersonId));
        Assert.Null(updatedPerson.QtsDate);

        var RaisedByUserId = GetCurrentUserId();

        EventPublisher.AssertEventsSaved(e =>
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
    public async Task Post_Confirm_WithAwardedQtsRouteType_UpdatesPersonQtsDateWithOlderRouteDateAndHasChangesInEvent()
    {
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.ProfessionalStatusType == ProfessionalStatusType.QualifiedTeacherStatus)
            .RandomOne();
        var awardedDateEarliest = Clock.Today.AddYears(-1);
        var awardedDateLatest = awardedDateEarliest.AddMonths(1);
        var person = await TestData.CreatePersonAsync(p => p
            .WithQts(awardedDateEarliest)
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(RouteToProfessionalStatusStatus.Awarded)
                .WithAwardedDate(awardedDateEarliest))
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(RouteToProfessionalStatusStatus.Awarded)
                .WithAwardedDate(awardedDateLatest)));
        EventPublisher.Clear();

        var qualificationIdEarliestDate = person.ProfessionalStatuses.Single(p => p.AwardedDate == awardedDateEarliest).QualificationId;
        var qualificationIdLatestDate = person.ProfessionalStatuses.Single(p => p.AwardedDate == awardedDateLatest).QualificationId;
        var deleteRouteState = new DeleteRouteState()
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

        var request = new HttpRequestMessage(HttpMethod.Post, $"/route/{qualificationIdEarliestDate}/delete/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var updatedPerson = await WithDbContext(dbContext => dbContext.Persons.SingleAsync(p => p.PersonId == person.PersonId));
        Assert.Equal(awardedDateLatest, updatedPerson.QtsDate);

        EventPublisher.AssertEventsSaved(e =>
        {
            var deletedEvent = Assert.IsType<RouteToProfessionalStatusDeletedEvent>(e);
            Assert.Equal(Clock.UtcNow, deletedEvent.CreatedUtc);
            Assert.Equal(person.PersonId, deletedEvent.PersonId);
            Assert.Equal(awardedDateEarliest, deletedEvent.OldPersonAttributes.QtsDate);
            Assert.Equal(awardedDateLatest, deletedEvent.PersonAttributes.QtsDate);
            Assert.Equal(RouteToProfessionalStatusDeletedEventChanges.PersonQtsDate, deletedEvent.Changes);
        });

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.True(journeyInstance.Completed);
    }

    private Task<JourneyInstance<DeleteRouteState>> CreateJourneyInstanceAsync(Guid qualificationId, DeleteRouteState? state = null) =>
        CreateJourneyInstance(
            JourneyNames.DeleteRouteToProfessionalStatus,
            state ?? new DeleteRouteState(),
            new KeyValuePair<string, object>("qualificationId", qualificationId));
}
