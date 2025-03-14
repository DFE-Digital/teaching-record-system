using AngleSharp.Html.Dom;
using TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.RoutesToProfessionalStatus.EditRoute;

public class StartDateTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Post_WhenStartDateIsAfterEndDate_NoError()
    {
        // Arrange
        var startDate = new DateOnly(2024, 01, 01);
        var endDate = startDate.AddDays(-1);
        var route = (await ReferenceDataCache.GetRoutesToProfessionalStatusAsync())
            .Where(r => r.TrainingStartDateRequired == FieldRequirement.Mandatory && r.TrainingEndDateRequired == FieldRequirement.Mandatory)
            .RandomOne();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.TrainingStartDateRequired == FieldRequirement.Mandatory && s.TrainingEndDateRequired == FieldRequirement.Mandatory)
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

        var request = new HttpRequestMessage(HttpMethod.Post, $"/route/{qualificationid}/edit/start-date?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "TrainingStartDate.Day", $"{startDate:%d}" },
                { "TrainingStartDate.Month", $"{startDate:%M}" },
                { "TrainingStartDate.Year", $"{startDate:yyyy}" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/route/{qualificationid}/edit/detail?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_WhenTrainingStartDateIsEntered_RedirectsToDetail()
    {
        // Arrange
        var startDate = new DateOnly(2024, 01, 01);
        var endDate = new DateOnly(2025, 01, 01);
        var route = (await ReferenceDataCache.GetRoutesToProfessionalStatusAsync())
            .Where(r => r.TrainingStartDateRequired == FieldRequirement.Mandatory && r.TrainingEndDateRequired == FieldRequirement.Mandatory)
            .RandomOne();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.TrainingStartDateRequired == FieldRequirement.Mandatory && s.TrainingEndDateRequired == FieldRequirement.Mandatory)
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
            .WithTrainingStartDate(startDate)
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationid,
            editRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Post, $"/route/{qualificationid}/edit/Start-date?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "TrainingStartDate.Day", $"{startDate:%d}" },
                { "TrainingStartDate.Month", $"{startDate:%M}" },
                { "TrainingStartDate.Year", $"{startDate:yyyy}" },
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/route/{qualificationid}/edit/detail?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_WhenNoStartDateIsEntered_ReturnsError()
    {
        // Arrange
        var startDate = new DateOnly(2024, 01, 01);
        var endDate = new DateOnly(2025, 01, 01);
        var route = (await ReferenceDataCache.GetRoutesToProfessionalStatusAsync())
            .Where(r => r.TrainingStartDateRequired == FieldRequirement.Mandatory && r.TrainingEndDateRequired == FieldRequirement.Mandatory)
            .RandomOne();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.TrainingStartDateRequired == FieldRequirement.Mandatory && s.TrainingEndDateRequired == FieldRequirement.Mandatory)
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
            .WithTrainingStartDate(startDate)
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationid,
            editRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Post, $"/route/{qualificationid}/edit/start-date?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>())
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "TrainingStartDate", "Enter a start date");
    }

    [Fact]
    public async Task Post_StartDateAfterEndDate_RedirectToEndDatePage()
    {
        // Arrange
        var startDate = new DateOnly(2024, 01, 01);
        var endDate = startDate.AddDays(1);
        var route = (await ReferenceDataCache.GetRoutesToProfessionalStatusAsync())
            .Where(r => r.TrainingStartDateRequired == FieldRequirement.Mandatory && r.TrainingEndDateRequired == FieldRequirement.Mandatory)
            .RandomOne();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.TrainingStartDateRequired == FieldRequirement.Mandatory && s.TrainingEndDateRequired == FieldRequirement.Mandatory)
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
            .WithTrainingStartDate(startDate)
            .WithTrainingEndDate(endDate)
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            editRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Post, $"/route/{qualificationId}/edit/start-date?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(
                new EditRoutePostRequestBuilder()
                    .WithStartDate(endDate.AddDays(1))
                    .Build())
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith($"/route/{qualificationId}/edit/end-date?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Cancel_RedirectsToExpectedPage()
    {
        // Arrange
        var startDate = new DateOnly(2024, 01, 01);
        var endDate = new DateOnly(2025, 01, 01);
        var route = (await ReferenceDataCache.GetRoutesToProfessionalStatusAsync())
            .Where(r => r.TrainingStartDateRequired == FieldRequirement.Mandatory && r.TrainingEndDateRequired == FieldRequirement.Mandatory)
            .RandomOne();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.TrainingStartDateRequired == FieldRequirement.Mandatory && s.TrainingEndDateRequired == FieldRequirement.Mandatory)
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
            .WithTrainingStartDate(startDate)
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationid,
            editRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Get, $"/route/{qualificationid}/edit/start-date?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var cancelButton = doc.GetElementByTestId("cancel-button") as IHtmlButtonElement;
        var redirectRequest = new HttpRequestMessage(HttpMethod.Post, cancelButton!.FormAction);
        var redirectResponse = await HttpClient.SendAsync(redirectRequest);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)redirectResponse.StatusCode);
        var location = redirectResponse.Headers.Location?.OriginalString;
        Assert.Equal($"/route/{qualificationid}/edit/detail?{journeyInstance.GetUniqueIdQueryParameter()}", location);
    }

    private Task<JourneyInstance<EditRouteState>> CreateJourneyInstanceAsync(Guid qualificationId, EditRouteState? state = null) =>
        CreateJourneyInstance(
           JourneyNames.EditRouteToProfessionalStatus,
           state ?? new EditRouteState(),
           new KeyValuePair<string, object>("qualificationId", qualificationId));

}
