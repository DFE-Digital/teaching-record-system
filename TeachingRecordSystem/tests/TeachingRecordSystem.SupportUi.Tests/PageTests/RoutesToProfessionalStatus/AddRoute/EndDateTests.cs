using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.RoutesToProfessionalStatus.AddRoute;

public class EndDateTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_ShowsPreviouslyStoredEntry()
    {
        // Arrange
        var startDate = new DateOnly(2024, 01, 01);
        var endDate = startDate.AddMonths(1);
        var route = (await ReferenceDataCache.GetRoutesToProfessionalStatusAsync())
            .Where(r => r.TrainingStartDateRequired == FieldRequirement.Mandatory && r.TrainingEndDateRequired == FieldRequirement.Mandatory)
            .RandomOne();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.TrainingStartDateRequired == FieldRequirement.Mandatory && s.TrainingEndDateRequired == FieldRequirement.Mandatory)
            .RandomOne()
            .Value;
        var person = await TestData.CreatePersonAsync();
        var addRouteState = new AddRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusId)
            .WithStatus(status)
            .WithTrainingStartDate(startDate)
            .WithTrainingEndDate(endDate)
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            addRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Get, $"/route/add/end-date?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var displayedDate = doc.QuerySelectorAll<IHtmlInputElement>("[type=text]");
        Assert.Equal(endDate.Day.ToString(), displayedDate.ElementAt(0).Value);
        Assert.Equal(endDate.Month.ToString(), displayedDate.ElementAt(1).Value);
        Assert.Equal(endDate.Year.ToString(), displayedDate.ElementAt(2).Value);
    }

    [Fact]
    public async Task Post_WhenTrainingEndDateIsEntered_SavesDateAndRedirectsToNextPage()
    {
        // Arrange
        var startDate = new DateOnly(2024, 01, 01);
        var endDate = startDate.AddMonths(1);
        var route = (await ReferenceDataCache.GetRoutesToProfessionalStatusAsync())
            .Where(r => r.TrainingEndDateRequired == FieldRequirement.Mandatory && r.TrainingProviderRequired == FieldRequirement.Mandatory)
            .RandomOne();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.TrainingEndDateRequired == FieldRequirement.Mandatory && s.TrainingProviderRequired == FieldRequirement.Optional && s.AwardDateRequired == FieldRequirement.NotApplicable)
            .RandomOne()
            .Value;
        var person = await TestData.CreatePersonAsync();
        var addRouteState = new AddRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusId)
            .WithStatus(status)
            .WithTrainingStartDate(startDate)
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            addRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Post, $"/route/add/end-date?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "TrainingEndDate.Day", $"{endDate:%d}" },
                { "TrainingEndDate.Month", $"{endDate:%M}" },
                { "TrainingEndDate.Year", $"{endDate:yyyy}" },
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/route/add/training-provider?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(endDate, journeyInstance.State.TrainingEndDate);
    }

    [Fact]
    public async Task Post_WhenNoEndDateIsEntered_ReturnsError()
    {
        // Arrange
        var startDate = new DateOnly(2024, 01, 01);

        var route = (await ReferenceDataCache.GetRoutesToProfessionalStatusAsync())
            .Where(r => r.TrainingStartDateRequired == FieldRequirement.Mandatory && r.TrainingEndDateRequired == FieldRequirement.Mandatory)
            .RandomOne();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.TrainingStartDateRequired == FieldRequirement.Mandatory && s.TrainingEndDateRequired == FieldRequirement.Mandatory)
            .RandomOne()
            .Value;
        var person = await TestData.CreatePersonAsync();

        var addRouteState = new AddRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusId)
            .WithStatus(status)
            .WithTrainingStartDate(startDate)
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            addRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Post, $"/route/add/end-date?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>())
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "TrainingEndDate", "Enter an end date");
    }

    [Fact]
    public async Task Post_EndDateBeforeStartDate_ShowsError()
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
        var person = await TestData.CreatePersonAsync();

        var addRouteState = new AddRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusId)
            .WithStatus(status)
            .WithTrainingStartDate(startDate)
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            addRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Post, $"/route/add/end-date?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "TrainingEndDate.Day", $"{endDate:%d}" },
                { "TrainingEndDate.Month", $"{endDate:%M}" },
                { "TrainingEndDate.Year", $"{endDate:yyyy}" },
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "TrainingEndDate", "End date must be after start date");
    }

    [Fact]
    public async Task Cancel_RedirectsToExpectedPage()
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRoutesToProfessionalStatusAsync())
            .Where(r => r.TrainingEndDateRequired == FieldRequirement.Mandatory)
            .RandomOne();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.TrainingEndDateRequired == FieldRequirement.Mandatory)
            .RandomOne()
            .Value;
        var person = await TestData.CreatePersonAsync();
        var addRouteState = new AddRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusId)
            .WithStatus(status)
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            addRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Get, $"/route/add/end-date?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

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
        Assert.Equal($"/persons/{person.PersonId}/qualifications", location);
    }

    private Task<JourneyInstance<AddRouteState>> CreateJourneyInstanceAsync(Guid personId, AddRouteState? state = null) =>
        CreateJourneyInstance(
           JourneyNames.AddRouteToProfessionalStatus,
           state ?? new AddRouteState(),
           new KeyValuePair<string, object>("personId", personId));

}
