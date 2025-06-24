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
        var professionalStatuses = doc.GetElementByTestId("no-professional-statuses");
        Assert.NotNull(professionalStatuses);
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

    [Fact]
    public async Task Get_PersonWithRouteToProfessionalStatus_DisplaysExpectedContent()
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
        DateOnly holdsFrom = new DateOnly(2024, 01, 01);
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).Where(r => r.Name == "NI R").Single();
        var ageRange = TrainingAgeSpecialismType.KeyStage3;
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r =>
            {
                r.WithRouteType(route.RouteToProfessionalStatusTypeId);
                r.WithStatus(status);
                r.WithTrainingStartDate(startDate.Value);
                r.WithTrainingEndDate(endDate.Value);
                r.WithTrainingProviderId(trainingProvider.TrainingProviderId);
                r.WithTrainingSubjectIds(subjects.Select(s => s.TrainingSubjectId).ToArray());
                r.WithTrainingCountryId(country.CountryId);
                r.WithTrainingAgeSpecialismType(ageRange);
                r.WithDegreeTypeId(degreeType.DegreeTypeId);
                r.WithHoldsFrom(holdsFrom);
            })
        );
        var qualificationid = person.ProfessionalStatuses.First().QualificationId;
        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/qualifications");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var professionalStatuses = doc.GetElementByTestId("no-professional-statuses");
        Assert.Null(professionalStatuses);

        var professionalStatus = doc.GetElementByTestId($"professionalstatus-id-{qualificationid}");
        Assert.NotNull(professionalStatus);
        Assert.Equal(route.Name, professionalStatus.GetElementByTestId($"route-type-{qualificationid}")!.TrimmedText());
        Assert.Equal(startDate.Value.ToString(UiDefaults.DateOnlyDisplayFormat), professionalStatus.GetElementByTestId($"training-start-date-{qualificationid}")!.TrimmedText());
        Assert.Equal(endDate.Value.ToString(UiDefaults.DateOnlyDisplayFormat), professionalStatus.GetElementByTestId($"training-end-date-{qualificationid}")!.TrimmedText());
        Assert.Equal(holdsFrom.ToString(UiDefaults.DateOnlyDisplayFormat), professionalStatus.GetElementByTestId($"award-date-{qualificationid}")!.TrimmedText());
        Assert.Equal(trainingProvider.Name, professionalStatus.GetElementByTestId($"training-provider-{qualificationid}")!.TrimmedText());
        Assert.Contains(degreeType.Name, professionalStatus.GetElementByTestId($"training-degreetype-{qualificationid}")!.TrimmedText());
        Assert.Equal(country.Name, professionalStatus.GetElementByTestId($"training-country-{qualificationid}")!.TrimmedText());
        Assert.Equal(ageRange.GetDisplayName(), professionalStatus.GetElementByTestId($"training-agespecialism-{qualificationid}")!.TrimmedText());
        doc.AssertRowContentMatches("Subjects", subjects.Select(s => $"{s.Reference} - {s.Name}"));
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
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).Where(r => r.Name == "NI R").Single();
        var ageFrom = 4;
        var ageTo = 11;
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r =>
            {
                r.WithRouteType(route.RouteToProfessionalStatusTypeId);
                r.WithStatus(status);
                r.WithTrainingAgeSpecialismRangeFrom(ageFrom);
                r.WithTrainingAgeSpecialismRangeTo(ageTo);
            })
        );
        var qualificationid = person.ProfessionalStatuses.First().QualificationId;
        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/qualifications");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var professionalStatuses = doc.GetElementByTestId("no-professional-statuses");
        Assert.Null(professionalStatuses);

        var professionalStatus = doc.GetElementByTestId($"professionalstatus-id-{qualificationid}");
        Assert.NotNull(professionalStatus);
        Assert.Equal($"From {ageFrom} to {ageTo}", professionalStatus.GetElementByTestId($"training-agespecialism-{qualificationid}")!.TrimmedText());
    }

    [Theory]
    [InlineData(RouteToProfessionalStatusStatus.InTraining, null, "Not provided")]
    public async Task Get_PersonWithRouteToProfessionalStatus_RouteAllowsInductionExemption_DisplaysExpectedContent(RouteToProfessionalStatusStatus status, bool? isExempt, string expectedContent)
    {
        // Arrange
        FeatureProvider.Features.Add(FeatureNames.RoutesToProfessionalStatus);
        DateOnly? startDate = new DateOnly(2022, 01, 01);
        DateOnly? endDate = new DateOnly(2023, 01, 01);
        DateOnly holdsFrom = new DateOnly(2024, 01, 01);
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).Where(r => r.Name == "NI R").Single();
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r =>
            {
                r.WithRouteType(route.RouteToProfessionalStatusTypeId);
                r.WithStatus(status);
                r.WithTrainingStartDate(startDate.Value);
                r.WithTrainingEndDate(endDate.Value);
                r.WithHoldsFrom(holdsFrom);
                r.WithInductionExemption(isExempt);
            })
        );
        var qualificationid = person.ProfessionalStatuses.First().QualificationId;
        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/qualifications");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var professionalStatuses = doc.GetElementByTestId("no-professional-statuses");
        Assert.Null(professionalStatuses);

        var professionalStatus = doc.GetElementByTestId($"professionalstatus-id-{qualificationid}");
        Assert.NotNull(professionalStatus);
        Assert.Equal(expectedContent, professionalStatus.GetElementByTestId($"training-exemption-{qualificationid}")!.TrimmedText());
    }

    [Theory]
    [InlineData(UserRoles.AccessManager, false)]
    [InlineData(UserRoles.Viewer, true)]
    [InlineData(UserRoles.AlertsManagerTra, true)]
    [InlineData(UserRoles.AlertsManagerTraDbs, true)]
    [InlineData(UserRoles.RecordManager, true)]
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
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).Where(r => r.Name == "NI R").Single();

        var qualificationProvider = MandatoryQualificationProvider.All.Single(p => p.Name == "University of Birmingham");
        var qualificationSpecialism = MandatoryQualificationSpecialism.Hearing;
        var qualificationStatus = MandatoryQualificationStatus.Passed;
        DateOnly? qualificationStartDate = DateOnly.Parse("2022-01-05");
        DateOnly? qualificationEndDate = DateOnly.Parse("2022-07-13");

        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r =>
            {
                r.WithRouteType(route.RouteToProfessionalStatusTypeId);
                r.WithStatus(status);
                r.WithTrainingStartDate(startDate.Value);
                r.WithTrainingEndDate(endDate.Value);
                r.WithTrainingProviderId(trainingProvider.TrainingProviderId);
                r.WithTrainingSubjectIds(subjects.Select(s => s.TrainingSubjectId).ToArray());
                r.WithTrainingCountryId(country.CountryId);
                r.WithTrainingAgeSpecialismType(ageRange);
                r.WithDegreeTypeId(degreeType.DegreeTypeId);
                r.WithHoldsFrom(holdsFrom);
            })
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
