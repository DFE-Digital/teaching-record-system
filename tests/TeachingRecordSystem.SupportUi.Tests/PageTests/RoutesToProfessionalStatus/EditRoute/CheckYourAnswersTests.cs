using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.ChangeReasons;
using TeachingRecordSystem.SupportUi.Pages.Persons.PersonDetail.SetStatus;
using TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus;
using TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.RoutesToProfessionalStatus.EditRoute;

public class CheckYourAnswersTests(HostFixture hostFixture) : EditRouteTestBase(hostFixture)
{
    private static string _countryCode = "AG";

    [Fact]
    public async Task Post_Cancel_DeletesJourneyAndRedirectsToQualifications()
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).Where(r => r.Name == "Northern Irish Recognition").Single();
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(RouteToProfessionalStatusStatus.Deferred)));
        var qualificationId = person.Qualifications!.OfType<RouteToProfessionalStatus>().First().QualificationId;
        var editRouteState = new EditRouteState
        {
            RouteToProfessionalStatusId = route.RouteToProfessionalStatusTypeId,
            Status = RouteToProfessionalStatusStatus.Deferred,
            CurrentStatus = RouteToProfessionalStatusStatus.Deferred,
            TrainingCountryId = _countryCode,
            ChangeReason = ChangeReasonOption.AnotherReason,
            ChangeReasonDetail = CreateChangeReasonDetail()
        };

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            editRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Post, $"/routes/{qualificationId}/edit/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder().Add("Cancel", bool.TrueString)
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        var location = response.Headers.Location?.OriginalString;
        Assert.Equal($"/persons/{person.PersonId}/qualifications", location);
        Assert.Null(GetJourneyInstanceState(journeyInstance));
    }

    [Theory]
    [InlineData("Apply for Qualified Teacher Status in England", RouteToProfessionalStatusStatus.Holds, EditRoutePage.StartAndEndDate, null)]
    [InlineData("Apply for Qualified Teacher Status in England", RouteToProfessionalStatusStatus.Holds, EditRoutePage.HoldsFrom, "holds-from")]
    [InlineData("Apply for Qualified Teacher Status in England", RouteToProfessionalStatusStatus.Holds, EditRoutePage.InductionExemption, "induction-exemption")]
    [InlineData("Apply for Qualified Teacher Status in England", RouteToProfessionalStatusStatus.Holds, EditRoutePage.TrainingProvider, null)]
    [InlineData("Apply for Qualified Teacher Status in England", RouteToProfessionalStatusStatus.Holds, EditRoutePage.DegreeType, null)]
    [InlineData("Apply for Qualified Teacher Status in England", RouteToProfessionalStatusStatus.Holds, EditRoutePage.Country, "country")]
    [InlineData("Apply for Qualified Teacher Status in England", RouteToProfessionalStatusStatus.Holds, EditRoutePage.AgeRangeSpecialism, null)]
    [InlineData("Apply for Qualified Teacher Status in England", RouteToProfessionalStatusStatus.Holds, EditRoutePage.SubjectSpecialisms, null)]
    [InlineData("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.InTraining, EditRoutePage.StartAndEndDate, null)]
    [InlineData("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.InTraining, EditRoutePage.HoldsFrom, null)]
    [InlineData("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.InTraining, EditRoutePage.InductionExemption, null)]
    [InlineData("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.InTraining, EditRoutePage.TrainingProvider, "training-provider")]
    [InlineData("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.InTraining, EditRoutePage.DegreeType, "degree-type")]
    [InlineData("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.InTraining, EditRoutePage.Country, "country")]
    [InlineData("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.InTraining, EditRoutePage.AgeRangeSpecialism, null)]
    [InlineData("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.InTraining, EditRoutePage.SubjectSpecialisms, null)]
    [InlineData("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.Holds, EditRoutePage.StartAndEndDate, null)]
    [InlineData("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.Holds, EditRoutePage.HoldsFrom, "holds-from")]
    [InlineData("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.Holds, EditRoutePage.InductionExemption, null)]
    [InlineData("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.Holds, EditRoutePage.TrainingProvider, "training-provider")]
    [InlineData("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.Holds, EditRoutePage.DegreeType, "degree-type")]
    [InlineData("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.Holds, EditRoutePage.Country, "country")]
    [InlineData("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.Holds, EditRoutePage.AgeRangeSpecialism, null)]
    [InlineData("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.Holds, EditRoutePage.SubjectSpecialisms, null)]
    [InlineData("Early Years Teacher Degree Apprenticeship", RouteToProfessionalStatusStatus.Holds, EditRoutePage.StartAndEndDate, null)]
    [InlineData("Early Years Teacher Degree Apprenticeship", RouteToProfessionalStatusStatus.Holds, EditRoutePage.HoldsFrom, "holds-from")]
    [InlineData("Early Years Teacher Degree Apprenticeship", RouteToProfessionalStatusStatus.Holds, EditRoutePage.InductionExemption, null)]
    [InlineData("Early Years Teacher Degree Apprenticeship", RouteToProfessionalStatusStatus.Holds, EditRoutePage.TrainingProvider, null)]
    [InlineData("Early Years Teacher Degree Apprenticeship", RouteToProfessionalStatusStatus.Holds, EditRoutePage.DegreeType, "degree-type")]
    [InlineData("Early Years Teacher Degree Apprenticeship", RouteToProfessionalStatusStatus.Holds, EditRoutePage.Country, "country")]
    [InlineData("Early Years Teacher Degree Apprenticeship", RouteToProfessionalStatusStatus.Holds, EditRoutePage.AgeRangeSpecialism, "age-range")]
    [InlineData("Early Years Teacher Degree Apprenticeship", RouteToProfessionalStatusStatus.Holds, EditRoutePage.SubjectSpecialisms, "subjects")]
    public async Task Get_IncompleteJourney_RedirectsToExpectedPage(string routeName, RouteToProfessionalStatusStatus status, EditRoutePage incompletePage, string? expectedRedirectPage)
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).Where(r => r.Name == routeName).Single();
        var startDate = TimeProvider.Today.AddYears(-1);
        var endDate = TimeProvider.Today.AddDays(-1);
        var holdsFrom = endDate.AddDays(1);
        var subjects = (await ReferenceDataCache.GetTrainingSubjectsAsync()).Where(s => !s.Name.Contains('\'')).Take(1);
        var trainingProvider = (await ReferenceDataCache.GetTrainingProvidersAsync()).Where(s => !s.Name.Contains('\'')).SingleRandom();
        var degreeType = (await ReferenceDataCache.GetDegreeTypesAsync()).SingleRandom();
        var country = (await ReferenceDataCache.GetTrainingCountriesAsync()).SingleRandom();

        var editRouteState = new EditRouteState
        {
            RouteToProfessionalStatusId = route.RouteToProfessionalStatusTypeId,
            Status = status,
            CurrentStatus = status,
            TrainingStartDate = incompletePage != EditRoutePage.StartAndEndDate ? startDate : null,
            TrainingEndDate = incompletePage != EditRoutePage.StartAndEndDate ? endDate : null,
            HoldsFrom = incompletePage != EditRoutePage.HoldsFrom ? holdsFrom : null,
            TrainingProviderId = incompletePage != EditRoutePage.TrainingProvider ? trainingProvider.TrainingProviderId : null,
            TrainingCountryId = incompletePage != EditRoutePage.Country ? country.CountryId : null,
            TrainingSubjectIds = incompletePage != EditRoutePage.SubjectSpecialisms ? subjects.Select(s => s.TrainingSubjectId).ToArray() : [],
            TrainingAgeSpecialismType = incompletePage != EditRoutePage.AgeRangeSpecialism ? TrainingAgeSpecialismType.FoundationStage : null,
            DegreeTypeId = incompletePage != EditRoutePage.DegreeType ? degreeType.DegreeTypeId : null,
            IsExemptFromInduction = incompletePage != EditRoutePage.InductionExemption ? true : null,
            ChangeReason = incompletePage != EditRoutePage.ChangeReason ? ChangeReasonOption.AnotherReason : null,
            ChangeReasonDetail = incompletePage != EditRoutePage.ChangeReason ? CreateChangeReasonDetail() : new()
        };
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
            .WithRouteType(route.RouteToProfessionalStatusTypeId)
            .WithStatus(RouteToProfessionalStatusStatus.Deferred)));
        var qualificationId = person.Qualifications!.OfType<RouteToProfessionalStatus>().First().QualificationId;

        var journeyInstance = await CreateJourneyInstanceAsync(qualificationId, editRouteState);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/routes/{qualificationId}/edit/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        if (expectedRedirectPage != null)
        {
            Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
            var location = response.Headers.Location?.OriginalString;
            var checkAnswersUrl = GetCheckAnswersReturnUrl(journeyInstance, qualificationId);
            Assert.Equal(
                $"/routes/{qualificationId}/edit/{expectedRedirectPage}?returnUrl={Uri.EscapeDataString(checkAnswersUrl)}&{journeyInstance.GetUniqueIdQueryParameter()}",
                location);
        }
        else
        {
            Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);
        }
    }

    [Fact]
    public async Task Post_RedirectsToExpectedPage()
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).Where(r => r.Name == "Northern Irish Recognition").Single();
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(RouteToProfessionalStatusStatus.Deferred)));
        var qualificationId = person.Qualifications!.OfType<RouteToProfessionalStatus>().First().QualificationId;
        var editRouteState = new EditRouteState
        {
            RouteToProfessionalStatusId = route.RouteToProfessionalStatusTypeId,
            Status = RouteToProfessionalStatusStatus.Deferred,
            CurrentStatus = RouteToProfessionalStatusStatus.Deferred,
            TrainingCountryId = _countryCode,
            ChangeReason = ChangeReasonOption.AnotherReason,
            ChangeReasonDetail = CreateChangeReasonDetail()
        };

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            editRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Post, $"/routes/{qualificationId}/edit/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

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
        var startDate = TimeProvider.Today.AddYears(-1);
        var endDate = TimeProvider.Today.AddDays(-1);
        var holdsFrom = endDate.AddDays(1);
        var route = await ReferenceDataCache.GetRouteWhereAllFieldsApplyAsync();
        var status = TestDataHelper.GetRouteStatusWhereAllFieldsApply();
        var subjects = (await ReferenceDataCache.GetTrainingSubjectsAsync()).Where(s => !s.Name.Contains('\'')).Take(1);
        var trainingProvider = (await ReferenceDataCache.GetTrainingProvidersAsync()).Where(s => !s.Name.Contains('\'')).SingleRandom();
        var degreeType = (await ReferenceDataCache.GetDegreeTypesAsync()).SingleRandom();
        var country = (await ReferenceDataCache.GetTrainingCountriesAsync()).SingleRandom();
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(RouteToProfessionalStatusStatus.InTraining)));

        var qualificationId = person.Qualifications!.OfType<RouteToProfessionalStatus>().First().QualificationId;
        var editRouteState = new EditRouteState
        {
            RouteToProfessionalStatusId = route.RouteToProfessionalStatusTypeId,
            Status = status,
            CurrentStatus = status,
            TrainingStartDate = startDate,
            TrainingEndDate = endDate,
            HoldsFrom = holdsFrom,
            TrainingProviderId = trainingProvider.TrainingProviderId,
            TrainingCountryId = country.CountryId,
            TrainingSubjectIds = subjects.Select(s => s.TrainingSubjectId).ToArray(),
            TrainingAgeSpecialismType = TrainingAgeSpecialismType.FoundationStage,
            DegreeTypeId = degreeType.DegreeTypeId,
            IsExemptFromInduction = true,
            ChangeReason = ChangeReasonOption.AnotherReason,
            ChangeReasonDetail = CreateChangeReasonDetail()
        };

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            editRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Get, $"/routes/{qualificationId}/edit/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertSummaryListRowValueContentMatches("Route", route.Name);
        doc.AssertSummaryListRowValueContentMatches("Status", status.GetTitle());
        doc.AssertSummaryListRowValueContentMatches("Start date", startDate.ToString(WebConstants.DateDisplayFormat));
        doc.AssertSummaryListRowValueContentMatches("End date", endDate.ToString(WebConstants.DateDisplayFormat));
        doc.AssertSummaryListRowValueContentMatches("Held since", holdsFrom.ToString(WebConstants.DateDisplayFormat));
        doc.AssertSummaryListRowValueContentMatches("Training provider", trainingProvider.Name);
        doc.AssertSummaryListRowValueContentMatches("Degree type", degreeType.Name);
        doc.AssertSummaryListRowValueContentMatches("Country of training", country.Name);
        doc.AssertSummaryListRowValueContentMatches("Age range", "Foundation stage");
        doc.AssertSummaryListRowValueContentMatches("Subjects", subjects.Select(s => $"{s.Reference} - {s.Name}"));
    }

    [Fact]
    public async Task Get_ShowsExemptionAnswer_AsExpected()
    {
        // Arrange
        var startDate = TimeProvider.Today.AddYears(-1);
        var endDate = TimeProvider.Today;
        var country = (await ReferenceDataCache.GetTrainingCountriesAsync())
            .SingleRandom();
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.InductionExemptionRequired == FieldRequirement.Mandatory && r.TrainingProviderRequired == FieldRequirement.NotApplicable)
            .SingleRandom();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.Value.GetInductionExemptionRequirement() == FieldRequirement.Mandatory)
            .SingleRandom();
        var exemptionReason = (await ReferenceDataCache.GetInductionExemptionReasonsAsync()).SingleRandom();

        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(status.Value)
                .WithHoldsFrom(endDate)));

        var qualificationId = person.Qualifications!.OfType<RouteToProfessionalStatus>().First().QualificationId;
        var editRouteState = new EditRouteState
        {
            RouteToProfessionalStatusId = route.RouteToProfessionalStatusTypeId,
            Status = status.Value,
            CurrentStatus = status.Value,
            TrainingStartDate = startDate,
            TrainingEndDate = endDate,
            HoldsFrom = endDate,
            TrainingCountryId = country.CountryId,
            IsExemptFromInduction = true,
            ChangeReason = ChangeReasonOption.AnotherReason,
            ChangeReasonDetail = CreateChangeReasonDetail()
        };

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            editRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Get, $"/routes/{qualificationId}/edit/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertSummaryListRowValueContentMatches("Held since", endDate.ToString(WebConstants.DateDisplayFormat));
        doc.AssertSummaryListRowValueContentMatches("Induction exemption", "Yes");
    }

    [Fact]
    public async Task Get_ShowsOptionalAnswersNotPopulated_AsExpected()
    {
        // Arrange
        var startDate = TimeProvider.Today.AddYears(-1);
        var endDate = TimeProvider.Today;
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).Where(r => r.Name == "Postgraduate Teaching Apprenticeship").Single();
        var trainingProvider = (await ReferenceDataCache.GetTrainingProvidersAsync()).Where(s => !s.Name.Contains('\''))
            .SingleRandom();
        var degreeType = (await ReferenceDataCache.GetDegreeTypesAsync())
            .SingleRandom();
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(RouteToProfessionalStatusStatus.InTraining)));

        var qualificationId = person.Qualifications!.OfType<RouteToProfessionalStatus>().First().QualificationId;
        var editRouteState = new EditRouteState
        {
            RouteToProfessionalStatusId = route.RouteToProfessionalStatusTypeId,
            Status = RouteToProfessionalStatusStatus.InTraining,
            CurrentStatus = RouteToProfessionalStatusStatus.InTraining,
            TrainingStartDate = startDate,
            TrainingEndDate = endDate,
            TrainingCountryId = _countryCode,
            TrainingProviderId = trainingProvider.TrainingProviderId,
            DegreeTypeId = degreeType.DegreeTypeId,
            ChangeReason = ChangeReasonOption.AnotherReason,
            ChangeReasonDetail = CreateChangeReasonDetail()
        };

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            editRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Get, $"/routes/{qualificationId}/edit/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertSummaryListRowValueContentMatches("Route", route.Name);
        doc.AssertSummaryListRowValueContentMatches("Status", "In training");
        doc.AssertSummaryListRowValueContentMatches("Start date", startDate.ToString(WebConstants.DateDisplayFormat));
        doc.AssertSummaryListRowValueContentMatches("End date", endDate.ToString(WebConstants.DateDisplayFormat));
        doc.AssertSummaryListRowValueContentMatches("Training provider", trainingProvider.Name);
        doc.AssertSummaryListRowValueContentMatches("Age range", "Not provided");
        doc.AssertSummaryListRowValueContentMatches("Subjects", "Not provided");
    }

    [Fact]
    public async Task Get_ShowsChangeReasonAnswers_AsExpected()
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).Where(r => r.Name == "Northern Irish Recognition").Single();
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(RouteToProfessionalStatusStatus.Deferred)));
        var qualificationId = person.Qualifications!.OfType<RouteToProfessionalStatus>().First().QualificationId;
        var editRouteState = new EditRouteState
        {
            RouteToProfessionalStatusId = route.RouteToProfessionalStatusTypeId,
            Status = RouteToProfessionalStatusStatus.Deferred,
            CurrentStatus = RouteToProfessionalStatusStatus.Deferred,
            TrainingCountryId = _countryCode,
            ChangeReason = ChangeReasonOption.AnotherReason,
            ChangeReasonDetail = new()
            {
                ChangeReasonDetail = "Some reason",
                ProvideAdditionalInformation = ProvideMoreInformationOption.Yes,
                AdditionalInformation = "Some additional information",
                Evidence = new() { UploadEvidence = false }
            }
        };

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            editRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Get, $"/routes/{qualificationId}/edit/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertSummaryListRowValueContentMatches("Reason", editRouteState.ChangeReason!.GetDisplayName()!);
        doc.AssertSummaryListRowValueContentMatches("Additional information", editRouteState.ChangeReasonDetail!.AdditionalInformation!);
        doc.AssertSummaryListRowValueContentMatches("Reason details", editRouteState.ChangeReasonDetail!.ChangeReasonDetail!);
        doc.AssertSummaryListRowValueContentMatches("Evidence", "Not provided");
    }

    [Fact]
    public async Task Post_Confirm_UpdatesProfessionalStatusCreatesEventCompletesJourneyAndRedirectsWithFlashMessage()
    {
        // Arrange
        var route = await ReferenceDataCache.GetRouteWhereAllFieldsApplyAsync();
        var status = TestDataHelper.GetRouteStatusWhereAllFieldsApply();
        var trainingProvider = (await ReferenceDataCache.GetTrainingProvidersAsync()).Where(s => !s.Name.Contains('\'')).First();
        var subjects = (await ReferenceDataCache.GetTrainingSubjectsAsync()).Where(s => !s.Name.Contains('\'')).Take(1);
        var country = (await ReferenceDataCache.GetTrainingCountriesAsync()).SingleRandom();
        var degreeType = (await ReferenceDataCache.GetDegreeTypesAsync()).SingleRandom();
        var ageRange = TrainingAgeSpecialismType.KeyStage3;

        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(RouteToProfessionalStatusStatus.Deferred)));
        EventObserver.Clear();

        var qualificationId = person.Qualifications!.OfType<RouteToProfessionalStatus>().First().QualificationId;
        var editRouteState = new EditRouteState
        {
            RouteToProfessionalStatusId = route.RouteToProfessionalStatusTypeId,
            Status = status,
            CurrentStatus = status,
            TrainingStartDate = TimeProvider.Today.AddYears(-1),
            TrainingEndDate = TimeProvider.Today.AddDays(-1),
            HoldsFrom = TimeProvider.Today,
            TrainingProviderId = trainingProvider.TrainingProviderId,
            TrainingSubjectIds = subjects.Select(s => s.TrainingSubjectId).ToArray(),
            TrainingCountryId = country.CountryId,
            TrainingAgeSpecialismType = ageRange,
            IsExemptFromInduction = true,
            DegreeTypeId = degreeType.DegreeTypeId,
            ChangeReason = ChangeReasonOption.AnotherReason,
            ChangeReasonDetail = CreateChangeReasonDetail()
        };

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            editRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Post, $"/routes/{qualificationId}/edit/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        var state = GetJourneyInstanceState(journeyInstance)!;

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var redirectResponse = await response.FollowRedirectAsync(HttpClient);
        var redirectDoc = await redirectResponse.GetDocumentAsync();
        AssertEx.HtmlDocumentHasFlashNotificationBanner(redirectDoc, "Route to professional status updated");

        await WithDbContextAsync(async dbContext =>
        {
            var updatedProfessionalStatusRecord = await dbContext.RouteToProfessionalStatuses.FirstOrDefaultAsync(q => q.QualificationId == qualificationId);
            Assert.Equal(state.IsExemptFromInduction, updatedProfessionalStatusRecord!.ExemptFromInduction);
            Assert.Equal(state.Status, updatedProfessionalStatusRecord!.Status);
            Assert.Equal(state.RouteToProfessionalStatusId, updatedProfessionalStatusRecord!.RouteToProfessionalStatusTypeId);
            Assert.Equal(state.TrainingStartDate, updatedProfessionalStatusRecord!.TrainingStartDate);
            Assert.Equal(state.TrainingEndDate, updatedProfessionalStatusRecord!.TrainingEndDate);
            Assert.Equal(state.HoldsFrom, updatedProfessionalStatusRecord!.HoldsFrom);
            Assert.Equal(state.TrainingProviderId, updatedProfessionalStatusRecord!.TrainingProviderId);
            Assert.Equal(state.TrainingCountryId, updatedProfessionalStatusRecord!.TrainingCountryId);
            Assert.Equal(state.TrainingAgeSpecialismType, updatedProfessionalStatusRecord!.TrainingAgeSpecialismType);
            Assert.Equal(state.TrainingAgeSpecialismRangeFrom, updatedProfessionalStatusRecord!.TrainingAgeSpecialismRangeFrom);
            Assert.Equal(state.TrainingAgeSpecialismRangeTo, updatedProfessionalStatusRecord!.TrainingAgeSpecialismRangeTo);
            Assert.Equal(state.TrainingSubjectIds, updatedProfessionalStatusRecord!.TrainingSubjectIds);
            Assert.Equal(state.DegreeTypeId, updatedProfessionalStatusRecord!.DegreeTypeId);
        });

        var raisedBy = GetCurrentUserId();

        Events.AssertProcessesCreated(p =>
        {
            Assert.Equal(ProcessType.RouteToProfessionalStatusUpdating, p.ProcessContext.ProcessType);
            var changeReason = p.ProcessContext.Process.ChangeReason as ChangeReasonWithDetailsAndEvidence;

            p.AssertProcessHasEvents<RouteToProfessionalStatusUpdatedEvent>(actualUpdatedEvent =>
            {
                Assert.Equal(person.PersonId, actualUpdatedEvent.PersonId);
                Assert.Equal(state.Status, actualUpdatedEvent.RouteToProfessionalStatus.Status);
                Assert.Equal(state.RouteToProfessionalStatusId, actualUpdatedEvent.RouteToProfessionalStatus.RouteToProfessionalStatusTypeId);
                Assert.Equal(state.TrainingStartDate, actualUpdatedEvent.RouteToProfessionalStatus.TrainingStartDate);
                Assert.Equal(state.TrainingEndDate, actualUpdatedEvent.RouteToProfessionalStatus.TrainingEndDate);
                Assert.Equal(state.HoldsFrom, actualUpdatedEvent.RouteToProfessionalStatus.HoldsFrom);
                Assert.Equal(state.TrainingAgeSpecialismType, actualUpdatedEvent.RouteToProfessionalStatus.TrainingAgeSpecialismType);
                Assert.Equal(state.TrainingAgeSpecialismRangeFrom, actualUpdatedEvent.RouteToProfessionalStatus.TrainingAgeSpecialismRangeFrom);
                Assert.Equal(state.TrainingAgeSpecialismRangeTo, actualUpdatedEvent.RouteToProfessionalStatus.TrainingAgeSpecialismRangeTo);
                Assert.Equal(state.TrainingSubjectIds, actualUpdatedEvent.RouteToProfessionalStatus.TrainingSubjectIds);
                Assert.Equal(state.IsExemptFromInduction, actualUpdatedEvent.RouteToProfessionalStatus.ExemptFromInduction);
                Assert.Equal(state.ChangeReason!.GetDisplayName(), changeReason?.Reason);
                Assert.Equal(state.ChangeReasonDetail.ChangeReasonDetail, changeReason?.Details);
                Assert.Null(changeReason?.EvidenceFile);
            });
        });
        Assert.Null(GetJourneyInstanceState(journeyInstance));
    }

    [Fact]
    public async Task Post_Confirm_WithAwardedQtsRouteTypeUpdatesPersonQtsDateAndHasChangesInEvent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithHoldsRouteToProfessionalStatus(ProfessionalStatusType.QualifiedTeacherStatus));
        EventObserver.Clear();

        var qualification = person.Qualifications!.OfType<RouteToProfessionalStatus>().First();

        var journeyInstance = await CreateJourneyInstanceAsync(qualification, e => e.HoldsFrom = qualification.HoldsFrom!.Value.AddDays(-1));

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/routes/{qualification.QualificationId}/edit/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        var state = GetJourneyInstanceState(journeyInstance)!;

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var updatedPerson = await WithDbContextAsync(dbContext => dbContext.Persons.SingleAsync(p => p.PersonId == person.PersonId));
        Assert.Equal(state.HoldsFrom, updatedPerson.QtsDate);

        Events.AssertProcessesCreated(p =>
        {
            Assert.Equal(ProcessType.RouteToProfessionalStatusUpdating, p.ProcessContext.ProcessType);
            p.AssertProcessHasEvents<RouteToProfessionalStatusUpdatedEvent>(actualCreatedEvent =>
            {
                Assert.Equal(state.HoldsFrom, actualCreatedEvent.PersonAttributes.QtsDate);
                Assert.Equal(qualification.HoldsFrom, actualCreatedEvent.OldPersonAttributes.QtsDate);
            });
        });
    }

    [Fact]
    public async Task Post_Confirm_WithAwardedEytsRouteTypeUpdatesPersonEytsDateAndHasChangesInEvent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithHoldsRouteToProfessionalStatus(ProfessionalStatusType.EarlyYearsTeacherStatus));
        EventObserver.Clear();

        var qualification = person.Qualifications!.OfType<RouteToProfessionalStatus>().First();

        var journeyInstance = await CreateJourneyInstanceAsync(qualification, e => e.HoldsFrom = qualification.HoldsFrom!.Value.AddDays(-1));
        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/routes/{qualification.QualificationId}/edit/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        var state = GetJourneyInstanceState(journeyInstance)!;

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var updatedPerson = await WithDbContextAsync(dbContext => dbContext.Persons.SingleAsync(p => p.PersonId == person.PersonId));
        Assert.Equal(state.HoldsFrom, updatedPerson.EytsDate);

        Events.AssertProcessesCreated(p =>
        {
            Assert.Equal(ProcessType.RouteToProfessionalStatusUpdating, p.ProcessContext.ProcessType);
            p.AssertProcessHasEvents<RouteToProfessionalStatusUpdatedEvent>(actualCreatedEvent =>
            {
                Assert.Equal(state.HoldsFrom, actualCreatedEvent.PersonAttributes.EytsDate);
                Assert.Equal(qualification.HoldsFrom, actualCreatedEvent.OldPersonAttributes.EytsDate);
            });
        });
    }

    // N.B. There's no test for EYPS since our one EYPS route has to have a Professional status date
    // (so there's no edit that can be made through this journey that can affect Person.HasEyps)

    [Fact]
    public async Task Post_Confirm_WithAwardedPqtsRouteTypeUpdatesPersonQtsDateAndHasChangesInEvent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(p => p
            .WithHoldsRouteToProfessionalStatus(ProfessionalStatusType.PartialQualifiedTeacherStatus));
        EventObserver.Clear();

        var qualification = person.Qualifications!.OfType<RouteToProfessionalStatus>().First();

        var journeyInstance = await CreateJourneyInstanceAsync(qualification, e => e.HoldsFrom = qualification.HoldsFrom!.Value.AddDays(1));

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/routes/{qualification.QualificationId}/edit/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        var state = GetJourneyInstanceState(journeyInstance)!;

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var updatedPerson = await WithDbContextAsync(dbContext => dbContext.Persons.SingleAsync(p => p.PersonId == person.PersonId));
        Assert.Equal(state.HoldsFrom, updatedPerson.PqtsDate);

        Events.AssertProcessesCreated(p =>
        {
            Assert.Equal(ProcessType.RouteToProfessionalStatusUpdating, p.ProcessContext.ProcessType);
            p.AssertProcessHasEvents<RouteToProfessionalStatusUpdatedEvent>(actualCreatedEvent =>
            {
                Assert.Equal(state.HoldsFrom, actualCreatedEvent.PersonAttributes.PqtsDate);
                Assert.Equal(qualification.HoldsFrom, actualCreatedEvent.OldPersonAttributes.PqtsDate);
            });
        });
    }

    [Theory]
    [HttpMethods(TestHttpMethods.GetAndPost)]
    public async Task PersonIsDeactivated_ReturnsBadRequest(HttpMethod httpMethod)
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).Where(r => r.Name == "Northern Irish Recognition").Single();
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(RouteToProfessionalStatusStatus.Deferred)));
        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(person);
            person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });
        var qualificationId = person.Qualifications!.OfType<RouteToProfessionalStatus>().First().QualificationId;
        var editRouteState = new EditRouteState
        {
            RouteToProfessionalStatusId = route.RouteToProfessionalStatusTypeId,
            Status = RouteToProfessionalStatusStatus.Deferred,
            CurrentStatus = RouteToProfessionalStatusStatus.Deferred,
            TrainingCountryId = _countryCode,
            ChangeReason = ChangeReasonOption.AnotherReason,
            ChangeReasonDetail = CreateChangeReasonDetail()
        };

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            editRouteState
            );

        var request = new HttpRequestMessage(httpMethod, $"/routes/{qualificationId}/edit/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    private Task<EditRouteJourneyCoordinator> CreateJourneyInstanceAsync(RouteToProfessionalStatus qualification, Action<EditRouteState> action)
    {
        var editRouteState = CreateStateFromRoute(qualification);
        action(editRouteState);
        editRouteState.ChangeReason = ChangeReasonOption.AnotherReason;

        editRouteState.ChangeReasonDetail = new ChangeReasonDetailsState
        {
            ProvideAdditionalInformation = ProvideMoreInformationOption.No,
            ChangeReasonDetail = "this is the change Reason",
            AdditionalInformation = null
        };

        return CreateJourneyInstanceAsync(qualification.QualificationId, editRouteState);
    }

    // The answers the journey starts with, as the coordinator seeds them from the route.
    private static EditRouteState CreateStateFromRoute(RouteToProfessionalStatus qualification) =>
        new()
        {
            QualificationType = qualification.QualificationType,
            RouteToProfessionalStatusId = qualification.RouteToProfessionalStatusTypeId,
            CurrentStatus = qualification.Status,
            Status = qualification.Status,
            HoldsFrom = qualification.HoldsFrom,
            TrainingStartDate = qualification.TrainingStartDate,
            TrainingEndDate = qualification.TrainingEndDate,
            TrainingSubjectIds = qualification.TrainingSubjectIds,
            TrainingAgeSpecialismType = qualification.TrainingAgeSpecialismType,
            TrainingAgeSpecialismRangeFrom = qualification.TrainingAgeSpecialismRangeFrom,
            TrainingAgeSpecialismRangeTo = qualification.TrainingAgeSpecialismRangeTo,
            TrainingCountryId = qualification.TrainingCountryId,
            TrainingProviderId = qualification.TrainingProviderId,
            IsExemptFromInduction = qualification.ExemptFromInduction,
            DegreeTypeId = qualification.DegreeTypeId
        };
}
