using AngleSharp.Html.Dom;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus;
using TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.RoutesToProfessionalStatus.EditRoute;

public class CheckYourAnswersTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    private static string _countryCode = "AG";

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
        var editRouteState = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(RouteToProfessionalStatusStatus.Deferred)
            .WithTrainingCountryId(_countryCode)
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
        var editRouteState = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(RouteToProfessionalStatusStatus.Deferred)
            .WithTrainingCountryId(_countryCode)
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
        var endDate = Clock.Today.AddDays(-1);
        var holdsFrom = endDate.AddDays(1);
        var route = await ReferenceDataCache.GetRouteWhereAllFieldsApplyAsync();
        var status = TestDataHelper.GetRouteStatusWhereAllFieldsApply();
        var subjects = (await ReferenceDataCache.GetTrainingSubjectsAsync()).Take(1);
        var trainingProvider = (await ReferenceDataCache.GetTrainingProvidersAsync()).RandomOne();
        var degreeType = (await ReferenceDataCache.GetDegreeTypesAsync()).RandomOne();
        var country = (await ReferenceDataCache.GetTrainingCountriesAsync()).RandomOne();
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(RouteToProfessionalStatusStatus.InTraining)));

        var qualificationid = person.ProfessionalStatuses.First().QualificationId;
        var editRouteState = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(status)
            .WithTrainingStartDate(startDate)
            .WithTrainingEndDate(endDate)
            .WithHoldsFrom(holdsFrom)
            .WithTrainingProviderId(trainingProvider.TrainingProviderId)
            .WithTrainingCountryId(country.CountryId)
            .WithTrainingSubjectIds(subjects.Select(s => s.TrainingSubjectId).ToArray())
            .WithTrainingAgeSpecialismType(TrainingAgeSpecialismType.FoundationStage)
            .WithDegreeTypeId(degreeType.DegreeTypeId)
            .WithInductionExemption(true)
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
        doc.AssertRowContentMatches("Status", status.GetTitle());
        doc.AssertRowContentMatches("Start date", startDate.ToString(UiDefaults.DateOnlyDisplayFormat));
        doc.AssertRowContentMatches("End date", endDate.ToString(UiDefaults.DateOnlyDisplayFormat));
        doc.AssertRowContentMatches("Award date", holdsFrom.ToString(UiDefaults.DateOnlyDisplayFormat));
        doc.AssertRowContentMatches("Training provider", trainingProvider.Name);
        doc.AssertRowContentMatches("Degree type", degreeType.Name);
        doc.AssertRowContentMatches("Country of training", country.Name);
        doc.AssertRowContentMatches("Age range", "Foundation stage");
        doc.AssertRowContentMatches("Subjects", subjects.Select(s => s.Name));
    }

    [Fact]
    public async Task Get_ShowsExemptionAnswer_AsExpected()
    {
        // Arrange
        var startDate = Clock.Today.AddYears(-1);
        var endDate = Clock.Today;
        var country = (await ReferenceDataCache.GetTrainingCountriesAsync())
            .RandomOne();
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.InductionExemptionRequired == FieldRequirement.Mandatory && r.TrainingProviderRequired == FieldRequirement.NotApplicable)
            .RandomOne();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.Value.GetInductionExemptionRequirement() == FieldRequirement.Mandatory)
            .RandomOne();
        var exemptionReason = (await ReferenceDataCache.GetInductionExemptionReasonsAsync()).RandomOne();

        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(status.Value)
                .WithHoldsFrom(endDate)));

        var qualificationid = person.ProfessionalStatuses.First().QualificationId;
        var editRouteState = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(status.Value)
            .WithTrainingStartDate(startDate)
            .WithTrainingEndDate(endDate)
            .WithHoldsFrom(endDate)
            .WithTrainingCountryId(country.CountryId)
            .WithInductionExemption(true)
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

        doc.AssertRowContentMatches("Award date", endDate.ToString(UiDefaults.DateOnlyDisplayFormat));
        doc.AssertRowContentMatches("Has exemption", "Yes");
    }

    [Fact]
    public async Task Get_ShowsOptionalAnswersNotPopulated_AsExpected()
    {
        // Arrange
        var startDate = Clock.Today.AddYears(-1);
        var endDate = Clock.Today;
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).Where(r => r.Name == "Apprenticeship").Single();
        var trainingProvider = (await ReferenceDataCache.GetTrainingProvidersAsync())
            .RandomOne();
        var degreeType = (await ReferenceDataCache.GetDegreeTypesAsync())
            .RandomOne();
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(RouteToProfessionalStatusStatus.InTraining)));

        var qualificationid = person.ProfessionalStatuses.First().QualificationId;
        var editRouteState = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(RouteToProfessionalStatusStatus.InTraining)
            .WithTrainingStartDate(startDate)
            .WithTrainingEndDate(endDate)
            .WithTrainingCountryId(_countryCode)
            .WithTrainingProviderId(trainingProvider.TrainingProviderId)
            .WithDegreeTypeId(degreeType.DegreeTypeId)
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
        doc.AssertRowContentMatches("Training provider", trainingProvider.Name);
        doc.AssertRowContentMatches("Age range", "Not provided");
        doc.AssertRowContentMatches("Subjects", "Not provided");
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
        var editRouteState = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(RouteToProfessionalStatusStatus.Deferred)
            .WithTrainingCountryId(_countryCode)
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
        // Arrange
        var route = await ReferenceDataCache.GetRouteWhereAllFieldsApplyAsync();
        var status = TestDataHelper.GetRouteStatusWhereAllFieldsApply();
        var trainingProvider = (await ReferenceDataCache.GetTrainingProvidersAsync()).First();
        var subjects = (await ReferenceDataCache.GetTrainingSubjectsAsync()).Take(1);
        var country = (await ReferenceDataCache.GetTrainingCountriesAsync()).RandomOne();
        var degreeType = (await ReferenceDataCache.GetDegreeTypesAsync()).RandomOne();
        var ageRange = TrainingAgeSpecialismType.KeyStage3;

        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(RouteToProfessionalStatusStatus.Deferred)));
        EventPublisher.Clear();

        var qualificationId = person.ProfessionalStatuses.First().QualificationId;
        var editRouteState = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(status)
            .WithHoldsStatusFields(Clock)
            .WithTrainingProviderId(trainingProvider.TrainingProviderId)
            .WithTrainingSubjectIds(subjects.Select(s => s.TrainingSubjectId).ToArray())
            .WithTrainingCountryId(country.CountryId)
            .WithTrainingAgeSpecialismType(ageRange)
            .WithInductionExemption(true)
            .WithValidChangeReasonOption()
            .WithDefaultChangeReasonNoUploadFileDetail()
            .WithDegreeTypeId(degreeType.DegreeTypeId)
            .WithValidChangeReasonOption()
            .WithDefaultChangeReasonNoUploadFileDetail()
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            editRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Post, $"/route/{qualificationId}/edit/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var redirectResponse = await response.FollowRedirectAsync(HttpClient);
        var redirectDoc = await redirectResponse.GetDocumentAsync();
        AssertEx.HtmlDocumentHasFlashSuccess(redirectDoc, "Route to professional status updated");

        await WithDbContext(async dbContext =>
        {
            var updatedProfessionalStatusRecord = await dbContext.RouteToProfessionalStatuses.FirstOrDefaultAsync(q => q.QualificationId == qualificationId);
            Assert.Equal(journeyInstance.State.IsExemptFromInduction, updatedProfessionalStatusRecord!.ExemptFromInduction);
            Assert.Equal(journeyInstance.State.Status, updatedProfessionalStatusRecord!.Status);
            Assert.Equal(journeyInstance.State.RouteToProfessionalStatusId, updatedProfessionalStatusRecord!.RouteToProfessionalStatusTypeId);
            Assert.Equal(journeyInstance.State.TrainingStartDate, updatedProfessionalStatusRecord!.TrainingStartDate);
            Assert.Equal(journeyInstance.State.TrainingEndDate, updatedProfessionalStatusRecord!.TrainingEndDate);
            Assert.Equal(journeyInstance.State.HoldsFrom, updatedProfessionalStatusRecord!.HoldsFrom);
            Assert.Equal(journeyInstance.State.TrainingProviderId, updatedProfessionalStatusRecord!.TrainingProviderId);
            Assert.Equal(journeyInstance.State.TrainingCountryId, updatedProfessionalStatusRecord!.TrainingCountryId);
            Assert.Equal(journeyInstance.State.TrainingAgeSpecialismType, updatedProfessionalStatusRecord!.TrainingAgeSpecialismType);
            Assert.Equal(journeyInstance.State.TrainingAgeSpecialismRangeFrom, updatedProfessionalStatusRecord!.TrainingAgeSpecialismRangeFrom);
            Assert.Equal(journeyInstance.State.TrainingAgeSpecialismRangeTo, updatedProfessionalStatusRecord!.TrainingAgeSpecialismRangeTo);
            Assert.Equal(journeyInstance.State.TrainingSubjectIds, updatedProfessionalStatusRecord!.TrainingSubjectIds);
            Assert.Equal(journeyInstance.State.DegreeTypeId, updatedProfessionalStatusRecord!.DegreeTypeId);
        });

        var raisedBy = GetCurrentUserId();

        EventPublisher.AssertEventsSaved(e =>
        {
            var actualUpdatedEvent = Assert.IsType<RouteToProfessionalStatusUpdatedEvent>(e);

            Assert.Equal(Clock.UtcNow, actualUpdatedEvent.CreatedUtc);
            Assert.Equal(person.PersonId, actualUpdatedEvent.PersonId);
            Assert.Equal(journeyInstance.State.Status, actualUpdatedEvent.RouteToProfessionalStatus.Status);
            Assert.Equal(journeyInstance.State.RouteToProfessionalStatusId, actualUpdatedEvent.RouteToProfessionalStatus.RouteToProfessionalStatusTypeId);
            Assert.Equal(journeyInstance.State.TrainingStartDate, actualUpdatedEvent.RouteToProfessionalStatus.TrainingStartDate);
            Assert.Equal(journeyInstance.State.TrainingEndDate, actualUpdatedEvent.RouteToProfessionalStatus.TrainingEndDate);
            Assert.Equal(journeyInstance.State.HoldsFrom, actualUpdatedEvent.RouteToProfessionalStatus.HoldsFrom);
            Assert.Equal(journeyInstance.State.TrainingAgeSpecialismType, actualUpdatedEvent.RouteToProfessionalStatus.TrainingAgeSpecialismType);
            Assert.Equal(journeyInstance.State.TrainingAgeSpecialismRangeFrom, actualUpdatedEvent.RouteToProfessionalStatus.TrainingAgeSpecialismRangeFrom);
            Assert.Equal(journeyInstance.State.TrainingAgeSpecialismRangeTo, actualUpdatedEvent.RouteToProfessionalStatus.TrainingAgeSpecialismRangeTo);
            Assert.Equal(journeyInstance.State.TrainingSubjectIds, actualUpdatedEvent.RouteToProfessionalStatus.TrainingSubjectIds);
            Assert.Equal(journeyInstance.State.IsExemptFromInduction, actualUpdatedEvent.RouteToProfessionalStatus.ExemptFromInduction);
            Assert.Equal(journeyInstance.State.ChangeReason!.GetDisplayName(), actualUpdatedEvent.ChangeReason);
            Assert.Equal(journeyInstance.State.ChangeReasonDetail.ChangeReasonDetail, actualUpdatedEvent.ChangeReasonDetail);
            Assert.Equal(journeyInstance.State.ChangeReasonDetail.EvidenceFileId, actualUpdatedEvent.EvidenceFile?.FileId);
            Assert.Equal(journeyInstance.State.ChangeReasonDetail.EvidenceFileName, actualUpdatedEvent.EvidenceFile?.Name);
        });

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.True(journeyInstance.Completed);
    }

    [Fact]
    public async Task Post_Confirm_WithAwardedQtsRouteTypeUpdatesPersonQtsDateAndHasChangesInEvent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithHoldsRouteToProfessionalStatus(ProfessionalStatusType.QualifiedTeacherStatus));
        EventPublisher.Clear();

        var qualification = person.ProfessionalStatuses.First();

        var journeyInstance = await CreateJourneyInstanceAsync(qualification, e => e.HoldsFrom = qualification.HoldsFrom!.Value.AddDays(-1));

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/route/{qualification.QualificationId}/edit/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var updatedPerson = await WithDbContext(dbContext => dbContext.Persons.SingleAsync(p => p.PersonId == person.PersonId));
        Assert.Equal(journeyInstance.State.HoldsFrom, updatedPerson.QtsDate);

        EventPublisher.AssertEventsSaved(e =>
        {
            var actualCreatedEvent = Assert.IsType<RouteToProfessionalStatusUpdatedEvent>(e);

            Assert.Equal(journeyInstance.State.HoldsFrom, actualCreatedEvent.PersonAttributes.QtsDate);
            Assert.Equal(qualification.HoldsFrom, actualCreatedEvent.OldPersonAttributes.QtsDate);
        });
    }

    [Fact]
    public async Task Post_Confirm_WithAwardedEytsRouteTypeUpdatesPersonEytsDateAndHasChangesInEvent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithHoldsRouteToProfessionalStatus(ProfessionalStatusType.EarlyYearsTeacherStatus));
        EventPublisher.Clear();

        var qualification = person.ProfessionalStatuses.First();

        var journeyInstance = await CreateJourneyInstanceAsync(qualification, e => e.HoldsFrom = qualification.HoldsFrom!.Value.AddDays(-1));
        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/route/{qualification.QualificationId}/edit/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var updatedPerson = await WithDbContext(dbContext => dbContext.Persons.SingleAsync(p => p.PersonId == person.PersonId));
        Assert.Equal(journeyInstance.State.HoldsFrom, updatedPerson.EytsDate);

        EventPublisher.AssertEventsSaved(e =>
        {
            var actualCreatedEvent = Assert.IsType<RouteToProfessionalStatusUpdatedEvent>(e);

            Assert.Equal(journeyInstance.State.HoldsFrom, actualCreatedEvent.PersonAttributes.EytsDate);
            Assert.Equal(qualification.HoldsFrom, actualCreatedEvent.OldPersonAttributes.EytsDate);
        });
    }

    // N.B. There's no test for EYPS since our one EYPS route has to have an award date
    // (so there's no edit that can be made through this journey that can affect Person.HasEyps)

    [Fact]
    public async Task Post_Confirm_WithAwardedPqtsRouteTypeUpdatesPersonQtsDateAndHasChangesInEvent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithHoldsRouteToProfessionalStatus(ProfessionalStatusType.PartialQualifiedTeacherStatus));
        EventPublisher.Clear();

        var qualification = person.ProfessionalStatuses.First();

        var journeyInstance = await CreateJourneyInstanceAsync(qualification, e => e.HoldsFrom = qualification.HoldsFrom!.Value.AddDays(1));

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/route/{qualification.QualificationId}/edit/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var updatedPerson = await WithDbContext(dbContext => dbContext.Persons.SingleAsync(p => p.PersonId == person.PersonId));
        Assert.Equal(journeyInstance.State.HoldsFrom, updatedPerson.PqtsDate);

        EventPublisher.AssertEventsSaved(e =>
        {
            var actualCreatedEvent = Assert.IsType<RouteToProfessionalStatusUpdatedEvent>(e);

            Assert.Equal(journeyInstance.State.HoldsFrom, actualCreatedEvent.PersonAttributes.PqtsDate);
            Assert.Equal(qualification.HoldsFrom, actualCreatedEvent.OldPersonAttributes.PqtsDate);
        });
    }

    private Task<JourneyInstance<EditRouteState>> CreateJourneyInstanceAsync(Guid qualificationId, EditRouteState? state = null) =>
        CreateJourneyInstance(
            JourneyNames.EditRouteToProfessionalStatus,
            state ?? new EditRouteState(),
            new KeyValuePair<string, object>("qualificationId", qualificationId));

    private Task<JourneyInstance<EditRouteState>> CreateJourneyInstanceAsync(RouteToProfessionalStatus qualification, Action<EditRouteState> action)
    {
        var editRouteState = new EditRouteState();
        editRouteState.EnsureInitialized(qualification);
        action(editRouteState);
        editRouteState.ChangeReason = ChangeReasonOption.AnotherReason;
        editRouteState.ChangeReasonDetail = new ChangeReasonDetailsState()
        {
            HasAdditionalReasonDetail = false,
            ChangeReasonDetail = null,
            UploadEvidence = false,
            EvidenceFileId = null,
            EvidenceFileName = null,
            EvidenceFileSizeDescription = null
        };

        return CreateJourneyInstanceAsync(qualification.QualificationId, editRouteState);
    }
}
