using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail;
using TeachingRecordSystem.SupportUi.Tests.PageTests.RoutesToProfessionalStatus;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail;

public class ChangeLogProfessionalStatusEventsTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task ProfessionalStatusCreatedEvent_RendersExpectedContent()
    {
        // Arrange
        var startDate = Clock.Today.AddYears(-1);
        var endDate = Clock.Today.AddDays(-1);
        var route = await ReferenceDataCache.GetRouteWhereAllFieldsApplyAsync();
        var status = ProfessionalStatusStatus.InTraining;
        var subjects = (await ReferenceDataCache.GetTrainingSubjectsAsync()).Where(s => s.Name.IndexOf('\'') == -1).Take(1);
        var trainingProvider = (await ReferenceDataCache.GetTrainingProvidersAsync()).RandomOne();
        var degreeType = (await ReferenceDataCache.GetDegreeTypesAsync()).RandomOne();
        var country = (await ReferenceDataCache.GetTrainingCountriesAsync()).RandomOne();
        var ageRange = TrainingAgeSpecialismType.FoundationStage;
        var createdByUser = await TestData.CreateUserAsync();
        var person = await TestData.CreatePersonAsync(b => b
            .WithProfessionalStatus(q => q
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(status)
                .WithTrainingStartDate(startDate)
                .WithTrainingEndDate(endDate)
                .WithTrainingProviderId(trainingProvider.TrainingProviderId)
                .WithTrainingCountryId(country.CountryId)
                .WithTrainingSubjectIds(subjects.Select(s => s.TrainingSubjectId).ToArray())
                .WithTrainingAgeSpecialismType(ageRange)
                .WithDegreeTypeId(degreeType.DegreeTypeId)
                .WithCreatedByUser(EventModels.RaisedByUserInfo.FromUserId(createdByUser.UserId))
                    ));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var timelineItem = doc.GetElementByTestId("timeline-item-route-created-event");
        Assert.NotNull(timelineItem);
        Assert.Equal($"By {createdByUser.Name} on", timelineItem.GetElementByTestId("raised-by")?.TrimmedText());
        Assert.Null(timelineItem.GetElementByTestId("eyts-date"));
        Assert.Null(timelineItem.GetElementByTestId("pqts-date"));
        Assert.Null(timelineItem.GetElementByTestId("qts-date"));
        Assert.Null(timelineItem.GetElementByTestId("has-eyps"));
        Assert.Equal("Not provided", timelineItem.GetElementByTestId("award-date")?.TrimmedText());
        Assert.Equal(status.GetDisplayName(), timelineItem.GetElementByTestId("status")?.TrimmedText());
        Assert.Equal(route.Name, timelineItem.GetElementByTestId("route")?.TrimmedText());
        Assert.Equal(startDate.ToString(UiDefaults.DateOnlyDisplayFormat), timelineItem.GetElementByTestId("start-date")!.TrimmedText());
        Assert.Equal(endDate.ToString(UiDefaults.DateOnlyDisplayFormat), timelineItem.GetElementByTestId("end-date")!.TrimmedText());
        Assert.Equal("Not provided", timelineItem.GetElementByTestId("exemption")?.TrimmedText());
        Assert.Equal(trainingProvider.Name, timelineItem.GetElementByTestId("training-provider")?.TrimmedText());
        Assert.Equal(degreeType.Name, timelineItem.GetElementByTestId("degree-type")?.TrimmedText());
        Assert.Equal(country.Name, timelineItem.GetElementByTestId("country")?.TrimmedText());
        Assert.Equal(ageRange.GetDisplayName(), timelineItem.GetElementByTestId("age-range-type")?.TrimmedText());
        Assert.Equal(subjects.Single().Name, timelineItem.GetElementByTestId("subjects")?.TrimmedText());
    }

    [Fact]
    public async Task ProfessionalStatusCreatedEvent_AffectsPersonProfessionalStatus_RendersExpectedContent()
    {
        // Arrange
        var awardDate = Clock.Today;
        var oldAwardDate = awardDate.AddDays(1);
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.ProfessionalStatusType == ProfessionalStatusType.EarlyYearsTeacherStatus)
            .RandomOne();
        var person = await TestData.CreatePersonAsync(b => b
            .WithProfessionalStatus(q =>
            {
                q.WithRouteType(route.RouteToProfessionalStatusTypeId);
                q.WithStatus(ProfessionalStatusStatus.Awarded);
                q.WithAwardedDate(Clock.Today);
                q.WithInductionExemption(true);
            }));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var timelineItem = doc.GetElementByTestId("timeline-item-route-created-event");
        Assert.NotNull(timelineItem);
        Assert.Equal(awardDate.ToString(UiDefaults.DateOnlyDisplayFormat), timelineItem.GetElementByTestId("eyts-date")?.TrimmedText());
        Assert.Null(timelineItem.GetElementByTestId("pqts-date"));
        Assert.Null(timelineItem.GetElementByTestId("qts-date"));
        Assert.Null(timelineItem.GetElementByTestId("has-eyps"));
        Assert.Equal("Not provided", timelineItem.GetElementByTestId("old-eyts-date")?.TrimmedText());
    }

    [Fact]
    public async Task ProfessionalStatusUpdatedEvent_RendersExpectedContent()
    {
        // Arrange
        var oldStartDate = Clock.Today.AddYears(-2);
        var oldEndDate = oldStartDate.AddYears(1);
        var oldAwardDate = oldEndDate.AddDays(-1);
        var oldRoute = await ReferenceDataCache.GetRouteWhereAllFieldsApplyAsync();
        var oldStatus = TestDataHelper.GetRouteStatusWhereAllFieldsApply();
        var oldSubject = (await ReferenceDataCache.GetTrainingSubjectsAsync()).RandomOne();
        var oldTrainingProvider = (await ReferenceDataCache.GetTrainingProvidersAsync()).RandomOne();
        var oldDegreeType = (await ReferenceDataCache.GetDegreeTypesAsync()).RandomOne();
        var oldCountry = (await ReferenceDataCache.GetTrainingCountriesAsync()).RandomOne();
        var oldAgeRange = TrainingAgeSpecialismType.FoundationStage;
        var awardDate = oldAwardDate.AddDays(1);
        var startDate = oldStartDate.AddDays(1);
        var endDate = oldEndDate.AddDays(1);
        var route = oldRoute;
        var subject = (await ReferenceDataCache.GetTrainingSubjectsAsync()).Where(x => x.TrainingSubjectId != oldSubject.TrainingSubjectId).Where(s => s.Name.IndexOf('\'') == -1).RandomOne();
        var trainingProvider = (await ReferenceDataCache.GetTrainingProvidersAsync()).Where(x => x.TrainingProviderId != oldTrainingProvider.TrainingProviderId).RandomOne();
        var degreeType = (await ReferenceDataCache.GetDegreeTypesAsync()).Where(x => x.DegreeTypeId != oldDegreeType.DegreeTypeId).RandomOne();
        var country = (await ReferenceDataCache.GetTrainingCountriesAsync()).Where(x => x.CountryId != oldCountry.CountryId).RandomOne();
        var ageRange = TrainingAgeSpecialismType.KeyStage1;
        var person = await TestData.CreatePersonAsync(p => p
            .WithProfessionalStatus(q =>
            {
                q.WithRouteType(route.RouteToProfessionalStatusTypeId);
                q.WithStatus(oldStatus);
                q.WithInductionExemption(true);
                q.WithTrainingStartDate(startDate);
                q.WithTrainingEndDate(endDate);
                q.WithAwardedDate(awardDate);
                q.WithTrainingProviderId(trainingProvider.TrainingProviderId);
                q.WithTrainingCountryId(country.CountryId);
                q.WithTrainingSubjectIds(new List<Guid>() { subject.TrainingSubjectId }.ToArray());
                q.WithTrainingAgeSpecialismType(ageRange);
                q.WithDegreeTypeId(degreeType.DegreeTypeId);
                q.WithInductionExemption(true);
            }));

        var professionalStatus = person.Person.Qualifications!.OfType<RouteToProfessionalStatus>().Single();

        var oldProfessionalStatus = TeachingRecordSystem.Core.Events.Models.RouteToProfessionalStatus.FromModel(professionalStatus) with
        {
            AwardedDate = oldAwardDate,
            DegreeTypeId = oldDegreeType.DegreeTypeId,
            ExemptFromInduction = false,
            TrainingCountryId = oldCountry.CountryId,
            TrainingEndDate = oldEndDate,
            TrainingStartDate = oldStartDate,
            TrainingProviderId = oldTrainingProvider.TrainingProviderId,
            TrainingSubjectIds = new List<Guid>() { oldSubject.TrainingSubjectId }.ToArray(),
            TrainingAgeSpecialismType = oldAgeRange
        };
        var createdByUser = await TestData.CreateUserAsync();
        var raisedByUser = TeachingRecordSystem.Core.Events.Models.RaisedByUserInfo.FromUserId(createdByUser.UserId);
        var professionalStatusUpdatedEvent = await TestData.CreateProfessionalStatusUpdatedEventAsync(e => e
            .ForPerson(person.Person)
            .WithProfessionalStatus(professionalStatus)
            .WithOldProfessionalStatus(oldProfessionalStatus)
            .WithCreatedByUser(raisedByUser)
            .WithCreatedUtc(Clock.UtcNow)
            .WithChanges(
                ProfessionalStatusUpdatedEventChanges.AwardedDate
                | ProfessionalStatusUpdatedEventChanges.StartDate
                | ProfessionalStatusUpdatedEventChanges.EndDate
                | ProfessionalStatusUpdatedEventChanges.DegreeType
                | ProfessionalStatusUpdatedEventChanges.TrainingSubjectIds
                | ProfessionalStatusUpdatedEventChanges.TrainingAgeSpecialismType
                | ProfessionalStatusUpdatedEventChanges.TrainingCountry
                | ProfessionalStatusUpdatedEventChanges.TrainingProvider
                | ProfessionalStatusUpdatedEventChanges.InductionExemptionReasons)
            ) as ProfessionalStatusUpdatedEvent;

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var timelineItem = doc.GetElementByTestId("timeline-item-route-updated-event");
        Assert.NotNull(timelineItem);
        Assert.Equal($"By {createdByUser.Name} on", timelineItem.GetElementByTestId("raised-by")?.TrimmedText());
        Assert.Equal(Clock.NowGmt.ToString(TimelineItem.TimestampFormat), timelineItem.GetElementByTestId("timeline-item-time")?.TrimmedText());
        Assert.Null(timelineItem.GetElementByTestId("eyts-date"));
        Assert.Null(timelineItem.GetElementByTestId("pqts-date"));
        Assert.Null(timelineItem.GetElementByTestId("qts-date"));
        Assert.Null(timelineItem.GetElementByTestId("has-eyps"));
        Assert.Null(timelineItem.GetElementByTestId("status"));
        Assert.Equal(awardDate.ToString(UiDefaults.DateOnlyDisplayFormat), timelineItem.GetElementByTestId("award-date")?.TrimmedText());
        Assert.Equal(startDate.ToString(UiDefaults.DateOnlyDisplayFormat), timelineItem.GetElementByTestId("start-date")!.TrimmedText());
        Assert.Equal(endDate.ToString(UiDefaults.DateOnlyDisplayFormat), timelineItem.GetElementByTestId("end-date")!.TrimmedText());
        Assert.Equal("Yes", timelineItem.GetElementByTestId("exemption")?.TrimmedText());
        Assert.Equal(trainingProvider.Name, timelineItem.GetElementByTestId("training-provider")?.TrimmedText());
        Assert.Equal(degreeType.Name, timelineItem.GetElementByTestId("degree-type")?.TrimmedText());
        Assert.Equal(country.Name, timelineItem.GetElementByTestId("country")?.TrimmedText());
        Assert.Equal(ageRange.GetDisplayName(), timelineItem.GetElementByTestId("age-range-type")?.TrimmedText());
        Assert.Equal(subject.Name, timelineItem.GetElementByTestId("subjects")?.TrimmedText());

        Assert.Equal(oldAwardDate.ToString(UiDefaults.DateOnlyDisplayFormat), timelineItem.GetElementByTestId("old-award-date")?.TrimmedText());
        Assert.Null(timelineItem.GetElementByTestId("status"));
        Assert.Equal(oldStartDate.ToString(UiDefaults.DateOnlyDisplayFormat), timelineItem.GetElementByTestId("old-start-date")!.TrimmedText());
        Assert.Equal(oldEndDate.ToString(UiDefaults.DateOnlyDisplayFormat), timelineItem.GetElementByTestId("old-end-date")!.TrimmedText());
        Assert.Equal("No", timelineItem.GetElementByTestId("old-exemption")?.TrimmedText());
        Assert.Equal(oldTrainingProvider.Name, timelineItem.GetElementByTestId("old-training-provider")?.TrimmedText());
        Assert.Equal(oldDegreeType.Name, timelineItem.GetElementByTestId("old-degree-type")?.TrimmedText());
        Assert.Equal(oldCountry.Name, timelineItem.GetElementByTestId("old-country")?.TrimmedText());
        Assert.Equal(oldAgeRange.GetDisplayName(), timelineItem.GetElementByTestId("old-age-range-type")?.TrimmedText());
        Assert.Equal(oldSubject.Name, timelineItem.GetElementByTestId("old-subjects")?.TrimmedText());
    }

    [Fact]
    public async Task ProfessionalStatusUpdatedEvent_StatusChanged_PersonQtsChanged_RendersExpectedContent()
    {
        // Arrange
        var oldStatus = ProfessionalStatusStatus.InTraining;
        var startDate = Clock.Today.AddYears(-2);
        var endDate = startDate.AddYears(1);
        var awardDate = endDate.AddDays(1);
        var route = await ReferenceDataCache.GetRouteWhereAllFieldsApplyAsync();
        var status = ProfessionalStatusStatus.Awarded;
        var subject = (await ReferenceDataCache.GetTrainingSubjectsAsync()).Where(s => s.Name.IndexOf('\'') == -1).RandomOne();
        var trainingProvider = (await ReferenceDataCache.GetTrainingProvidersAsync()).RandomOne();
        var degreeType = (await ReferenceDataCache.GetDegreeTypesAsync()).RandomOne();
        var country = (await ReferenceDataCache.GetTrainingCountriesAsync()).RandomOne();
        var ageRange = TrainingAgeSpecialismType.KeyStage1;
        var person = await TestData.CreatePersonAsync(p => p
            .WithProfessionalStatus(q =>
            {
                q.WithRouteType(route.RouteToProfessionalStatusTypeId);
                q.WithStatus(status);
                q.WithInductionExemption(true);
                q.WithTrainingStartDate(startDate);
                q.WithTrainingEndDate(endDate);
                q.WithAwardedDate(awardDate);
                q.WithTrainingProviderId(trainingProvider.TrainingProviderId);
                q.WithTrainingCountryId(country.CountryId);
                q.WithTrainingSubjectIds(new List<Guid>() { subject.TrainingSubjectId }.ToArray());
                q.WithTrainingAgeSpecialismType(ageRange);
                q.WithDegreeTypeId(degreeType.DegreeTypeId);
                q.WithInductionExemption(true);
            }));

        var professionalStatus = person.Person.Qualifications!.OfType<RouteToProfessionalStatus>().Single();
        var oldProfessionalStatus = TeachingRecordSystem.Core.Events.Models.RouteToProfessionalStatus.FromModel(professionalStatus) with
        {
            Status = oldStatus
        };
        var createdByUser = await TestData.CreateUserAsync();
        var raisedByUser = TeachingRecordSystem.Core.Events.Models.RaisedByUserInfo.FromUserId(createdByUser.UserId);
        var professionalStatusUpdatedEvent = await TestData.CreateProfessionalStatusUpdatedEventAsync(e => e
            .ForPerson(person.Person)
            .WithProfessionalStatus(professionalStatus)
            .WithOldProfessionalStatus(oldProfessionalStatus)
            .WithCreatedByUser(raisedByUser)
            .WithCreatedUtc(Clock.UtcNow)
            .WithPersonAttributes(new Core.Events.Models.ProfessionalStatusPersonAttributes() { EytsDate = awardDate, HasEyps = false, PqtsDate = null, QtsDate = null })
            .WithChanges(ProfessionalStatusUpdatedEventChanges.Status | ProfessionalStatusUpdatedEventChanges.PersonEytsDate)
            ) as ProfessionalStatusUpdatedEvent;

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var timelineItem = doc.GetElementByTestId("timeline-item-route-updated-event");
        Assert.NotNull(timelineItem);
        Assert.Equal($"By {createdByUser.Name} on", timelineItem.GetElementByTestId("raised-by")?.TrimmedText());
        Assert.Equal(Clock.NowGmt.ToString(TimelineItem.TimestampFormat), timelineItem.GetElementByTestId("timeline-item-time")?.TrimmedText());
        Assert.Equal(status.GetDisplayName(), timelineItem.GetElementByTestId("status")?.TrimmedText());
        Assert.Equal(awardDate.ToString(UiDefaults.DateOnlyDisplayFormat), timelineItem.GetElementByTestId("eyts-date")?.TrimmedText());
        Assert.Null(timelineItem.GetElementByTestId("pqts-date"));
        Assert.Null(timelineItem.GetElementByTestId("qts-date"));
        Assert.Null(timelineItem.GetElementByTestId("has-eyps"));
        Assert.Null(timelineItem.GetElementByTestId("award-date"));
        Assert.Null(timelineItem.GetElementByTestId("start-date"));
        Assert.Null(timelineItem.GetElementByTestId("end-date"));
        Assert.Null(timelineItem.GetElementByTestId("exemption"));
        Assert.Null(timelineItem.GetElementByTestId("training-provider"));
        Assert.Null(timelineItem.GetElementByTestId("degree-type"));
        Assert.Null(timelineItem.GetElementByTestId("country"));
        Assert.Null(timelineItem.GetElementByTestId("age-range-type"));
        Assert.Null(timelineItem.GetElementByTestId("subjects"));

        Assert.Equal(oldStatus.GetDisplayName(), timelineItem.GetElementByTestId("old-status")?.TrimmedText());
        Assert.Equal("Not provided", timelineItem.GetElementByTestId("old-eyts-date")?.TrimmedText());
        Assert.Null(timelineItem.GetElementByTestId("old-start-date"));
        Assert.Null(timelineItem.GetElementByTestId("old-end-date"));
        Assert.Null(timelineItem.GetElementByTestId("old-exemption"));
        Assert.Null(timelineItem.GetElementByTestId("old-training-provider"));
        Assert.Null(timelineItem.GetElementByTestId("old-degree-type"));
        Assert.Null(timelineItem.GetElementByTestId("old-country"));
        Assert.Null(timelineItem.GetElementByTestId("old-age-range-type"));
        Assert.Null(timelineItem.GetElementByTestId("old-subjects"));
    }

    [Fact]
    public async Task ProfessionalStatusUpdatedEvent_RendersExpectedChangeReasonContent()
    {
        // Arrange
        var oldStatus = ProfessionalStatusStatus.InTraining;
        var startDate = Clock.Today.AddYears(-2);
        var endDate = startDate.AddYears(1);
        var awardDate = endDate.AddDays(1);
        var route = await ReferenceDataCache.GetRouteWhereAllFieldsApplyAsync();
        var status = ProfessionalStatusStatus.Awarded;
        var changeReason = "Text from change reason selection";
        var changeReasonDetails = TestData.GenerateLoremIpsum();
        var person = await TestData.CreatePersonAsync(p => p
            .WithProfessionalStatus(q =>
            {
                q.WithRouteType(route.RouteToProfessionalStatusTypeId);
                q.WithStatus(status);
                q.WithInductionExemption(true);
                q.WithAwardedDate(awardDate);
            }));

        var professionalStatus = person.Person.Qualifications!.OfType<RouteToProfessionalStatus>().Single();
        var oldProfessionalStatus = TeachingRecordSystem.Core.Events.Models.RouteToProfessionalStatus.FromModel(professionalStatus) with
        {
            Status = oldStatus
        };

        var createdByUser = await TestData.CreateUserAsync();
        var raisedByUser = TeachingRecordSystem.Core.Events.Models.RaisedByUserInfo.FromUserId(createdByUser.UserId);
        var professionalStatusUpdatedEvent = await TestData.CreateProfessionalStatusUpdatedEventAsync(e => e
            .ForPerson(person.Person)
            .WithProfessionalStatus(professionalStatus)
            .WithOldProfessionalStatus(oldProfessionalStatus)
            .WithCreatedByUser(raisedByUser)
            .WithCreatedUtc(Clock.UtcNow)
            .WithChanges(ProfessionalStatusUpdatedEventChanges.Status)
            .WithChangeReason(changeReason)
            .WithChangeReasonDetails(changeReasonDetails)
            ) as ProfessionalStatusUpdatedEvent;

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var timelineItem = doc.GetElementByTestId("timeline-item-route-updated-event");
        Assert.NotNull(timelineItem);
        Assert.Equal(changeReason, timelineItem.GetElementByTestId("reason")?.TrimmedText());
        Assert.Equal(changeReasonDetails, timelineItem.GetElementByTestId("reason-detail")?.TrimmedText());
    }

    [Fact]
    public async Task ProfessionalStatusDeletedEvent_RendersExpectedContent()
    {
        // Arrange
        var startDate = Clock.Today.AddYears(-1);
        var endDate = Clock.Today.AddDays(-1);
        var awardedDate = endDate.AddDays(1);
        var route = await ReferenceDataCache.GetRouteWhereAllFieldsApplyAsync();
        var status = TestDataHelper.GetRouteStatusWhereAllFieldsApply();
        var subjects = (await ReferenceDataCache.GetTrainingSubjectsAsync()).Where(s => s.Name.IndexOf('\'') == -1).Take(1);
        var trainingProvider = (await ReferenceDataCache.GetTrainingProvidersAsync()).RandomOne();
        var degreeType = (await ReferenceDataCache.GetDegreeTypesAsync()).RandomOne();
        var country = (await ReferenceDataCache.GetTrainingCountriesAsync()).RandomOne();
        var ageRange = TrainingAgeSpecialismType.FoundationStage;
        var person = await TestData.CreatePersonAsync(b => b
            .WithProfessionalStatus(q =>
            {
                q.WithRouteType(route.RouteToProfessionalStatusTypeId);
                q.WithStatus(status);
                q.WithAwardedDate(Clock.Today);
                q.WithInductionExemption(true);
                q.WithTrainingStartDate(startDate);
                q.WithTrainingEndDate(endDate);
                q.WithAwardedDate(awardedDate);
                q.WithTrainingProviderId(trainingProvider.TrainingProviderId);
                q.WithTrainingCountryId(country.CountryId);
                q.WithTrainingSubjectIds(subjects.Select(s => s.TrainingSubjectId).ToArray());
                q.WithTrainingAgeSpecialismType(ageRange);
                q.WithDegreeTypeId(degreeType.DegreeTypeId);
                q.WithInductionExemption(true);
            }));

        var professionalStatus = person.Person.Qualifications!.OfType<RouteToProfessionalStatus>().Single();
        var createdByUser = await TestData.CreateUserAsync();
        var raisedByUser = TeachingRecordSystem.Core.Events.Models.RaisedByUserInfo.FromUserId(createdByUser.UserId);
        var professionalStatusDeletedEvent = await TestData.CreateProfessionalStatusDeletedEventAsync(e => e
            .ForPerson(person.Person)
            .WithProfessionalStatus(professionalStatus)
            .WithCreatedByUser(raisedByUser)
            .WithCreatedUtc(Clock.UtcNow)
            ) as ProfessionalStatusDeletedEvent;

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var timelineItem = doc.GetElementByTestId("timeline-item-route-deleted-event");
        Assert.NotNull(timelineItem);
        Assert.Equal($"By {createdByUser.Name} on", timelineItem.GetElementByTestId("raised-by")?.TrimmedText());
        Assert.Equal(Clock.NowGmt.ToString(TimelineItem.TimestampFormat), timelineItem.GetElementByTestId("timeline-item-time")?.TrimmedText());
        Assert.Null(timelineItem.GetElementByTestId("eyts-date"));
        Assert.Null(timelineItem.GetElementByTestId("pqts-date"));
        Assert.Null(timelineItem.GetElementByTestId("qts-date"));
        Assert.Null(timelineItem.GetElementByTestId("has-eyps"));
        Assert.Equal(awardedDate.ToString(UiDefaults.DateOnlyDisplayFormat), timelineItem.GetElementByTestId("award-date")?.TrimmedText());
        Assert.Equal(status.GetDisplayName(), timelineItem.GetElementByTestId("status")?.TrimmedText());
        Assert.Equal(route.Name, timelineItem.GetElementByTestId("route")?.TrimmedText());
        Assert.Equal(startDate.ToString(UiDefaults.DateOnlyDisplayFormat), timelineItem.GetElementByTestId("start-date")!.TrimmedText());
        Assert.Equal(endDate.ToString(UiDefaults.DateOnlyDisplayFormat), timelineItem.GetElementByTestId("end-date")!.TrimmedText());
        Assert.Equal("Yes", timelineItem.GetElementByTestId("exemption")?.TrimmedText());
        Assert.Equal(trainingProvider.Name, timelineItem.GetElementByTestId("training-provider")?.TrimmedText());
        Assert.Equal(degreeType.Name, timelineItem.GetElementByTestId("degree-type")?.TrimmedText());
        Assert.Equal(country.Name, timelineItem.GetElementByTestId("country")?.TrimmedText());
        Assert.Equal(ageRange.GetDisplayName(), timelineItem.GetElementByTestId("age-range-type")?.TrimmedText());
        Assert.Equal(subjects.Single().Name, timelineItem.GetElementByTestId("subjects")?.TrimmedText());
    }

    [Fact]
    public async Task ProfessionalStatusDeletedEvent_AffectsPersonProfessionalStatus_RendersExpectedContent()
    {
        // Arrange
        var startDate = Clock.Today.AddYears(-1);
        var endDate = Clock.Today.AddDays(-1);
        var awardedDate = endDate.AddDays(1);
        var route = await ReferenceDataCache.GetRouteWhereAllFieldsApplyAsync();
        var status = TestDataHelper.GetRouteStatusWhereAllFieldsApply();
        var subjects = (await ReferenceDataCache.GetTrainingSubjectsAsync()).Where(s => s.Name.IndexOf('\'') == -1).Take(1);
        var trainingProvider = (await ReferenceDataCache.GetTrainingProvidersAsync()).RandomOne();
        var degreeType = (await ReferenceDataCache.GetDegreeTypesAsync()).RandomOne();
        var country = (await ReferenceDataCache.GetTrainingCountriesAsync()).RandomOne();
        var ageRange = TrainingAgeSpecialismType.FoundationStage;
        var person = await TestData.CreatePersonAsync(b => b
            .WithProfessionalStatus(q =>
            {
                q.WithRouteType(route.RouteToProfessionalStatusTypeId);
                q.WithStatus(status);
                q.WithAwardedDate(Clock.Today);
                q.WithInductionExemption(true);
                q.WithTrainingStartDate(startDate);
                q.WithTrainingEndDate(endDate);
                q.WithAwardedDate(awardedDate);
                q.WithTrainingProviderId(trainingProvider.TrainingProviderId);
                q.WithTrainingCountryId(country.CountryId);
                q.WithTrainingSubjectIds(subjects.Select(s => s.TrainingSubjectId).ToArray());
                q.WithTrainingAgeSpecialismType(ageRange);
                q.WithDegreeTypeId(degreeType.DegreeTypeId);
                q.WithInductionExemption(true);
            }));

        var professionalStatus = person.Person.Qualifications!.OfType<RouteToProfessionalStatus>().Single();
        var createdByUser = await TestData.CreateUserAsync();
        var raisedByUser = TeachingRecordSystem.Core.Events.Models.RaisedByUserInfo.FromUserId(createdByUser.UserId);
        var professionalStatusDeletedEvent = await TestData.CreateProfessionalStatusDeletedEventAsync(e => e
            .ForPerson(person.Person)
            .WithProfessionalStatus(professionalStatus)
            .WithPersonAttributes(new Core.Events.Models.ProfessionalStatusPersonAttributes() { EytsDate = null, HasEyps = false, PqtsDate = null, QtsDate = null })
            .WithOldPersonAttributes(new Core.Events.Models.ProfessionalStatusPersonAttributes() { EytsDate = null, HasEyps = true, PqtsDate = null, QtsDate = null })
            .WithCreatedByUser(raisedByUser)
            .WithCreatedUtc(Clock.UtcNow)
            .WithChanges(ProfessionalStatusDeletedEventChanges.PersonHasEyps)
            ) as ProfessionalStatusDeletedEvent;

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var timelineItem = doc.GetElementByTestId("timeline-item-route-deleted-event");
        Assert.NotNull(timelineItem);
        Assert.Null(timelineItem.GetElementByTestId("eyts-date"));
        Assert.Null(timelineItem.GetElementByTestId("pqts-date"));
        Assert.Null(timelineItem.GetElementByTestId("qts-date"));
        Assert.Equal("No", timelineItem.GetElementByTestId("has-eyps")?.TrimmedText());
        Assert.Equal("Yes", timelineItem.GetElementByTestId("old-has-eyps")?.TrimmedText());
    }
}
