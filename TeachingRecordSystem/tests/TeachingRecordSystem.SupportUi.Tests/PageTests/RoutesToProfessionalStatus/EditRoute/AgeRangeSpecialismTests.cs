using AngleSharp.Html.Dom;
using TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.RoutesToProfessionalStatus.EditRoute;

public class AgeRangeSpecialismTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Post_WhenAgeSpecialismIsEntered_PersistsDataAndRedirectsToDetail()
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRoutesToProfessionalStatusAsync())
            .Where(r => r.TrainingAgeSpecialismTypeRequired == FieldRequirement.Optional)
            .RandomOne();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.TrainingAgeSpecialismTypeRequired == FieldRequirement.Optional)
            .RandomOne()
            .Value;
        var person = await TestData.CreatePersonAsync(p => p
            .WithProfessionalStatus(r => r
                .WithRoute(route.RouteToProfessionalStatusId)
                .WithStatus(status)));
        var qualificationId = person.ProfessionalStatuses.First().QualificationId;
        var editRouteState = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusId)
            .WithStatus(status)
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            editRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Post, $"/routes/{qualificationId}/edit/age-range?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { nameof(AgeRangeSpecialismModel.TrainingAgeSpecialism.AgeRangeType), TrainingAgeSpecialismType.KeyStage4 },
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(TrainingAgeSpecialismType.KeyStage4, journeyInstance.State.TrainingAgeSpecialismType);
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/routes/{qualificationId}/edit/detail?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Cancel_DeletesJourneyAndRedirectsToExpectedPage()
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRoutesToProfessionalStatusAsync())
            .Where(r => r.TrainingAgeSpecialismTypeRequired == FieldRequirement.Optional)
            .RandomOne();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.TrainingAgeSpecialismTypeRequired == FieldRequirement.Optional)
            .RandomOne()
            .Value;
        var person = await TestData.CreatePersonAsync(p => p
            .WithProfessionalStatus(r => r
                .WithRoute(route.RouteToProfessionalStatusId)
                .WithStatus(status)));
        var qualificationid = person.ProfessionalStatuses.First().QualificationId;
        var editRouteState = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusId)
            .WithStatus(status)
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationid,
            editRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Get, $"/routes/{qualificationid}/edit/age-range?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var cancelButton = doc.GetElementByTestId("cancel-button") as IHtmlButtonElement;

        // Act
        var redirectRequest = new HttpRequestMessage(HttpMethod.Post, cancelButton!.FormAction);
        var redirectResponse = await HttpClient.SendAsync(redirectRequest);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)redirectResponse.StatusCode);
        var location = redirectResponse.Headers.Location?.OriginalString;
        Assert.Equal($"/persons/{person.PersonId}/qualifications", location);
        Assert.Null(await ReloadJourneyInstance(journeyInstance));
    }

    private Task<JourneyInstance<EditRouteState>> CreateJourneyInstanceAsync(Guid qualificationId, EditRouteState? state = null) =>
        CreateJourneyInstance(
           JourneyNames.EditRouteToProfessionalStatus,
           state ?? new EditRouteState(),
           new KeyValuePair<string, object>("qualificationId", qualificationId));

}
