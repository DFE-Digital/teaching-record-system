using AngleSharp.Html.Dom;
using TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.RoutesToProfessionalStatus.AddRoute;

public class CheckYourAnswersTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    private static string _countryCode = "AG";

    [Fact]
    public async Task Cancel_DeletesJourneyAndRedirectsToExpectedPage()
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRoutesToProfessionalStatusAsync()).Where(r => r.Name == "NI R").Single();
        var person = await TestData.CreatePersonAsync();
        var addRouteState = new AddRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusId)
            .WithStatus(ProfessionalStatusStatus.Deferred)
            .WithTrainingCountryId(_countryCode)
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            addRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Get, $"/route/add/check-answers?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

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
        var route = (await ReferenceDataCache.GetRoutesToProfessionalStatusAsync()).Where(r => r.Name == "NI R").Single();
        var person = await TestData.CreatePersonAsync();
        var addRouteState = new AddRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusId)
            .WithStatus(ProfessionalStatusStatus.Deferred)
            .WithTrainingCountryId(_countryCode)
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            addRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Post, $"/route/add/check-answers?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

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
        var status = ReferenceDataCache.GetRouteStatusWhereAllFieldsApply();
        var subjects = (await ReferenceDataCache.GetTrainingSubjectsAsync()).Take(1);
        var trainingProvider = (await ReferenceDataCache.GetTrainingProvidersAsync()).RandomOne();
        var degreeType = (await ReferenceDataCache.GetDegreeTypesAsync()).RandomOne();
        var country = (await ReferenceDataCache.GetTrainingCountriesAsync()).RandomOne();
        var person = await TestData.CreatePersonAsync();
        var addRouteState = new AddRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusId)
            .WithStatus(status)
            .WithTrainingStartDate(startDate)
            .WithTrainingEndDate(endDate)
            .WithAwardedDate(awardedDate)
            .WithTrainingProviderId(trainingProvider.TrainingProviderId)
            .WithTrainingCountryId(country.CountryId)
            .WithTrainingSubjectIds(subjects.Select(s => s.TrainingSubjectId).ToArray())
            .WithTrainingAgeSpecialismType(TrainingAgeSpecialismType.FoundationStage)
            .WithDegreeTypeId(degreeType.DegreeTypeId)
            .WithInductionExemption(true)
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            addRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Get, $"/route/add/check-answers?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

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
    public async Task Get_ShowsExemptionAnswer_AsExpected()
    {
        // Arrange
        var startDate = Clock.Today.AddYears(-1);
        var endDate = Clock.Today;
        var country = (await ReferenceDataCache.GetTrainingCountriesAsync())
            .RandomOne();
        var route = (await ReferenceDataCache.GetRoutesToProfessionalStatusAsync())
            .Where(r => r.InductionExemptionRequired == FieldRequirement.Mandatory && r.TrainingProviderRequired == FieldRequirement.NotApplicable)
            .RandomOne();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.Value.GetInductionExemptionRequirement() == FieldRequirement.Mandatory)
            .RandomOne();
        var exemptionReason = (await ReferenceDataCache.GetInductionExemptionReasonsAsync()).RandomOne();

        var person = await TestData.CreatePersonAsync();
        var addRouteState = new AddRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusId)
            .WithStatus(status.Value)
            .WithTrainingStartDate(startDate)
            .WithTrainingEndDate(endDate)
            .WithAwardedDate(endDate)
            .WithTrainingCountryId(country.CountryId)
            .WithInductionExemption(true)
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            addRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Get, $"/route/add/check-answers?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

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
        var route = (await ReferenceDataCache.GetRoutesToProfessionalStatusAsync()).Where(r => r.Name == "Apprenticeship").Single();
        var trainingProvider = (await ReferenceDataCache.GetTrainingProvidersAsync())
            .RandomOne();
        var degreeType = (await ReferenceDataCache.GetDegreeTypesAsync())
            .RandomOne();
        var person = await TestData.CreatePersonAsync();
        var addRouteState = new AddRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusId)
            .WithStatus(ProfessionalStatusStatus.InTraining)
            .WithTrainingStartDate(startDate)
            .WithTrainingEndDate(endDate)
            .WithTrainingCountryId(_countryCode)
            .WithTrainingProviderId(trainingProvider.TrainingProviderId)
            .WithDegreeTypeId(degreeType.DegreeTypeId)
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            addRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Get, $"/route/add/check-answers?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

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
    public async Task Post_Confirm_AddsProfessionalStatusCreatesEventCompletesJourneyAndRedirectsWithFlashMessage()
    {
        var route = await ReferenceDataCache.GetRouteWhereAllFieldsApplyAsync();
        var status = ReferenceDataCache.GetRouteStatusWhereAllFieldsApply();
        var trainingProvider = (await ReferenceDataCache.GetTrainingProvidersAsync()).First();
        var subjects = (await ReferenceDataCache.GetTrainingSubjectsAsync()).Take(1);
        var country = (await ReferenceDataCache.GetTrainingCountriesAsync()).RandomOne();
        var degreeType = (await ReferenceDataCache.GetDegreeTypesAsync()).RandomOne();
        var ageRange = TrainingAgeSpecialismType.KeyStage3;

        var person = await TestData.CreatePersonAsync();
        var addRouteState = new AddRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusId)
            .WithStatus(status)
            .WithAwardedStatusFields(Clock)
            .WithTrainingProviderId(trainingProvider.TrainingProviderId)
            .WithTrainingSubjectIds(subjects.Select(s => s.TrainingSubjectId).ToArray())
            .WithTrainingCountryId(country.CountryId)
            .WithTrainingAgeSpecialismType(ageRange)
            .WithInductionExemption(true)
            .WithDegreeTypeId(degreeType.DegreeTypeId)
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            addRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Post, $"/route/add/check-answers?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var redirectResponse = await response.FollowRedirectAsync(HttpClient);
        var redirectDoc = await redirectResponse.GetDocumentAsync();
        AssertEx.HtmlDocumentHasFlashSuccess(redirectDoc, "Route to professional status added");

        await WithDbContext(async dbContext =>
        {
            var addedProfessionalStatusRecord = await dbContext.ProfessionalStatuses.FirstOrDefaultAsync(p => p.PersonId == person.PersonId);
            Assert.Equal(journeyInstance.State.IsExemptFromInduction, addedProfessionalStatusRecord!.ExemptFromInduction);
            Assert.Equal(journeyInstance.State.Status, addedProfessionalStatusRecord!.Status);
            Assert.Equal(journeyInstance.State.RouteToProfessionalStatusId, addedProfessionalStatusRecord!.RouteToProfessionalStatusId);
            Assert.Equal(journeyInstance.State.TrainingStartDate, addedProfessionalStatusRecord!.TrainingStartDate);
            Assert.Equal(journeyInstance.State.TrainingEndDate, addedProfessionalStatusRecord!.TrainingEndDate);
            Assert.Equal(journeyInstance.State.AwardedDate, addedProfessionalStatusRecord!.AwardedDate);
            Assert.Equal(journeyInstance.State.TrainingProviderId, addedProfessionalStatusRecord!.TrainingProviderId);
            Assert.Equal(journeyInstance.State.TrainingCountryId, addedProfessionalStatusRecord!.TrainingCountryId);
            Assert.Equal(journeyInstance.State.TrainingAgeSpecialismType, addedProfessionalStatusRecord!.TrainingAgeSpecialismType);
            Assert.Equal(journeyInstance.State.TrainingAgeSpecialismRangeFrom, addedProfessionalStatusRecord!.TrainingAgeSpecialismRangeFrom);
            Assert.Equal(journeyInstance.State.TrainingAgeSpecialismRangeTo, addedProfessionalStatusRecord!.TrainingAgeSpecialismRangeTo);
            Assert.Equal(journeyInstance.State.TrainingSubjectIds, addedProfessionalStatusRecord!.TrainingSubjectIds);
            Assert.Equal(journeyInstance.State.DegreeTypeId, addedProfessionalStatusRecord!.DegreeTypeId);
        });

        var RaisedBy = GetCurrentUserId();

        EventPublisher.AssertEventsSaved(e =>
        {
            var actualCreatedEvent = Assert.IsType<ProfessionalStatusCreatedEvent>(e);

            Assert.Equal(Clock.UtcNow, actualCreatedEvent.CreatedUtc);
            Assert.Equal(person.PersonId, actualCreatedEvent.PersonId);
            Assert.Equal(journeyInstance.State.Status, actualCreatedEvent.ProfessionalStatus.Status);
            Assert.Equal(journeyInstance.State.RouteToProfessionalStatusId, actualCreatedEvent.ProfessionalStatus.RouteToProfessionalStatusId);
            Assert.Equal(journeyInstance.State.TrainingStartDate, actualCreatedEvent.ProfessionalStatus.TrainingStartDate);
            Assert.Equal(journeyInstance.State.TrainingEndDate, actualCreatedEvent.ProfessionalStatus.TrainingEndDate);
            Assert.Equal(journeyInstance.State.AwardedDate, actualCreatedEvent.ProfessionalStatus.AwardedDate);
            Assert.Equal(journeyInstance.State.TrainingAgeSpecialismType, actualCreatedEvent.ProfessionalStatus.TrainingAgeSpecialismType);
            Assert.Equal(journeyInstance.State.TrainingAgeSpecialismRangeFrom, actualCreatedEvent.ProfessionalStatus.TrainingAgeSpecialismRangeFrom);
            Assert.Equal(journeyInstance.State.TrainingAgeSpecialismRangeTo, actualCreatedEvent.ProfessionalStatus.TrainingAgeSpecialismRangeTo);
            Assert.Equal(journeyInstance.State.TrainingSubjectIds, actualCreatedEvent.ProfessionalStatus.TrainingSubjectIds);
            Assert.Equal(journeyInstance.State.IsExemptFromInduction, actualCreatedEvent.ProfessionalStatus.ExemptFromInduction);
        });

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.True(journeyInstance.Completed);
    }

    private Task<JourneyInstance<AddRouteState>> CreateJourneyInstanceAsync(Guid personId, AddRouteState? state = null) =>
        CreateJourneyInstance(
            JourneyNames.AddRouteToProfessionalStatus,
            state ?? new AddRouteState(),
            new KeyValuePair<string, object>("personId", personId));
}
