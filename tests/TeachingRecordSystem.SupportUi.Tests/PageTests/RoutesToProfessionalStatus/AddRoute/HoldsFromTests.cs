using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.RoutesToProfessionalStatus.AddRoute;

public class HoldsFromTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Theory]
    [InlineData("Apply for Qualified Teacher Status in England", RouteToProfessionalStatusStatus.Holds, false)]
    [InlineData("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.InTraining, true)]
    [InlineData("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.Holds, false)]
    public async Task Get_FieldsMarkedAsOptional_BasedOnRouteAndStatusFieldRequirements(string routeName, RouteToProfessionalStatusStatus status, bool expectFieldsToBeOptional)
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).Single(r => r.Name == routeName);
        var startDate = new DateOnly(2024, 01, 01);
        var endDate = startDate.AddMonths(1);
        var holdsFrom = endDate.AddDays(1);
        var person = await TestData.CreatePersonAsync();
        var addRouteState = new AddRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(status)
            .WithHoldsFrom(holdsFrom)
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(person.PersonId, addRouteState);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/routes/add/holds-from?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var heading = doc.QuerySelector("h1.govuk-fieldset__heading");
        Assert.NotNull(heading);
        if (expectFieldsToBeOptional)
        {
            Assert.Contains("(optional)", heading.TrimmedText());
        }
        else
        {
            Assert.DoesNotContain("(optional)", heading.TrimmedText());
        }
    }

    [Theory]
    [InlineData("Apply for Qualified Teacher Status in England", RouteToProfessionalStatusStatus.Holds, false)]
    [InlineData("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.InTraining, true)]
    [InlineData("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.Holds, false)]
    public async Task Post_MissingValues_ValidOrInvalid_BasedOnRouteAndStatusFieldRequirements(string routeName, RouteToProfessionalStatusStatus status, bool expectFieldsToBeOptional)
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).Single(r => r.Name == routeName);
        var startDate = new DateOnly(2024, 01, 01);
        var endDate = startDate.AddMonths(1);
        var holdsFrom = endDate.AddDays(1);
        var person = await TestData.CreatePersonAsync();
        var addRouteState = new AddRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(status)
            .WithHoldsFrom(holdsFrom)
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(person.PersonId, addRouteState);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/routes/add/holds-from?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        if (expectFieldsToBeOptional)
        {
            Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        }
        else
        {
            await AssertEx.HtmlResponseHasErrorAsync(response, "HoldsFrom", "Enter the date they first held this professional status");
        }
    }

    [Fact]
    public async Task Get_ShowsPreviouslyStoredEntry()
    {
        // Arrange
        var startDate = new DateOnly(2024, 01, 01);
        var endDate = startDate.AddMonths(1);
        var holdsFrom = endDate.AddDays(1);
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.HoldsFromRequired == FieldRequirement.Mandatory)
            .SingleRandom();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.HoldsFromRequired == FieldRequirement.Mandatory)
            .SingleRandom()
            .Value;
        var person = await TestData.CreatePersonAsync();
        var addRouteState = new AddRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(status)
            .WithHoldsFrom(holdsFrom)
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(person.PersonId, addRouteState);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/routes/add/holds-from?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

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
        var holdsFrom = Clock.Today.AddYears(-1);
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.HoldsFromRequired == FieldRequirement.Mandatory
                && r.InductionExemptionRequired == FieldRequirement.Mandatory
                && r.InductionExemptionReason is not null
                && r.InductionExemptionReason.RouteImplicitExemption == false)
            .SingleRandom();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.HoldsFromRequired == FieldRequirement.Mandatory && s.InductionExemptionRequired == FieldRequirement.Mandatory)
            .SingleRandom()
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

        var request = new HttpRequestMessage(HttpMethod.Post, $"/routes/add/holds-from?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "HoldsFrom.Day", $"{holdsFrom:%d}" },
                { "HoldsFrom.Month", $"{holdsFrom:%M}" },
                { "HoldsFrom.Year", $"{holdsFrom:yyyy}" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/routes/add/induction-exemption?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(holdsFrom, journeyInstance.State.HoldsFrom);
    }

    [Fact]
    public async Task Post_ImplicitExemptionRoute_WhenAwardDateIsEntered_SavesDateAndRedirectsToNextPage()
    {
        // Arrange
        var holdsFrom = Clock.Today.AddYears(-1);
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.InductionExemptionReason is not null && r.InductionExemptionReason.RouteImplicitExemption)
            .SingleRandom();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.HoldsFromRequired == FieldRequirement.Mandatory && s.InductionExemptionRequired == FieldRequirement.Mandatory)
            .SingleRandom()
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

        var request = new HttpRequestMessage(HttpMethod.Post, $"/routes/add/holds-from?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "HoldsFrom.Day", $"{holdsFrom:%d}" },
                { "HoldsFrom.Month", $"{holdsFrom:%M}" },
                { "HoldsFrom.Year", $"{holdsFrom:yyyy}" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/routes/add/training-provider?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(holdsFrom, journeyInstance.State.HoldsFrom);
    }

    [Fact]
    public async Task Post_FromCya_WhenAwardDateIsEntered_RedirectsToCya()
    {
        // Arrange
        var holdsFrom = Clock.Today.AddYears(-1);
        var newAwardDate = holdsFrom.AddMonths(1);
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.HoldsFromRequired == FieldRequirement.Mandatory)
            .SingleRandom();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.HoldsFromRequired == FieldRequirement.Mandatory)
            .SingleRandom()
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

        var request = new HttpRequestMessage(HttpMethod.Post, $"/routes/add/holds-from?personId={person.PersonId}&FromCheckAnswers=True&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "HoldsFrom.Day", $"{newAwardDate:%d}" },
                { "HoldsFrom.Month", $"{newAwardDate:%M}" },
                { "HoldsFrom.Year", $"{newAwardDate:yyyy}" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/routes/add/check-answers?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(newAwardDate, journeyInstance.State.HoldsFrom);
    }

    [Fact]
    public async Task Post_WhenNoDateIsEntered_ReturnsError()
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.HoldsFromRequired == FieldRequirement.Mandatory)
            .SingleRandom();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.HoldsFromRequired == FieldRequirement.Mandatory)
            .SingleRandom()
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

        var request = new HttpRequestMessage(HttpMethod.Post, $"/routes/add/holds-from?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "HoldsFrom", "Enter the date they first held this professional status");
    }

    [Fact]
    public async Task Post_WhenFutureDateIsEntered_ReturnsError()
    {
        // Arrange
        var holdsDate = Clock.UtcNow.AddDays(1);
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.HoldsFromRequired == FieldRequirement.Mandatory)
            .SingleRandom();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.HoldsFromRequired == FieldRequirement.Mandatory)
            .SingleRandom()
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

        var request = new HttpRequestMessage(HttpMethod.Post, $"/routes/add/holds-from?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "HoldsFrom.Day", $"{holdsDate:%d}" },
                { "HoldsFrom.Month", $"{holdsDate:%M}" },
                { "HoldsFrom.Year", $"{holdsDate:yyyy}" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "HoldsFrom", "The date they first held this professional status must not be in the future");
    }

    [Fact]
    public async Task Cancel_RedirectsToExpectedPage()
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .SingleRandom();
        var status = ProfessionalStatusStatusRegistry.All
            .SingleRandom()
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

        var request = new HttpRequestMessage(HttpMethod.Get, $"/routes/add/holds-from?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

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

    [Theory]
    [HttpMethods(TestHttpMethods.GetAndPost)]
    public async Task PersonIsDeactivated_ReturnsBadRequest(HttpMethod httpMethod)
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).Single(r => r.Name == "Apply for Qualified Teacher Status in England");
        var startDate = new DateOnly(2024, 01, 01);
        var endDate = startDate.AddMonths(1);
        var holdsFrom = endDate.AddDays(1);
        var person = await TestData.CreatePersonAsync();
        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(person.Person);
            person.Person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });
        var addRouteState = new AddRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(RouteToProfessionalStatusStatus.Holds)
            .WithHoldsFrom(holdsFrom)
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(person.PersonId, addRouteState);

        var request = new HttpRequestMessage(httpMethod, $"/routes/add/holds-from?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    private Task<JourneyInstance<AddRouteState>> CreateJourneyInstanceAsync(Guid personId, AddRouteState? state = null) =>
        CreateJourneyInstance(
           JourneyNames.AddRouteToProfessionalStatus,
           state ?? new AddRouteState(),
           new KeyValuePair<string, object>("personId", personId));

}
