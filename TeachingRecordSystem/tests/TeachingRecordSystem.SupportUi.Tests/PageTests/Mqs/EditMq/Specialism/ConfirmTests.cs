using FormFlow;
using TeachingRecordSystem.Core.Dqt.Models;
using TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Specialism;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Mqs.EditMq.Specialism;

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
            new EditMqSpecialismState()
            {
                Initialized = true,
                PersonId = person.PersonId,
                PersonName = person.Contact.ResolveFullName(includeMiddleName: false)
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/{qualificationId}/specialism/confirm?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/mqs/{qualificationId}/specialism?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_ValidRequest_DisplaysContentAsExpected()
    {
        // Arrange
        var oldMqSpecialism = MandatoryQualificationSpecialism.Hearing;
        var newMqSpecialism = MandatoryQualificationSpecialism.Visual;
        var person = await TestData.CreatePerson(b => b.WithMandatoryQualification(q => q.WithSpecialism(oldMqSpecialism)));
        var qualificationId = person.MandatoryQualifications!.First().QualificationId;
        var journeyInstance = await CreateJourneyInstance(
            qualificationId,
            new EditMqSpecialismState()
            {
                Initialized = true,
                PersonId = person.PersonId,
                PersonName = person.Contact.ResolveFullName(includeMiddleName: false),
                Specialism = newMqSpecialism,
                CurrentSpecialism = oldMqSpecialism,
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/{qualificationId}/specialism/confirm?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status200OK, (int)response.StatusCode);

        var doc = await response.GetDocument();
        var changeDetails = doc.GetElementByTestId("change-details");
        Assert.NotNull(changeDetails);
        Assert.Equal(oldMqSpecialism.GetTitle(), changeDetails.GetElementByTestId("current-value")!.TextContent);
        Assert.Equal(newMqSpecialism.GetTitle(), changeDetails.GetElementByTestId("new-value")!.TextContent);
    }

    [Fact]
    public async Task Post_MissingDataInJourneyState_Redirects()
    {
        // Arrange        
        var person = await TestData.CreatePerson(b => b.WithMandatoryQualification());
        var qualificationId = person.MandatoryQualifications!.First().QualificationId;
        var journeyInstance = await CreateJourneyInstance(
            qualificationId,
            new EditMqSpecialismState()
            {
                Initialized = true,
                PersonId = person.PersonId,
                PersonName = person.Contact.ResolveFullName(includeMiddleName: false)
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/specialism/confirm?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/mqs/{qualificationId}/specialism?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_Confirm_CompletesJourneyAndRedirectsWithFlashMessage()
    {
        // Arrange
        var oldMqSpecialism = MandatoryQualificationSpecialism.Hearing;
        var newMqSpecialism = MandatoryQualificationSpecialism.Visual;
        var person = await TestData.CreatePerson(b => b.WithMandatoryQualification(q => q.WithSpecialism(oldMqSpecialism)));
        var qualificationId = person.MandatoryQualifications!.First().QualificationId;
        var journeyInstance = await CreateJourneyInstance(
            qualificationId,
            new EditMqSpecialismState()
            {
                Initialized = true,
                PersonId = person.PersonId,
                PersonName = person.Contact.ResolveFullName(includeMiddleName: false),
                Specialism = newMqSpecialism
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/specialism/confirm?{journeyInstance.GetUniqueIdQueryParameter()}")
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
        var specialism = MandatoryQualificationSpecialism.Hearing;
        var person = await TestData.CreatePerson(b => b.WithMandatoryQualification(q => q.WithSpecialism(specialism)));
        var qualificationId = person.MandatoryQualifications!.First().QualificationId;
        var journeyInstance = await CreateJourneyInstance(
            qualificationId,
            new EditMqSpecialismState()
            {
                Initialized = true,
                PersonId = person.PersonId,
                PersonName = person.Contact.ResolveFullName(includeMiddleName: false),
                Specialism = specialism
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/specialism/confirm/cancel?{journeyInstance.GetUniqueIdQueryParameter()}")
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

    private async Task<JourneyInstance<EditMqSpecialismState>> CreateJourneyInstance(Guid qualificationId, EditMqSpecialismState? state = null) =>
    await CreateJourneyInstance(
        JourneyNames.EditMqSpecialism,
        state ?? new EditMqSpecialismState(),
        new KeyValuePair<string, object>("qualificationId", qualificationId));
}
