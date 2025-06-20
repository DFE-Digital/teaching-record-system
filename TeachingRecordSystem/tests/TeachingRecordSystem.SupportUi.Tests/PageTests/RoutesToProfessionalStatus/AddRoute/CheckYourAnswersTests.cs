using AngleSharp.Html.Dom;
using TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus;
using TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.RoutesToProfessionalStatus.AddRoute;

public class CheckYourAnswersTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    private const string CountryCode = "AG";

    [Fact]
    public async Task Cancel_DeletesJourneyAndRedirectsToExpectedPage()
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).Where(r => r.Name == "NI R").Single();
        var person = await TestData.CreatePersonAsync();
        var addRouteState = new AddRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(RouteToProfessionalStatusStatus.Deferred)
            .WithTrainingCountryId(CountryCode)
            .WithValidChangeReasonOption()
            .WithDefaultChangeReasonNoUploadFileDetail()
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
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).Where(r => r.Name == "NI R").Single();
        var person = await TestData.CreatePersonAsync();
        var addRouteState = new AddRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(RouteToProfessionalStatusStatus.Deferred)
            .WithTrainingCountryId(CountryCode)
            .WithValidChangeReasonOption()
            .WithDefaultChangeReasonNoUploadFileDetail()
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
        var holdsFrom = endDate.AddDays(1);
        var route = await ReferenceDataCache.GetRouteWhereAllFieldsApplyAsync();
        var status = TestDataHelper.GetRouteStatusWhereAllFieldsApply();
        var subjects = (await ReferenceDataCache.GetTrainingSubjectsAsync()).Take(1);
        var trainingProvider = (await ReferenceDataCache.GetTrainingProvidersAsync()).RandomOne();
        var degreeType = (await ReferenceDataCache.GetDegreeTypesAsync()).RandomOne();
        var country = (await ReferenceDataCache.GetTrainingCountriesAsync()).RandomOne();
        var person = await TestData.CreatePersonAsync();
        var addRouteState = new AddRouteStateBuilder()
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
        doc.AssertRowContentMatches("Professional qualification date", holdsFrom.ToString(UiDefaults.DateOnlyDisplayFormat));
        doc.AssertRowContentMatches("Training provider", trainingProvider.Name);
        doc.AssertRowContentMatches("Degree type", degreeType.Name);
        doc.AssertRowContentMatches("Country of training", country.Name);
        doc.AssertRowContentMatches("Age range", "Foundation stage");
        doc.AssertRowContentMatches("Subjects", subjects.Select(s => $"{s.Reference} - {s.Name}"));
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

        var person = await TestData.CreatePersonAsync();
        var addRouteState = new AddRouteStateBuilder()
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
            person.PersonId,
            addRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Get, $"/route/add/check-answers?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertRowContentMatches("Professional qualification date", endDate.ToString(UiDefaults.DateOnlyDisplayFormat));
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
        var person = await TestData.CreatePersonAsync();
        var addRouteState = new AddRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(RouteToProfessionalStatusStatus.InTraining)
            .WithTrainingStartDate(startDate)
            .WithTrainingEndDate(endDate)
            .WithTrainingCountryId(CountryCode)
            .WithTrainingProviderId(trainingProvider.TrainingProviderId)
            .WithDegreeTypeId(degreeType.DegreeTypeId)
            .WithValidChangeReasonOption()
            .WithDefaultChangeReasonNoUploadFileDetail()
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
        // Arrange
        var route = await ReferenceDataCache.GetRouteWhereAllFieldsApplyAsync();
        var status = TestDataHelper.GetRouteStatusWhereAllFieldsApply();
        var trainingProvider = (await ReferenceDataCache.GetTrainingProvidersAsync()).First();
        var subjects = (await ReferenceDataCache.GetTrainingSubjectsAsync()).Take(1);
        var country = (await ReferenceDataCache.GetTrainingCountriesAsync()).RandomOne();
        var degreeType = (await ReferenceDataCache.GetDegreeTypesAsync()).RandomOne();
        var ageRange = TrainingAgeSpecialismType.KeyStage3;

        var person = await TestData.CreatePersonAsync();
        var addRouteState = new AddRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(status)
            .WithHoldsStatusFields(Clock)
            .WithTrainingProviderId(trainingProvider.TrainingProviderId)
            .WithTrainingSubjectIds(subjects.Select(s => s.TrainingSubjectId).ToArray())
            .WithTrainingCountryId(country.CountryId)
            .WithTrainingAgeSpecialismType(ageRange)
            .WithInductionExemption(true)
            .WithDegreeTypeId(degreeType.DegreeTypeId)
            .WithValidChangeReasonOption()
            .WithDefaultChangeReasonNoUploadFileDetail()
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
            var addedProfessionalStatusRecord = await dbContext.RouteToProfessionalStatuses.FirstOrDefaultAsync(p => p.PersonId == person.PersonId);
            Assert.NotNull(addedProfessionalStatusRecord);
            Assert.Equal(journeyInstance.State.IsExemptFromInduction, addedProfessionalStatusRecord.ExemptFromInduction);
            Assert.Equal(journeyInstance.State.Status, addedProfessionalStatusRecord.Status);
            Assert.Equal(journeyInstance.State.RouteToProfessionalStatusId, addedProfessionalStatusRecord.RouteToProfessionalStatusTypeId);
            Assert.Equal(journeyInstance.State.TrainingStartDate, addedProfessionalStatusRecord.TrainingStartDate);
            Assert.Equal(journeyInstance.State.TrainingEndDate, addedProfessionalStatusRecord.TrainingEndDate);
            Assert.Equal(journeyInstance.State.HoldsFrom, addedProfessionalStatusRecord.HoldsFrom);
            Assert.Equal(journeyInstance.State.TrainingProviderId, addedProfessionalStatusRecord.TrainingProviderId);
            Assert.Equal(journeyInstance.State.TrainingCountryId, addedProfessionalStatusRecord.TrainingCountryId);
            Assert.Equal(journeyInstance.State.TrainingAgeSpecialismType, addedProfessionalStatusRecord.TrainingAgeSpecialismType);
            Assert.Equal(journeyInstance.State.TrainingAgeSpecialismRangeFrom, addedProfessionalStatusRecord.TrainingAgeSpecialismRangeFrom);
            Assert.Equal(journeyInstance.State.TrainingAgeSpecialismRangeTo, addedProfessionalStatusRecord.TrainingAgeSpecialismRangeTo);
            Assert.Equal(journeyInstance.State.TrainingSubjectIds, addedProfessionalStatusRecord.TrainingSubjectIds);
            Assert.Equal(journeyInstance.State.DegreeTypeId, addedProfessionalStatusRecord.DegreeTypeId);
        });

        EventPublisher.AssertEventsSaved(e =>
        {
            var actualCreatedEvent = Assert.IsType<RouteToProfessionalStatusCreatedEvent>(e);

            Assert.Equal(Clock.UtcNow, actualCreatedEvent.CreatedUtc);
            Assert.Equal(person.PersonId, actualCreatedEvent.PersonId);
            Assert.Equal(journeyInstance.State.Status, actualCreatedEvent.RouteToProfessionalStatus.Status);
            Assert.Equal(journeyInstance.State.RouteToProfessionalStatusId, actualCreatedEvent.RouteToProfessionalStatus.RouteToProfessionalStatusTypeId);
            Assert.Equal(journeyInstance.State.TrainingStartDate, actualCreatedEvent.RouteToProfessionalStatus.TrainingStartDate);
            Assert.Equal(journeyInstance.State.TrainingEndDate, actualCreatedEvent.RouteToProfessionalStatus.TrainingEndDate);
            Assert.Equal(journeyInstance.State.HoldsFrom, actualCreatedEvent.RouteToProfessionalStatus.HoldsFrom);
            Assert.Equal(journeyInstance.State.TrainingAgeSpecialismType, actualCreatedEvent.RouteToProfessionalStatus.TrainingAgeSpecialismType);
            Assert.Equal(journeyInstance.State.TrainingAgeSpecialismRangeFrom, actualCreatedEvent.RouteToProfessionalStatus.TrainingAgeSpecialismRangeFrom);
            Assert.Equal(journeyInstance.State.TrainingAgeSpecialismRangeTo, actualCreatedEvent.RouteToProfessionalStatus.TrainingAgeSpecialismRangeTo);
            Assert.Equal(journeyInstance.State.TrainingSubjectIds, actualCreatedEvent.RouteToProfessionalStatus.TrainingSubjectIds);
            Assert.Equal(journeyInstance.State.IsExemptFromInduction, actualCreatedEvent.RouteToProfessionalStatus.ExemptFromInduction);
        });

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.True(journeyInstance.Completed);
    }

    [Fact]
    public async Task Post_Confirm_WithAwardedQtsRouteTypeUpdatesPersonQtsDateAndHasChangesInEvent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            ProfessionalStatusType.QualifiedTeacherStatus);

        var request = new HttpRequestMessage(HttpMethod.Post,
            $"/route/add/check-answers?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var updatedPerson = await WithDbContext(dbContext => dbContext.Persons.SingleAsync(p => p.PersonId == person.PersonId));
        Assert.Equal(journeyInstance.State.HoldsFrom, updatedPerson.QtsDate);

        EventPublisher.AssertEventsSaved(e =>
        {
            var actualCreatedEvent = Assert.IsType<RouteToProfessionalStatusCreatedEvent>(e);

            Assert.Equal(journeyInstance.State.HoldsFrom, actualCreatedEvent.PersonAttributes.QtsDate);
            Assert.Null(actualCreatedEvent.OldPersonAttributes.QtsDate);
        });
    }

    [Fact]
    public async Task Post_Confirm_WithAwardedEytsRouteTypeUpdatesPersonEytsDateAndHasChangesInEvent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            ProfessionalStatusType.EarlyYearsTeacherStatus);

        var request = new HttpRequestMessage(HttpMethod.Post,
            $"/route/add/check-answers?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var updatedPerson = await WithDbContext(dbContext => dbContext.Persons.SingleAsync(p => p.PersonId == person.PersonId));
        Assert.Equal(journeyInstance.State.HoldsFrom, updatedPerson.EytsDate);

        EventPublisher.AssertEventsSaved(e =>
        {
            var actualCreatedEvent = Assert.IsType<RouteToProfessionalStatusCreatedEvent>(e);

            Assert.Equal(journeyInstance.State.HoldsFrom, actualCreatedEvent.PersonAttributes.EytsDate);
            Assert.Null(actualCreatedEvent.OldPersonAttributes.EytsDate);
        });
    }

    [Fact]
    public async Task Post_Confirm_WithAwardedEypsRouteTypeUpdatesPersonHasEypsAndHasChangesInEvent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            ProfessionalStatusType.EarlyYearsProfessionalStatus);

        var request = new HttpRequestMessage(HttpMethod.Post,
            $"/route/add/check-answers?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var updatedPerson = await WithDbContext(dbContext => dbContext.Persons.SingleAsync(p => p.PersonId == person.PersonId));
        Assert.True(updatedPerson.HasEyps);

        EventPublisher.AssertEventsSaved(e =>
        {
            var actualCreatedEvent = Assert.IsType<RouteToProfessionalStatusCreatedEvent>(e);

            Assert.True(actualCreatedEvent.PersonAttributes.HasEyps);
            Assert.False(actualCreatedEvent.OldPersonAttributes.HasEyps);
        });
    }

    [Fact]
    public async Task Post_Confirm_WithAwardedPqtsRouteTypeUpdatesPersonPqtsDateAndHasChangesInEvent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            ProfessionalStatusType.PartialQualifiedTeacherStatus);

        var request = new HttpRequestMessage(HttpMethod.Post,
            $"/route/add/check-answers?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var updatedPerson = await WithDbContext(dbContext => dbContext.Persons.SingleAsync(p => p.PersonId == person.PersonId));
        Assert.Equal(journeyInstance.State.HoldsFrom, updatedPerson.PqtsDate);

        EventPublisher.AssertEventsSaved(e =>
        {
            var actualCreatedEvent = Assert.IsType<RouteToProfessionalStatusCreatedEvent>(e);

            Assert.Equal(journeyInstance.State.HoldsFrom, actualCreatedEvent.PersonAttributes.PqtsDate);
            Assert.Null(actualCreatedEvent.OldPersonAttributes.PqtsDate);
        });
    }

    private Task<JourneyInstance<AddRouteState>> CreateJourneyInstanceAsync(Guid personId, AddRouteState? state = null) =>
        CreateJourneyInstance(
            JourneyNames.AddRouteToProfessionalStatus,
            state ?? new AddRouteState(),
            new KeyValuePair<string, object>("personId", personId));

    private async Task<JourneyInstance<AddRouteState>> CreateJourneyInstanceAsync(Guid personId, ProfessionalStatusType professionalStatusType)
    {
        var provider = (await ReferenceDataCache.GetTrainingProvidersAsync()).RandomOne();

        AddRouteState state = professionalStatusType switch
        {
            ProfessionalStatusType.QualifiedTeacherStatus => new()
            {
                Initialized = true,
                RouteToProfessionalStatusId = new("6F27BDEB-D00A-4EF9-B0EA-26498CE64713"),  // Apply for QTS
                Status = RouteToProfessionalStatusStatus.Holds,
                HoldsFrom = new(2024, 10, 10),
                TrainingStartDate = null,
                TrainingEndDate = null,
                TrainingSubjectIds = [],
                TrainingAgeSpecialismType = null,
                TrainingAgeSpecialismRangeFrom = null,
                TrainingAgeSpecialismRangeTo = null,
                TrainingCountryId = "GB-SCT",  // Scotland
                TrainingProviderId = null,
                IsExemptFromInduction = false,
                DegreeTypeId = null,
                ChangeReason = ChangeReasonOption.AnotherReason,
                ChangeReasonDetail = new ChangeReasonDetailsState()
                {
                    HasAdditionalReasonDetail = false,
                    UploadEvidence = false
                }
            },
            ProfessionalStatusType.EarlyYearsTeacherStatus => new()
            {
                Initialized = true,
                RouteToProfessionalStatusId = new("D9EEF3F8-FDE6-4A3F-A361-F6655A42FA1E"), // Early Years ITT Assessment Only
                Status = RouteToProfessionalStatusStatus.Holds,
                HoldsFrom = new(2024, 10, 10),
                TrainingStartDate = new(2023, 9, 1),
                TrainingEndDate = new(2024, 5, 1),
                TrainingSubjectIds = [],
                TrainingAgeSpecialismType = null,
                TrainingAgeSpecialismRangeFrom = null,
                TrainingAgeSpecialismRangeTo = null,
                TrainingCountryId = null,
                TrainingProviderId = provider.TrainingProviderId,
                IsExemptFromInduction = null,
                DegreeTypeId = null,
                ChangeReason = ChangeReasonOption.AnotherReason,
                ChangeReasonDetail = new ChangeReasonDetailsState()
                {
                    HasAdditionalReasonDetail = false,
                    UploadEvidence = false
                }
            },
            ProfessionalStatusType.EarlyYearsProfessionalStatus => new()
            {
                Initialized = true,
                RouteToProfessionalStatusId = new("8F5C0431-D006-4EDA-9336-16DFC6A26A78"),  // EYPS
                Status = RouteToProfessionalStatusStatus.Holds,
                HoldsFrom = new(2024, 10, 10),
                TrainingStartDate = null,
                TrainingEndDate = null,
                TrainingSubjectIds = [],
                TrainingAgeSpecialismType = null,
                TrainingAgeSpecialismRangeFrom = null,
                TrainingAgeSpecialismRangeTo = null,
                TrainingCountryId = null,
                TrainingProviderId = null,
                IsExemptFromInduction = null,
                DegreeTypeId = null,
                ChangeReason = ChangeReasonOption.AnotherReason,
                ChangeReasonDetail = new ChangeReasonDetailsState()
                {
                    HasAdditionalReasonDetail = false,
                    UploadEvidence = false
                }
            },
            ProfessionalStatusType.PartialQualifiedTeacherStatus => new()
            {
                Initialized = true,
                RouteToProfessionalStatusId = new("EC95C276-25D9-491F-99A2-8D92F10E1E94"),  // European Recognition - PQTS
                Status = RouteToProfessionalStatusStatus.Holds,
                HoldsFrom = new(2024, 10, 10),
                TrainingStartDate = null,
                TrainingEndDate = null,
                TrainingSubjectIds = [],
                TrainingAgeSpecialismType = null,
                TrainingAgeSpecialismRangeFrom = null,
                TrainingAgeSpecialismRangeTo = null,
                TrainingCountryId = null,
                TrainingProviderId = null,
                IsExemptFromInduction = null,
                DegreeTypeId = null,
                ChangeReason = ChangeReasonOption.AnotherReason,
                ChangeReasonDetail = new ChangeReasonDetailsState()
                {
                    HasAdditionalReasonDetail = false,
                    UploadEvidence = false
                }
            },
            _ => throw new NotImplementedException()
        };

        return await CreateJourneyInstanceAsync(personId, state);
    }
}
