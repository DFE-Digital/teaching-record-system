using TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.RoutesToProfessionalStatus.EditRoute;

public class TrainingProviderTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task NoTrainingProviderSelected_ShowsError()
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRoutesToProfessionalStatusAsync())
            .Where(r => r.TrainingProviderRequired == FieldRequirement.Mandatory)
            .RandomOne();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.TrainingProviderRequired == FieldRequirement.Optional)
            .RandomOne();
        var person = await TestData.CreatePersonAsync(p => p
            .WithProfessionalStatus(r => r
                .WithRoute(route.RouteToProfessionalStatusId)
                .WithStatus(ProfessionalStatusStatus.Deferred)));
        var qualificationId = person.ProfessionalStatuses.First().QualificationId;

        var editRouteState = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusId)
            .WithStatus(status.Value)
            .Build();
        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            editRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Post, $"/route/{qualificationId}/edit/training-provider?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "TrainingProviderId", "" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "TrainingProviderId", "Select a training provider");
    }

    [Fact]
    public async Task Post_SelectTrainingProvider_PersistsSelection()
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRoutesToProfessionalStatusAsync())
            .Where(r => r.TrainingProviderRequired == FieldRequirement.Mandatory)
            .RandomOne();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.TrainingProviderRequired == FieldRequirement.Optional)
            .RandomOne();
        var person = await TestData.CreatePersonAsync(p => p
            .WithProfessionalStatus(r => r
                .WithRoute(route.RouteToProfessionalStatusId)
                .WithStatus(ProfessionalStatusStatus.Approved)));
        var qualificationId = person.ProfessionalStatuses.First().QualificationId;
        var trainingProvider = (await ReferenceDataCache.GetTrainingProvidersAsync()).RandomOne();
        var editRouteState = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusId)
            .WithStatus(status.Value)
            .Build();
        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            editRouteState
            );

        var postRequest = new HttpRequestMessage(HttpMethod.Post, $"/route/{qualificationId}/edit/training-provider?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "TrainingProviderId", trainingProvider.TrainingProviderId.ToString()}
            }
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(trainingProvider.TrainingProviderId, journeyInstance.State.TrainingProviderId);
    }

    [Fact]
    public async Task Post_WhenTrainingProviderIsEntered_RedirectsToDetail()
    {
        var route = (await ReferenceDataCache.GetRoutesToProfessionalStatusAsync())
            .Where(r => r.TrainingProviderRequired == FieldRequirement.Mandatory)
            .RandomOne();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.TrainingProviderRequired == FieldRequirement.Optional)
            .RandomOne();
        var person = await TestData.CreatePersonAsync(p => p
            .WithProfessionalStatus(r => r
                .WithRoute(route.RouteToProfessionalStatusId)
                .WithStatus(ProfessionalStatusStatus.Approved)));
        var qualificationId = person.ProfessionalStatuses.First().QualificationId;
        var trainingProvider = (await ReferenceDataCache.GetTrainingProvidersAsync()).RandomOne();
        var editRouteState = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusId)
            .WithStatus(status.Value)
            .Build();
        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            editRouteState
            );

        var postRequest = new HttpRequestMessage(HttpMethod.Post, $"/route/{qualificationId}/edit/training-provider?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "TrainingProviderId" , trainingProvider.TrainingProviderId.ToString()}
            }
        };

        // Act
        var response = await HttpClient.SendAsync(postRequest);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/route/{qualificationId}/edit/detail?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    private Task<JourneyInstance<EditRouteState>> CreateJourneyInstanceAsync(Guid qualificationId, EditRouteState? state = null) =>
    CreateJourneyInstance(
        JourneyNames.EditRouteToProfessionalStatus,
        state ?? new EditRouteState(),
        new KeyValuePair<string, object>("qualificationId", qualificationId));
}
