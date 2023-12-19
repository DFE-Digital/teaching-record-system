using FormFlow;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Provider;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Mqs.EditMq.Provider;

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
            new EditMqProviderState()
            {
                Initialized = true,
                PersonId = person.PersonId,
                PersonName = person.Contact.ResolveFullName(includeMiddleName: false)
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/{qualificationId}/provider/confirm?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/mqs/{qualificationId}/provider?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_ValidRequest_DisplaysContentAsExpected()
    {
        // Arrange
        var oldMqEstablishmentValue = "955"; // University of Birmingham
        var oldMqEstablishment = await TestData.ReferenceDataCache.GetMqEstablishmentByValue(oldMqEstablishmentValue);
        var newMqEstablishmentValue = "959"; // University of Leeds
        var newMqEstablishment = await TestData.ReferenceDataCache.GetMqEstablishmentByValue(newMqEstablishmentValue);
        var person = await TestData.CreatePerson(b => b.WithMandatoryQualification(q => q.WithDqtMqEstablishmentValue(oldMqEstablishmentValue)));
        var qualificationId = person.MandatoryQualifications!.First().QualificationId;
        var journeyInstance = await CreateJourneyInstance(
            qualificationId,
            new EditMqProviderState()
            {
                Initialized = true,
                PersonId = person.PersonId,
                PersonName = person.Contact.ResolveFullName(includeMiddleName: false),
                MqEstablishmentValue = newMqEstablishmentValue,
                CurrentMqEstablishmentName = oldMqEstablishment.dfeta_name,
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/{qualificationId}/provider/confirm?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        var doc = await response.GetDocument();
        var changeDetails = doc.GetElementByTestId("change-details");
        Assert.NotNull(changeDetails);
        Assert.Equal(oldMqEstablishment.dfeta_name, changeDetails.GetElementByTestId("current-value")!.TextContent);
        Assert.Equal(newMqEstablishment.dfeta_name, changeDetails.GetElementByTestId("new-value")!.TextContent);
    }

    [Fact]
    public async Task Post_MissingDataInJourneyState_Redirects()
    {
        // Arrange        
        var person = await TestData.CreatePerson(b => b.WithMandatoryQualification());
        var qualificationId = person.MandatoryQualifications!.First().QualificationId;
        var journeyInstance = await CreateJourneyInstance(
            qualificationId,
            new EditMqProviderState()
            {
                Initialized = true,
                PersonId = person.PersonId,
                PersonName = person.Contact.ResolveFullName(includeMiddleName: false)
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/provider/confirm?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/mqs/{qualificationId}/provider?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_Confirm_CompletesJourneyAndRedirectsWithFlashMessage()
    {
        // Arrange
        var oldMqEstablishmentValue = "955"; // University of Birmingham
        var newMqEstablishmentValue = "959"; // University of Leeds
        var person = await TestData.CreatePerson(b => b.WithMandatoryQualification(q => q.WithDqtMqEstablishmentValue(oldMqEstablishmentValue)));
        var qualificationId = person.MandatoryQualifications!.First().QualificationId;
        var journeyInstance = await CreateJourneyInstance(
            qualificationId,
            new EditMqProviderState()
            {
                Initialized = true,
                PersonId = person.PersonId,
                PersonName = person.Contact.ResolveFullName(includeMiddleName: false),
                MqEstablishmentValue = newMqEstablishmentValue
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/provider/confirm?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var redirectResponse = await response.FollowRedirect(HttpClient);
        var redirectDoc = await redirectResponse.GetDocument();
        AssertEx.HtmlDocumentHasFlashSuccess(redirectDoc, "Mandatory qualification changed");

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.True(journeyInstance.Completed);
    }

    [Fact]
    public async Task Post_Cancel_DeletesJourneyAndRedirects()
    {
        // Arrange
        var oldMqEstablishmentValue = "955"; // University of Birmingham
        var newMqEstablishmentValue = "959"; // University of Leeds
        var person = await TestData.CreatePerson(b => b.WithMandatoryQualification(q => q.WithDqtMqEstablishmentValue(oldMqEstablishmentValue)));
        var qualificationId = person.MandatoryQualifications!.First().QualificationId;
        var journeyInstance = await CreateJourneyInstance(
            qualificationId,
            new EditMqProviderState()
            {
                Initialized = true,
                PersonId = person.PersonId,
                PersonName = person.Contact.ResolveFullName(includeMiddleName: false),
                MqEstablishmentValue = newMqEstablishmentValue
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/provider/confirm/cancel?{journeyInstance.GetUniqueIdQueryParameter()}")
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

    private async Task<JourneyInstance<EditMqProviderState>> CreateJourneyInstance(Guid qualificationId, EditMqProviderState? state = null) =>
        await CreateJourneyInstance(
            JourneyNames.EditMqProvider,
            state ?? new EditMqProviderState(),
            new KeyValuePair<string, object>("qualificationId", qualificationId));
}
