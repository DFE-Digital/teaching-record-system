using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.RoutesToProfessionalStatus.AddRoute;

public class AwardDateTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Fact]
    public async Task Get_ShowsPreviouslyStoredEntry()
    {
        // Arrange
        var startDate = new DateOnly(2024, 01, 01);
        var endDate = startDate.AddMonths(1);
        var holdsFrom = endDate.AddDays(1);
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.HoldsFromRequired == FieldRequirement.Mandatory)
            .RandomOne();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.HoldsFromRequired == FieldRequirement.Mandatory)
            .RandomOne()
            .Value;
        var person = await TestData.CreatePersonAsync();
        var addRouteState = new AddRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(status)
            .WithHoldsFrom(holdsFrom)
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            addRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Get, $"/route/add/holds-from?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        var displayedDate = doc.QuerySelectorAll<IHtmlInputElement>("[type=text]");
        Assert.Equal(holdsFrom.Day.ToString(), displayedDate.ElementAt(0).Value);
        Assert.Equal(holdsFrom.Month.ToString(), displayedDate.ElementAt(1).Value);
        Assert.Equal(holdsFrom.Year.ToString(), displayedDate.ElementAt(2).Value);
    }

    [Fact]
    public async Task Post_WhenAwardDateIsEntered_SavesDateAndRedirectsToInductionExemptionPage()
    {
        // Arrange
        var holdsFrom = new DateOnly(2024, 01, 01);
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.HoldsFromRequired == FieldRequirement.Mandatory
                && r.InductionExemptionRequired == FieldRequirement.Mandatory
                && r.InductionExemptionReason is not null
                && r.InductionExemptionReason.RouteImplicitExemption == false)
            .RandomOne();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.HoldsFromRequired == FieldRequirement.Mandatory && s.InductionExemptionRequired == FieldRequirement.Mandatory)
            .RandomOne()
            .Value;
        var person = await TestData.CreatePersonAsync();
        var addRouteState = new AddRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(status)
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            addRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Post, $"/route/add/holds-from?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "HoldsFrom.Day", $"{holdsFrom:%d}" },
                { "HoldsFrom.Month", $"{holdsFrom:%M}" },
                { "HoldsFrom.Year", $"{holdsFrom:yyyy}" },
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/route/add/induction-exemption?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(holdsFrom, journeyInstance.State.HoldsFrom);
    }

    [Fact]
    public async Task Post_ImplicitExemptionRoute_WhenAwardDateIsEntered_SavesDateAndRedirectsToNextPage()
    {
        // Arrange
        var holdsFrom = new DateOnly(2024, 01, 01);
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.InductionExemptionReason is not null && r.InductionExemptionReason.RouteImplicitExemption)
            .RandomOne();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.HoldsFromRequired == FieldRequirement.Mandatory && s.InductionExemptionRequired == FieldRequirement.Mandatory)
            .RandomOne()
            .Value;
        var person = await TestData.CreatePersonAsync();
        var addRouteState = new AddRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(status)
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            addRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Post, $"/route/add/holds-from?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "HoldsFrom.Day", $"{holdsFrom:%d}" },
                { "HoldsFrom.Month", $"{holdsFrom:%M}" },
                { "HoldsFrom.Year", $"{holdsFrom:yyyy}" },
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/route/add/training-provider?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(holdsFrom, journeyInstance.State.HoldsFrom);
    }

    [Fact]
    public async Task Post_FromCya_WhenAwardDateIsEntered_RedirectsToCya()
    {
        // Arrange
        var holdsFrom = new DateOnly(2024, 01, 01);
        var newAwardDate = holdsFrom.AddMonths(1);
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.HoldsFromRequired == FieldRequirement.Mandatory)
            .RandomOne();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.HoldsFromRequired == FieldRequirement.Mandatory)
            .RandomOne()
            .Value;
        var person = await TestData.CreatePersonAsync();
        var addRouteState = new AddRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(status)
            .WithHoldsFrom(holdsFrom)
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            addRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Post, $"/route/add/holds-from?personId={person.PersonId}&FromCheckAnswers=True&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "HoldsFrom.Day", $"{newAwardDate:%d}" },
                { "HoldsFrom.Month", $"{newAwardDate:%M}" },
                { "HoldsFrom.Year", $"{newAwardDate:yyyy}" },
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/route/add/check-answers?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(newAwardDate, journeyInstance.State.HoldsFrom);
    }

    [Fact]
    public async Task Post_WhenNoDateIsEntered_ReturnsError()
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.HoldsFromRequired == FieldRequirement.Mandatory)
            .RandomOne();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.HoldsFromRequired == FieldRequirement.Mandatory)
            .RandomOne()
            .Value;
        var person = await TestData.CreatePersonAsync();

        var addRouteState = new AddRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(status)
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            addRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Post, $"/route/add/holds-from?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "HoldsFrom", "Enter the professional status award date");
    }

    [Fact]
    public async Task Post_WhenFutureDateIsEntered_ReturnsError()
    {
        // Arrange
        var holdsDate = Clock.UtcNow.AddDays(1);
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.HoldsFromRequired == FieldRequirement.Mandatory)
            .RandomOne();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.HoldsFromRequired == FieldRequirement.Mandatory)
            .RandomOne()
            .Value;
        var person = await TestData.CreatePersonAsync();

        var addRouteState = new AddRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(status)
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            addRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Post, $"/route/add/holds-from?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder()
            {
                { "HoldsFrom.Day", $"{holdsDate:%d}" },
                { "HoldsFrom.Month", $"{holdsDate:%M}" },
                { "HoldsFrom.Year", $"{holdsDate:yyyy}" },
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "HoldsFrom", "Professional Status Date must not be in the future");
    }

    [Fact]
    public async Task Cancel_RedirectsToExpectedPage()
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.TrainingEndDateRequired == FieldRequirement.Mandatory)
            .RandomOne();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.TrainingEndDateRequired == FieldRequirement.Mandatory)
            .RandomOne()
            .Value;
        var person = await TestData.CreatePersonAsync();
        var addRouteState = new AddRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(status)
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            addRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Get, $"/route/add/holds-from?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

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
