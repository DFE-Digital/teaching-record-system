using AngleSharp.Html.Dom;
using FormFlow;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Provider;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Mqs.EditMq.Provider;

public class IndexTests : TestBase
{
    public IndexTests(HostFixture hostFixture)
        : base(hostFixture)
    {
    }

    [Fact]
    public async Task Get_WithQualificationIdForNonExistentQualification_ReturnsNotFound()
    {
        // Arrange
        var qualificationId = Guid.NewGuid();
        var journeyInstance = await CreateJourneyInstance(qualificationId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/{qualificationId}/provider?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_ValidRequestWithUninitializedJourneyState_PopulatesModelFromDatabase()
    {
        // Arrange        
        var databaseMqEstablishmentValue = "955"; // University of Birmingham
        var person = await TestData.CreatePerson(b => b.WithMandatoryQualification(q => q.WithDqtMqEstablishmentValue(databaseMqEstablishmentValue)));
        var qualificationId = person.MandatoryQualifications!.First().QualificationId;
        var journeyInstance = await CreateJourneyInstance(qualificationId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/{qualificationId}/provider?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        var doc = await response.GetDocument();
        var selectedProvider = doc.GetElementById("MqEstablishmentValue") as IHtmlSelectElement;
        Assert.NotNull(selectedProvider);
        Assert.Equal(databaseMqEstablishmentValue, selectedProvider.Value);
    }

    [Fact]
    public async Task Get_ValidRequestWithInitializedJourneyState_PopulatesModelFromJourneyState()
    {
        // Arrange        
        var databaseMqEstablishmentValue = "955"; // University of Birmingham
        var journeyMqEstablishmentValue = "959"; // University of Leeds
        var person = await TestData.CreatePerson(b => b.WithMandatoryQualification(q => q.WithDqtMqEstablishmentValue(databaseMqEstablishmentValue)));
        var qualificationId = person.MandatoryQualifications!.First().QualificationId;
        var journeyInstance = await CreateJourneyInstance(
            qualificationId,
            new EditMqProviderState()
            {
                Initialized = true,
                PersonId = person.PersonId,
                PersonName = person.Contact.ResolveFullName(includeMiddleName: false),
                MqEstablishmentValue = journeyMqEstablishmentValue
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/{qualificationId}/provider?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        var doc = await response.GetDocument();
        var selectedProvider = doc.GetElementById("MqEstablishmentValue") as IHtmlSelectElement;
        Assert.NotNull(selectedProvider);
        Assert.Equal(journeyMqEstablishmentValue, selectedProvider.Value);
    }

    [Fact]
    public async Task Post_WithQualificationIdForNonExistentQualification_ReturnsNotFound()
    {
        // Arrange
        var qualificationId = Guid.NewGuid();
        var mqEstablishmentValue = "959"; // University of Leeds
        var journeyInstance = await CreateJourneyInstance(qualificationId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/provider?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "MqEstablishmentValue", mqEstablishmentValue }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_WhenNoProviderIsSelected_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePerson(
            b => b.WithMandatoryQualification());
        var qualificationId = person.MandatoryQualifications!.First().QualificationId;
        var journeyInstance = await CreateJourneyInstance(qualificationId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/provider?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasError(response, "MqEstablishmentValue", "Select a training provider");
    }

    [Fact]
    public async Task Post_WhenProviderIsSelected_RedirectsToConfirmPage()
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
                MqEstablishmentValue = oldMqEstablishmentValue
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/provider?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "MqEstablishmentValue", newMqEstablishmentValue }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/mqs/{qualificationId}/provider/confirm?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_Cancel_DeletesJourneyAndRedirects()
    {
        // Arrange
        var mqEstablishmentValue = "955"; // University of Birmingham
        var person = await TestData.CreatePerson(
            b => b.WithMandatoryQualification());
        var qualificationId = person.MandatoryQualifications!.First().QualificationId;
        var journeyInstance = await CreateJourneyInstance(
            qualificationId,
            new EditMqProviderState()
            {
                Initialized = true,
                PersonId = person.PersonId,
                PersonName = person.Contact.ResolveFullName(includeMiddleName: false),
                MqEstablishmentValue = mqEstablishmentValue
            });
        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/provider/cancel?{journeyInstance.GetUniqueIdQueryParameter()}")
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
