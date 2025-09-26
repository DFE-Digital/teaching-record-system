using System.Diagnostics;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Dqt.Models;
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
        var subjects = (await ReferenceDataCache.GetTrainingSubjectsAsync()).Where(s => !s.Name.Contains('\'')).Take(1);
        var trainingProvider = (await ReferenceDataCache.GetTrainingProvidersAsync()).RandomOne();
        var degreeType = (await ReferenceDataCache.GetDegreeTypesAsync()).RandomOne();
        var country = (await ReferenceDataCache.GetTrainingCountriesAsync()).RandomOne();
        var ageRange = TrainingAgeSpecialismType.FoundationStage;
        var createdByUser = await TestData.CreateUserAsync();
        var sourceApplicationReference = "TEST-REFERENCE";

        var person = await TestData.CreatePersonAsync(b => b
            .WithRouteToProfessionalStatus(q => q
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
                .WithSourceApplicationReference(sourceApplicationReference)));

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
        Assert.Equal($"{subjects.Single().Reference} - {subjects.Single().Name}", timelineItem.GetElementByTestId("subjects")?.TrimmedText());
        Assert.Equal(sourceApplicationReference, timelineItem.GetElementByTestId("source-application-reference")?.TrimmedText());
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
                q.WithRouteType(route.RouteToProfessionalStatusTypeId);
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
    public async Task ProfessionalStatusCreatedEvent_RendersExpectedChangeReasonContent()
    {
        // Arrange
        var awardDate = Clock.Today.AddYears(-2).AddDays(1);
        var route = await ReferenceDataCache.GetRouteWhereAllFieldsApplyAsync();
        var status = RouteToProfessionalStatusStatus.Holds;
        var changeReason = "Text from change reason selection";
        var changeReasonDetail = TestData.GenerateLoremIpsum();
        var filename = "filename.txt";

        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(q =>
            {
                q.WithRouteType(route.RouteToProfessionalStatusTypeId);
                q.WithStatus(status);
                q.WithInductionExemption(true);
                q.WithHoldsFrom(awardDate);
                q.WithChangeReason(changeReason, changeReasonDetail);
                q.WithEvidenceFile(filename);
            }));

        var professionalStatus = person.Person.Qualifications!.OfType<RouteToProfessionalStatus>().Single();

        var updatedByUser = await TestData.CreateUserAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var timelineItem = doc.GetElementByTestId("timeline-item-route-created-event");
        Assert.NotNull(timelineItem);
        Assert.Equal(changeReason, timelineItem.GetElementByTestId("reason")?.TrimmedText());
        Assert.Equal(changeReasonDetail, timelineItem.GetElementByTestId("reason-detail")?.TrimmedText());
        Assert.Equal($"{filename} (opens in new tab)", timelineItem.GetElementByTestId("uploaded-evidence-link")?.TrimmedText());
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
        var oldExemptFromInduction = false;
        var oldSourceApplicationReference = "TEST-REFERENCE";
        var holdsFrom = oldAwardDate.AddDays(1);
        var startDate = oldStartDate.AddDays(1);
        var endDate = oldEndDate.AddDays(1);
        var route = oldRoute;
        var subject = (await ReferenceDataCache.GetTrainingSubjectsAsync()).Where(x => x.TrainingSubjectId != oldSubject.TrainingSubjectId).Where(s => !s.Name.Contains('\'')).RandomOne();
        var trainingProvider = (await ReferenceDataCache.GetTrainingProvidersAsync()).Where(x => x.TrainingProviderId != oldTrainingProvider.TrainingProviderId).RandomOne();
        var degreeType = (await ReferenceDataCache.GetDegreeTypesAsync()).Where(x => x.DegreeTypeId != oldDegreeType.DegreeTypeId).RandomOne();
        var country = (await ReferenceDataCache.GetTrainingCountriesAsync()).Where(x => x.CountryId != oldCountry.CountryId).RandomOne();
        var ageRange = TrainingAgeSpecialismType.KeyStage1;
        var exemptFromInduction = true;


        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(q =>
            {
                q.WithRouteType(route.RouteToProfessionalStatusTypeId);
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
                q.WithSourceApplicationReference(oldSourceApplicationReference);
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
        Assert.Equal($"{subject.Reference} - {subject.Name}", timelineItem.GetElementByTestId("subjects")?.TrimmedText());
        Assert.Equal(oldSourceApplicationReference, timelineItem.GetElementByTestId("source-application-reference")?.TrimmedText());

        Assert.Equal(oldAwardDate.ToString(UiDefaults.DateOnlyDisplayFormat), timelineItem.GetElementByTestId("old-award-date")?.TrimmedText());
        Assert.Null(timelineItem.GetElementByTestId("status"));
        Assert.Equal(oldStartDate.ToString(UiDefaults.DateOnlyDisplayFormat), timelineItem.GetElementByTestId("old-start-date")!.TrimmedText());
        Assert.Equal(oldEndDate.ToString(UiDefaults.DateOnlyDisplayFormat), timelineItem.GetElementByTestId("old-end-date")!.TrimmedText());
        Assert.Equal("No", timelineItem.GetElementByTestId("old-exemption")?.TrimmedText());
        Assert.Equal(oldTrainingProvider.Name, timelineItem.GetElementByTestId("old-training-provider")?.TrimmedText());
        Assert.Equal(oldDegreeType.Name, timelineItem.GetElementByTestId("old-degree-type")?.TrimmedText());
        Assert.Equal(oldCountry.Name, timelineItem.GetElementByTestId("old-country")?.TrimmedText());
        Assert.Equal(oldAgeRange.GetDisplayName(), timelineItem.GetElementByTestId("old-age-range-type")?.TrimmedText());
        Assert.Equal($"{oldSubject.Reference} - {oldSubject.Name}", timelineItem.GetElementByTestId("old-subjects")?.TrimmedText());
        Assert.Equal(oldSourceApplicationReference, timelineItem.GetElementByTestId("old-source-application-reference")?.TrimmedText());
    }

    [Fact]
    public async Task ProfessionalStatusUpdatedEvent_StatusChangedToHolds_PersonQtsChanged_RendersExpectedContent()
    {
        // Arrange
        var oldStatus = RouteToProfessionalStatusStatus.InTraining;
        var startDate = Clock.Today.AddYears(-2);
        var endDate = startDate.AddYears(1);
        var awardDate = endDate.AddDays(1);
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.ProfessionalStatusType == ProfessionalStatusType.QualifiedTeacherStatus)
            .RandomOne();
        var status = RouteToProfessionalStatusStatus.Holds;
        var subject = (await ReferenceDataCache.GetTrainingSubjectsAsync()).Where(s => !s.Name.Contains('\'')).RandomOne();
        var trainingProvider = (await ReferenceDataCache.GetTrainingProvidersAsync()).RandomOne();
        var degreeType = (await ReferenceDataCache.GetDegreeTypesAsync()).RandomOne();
        var country = (await ReferenceDataCache.GetTrainingCountriesAsync()).RandomOne();
        var ageRange = TrainingAgeSpecialismType.KeyStage1;

        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(q =>
            {
                q.WithRouteType(route.RouteToProfessionalStatusTypeId);
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
                q.WithRouteType(route.RouteToProfessionalStatusTypeId);
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
        var subjects = (await ReferenceDataCache.GetTrainingSubjectsAsync()).Where(s => !s.Name.Contains('\'')).Take(1);
        var trainingProvider = (await ReferenceDataCache.GetTrainingProvidersAsync()).RandomOne();
        var degreeType = (await ReferenceDataCache.GetDegreeTypesAsync()).RandomOne();
        var country = (await ReferenceDataCache.GetTrainingCountriesAsync()).RandomOne();
        var ageRange = TrainingAgeSpecialismType.FoundationStage;
        var sourceApplicationReference = "TEST-REFERENCE";

        var person = await TestData.CreatePersonAsync(b => b
            .WithRouteToProfessionalStatus(q =>
            {
                q.WithRouteType(route.RouteToProfessionalStatusTypeId);
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
                q.WithSourceApplicationReference(sourceApplicationReference);
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
        Assert.Equal($"{subjects.Single().Reference} - {subjects.Single().Name}", timelineItem.GetElementByTestId("subjects")?.TrimmedText());
        Assert.Equal(sourceApplicationReference, timelineItem.GetElementByTestId("source-application-reference")?.TrimmedText());
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

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ProfessionalStatusMigratedEvent_RendersExpectedContent(bool populateOptional)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var startDate = Clock.Today.AddYears(-1);
        var endDate = Clock.Today.AddDays(-1);
        var awardDate = endDate.AddDays(1);
        var route = await ReferenceDataCache.GetRouteWhereAllFieldsApplyAsync();
        var status = populateOptional ? RouteToProfessionalStatusStatus.Holds : RouteToProfessionalStatusStatus.InTraining;
        var subjects = (await ReferenceDataCache.GetTrainingSubjectsAsync()).Where(s => !s.Name.Contains('\'')).Take(1);
        var trainingProvider = (await ReferenceDataCache.GetTrainingProvidersAsync()).RandomOne();
        var degreeType = (await ReferenceDataCache.GetDegreeTypesAsync()).RandomOne();
        var country = (await ReferenceDataCache.GetTrainingCountriesAsync()).RandomOne();
        var ageRangeType = TrainingAgeSpecialismType.Range;
        var ageRangeFrom = 10;
        var ageRangeTo = 16;
        var createdByUser = await TestData.CreateUserAsync();
        var sourceApplicationUserId = Guid.NewGuid();
        var sourceApplicationReference = "source-application-reference";
        var qualifiedTeacherStatus = LegacyDataCache.Instance.GetTeacherStatusByValue("71");
        var earlyYearsTeacherStatus = LegacyDataCache.Instance.GetEarlyYearsStatusByValue("221");
        var qtsDate = awardDate;
        var eytsDate = awardDate.AddDays(1);
        var pqtsDate = awardDate.AddDays(2);
        var qtlsDate = awardDate.AddDays(3);
        var ittQualification = new { dfeta_Value = "008", dfeta_name = "Qualification" };
        var ittProvider = new { dfeta_UKPRN = "10007799", Name = "ITT Provider", Id = Guid.NewGuid() };
        var ittSubject1 = new { dfeta_Value = "100078", dfeta_name = "Subject 1" };
        var ittSubject2 = new { dfeta_Value = "100079", dfeta_name = "Subject 2" };
        var ittSubject3 = new { dfeta_Value = "100343", dfeta_name = "Subject 3" };
        var dqtAgeRangeFrom = dfeta_AgeRange._10;
        var dqtAgeRangeTo = dfeta_AgeRange._16;
        var programmeType = dfeta_ITTProgrammeType.AssessmentOnlyRoute;
        var ittResult = dfeta_ITTResult.Pass;
        var dqtCountry = new { dfeta_Value = "XK", dfeta_name = "United Kingdom" };

        // Use populateOptional to deliberately populate OR not populate ALL optional fields to test rendering.
        // (even though in reality not all combinations of these fields would happen).
        EventModels.RouteToProfessionalStatus routeToProfessionalStatus;
        EventModels.DqtQtsRegistration? dqtQtsRegistration = null;
        EventModels.DqtInitialTeacherTraining? dqtInitialTeacherTraining = null;

        if (populateOptional)
        {
            routeToProfessionalStatus = new EventModels.RouteToProfessionalStatus
            {
                QualificationId = Guid.NewGuid(),
                RouteToProfessionalStatusTypeId = route.RouteToProfessionalStatusTypeId,
                Status = status,
                TrainingStartDate = startDate,
                TrainingEndDate = endDate,
                HoldsFrom = awardDate,
                TrainingProviderId = trainingProvider.TrainingProviderId,
                TrainingCountryId = country.CountryId,
                TrainingSubjectIds = subjects.Select(s => s.TrainingSubjectId).ToArray(),
                TrainingAgeSpecialismType = ageRangeType,
                TrainingAgeSpecialismRangeFrom = ageRangeFrom,
                TrainingAgeSpecialismRangeTo = ageRangeTo,
                DegreeTypeId = degreeType.DegreeTypeId,
                SourceApplicationUserId = sourceApplicationUserId,
                SourceApplicationReference = sourceApplicationReference,
                ExemptFromInduction = true,
                ExemptFromInductionDueToQtsDate = true
            };

            dqtQtsRegistration = new EventModels.DqtQtsRegistration
            {
                QtsRegistrationId = Guid.NewGuid(),
                TeacherStatusName = qualifiedTeacherStatus.Name,
                TeacherStatusValue = qualifiedTeacherStatus.Value,
                EarlyYearsStatusName = earlyYearsTeacherStatus.Name,
                EarlyYearsStatusValue = earlyYearsTeacherStatus.Value,
                QtsDate = qtsDate,
                EytsDate = eytsDate,
                PartialRecognitionDate = pqtsDate
            };

            dqtInitialTeacherTraining = new EventModels.DqtInitialTeacherTraining
            {
                InitialTeacherTrainingId = Guid.NewGuid(),
                SlugId = sourceApplicationReference,
                ProgrammeType = programmeType.ToString(),
                ProgrammeStartDate = startDate,
                ProgrammeEndDate = endDate,
                Result = ittResult.ToString(),
                QualificationName = ittQualification.dfeta_name,
                QualificationValue = ittQualification.dfeta_Value,
                ProviderId = ittProvider.Id,
                ProviderName = ittProvider.Name,
                ProviderUkprn = ittProvider.dfeta_UKPRN,
                CountryName = dqtCountry.dfeta_name,
                CountryValue = dqtCountry.dfeta_Value,
                Subject1Name = ittSubject1.dfeta_name,
                Subject1Value = ittSubject1.dfeta_Value,
                Subject2Name = ittSubject2.dfeta_name,
                Subject2Value = ittSubject2.dfeta_Value,
                Subject3Name = ittSubject3.dfeta_name,
                Subject3Value = ittSubject3.dfeta_Value,
                AgeRangeFrom = dqtAgeRangeFrom.ToString(),
                AgeRangeTo = dqtAgeRangeTo.ToString()
            };
        }
        else
        {
            routeToProfessionalStatus = new EventModels.RouteToProfessionalStatus
            {
                QualificationId = Guid.NewGuid(),
                RouteToProfessionalStatusTypeId = route.RouteToProfessionalStatusTypeId,
                Status = status,
                TrainingStartDate = null,
                TrainingEndDate = null,
                HoldsFrom = null,
                TrainingProviderId = null,
                TrainingCountryId = null,
                TrainingSubjectIds = [],
                TrainingAgeSpecialismType = null,
                TrainingAgeSpecialismRangeFrom = null,
                TrainingAgeSpecialismRangeTo = null,
                DegreeTypeId = null,
                SourceApplicationUserId = null,
                SourceApplicationReference = null,
                ExemptFromInduction = null,
                ExemptFromInductionDueToQtsDate = null
            };
        }

        var migratedEvent = new RouteToProfessionalStatusMigratedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = Clock.UtcNow,
            RaisedBy = createdByUser.UserId,
            PersonId = person.PersonId,
            RouteToProfessionalStatus = routeToProfessionalStatus,
            DqtQtsRegistration = dqtQtsRegistration,
            DqtInitialTeacherTraining = dqtInitialTeacherTraining,
            DqtQtlsDate = populateOptional ? qtlsDate : null,
            DqtQtlsDateHasBeenSet = populateOptional ? true : null,
            PersonAttributes = EventModels.ProfessionalStatusPersonAttributes.FromModel(person.Person),
            OldPersonAttributes = EventModels.ProfessionalStatusPersonAttributes.FromModel(person.Person)
        };

        await WithDbContext(async dbContext =>
        {
            dbContext.AddEventWithoutBroadcast(migratedEvent);
            await dbContext.SaveChangesAsync();
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var timelineItem = doc.GetElementByTestId("timeline-item-route-migrated-event");
        Assert.NotNull(timelineItem);
        Assert.Equal($"By {createdByUser.Name} on", timelineItem.GetElementByTestId("raised-by")?.TrimmedText());
        Assert.Equal(populateOptional ? awardDate.ToString(UiDefaults.DateOnlyDisplayFormat) : UiDefaults.EmptyDisplayContent, timelineItem.GetElementByTestId("award-date")?.TrimmedText());
        Assert.Equal(status.GetDisplayName(), timelineItem.GetElementByTestId("status")?.TrimmedText());
        Assert.Equal(route.Name, timelineItem.GetElementByTestId("route")?.TrimmedText());
        Assert.Equal(populateOptional ? startDate.ToString(UiDefaults.DateOnlyDisplayFormat) : UiDefaults.EmptyDisplayContent, timelineItem.GetElementByTestId("start-date")!.TrimmedText());
        Assert.Equal(populateOptional ? endDate.ToString(UiDefaults.DateOnlyDisplayFormat) : UiDefaults.EmptyDisplayContent, timelineItem.GetElementByTestId("end-date")!.TrimmedText());
        Assert.Equal(populateOptional ? "Yes" : UiDefaults.EmptyDisplayContent, timelineItem.GetElementByTestId("exemption")?.TrimmedText());
        Assert.Equal(populateOptional ? trainingProvider.Name : UiDefaults.EmptyDisplayContent, timelineItem.GetElementByTestId("training-provider")?.TrimmedText());
        Assert.Equal(populateOptional ? degreeType.Name : UiDefaults.EmptyDisplayContent, timelineItem.GetElementByTestId("degree-type")?.TrimmedText());
        Assert.Equal(populateOptional ? country.Name : UiDefaults.EmptyDisplayContent, timelineItem.GetElementByTestId("country")?.TrimmedText());
        Assert.Equal(populateOptional ? $"From {ageRangeFrom} to {ageRangeTo}" : UiDefaults.EmptyDisplayContent, timelineItem.GetElementByTestId("age-range")?.TrimmedText());
        Assert.Equal(populateOptional ? $"{subjects.Single().Reference} - {subjects.Single().Name}" : UiDefaults.EmptyDisplayContent, timelineItem.GetElementByTestId("subjects")?.TrimmedText());
        Assert.Equal(populateOptional ? sourceApplicationReference : UiDefaults.EmptyDisplayContent, timelineItem.GetElementByTestId("source-application-reference")?.TrimmedText());
        Assert.Equal(populateOptional ? dqtQtsRegistration!.QtsRegistrationId.ToString() : UiDefaults.EmptyDisplayContent, timelineItem.GetElementByTestId("qts-registration-id")?.TrimmedText());
        Assert.Equal(populateOptional ? dqtQtsRegistration!.TeacherStatusName : UiDefaults.EmptyDisplayContent, timelineItem.GetElementByTestId("teacher-status-name")?.TrimmedText());
        Assert.Equal(populateOptional ? dqtQtsRegistration!.TeacherStatusValue : UiDefaults.EmptyDisplayContent, timelineItem.GetElementByTestId("teacher-status-value")?.TrimmedText());
        Assert.Equal(populateOptional ? dqtQtsRegistration!.EarlyYearsStatusName : UiDefaults.EmptyDisplayContent, timelineItem.GetElementByTestId("early-years-status-name")?.TrimmedText());
        Assert.Equal(populateOptional ? dqtQtsRegistration!.EarlyYearsStatusValue : UiDefaults.EmptyDisplayContent, timelineItem.GetElementByTestId("early-years-status-value")?.TrimmedText());
        Assert.Equal(populateOptional ? qtsDate.ToString(UiDefaults.DateOnlyDisplayFormat) : UiDefaults.EmptyDisplayContent, timelineItem.GetElementByTestId("qts-date")?.TrimmedText());
        Assert.Equal(populateOptional ? eytsDate.ToString(UiDefaults.DateOnlyDisplayFormat) : UiDefaults.EmptyDisplayContent, timelineItem.GetElementByTestId("eyts-date")?.TrimmedText());
        Assert.Equal(populateOptional ? pqtsDate.ToString(UiDefaults.DateOnlyDisplayFormat) : UiDefaults.EmptyDisplayContent, timelineItem.GetElementByTestId("pqts-date")?.TrimmedText());
        Assert.Equal(populateOptional ? qtlsDate.ToString(UiDefaults.DateOnlyDisplayFormat) : UiDefaults.EmptyDisplayContent, timelineItem.GetElementByTestId("qtls-date")?.TrimmedText());
        Assert.Equal(populateOptional ? dqtInitialTeacherTraining!.InitialTeacherTrainingId.ToString() : UiDefaults.EmptyDisplayContent, timelineItem.GetElementByTestId("itt-id")?.TrimmedText());
        Assert.Equal(populateOptional ? dqtInitialTeacherTraining?.SlugId : UiDefaults.EmptyDisplayContent, timelineItem.GetElementByTestId("slug-id")?.TrimmedText());
        Assert.Equal(populateOptional ? dqtInitialTeacherTraining?.ProgrammeType : UiDefaults.EmptyDisplayContent, timelineItem.GetElementByTestId("programme-type")?.TrimmedText());
        Assert.Equal(populateOptional ? startDate.ToString(UiDefaults.DateOnlyDisplayFormat) : UiDefaults.EmptyDisplayContent, timelineItem.GetElementByTestId("programme-start-date")?.TrimmedText());
        Assert.Equal(populateOptional ? endDate.ToString(UiDefaults.DateOnlyDisplayFormat) : UiDefaults.EmptyDisplayContent, timelineItem.GetElementByTestId("programme-end-date")?.TrimmedText());
        Assert.Equal(populateOptional ? ittResult.ToString() : UiDefaults.EmptyDisplayContent, timelineItem.GetElementByTestId("itt-result")?.TrimmedText());
        Assert.Equal(populateOptional ? ittQualification.dfeta_name : UiDefaults.EmptyDisplayContent, timelineItem.GetElementByTestId("qualification-name")?.TrimmedText());
        Assert.Equal(populateOptional ? ittQualification.dfeta_Value : UiDefaults.EmptyDisplayContent, timelineItem.GetElementByTestId("qualification-value")?.TrimmedText());
        Assert.Equal(populateOptional ? ittProvider!.Id.ToString() : UiDefaults.EmptyDisplayContent, timelineItem.GetElementByTestId("provider-id")?.TrimmedText());
        Assert.Equal(populateOptional ? ittProvider!.Name : UiDefaults.EmptyDisplayContent, timelineItem.GetElementByTestId("provider-name")?.TrimmedText());
        Assert.Equal(populateOptional ? ittProvider!.dfeta_UKPRN : UiDefaults.EmptyDisplayContent, timelineItem.GetElementByTestId("provider-ukprn")?.TrimmedText());
        Assert.Equal(populateOptional ? dqtCountry!.dfeta_name : UiDefaults.EmptyDisplayContent, timelineItem.GetElementByTestId("country-name")?.TrimmedText());
        Assert.Equal(populateOptional ? dqtCountry!.dfeta_Value : UiDefaults.EmptyDisplayContent, timelineItem.GetElementByTestId("country-value")?.TrimmedText());
        Assert.Equal(populateOptional ? dqtAgeRangeFrom.ToString() : UiDefaults.EmptyDisplayContent, timelineItem.GetElementByTestId("dqt-age-range-from")?.TrimmedText());
        Assert.Equal(populateOptional ? dqtAgeRangeTo.ToString() : UiDefaults.EmptyDisplayContent, timelineItem.GetElementByTestId("dqt-age-range-to")?.TrimmedText());
        Assert.Equal(populateOptional ? $"{ittSubject1!.dfeta_Value} - {ittSubject1!.dfeta_name}{ittSubject2!.dfeta_Value} - {ittSubject2!.dfeta_name}{ittSubject3!.dfeta_Value} - {ittSubject3!.dfeta_name}" : UiDefaults.EmptyDisplayContent, timelineItem.GetElementByTestId("dqt-subjects")?.TrimmedText());
    }

    [Fact]
    public async Task DqtInitialTeacherTrainingCreatedEvent_RendersExpectedContent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var ittResult = dfeta_ITTResult.InTraining;
        var createdByUser = await TestData.CreateUserAsync();

        var createdEvent = new DqtInitialTeacherTrainingCreatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = Clock.UtcNow,
            RaisedBy = createdByUser.UserId,
            PersonId = person.PersonId,
            InitialTeacherTraining = new EventModels.DqtInitialTeacherTraining
            {
                Result = ittResult.ToString()
            }
        };

        await WithDbContext(async dbContext =>
        {
            dbContext.AddEventWithoutBroadcast(createdEvent);
            await dbContext.SaveChangesAsync();
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var timelineItem = doc.GetElementByTestId("timeline-item-dqt-itt-created-event");
        Assert.NotNull(timelineItem);
        Assert.Equal($"By {createdByUser.Name} on", timelineItem.GetElementByTestId("raised-by")?.TrimmedText());
        Assert.Equal(ittResult.ToString(), timelineItem.GetElementByTestId("itt-result")?.TrimmedText());
    }

    [Fact]
    public async Task DqtInitialTeacherTrainingUpdatedEvent_RendersExpectedContent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var ittResult = dfeta_ITTResult.Pass;
        var oldIttResult = dfeta_ITTResult.InTraining;
        var createdByUser = await TestData.CreateUserAsync();

        var updatedEvent = new DqtInitialTeacherTrainingUpdatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = Clock.UtcNow,
            RaisedBy = createdByUser.UserId,
            PersonId = person.PersonId,
            InitialTeacherTraining = new EventModels.DqtInitialTeacherTraining
            {
                Result = ittResult.ToString()
            },
            OldInitialTeacherTraining = new EventModels.DqtInitialTeacherTraining
            {
                Result = oldIttResult.ToString()
            },
            Changes = DqtInitialTeacherTrainingUpdatedEventChanges.Result
        };

        await WithDbContext(async dbContext =>
        {
            dbContext.AddEventWithoutBroadcast(updatedEvent);
            await dbContext.SaveChangesAsync();
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var timelineItem = doc.GetElementByTestId("timeline-item-dqt-itt-updated-event");
        Assert.NotNull(timelineItem);
        Assert.Equal($"By {createdByUser.Name} on", timelineItem.GetElementByTestId("raised-by")?.TrimmedText());
        Assert.Equal(ittResult.ToString(), timelineItem.GetElementByTestId("itt-result")?.TrimmedText());
        Assert.Equal(oldIttResult.ToString(), timelineItem.GetElementByTestId("old-itt-result")?.TrimmedText());
    }

    [Theory]
    [InlineData(DqtQtsRegistrationUpdatedEventChanges.TeacherStatusValue)]
    [InlineData(DqtQtsRegistrationUpdatedEventChanges.EarlyYearsStatusValue)]
    [InlineData(DqtQtsRegistrationUpdatedEventChanges.QtsDate)]
    [InlineData(DqtQtsRegistrationUpdatedEventChanges.EytsDate)]
    public async Task DqtQtsRegistrationCreatedEvent_RendersExpectedContent(DqtQtsRegistrationUpdatedEventChanges changes)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var teacherStatusName = "Trainee teacher";
        var earlyYearsStatusName = "Early Years Trainee";
        var qtsDate = Clock.Today.AddDays(-100);
        var eytsDate = Clock.Today.AddDays(-50);
        var createdByUser = await TestData.CreateUserAsync();

        var createdEvent = new DqtQtsRegistrationCreatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = Clock.UtcNow,
            RaisedBy = createdByUser.UserId,
            PersonId = person.PersonId,
            QtsRegistration = new EventModels.DqtQtsRegistration
            {
                TeacherStatusName = changes == DqtQtsRegistrationUpdatedEventChanges.TeacherStatusValue ? teacherStatusName : null,
                EarlyYearsStatusName = changes == DqtQtsRegistrationUpdatedEventChanges.EarlyYearsStatusValue ? earlyYearsStatusName : null,
                QtsDate = changes == DqtQtsRegistrationUpdatedEventChanges.QtsDate ? qtsDate : null,
                EytsDate = changes == DqtQtsRegistrationUpdatedEventChanges.EytsDate ? eytsDate : null
            }
        };

        await WithDbContext(async dbContext =>
        {
            dbContext.AddEventWithoutBroadcast(createdEvent);
            await dbContext.SaveChangesAsync();
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var timelineItem = doc.GetElementByTestId("timeline-item-dqt-qts-created-event");
        Assert.NotNull(timelineItem);
        Assert.Equal($"By {createdByUser.Name} on", timelineItem.GetElementByTestId("raised-by")?.TrimmedText());
        if (changes == DqtQtsRegistrationUpdatedEventChanges.TeacherStatusValue)
        {
            Assert.Equal(teacherStatusName, timelineItem.GetElementByTestId("teacher-status-name")?.TrimmedText());
        }
        else
        {
            Assert.Null(timelineItem.GetElementByTestId("teacher-status-name"));
        }

        if (changes == DqtQtsRegistrationUpdatedEventChanges.EarlyYearsStatusValue)
        {
            Assert.Equal(earlyYearsStatusName, timelineItem.GetElementByTestId("early-years-status-name")?.TrimmedText());
        }
        else
        {
            Assert.Null(timelineItem.GetElementByTestId("early-years-status-name"));
        }

        if (changes == DqtQtsRegistrationUpdatedEventChanges.QtsDate)
        {
            Assert.Equal(qtsDate.ToString(UiDefaults.DateOnlyDisplayFormat), timelineItem.GetElementByTestId("qts-date")?.TrimmedText());
        }
        else
        {
            Assert.Null(timelineItem.GetElementByTestId("qts-date"));
        }

        if (changes == DqtQtsRegistrationUpdatedEventChanges.EytsDate)
        {
            Assert.Equal(eytsDate.ToString(UiDefaults.DateOnlyDisplayFormat), timelineItem.GetElementByTestId("eyts-date")?.TrimmedText());
        }
        else
        {
            Assert.Null(timelineItem.GetElementByTestId("eyts-date"));
        }
    }

    [Theory]
    [InlineData(DqtQtsRegistrationUpdatedEventChanges.TeacherStatusValue)]
    [InlineData(DqtQtsRegistrationUpdatedEventChanges.EarlyYearsStatusValue)]
    [InlineData(DqtQtsRegistrationUpdatedEventChanges.QtsDate)]
    [InlineData(DqtQtsRegistrationUpdatedEventChanges.EytsDate)]
    public async Task DqtQtsRegistrationUpdatedEvent_RendersExpectedContent(DqtQtsRegistrationUpdatedEventChanges changes)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var oldTeacherStatusName = "Trainee teacher";
        var oldEarlyYearsStatusName = "Early Years Trainee";
        var oldQtsDate = Clock.Today.AddDays(-100);
        var oldEytsDate = Clock.Today.AddDays(-50);
        var teacherStatusName = "Qualified Teacher (trained)";
        var earlyYearsStatusName = "Early Years Teacher Status";
        var qtsDate = oldQtsDate.AddDays(1);
        var eytsDate = oldEytsDate.AddDays(1);
        var createdByUser = await TestData.CreateUserAsync();

        var updatedEvent = new DqtQtsRegistrationUpdatedEvent
        {
            EventId = Guid.NewGuid(),
            CreatedUtc = Clock.UtcNow,
            RaisedBy = createdByUser.UserId,
            PersonId = person.PersonId,
            QtsRegistration = new EventModels.DqtQtsRegistration
            {
                TeacherStatusName = changes == DqtQtsRegistrationUpdatedEventChanges.TeacherStatusValue ? teacherStatusName : null,
                EarlyYearsStatusName = changes == DqtQtsRegistrationUpdatedEventChanges.EarlyYearsStatusValue ? earlyYearsStatusName : null,
                QtsDate = changes == DqtQtsRegistrationUpdatedEventChanges.QtsDate ? qtsDate : null,
                EytsDate = changes == DqtQtsRegistrationUpdatedEventChanges.EytsDate ? eytsDate : null
            },
            OldQtsRegistration = new EventModels.DqtQtsRegistration
            {
                TeacherStatusName = changes == DqtQtsRegistrationUpdatedEventChanges.TeacherStatusValue ? oldTeacherStatusName : null,
                EarlyYearsStatusName = changes == DqtQtsRegistrationUpdatedEventChanges.EarlyYearsStatusValue ? oldEarlyYearsStatusName : null,
                QtsDate = changes == DqtQtsRegistrationUpdatedEventChanges.QtsDate ? oldQtsDate : null,
                EytsDate = changes == DqtQtsRegistrationUpdatedEventChanges.EytsDate ? oldEytsDate : null
            },
            Changes = changes
        };

        await WithDbContext(async dbContext =>
        {
            dbContext.AddEventWithoutBroadcast(updatedEvent);
            await dbContext.SaveChangesAsync();
        });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var timelineItem = doc.GetElementByTestId("timeline-item-dqt-qts-updated-event");
        Assert.NotNull(timelineItem);
        Assert.Equal($"By {createdByUser.Name} on", timelineItem.GetElementByTestId("raised-by")?.TrimmedText());
        if (changes == DqtQtsRegistrationUpdatedEventChanges.TeacherStatusValue)
        {
            Assert.Equal(teacherStatusName, timelineItem.GetElementByTestId("teacher-status-name")?.TrimmedText());
            Assert.Equal(oldTeacherStatusName, timelineItem.GetElementByTestId("old-teacher-status-name")?.TrimmedText());
        }
        else
        {
            Assert.Null(timelineItem.GetElementByTestId("teacher-status-name"));
            Assert.Null(timelineItem.GetElementByTestId("old-teacher-status-name"));
        }

        if (changes == DqtQtsRegistrationUpdatedEventChanges.EarlyYearsStatusValue)
        {
            Assert.Equal(earlyYearsStatusName, timelineItem.GetElementByTestId("early-years-status-name")?.TrimmedText());
            Assert.Equal(oldEarlyYearsStatusName, timelineItem.GetElementByTestId("old-early-years-status-name")?.TrimmedText());
        }
        else
        {
            Assert.Null(timelineItem.GetElementByTestId("early-years-status-name"));
            Assert.Null(timelineItem.GetElementByTestId("old-early-years-status-name"));
        }

        if (changes == DqtQtsRegistrationUpdatedEventChanges.QtsDate)
        {
            Assert.Equal(qtsDate.ToString(UiDefaults.DateOnlyDisplayFormat), timelineItem.GetElementByTestId("qts-date")?.TrimmedText());
            Assert.Equal(oldQtsDate.ToString(UiDefaults.DateOnlyDisplayFormat), timelineItem.GetElementByTestId("old-qts-date")?.TrimmedText());
        }
        else
        {
            Assert.Null(timelineItem.GetElementByTestId("qts-date"));
            Assert.Null(timelineItem.GetElementByTestId("old-qts-date"));
        }

        if (changes == DqtQtsRegistrationUpdatedEventChanges.EytsDate)
        {
            Assert.Equal(eytsDate.ToString(UiDefaults.DateOnlyDisplayFormat), timelineItem.GetElementByTestId("eyts-date")?.TrimmedText());
            Assert.Equal(oldEytsDate.ToString(UiDefaults.DateOnlyDisplayFormat), timelineItem.GetElementByTestId("old-eyts-date")?.TrimmedText());
        }
        else
        {
            Assert.Null(timelineItem.GetElementByTestId("eyts-date"));
            Assert.Null(timelineItem.GetElementByTestId("old-eyts-date"));
        }
    }
}
