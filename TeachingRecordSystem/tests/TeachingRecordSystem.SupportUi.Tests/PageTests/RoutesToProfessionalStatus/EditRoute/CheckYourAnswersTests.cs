using AngleSharp.Html.Dom;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.Legacy;
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
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).Where(r => r.Name == "Northern Irish Recognition").Single();
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(RouteToProfessionalStatusStatus.Deferred)));
        var qualificationId = person.ProfessionalStatuses.First().QualificationId;
        var editRouteState = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(RouteToProfessionalStatusStatus.Deferred)
            .WithTrainingCountryId(_countryCode)
            .WithValidChangeReasonOption()
            .WithDefaultChangeReasonNoUploadFileDetail()
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            editRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Get, $"/routes/{qualificationId}/edit/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

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

    [Theory]
    [InlineData("Apply for Qualified Teacher Status in England", RouteToProfessionalStatusStatus.Holds, AddRoutePage.StartAndEndDate, null)]
    [InlineData("Apply for Qualified Teacher Status in England", RouteToProfessionalStatusStatus.Holds, AddRoutePage.HoldsFrom, "holds-from")]
    [InlineData("Apply for Qualified Teacher Status in England", RouteToProfessionalStatusStatus.Holds, AddRoutePage.InductionExemption, "induction-exemption")]
    [InlineData("Apply for Qualified Teacher Status in England", RouteToProfessionalStatusStatus.Holds, AddRoutePage.TrainingProvider, null)]
    [InlineData("Apply for Qualified Teacher Status in England", RouteToProfessionalStatusStatus.Holds, AddRoutePage.DegreeType, null)]
    [InlineData("Apply for Qualified Teacher Status in England", RouteToProfessionalStatusStatus.Holds, AddRoutePage.Country, "country")]
    [InlineData("Apply for Qualified Teacher Status in England", RouteToProfessionalStatusStatus.Holds, AddRoutePage.AgeRangeSpecialism, null)]
    [InlineData("Apply for Qualified Teacher Status in England", RouteToProfessionalStatusStatus.Holds, AddRoutePage.SubjectSpecialisms, null)]
    [InlineData("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.InTraining, AddRoutePage.StartAndEndDate, null)]
    [InlineData("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.InTraining, AddRoutePage.HoldsFrom, null)]
    [InlineData("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.InTraining, AddRoutePage.InductionExemption, null)]
    [InlineData("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.InTraining, AddRoutePage.TrainingProvider, "training-provider")]
    [InlineData("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.InTraining, AddRoutePage.DegreeType, "degree-type")]
    [InlineData("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.InTraining, AddRoutePage.Country, "country")]
    [InlineData("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.InTraining, AddRoutePage.AgeRangeSpecialism, null)]
    [InlineData("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.InTraining, AddRoutePage.SubjectSpecialisms, null)]
    [InlineData("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.Holds, AddRoutePage.StartAndEndDate, null)]
    [InlineData("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.Holds, AddRoutePage.HoldsFrom, "holds-from")]
    [InlineData("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.Holds, AddRoutePage.InductionExemption, null)]
    [InlineData("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.Holds, AddRoutePage.TrainingProvider, "training-provider")]
    [InlineData("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.Holds, AddRoutePage.DegreeType, "degree-type")]
    [InlineData("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.Holds, AddRoutePage.Country, "country")]
    [InlineData("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.Holds, AddRoutePage.AgeRangeSpecialism, null)]
    [InlineData("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.Holds, AddRoutePage.SubjectSpecialisms, null)]
    [InlineData("Early Years Teacher Degree Apprenticeship", RouteToProfessionalStatusStatus.Holds, AddRoutePage.StartAndEndDate, null)]
    [InlineData("Early Years Teacher Degree Apprenticeship", RouteToProfessionalStatusStatus.Holds, AddRoutePage.HoldsFrom, "holds-from")]
    [InlineData("Early Years Teacher Degree Apprenticeship", RouteToProfessionalStatusStatus.Holds, AddRoutePage.InductionExemption, null)]
    [InlineData("Early Years Teacher Degree Apprenticeship", RouteToProfessionalStatusStatus.Holds, AddRoutePage.TrainingProvider, null)]
    [InlineData("Early Years Teacher Degree Apprenticeship", RouteToProfessionalStatusStatus.Holds, AddRoutePage.DegreeType, "degree-type")]
    [InlineData("Early Years Teacher Degree Apprenticeship", RouteToProfessionalStatusStatus.Holds, AddRoutePage.Country, "country")]
    [InlineData("Early Years Teacher Degree Apprenticeship", RouteToProfessionalStatusStatus.Holds, AddRoutePage.AgeRangeSpecialism, "age-range")]
    [InlineData("Early Years Teacher Degree Apprenticeship", RouteToProfessionalStatusStatus.Holds, AddRoutePage.SubjectSpecialisms, "subjects")]
    public async Task Get_IncompleteJourney_RedirectsToExpectedPage(string routeName, RouteToProfessionalStatusStatus status, AddRoutePage incompletePage, string? expectedRedirectPage)
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).Where(r => r.Name == routeName).Single();
        var startDate = Clock.Today.AddYears(-1);
        var endDate = Clock.Today.AddDays(-1);
        var holdsFrom = endDate.AddDays(1);
        var subjects = (await ReferenceDataCache.GetTrainingSubjectsAsync()).Where(s => !s.Name.Contains('\'')).Take(1);
        var trainingProvider = (await ReferenceDataCache.GetTrainingProvidersAsync()).Where(s => !s.Name.Contains('\'')).SingleRandom();
        var degreeType = (await ReferenceDataCache.GetDegreeTypesAsync()).SingleRandom();
        var country = (await ReferenceDataCache.GetTrainingCountriesAsync()).SingleRandom();

        var builder = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(status);

        if (incompletePage != AddRoutePage.StartAndEndDate)
        {
            builder = builder
                .WithTrainingStartDate(startDate)
                .WithTrainingEndDate(endDate);
        }

        if (incompletePage != AddRoutePage.HoldsFrom)
        {
            builder = builder
                .WithHoldsFrom(holdsFrom);
        }

        if (incompletePage != AddRoutePage.TrainingProvider)
        {
            builder = builder
                .WithTrainingProviderId(trainingProvider.TrainingProviderId);
        }

        if (incompletePage != AddRoutePage.Country)
        {
            builder = builder
                .WithTrainingCountryId(country.CountryId);
        }

        if (incompletePage != AddRoutePage.SubjectSpecialisms)
        {
            builder = builder
                .WithTrainingSubjectIds(subjects.Select(s => s.TrainingSubjectId).ToArray());
        }

        if (incompletePage != AddRoutePage.AgeRangeSpecialism)
        {
            builder = builder
                .WithTrainingAgeSpecialismType(TrainingAgeSpecialismType.FoundationStage);
        }

        if (incompletePage != AddRoutePage.DegreeType)
        {
            builder = builder
                .WithDegreeTypeId(degreeType.DegreeTypeId);
        }

        if (incompletePage != AddRoutePage.InductionExemption)
        {
            builder = builder
                .WithInductionExemption(true);
        }

        if (incompletePage != AddRoutePage.ChangeReason)
        {
            builder = builder
                .WithValidChangeReasonOption()
                .WithDefaultChangeReasonNoUploadFileDetail();
        }

        var editRouteState = builder.Build();
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
            .WithRouteType(route.RouteToProfessionalStatusTypeId)
            .WithStatus(RouteToProfessionalStatusStatus.Deferred)));
        var qualificationId = person.ProfessionalStatuses.First().QualificationId;

        var journeyInstance = await CreateJourneyInstanceAsync(qualificationId, editRouteState);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/routes/{qualificationId}/edit/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        if (expectedRedirectPage != null)
        {
            Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
            var location = response.Headers.Location?.OriginalString;
            Assert.Equal($"/routes/{qualificationId}/edit/{expectedRedirectPage}?fromCheckAnswers=True&{journeyInstance.GetUniqueIdQueryParameter()}", location);
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
        var qualificationId = person.ProfessionalStatuses.First().QualificationId;
        var editRouteState = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(RouteToProfessionalStatusStatus.Deferred)
            .WithTrainingCountryId(_countryCode)
            .WithValidChangeReasonOption()
            .WithDefaultChangeReasonNoUploadFileDetail()
            .Build();

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
        var startDate = Clock.Today.AddYears(-1);
        var endDate = Clock.Today.AddDays(-1);
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

        var qualificationId = person.ProfessionalStatuses.First().QualificationId;
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
        doc.AssertSummaryListRowValueContentMatches("Start date", startDate.ToString(WebConstants.DateOnlyDisplayFormat));
        doc.AssertSummaryListRowValueContentMatches("End date", endDate.ToString(WebConstants.DateOnlyDisplayFormat));
        doc.AssertSummaryListRowValueContentMatches("Held since", holdsFrom.ToString(WebConstants.DateOnlyDisplayFormat));
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
        var startDate = Clock.Today.AddYears(-1);
        var endDate = Clock.Today;
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

        var qualificationId = person.ProfessionalStatuses.First().QualificationId;
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
            qualificationId,
            editRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Get, $"/routes/{qualificationId}/edit/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertSummaryListRowValueContentMatches("Held since", endDate.ToString(WebConstants.DateOnlyDisplayFormat));
        doc.AssertSummaryListRowValueContentMatches("Induction exemption", "Yes");
    }

    [Fact]
    public async Task Get_ShowsOptionalAnswersNotPopulated_AsExpected()
    {
        // Arrange
        var startDate = Clock.Today.AddYears(-1);
        var endDate = Clock.Today;
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).Where(r => r.Name == "Postgraduate Teaching Apprenticeship").Single();
        var trainingProvider = (await ReferenceDataCache.GetTrainingProvidersAsync()).Where(s => !s.Name.Contains('\''))
            .SingleRandom();
        var degreeType = (await ReferenceDataCache.GetDegreeTypesAsync())
            .SingleRandom();
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(RouteToProfessionalStatusStatus.InTraining)));

        var qualificationId = person.ProfessionalStatuses.First().QualificationId;
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
        doc.AssertSummaryListRowValueContentMatches("Start date", startDate.ToString(WebConstants.DateOnlyDisplayFormat));
        doc.AssertSummaryListRowValueContentMatches("End date", endDate.ToString(WebConstants.DateOnlyDisplayFormat));
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
        var qualificationId = person.ProfessionalStatuses.First().QualificationId;
        var editRouteState = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(RouteToProfessionalStatusStatus.Deferred)
            .WithTrainingCountryId(_countryCode)
            .WithValidChangeReasonOption()
            .WithDefaultChangeReasonNoUploadFileDetail()
            .Build();

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
        doc.AssertSummaryListRowValueContentMatches("Additional information", editRouteState.ChangeReasonDetail!.ChangeReasonDetail!);
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

        var request = new HttpRequestMessage(HttpMethod.Post, $"/routes/{qualificationId}/edit/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var redirectResponse = await response.FollowRedirectAsync(HttpClient);
        var redirectDoc = await redirectResponse.GetDocumentAsync();
        AssertEx.HtmlDocumentHasFlashSuccess(redirectDoc, "Route to professional status updated");

        await WithDbContextAsync(async dbContext =>
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

        EventObserver.AssertEventsSaved(e =>
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
            Assert.Null(actualUpdatedEvent.EvidenceFile);
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
        EventObserver.Clear();

        var qualification = person.ProfessionalStatuses.First();

        var journeyInstance = await CreateJourneyInstanceAsync(qualification, e => e.HoldsFrom = qualification.HoldsFrom!.Value.AddDays(-1));

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/routes/{qualification.QualificationId}/edit/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var updatedPerson = await WithDbContextAsync(dbContext => dbContext.Persons.SingleAsync(p => p.PersonId == person.PersonId));
        Assert.Equal(journeyInstance.State.HoldsFrom, updatedPerson.QtsDate);

        EventObserver.AssertEventsSaved(e =>
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
        EventObserver.Clear();

        var qualification = person.ProfessionalStatuses.First();

        var journeyInstance = await CreateJourneyInstanceAsync(qualification, e => e.HoldsFrom = qualification.HoldsFrom!.Value.AddDays(-1));
        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/routes/{qualification.QualificationId}/edit/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var updatedPerson = await WithDbContextAsync(dbContext => dbContext.Persons.SingleAsync(p => p.PersonId == person.PersonId));
        Assert.Equal(journeyInstance.State.HoldsFrom, updatedPerson.EytsDate);

        EventObserver.AssertEventsSaved(e =>
        {
            var actualCreatedEvent = Assert.IsType<RouteToProfessionalStatusUpdatedEvent>(e);

            Assert.Equal(journeyInstance.State.HoldsFrom, actualCreatedEvent.PersonAttributes.EytsDate);
            Assert.Equal(qualification.HoldsFrom, actualCreatedEvent.OldPersonAttributes.EytsDate);
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

        var qualification = person.ProfessionalStatuses.First();

        var journeyInstance = await CreateJourneyInstanceAsync(qualification, e => e.HoldsFrom = qualification.HoldsFrom!.Value.AddDays(1));

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/routes/{qualification.QualificationId}/edit/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var updatedPerson = await WithDbContextAsync(dbContext => dbContext.Persons.SingleAsync(p => p.PersonId == person.PersonId));
        Assert.Equal(journeyInstance.State.HoldsFrom, updatedPerson.PqtsDate);

        EventObserver.AssertEventsSaved(e =>
        {
            var actualCreatedEvent = Assert.IsType<RouteToProfessionalStatusUpdatedEvent>(e);

            Assert.Equal(journeyInstance.State.HoldsFrom, actualCreatedEvent.PersonAttributes.PqtsDate);
            Assert.Equal(qualification.HoldsFrom, actualCreatedEvent.OldPersonAttributes.PqtsDate);
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
            dbContext.Attach(person.Person);
            person.Person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });
        var qualificationId = person.ProfessionalStatuses.First().QualificationId;
        var editRouteState = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(RouteToProfessionalStatusStatus.Deferred)
            .WithTrainingCountryId(_countryCode)
            .WithValidChangeReasonOption()
            .WithDefaultChangeReasonNoUploadFileDetail()
            .Build();

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
        editRouteState.ChangeReasonDetail = new ChangeReasonDetailsState
        {
            HasAdditionalReasonDetail = false,
            ChangeReasonDetail = null
        };

        return CreateJourneyInstanceAsync(qualification.QualificationId, editRouteState);
    }
}
