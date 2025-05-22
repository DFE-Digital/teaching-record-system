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
        var awardedDate = endDate.AddDays(1);
        var route = await ReferenceDataCache.GetRouteWhereAllFieldsApplyAsync();
        var status = TestDataHelper.GetRouteStatusWhereAllFieldsApply();
        var subjects = (await ReferenceDataCache.GetTrainingSubjectsAsync()).Take(1);
        var trainingProvider = (await ReferenceDataCache.GetTrainingProvidersAsync()).RandomOne();
        var degreeType = (await ReferenceDataCache.GetDegreeTypesAsync()).RandomOne();
        var country = (await ReferenceDataCache.GetTrainingCountriesAsync()).RandomOne();
        var ageRange = TrainingAgeSpecialismType.FoundationStage;
        var person = await TestData.CreatePersonAsync(b => b
            .WithProfessionalStatus(q =>
            {
                q.WithRoute(route);
                q.WithStatus(status);
                q.WithAwardedDate(Clock.Today);
                q.WithInductionExemption(true);
                q.WithTrainingStartDate(startDate);
                q.WithTrainingEndDate(endDate);
                q.WithAwardedDate(awardedDate);
                q.WithTrainingProvider(trainingProvider);
                q.WithTrainingCountryId(country.CountryId);
                q.WithTrainingSubjectIds(subjects.Select(s => s.TrainingSubjectId).ToArray());
                q.WithTrainingAgeSpecialismType(ageRange);
                q.WithDegreeTypeId(degreeType.DegreeTypeId);
                q.WithInductionExemption(true);
            }));

        var professionalStatus = person.Person.Qualifications!.OfType<ProfessionalStatus>().Single();
        var createdByUser = await TestData.CreateUserAsync();
        var raisedByUser = TeachingRecordSystem.Core.Events.Models.RaisedByUserInfo.FromUserId(createdByUser.UserId);
        var professionalStatusCreatedEvent = await TestData.CreateProfessionalStatusCreatedEventAsync(e => e
            .ForPerson(person.Person)
            .WithProfessionalStatus(professionalStatus)
            .WithCreatedByUser(raisedByUser)
            .WithCreatedUtc(Clock.UtcNow)
            ) as ProfessionalStatusCreatedEvent;

        var qualification = person.ProfessionalStatuses.First();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var timelineItem = doc.GetElementByTestId("timeline-item-route-created-event");
        Assert.NotNull(timelineItem);
        Assert.Equal($"By {createdByUser.Name} on", timelineItem.GetElementByTestId("raised-by")?.TextContent.Trim());
        Assert.Equal(Clock.NowGmt.ToString(TimelineItem.TimestampFormat), timelineItem.GetElementByTestId("timeline-item-time")?.TextContent.Trim());
        Assert.Null(timelineItem.GetElementByTestId("eyts-date"));
        Assert.Null(timelineItem.GetElementByTestId("pqts-date"));
        Assert.Null(timelineItem.GetElementByTestId("qts-date"));
        Assert.Null(timelineItem.GetElementByTestId("has-eyps"));
        Assert.Equal(awardedDate.ToString(UiDefaults.DateOnlyDisplayFormat), timelineItem.GetElementByTestId("award-date")?.TextContent.Trim());
        Assert.Equal(status.GetDisplayName(), timelineItem.GetElementByTestId("status")?.TextContent.Trim());
        Assert.Equal(route.Name, timelineItem.GetElementByTestId("route")?.TextContent.Trim());
        Assert.Equal(startDate.ToString(UiDefaults.DateOnlyDisplayFormat), timelineItem.GetElementByTestId("start-date")!.TextContent.Trim());
        Assert.Equal(endDate.ToString(UiDefaults.DateOnlyDisplayFormat), timelineItem.GetElementByTestId("end-date")!.TextContent.Trim());
        Assert.Equal("Yes", timelineItem.GetElementByTestId("exemption")?.TextContent.Trim());
        Assert.Equal(trainingProvider.Name, timelineItem.GetElementByTestId("training-provider")?.TextContent.Trim());
        Assert.Equal(degreeType.Name, timelineItem.GetElementByTestId("degree-type")?.TextContent.Trim());
        Assert.Equal(country.Name, timelineItem.GetElementByTestId("country")?.TextContent.Trim());
        Assert.Equal(ageRange.GetDisplayName(), timelineItem.GetElementByTestId("age-range-type")?.TextContent.Trim());
        Assert.Equal(subjects.Single().Name, timelineItem.GetElementByTestId("subjects")?.TextContent.Trim());
    }

    [Fact]
    public async Task ProfessionalStatusCreatedEvent_AffectsPersonProfessionalStatus_RendersExpectedContent()
    {
        // Arrange
        var awardDate = Clock.Today;
        var route = (await ReferenceDataCache.GetRoutesToProfessionalStatusAsync())
            .Where(r => r.InductionExemptionRequired == FieldRequirement.Mandatory)
            .RandomOne();
        var person = await TestData.CreatePersonAsync(b => b
            .WithProfessionalStatus(q =>
            {
                q.WithRoute(route.RouteToProfessionalStatusId);
                q.WithStatus(ProfessionalStatusStatus.Awarded);
                q.WithAwardedDate(Clock.Today);
                q.WithInductionExemption(true);
            }));

        var professionalStatus = person.Person.Qualifications!.OfType<ProfessionalStatus>().Single();
        var createdByUser = await TestData.CreateUserAsync();
        var raisedByUser = TeachingRecordSystem.Core.Events.Models.RaisedByUserInfo.FromUserId(createdByUser.UserId);
        var professionalStatusCreatedEvent = await TestData.CreateProfessionalStatusCreatedEventAsync(e => e
            .ForPerson(person.Person)
            .WithProfessionalStatus(professionalStatus)
            .WithCreatedByUser(raisedByUser)
            .WithCreatedUtc(Clock.UtcNow)
            .WithPersonAttributes(new Core.Events.Models.ProfessionalStatusPersonAttributes() { EytsDate = awardDate, HasEyps = false, PqtsDate = null, QtsDate = null })
            .WithChanges(ProfessionalStatusCreatedEventChanges.PersonEytsDate)
            ) as ProfessionalStatusCreatedEvent;

        var qualification = person.ProfessionalStatuses.First();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var timelineItem = doc.GetElementByTestId("timeline-item-route-created-event");
        Assert.NotNull(timelineItem);
        Assert.Equal($"By {createdByUser.Name} on", timelineItem.GetElementByTestId("raised-by")?.TextContent.Trim());
        Assert.Equal(Clock.NowGmt.ToString(TimelineItem.TimestampFormat), timelineItem.GetElementByTestId("timeline-item-time")?.TextContent.Trim());
        Assert.Equal(awardDate.ToString(UiDefaults.DateOnlyDisplayFormat), timelineItem.GetElementByTestId("eyts-date")?.TextContent.Trim());
        Assert.Null(timelineItem.GetElementByTestId("pqts-date"));
        Assert.Null(timelineItem.GetElementByTestId("qts-date"));
        Assert.Null(timelineItem.GetElementByTestId("has-eyps")); Assert.Equal(professionalStatusCreatedEvent!.PersonAttributes.EytsDate?.ToString(UiDefaults.DateOnlyDisplayFormat), timelineItem.GetElementByTestId("eyts-date")?.TextContent.Trim());
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
        var route = await ReferenceDataCache.GetRouteWhereAllFieldsApplyAsync();
        var status = TestDataHelper.GetRouteStatusWhereAllFieldsApply();
        var subject = (await ReferenceDataCache.GetTrainingSubjectsAsync()).Where(x => x.TrainingSubjectId != oldSubject.TrainingSubjectId).RandomOne();
        var trainingProvider = (await ReferenceDataCache.GetTrainingProvidersAsync()).Where(x => x.TrainingProviderId != oldTrainingProvider.TrainingProviderId).RandomOne();
        var degreeType = (await ReferenceDataCache.GetDegreeTypesAsync()).Where(x => x.DegreeTypeId != oldDegreeType.DegreeTypeId).RandomOne();
        var country = (await ReferenceDataCache.GetTrainingCountriesAsync()).Where(x => x.CountryId != oldCountry.CountryId).RandomOne();
        var ageRange = TrainingAgeSpecialismType.KeyStage1;
        var person = await TestData.CreatePersonAsync(p => p
            .WithProfessionalStatus(q =>
            {
                q.WithRoute(route);
                q.WithStatus(status);
                q.WithAwardedDate(Clock.Today);
                q.WithInductionExemption(true);
                q.WithTrainingStartDate(startDate);
                q.WithTrainingEndDate(endDate);
                q.WithAwardedDate(awardDate);
                q.WithTrainingProvider(trainingProvider);
                q.WithTrainingCountryId(country.CountryId);
                q.WithTrainingSubjectIds(new List<Guid>() { subject.TrainingSubjectId }.ToArray());
                q.WithTrainingAgeSpecialismType(ageRange);
                q.WithDegreeTypeId(degreeType.DegreeTypeId);
                q.WithInductionExemption(true);
            }));

        var professionalStatus = person.Person.Qualifications!.OfType<ProfessionalStatus>().Single();
        var oldProfessionalStatus = TeachingRecordSystem.Core.Events.Models.ProfessionalStatus.FromModel(professionalStatus) with
        {
            TrainingAgeSpecialismType = ageRange
        };
        var createdByUser = await TestData.CreateUserAsync();
        var raisedByUser = TeachingRecordSystem.Core.Events.Models.RaisedByUserInfo.FromUserId(createdByUser.UserId);
        var professionalStatusCreatedEvent = await TestData.CreateProfessionalStatusCreatedEventAsync(e => e
            .ForPerson(person.Person)
            .WithProfessionalStatus(professionalStatus)
            .WithOldProfessionalStatus(oldProfessionalStatus)
            .WithCreatedByUser(raisedByUser)
            .WithCreatedUtc(Clock.UtcNow)
            ) as ProfessionalStatusCreatedEvent;

        var qualification = person.ProfessionalStatuses.First();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var timelineItem = doc.GetElementByTestId("timeline-item-route-created-event");
        Assert.NotNull(timelineItem);
        Assert.Equal($"By {createdByUser.Name} on", timelineItem.GetElementByTestId("raised-by")?.TextContent.Trim());
        Assert.Equal(Clock.NowGmt.ToString(TimelineItem.TimestampFormat), timelineItem.GetElementByTestId("timeline-item-time")?.TextContent.Trim());
        Assert.Null(timelineItem.GetElementByTestId("eyts-date"));
        Assert.Null(timelineItem.GetElementByTestId("pqts-date"));
        Assert.Null(timelineItem.GetElementByTestId("qts-date"));
        Assert.Null(timelineItem.GetElementByTestId("has-eyps"));
        Assert.Equal(oldAwardDate.ToString(UiDefaults.DateOnlyDisplayFormat), timelineItem.GetElementByTestId("award-date")?.TextContent.Trim());
        Assert.Equal(oldStatus.GetDisplayName(), timelineItem.GetElementByTestId("status")?.TextContent.Trim());
        Assert.Equal(oldRoute.Name, timelineItem.GetElementByTestId("route")?.TextContent.Trim());
        Assert.Equal(oldStartDate.ToString(UiDefaults.DateOnlyDisplayFormat), timelineItem.GetElementByTestId("start-date")!.TextContent.Trim());
        Assert.Equal(oldEndDate.ToString(UiDefaults.DateOnlyDisplayFormat), timelineItem.GetElementByTestId("end-date")!.TextContent.Trim());
        Assert.Equal("Yes", timelineItem.GetElementByTestId("exemption")?.TextContent.Trim());
        Assert.Equal(oldTrainingProvider.Name, timelineItem.GetElementByTestId("training-provider")?.TextContent.Trim());
        Assert.Equal(oldDegreeType.Name, timelineItem.GetElementByTestId("degree-type")?.TextContent.Trim());
        Assert.Equal(oldCountry.Name, timelineItem.GetElementByTestId("country")?.TextContent.Trim());
        Assert.Equal(oldAgeRange.GetDisplayName(), timelineItem.GetElementByTestId("age-range-type")?.TextContent.Trim());
        Assert.Equal(oldSubject.Name, timelineItem.GetElementByTestId("subjects")?.TextContent.Trim());
    }
}
