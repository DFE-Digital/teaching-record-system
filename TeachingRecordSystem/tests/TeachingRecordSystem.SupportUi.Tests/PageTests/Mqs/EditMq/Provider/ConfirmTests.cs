using FormFlow;
using Microsoft.EntityFrameworkCore;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events;
using TeachingRecordSystem.SupportUi.Pages.Mqs.EditMq.Provider;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.Mqs.EditMq.Provider;

public class ConfirmTests(HostFixture hostFixture) : TestBase(hostFixture)
{
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
                Initialized = true
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
                Initialized = true
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
    public async Task Post_Confirm_UpdatesMqCreatesEventAndCompletesJourneyRedirectsWithFlashMessage()
    {
        // Arrange
        var oldMqEstablishmentValue = "955"; // University of Birmingham
        var newMqEstablishmentValue = "959"; // University of Leeds
        var oldEstablishment = await TestData.ReferenceDataCache.GetMqEstablishmentByValue(oldMqEstablishmentValue);
        MandatoryQualificationProvider.TryMapFromDqtMqEstablishment(oldEstablishment, out var oldProvider);
        Assert.NotNull(oldProvider);
        var newEstablishment = await TestData.ReferenceDataCache.GetMqEstablishmentByValue(newMqEstablishmentValue);
        MandatoryQualificationProvider.TryMapFromDqtMqEstablishment(newEstablishment, out var newProvider);
        Assert.NotNull(newProvider);

        var person = await TestData.CreatePerson(b => b.WithMandatoryQualification(q => q.WithDqtMqEstablishmentValue(oldMqEstablishmentValue)));
        var qualification = person.MandatoryQualifications.First();
        var qualificationId = qualification.QualificationId;

        EventObserver.Clear();

        var journeyInstance = await CreateJourneyInstance(
            qualificationId,
            new EditMqProviderState()
            {
                Initialized = true,
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

        await WithDbContext(async dbContext =>
        {
            var qualification = await dbContext.MandatoryQualifications.SingleAsync(q => q.PersonId == person.PersonId);
            Assert.Equal(newProvider?.MandatoryQualificationProviderId, qualification.ProviderId);
        });

        EventObserver.AssertEventsSaved(e =>
        {
            var expectedMqUpdatedEvent = new MandatoryQualificationUpdatedEvent()
            {
                EventId = Guid.Empty,
                CreatedUtc = Clock.UtcNow,
                RaisedBy = GetCurrentUserId(),
                PersonId = person.PersonId,
                MandatoryQualification = new()
                {
                    QualificationId = qualificationId,
                    Provider = new()
                    {
                        MandatoryQualificationProviderId = newProvider.MandatoryQualificationProviderId,
                        Name = newProvider.Name,
                        DqtMqEstablishmentId = newEstablishment.Id,
                        DqtMqEstablishmentName = newEstablishment.dfeta_name
                    },
                    Specialism = qualification.Specialism,
                    Status = qualification.Status,
                    StartDate = qualification.StartDate,
                    EndDate = qualification.EndDate
                },
                OldMandatoryQualification = new()
                {
                    QualificationId = qualificationId,
                    Provider = new()
                    {
                        MandatoryQualificationProviderId = oldProvider.MandatoryQualificationProviderId,
                        Name = oldProvider.Name,
                        DqtMqEstablishmentId = oldEstablishment.Id,
                        DqtMqEstablishmentName = oldEstablishment.dfeta_name
                    },
                    Specialism = qualification.Specialism,
                    Status = qualification.Status,
                    StartDate = qualification.StartDate,
                    EndDate = qualification.EndDate
                },
                Changes = MandatoryQualificationUpdatedEventChanges.Provider
            };

            var actualMqUpdatedEvent = Assert.IsType<MandatoryQualificationUpdatedEvent>(e);
            Assert.Equivalent(expectedMqUpdatedEvent with { EventId = actualMqUpdatedEvent.EventId }, actualMqUpdatedEvent);
        });
    }

    [Fact]
    public async Task Post_Cancel_DeletesJourneyAndRedirectsAndDoesNotUpdateMq()
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

        await WithDbContext(async dbContext =>
        {
            var qualification = await dbContext.MandatoryQualifications.SingleAsync(q => q.PersonId == person.PersonId);
            MandatoryQualificationProvider.TryMapFromDqtMqEstablishmentValue(oldMqEstablishmentValue, out var expectedProvider);
            Assert.Equal(expectedProvider?.MandatoryQualificationProviderId, qualification.ProviderId);
        });
    }

    private async Task<JourneyInstance<EditMqProviderState>> CreateJourneyInstance(Guid qualificationId, EditMqProviderState? state = null) =>
        await CreateJourneyInstance(
            JourneyNames.EditMqProvider,
            state ?? new EditMqProviderState(),
            new KeyValuePair<string, object>("qualificationId", qualificationId));
}
