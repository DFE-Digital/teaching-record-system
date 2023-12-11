using TeachingRecordSystem.Core.Dqt.Models;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Persons.PersonDetail;

public class QualificationsTests : TestBase
{
    public QualificationsTests(HostFixture hostFixture)
        : base(hostFixture)
    {
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
        var person = await TestData.CreatePerson();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/qualifications");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        var doc = await response.GetDocument();
        var noMandatoryQualifications = doc.GetElementByTestId("no-mandatory-qualifications");
        Assert.NotNull(noMandatoryQualifications);
    }

    [Theory]
    [InlineData("959", "Hearing", "2022-01-05", dfeta_qualification_dfeta_MQ_Status.Passed, "2022-07-13")]
    [InlineData("959", "Hearing", "2022-01-05", dfeta_qualification_dfeta_MQ_Status.Deferred, null)]
    [InlineData(null, null, null, null, null)]
    public async Task Get_WithPersonIdForPersonWithMandatoryQualifications_DisplaysExpectedContent(
        string? providerValue,
        string? specialismValue,
        string? startDateString,
        dfeta_qualification_dfeta_MQ_Status? result,
        string? endDateString)
    {
        // Arrange
        var mqEstablishment = !string.IsNullOrEmpty(providerValue) ? await TestData.ReferenceDataCache.GetMqEstablishmentByValue(providerValue) : null;
        var specialism = !string.IsNullOrEmpty(specialismValue) ? await TestData.ReferenceDataCache.GetMqSpecialismByValue(specialismValue) : null;
        DateOnly? startDate = !string.IsNullOrEmpty(startDateString) ? DateOnly.Parse(startDateString) : null;
        DateOnly? endDate = !string.IsNullOrEmpty(endDateString) ? DateOnly.Parse(endDateString) : null;
        var person = await TestData.CreatePerson(
            x => x.WithQts(qtsDate: new DateOnly(2021, 10, 5))
                    .WithMandatoryQualification(providerValue, specialismValue, startDate, endDate, result));
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;

        var request = new HttpRequestMessage(HttpMethod.Get, $"/persons/{person.ContactId}/qualifications");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        var doc = await response.GetDocument();
        var noMandatoryQualifications = doc.GetElementByTestId("no-mandatory-qualifications");
        Assert.Null(noMandatoryQualifications);

        var mandatoryQualificationSummary = doc.GetElementByTestId($"mq-{qualificationId}");
        Assert.NotNull(mandatoryQualificationSummary);
        Assert.Equal(mqEstablishment is not null ? mqEstablishment.dfeta_name : "None", mandatoryQualificationSummary.GetElementByTestId($"mq-provider-{qualificationId}")!.TextContent);
        Assert.Equal(specialism is not null ? specialism.dfeta_name : "None", mandatoryQualificationSummary.GetElementByTestId($"mq-specialism-{qualificationId}")!.TextContent);
        Assert.Equal(startDate is not null ? startDate.Value.ToString("d MMMM yyyy") : "None", mandatoryQualificationSummary.GetElementByTestId($"mq-start-date-{qualificationId}")!.TextContent);
        Assert.Equal(result is not null ? result.Value.ToString() : "None", mandatoryQualificationSummary.GetElementByTestId($"mq-result-{qualificationId}")!.TextContent);
        Assert.Equal(endDate is not null ? endDate.Value.ToString("d MMMM yyyy") : "None", mandatoryQualificationSummary.GetElementByTestId($"mq-end-date-{qualificationId}")!.TextContent);
    }
}