using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail;

public class QualificationsTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    public override void Dispose()
    {
        FeatureProvider.Features.Remove(FeatureNames.ContactsMigrated);
        FeatureProvider.Features.Remove(FeatureNames.RoutesToProfessionalStatus);

        base.Dispose();
    }

    [Fact]
    public async Task Get_WithPersonIdForNonExistentPerson_ReturnsNotFound()
    {
        // Arrange
        var nonExistentPersonId = Guid.NewGuid().ToString();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{nonExistentPersonId}/qualifications");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_WithPersonIdForPersonWithNoMandatoryQualifications_DisplaysNoMandatoryQualifications()
    {
        // Arrange
        FeatureProvider.Features.Add(FeatureNames.RoutesToProfessionalStatus);
        var person = await TestData.CreatePersonAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/qualifications");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var noMandatoryQualifications = doc.GetElementByTestId("no-mandatory-qualifications");
        Assert.NotNull(noMandatoryQualifications);
    }

    [Fact]
    public async Task Get_WithPersonIdForPersonWithProfessionalStatuses_DisplaysNoProfessionalStatuses()
    {
        // Arrange
        FeatureProvider.Features.Add(FeatureNames.RoutesToProfessionalStatus);
        var person = await TestData.CreatePersonAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/qualifications");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var noProfessionalStatuses = doc.GetElementByTestId("no-professional-statuses");
        Assert.NotNull(noProfessionalStatuses);
    }

    [Theory]
    [InlineData("University of Birmingham", MandatoryQualificationSpecialism.Hearing, "2022-01-05", MandatoryQualificationStatus.Passed, "2022-07-13")]
    [InlineData("University of Birmingham", MandatoryQualificationSpecialism.Hearing, "2022-01-05", MandatoryQualificationStatus.Deferred, null)]
    [InlineData(null, null, null, null, null)]
    public async Task Get_WithPersonIdForPersonWithMandatoryQualifications_DisplaysExpectedContent(
        string? providerName,
        MandatoryQualificationSpecialism? specialism,
        string? startDateString,
        MandatoryQualificationStatus? status,
        string? endDateString)
    {
        // Arrange
        var provider = providerName is not null ? MandatoryQualificationProvider.All.Single(p => p.Name == providerName) : null;
        DateOnly? startDate = !string.IsNullOrEmpty(startDateString) ? DateOnly.Parse(startDateString) : null;
        DateOnly? endDate = !string.IsNullOrEmpty(endDateString) ? DateOnly.Parse(endDateString) : null;

        var person = await TestData.CreatePersonAsync(x => x
            .WithMandatoryQualification(q => q
                .WithProvider(provider?.MandatoryQualificationProviderId)
                .WithSpecialism(specialism)
                .WithStartDate(startDate)
                .WithStatus(status, endDate)));
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/qualifications");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var noMandatoryQualifications = doc.GetElementByTestId("no-mandatory-qualifications");
        Assert.Null(noMandatoryQualifications);

        var mandatoryQualificationSummary = doc.GetElementByTestId($"mq-{qualificationId}");
        Assert.NotNull(mandatoryQualificationSummary);

        Assert.Equal(provider?.Name ?? "None", mandatoryQualificationSummary.GetElementByTestId($"mq-provider-{qualificationId}")!.TrimmedText());
        Assert.Equal(specialism?.GetTitle() ?? "None", mandatoryQualificationSummary.GetElementByTestId($"mq-specialism-{qualificationId}")!.TrimmedText());
        Assert.Equal(startDate is not null ? startDate.Value.ToString(UiDefaults.DateOnlyDisplayFormat) : "None", mandatoryQualificationSummary.GetElementByTestId($"mq-start-date-{qualificationId}")!.TrimmedText());
        Assert.Equal(status is not null ? status.Value.ToString() : "None", mandatoryQualificationSummary.GetElementByTestId($"mq-status-{qualificationId}")!.TrimmedText());
        Assert.Equal(endDate is not null ? endDate.Value.ToString(UiDefaults.DateOnlyDisplayFormat) : "None", mandatoryQualificationSummary.GetElementByTestId($"mq-end-date-{qualificationId}")!.TrimmedText());
    }

    [Theory]
    [InlineData(ProfessionalStatusType.EarlyYearsProfessionalStatus)]
    [InlineData(ProfessionalStatusType.EarlyYearsTeacherStatus)]
    [InlineData(ProfessionalStatusType.PartialQualifiedTeacherStatus)]
    [InlineData(ProfessionalStatusType.QualifiedTeacherStatus)]
    public async Task Get_PersonWithRouteToProfessionalStatus_DisplaysExpectedCardTitle(ProfessionalStatusType statusType)
    {
        // Arrange
        FeatureProvider.Features.Add(FeatureNames.RoutesToProfessionalStatus);
        var status = RouteToProfessionalStatusStatus.Deferred;
        DateOnly? startDate = new DateOnly(2022, 01, 01);
        DateOnly? endDate = new DateOnly(2023, 01, 01);
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).First(r => r.ProfessionalStatusType == statusType);
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(status)
                .WithTrainingStartDate(startDate.Value)
                .WithTrainingEndDate(endDate.Value)));
        var qualificationid = person.ProfessionalStatuses.First().QualificationId;
        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/qualifications");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var noProfessionalStatuses = doc.GetElementByTestId("no-professional-statuses");
        Assert.Null(noProfessionalStatuses);

        var cardTitle = doc.GetElementByTestId($"professionalstatus-{qualificationid}")?
            .QuerySelector(".govuk-summary-card__title");
        Assert.NotNull(cardTitle);
        Assert.Equal(statusType.GetDisplayName(), cardTitle.TrimmedText());
    }

    [Fact]
    public async Task Get_PersonWithRouteToProfessionalStatusApprenticeship_DisplaysExpectedContent()
    {
        // Arrange
        FeatureProvider.Features.Add(FeatureNames.RoutesToProfessionalStatus);
        var subjects = (await ReferenceDataCache.GetTrainingSubjectsAsync()).Take(1);
        var country = (await ReferenceDataCache.GetTrainingCountriesAsync()).Take(1).First();
        var trainingProvider = (await ReferenceDataCache.GetTrainingProvidersAsync()).First();
        var degreeType = (await ReferenceDataCache.GetDegreeTypesAsync()).First();
        var status = RouteToProfessionalStatusStatus.Holds;
        DateOnly? startDate = new DateOnly(2022, 01, 01);
        DateOnly? endDate = new DateOnly(2023, 01, 01);
        DateOnly holdsFrom = new DateOnly(2024, 01, 01);
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).Single(r => r.Name == "Postgraduate Teaching Apprenticeship");
        var ageRange = TrainingAgeSpecialismType.KeyStage3;
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(status)
                .WithTrainingStartDate(startDate.Value)
                .WithTrainingEndDate(endDate.Value)
                .WithTrainingProviderId(trainingProvider.TrainingProviderId)
                .WithTrainingSubjectIds(subjects.Select(s => s.TrainingSubjectId).ToArray())
                .WithTrainingCountryId(country.CountryId)
                .WithTrainingAgeSpecialismType(ageRange)
                .WithDegreeTypeId(degreeType.DegreeTypeId)
                .WithHoldsFrom(holdsFrom)
                .WithSourceApplicationReference("TESTREFERENCE")));
        var qualificationid = person.ProfessionalStatuses.First().QualificationId;
        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/qualifications");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var noProfessionalStatuses = doc.GetElementByTestId("no-professional-statuses");
        Assert.Null(noProfessionalStatuses);

        doc.AssertRowContentMatches($"professionalstatus-{qualificationid}", "Route", route.Name);
        doc.AssertRowContentMatches($"professionalstatus-{qualificationid}", "Start date", startDate.Value.ToString(UiDefaults.DateOnlyDisplayFormat));
        doc.AssertRowContentMatches($"professionalstatus-{qualificationid}", "End date", endDate.Value.ToString(UiDefaults.DateOnlyDisplayFormat));
        doc.AssertRowContentMatches($"professionalstatus-{qualificationid}", "Professional status date", holdsFrom.ToString(UiDefaults.DateOnlyDisplayFormat));
        doc.AssertRowDoesNotExist($"professionalstatus-{qualificationid}", "Induction exemption");
        doc.AssertRowContentMatches($"professionalstatus-{qualificationid}", "Training provider", trainingProvider.Name);
        doc.AssertRowContentContains($"professionalstatus-{qualificationid}", "Degree type", degreeType.Name);
        doc.AssertRowContentMatches($"professionalstatus-{qualificationid}", "Country of training", country.Name);
        doc.AssertRowContentMatches($"professionalstatus-{qualificationid}", "Age range", ageRange.GetDisplayName());
        doc.AssertRowContentMatches($"professionalstatus-{qualificationid}", "Subjects", subjects.Select(s => $"{s.Reference} - {s.Name}"));
        doc.AssertRowContentMatches($"professionalstatus-{qualificationid}", "Source application reference", "TESTREFERENCE");
    }

    [Fact]
    public async Task Get_PersonWithRouteToProfessionalStatusGraduateTeacherProgramme_DisplaysExpectedContent()
    {
        // Arrange
        FeatureProvider.Features.Add(FeatureNames.RoutesToProfessionalStatus);
        var country = (await ReferenceDataCache.GetTrainingCountriesAsync()).Take(1).First();
        var status = RouteToProfessionalStatusStatus.InTraining;
        DateOnly? startDate = new DateOnly(2022, 01, 01);
        DateOnly? endDate = new DateOnly(2023, 01, 01);
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).Single(r => r.RouteToProfessionalStatusTypeId == RouteToProfessionalStatusType.EuropeanRecognitionId);
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(status)
                .WithTrainingStartDate(startDate.Value)
                .WithTrainingEndDate(endDate.Value)
                .WithTrainingCountryId(country.CountryId)));
        var qualificationid = person.ProfessionalStatuses.First().QualificationId;
        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/qualifications");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var noProfessionalStatuses = doc.GetElementByTestId("no-professional-statuses");
        Assert.Null(noProfessionalStatuses);

        doc.AssertRowContentMatches($"professionalstatus-{qualificationid}", "Route", route.Name);
        doc.AssertRowContentMatches($"professionalstatus-{qualificationid}", "Start date", startDate.Value.ToString(UiDefaults.DateOnlyDisplayFormat));
        doc.AssertRowContentMatches($"professionalstatus-{qualificationid}", "End date", endDate.Value.ToString(UiDefaults.DateOnlyDisplayFormat));
        doc.AssertRowDoesNotExist($"professionalstatus-{qualificationid}", "Professional status date");
        doc.AssertRowDoesNotExist($"professionalstatus-{qualificationid}", "Induction exemption");
        doc.AssertRowContentMatches($"professionalstatus-{qualificationid}", "Training provider", "Not provided");
        doc.AssertRowContentMatches($"professionalstatus-{qualificationid}", "Degree type", "Not provided");
        doc.AssertRowContentMatches($"professionalstatus-{qualificationid}", "Country of training", country.Name);
        doc.AssertRowContentMatches($"professionalstatus-{qualificationid}", "Age range", "Not provided");
        doc.AssertRowContentMatches($"professionalstatus-{qualificationid}", "Subjects", "Not provided");
        doc.AssertRowContentMatches($"professionalstatus-{qualificationid}", "Source application reference", "Not provided");
    }

    [Fact]
    public async Task Get_PersonWithRouteToProfessionalStatusNIRecognition_DisplaysExpectedContent()
    {
        // Arrange
        FeatureProvider.Features.Add(FeatureNames.RoutesToProfessionalStatus);
        var country = (await ReferenceDataCache.GetTrainingCountriesAsync()).Take(1).First();
        var status = RouteToProfessionalStatusStatus.Deferred;
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).Single(r => r.RouteToProfessionalStatusTypeId == RouteToProfessionalStatusType.NiRId);
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(status)
                .WithTrainingCountryId(country.CountryId)));
        var qualificationid = person.ProfessionalStatuses.First().QualificationId;
        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/qualifications");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var noProfessionalStatuses = doc.GetElementByTestId("no-professional-statuses");
        Assert.Null(noProfessionalStatuses);

        doc.AssertRowContentMatches($"professionalstatus-{qualificationid}", "Route", route.Name);
        doc.AssertRowDoesNotExist($"professionalstatus-{qualificationid}", "Start date");
        doc.AssertRowDoesNotExist($"professionalstatus-{qualificationid}", "End date");
        doc.AssertRowDoesNotExist($"professionalstatus-{qualificationid}", "Professional status date");
        doc.AssertRowDoesNotExist($"professionalstatus-{qualificationid}", "Induction exemption");
        doc.AssertRowDoesNotExist($"professionalstatus-{qualificationid}", "Training provider");
        doc.AssertRowDoesNotExist($"professionalstatus-{qualificationid}", "Degree type");
        doc.AssertRowContentMatches($"professionalstatus-{qualificationid}", "Country of training", country.Name);
        doc.AssertRowDoesNotExist($"professionalstatus-{qualificationid}", "Age range");
        doc.AssertRowDoesNotExist($"professionalstatus-{qualificationid}", "Subjects");
        doc.AssertRowContentMatches($"professionalstatus-{qualificationid}", "Source application reference", "Not provided");
    }

    [Fact]
    public async Task Get_PersonWithRouteToProfessionalStatusHoldsNIRecognition_DisplaysExpectedContent()
    {
        // Arrange
        FeatureProvider.Features.Add(FeatureNames.RoutesToProfessionalStatus);
        var country = (await ReferenceDataCache.GetTrainingCountriesAsync()).Take(1).First();
        var holdsFromDate = Clock.Today.AddDays(-1);
        var status = RouteToProfessionalStatusStatus.Holds;
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).Single(r => r.RouteToProfessionalStatusTypeId == RouteToProfessionalStatusType.NiRId);
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(status)
                .WithTrainingCountryId(country.CountryId)
                .WithHoldsFrom(holdsFromDate)
                .WithInductionExemption(true)));
        var qualificationid = person.ProfessionalStatuses.First().QualificationId;
        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/qualifications");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var noProfessionalStatuses = doc.GetElementByTestId("no-professional-statuses");
        Assert.Null(noProfessionalStatuses);

        doc.AssertRowContentMatches($"professionalstatus-{qualificationid}", "Route", route.Name);
        doc.AssertRowDoesNotExist($"professionalstatus-{qualificationid}", "Start date");
        doc.AssertRowDoesNotExist($"professionalstatus-{qualificationid}", "End date");
        doc.AssertRowContentMatches($"professionalstatus-{qualificationid}", "Professional status date", holdsFromDate.ToString(UiDefaults.DateOnlyDisplayFormat));
        doc.AssertRowContentMatches($"professionalstatus-{qualificationid}", "Induction exemption", "Yes");
        doc.AssertRowDoesNotExist($"professionalstatus-{qualificationid}", "Training provider");
        doc.AssertRowDoesNotExist($"professionalstatus-{qualificationid}", "Degree type");
        doc.AssertRowContentMatches($"professionalstatus-{qualificationid}", "Country of training", country.Name);
        doc.AssertRowContentMatches($"professionalstatus-{qualificationid}", "Age range", "Not provided");
        doc.AssertRowContentMatches($"professionalstatus-{qualificationid}", "Subjects", "Not provided");
        doc.AssertRowContentMatches($"professionalstatus-{qualificationid}", "Source application reference", "Not provided");
    }

    [Fact]
    public async Task Get_PersonWithRouteToProfessionalStatus_AgeRangeFromTo_DisplaysExpectedContent()
    {
        // Arrange
        FeatureProvider.Features.Add(FeatureNames.RoutesToProfessionalStatus);
        var subjects = (await ReferenceDataCache.GetTrainingSubjectsAsync()).Take(1);
        var country = (await ReferenceDataCache.GetTrainingCountriesAsync()).Take(1).First();
        var trainingProvider = (await ReferenceDataCache.GetTrainingProvidersAsync()).First();
        var degreeType = (await ReferenceDataCache.GetDegreeTypesAsync()).First();
        var status = RouteToProfessionalStatusStatus.InTraining;
        DateOnly? startDate = new DateOnly(2022, 01, 01);
        DateOnly? endDate = new DateOnly(2023, 01, 01);
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).Where(r => r.Name == "Northern Irish Recognition").Single();
        var ageFrom = 4;
        var ageTo = 11;
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(status)
                .WithTrainingAgeSpecialismRangeFrom(ageFrom)
                .WithTrainingAgeSpecialismRangeTo(ageTo)));
        var qualificationid = person.ProfessionalStatuses.First().QualificationId;
        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/qualifications");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var noProfessionalStatuses = doc.GetElementByTestId("no-professional-statuses");
        Assert.Null(noProfessionalStatuses);

        doc.AssertRowContentMatches($"professionalstatus-{qualificationid}", "Age range", $"From {ageFrom} to {ageTo}");
    }

    [Theory]
    [InlineData(UserRoles.Viewer, true)]
    [InlineData(UserRoles.AlertsManagerTra, true)]
    [InlineData(UserRoles.AlertsManagerTraDbs, true)]
    [InlineData(UserRoles.RecordManager, true)]
    [InlineData(UserRoles.AccessManager, true)]
    [InlineData(UserRoles.Administrator, true)]
    public async Task Get_RoutesPage_UserRoles_CanViewPageAsExpected(string userRole, bool canViewPage)
    {
        // Arrange
        FeatureProvider.Features.Add(FeatureNames.ContactsMigrated);
        FeatureProvider.Features.Add(FeatureNames.RoutesToProfessionalStatus);
        var user = await TestData.CreateUserAsync(role: userRole);
        SetCurrentUser(user);

        var person = await TestData.CreatePersonAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/qualifications");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(canViewPage ? System.Net.HttpStatusCode.OK : System.Net.HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Theory]
    [InlineData(UserRoles.Viewer, false)]
    [InlineData(UserRoles.AlertsManagerTra, false)]
    [InlineData(UserRoles.AlertsManagerTraDbs, false)]
    [InlineData(UserRoles.RecordManager, true)]
    [InlineData(UserRoles.AccessManager, true)]
    [InlineData(UserRoles.Administrator, true)]
    public async Task Get_RoutesPage_UserRolesWithViewOrEditRoutesPermissions_EditLinkShownAsExpected(string userRole, bool canSeeEditLinks)
    {
        // Arrange
        FeatureProvider.Features.Add(FeatureNames.ContactsMigrated);
        FeatureProvider.Features.Add(FeatureNames.RoutesToProfessionalStatus);
        var user = await TestData.CreateUserAsync(role: userRole);
        SetCurrentUser(user);

        var subjects = (await ReferenceDataCache.GetTrainingSubjectsAsync()).Take(1);
        var country = (await ReferenceDataCache.GetTrainingCountriesAsync()).Take(1).First();
        var trainingProvider = (await ReferenceDataCache.GetTrainingProvidersAsync()).First();
        var degreeType = (await ReferenceDataCache.GetDegreeTypesAsync()).First();
        var status = RouteToProfessionalStatusStatus.InTraining;
        var ageRange = TrainingAgeSpecialismType.KeyStage3;
        DateOnly? startDate = new DateOnly(2022, 01, 01);
        DateOnly? endDate = new DateOnly(2023, 01, 01);
        DateOnly holdsFrom = new DateOnly(2024, 01, 01);
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).Where(r => r.Name == "Northern Irish Recognition").Single();

        var qualificationProvider = MandatoryQualificationProvider.All.Single(p => p.Name == "University of Birmingham");
        var qualificationSpecialism = MandatoryQualificationSpecialism.Hearing;
        var qualificationStatus = MandatoryQualificationStatus.Passed;
        DateOnly? qualificationStartDate = DateOnly.Parse("2022-01-05");
        DateOnly? qualificationEndDate = DateOnly.Parse("2022-07-13");

        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(status)
                .WithTrainingStartDate(startDate.Value)
                .WithTrainingEndDate(endDate.Value)
                .WithTrainingProviderId(trainingProvider.TrainingProviderId)
                .WithTrainingSubjectIds(subjects.Select(s => s.TrainingSubjectId).ToArray())
                .WithTrainingCountryId(country.CountryId)
                .WithTrainingAgeSpecialismType(ageRange)
                .WithDegreeTypeId(degreeType.DegreeTypeId)
                .WithHoldsFrom(holdsFrom))
            .WithMandatoryQualification(q => q
                .WithProvider(qualificationProvider?.MandatoryQualificationProviderId)
                .WithSpecialism(qualificationSpecialism)
                .WithStartDate(qualificationStartDate)
                .WithStatus(qualificationStatus, qualificationEndDate)));
        var professionalStatusQualificationId = person.ProfessionalStatuses.First().QualificationId;
        var mandatoryQualificationId = person.MandatoryQualifications.First().QualificationId;

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/qualifications");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var editLinks = doc.GetAllElementsByTestId(
            "add-route",
            $"edit-route-link-{professionalStatusQualificationId}",
            $"delete-route-link-{professionalStatusQualificationId}",
            "add-mandatory-qualification",
            $"delete-link-{mandatoryQualificationId}",
            $"provider-change-link-{mandatoryQualificationId}",
            $"specialism-change-link-{mandatoryQualificationId}",
            $"start-date-change-link-{mandatoryQualificationId}",
            $"status-change-link-{mandatoryQualificationId}",
            $"end-date-change-link-{mandatoryQualificationId}");

        if (canSeeEditLinks)
        {
            Assert.NotEmpty(editLinks);
        }
        else
        {
            Assert.Empty(editLinks);
        }
    }
}
