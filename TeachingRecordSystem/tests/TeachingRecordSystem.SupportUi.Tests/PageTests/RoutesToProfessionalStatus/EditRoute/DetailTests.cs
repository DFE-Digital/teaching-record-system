using AngleSharp.Html.Dom;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.RoutesToProfessionalStatus.EditRoute;

public class DetailTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Cancel_RedirectsToExpectedPage()
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRoutesToProfessionalStatusAsync()).RandomOne();
        var person = await TestData.CreatePersonAsync(p => p
            .WithProfessionalStatus(r => r
                .WithRoute(route.RouteToProfessionalStatusId)
                .WithStatus(ProfessionalStatusStatus.Deferred)));
        var qualificationid = person.ProfessionalStatuses.First().QualificationId;
        var editRouteState = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusId)
            .WithStatus(ProfessionalStatusStatus.Deferred)
            .WithValidChangeReasonOption()
            .WithDefaultChangeReasonNoUploadFileDetail()
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationid,
            editRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Get, $"/route/{qualificationid}/edit/detail?{journeyInstance.GetUniqueIdQueryParameter()}");

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
    }

    [Fact]
    public async Task Continue_LinksToExpectedPage()
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRoutesToProfessionalStatusAsync()).RandomOne();
        var person = await TestData.CreatePersonAsync(p => p
            .WithProfessionalStatus(r => r
                .WithRoute(route.RouteToProfessionalStatusId)
                .WithStatus(ProfessionalStatusStatus.Deferred)));
        var qualificationid = person.ProfessionalStatuses.First().QualificationId;
        var editRouteState = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusId)
            .WithStatus(ProfessionalStatusStatus.Deferred)
            .WithValidChangeReasonOption()
            .WithDefaultChangeReasonNoUploadFileDetail()
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationid,
            editRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Get, $"/route/{qualificationid}/edit/detail?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var continueButton = doc.GetElementByTestId("continue-button") as IHtmlAnchorElement;
        Assert.Contains($"/route/{qualificationid}/edit/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}", continueButton!.Href);
    }

    [Fact]
    public async Task Get_RouteAndStatusWithAllFieldsApplicable_AllFieldsShown()
    {
        // Arrange
        var startDate = Clock.Today.AddYears(-1);
        var endDate = Clock.Today;
        var awardDate = Clock.Today;
        var route = await ReferenceDataCache.GetRouteWhereAllFieldsApplyAsync();
        var status = ReferenceDataCache.GetRouteStatusWhereAllFieldsApply();
        var trainingProvider = (await ReferenceDataCache.GetTrainingProvidersAsync()).First();
        var subjects = (await ReferenceDataCache.GetTrainingSubjectsAsync()).Take(1);
        var country = (await ReferenceDataCache.GetTrainingCountriesAsync()).RandomOne();
        var degreeType = (await ReferenceDataCache.GetDegreeTypesAsync()).RandomOne();
        var ageRange = TrainingAgeSpecialismType.KeyStage3;

        var person = await TestData.CreatePersonAsync(p => p
            .WithProfessionalStatus(r => r
                .WithRoute(route.RouteToProfessionalStatusId)
                .WithStatus(status)));

        var qualificationid = person.ProfessionalStatuses.First().QualificationId;
        var editRouteState = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusId)
            .WithStatus(status)
            .WithTrainingStartDate(startDate)
            .WithTrainingEndDate(endDate)
            .WithAwardedDate(awardDate)
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

        var request = new HttpRequestMessage(HttpMethod.Get, $"/route/{qualificationid}/edit/detail?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertRowContentMatches("Route", route.Name);
        doc.AssertRowContentMatches("Status", status.GetDisplayName()!);
        doc.AssertRowContentMatches("Start date", startDate.ToString(UiDefaults.DateOnlyDisplayFormat));
        doc.AssertChangeLinkExists("Start date");
        doc.AssertRowContentMatches("End date", endDate.ToString(UiDefaults.DateOnlyDisplayFormat));
        doc.AssertChangeLinkExists("End date");
        doc.AssertRowContentMatches("Award date", awardDate.ToString(UiDefaults.DateOnlyDisplayFormat));
        doc.AssertRowContentMatches("Has exemption", "Not provided");
        doc.AssertRowContentMatches("Training provider", trainingProvider.Name);
        doc.AssertChangeLinkExists("Training provider");
        doc.AssertRowContentMatches("Degree type", degreeType.Name);
        doc.AssertChangeLinkExists("Degree type");
        doc.AssertRowContentMatches("Country of training", country.Name);
        doc.AssertRowContentMatches("Age range", ageRange.GetDisplayName()!);
        doc.AssertRowContentMatches("Subjects", subjects.Select(s => s.Name));
    }

    [Fact]
    public async Task Get_ShowsOptionalAnswers_AsExpected()
    {
        // Arrange
        var startDate = Clock.Today.AddYears(-1);
        var endDate = Clock.Today;
        var route = await ReferenceDataCache.GetRouteWhereAllFieldsHaveFieldRequirementAsync(FieldRequirement.Optional);
        var status = ProfessionalStatusStatusRegistry.All.Where(s => s.Value == ProfessionalStatusStatus.InTraining).RandomOne();
        var person = await TestData.CreatePersonAsync(p => p
            .WithProfessionalStatus(r => r
                .WithRoute(route.RouteToProfessionalStatusId)
                .WithStatus(status.Value)
                .WithTrainingStartDate(startDate)
                .WithTrainingEndDate(endDate)
                 ));

        var qualificationid = person.ProfessionalStatuses.First().QualificationId;

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationid,
            new EditRouteState()
            );

        var request = new HttpRequestMessage(HttpMethod.Get, $"/route/{qualificationid}/edit/detail?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertRowContentMatches("Route", route.Name);
        doc.AssertRowContentMatches("Status", status.Title);
        doc.AssertRowContentMatches("Start date", startDate.ToString(UiDefaults.DateOnlyDisplayFormat));
        doc.AssertRowContentMatches("End date", endDate.ToString(UiDefaults.DateOnlyDisplayFormat));
        doc.AssertRowContentMatches("Training provider", "Not provided");
        doc.AssertRowContentMatches("Degree type", "Not provided");
        doc.AssertRowContentMatches("Country of training", "Not provided");
        doc.AssertRowContentMatches("Age range", "Not provided");
        doc.AssertRowContentMatches("Subjects", "Not provided");
    }

    [Theory]
    [InlineData("Start date", "TrainingStartDateRequired")]
    [InlineData("End date", "TrainingEndDateRequired")]
    [InlineData("Award date", "AwardDateRequired")]
    [InlineData("Degree type", "DegreeTypeRequired")]
    [InlineData("Training provider", "TrainingProviderRequired")]
    [InlineData("Country of training", "TrainingCountryRequired")]
    [InlineData("Age range", "TrainingAgeSpecialismTypeRequired")]
    [InlineData("Subjects provider", "TrainingSubjectsRequired")]
    [InlineData("Has exemption", "InductionExemptionRequired")]
    public async Task Get_FieldNotApplicable_FieldNotShown(string elementText, string propertySelector)
    {
        // Arrange
        var applicableRoutes = (await ReferenceDataCache.GetRoutesToProfessionalStatusAsync())
            .Where(TestDataHelper.PropertyHasFieldRequirement<RouteToProfessionalStatus>(propertySelector, FieldRequirement.NotApplicable));
        var route = applicableRoutes.Any() ? applicableRoutes.RandomOne() : (await ReferenceDataCache.GetRoutesToProfessionalStatusAsync()).RandomOne();

        var status = ProfessionalStatusStatusRegistry.All
            .Where(TestDataHelper.PropertyHasFieldRequirement<ProfessionalStatusStatusInfo>(propertySelector, FieldRequirement.NotApplicable))
            .RandomOne();

        var person = await TestData.CreatePersonAsync(p => p
            .WithProfessionalStatus(r => r
                .WithRoute(route.RouteToProfessionalStatusId)
                .WithStatus(ProfessionalStatusStatus.InTraining)));

        var qualificationid = person.ProfessionalStatuses.First().QualificationId;
        var editRouteState = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusId)
            .WithStatus(status.Value)
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationid,
            editRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Get, $"/route/{qualificationid}/edit/detail?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Empty(doc.QuerySelectorAll(".govuk-summary-list__key").Where(e => e.TextContent == elementText));
    }

    private Task<JourneyInstance<EditRouteState>> CreateJourneyInstanceAsync(Guid qualificationId, EditRouteState? state = null) =>
    CreateJourneyInstance(
        JourneyNames.EditRouteToProfessionalStatus,
        state ?? new EditRouteState(),
        new KeyValuePair<string, object>("qualificationId", qualificationId));
}
