using AngleSharp.Html.Dom;
using TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.RoutesToProfessionalStatus.EditRoute;

public class AgeRangeSpecialismTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Post_WhenNothingIsSelected_DisplaysError()
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.TrainingAgeSpecialismTypeRequired == FieldRequirement.Optional)
            .RandomOne();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.TrainingAgeSpecialismTypeRequired == FieldRequirement.Optional && s.HoldsFromRequired == FieldRequirement.NotApplicable)
            .RandomOne()
            .Value;
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(status)));
        var qualificationId = person.ProfessionalStatuses.First().QualificationId;
        var editRouteState = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(status)
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            editRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Post, $"/route/{qualificationId}/edit/age-range?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, nameof(AgeRangeSpecialismModel.TrainingAgeSpecialism.AgeRangeType), "Enter an age specialism or select 'Not provided'");
    }

    [Fact]
    public async Task Post_WhenAgeRangeFromToIsEntered_PersistsDataAndRedirectsToDetail()
    {
        // Arrange
        var ageFrom = 4;
        var ageTo = 8;
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.TrainingAgeSpecialismTypeRequired == FieldRequirement.Optional)
            .RandomOne();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.TrainingAgeSpecialismTypeRequired == FieldRequirement.Optional && s.HoldsFromRequired == FieldRequirement.NotApplicable)
            .RandomOne()
            .Value;
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(status)));
        var qualificationId = person.ProfessionalStatuses.First().QualificationId;
        var editRouteState = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(status)
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            editRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Post, $"/route/{qualificationId}/edit/age-range?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { nameof(AgeRangeSpecialismModel.TrainingAgeSpecialism.AgeRangeFrom), ageFrom },
                { nameof(AgeRangeSpecialismModel.TrainingAgeSpecialism.AgeRangeTo), ageTo }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(ageFrom, journeyInstance.State.TrainingAgeSpecialismRangeFrom);
        Assert.Equal(ageFrom, journeyInstance.State.TrainingAgeSpecialismRangeTo);
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/route/{qualificationId}/edit/detail?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_WhenAgeSpecialismIsEntered_PersistsDataAndRedirectsToDetail()
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.TrainingAgeSpecialismTypeRequired == FieldRequirement.Optional)
            .RandomOne();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.TrainingAgeSpecialismTypeRequired == FieldRequirement.Optional && s.HoldsFromRequired == FieldRequirement.NotApplicable)
            .RandomOne()
            .Value;
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(status)));
        var qualificationId = person.ProfessionalStatuses.First().QualificationId;
        var editRouteState = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(status)
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            editRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Post, $"/route/{qualificationId}/edit/age-range?{journeyInstance.GetUniqueIdQueryParameter()}")
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
        Assert.Equal($"/route/{qualificationId}/edit/detail?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Cancel_DeletesJourneyAndRedirectsToExpectedPage()
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.TrainingAgeSpecialismTypeRequired == FieldRequirement.Optional)
            .RandomOne();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.TrainingAgeSpecialismTypeRequired == FieldRequirement.Optional && s.HoldsFromRequired == FieldRequirement.NotApplicable)
            .RandomOne()
            .Value;
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(status)));
        var qualificationid = person.ProfessionalStatuses.First().QualificationId;
        var editRouteState = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(status)
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationid,
            editRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Get, $"/route/{qualificationid}/edit/age-range?{journeyInstance.GetUniqueIdQueryParameter()}");

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
