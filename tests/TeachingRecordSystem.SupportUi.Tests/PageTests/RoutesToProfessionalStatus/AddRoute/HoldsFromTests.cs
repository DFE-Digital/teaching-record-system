using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.RoutesToProfessionalStatus.AddRoute;

public class HoldsFromTests(HostFixture hostFixture) : AddRouteTestBase(hostFixture)
{
    [Theory]
    [InlineData("Apply for Qualified Teacher Status in England", RouteToProfessionalStatusStatus.Holds, false)]
    [InlineData("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.Holds, false)]
    public async Task Get_FieldsMarkedAsOptional_BasedOnRouteAndStatusFieldRequirements(string routeName, RouteToProfessionalStatusStatus status, bool expectFieldsToBeOptional)
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).Single(r => r.Name == routeName);
        var holdsFrom = new DateOnly(2024, 02, 02);
        var person = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new AddRouteState
            {
                RouteToProfessionalStatusId = route.RouteToProfessionalStatusTypeId,
                Status = status,
                HoldsFrom = holdsFrom
            });

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
    [InlineData("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.Holds, false)]
    public async Task Post_MissingValues_ValidOrInvalid_BasedOnRouteAndStatusFieldRequirements(string routeName, RouteToProfessionalStatusStatus status, bool expectFieldsToBeOptional)
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).Single(r => r.Name == routeName);
        var holdsFrom = new DateOnly(2024, 02, 02);
        var person = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new AddRouteState
            {
                RouteToProfessionalStatusId = route.RouteToProfessionalStatusTypeId,
                Status = status,
                HoldsFrom = holdsFrom
            });

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
        var holdsFrom = new DateOnly(2024, 02, 02);
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.HoldsFromRequired == FieldRequirement.Mandatory)
            .SingleRandom();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.HoldsFromRequired == FieldRequirement.Mandatory)
            .SingleRandom()
            .Value;
        var person = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new AddRouteState
            {
                RouteToProfessionalStatusId = route.RouteToProfessionalStatusTypeId,
                Status = status,
                HoldsFrom = holdsFrom
            });

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
    public async Task Post_WhenHoldsFromDateIsEntered_SavesDateAndRedirectsToInductionExemptionPage()
    {
        // Arrange
        var holdsFrom = TimeProvider.Today.AddYears(-1);
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.HoldsFromRequired == FieldRequirement.Mandatory
                && r.InductionExemptionRequired == FieldRequirement.Mandatory
                && r.InductionExemptionReason is not null
                && !r.InductionExemptionReason.RouteImplicitExemption)
            .SingleRandom();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.HoldsFromRequired == FieldRequirement.Mandatory && s.InductionExemptionRequired == FieldRequirement.Mandatory)
            .SingleRandom()
            .Value;
        var person = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new AddRouteState
            {
                RouteToProfessionalStatusId = route.RouteToProfessionalStatusTypeId,
                Status = status
            });

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
        Assert.Equal(holdsFrom, GetJourneyInstanceState(journeyInstance)!.HoldsFrom);
    }

    [Fact]
    public async Task Post_ImplicitExemptionRoute_WhenHoldsFromDateIsEntered_SavesDateAndRedirectsToNextPage()
    {
        // Arrange
        var holdsFrom = TimeProvider.Today.AddYears(-1);
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.InductionExemptionReason is not null && r.InductionExemptionReason.RouteImplicitExemption)
            .SingleRandom();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.HoldsFromRequired == FieldRequirement.Mandatory && s.InductionExemptionRequired == FieldRequirement.Mandatory)
            .SingleRandom()
            .Value;
        var person = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new AddRouteState
            {
                RouteToProfessionalStatusId = route.RouteToProfessionalStatusTypeId,
                Status = status
            });

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
        Assert.Equal(holdsFrom, GetJourneyInstanceState(journeyInstance)!.HoldsFrom);
    }

    [Fact]
    public async Task Post_FromCheckAnswers_WhenHoldsFromDateIsEntered_RedirectsToCheckAnswers()
    {
        // Arrange
        var holdsFrom = TimeProvider.Today.AddYears(-1);
        var newHoldsFrom = holdsFrom.AddMonths(1);
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.HoldsFromRequired == FieldRequirement.Mandatory)
            .SingleRandom();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.HoldsFromRequired == FieldRequirement.Mandatory)
            .SingleRandom()
            .Value;
        var person = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new AddRouteState
            {
                RouteToProfessionalStatusId = route.RouteToProfessionalStatusTypeId,
                Status = status,
                HoldsFrom = holdsFrom
            });

        var checkAnswersUrl = GetCheckAnswersReturnUrl(journeyInstance, person.PersonId);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/routes/add/holds-from?personId={person.PersonId}&returnUrl={Uri.EscapeDataString(checkAnswersUrl)}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "HoldsFrom.Day", $"{newHoldsFrom:%d}" },
                { "HoldsFrom.Month", $"{newHoldsFrom:%M}" },
                { "HoldsFrom.Year", $"{newHoldsFrom:yyyy}" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(checkAnswersUrl, response.Headers.Location?.OriginalString);
        Assert.Equal(newHoldsFrom, GetJourneyInstanceState(journeyInstance)!.HoldsFrom);
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

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new AddRouteState
            {
                RouteToProfessionalStatusId = route.RouteToProfessionalStatusTypeId,
                Status = status
            });

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
        var holdsFrom = TimeProvider.Today.AddDays(1);
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.HoldsFromRequired == FieldRequirement.Mandatory)
            .SingleRandom();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.HoldsFromRequired == FieldRequirement.Mandatory)
            .SingleRandom()
            .Value;
        var person = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new AddRouteState
            {
                RouteToProfessionalStatusId = route.RouteToProfessionalStatusTypeId,
                Status = status
            });

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
        await AssertEx.HtmlResponseHasErrorAsync(response, "HoldsFrom", "The date they first held this professional status must not be in the future");
    }

    [Fact]
    public async Task Post_Cancel_DeletesJourneyAndRedirectsToQualifications()
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

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new AddRouteState
            {
                RouteToProfessionalStatusId = route.RouteToProfessionalStatusTypeId,
                Status = status
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/routes/add/holds-from?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder().Add("Cancel", bool.TrueString)
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/persons/{person.PersonId}/qualifications", response.Headers.Location?.OriginalString);
        Assert.Null(GetJourneyInstanceState(journeyInstance));
    }

    [Fact]
    public async Task Get_QuestionIsNotAskedForRouteAndStatus_RedirectsToCheckAnswers()
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).Single(r => r.Name == "Postgraduate Teaching Apprenticeship");
        var person = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new AddRouteState
            {
                RouteToProfessionalStatusId = route.RouteToProfessionalStatusTypeId,
                Status = RouteToProfessionalStatusStatus.InTraining
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/routes/add/holds-from?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/routes/add/check-answers?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);
    }

    [Theory]
    [HttpMethods(TestHttpMethods.GetAndPost)]
    public async Task PersonIsDeactivated_ReturnsBadRequest(HttpMethod httpMethod)
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Single(r => r.Name == "Apply for Qualified Teacher Status in England");
        var holdsFrom = new DateOnly(2024, 02, 02);
        var person = await TestData.CreatePersonAsync();
        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(person);
            person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new AddRouteState
            {
                RouteToProfessionalStatusId = route.RouteToProfessionalStatusTypeId,
                Status = RouteToProfessionalStatusStatus.Holds,
                HoldsFrom = holdsFrom
            });

        var request = new HttpRequestMessage(httpMethod, $"/routes/add/holds-from?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }
}
