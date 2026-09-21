using System.Diagnostics;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Optional;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.ChangeReasons;
using TeachingRecordSystem.Core.Services.RoutesToProfessionalStatus;
using TeachingRecordSystem.SupportUi.Tests.PageTests.RoutesToProfessionalStatus;
using ProfessionalStatusType = TeachingRecordSystem.Core.Models.ProfessionalStatusType;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail;

public class ChangeLogRouteToProfessionalStatusProcessTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task ProfessionalStatusCreatedEvent_RendersExpectedContent()
    {
        // Arrange
        var startDate = TimeProvider.Today.AddYears(-1);
        var endDate = TimeProvider.Today.AddDays(-1);
        var route = await ReferenceDataCache.GetRouteWhereAllFieldsApplyAsync();
        var status = RouteToProfessionalStatusStatus.InTraining;
        var subjects = (await ReferenceDataCache.GetTrainingSubjectsAsync()).Where(s => !s.Name.Contains('\'')).Take(1);
        var trainingProvider = (await ReferenceDataCache.GetTrainingProvidersAsync()).Where(s => !s.Name.Contains('\'')).SingleRandom();
        var degreeType = (await ReferenceDataCache.GetDegreeTypesAsync()).SingleRandom();
        var country = (await ReferenceDataCache.GetTrainingCountriesAsync()).SingleRandom();
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

        var timelineItem = await GetChangeHistoryEntryAsync(doc, person.PersonId, ProcessType.RouteToProfessionalStatusCreating);
        AssertRaisedBy(timelineItem, createdByUser.Name);
        Assert.Null(timelineItem.GetElementByTestId("eyts-date"));
        Assert.Null(timelineItem.GetElementByTestId("pqts-date"));
        Assert.Null(timelineItem.GetElementByTestId("qts-date"));
        Assert.Null(timelineItem.GetElementByTestId("has-eyps"));
        Assert.Equal("Not provided", timelineItem.GetElementByTestId("award-date")?.TrimmedText());
        Assert.Equal(status.GetTitle(), timelineItem.GetElementByTestId("status")?.TrimmedText());
        Assert.Equal(route.Name, timelineItem.GetElementByTestId("route-name")?.TrimmedText());
        Assert.Equal(startDate.ToString(WebConstants.DateDisplayFormat), timelineItem.GetElementByTestId("start-date")!.TrimmedText());
        Assert.Equal(endDate.ToString(WebConstants.DateDisplayFormat), timelineItem.GetElementByTestId("end-date")!.TrimmedText());
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
        var awardDate = TimeProvider.Today;

        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.ProfessionalStatusType == ProfessionalStatusType.EarlyYearsTeacherStatus)
            .SingleRandom();

        var person = await TestData.CreatePersonAsync(b => b
            .WithRouteToProfessionalStatus(q =>
            {
                q.WithRouteType(route.RouteToProfessionalStatusTypeId);
                q.WithStatus(RouteToProfessionalStatusStatus.Holds);
                q.WithHoldsFrom(TimeProvider.Today);
                q.WithInductionExemption(true);
            }));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var timelineItem = await GetChangeHistoryEntryAsync(doc, person.PersonId, ProcessType.RouteToProfessionalStatusCreating);
        Assert.Equal($"EYTS date changed from None to {awardDate.ToString(WebConstants.DateDisplayFormat)}", timelineItem.GetElementByTestId("eyts-date")?.TrimmedText());
        Assert.Null(timelineItem.GetElementByTestId("pqts-date"));
        Assert.Null(timelineItem.GetElementByTestId("qts-date"));
        Assert.Null(timelineItem.GetElementByTestId("has-eyps"));
        Assert.Null(timelineItem.GetElementByTestId("old-eyts-date"));
    }

    [Fact]
    public async Task ProfessionalStatusCreatedEvent_RendersExpectedChangeReasonContent()
    {
        // Arrange
        var awardDate = TimeProvider.Today.AddYears(-2).AddDays(1);
        var route = await ReferenceDataCache.GetRouteWhereAllFieldsApplyAsync();
        var status = RouteToProfessionalStatusStatus.Holds;
        var changeReason = "Another reason";
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

        var professionalStatus = person.Qualifications!.OfType<RouteToProfessionalStatus>().Single();

        var updatedByUser = await TestData.CreateUserAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var timelineItem = await GetChangeHistoryEntryAsync(doc, person.PersonId, ProcessType.RouteToProfessionalStatusCreating);
        var reasonText = timelineItem.GetElementByTestId("reason")?.TrimmedText();
        Assert.Equal(changeReasonDetail, reasonText);
        Assert.Equal($"{filename} (opens in new tab)", timelineItem.GetElementByTestId("uploaded-evidence-link")?.TrimmedText());
    }

    [Fact]
    public async Task ProfessionalStatusUpdatedEvent_RendersExpectedContent()
    {
        // Arrange
        var oldStartDate = TimeProvider.Today.AddYears(-2);
        var oldEndDate = oldStartDate.AddYears(1);
        var oldAwardDate = oldEndDate.AddDays(-1);
        var oldRoute = await ReferenceDataCache.GetRouteWhereAllFieldsApplyAsync();
        var oldStatus = TestDataHelper.GetRouteStatusWhereAllFieldsApply();
        var oldSubject = (await ReferenceDataCache.GetTrainingSubjectsAsync()).Where(s => !s.Name.Contains('\'')).SingleRandom();
        var oldTrainingProvider = (await ReferenceDataCache.GetTrainingProvidersAsync()).Where(s => !s.Name.Contains('\'')).SingleRandom();
        var oldDegreeType = (await ReferenceDataCache.GetDegreeTypesAsync()).SingleRandom();
        var oldCountry = (await ReferenceDataCache.GetTrainingCountriesAsync()).SingleRandom();
        var oldAgeRange = TrainingAgeSpecialismType.FoundationStage;
        var oldExemptFromInduction = false;
        var oldSourceApplicationReference = "TEST-REFERENCE";
        var holdsFrom = oldAwardDate.AddDays(1);
        var startDate = oldStartDate.AddDays(1);
        var endDate = oldEndDate.AddDays(1);
        var route = oldRoute;
        var subject = (await ReferenceDataCache.GetTrainingSubjectsAsync()).Where(x => x.TrainingSubjectId != oldSubject.TrainingSubjectId).Where(s => !s.Name.Contains('\'')).SingleRandom();
        var trainingProvider = (await ReferenceDataCache.GetTrainingProvidersAsync()).Where(x => x.TrainingProviderId != oldTrainingProvider.TrainingProviderId).Where(s => !s.Name.Contains('\'')).SingleRandom();
        var degreeType = (await ReferenceDataCache.GetDegreeTypesAsync()).Where(x => x.DegreeTypeId != oldDegreeType.DegreeTypeId).SingleRandom();
        var country = (await ReferenceDataCache.GetTrainingCountriesAsync()).Where(x => x.CountryId != oldCountry.CountryId).SingleRandom();
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

        var professionalStatus = person.Qualifications!.OfType<RouteToProfessionalStatus>().Single();

        var updatedByUser = await TestData.CreateUserAsync();

        var changes = await RoutesToProfessionalStatusService.UpdateRouteToProfessionalStatusAsync(
            new UpdateRouteToProfessionalStatusOptions
            {
                QualificationId = professionalStatus.QualificationId,
                HoldsFrom = Option.Some<DateOnly?>(holdsFrom),
                TrainingStartDate = Option.Some<DateOnly?>(startDate),
                TrainingEndDate = Option.Some<DateOnly?>(endDate),
                DegreeTypeId = Option.Some<Guid?>(degreeType.DegreeTypeId),
                TrainingSubjectIds = Option.Some<Guid[]>([subject.TrainingSubjectId]),
                TrainingProviderId = Option.Some<Guid?>(trainingProvider.TrainingProviderId),
                TrainingAgeSpecialismType = Option.Some<TrainingAgeSpecialismType?>(ageRange),
                TrainingCountryId = Option.Some<string?>(country.CountryId),
                ExemptFromInduction = Option.Some<bool?>(exemptFromInduction)
            },
            new ProcessContext(
                ProcessType.RouteToProfessionalStatusUpdating,
                TimeProvider.UtcNow,
                updatedByUser.UserId));

        Debug.Assert(changes is not RouteToProfessionalStatusUpdatedEventChanges.None);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var timelineItem = await GetChangeHistoryEntryAsync(doc, person.PersonId, ProcessType.RouteToProfessionalStatusUpdating);
        AssertRaisedBy(timelineItem, updatedByUser.Name);
        Assert.Equal(route.Name, timelineItem.GetElementByTestId("route-name")?.TrimmedText());
        Assert.Null(timelineItem.GetElementByTestId("status"));
        Assert.Equal($"Held since changed from {oldAwardDate.ToString(WebConstants.DateDisplayFormat)} to {holdsFrom.ToString(WebConstants.DateDisplayFormat)}", timelineItem.GetElementByTestId("award-date")?.TrimmedText());
        Assert.Equal($"Start date changed from {oldStartDate.ToString(WebConstants.DateDisplayFormat)} to {startDate.ToString(WebConstants.DateDisplayFormat)}", timelineItem.GetElementByTestId("start-date")!.TrimmedText());
        Assert.Equal($"End date changed from {oldEndDate.ToString(WebConstants.DateDisplayFormat)} to {endDate.ToString(WebConstants.DateDisplayFormat)}", timelineItem.GetElementByTestId("end-date")!.TrimmedText());
        Assert.Equal($"Induction exemption changed from No to Yes", timelineItem.GetElementByTestId("exemption")?.TrimmedText());
        Assert.Equal($"Training provider changed from {oldTrainingProvider.Name} to {trainingProvider.Name}", timelineItem.GetElementByTestId("training-provider")?.TrimmedText());
        Assert.Equal($"Degree type changed from {oldDegreeType.Name} to {degreeType.Name}", timelineItem.GetElementByTestId("degree-type")?.TrimmedText());
        Assert.Equal($"Country changed from {oldCountry.Name} to {country.Name}", timelineItem.GetElementByTestId("country")?.TrimmedText());
        Assert.Equal($"Age range changed from {oldAgeRange.GetDisplayName()} to {ageRange.GetDisplayName()}", timelineItem.GetElementByTestId("age-range-type")?.TrimmedText());
        Assert.Equal($"Subjects changed from {oldSubject.Reference} - {oldSubject.Name} to {subject.Reference} - {subject.Name}", timelineItem.GetElementByTestId("subjects")?.TrimmedText());
        Assert.Equal($"Source application reference: {oldSourceApplicationReference}", timelineItem.GetElementByTestId("source-application-reference")?.TrimmedText());
    }

    [Fact]
    public async Task ProfessionalStatusUpdatedEvent_StatusChangedToHolds_PersonQtsChanged_RendersExpectedContent()
    {
        // Arrange
        var oldStatus = RouteToProfessionalStatusStatus.InTraining;
        var startDate = TimeProvider.Today.AddYears(-2);
        var endDate = startDate.AddYears(1);
        var awardDate = endDate.AddDays(1);
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.ProfessionalStatusType == ProfessionalStatusType.QualifiedTeacherStatus)
            .SingleRandom();
        var status = RouteToProfessionalStatusStatus.Holds;
        var subject = (await ReferenceDataCache.GetTrainingSubjectsAsync()).Where(s => !s.Name.Contains('\'')).SingleRandom();
        var trainingProvider = (await ReferenceDataCache.GetTrainingProvidersAsync()).Where(s => !s.Name.Contains('\'')).SingleRandom();
        var degreeType = (await ReferenceDataCache.GetDegreeTypesAsync()).SingleRandom();
        var country = (await ReferenceDataCache.GetTrainingCountriesAsync()).SingleRandom();
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

        var professionalStatus = person.Qualifications!.OfType<RouteToProfessionalStatus>().Single();

        var updatedByUser = await TestData.CreateUserAsync();

        var changes = await RoutesToProfessionalStatusService.UpdateRouteToProfessionalStatusAsync(
            new UpdateRouteToProfessionalStatusOptions
            {
                QualificationId = professionalStatus.QualificationId,
                Status = Option.Some(status)
            },
            new ProcessContext(
                ProcessType.RouteToProfessionalStatusUpdating,
                TimeProvider.UtcNow,
                updatedByUser.UserId));
        Debug.Assert(changes.HasFlag(RouteToProfessionalStatusUpdatedEventChanges.Status));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var timelineItem = await GetChangeHistoryEntryAsync(doc, person.PersonId, ProcessType.RouteToProfessionalStatusUpdating);
        AssertRaisedBy(timelineItem, updatedByUser.Name);
        Assert.Equal($"Status changed from {oldStatus.GetTitle()} to {status.GetTitle()}", timelineItem.GetElementByTestId("status")?.TrimmedText());
        Assert.Equal($"QTS date changed from None to {awardDate.ToString(WebConstants.DateDisplayFormat)}", timelineItem.GetElementByTestId("qts-date")?.TrimmedText());
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

        Assert.Null(timelineItem.GetElementByTestId("old-status"));
        Assert.Null(timelineItem.GetElementByTestId("old-qts-date"));
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
        var startDate = TimeProvider.Today.AddYears(-2);
        var endDate = startDate.AddYears(1);
        var awardDate = endDate.AddDays(1);
        var route = await ReferenceDataCache.GetRouteWhereAllFieldsApplyAsync();
        var status = RouteToProfessionalStatusStatus.Holds;
        var changeReason = "Another reason";
        var changeReasonDetail = TestData.GenerateLoremIpsum();

        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(q =>
            {
                q.WithRouteType(route.RouteToProfessionalStatusTypeId);
                q.WithStatus(oldStatus);
                q.WithInductionExemption(true);
                q.WithHoldsFrom(awardDate);
            }));

        var professionalStatus = person.Qualifications!.OfType<RouteToProfessionalStatus>().Single();

        var updatedByUser = await TestData.CreateUserAsync();

        var changes = await RoutesToProfessionalStatusService.UpdateRouteToProfessionalStatusAsync(
            new UpdateRouteToProfessionalStatusOptions
            {
                QualificationId = professionalStatus.QualificationId,
                Status = Option.Some(status)
            },
            new ProcessContext(
                ProcessType.RouteToProfessionalStatusUpdating,
                TimeProvider.UtcNow,
                updatedByUser.UserId,
                new ChangeReasonWithDetailsAndEvidence
                {
                    Reason = changeReason,
                    Details = changeReasonDetail,
                    EvidenceFile = null,
                    AdditionalInformation = null
                }));
        Debug.Assert(changes.HasFlag(RouteToProfessionalStatusUpdatedEventChanges.Status));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var timelineItem = await GetChangeHistoryEntryAsync(doc, person.PersonId, ProcessType.RouteToProfessionalStatusUpdating);
        var reasonText = timelineItem.GetElementByTestId("reason")?.TrimmedText();
        Assert.Equal(changeReasonDetail, reasonText);
    }

    [Fact]
    public async Task ProfessionalStatusDeletedEvent_RendersExpectedContent()
    {
        // Arrange
        var startDate = TimeProvider.Today.AddYears(-1);
        var endDate = TimeProvider.Today.AddDays(-1);
        var holdsFrom = endDate.AddDays(1);
        var route = await ReferenceDataCache.GetRouteWhereAllFieldsApplyAsync(ProfessionalStatusType.QualifiedTeacherStatus);
        var status = RouteToProfessionalStatusStatus.Holds;
        var subjects = (await ReferenceDataCache.GetTrainingSubjectsAsync()).Where(s => !s.Name.Contains('\'')).Take(1);
        var trainingProvider = (await ReferenceDataCache.GetTrainingProvidersAsync()).Where(s => !s.Name.Contains('\'')).SingleRandom();
        var degreeType = (await ReferenceDataCache.GetDegreeTypesAsync()).SingleRandom();
        var country = (await ReferenceDataCache.GetTrainingCountriesAsync()).SingleRandom();
        var ageRange = TrainingAgeSpecialismType.FoundationStage;
        var sourceApplicationReference = "TEST-REFERENCE";

        var person = await TestData.CreatePersonAsync(b => b
            .WithRouteToProfessionalStatus(q =>
            {
                q.WithRouteType(route.RouteToProfessionalStatusTypeId);
                q.WithStatus(status);
                q.WithHoldsFrom(TimeProvider.Today);
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

        var professionalStatus = person.Qualifications!.OfType<RouteToProfessionalStatus>().Single();
        var deletedByUser = await TestData.CreateUserAsync();

        await RoutesToProfessionalStatusService.DeleteRouteToProfessionalStatusAsync(
            new DeleteRouteToProfessionalStatusOptions
            {
                QualificationId = professionalStatus.QualificationId
            },
            new ProcessContext(
                ProcessType.RouteToProfessionalStatusDeleting,
                TimeProvider.UtcNow,
                deletedByUser.UserId));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var timelineItem = await GetChangeHistoryEntryAsync(doc, person.PersonId, ProcessType.RouteToProfessionalStatusDeleting);
        AssertRaisedBy(timelineItem, deletedByUser.Name);
        Assert.Null(timelineItem.GetElementByTestId("eyts-date"));
        Assert.Null(timelineItem.GetElementByTestId("pqts-date"));
        Assert.Equal($"QTS date changed from {holdsFrom.ToString(WebConstants.DateDisplayFormat)} to None", timelineItem.GetElementByTestId("qts-date")?.TrimmedText());
        Assert.Null(timelineItem.GetElementByTestId("has-eyps"));
        Assert.Equal(holdsFrom.ToString(WebConstants.DateDisplayFormat), timelineItem.GetElementByTestId("award-date")?.TrimmedText());
        Assert.Equal(status.GetTitle(), timelineItem.GetElementByTestId("status")?.TrimmedText());
        Assert.Equal($"{route.Name} deleted", timelineItem.GetElementByTestId("route-name")?.TrimmedText());
        Assert.Equal(startDate.ToString(WebConstants.DateDisplayFormat), timelineItem.GetElementByTestId("start-date")!.TrimmedText());
        Assert.Equal(endDate.ToString(WebConstants.DateDisplayFormat), timelineItem.GetElementByTestId("end-date")!.TrimmedText());
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

        var professionalStatus = person.Qualifications!.OfType<RouteToProfessionalStatus>().Single();
        var deletedByUser = await TestData.CreateUserAsync();

        await RoutesToProfessionalStatusService.DeleteRouteToProfessionalStatusAsync(
            new DeleteRouteToProfessionalStatusOptions
            {
                QualificationId = professionalStatus.QualificationId
            },
            new ProcessContext(
                ProcessType.RouteToProfessionalStatusDeleting,
                TimeProvider.UtcNow,
                deletedByUser.UserId));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var timelineItem = await GetChangeHistoryEntryAsync(doc, person.PersonId, ProcessType.RouteToProfessionalStatusDeleting);
        Assert.Null(timelineItem.GetElementByTestId("old-qts-date"));
        Assert.Equal($"QTS date changed from {professionalStatus.HoldsFrom?.ToString(WebConstants.DateDisplayFormat)} to None", timelineItem.GetElementByTestId("qts-date")?.TrimmedText());
    }

    [Fact]
    public async Task ProfessionalStatusDeletedEvent_AffectsPersonEyts_RendersExpectedContent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(b => b.WithHoldsRouteToProfessionalStatus(ProfessionalStatusType.EarlyYearsTeacherStatus));

        var professionalStatus = person.Qualifications!.OfType<RouteToProfessionalStatus>().Single();
        var deletedByUser = await TestData.CreateUserAsync();

        await RoutesToProfessionalStatusService.DeleteRouteToProfessionalStatusAsync(
            new DeleteRouteToProfessionalStatusOptions
            {
                QualificationId = professionalStatus.QualificationId
            },
            new ProcessContext(
                ProcessType.RouteToProfessionalStatusDeleting,
                TimeProvider.UtcNow,
                deletedByUser.UserId));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var timelineItem = await GetChangeHistoryEntryAsync(doc, person.PersonId, ProcessType.RouteToProfessionalStatusDeleting);
        Assert.Null(timelineItem.GetElementByTestId("old-eyts-date"));
        Assert.Equal($"EYTS date changed from {professionalStatus.HoldsFrom?.ToString(WebConstants.DateDisplayFormat)} to None", timelineItem.GetElementByTestId("eyts-date")?.TrimmedText());
    }

    [Fact]
    public async Task ProfessionalStatusDeletedEvent_AffectsPersonPqts_RendersExpectedContent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(b => b.WithHoldsRouteToProfessionalStatus(ProfessionalStatusType.PartialQualifiedTeacherStatus));

        var professionalStatus = person.Qualifications!.OfType<RouteToProfessionalStatus>().Single();
        var deletedByUser = await TestData.CreateUserAsync();

        await RoutesToProfessionalStatusService.DeleteRouteToProfessionalStatusAsync(
            new DeleteRouteToProfessionalStatusOptions
            {
                QualificationId = professionalStatus.QualificationId
            },
            new ProcessContext(
                ProcessType.RouteToProfessionalStatusDeleting,
                TimeProvider.UtcNow,
                deletedByUser.UserId));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var timelineItem = await GetChangeHistoryEntryAsync(doc, person.PersonId, ProcessType.RouteToProfessionalStatusDeleting);
        Assert.Null(timelineItem.GetElementByTestId("old-pqts-date"));
        Assert.Equal($"PQTS date changed from {professionalStatus.HoldsFrom?.ToString(WebConstants.DateDisplayFormat)} to None", timelineItem.GetElementByTestId("pqts-date")?.TrimmedText());
    }

    [Fact]
    public async Task ProfessionalStatusDeletedEvent_AffectsPersonEyps_RendersExpectedContent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(b => b.WithHoldsRouteToProfessionalStatus(ProfessionalStatusType.EarlyYearsProfessionalStatus));

        var professionalStatus = person.Qualifications!.OfType<RouteToProfessionalStatus>().Single();
        var deletedByUser = await TestData.CreateUserAsync();

        await RoutesToProfessionalStatusService.DeleteRouteToProfessionalStatusAsync(
            new DeleteRouteToProfessionalStatusOptions
            {
                QualificationId = professionalStatus.QualificationId
            },
            new ProcessContext(
                ProcessType.RouteToProfessionalStatusDeleting,
                TimeProvider.UtcNow,
                deletedByUser.UserId));

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var timelineItem = await GetChangeHistoryEntryAsync(doc, person.PersonId, ProcessType.RouteToProfessionalStatusDeleting);
        Assert.Equal("Person has EYPS changed from Yes to No", timelineItem.GetElementByTestId("has-eyps")?.TrimmedText());
        Assert.Null(timelineItem.GetElementByTestId("old-has-eyps"));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ProfessionalStatusMigratedEvent_RendersExpectedContent(bool populateOptional)
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();
        var startDate = TimeProvider.Today.AddYears(-1);
        var endDate = TimeProvider.Today.AddDays(-1);
        var awardDate = endDate.AddDays(1);
        var route = await ReferenceDataCache.GetRouteWhereAllFieldsApplyAsync();
        var status = populateOptional ? RouteToProfessionalStatusStatus.Holds : RouteToProfessionalStatusStatus.InTraining;
        var subjects = (await ReferenceDataCache.GetTrainingSubjectsAsync()).Where(s => !s.Name.Contains('\'')).Take(1);
        var trainingProvider = (await ReferenceDataCache.GetTrainingProvidersAsync()).Where(s => !s.Name.Contains('\'')).SingleRandom();
        var degreeType = (await ReferenceDataCache.GetDegreeTypesAsync()).SingleRandom();
        var country = (await ReferenceDataCache.GetTrainingCountriesAsync()).SingleRandom();
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
        var dqtAgeRangeFrom = "10";
        var dqtAgeRangeTo = "16";
        var programmeType = "AssessmentOnlyRoute";
        var ittResult = "Pass";
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
                ProgrammeType = programmeType,
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
            PersonId = person.PersonId,
            RouteToProfessionalStatus = routeToProfessionalStatus,
            DqtQtsRegistration = dqtQtsRegistration,
            DqtInitialTeacherTraining = dqtInitialTeacherTraining,
            DqtQtlsDate = populateOptional ? qtlsDate : null,
            DqtQtlsDateHasBeenSet = populateOptional ? true : null
        };

        await TestData.CreateProcessAsync(
            ProcessType.RouteToProfessionalStatusMigratingFromDqt,
            createdByUser.UserId,
            changeReason: null,
            migratedEvent);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/change-history");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var timelineItem = await GetChangeHistoryEntryAsync(doc, person.PersonId, ProcessType.RouteToProfessionalStatusMigratingFromDqt);
        AssertRaisedBy(timelineItem, createdByUser.Name);
        Assert.Equal(populateOptional ? awardDate.ToString(WebConstants.DateDisplayFormat) : WebConstants.EmptyFallbackContent, timelineItem.GetElementByTestId("award-date")?.TrimmedText());
        Assert.Equal(status.GetTitle(), timelineItem.GetElementByTestId("status")?.TrimmedText());
        Assert.Equal($"{route.Name} migrated", timelineItem.GetElementByTestId("route-name")?.TrimmedText());
        Assert.Equal(populateOptional ? startDate.ToString(WebConstants.DateDisplayFormat) : WebConstants.EmptyFallbackContent, timelineItem.GetElementByTestId("start-date")!.TrimmedText());
        Assert.Equal(populateOptional ? endDate.ToString(WebConstants.DateDisplayFormat) : WebConstants.EmptyFallbackContent, timelineItem.GetElementByTestId("end-date")!.TrimmedText());
        Assert.Equal(populateOptional ? "Yes" : WebConstants.EmptyFallbackContent, timelineItem.GetElementByTestId("exemption")?.TrimmedText());
        Assert.Equal(populateOptional ? trainingProvider.Name : WebConstants.EmptyFallbackContent, timelineItem.GetElementByTestId("training-provider")?.TrimmedText());
        Assert.Equal(populateOptional ? degreeType.Name : WebConstants.EmptyFallbackContent, timelineItem.GetElementByTestId("degree-type")?.TrimmedText());
        Assert.Equal(populateOptional ? country.Name : WebConstants.EmptyFallbackContent, timelineItem.GetElementByTestId("country")?.TrimmedText());
        Assert.Equal(populateOptional ? $"From {ageRangeFrom} to {ageRangeTo}" : WebConstants.EmptyFallbackContent, timelineItem.GetElementByTestId("age-range")?.TrimmedText());
        Assert.Equal(populateOptional ? $"{subjects.Single().Reference} - {subjects.Single().Name}" : WebConstants.EmptyFallbackContent, timelineItem.GetElementByTestId("subjects")?.TrimmedText());
        Assert.Equal(populateOptional ? sourceApplicationReference : WebConstants.EmptyFallbackContent, timelineItem.GetElementByTestId("source-application-reference")?.TrimmedText());
        Assert.Equal(populateOptional ? dqtQtsRegistration!.QtsRegistrationId.ToString() : WebConstants.EmptyFallbackContent, timelineItem.GetElementByTestId("qts-registration-id")?.TrimmedText());
        Assert.Equal(populateOptional ? dqtQtsRegistration!.TeacherStatusName : WebConstants.EmptyFallbackContent, timelineItem.GetElementByTestId("teacher-status-name")?.TrimmedText());
        Assert.Equal(populateOptional ? dqtQtsRegistration!.TeacherStatusValue : WebConstants.EmptyFallbackContent, timelineItem.GetElementByTestId("teacher-status-value")?.TrimmedText());
        Assert.Equal(populateOptional ? dqtQtsRegistration!.EarlyYearsStatusName : WebConstants.EmptyFallbackContent, timelineItem.GetElementByTestId("early-years-status-name")?.TrimmedText());
        Assert.Equal(populateOptional ? dqtQtsRegistration!.EarlyYearsStatusValue : WebConstants.EmptyFallbackContent, timelineItem.GetElementByTestId("early-years-status-value")?.TrimmedText());
        Assert.Equal(populateOptional ? qtsDate.ToString(WebConstants.DateDisplayFormat) : WebConstants.EmptyFallbackContent, timelineItem.GetElementByTestId("qts-date")?.TrimmedText());
        Assert.Equal(populateOptional ? eytsDate.ToString(WebConstants.DateDisplayFormat) : WebConstants.EmptyFallbackContent, timelineItem.GetElementByTestId("eyts-date")?.TrimmedText());
        Assert.Equal(populateOptional ? pqtsDate.ToString(WebConstants.DateDisplayFormat) : WebConstants.EmptyFallbackContent, timelineItem.GetElementByTestId("pqts-date")?.TrimmedText());
        Assert.Equal(populateOptional ? qtlsDate.ToString(WebConstants.DateDisplayFormat) : WebConstants.EmptyFallbackContent, timelineItem.GetElementByTestId("qtls-date")?.TrimmedText());
        Assert.Equal(populateOptional ? dqtInitialTeacherTraining!.InitialTeacherTrainingId.ToString() : WebConstants.EmptyFallbackContent, timelineItem.GetElementByTestId("itt-id")?.TrimmedText());
        Assert.Equal(populateOptional ? dqtInitialTeacherTraining?.SlugId : WebConstants.EmptyFallbackContent, timelineItem.GetElementByTestId("slug-id")?.TrimmedText());
        Assert.Equal(populateOptional ? dqtInitialTeacherTraining?.ProgrammeType : WebConstants.EmptyFallbackContent, timelineItem.GetElementByTestId("programme-type")?.TrimmedText());
        Assert.Equal(populateOptional ? startDate.ToString(WebConstants.DateDisplayFormat) : WebConstants.EmptyFallbackContent, timelineItem.GetElementByTestId("programme-start-date")?.TrimmedText());
        Assert.Equal(populateOptional ? endDate.ToString(WebConstants.DateDisplayFormat) : WebConstants.EmptyFallbackContent, timelineItem.GetElementByTestId("programme-end-date")?.TrimmedText());
        Assert.Equal(populateOptional ? ittResult.ToString() : WebConstants.EmptyFallbackContent, timelineItem.GetElementByTestId("itt-result")?.TrimmedText());
        Assert.Equal(populateOptional ? ittQualification.dfeta_name : WebConstants.EmptyFallbackContent, timelineItem.GetElementByTestId("qualification-name")?.TrimmedText());
        Assert.Equal(populateOptional ? ittQualification.dfeta_Value : WebConstants.EmptyFallbackContent, timelineItem.GetElementByTestId("qualification-value")?.TrimmedText());
        Assert.Equal(populateOptional ? ittProvider!.Id.ToString() : WebConstants.EmptyFallbackContent, timelineItem.GetElementByTestId("provider-id")?.TrimmedText());
        Assert.Equal(populateOptional ? ittProvider!.Name : WebConstants.EmptyFallbackContent, timelineItem.GetElementByTestId("provider-name")?.TrimmedText());
        Assert.Equal(populateOptional ? ittProvider!.dfeta_UKPRN : WebConstants.EmptyFallbackContent, timelineItem.GetElementByTestId("provider-ukprn")?.TrimmedText());
        Assert.Equal(populateOptional ? dqtCountry!.dfeta_name : WebConstants.EmptyFallbackContent, timelineItem.GetElementByTestId("country-name")?.TrimmedText());
        Assert.Equal(populateOptional ? dqtCountry!.dfeta_Value : WebConstants.EmptyFallbackContent, timelineItem.GetElementByTestId("country-value")?.TrimmedText());
        Assert.Equal(populateOptional ? dqtAgeRangeFrom.ToString() : WebConstants.EmptyFallbackContent, timelineItem.GetElementByTestId("dqt-age-range-from")?.TrimmedText());
        Assert.Equal(populateOptional ? dqtAgeRangeTo.ToString() : WebConstants.EmptyFallbackContent, timelineItem.GetElementByTestId("dqt-age-range-to")?.TrimmedText());
        Assert.Equal(populateOptional ? $"{ittSubject1!.dfeta_Value} - {ittSubject1!.dfeta_name}{ittSubject2!.dfeta_Value} - {ittSubject2!.dfeta_name}{ittSubject3!.dfeta_Value} - {ittSubject3!.dfeta_name}" : WebConstants.EmptyFallbackContent, timelineItem.GetElementByTestId("dqt-subjects")?.TrimmedText());
    }

    private async Task<IElement> GetChangeHistoryEntryAsync(IHtmlDocument doc, Guid personId, ProcessType processType)
    {
        var processId = await WithDbContextAsync(dbContext => dbContext.Processes
            .Where(p => p.PersonIds.Contains(personId) && p.ProcessType == processType)
            .Select(p => p.ProcessId)
            .SingleAsync());

        var entry = doc.GetElementByDataAttribute("data-process-id", processId.ToString());
        Assert.NotNull(entry);
        return entry;
    }

    private static void AssertRaisedBy(IElement changeHistoryEntry, string expectedUserName)
    {
        var date = changeHistoryEntry.GetElementsByClassName("moj-timeline__date").SingleOrDefault();
        Assert.Contains($"By {expectedUserName} on", date?.TrimmedText().ReplaceLineEndings(" "));
    }
}
