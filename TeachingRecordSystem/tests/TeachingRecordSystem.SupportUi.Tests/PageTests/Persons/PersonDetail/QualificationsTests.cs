using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail;

public class QualificationsTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Test]
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

    [Test]
    public async Task Get_WithPersonIdForPersonWithNoMandatoryQualifications_DisplaysNoMandatoryQualifications()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/qualifications");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var noMandatoryQualifications = doc.GetElementByTestId("no-mandatory-qualifications");
        Assert.NotNull(noMandatoryQualifications);
    }

    [Test]
    public async Task Get_WithPersonIdForPersonWithProfessionalStatuses_DisplaysNoProfessionalStatuses()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/qualifications");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var noProfessionalStatuses = doc.GetElementByTestId("no-professional-statuses");
        Assert.NotNull(noProfessionalStatuses);
    }

    [Test]
    [Arguments("University of Birmingham", MandatoryQualificationSpecialism.Hearing, "2022-01-05", MandatoryQualificationStatus.Passed, "2022-07-13")]
    [Arguments("University of Birmingham", MandatoryQualificationSpecialism.Hearing, "2022-01-05", MandatoryQualificationStatus.Deferred, null)]
    [Arguments(null, null, null, null, null)]
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

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/qualifications");

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

    [Test]
    [Arguments(ProfessionalStatusType.EarlyYearsProfessionalStatus)]
    [Arguments(ProfessionalStatusType.EarlyYearsTeacherStatus)]
    [Arguments(ProfessionalStatusType.PartialQualifiedTeacherStatus)]
    [Arguments(ProfessionalStatusType.QualifiedTeacherStatus)]
    public async Task Get_PersonWithRouteToProfessionalStatus_DisplaysExpectedCardTitle(ProfessionalStatusType statusType)
    {
        // Arrange
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
        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/qualifications");

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

    [Test]
    public async Task Get_PersonWithRouteToProfessionalStatusApprenticeship_DisplaysExpectedContent()
    {
        // Arrange
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
        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/qualifications");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var noProfessionalStatuses = doc.GetElementByTestId("no-professional-statuses");
        Assert.Null(noProfessionalStatuses);

        doc.AssertSummaryListRowValueContentMatches($"professionalstatus-{qualificationid}", "Route", route.Name);
        doc.AssertSummaryListRowValueContentMatches($"professionalstatus-{qualificationid}", "Start date", startDate.Value.ToString(UiDefaults.DateOnlyDisplayFormat));
        doc.AssertSummaryListRowValueContentMatches($"professionalstatus-{qualificationid}", "End date", endDate.Value.ToString(UiDefaults.DateOnlyDisplayFormat));
        doc.AssertSummaryListRowValueContentMatches($"professionalstatus-{qualificationid}", "Held since", holdsFrom.ToString(UiDefaults.DateOnlyDisplayFormat));
        doc.AssertSummaryListRowDoesNotExist($"professionalstatus-{qualificationid}", "Induction exemption");
        doc.AssertSummaryListRowValueContentMatches($"professionalstatus-{qualificationid}", "Training provider", trainingProvider.Name);
        doc.AssertSummaryListRowContentContains($"professionalstatus-{qualificationid}", "Degree type", degreeType.Name);
        doc.AssertSummaryListRowValueContentMatches($"professionalstatus-{qualificationid}", "Country of training", country.Name);
        doc.AssertSummaryListRowValueContentMatches($"professionalstatus-{qualificationid}", "Age range", ageRange.GetDisplayName());
        doc.AssertSummaryListRowValueContentMatches($"professionalstatus-{qualificationid}", "Subjects", subjects.Select(s => $"{s.Reference} - {s.Name}"));
        doc.AssertSummaryListRowValueContentMatches($"professionalstatus-{qualificationid}", "Source application reference", "TESTREFERENCE");
    }

    [Test]
    public async Task Get_PersonWithRouteToProfessionalStatusGraduateTeacherProgramme_DisplaysExpectedContent()
    {
        // Arrange
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
        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/qualifications");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var noProfessionalStatuses = doc.GetElementByTestId("no-professional-statuses");
        Assert.Null(noProfessionalStatuses);

        doc.AssertSummaryListRowValueContentMatches($"professionalstatus-{qualificationid}", "Route", route.Name);
        doc.AssertSummaryListRowValueContentMatches($"professionalstatus-{qualificationid}", "Start date", startDate.Value.ToString(UiDefaults.DateOnlyDisplayFormat));
        doc.AssertSummaryListRowValueContentMatches($"professionalstatus-{qualificationid}", "End date", endDate.Value.ToString(UiDefaults.DateOnlyDisplayFormat));
        doc.AssertSummaryListRowDoesNotExist($"professionalstatus-{qualificationid}", "Held since");
        doc.AssertSummaryListRowDoesNotExist($"professionalstatus-{qualificationid}", "Induction exemption");
        doc.AssertSummaryListRowValueContentMatches($"professionalstatus-{qualificationid}", "Training provider", "Not provided");
        doc.AssertSummaryListRowValueContentMatches($"professionalstatus-{qualificationid}", "Degree type", "Not provided");
        doc.AssertSummaryListRowValueContentMatches($"professionalstatus-{qualificationid}", "Country of training", country.Name);
        doc.AssertSummaryListRowValueContentMatches($"professionalstatus-{qualificationid}", "Age range", "Not provided");
        doc.AssertSummaryListRowValueContentMatches($"professionalstatus-{qualificationid}", "Subjects", "Not provided");
        doc.AssertSummaryListRowValueContentMatches($"professionalstatus-{qualificationid}", "Source application reference", "Not provided");
    }

    [Test]
    public async Task Get_PersonWithRouteToProfessionalStatusNIRecognition_DisplaysExpectedContent()
    {
        // Arrange
        var country = (await ReferenceDataCache.GetTrainingCountriesAsync()).Take(1).First();
        var status = RouteToProfessionalStatusStatus.Deferred;
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).Single(r => r.RouteToProfessionalStatusTypeId == RouteToProfessionalStatusType.NiRId);
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(status)
                .WithTrainingCountryId(country.CountryId)));
        var qualificationid = person.ProfessionalStatuses.First().QualificationId;
        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/qualifications");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var noProfessionalStatuses = doc.GetElementByTestId("no-professional-statuses");
        Assert.Null(noProfessionalStatuses);

        doc.AssertSummaryListRowValueContentMatches($"professionalstatus-{qualificationid}", "Route", route.Name);
        doc.AssertSummaryListRowDoesNotExist($"professionalstatus-{qualificationid}", "Start date");
        doc.AssertSummaryListRowDoesNotExist($"professionalstatus-{qualificationid}", "End date");
        doc.AssertSummaryListRowDoesNotExist($"professionalstatus-{qualificationid}", "Held since");
        doc.AssertSummaryListRowDoesNotExist($"professionalstatus-{qualificationid}", "Induction exemption");
        doc.AssertSummaryListRowDoesNotExist($"professionalstatus-{qualificationid}", "Training provider");
        doc.AssertSummaryListRowDoesNotExist($"professionalstatus-{qualificationid}", "Degree type");
        doc.AssertSummaryListRowValueContentMatches($"professionalstatus-{qualificationid}", "Country of training", country.Name);
        doc.AssertSummaryListRowDoesNotExist($"professionalstatus-{qualificationid}", "Age range");
        doc.AssertSummaryListRowDoesNotExist($"professionalstatus-{qualificationid}", "Subjects");
        doc.AssertSummaryListRowValueContentMatches($"professionalstatus-{qualificationid}", "Source application reference", "Not provided");
    }

    [Test]
    public async Task Get_PersonWithRouteToProfessionalStatusHoldsNIRecognition_DisplaysExpectedContent()
    {
        // Arrange
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
        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/qualifications");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var noProfessionalStatuses = doc.GetElementByTestId("no-professional-statuses");
        Assert.Null(noProfessionalStatuses);

        doc.AssertSummaryListRowValueContentMatches($"professionalstatus-{qualificationid}", "Route", route.Name);
        doc.AssertSummaryListRowDoesNotExist($"professionalstatus-{qualificationid}", "Start date");
        doc.AssertSummaryListRowDoesNotExist($"professionalstatus-{qualificationid}", "End date");
        doc.AssertSummaryListRowValueContentMatches($"professionalstatus-{qualificationid}", "Held since", holdsFromDate.ToString(UiDefaults.DateOnlyDisplayFormat));
        doc.AssertSummaryListRowValueContentMatches($"professionalstatus-{qualificationid}", "Induction exemption", "Yes");
        doc.AssertSummaryListRowDoesNotExist($"professionalstatus-{qualificationid}", "Training provider");
        doc.AssertSummaryListRowDoesNotExist($"professionalstatus-{qualificationid}", "Degree type");
        doc.AssertSummaryListRowValueContentMatches($"professionalstatus-{qualificationid}", "Country of training", country.Name);
        doc.AssertSummaryListRowValueContentMatches($"professionalstatus-{qualificationid}", "Age range", "Not provided");
        doc.AssertSummaryListRowValueContentMatches($"professionalstatus-{qualificationid}", "Subjects", "Not provided");
        doc.AssertSummaryListRowValueContentMatches($"professionalstatus-{qualificationid}", "Source application reference", "Not provided");
    }

    [Test]
    public async Task Get_PersonWithRouteToProfessionalStatus_AgeRangeFromTo_DisplaysExpectedContent()
    {
        // Arrange
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
        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/qualifications");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var noProfessionalStatuses = doc.GetElementByTestId("no-professional-statuses");
        Assert.Null(noProfessionalStatuses);

        doc.AssertSummaryListRowValueContentMatches($"professionalstatus-{qualificationid}", "Age range", $"From {ageFrom} to {ageTo}");
    }

    [Test]
    [Arguments(UserRoles.Viewer, true)]
    [Arguments(UserRoles.AlertsManagerTra, true)]
    [Arguments(UserRoles.AlertsManagerTraDbs, true)]
    [Arguments(UserRoles.RecordManager, true)]
    [Arguments(UserRoles.AccessManager, true)]
    [Arguments(UserRoles.Administrator, true)]
    public async Task Get_UserRoles_CanViewPageAsExpected(string userRole, bool canViewPage)
    {
        // Arrange
        var user = await TestData.CreateUserAsync(role: userRole);
        SetCurrentUser(user);

        var person = await TestData.CreatePersonAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.PersonId}/qualifications");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(canViewPage ? System.Net.HttpStatusCode.OK : System.Net.HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Test]
    [Arguments(UserRoles.Viewer, false)]
    [Arguments(UserRoles.AlertsManagerTra, false)]
    [Arguments(UserRoles.AlertsManagerTraDbs, false)]
    [Arguments(UserRoles.RecordManager, true)]
    [Arguments(UserRoles.AccessManager, true)]
    [Arguments(UserRoles.Administrator, true)]
    public async Task Get_UserRolesWithViewOrEditRoutesPermissions_EditLinksShownAsExpected(string userRole, bool canSeeEditLinks)
    {
        // Arrange
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

    [Test]
    [Arguments(PersonStatus.Active, true)]
    [Arguments(PersonStatus.Deactivated, false)]
    public async Task Get_PersonStatus_EditLinksShownAsExpected(PersonStatus personStatus, bool canSeeEditLinks)
    {
        // Arrange
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

        if (personStatus == PersonStatus.Deactivated)
        {
            await WithDbContext(async dbContext =>
            {
                dbContext.Attach(person.Person);
                person.Person.Status = PersonStatus.Deactivated;
                await dbContext.SaveChangesAsync();
            });
        }

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
