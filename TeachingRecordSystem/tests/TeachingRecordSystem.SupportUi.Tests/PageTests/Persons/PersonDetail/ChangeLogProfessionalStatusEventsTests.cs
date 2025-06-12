using System.Diagnostics;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail;
using TeachingRecordSystem.SupportUi.Tests.PageTests.RoutesToProfessionalStatus;
using ProfessionalStatusType = TeachingRecordSystem.Core.Models.ProfessionalStatusType;

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
        var status = RouteToProfessionalStatusStatus.InTraining;
        var subjects = (await ReferenceDataCache.GetTrainingSubjectsAsync()).Where(s => s.Name.IndexOf('\'') == -1).Take(1);
        var trainingProvider = (await ReferenceDataCache.GetTrainingProvidersAsync()).RandomOne();
        var degreeType = (await ReferenceDataCache.GetDegreeTypesAsync()).RandomOne();
        var country = (await ReferenceDataCache.GetTrainingCountriesAsync()).RandomOne();
        var ageRange = TrainingAgeSpecialismType.FoundationStage;
        var createdByUser = await TestData.CreateUserAsync();

        var person = await TestData.CreatePersonAsync(b => b
            .WithRouteToProfessionalStatus(q => q
                .WithRoute(route.RouteToProfessionalStatusTypeId)
                .WithStatus(status)
                .WithTrainingStartDate(startDate)
                .WithTrainingEndDate(endDate)
                .WithTrainingProviderId(trainingProvider.TrainingProviderId)
                .WithTrainingCountryId(country.CountryId)
                .WithTrainingSubjectIds(subjects.Select(s => s.TrainingSubjectId).ToArray())
                .WithTrainingAgeSpecialismType(ageRange)
                .WithDegreeTypeId(degreeType.DegreeTypeId)
                .WithCreatedByUser(EventModels.RaisedByUserInfo.FromUserId(createdByUser.UserId))));

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

        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.ProfessionalStatusType == ProfessionalStatusType.EarlyYearsTeacherStatus)
            .RandomOne();

        var person = await TestData.CreatePersonAsync(b => b
            .WithRouteToProfessionalStatus(q =>
            {
                q.WithRoute(route.RouteToProfessionalStatusTypeId);
                q.WithStatus(RouteToProfessionalStatusStatus.Holds);
                q.WithHoldsFrom(Clock.Today);
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
        var holdsFrom = oldAwardDate.AddDays(1);
        var startDate = oldStartDate.AddDays(1);
        var endDate = oldEndDate.AddDays(1);
        var route = oldRoute;
        var subject = (await ReferenceDataCache.GetTrainingSubjectsAsync()).Where(x => x.TrainingSubjectId != oldSubject.TrainingSubjectId).Where(s => s.Name.IndexOf('\'') == -1).RandomOne();
        var trainingProvider = (await ReferenceDataCache.GetTrainingProvidersAsync()).Where(x => x.TrainingProviderId != oldTrainingProvider.TrainingProviderId).RandomOne();
        var degreeType = (await ReferenceDataCache.GetDegreeTypesAsync()).Where(x => x.DegreeTypeId != oldDegreeType.DegreeTypeId).RandomOne();
        var country = (await ReferenceDataCache.GetTrainingCountriesAsync()).Where(x => x.CountryId != oldCountry.CountryId).RandomOne();
        var ageRange = TrainingAgeSpecialismType.KeyStage1;
        var oldExemptFromInduction = false;
        var exemptFromInduction = true;

        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(q =>
            {
                q.WithRoute(route.RouteToProfessionalStatusTypeId);
                q.WithStatus(oldStatus);
                q.WithInductionExemption(oldExemptFromInduction);
                q.WithTrainingStartDate(oldStartDate);
                q.WithTrainingEndDate(oldEndDate);
                q.WithHoldsFrom(oldAwardDate);
                q.WithTrainingProviderId(oldTrainingProvider.TrainingProviderId);
                q.WithTrainingCountryId(oldCountry.CountryId);
                q.WithTrainingSubjectIds([oldSubject.TrainingSubjectId]);
                q.WithTrainingAgeSpecialismType(oldAgeRange);
                q.WithDegreeTypeId(oldDegreeType.DegreeTypeId);
            }));

        var professionalStatus = person.Person.Qualifications!.OfType<RouteToProfessionalStatus>().Single();

        var updatedByUser = await TestData.CreateUserAsync();

        await WithDbContext(async dbContext =>
        {
            professionalStatus.Update(
                allRouteTypes: await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync(activeOnly: false),
                r =>
                {
                    r.HoldsFrom = holdsFrom;
                    r.TrainingStartDate = startDate;
                    r.TrainingEndDate = endDate;
                    r.DegreeTypeId = degreeType.DegreeTypeId;
                    r.TrainingSubjectIds = [subject.TrainingSubjectId];
                    r.TrainingProviderId = trainingProvider.TrainingProviderId;
                    r.TrainingAgeSpecialismType = ageRange;
                    r.TrainingCountryId = country.CountryId;
                    r.ExemptFromInduction = exemptFromInduction;
                },
                changeReason: null,
                changeReasonDetail: null,
                evidenceFile: null,
                updatedBy: updatedByUser.UserId,
                Clock.UtcNow,
                out var @event);

            Debug.Assert(@event is not null);

            dbContext.AddEventWithoutBroadcast(@event);

            await dbContext.SaveChangesAsync();
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var timelineItem = doc.GetElementByTestId("timeline-item-route-updated-event");
        Assert.NotNull(timelineItem);
        Assert.Equal($"By {updatedByUser.Name} on", timelineItem.GetElementByTestId("raised-by")?.TrimmedText());
        Assert.Equal(Clock.NowGmt.ToString(TimelineItem.TimestampFormat), timelineItem.GetElementByTestId("timeline-item-time")?.TrimmedText());
        Assert.Null(timelineItem.GetElementByTestId("status"));
        Assert.Equal(holdsFrom.ToString(UiDefaults.DateOnlyDisplayFormat), timelineItem.GetElementByTestId("award-date")?.TrimmedText());
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
        var oldStatus = RouteToProfessionalStatusStatus.InTraining;
        var startDate = Clock.Today.AddYears(-2);
        var endDate = startDate.AddYears(1);
        var awardDate = endDate.AddDays(1);
        var route = await ReferenceDataCache.GetRouteWhereAllFieldsApplyAsync(ProfessionalStatusType.QualifiedTeacherStatus);
        var status = RouteToProfessionalStatusStatus.Holds;
        var subject = (await ReferenceDataCache.GetTrainingSubjectsAsync()).Where(s => s.Name.IndexOf('\'') == -1).RandomOne();
        var trainingProvider = (await ReferenceDataCache.GetTrainingProvidersAsync()).RandomOne();
        var degreeType = (await ReferenceDataCache.GetDegreeTypesAsync()).RandomOne();
        var country = (await ReferenceDataCache.GetTrainingCountriesAsync()).RandomOne();
        var ageRange = TrainingAgeSpecialismType.KeyStage1;

        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(q =>
            {
                q.WithRoute(route.RouteToProfessionalStatusTypeId);
                q.WithStatus(oldStatus);
                q.WithInductionExemption(true);
                q.WithTrainingStartDate(startDate);
                q.WithTrainingEndDate(endDate);
                q.WithHoldsFrom(awardDate);
                q.WithTrainingProviderId(trainingProvider.TrainingProviderId);
                q.WithTrainingCountryId(country.CountryId);
                q.WithTrainingSubjectIds([subject.TrainingSubjectId]);
                q.WithTrainingAgeSpecialismType(ageRange);
                q.WithDegreeTypeId(degreeType.DegreeTypeId);
                q.WithInductionExemption(true);
            }));

        var professionalStatus = person.Person.Qualifications!.OfType<RouteToProfessionalStatus>().Single();

        var updatedByUser = await TestData.CreateUserAsync();

        await WithDbContext(async dbContext =>
        {
            professionalStatus.Update(
                allRouteTypes: await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync(activeOnly: false),
                r => r.Status = status,
                changeReason: null,
                changeReasonDetail: null,
                evidenceFile: null,
                updatedBy: updatedByUser.UserId,
                Clock.UtcNow,
                out var @event);
            Debug.Assert(@event is not null && @event.Changes.HasFlag(RouteToProfessionalStatusUpdatedEventChanges.Status));

            dbContext.AddEventWithoutBroadcast(@event);

            await dbContext.SaveChangesAsync();
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var timelineItem = doc.GetElementByTestId("timeline-item-route-updated-event");
        Assert.NotNull(timelineItem);
        Assert.Equal($"By {updatedByUser.Name} on", timelineItem.GetElementByTestId("raised-by")?.TrimmedText());
        Assert.Equal(Clock.NowGmt.ToString(TimelineItem.TimestampFormat), timelineItem.GetElementByTestId("timeline-item-time")?.TrimmedText());
        Assert.Equal(status.GetDisplayName(), timelineItem.GetElementByTestId("status")?.TrimmedText());
        Assert.Equal(awardDate.ToString(UiDefaults.DateOnlyDisplayFormat), timelineItem.GetElementByTestId("qts-date")?.TrimmedText());
        Assert.Null(timelineItem.GetElementByTestId("pqts-date"));
        Assert.Null(timelineItem.GetElementByTestId("eyts-date"));
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
        Assert.Equal(UiDefaults.EmptyDisplayContent, timelineItem.GetElementByTestId("old-qts-date")?.TrimmedText());
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
        var oldStatus = RouteToProfessionalStatusStatus.InTraining;
        var startDate = Clock.Today.AddYears(-2);
        var endDate = startDate.AddYears(1);
        var awardDate = endDate.AddDays(1);
        var route = await ReferenceDataCache.GetRouteWhereAllFieldsApplyAsync();
        var status = RouteToProfessionalStatusStatus.Holds;
        var changeReason = "Text from change reason selection";
        var changeReasonDetail = TestData.GenerateLoremIpsum();

        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(q =>
            {
                q.WithRoute(route.RouteToProfessionalStatusTypeId);
                q.WithStatus(oldStatus);
                q.WithInductionExemption(true);
                q.WithHoldsFrom(awardDate);
            }));

        var professionalStatus = person.Person.Qualifications!.OfType<RouteToProfessionalStatus>().Single();

        var updatedByUser = await TestData.CreateUserAsync();

        await WithDbContext(async dbContext =>
        {
            professionalStatus.Update(
                allRouteTypes: await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync(activeOnly: false),
                r => r.Status = status,
                changeReason: changeReason,
                changeReasonDetail: changeReasonDetail,
                evidenceFile: null,
                updatedBy: updatedByUser.UserId,
                Clock.UtcNow,
                out var @event);
            Debug.Assert(@event is not null && @event.Changes.HasFlag(RouteToProfessionalStatusUpdatedEventChanges.Status));

            dbContext.AddEventWithoutBroadcast(@event);

            await dbContext.SaveChangesAsync();
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var timelineItem = doc.GetElementByTestId("timeline-item-route-updated-event");
        Assert.NotNull(timelineItem);
        Assert.Equal(changeReason, timelineItem.GetElementByTestId("reason")?.TrimmedText());
        Assert.Equal(changeReasonDetail, timelineItem.GetElementByTestId("reason-detail")?.TrimmedText());
    }

    [Fact]
    public async Task ProfessionalStatusDeletedEvent_RendersExpectedContent()
    {
        // Arrange
        var startDate = Clock.Today.AddYears(-1);
        var endDate = Clock.Today.AddDays(-1);
        var holdsFrom = endDate.AddDays(1);
        var route = await ReferenceDataCache.GetRouteWhereAllFieldsApplyAsync(ProfessionalStatusType.QualifiedTeacherStatus);
        var status = RouteToProfessionalStatusStatus.Holds;
        var subjects = (await ReferenceDataCache.GetTrainingSubjectsAsync()).Where(s => s.Name.IndexOf('\'') == -1).Take(1);
        var trainingProvider = (await ReferenceDataCache.GetTrainingProvidersAsync()).RandomOne();
        var degreeType = (await ReferenceDataCache.GetDegreeTypesAsync()).RandomOne();
        var country = (await ReferenceDataCache.GetTrainingCountriesAsync()).RandomOne();
        var ageRange = TrainingAgeSpecialismType.FoundationStage;

        var person = await TestData.CreatePersonAsync(b => b
            .WithRouteToProfessionalStatus(q =>
            {
                q.WithRoute(route.RouteToProfessionalStatusTypeId);
                q.WithStatus(status);
                q.WithHoldsFrom(Clock.Today);
                q.WithInductionExemption(true);
                q.WithTrainingStartDate(startDate);
                q.WithTrainingEndDate(endDate);
                q.WithHoldsFrom(holdsFrom);
                q.WithTrainingProviderId(trainingProvider.TrainingProviderId);
                q.WithTrainingCountryId(country.CountryId);
                q.WithTrainingSubjectIds(subjects.Select(s => s.TrainingSubjectId).ToArray());
                q.WithTrainingAgeSpecialismType(ageRange);
                q.WithDegreeTypeId(degreeType.DegreeTypeId);
                q.WithInductionExemption(true);
            }));

        var professionalStatus = person.Person.Qualifications!.OfType<RouteToProfessionalStatus>().Single();
        var deletedByUser = await TestData.CreateUserAsync();

        await WithDbContext(async dbContext =>
        {
            professionalStatus.Delete(
                allRouteTypes: await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync(activeOnly: false),
                deletionReason: null,
                deletionReasonDetail: null,
                evidenceFile: null,
                deletedBy: deletedByUser.UserId,
                Clock.UtcNow,
                out var @event);

            dbContext.AddEventWithoutBroadcast(@event);

            await dbContext.SaveChangesAsync();
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var timelineItem = doc.GetElementByTestId("timeline-item-route-deleted-event");
        Assert.NotNull(timelineItem);
        Assert.Equal($"By {deletedByUser.Name} on", timelineItem.GetElementByTestId("raised-by")?.TrimmedText());
        Assert.Equal(Clock.NowGmt.ToString(TimelineItem.TimestampFormat), timelineItem.GetElementByTestId("timeline-item-time")?.TrimmedText());
        Assert.Null(timelineItem.GetElementByTestId("eyts-date"));
        Assert.Null(timelineItem.GetElementByTestId("pqts-date"));
        Assert.Equal(UiDefaults.EmptyDisplayContent, timelineItem.GetElementByTestId("qts-date")?.TrimmedText());
        Assert.Null(timelineItem.GetElementByTestId("has-eyps"));
        Assert.Equal(holdsFrom.ToString(UiDefaults.DateOnlyDisplayFormat), timelineItem.GetElementByTestId("award-date")?.TrimmedText());
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
    public async Task ProfessionalStatusDeletedEvent_AffectsPersonQts_RendersExpectedContent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(b => b.WithHoldsRouteToProfessionalStatus(ProfessionalStatusType.QualifiedTeacherStatus));

        var professionalStatus = person.Person.Qualifications!.OfType<RouteToProfessionalStatus>().Single();
        var deletedByUser = await TestData.CreateUserAsync();

        await WithDbContext(async dbContext =>
        {
            professionalStatus.Delete(
                allRouteTypes: await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync(activeOnly: false),
                deletionReason: null,
                deletionReasonDetail: null,
                evidenceFile: null,
                deletedBy: deletedByUser.UserId,
                Clock.UtcNow,
                out var @event);

            dbContext.AddEventWithoutBroadcast(@event);

            await dbContext.SaveChangesAsync();
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var timelineItem = doc.GetElementByTestId("timeline-item-route-deleted-event");
        Assert.NotNull(timelineItem);
        Assert.Equal(professionalStatus.HoldsFrom?.ToString(UiDefaults.DateOnlyDisplayFormat), timelineItem.GetElementByTestId("old-qts-date")?.TrimmedText());
        Assert.Equal(UiDefaults.EmptyDisplayContent, timelineItem.GetElementByTestId("qts-date")?.TrimmedText());
    }

    [Fact]
    public async Task ProfessionalStatusDeletedEvent_AffectsPersonEyts_RendersExpectedContent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(b => b.WithHoldsRouteToProfessionalStatus(ProfessionalStatusType.EarlyYearsTeacherStatus));

        var professionalStatus = person.Person.Qualifications!.OfType<RouteToProfessionalStatus>().Single();
        var deletedByUser = await TestData.CreateUserAsync();

        await WithDbContext(async dbContext =>
        {
            professionalStatus.Delete(
                allRouteTypes: await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync(activeOnly: false),
                deletionReason: null,
                deletionReasonDetail: null,
                evidenceFile: null,
                deletedBy: deletedByUser.UserId,
                Clock.UtcNow,
                out var @event);

            dbContext.AddEventWithoutBroadcast(@event);

            await dbContext.SaveChangesAsync();
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var timelineItem = doc.GetElementByTestId("timeline-item-route-deleted-event");
        Assert.NotNull(timelineItem);
        Assert.Equal(professionalStatus.HoldsFrom?.ToString(UiDefaults.DateOnlyDisplayFormat), timelineItem.GetElementByTestId("old-eyts-date")?.TrimmedText());
        Assert.Equal(UiDefaults.EmptyDisplayContent, timelineItem.GetElementByTestId("eyts-date")?.TrimmedText());
    }

    [Fact]
    public async Task ProfessionalStatusDeletedEvent_AffectsPersonPqts_RendersExpectedContent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(b => b.WithHoldsRouteToProfessionalStatus(ProfessionalStatusType.PartialQualifiedTeacherStatus));

        var professionalStatus = person.Person.Qualifications!.OfType<RouteToProfessionalStatus>().Single();
        var deletedByUser = await TestData.CreateUserAsync();

        await WithDbContext(async dbContext =>
        {
            professionalStatus.Delete(
                allRouteTypes: await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync(activeOnly: false),
                deletionReason: null,
                deletionReasonDetail: null,
                evidenceFile: null,
                deletedBy: deletedByUser.UserId,
                Clock.UtcNow,
                out var @event);

            dbContext.AddEventWithoutBroadcast(@event);

            await dbContext.SaveChangesAsync();
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var timelineItem = doc.GetElementByTestId("timeline-item-route-deleted-event");
        Assert.NotNull(timelineItem);
        Assert.Equal(professionalStatus.HoldsFrom?.ToString(UiDefaults.DateOnlyDisplayFormat), timelineItem.GetElementByTestId("old-pqts-date")?.TrimmedText());
        Assert.Equal(UiDefaults.EmptyDisplayContent, timelineItem.GetElementByTestId("pqts-date")?.TrimmedText());
    }

    [Fact]
    public async Task ProfessionalStatusDeletedEvent_AffectsPersonEyps_RendersExpectedContent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(b => b.WithHoldsRouteToProfessionalStatus(ProfessionalStatusType.EarlyYearsProfessionalStatus));

        var professionalStatus = person.Person.Qualifications!.OfType<RouteToProfessionalStatus>().Single();
        var deletedByUser = await TestData.CreateUserAsync();

        await WithDbContext(async dbContext =>
        {
            professionalStatus.Delete(
                allRouteTypes: await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync(activeOnly: false),
                deletionReason: null,
                deletionReasonDetail: null,
                evidenceFile: null,
                deletedBy: deletedByUser.UserId,
                Clock.UtcNow,
                out var @event);

            dbContext.AddEventWithoutBroadcast(@event);

            await dbContext.SaveChangesAsync();
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var timelineItem = doc.GetElementByTestId("timeline-item-route-deleted-event");
        Assert.NotNull(timelineItem);
        Assert.Equal("No", timelineItem.GetElementByTestId("has-eyps")?.TrimmedText());
        Assert.Equal("Yes", timelineItem.GetElementByTestId("old-has-eyps")?.TrimmedText());
    }
}
