using FormFlow;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.SupportUi.Pages.Mqs.DeleteMq;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Mqs.DeleteMq;

public class ConfirmTests : TestBase
{
    public ConfirmTests(HostFixture hostFixture)
        : base(hostFixture)
    {
    }

    [Fact]
    public async Task Get_MissingDataInJourneyState_Redirects()
    {
        // Arrange        
        var person = await TestData.CreatePerson(b => b.WithMandatoryQualification());
        var qualificationId = person.MandatoryQualifications!.First().QualificationId;
        var journeyInstance = await CreateJourneyInstance(
            qualificationId,
            new DeleteMqState()
            {
                Initialized = true,
                PersonId = person.PersonId,
                PersonName = person.Contact.ResolveFullName(includeMiddleName: false)
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/{qualificationId}/delete/confirm?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/mqs/{qualificationId}/delete?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Theory]
    [InlineData("959", "Hearing", "2021-10-05", dfeta_qualification_dfeta_MQ_Status.Passed, "2021-11-05", MqDeletionReasonOption.ProviderRequest, "Some details about the deletion reason")]
    [InlineData("959", "Hearing", "2021-10-05", dfeta_qualification_dfeta_MQ_Status.Deferred, null, MqDeletionReasonOption.ProviderRequest, null)]
    [InlineData(null, null, null, null, null, MqDeletionReasonOption.AnotherReason, null)]
    public async Task Get_ValidRequest_DisplaysContentAsExpected(
        string? providerValue,
        string? specialismValue,
        string? startDateString,
        dfeta_qualification_dfeta_MQ_Status? status,
        string? endDateString,
        MqDeletionReasonOption deletionReason,
        string? deletionReasonDetail)
    {
        // Arrange
        var mqEstablishment = !string.IsNullOrEmpty(providerValue) ? await TestData.ReferenceDataCache.GetMqEstablishmentByValue(providerValue) : null;
        var specialism = !string.IsNullOrEmpty(specialismValue) ? await TestData.ReferenceDataCache.GetMqSpecialismByValue(specialismValue) : null;
        DateOnly? startDate = !string.IsNullOrEmpty(startDateString) ? DateOnly.Parse(startDateString) : null;
        DateOnly? endDate = !string.IsNullOrEmpty(endDateString) ? DateOnly.Parse(endDateString) : null;

        var person = await TestData.CreatePerson(
            b => b.WithMandatoryQualification(
                providerValue: providerValue,
                specialismValue: specialismValue,
                startDate: startDate,
                endDate: endDate,
                result: status));
        var qualification = person.MandatoryQualifications.Single();
        var journeyInstance = await CreateJourneyInstance(
            qualification.QualificationId,
            new DeleteMqState
            {
                Initialized = true,
                PersonId = person.PersonId,
                PersonName = person.Contact.ResolveFullName(includeMiddleName: false),
                MqEstablishment = mqEstablishment is not null ? mqEstablishment.dfeta_name : null,
                Specialism = specialism is not null ? specialism.dfeta_name : null,
                Status = status,
                StartDate = startDate,
                EndDate = endDate,
                DeletionReason = deletionReason,
                DeletionReasonDetail = deletionReasonDetail
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/{qualification.QualificationId}/delete/confirm?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        var doc = await response.GetDocument();
        var deletionSummary = doc.GetElementByTestId("deletion-summary");
        Assert.NotNull(deletionSummary);
        Assert.Equal(deletionReason.GetDisplayName(), deletionSummary.GetElementByTestId("deletion-reason")!.TextContent);
        Assert.Equal(!string.IsNullOrEmpty(deletionReasonDetail) ? deletionReasonDetail : "None", deletionSummary.GetElementByTestId("deletion-reason-detail")!.TextContent);
        Assert.Equal(mqEstablishment is not null ? mqEstablishment.dfeta_name : "None", deletionSummary.GetElementByTestId("provider")!.TextContent);
        Assert.Equal(specialism is not null ? specialism.dfeta_name : "None", deletionSummary.GetElementByTestId("specialism")!.TextContent);
        Assert.Equal(status is not null ? status.Value.ToString() : "None", deletionSummary.GetElementByTestId("status")!.TextContent);
        Assert.Equal(startDate is not null ? startDate.Value.ToString("d MMMM yyyy") : "None", deletionSummary.GetElementByTestId("start-date")!.TextContent);
        Assert.Equal(endDate is not null ? endDate.Value.ToString("d MMMM yyyy") : "None", deletionSummary.GetElementByTestId("end-date")!.TextContent);
    }

    [Fact]
    public async Task Post_MissingDataInJourneyState_Redirects()
    {
        // Arrange        
        var person = await TestData.CreatePerson(b => b.WithMandatoryQualification());
        var qualificationId = person.MandatoryQualifications!.First().QualificationId;
        var journeyInstance = await CreateJourneyInstance(
            qualificationId,
            new DeleteMqState()
            {
                Initialized = true,
                PersonId = person.PersonId,
                PersonName = person.Contact.ResolveFullName(includeMiddleName: false)
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/delete/confirm?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/mqs/{qualificationId}/delete?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_Confirm_CompletesJourneyAndRedirectsWithFlashMessage()
    {
        // Arrange        
        var person = await TestData.CreatePerson(b => b.WithMandatoryQualification());
        var qualificationId = person.MandatoryQualifications!.First().QualificationId;
        var journeyInstance = await CreateJourneyInstance(
            qualificationId,
            new DeleteMqState()
            {
                Initialized = true,
                PersonId = person.PersonId,
                PersonName = person.Contact.ResolveFullName(includeMiddleName: false),
                MqEstablishment = "University of Leeds",
                Specialism = "Hearing",
                Status = dfeta_qualification_dfeta_MQ_Status.Passed,
                StartDate = new DateOnly(2023, 09, 01),
                EndDate = new DateOnly(2023, 11, 05),
                DeletionReason = MqDeletionReasonOption.ProviderRequest,
                DeletionReasonDetail = "Some details about the deletion reason",
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/delete/confirm?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var redirectResponse = await response.FollowRedirect(HttpClient);
        var redirectDoc = await redirectResponse.GetDocument();
        AssertEx.HtmlDocumentHasFlashSuccess(redirectDoc, "Mandatory qualification deleted");

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.True(journeyInstance.Completed);
    }

    [Fact]
    public async Task Post_Cancel_DeletesJourneyAndRedirects()
    {
        // Arrange
        var person = await TestData.CreatePerson(b => b.WithMandatoryQualification());
        var qualificationId = person.MandatoryQualifications!.First().QualificationId;
        var journeyInstance = await CreateJourneyInstance(
            qualificationId,
            new DeleteMqState()
            {
                Initialized = true,
                PersonId = person.PersonId,
                PersonName = person.Contact.ResolveFullName(includeMiddleName: false),
                MqEstablishment = "University of Leeds",
                Specialism = "Hearing",
                Status = dfeta_qualification_dfeta_MQ_Status.Passed,
                StartDate = new DateOnly(2023, 09, 01),
                EndDate = new DateOnly(2023, 11, 05),
                DeletionReason = MqDeletionReasonOption.ProviderRequest,
                DeletionReasonDetail = "Some details about the deletion reason",
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/delete/confirm/cancel?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Null(journeyInstance);
    }


    private async Task<JourneyInstance<DeleteMqState>> CreateJourneyInstance(Guid qualificationId, DeleteMqState? state = null) =>
        await CreateJourneyInstance(
            JourneyNames.DeleteMq,
            state ?? new DeleteMqState(),
            new KeyValuePair<string, object>("qualificationId", qualificationId));
}
