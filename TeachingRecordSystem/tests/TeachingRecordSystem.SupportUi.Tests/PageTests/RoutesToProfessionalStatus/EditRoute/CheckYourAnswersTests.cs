using AngleSharp.Html.Dom;
using TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.RoutesToProfessionalStatus.EditRoute;

public class CheckYourAnswersTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Cancel_RedirectsToExpectedPage()
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRoutesToProfessionalStatusAsync()).Where(r => r.Name == "NI R").Single();
        var person = await TestData.CreatePersonAsync(p => p
            .WithProfessionalStatus(r => r
                .WithRoute(route.RouteToProfessionalStatusId)
                .WithStatus(ProfessionalStatusStatus.Deferred)));
        var qualificationid = person.ProfessionalStatuses.First().QualificationId;
        var editRouteState = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusId)
            .WithStatus(ProfessionalStatusStatus.Deferred)
            .WithValidChangeReasonOption()
            .WithDefaultChangeReasonNoUploadFileDetail()
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationid,
            editRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Get, $"/route/{qualificationid}/edit/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

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
    }

    [Fact]
    public async Task Post_RedirectsToExpectedPage()
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRoutesToProfessionalStatusAsync()).Where(r => r.Name == "NI R").Single();
        var person = await TestData.CreatePersonAsync(p => p
            .WithProfessionalStatus(r => r
                .WithRoute(route.RouteToProfessionalStatusId)
                .WithStatus(ProfessionalStatusStatus.Deferred)));
        var qualificationid = person.ProfessionalStatuses.First().QualificationId;
        var editRouteState = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusId)
            .WithStatus(ProfessionalStatusStatus.Deferred)
            .WithValidChangeReasonOption()
            .WithDefaultChangeReasonNoUploadFileDetail()
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationid,
            editRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Post, $"/route/{qualificationid}/edit/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

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
        var endDate = Clock.Today;
        var route = (await ReferenceDataCache.GetRoutesToProfessionalStatusAsync()).Where(r => r.Name == "Apprenticeship").Single();
        var subjects = (await ReferenceDataCache.GetTrainingSubjectsAsync()).Take(1);
        var trainingProvider = (await ReferenceDataCache.GetTrainingProvidersAsync()).First();
        var person = await TestData.CreatePersonAsync(p => p
            .WithProfessionalStatus(r => r
                .WithRoute(route.RouteToProfessionalStatusId)
                .WithStatus(ProfessionalStatusStatus.InTraining)));

        var qualificationid = person.ProfessionalStatuses.First().QualificationId;
        var editRouteState = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusId)
            .WithStatus(ProfessionalStatusStatus.InTraining)
            .WithTrainingStartDate(startDate)
            .WithTrainingEndDate(endDate)
            .WithTrainingProviderId(trainingProvider.TrainingProviderId)
            .WithTrainingCountryId("GB")
            .WithTrainingSubjectIds(subjects.Select(s => s.TrainingSubjectId).ToArray())
            .WithTrainingAgeSpecialismType(TrainingAgeSpecialismType.FoundationStage)
            .WithValidChangeReasonOption()
            .WithDefaultChangeReasonNoUploadFileDetail()
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationid,
            editRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Get, $"/route/{qualificationid}/edit/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertRowContentMatches("Route", route.Name);
        doc.AssertRowContentMatches("Status", "In training");
        doc.AssertRowContentMatches("Start date", startDate.ToString(UiDefaults.DateOnlyDisplayFormat));
        doc.AssertRowContentMatches("End date", endDate.ToString(UiDefaults.DateOnlyDisplayFormat));
        //Assert.Null(doc.GetSummaryListRowForKey("Has Exemption"));
        doc.AssertRowContentMatches("Has exemption", "Not provided"); // CML TODO page will need to not show rows that don't apply to each RouteType and status combo
        doc.AssertRowContentMatches("Training provider", trainingProvider.Name);
        //doc.AssertRowContentMatches("Degree type", ); // CML TODO degree type not defined yet
        doc.AssertRowContentMatches("Country of training", "United Kingdom");
        doc.AssertRowContentMatches("Age range", "Foundation stage");
        doc.AssertRowContentMatches("Subjects", subjects.Select(s => s.Name));
    }

    [Fact]
    public async Task Get_ShowsOptionalAnswersNotPopulated_AsExpected()
    {
        // Arrange
        var startDate = Clock.Today.AddYears(-1);
        var endDate = Clock.Today;
        var route = (await ReferenceDataCache.GetRoutesToProfessionalStatusAsync()).Where(r => r.Name == "Apprenticeship").Single();
        var trainingProvider = (await ReferenceDataCache.GetTrainingProvidersAsync()).First();
        var person = await TestData.CreatePersonAsync(p => p
            .WithProfessionalStatus(r => r
                .WithRoute(route.RouteToProfessionalStatusId)
                .WithStatus(ProfessionalStatusStatus.InTraining)));

        var qualificationid = person.ProfessionalStatuses.First().QualificationId;
        var editRouteState = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusId)
            .WithStatus(ProfessionalStatusStatus.InTraining)
            .WithTrainingStartDate(startDate)
            .WithTrainingEndDate(endDate)
            .WithTrainingProviderId(trainingProvider.TrainingProviderId)
            .WithValidChangeReasonOption()
            .WithDefaultChangeReasonNoUploadFileDetail()
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationid,
            editRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Get, $"/route/{qualificationid}/edit/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertRowContentMatches("Route", route.Name);
        doc.AssertRowContentMatches("Status", "In training");
        doc.AssertRowContentMatches("Start date", startDate.ToString(UiDefaults.DateOnlyDisplayFormat));
        doc.AssertRowContentMatches("End date", endDate.ToString(UiDefaults.DateOnlyDisplayFormat));
        //Assert.Null(doc.GetSummaryListRowForKey("Has Exemption"));
        doc.AssertRowContentMatches("Has exemption", "Not provided"); // CML TODO page will need to not show rows that don't apply to each RouteType and status combo
        doc.AssertRowContentMatches("Training provider", trainingProvider.Name);
        //doc.AssertRowContentMatches("Degree type", ); // CML TODO degree type not defined yet
        doc.AssertRowContentMatches("Country of training", "Not provided");
        doc.AssertRowContentMatches("Age range", "Not provided");
        doc.AssertRowContentMatches("Subjects", "Not provided");
    }

    [Fact]
    public async Task Get_ShowsChangeReasonAnswers_AsExpected()
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRoutesToProfessionalStatusAsync()).Where(r => r.Name == "NI R").Single();
        var person = await TestData.CreatePersonAsync(p => p
            .WithProfessionalStatus(r => r
                .WithRoute(route.RouteToProfessionalStatusId)
                .WithStatus(ProfessionalStatusStatus.Deferred)));
        var qualificationid = person.ProfessionalStatuses.First().QualificationId;
        var editRouteState = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusId)
            .WithStatus(ProfessionalStatusStatus.Deferred)
            .WithValidChangeReasonOption()
            .WithDefaultChangeReasonNoUploadFileDetail()
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationid,
            editRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Get, $"/route/{qualificationid}/edit/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertRowContentMatches("Reason for change", editRouteState.ChangeReason!.GetDisplayName()!);
        doc.AssertRowContentMatches("Additional information", editRouteState.ChangeReasonDetail!.ChangeReasonDetail!);
        doc.AssertRowContentMatches("Evidence", "Not provided");
    }

    [Fact]
    public async Task Post_Confirm_UpdatesProfessionalStatusCreatesEventCompletesJourneyAndRedirectsWithFlashMessage()
    {
        var route = (await ReferenceDataCache.GetRoutesToProfessionalStatusAsync())
            .Where(r => r.ProfessionalStatusType == ProfessionalStatusType.QualifiedTeacherStatus)
            .RandomOne();
        var person = await TestData.CreatePersonAsync(p => p
            .WithProfessionalStatus(r => r
                .WithRoute(route.RouteToProfessionalStatusId)
                .WithStatus(ProfessionalStatusStatus.Deferred)));
        var qualificationid = person.ProfessionalStatuses.First().QualificationId;
        var editRouteState = (await (new EditRouteStateBuilder())
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusId)
            .WithPopulatedReferenceFieldsAsync(ReferenceDataCache))
            .WithAwardedStatusFields(Clock)
            .WithValidChangeReasonOption()
            .WithDefaultChangeReasonNoUploadFileDetail()
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationid,
            editRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Post, $"/route/{qualificationid}/edit/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var redirectResponse = await response.FollowRedirectAsync(HttpClient);
        var redirectDoc = await redirectResponse.GetDocumentAsync();
        AssertEx.HtmlDocumentHasFlashSuccess(redirectDoc, "Route to professional status updated");

        await WithDbContext(async dbContext =>
        {
            var updatedProfessionalStatusRecord = await dbContext.ProfessionalStatuses.FirstOrDefaultAsync(q => q.QualificationId == qualificationid);
            Assert.Equal(journeyInstance.State.InductionExemptionReasonId, updatedProfessionalStatusRecord!.InductionExemptionReasonId);
            Assert.Equal(journeyInstance.State.Status, updatedProfessionalStatusRecord!.Status);
            Assert.Equal(journeyInstance.State.RouteToProfessionalStatusId, updatedProfessionalStatusRecord!.RouteToProfessionalStatusId);
            Assert.Equal(journeyInstance.State.TrainingStartDate, updatedProfessionalStatusRecord!.TrainingStartDate);
            Assert.Equal(journeyInstance.State.TrainingEndDate, updatedProfessionalStatusRecord!.TrainingEndDate);
            Assert.Equal(journeyInstance.State.AwardedDate, updatedProfessionalStatusRecord!.AwardedDate);
            Assert.Equal(journeyInstance.State.TrainingProviderId, updatedProfessionalStatusRecord!.TrainingProviderId);
            Assert.Equal(journeyInstance.State.TrainingCountryId, updatedProfessionalStatusRecord!.TrainingCountryId);
            Assert.Equal(journeyInstance.State.TrainingAgeSpecialismType, updatedProfessionalStatusRecord!.TrainingAgeSpecialismType);
            Assert.Equal(journeyInstance.State.TrainingAgeSpecialismRangeFrom, updatedProfessionalStatusRecord!.TrainingAgeSpecialismRangeFrom);
            Assert.Equal(journeyInstance.State.TrainingAgeSpecialismRangeTo, updatedProfessionalStatusRecord!.TrainingAgeSpecialismRangeTo);
            Assert.Equal(journeyInstance.State.TrainingSubjectIds, updatedProfessionalStatusRecord!.TrainingSubjectIds);
            Assert.Equal(journeyInstance.State.InductionExemptionReasonId, updatedProfessionalStatusRecord!.InductionExemptionReasonId);
        });

        var RaisedBy = GetCurrentUserId();

        EventPublisher.AssertEventsSaved(e =>
        {
            var actualInductionUpdatedEvent = Assert.IsType<ProfessionalStatusUpdatedEvent>(e);

            Assert.Equal(Clock.UtcNow, actualInductionUpdatedEvent.CreatedUtc);
            Assert.Equal(person.PersonId, actualInductionUpdatedEvent.PersonId);
            Assert.Equal(journeyInstance.State.Status, actualInductionUpdatedEvent.ProfessionalStatus.Status);
            Assert.Equal(journeyInstance.State.RouteToProfessionalStatusId, actualInductionUpdatedEvent.ProfessionalStatus.Route.RouteToProfessionalStatusId);
            Assert.Equal(journeyInstance.State.TrainingStartDate, actualInductionUpdatedEvent.ProfessionalStatus.TrainingStartDate);
            Assert.Equal(journeyInstance.State.TrainingEndDate, actualInductionUpdatedEvent.ProfessionalStatus.TrainingEndDate);
            Assert.Equal(journeyInstance.State.AwardedDate, actualInductionUpdatedEvent.ProfessionalStatus.AwardedDate);
            Assert.Equal(journeyInstance.State.TrainingAgeSpecialismType, actualInductionUpdatedEvent.ProfessionalStatus.TrainingAgeSpecialismType);
            Assert.Equal(journeyInstance.State.TrainingAgeSpecialismRangeFrom, actualInductionUpdatedEvent.ProfessionalStatus.TrainingAgeSpecialismRangeFrom);
            Assert.Equal(journeyInstance.State.TrainingAgeSpecialismRangeTo, actualInductionUpdatedEvent.ProfessionalStatus.TrainingAgeSpecialismRangeTo);
            Assert.Equal(journeyInstance.State.TrainingSubjectIds, actualInductionUpdatedEvent.ProfessionalStatus.TrainingSubjectIds);
            Assert.Equal(journeyInstance.State.InductionExemptionReasonId, actualInductionUpdatedEvent.ProfessionalStatus.InductionExemptionReason?.InductionExemptionReasonId);
            Assert.Equal(journeyInstance.State.ChangeReason!.GetDisplayName(), actualInductionUpdatedEvent.ChangeReason);
            Assert.Equal(journeyInstance.State.ChangeReasonDetail.ChangeReasonDetail, actualInductionUpdatedEvent.ChangeReasonDetail);
            Assert.Equal(journeyInstance.State.ChangeReasonDetail.EvidenceFileId, actualInductionUpdatedEvent.EvidenceFile?.FileId);
            Assert.Equal(journeyInstance.State.ChangeReasonDetail.EvidenceFileName, actualInductionUpdatedEvent.EvidenceFile?.Name);
        });

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.True(journeyInstance.Completed);
    }

    private Task<JourneyInstance<EditRouteState>> CreateJourneyInstanceAsync(Guid qualificationId, EditRouteState? state = null) =>
        CreateJourneyInstance(
            JourneyNames.EditRouteToProfessionalStatus,
            state ?? new EditRouteState(),
            new KeyValuePair<string, object>("qualificationId", qualificationId));
}
