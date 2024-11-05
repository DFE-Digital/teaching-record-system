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
        var person = await TestData.CreatePerson();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/qualifications");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponse(response);
        var noMandatoryQualifications = doc.GetElementByTestId("no-mandatory-qualifications");
        Assert.NotNull(noMandatoryQualifications);
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

        var person = await TestData.CreatePerson(x => x
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
        var doc = await AssertEx.HtmlResponse(response);
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
}
