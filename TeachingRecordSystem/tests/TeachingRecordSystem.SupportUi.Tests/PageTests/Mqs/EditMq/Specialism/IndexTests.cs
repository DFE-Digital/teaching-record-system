using System.Diagnostics;
using TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Specialism;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Mqs.EditMq.Specialism;

public class IndexTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_WithQualificationIdForNonExistentQualification_ReturnsNotFound()
    {
        // Arrange
        var qualificationId = Guid.NewGuid();
        var journeyInstance = await CreateJourneyInstanceAsync(qualificationId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/{qualificationId}/specialism?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Get_ValidRequestWithUninitializedJourneyState_PopulatesModelFromDatabase()
    {
        // Arrange
        var databaseSpecialism = MandatoryQualificationSpecialism.Hearing;
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q.WithSpecialism(databaseSpecialism)));
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;
        var journeyInstance = await CreateJourneyInstanceAsync(qualificationId);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/{qualificationId}/specialism?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var specialismList = doc.GetElementByTestId("specialism-list");
        var radioButtons = specialismList!.GetElementsByTagName("input");
        var selectedSpecialism = radioButtons.SingleOrDefault(r => r.HasAttribute("checked"));
        Assert.NotNull(selectedSpecialism);
        Assert.Equal(databaseSpecialism.ToString(), selectedSpecialism.GetAttribute("value"));
    }

    [Fact]
    public async Task Get_ValidRequestWithInitializedJourneyState_PopulatesModelFromJourneyState()
    {
        // Arrange
        var databaseSpecialism = MandatoryQualificationSpecialism.Hearing;
        var journeySpecialism = MandatoryQualificationSpecialism.Visual;
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q.WithSpecialism(databaseSpecialism)));
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;
        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            new EditMqSpecialismState()
            {
                Initialized = true,
                Specialism = journeySpecialism
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/{qualificationId}/specialism?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var specialismList = doc.GetElementByTestId("specialism-list");
        var radioButtons = specialismList!.GetElementsByTagName("input");
        var selectedSpecialism = radioButtons.SingleOrDefault(r => r.HasAttribute("checked"));
        Assert.NotNull(selectedSpecialism);
        Assert.Equal(journeySpecialism.ToString(), selectedSpecialism.GetAttribute("value"));
    }

    [Fact]
    public async Task Get_QualificationWasMigratedFromDqtWithLegacySpecialism_ShowsLegacySpecialisms()
    {
        // Arrange
        var specialism = MandatoryQualificationSpecialism.DeafEducation;
        Debug.Assert(MandatoryQualificationSpecialismRegistry.IsLegacy(specialism));
        var dqtSpecialism = await TestData.ReferenceDataCache.GetMqSpecialismByValueAsync(specialism.GetDqtValue());
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q.WithSpecialism(specialism, dqtSpecialism.dfeta_specialismId)));
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            new EditMqSpecialismState()
            {
                Initialized = true,
                Specialism = specialism
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/{qualificationId}/specialism?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var legacySpecialisms = MandatoryQualificationSpecialismRegistry.GetAll(includeLegacy: true).Where(s => s.Legacy).ToArray();
        var specialismRadios = doc.GetElementsByName("Specialism");
        foreach (var s in legacySpecialisms)
        {
            Assert.Contains(specialismRadios, r => r.GetAttribute("value") == s.Value.ToString());
        }
    }

    [Fact]
    public async Task Get_QualificationWasMigratedFromDqtWithNonLegacySpecialism_DoesNotShowLegacySpecialisms()
    {
        // Arrange
        var specialism = MandatoryQualificationSpecialism.Hearing;
        Debug.Assert(!MandatoryQualificationSpecialismRegistry.IsLegacy(specialism));
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q.WithSpecialism(specialism)));
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            new EditMqSpecialismState()
            {
                Initialized = true,
                Specialism = specialism
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/mqs/{qualificationId}/specialism?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var legacySpecialisms = MandatoryQualificationSpecialismRegistry.GetAll(includeLegacy: true).Where(s => s.Legacy).ToArray();
        var specialismRadios = doc.GetElementsByName("Specialism");
        foreach (var s in legacySpecialisms)
        {
            Assert.DoesNotContain(specialismRadios, r => r.GetAttribute("value") == s.Value.ToString());
        }
    }

    [Fact]
    public async Task Post_WithQualificationIdForNonExistentQualification_ReturnsNotFound()
    {
        // Arrange
        var qualificationId = Guid.NewGuid();
        var specialismValue = "Hearing";
        var journeyInstance = await CreateJourneyInstanceAsync(qualificationId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/specialism?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "SpecialismValue", specialismValue }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status404NotFound, (int)response.StatusCode);
    }

    [Fact]
    public async Task Post_WhenNoSpecialismIsSelected_ReturnsError()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification());
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;
        var journeyInstance = await CreateJourneyInstanceAsync(qualificationId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/specialism?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "Specialism", "Select a specialism");
    }

    [Fact]
    public async Task Post_WhenSpecialismIsSelected_RedirectsToReasonPage()
    {
        // Arrange
        var oldSpecialism = MandatoryQualificationSpecialism.Hearing;
        var newSpecialism = MandatoryQualificationSpecialism.Visual;
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q.WithSpecialism(oldSpecialism)));
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;
        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            new EditMqSpecialismState()
            {
                Initialized = true,
                Specialism = oldSpecialism
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/specialism?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "Specialism", newSpecialism }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/mqs/{qualificationId}/specialism/change-reason?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_Cancel_DeletesJourneyAndRedirects()
    {
        // Arrange
        var specialism = MandatoryQualificationSpecialism.Hearing;
        var person = await TestData.CreatePersonAsync(b => b.WithMandatoryQualification(q => q.WithSpecialism(specialism)));
        var qualificationId = person.MandatoryQualifications.Single().QualificationId;
        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            new EditMqSpecialismState()
            {
                Initialized = true,
                Specialism = specialism
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/mqs/{qualificationId}/specialism/cancel?{journeyInstance.GetUniqueIdQueryParameter()}")
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


    private async Task<JourneyInstance<EditMqSpecialismState>> CreateJourneyInstanceAsync(Guid qualificationId, EditMqSpecialismState? state = null) =>
        await CreateJourneyInstance(
            JourneyNames.EditMqSpecialism,
            state ?? new EditMqSpecialismState(),
            new KeyValuePair<string, object>("qualificationId", qualificationId));
}
