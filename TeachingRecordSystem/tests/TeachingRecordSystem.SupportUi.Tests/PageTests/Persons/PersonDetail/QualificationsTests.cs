using TeachingRecordSystem.Core.DataStore.Postgres.Models;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail;

public class QualificationsTests(HostFixture hostFixture) : TestBase(hostFixture)
{
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
        Assert.Equal(provider?.Name ?? "None", mandatoryQualificationSummary.GetElementByTestId($"mq-provider-{qualificationId}")!.TextContent);
        Assert.Equal(specialism?.GetTitle() ?? "None", mandatoryQualificationSummary.GetElementByTestId($"mq-specialism-{qualificationId}")!.TextContent);
        Assert.Equal(startDate is not null ? startDate.Value.ToString(UiDefaults.DateOnlyDisplayFormat) : "None", mandatoryQualificationSummary.GetElementByTestId($"mq-start-date-{qualificationId}")!.TextContent);
        Assert.Equal(status is not null ? status.Value.ToString() : "None", mandatoryQualificationSummary.GetElementByTestId($"mq-status-{qualificationId}")!.TextContent);
        Assert.Equal(endDate is not null ? endDate.Value.ToString(UiDefaults.DateOnlyDisplayFormat) : "None", mandatoryQualificationSummary.GetElementByTestId($"mq-end-date-{qualificationId}")!.TextContent);
    }

    [Fact]
    public async Task Get_WithPersonIdForPersonWithProfessionalStatus_DisplaysExpectedContent()
    {
        // Arrange
        FeatureProvider.Features.Add(FeatureNames.RoutesToProfessionalStatus);
        var subjects = (await ReferenceDataCache.GetTrainingSubjectsAsync()).Take(1);
        var country = (await ReferenceDataCache.GetTrainingCountriesAsync()).Take(1).First();
        var trainingProvider = (await ReferenceDataCache.GetTrainingProvidersAsync()).First();
        var status = ProfessionalStatusStatus.InTraining;
        DateOnly? startDate = new DateOnly(2022, 01, 01);
        DateOnly? endDate = new DateOnly(2023, 01, 01);
        DateOnly awardedDate = new DateOnly(2024, 01, 01);
        var route = (await ReferenceDataCache.GetRoutesToProfessionalStatusAsync()).Where(r => r.Name == "NI R").Single();
        var ageRange = TrainingAgeSpecialismType.KeyStage3;
        var person = await TestData.CreatePersonAsync(p => p
            .WithProfessionalStatus(r =>
            {
                r.WithRoute(route.RouteToProfessionalStatusId);
                r.WithStatus(status);
                r.WithTrainingStartDate(startDate.Value);
                r.WithTrainingEndDate(endDate.Value);
                r.WithTrainingProvider(trainingProvider);
                r.WithTrainingSubjectIds(subjects.Select(s => s.TrainingSubjectId).ToArray());
                r.WithTrainingCountry(country);
                r.WithTrainingAgeSpecialismType(ageRange);
                r.WithAwardedDate(awardedDate);
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
        Assert.Equal(route.Name, professionalStatus.GetElementByTestId($"training-route-{qualificationid}")!.TextContent);
        Assert.Equal(startDate.Value.ToString(UiDefaults.DateOnlyDisplayFormat), professionalStatus.GetElementByTestId($"training-start-date-{qualificationid}")!.TextContent);
        Assert.Equal(endDate.Value.ToString(UiDefaults.DateOnlyDisplayFormat), professionalStatus.GetElementByTestId($"training-end-date-{qualificationid}")!.TextContent);
        Assert.Equal(awardedDate.ToString(UiDefaults.DateOnlyDisplayFormat), professionalStatus.GetElementByTestId($"award-date-{qualificationid}")!.TextContent);
        Assert.Equal("No", professionalStatus.GetElementByTestId($"training-exemption-{qualificationid}")!.TextContent);
        Assert.Equal(trainingProvider.Name, professionalStatus.GetElementByTestId($"training-provider-{qualificationid}")!.TextContent);
        Assert.Empty(professionalStatus.GetElementByTestId($"training-degreetype-{qualificationid}")!.TextContent); // TODO - don't have degree type yet
        Assert.Equal(country.Name, professionalStatus.GetElementByTestId($"training-country-{qualificationid}")!.TextContent);
        Assert.Equal(ageRange.ToString(), professionalStatus.GetElementByTestId($"training-agespecialism-{qualificationid}")!.TextContent);
        doc.AssertRowContentMatches("Subjects", subjects.Select(s => s.Name));
    }
}
