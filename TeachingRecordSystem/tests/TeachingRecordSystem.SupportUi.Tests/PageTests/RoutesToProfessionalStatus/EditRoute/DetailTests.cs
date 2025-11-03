using AngleSharp.Html.Dom;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.RoutesToProfessionalStatus.EditRoute;

public class DetailTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Cancel_DeletesJourneyAndRedirectsToExpectedPage()
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).SingleRandom();
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(RouteToProfessionalStatusStatus.Deferred)));
        var qualificationid = person.ProfessionalStatuses.First().QualificationId;
        var editRouteState = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(RouteToProfessionalStatusStatus.Deferred)
            .WithValidChangeReasonOption()
            .WithDefaultChangeReasonNoUploadFileDetail()
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationid,
            editRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Get, $"/routes/{qualificationid}/edit/detail?{journeyInstance.GetUniqueIdQueryParameter()}");

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
    public async Task Continue_LinksToExpectedPage()
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).SingleRandom();
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(RouteToProfessionalStatusStatus.Deferred)));
        var qualificationid = person.ProfessionalStatuses.First().QualificationId;
        var editRouteState = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(RouteToProfessionalStatusStatus.Deferred)
            .WithValidChangeReasonOption()
            .WithDefaultChangeReasonNoUploadFileDetail()
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationid,
            editRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Get, $"/routes/{qualificationid}/edit/detail?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var continueButton = doc.GetElementByTestId("continue-button") as IHtmlAnchorElement;
        Assert.Contains($"/routes/{qualificationid}/edit/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}", continueButton!.Href);
    }

    [Fact]
    public async Task Get_RouteAndStatusWithAllFieldsApplicable_AllFieldsShown()
    {
        // Arrange
        var startDate = Clock.Today.AddYears(-1);
        var endDate = Clock.Today;
        var awardDate = Clock.Today;
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
                .WithStatus(status)
                .WithHoldsFrom(awardDate)));

        var qualificationid = person.ProfessionalStatuses.First().QualificationId;
        var editRouteState = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(status)
            .WithTrainingStartDate(startDate)
            .WithTrainingEndDate(endDate)
            .WithHoldsFrom(awardDate)
            .WithTrainingProviderId(trainingProvider.TrainingProviderId)
            .WithTrainingSubjectIds(subjects.Select(s => s.TrainingSubjectId).ToArray())
            .WithTrainingCountryId(country.CountryId)
            .WithTrainingAgeSpecialismType(ageRange)
            .WithValidChangeReasonOption()
            .WithDefaultChangeReasonNoUploadFileDetail()
            .WithDegreeTypeId(degreeType.DegreeTypeId)
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationid,
            editRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Get, $"/routes/{qualificationid}/edit/detail?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertSummaryListRowValueContentMatches("Route", route.Name);
        doc.AssertSummaryListRowValueContentMatches("Status", status.GetDisplayName()!);
        doc.AssertSummaryListRowValueContentMatches("Start date", startDate.ToString(UiDefaults.DateOnlyDisplayFormat));
        doc.AssertChangeLinkExists("Start date");
        doc.AssertSummaryListRowValueContentMatches("End date", endDate.ToString(UiDefaults.DateOnlyDisplayFormat));
        doc.AssertChangeLinkExists("End date");
        doc.AssertSummaryListRowValueContentMatches("Held since", awardDate.ToString(UiDefaults.DateOnlyDisplayFormat));
        doc.AssertSummaryListRowValueContentMatches("Induction exemption", "Not provided");
        doc.AssertSummaryListRowValueContentMatches("Training provider", trainingProvider.Name);
        doc.AssertChangeLinkExists("Training provider");
        doc.AssertSummaryListRowValueContentMatches("Degree type", degreeType.Name);
        doc.AssertChangeLinkExists("Degree type");
        doc.AssertSummaryListRowValueContentMatches("Country of training", country.Name);
        doc.AssertSummaryListRowValueContentMatches("Age range", ageRange.GetDisplayName()!);
        doc.AssertSummaryListRowValueContentMatches("Subjects", subjects.Select(s => $"{s.Reference} - {s.Name}"));
    }

    [Fact]
    public async Task Get_ShowsOptionalAnswers_AsExpected()
    {
        // Arrange
        var startDate = Clock.Today.AddYears(-1);
        var endDate = Clock.Today;
        var route = await ReferenceDataCache.GetRouteWhereAllFieldsHaveFieldRequirementAsync(FieldRequirement.Optional);
        var status = ProfessionalStatusStatusRegistry.All.Where(s => s.Value == RouteToProfessionalStatusStatus.InTraining).SingleRandom();
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(status.Value)
                .WithTrainingStartDate(startDate)
                .WithTrainingEndDate(endDate)
                 ));

        var qualificationid = person.ProfessionalStatuses.First().QualificationId;

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationid,
            new EditRouteState()
            );

        var request = new HttpRequestMessage(HttpMethod.Get, $"/routes/{qualificationid}/edit/detail?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertSummaryListRowValueContentMatches("Route", route.Name);
        doc.AssertSummaryListRowValueContentMatches("Status", status.Title);
        doc.AssertSummaryListRowValueContentMatches("Start date", startDate.ToString(UiDefaults.DateOnlyDisplayFormat));
        doc.AssertSummaryListRowValueContentMatches("End date", endDate.ToString(UiDefaults.DateOnlyDisplayFormat));
        doc.AssertSummaryListRowValueContentMatches("Training provider", "Not provided");
        doc.AssertSummaryListRowValueContentMatches("Degree type", "Not provided");
        doc.AssertSummaryListRowValueContentMatches("Country of training", "Not provided");
        doc.AssertSummaryListRowValueContentMatches("Age range", "Not provided");
        doc.AssertSummaryListRowValueContentMatches("Subjects", "Not provided");
    }

    [Theory]
    [InlineData("Awarded date", "HoldsFromRequired")]
    [InlineData("Induction exemption", "InductionExemptionRequired")]
    public async Task Get_FieldNotApplicable_FieldNotShown(string elementText, string propertySelector)
    {
        // Arrange
        var applicableRoutes = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(TestDataHelper.PropertyHasFieldRequirement<RouteToProfessionalStatusType>(propertySelector, FieldRequirement.NotApplicable));
        var route = applicableRoutes.Any() ? applicableRoutes.SingleRandom() : (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).SingleRandom();

        var status = ProfessionalStatusStatusRegistry.All
            .Where(TestDataHelper.PropertyHasFieldRequirement<ProfessionalStatusStatusInfo>(propertySelector, FieldRequirement.NotApplicable))
            .SingleRandom();

        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(RouteToProfessionalStatusStatus.InTraining)));

        var qualificationid = person.ProfessionalStatuses.First().QualificationId;
        var editRouteState = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(status.Value)
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationid,
            editRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Get, $"/routes/{qualificationid}/edit/detail?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.DoesNotContain(doc.QuerySelectorAll(".govuk-summary-list__key"), e => e.TrimmedText() == elementText);
    }

    [Theory]
    [InlineData(RouteToProfessionalStatusStatus.Holds, true, "Yes")]
    [InlineData(RouteToProfessionalStatusStatus.Holds, false, "No")]
    public async Task Get_RouteAndStatusWithExemptionInduction_ShowsFieldAndChangeLink(RouteToProfessionalStatusStatus status, bool? hasExemption, string expectedContent)
    {
        // Arrange
        var startDate = Clock.Today.AddYears(-1);
        var endDate = Clock.Today;
        var awardDate = Clock.Today;
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.Name == "Northern Irish Recognition")
            .SingleRandom();

        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(status)
                .WithHoldsFrom(awardDate)));

        var qualificationid = person.ProfessionalStatuses.First().QualificationId;
        var editRouteState = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(status)
            .WithTrainingStartDate(startDate)
            .WithTrainingEndDate(endDate)
            .WithHoldsFrom(awardDate)
            .WithInductionExemption(isExempt: hasExemption)
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationid,
            editRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Get, $"/routes/{qualificationid}/edit/detail?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertSummaryListRowValueContentMatches("Induction exemption", expectedContent);
        doc.AssertChangeLinkExists("Induction exemption");
    }

    [Fact]
    public async Task Get_RouteAndStatusWithImplictExemptionInduction_ShowsFieldButNoChangeLink()
    {
        // Arrange
        var startDate = Clock.Today.AddYears(-1);
        var endDate = Clock.Today;
        var awardDate = Clock.Today;
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.InductionExemptionReason is not null && r.InductionExemptionReason.RouteImplicitExemption)
            .SingleRandom();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.InductionExemptionRequired == FieldRequirement.Mandatory)
            .SingleRandom()
            .Value;

        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(status)
                .WithHoldsFrom(awardDate)));

        var qualificationid = person.ProfessionalStatuses.First().QualificationId;
        var editRouteState = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(status)
            .WithTrainingStartDate(startDate)
            .WithTrainingEndDate(endDate)
            .WithHoldsFrom(awardDate)
            .WithInductionExemption(true)
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationid,
            editRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Get, $"/routes/{qualificationid}/edit/detail?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertSummaryListRowValueContentMatches("Induction exemption", "Yes");
        doc.AssertNoChangeLink("Induction exemption");
    }

    [Theory]
    [HttpMethods(TestHttpMethods.GetAndPost)]
    public async Task PersonIsDeactivated_ReturnsBadRequest(HttpMethod httpMethod)
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).SingleRandom();
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
        var qualificationid = person.ProfessionalStatuses.First().QualificationId;
        var editRouteState = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(RouteToProfessionalStatusStatus.Deferred)
            .WithValidChangeReasonOption()
            .WithDefaultChangeReasonNoUploadFileDetail()
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationid,
            editRouteState
            );

        var request = new HttpRequestMessage(httpMethod, $"/routes/{qualificationid}/edit/detail?{journeyInstance.GetUniqueIdQueryParameter()}");

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
}
