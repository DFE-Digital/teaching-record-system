using AngleSharp.Html.Dom;
using TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.RoutesToProfessionalStatus.EditRoute;

public class HoldsFromTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Theory]
    [InlineData("Apply for Qualified Teacher Status in England", RouteToProfessionalStatusStatus.Holds, false, false)]
    [InlineData("Apply for Qualified Teacher Status in England", RouteToProfessionalStatusStatus.Holds, true, false)]
    [InlineData("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.InTraining, false, true)]
    [InlineData("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.InTraining, true, true)]
    [InlineData("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.Holds, false, false)]
    [InlineData("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.Holds, true, false)]
    public async Task Get_FieldsMarkedAsOptional_BasedOnRouteAndStatusFieldRequirements(string routeName, RouteToProfessionalStatusStatus status, bool statusEditedDuringCurrentJourney, bool expectFieldsToBeOptional)
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).Single(r => r.Name == routeName);
        var startDate = new DateOnly(2024, 01, 01);
        var endDate = startDate.AddMonths(1);
        var holdsFrom = endDate.AddDays(1);
        var subjects = (await ReferenceDataCache.GetTrainingSubjectsAsync()).Where(s => !s.Name.Contains('\'')).Take(1).Select(s => s.TrainingSubjectId).ToArray();
        var trainingProvider = (await ReferenceDataCache.GetTrainingProvidersAsync()).Where(s => !s.Name.Contains('\'')).SingleRandom();
        var degreeType = (await ReferenceDataCache.GetDegreeTypesAsync()).SingleRandom();
        var country = (await ReferenceDataCache.GetTrainingCountriesAsync()).SingleRandom();

        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(statusEditedDuringCurrentJourney ? RouteToProfessionalStatusStatus.Deferred : status)
                .WithTrainingStartDate(startDate)
                .WithTrainingEndDate(endDate)
                .WithHoldsFrom(holdsFrom)
                .WithTrainingSubjectIds(subjects)
                .WithTrainingProviderId(trainingProvider.TrainingProviderId)
                .WithDegreeTypeId(degreeType.DegreeTypeId)
                .WithTrainingCountryId(country.CountryId)));

        var builder = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(statusEditedDuringCurrentJourney ? RouteToProfessionalStatusStatus.Deferred : status)
            .WithTrainingStartDate(startDate)
            .WithTrainingEndDate(endDate)
            .WithHoldsFrom(holdsFrom)
            .WithTrainingSubjectIds(subjects)
            .WithTrainingProviderId(trainingProvider.TrainingProviderId)
            .WithDegreeTypeId(degreeType.DegreeTypeId)
            .WithTrainingCountryId(country.CountryId);

        if (statusEditedDuringCurrentJourney)
        {
            builder = builder.WithEditRouteStatusState(builder => builder
                .WithStatus(status)
                .WithEndDate(endDate));
        }

        var editRouteState = builder.Build();

        var qualificationid = person.ProfessionalStatuses.First().QualificationId;
        var journeyInstance = await CreateJourneyInstanceAsync(qualificationid, editRouteState);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/routes/{qualificationid}/edit/holds-from?{journeyInstance.GetUniqueIdQueryParameter()}");

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
    [InlineData("Apply for Qualified Teacher Status in England", RouteToProfessionalStatusStatus.Holds, false, false)]
    [InlineData("Apply for Qualified Teacher Status in England", RouteToProfessionalStatusStatus.Holds, true, false)]
    [InlineData("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.InTraining, false, true)]
    [InlineData("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.InTraining, true, true)]
    [InlineData("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.Holds, false, false)]
    [InlineData("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.Holds, true, false)]
    public async Task Post_MissingValues_ValidOrInvalid_BasedOnRouteAndStatusFieldRequirements(string routeName, RouteToProfessionalStatusStatus status, bool statusEditedDuringCurrentJourney, bool expectFieldsToBeOptional)
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).Single(r => r.Name == routeName);
        var startDate = new DateOnly(2024, 01, 01);
        var endDate = startDate.AddMonths(1);
        var holdsFrom = endDate.AddDays(1);
        var subjects = (await ReferenceDataCache.GetTrainingSubjectsAsync()).Where(s => !s.Name.Contains('\'')).Take(1).Select(s => s.TrainingSubjectId).ToArray();
        var trainingProvider = (await ReferenceDataCache.GetTrainingProvidersAsync()).Where(s => !s.Name.Contains('\'')).SingleRandom();
        var degreeType = (await ReferenceDataCache.GetDegreeTypesAsync()).SingleRandom();
        var country = (await ReferenceDataCache.GetTrainingCountriesAsync()).SingleRandom();

        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(statusEditedDuringCurrentJourney ? RouteToProfessionalStatusStatus.Deferred : status)
                .WithTrainingStartDate(startDate)
                .WithTrainingEndDate(endDate)
                .WithHoldsFrom(holdsFrom)
                .WithTrainingSubjectIds(subjects)
                .WithTrainingProviderId(trainingProvider.TrainingProviderId)
                .WithDegreeTypeId(degreeType.DegreeTypeId)
                .WithTrainingCountryId(country.CountryId)));

        var builder = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(statusEditedDuringCurrentJourney ? RouteToProfessionalStatusStatus.Deferred : status)
            .WithTrainingStartDate(startDate)
            .WithTrainingEndDate(endDate)
            .WithHoldsFrom(holdsFrom)
            .WithTrainingSubjectIds(subjects)
            .WithTrainingProviderId(trainingProvider.TrainingProviderId)
            .WithDegreeTypeId(degreeType.DegreeTypeId)
            .WithTrainingCountryId(country.CountryId);

        if (statusEditedDuringCurrentJourney)
        {
            builder = builder.WithEditRouteStatusState(builder => builder
                .WithStatus(status)
                .WithEndDate(endDate));
        }

        var editRouteState = builder.Build();

        var qualificationid = person.ProfessionalStatuses.First().QualificationId;
        var journeyInstance = await CreateJourneyInstanceAsync(qualificationid, editRouteState);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/routes/{qualificationid}/edit/holds-from?{journeyInstance.GetUniqueIdQueryParameter()}");

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
    public async Task Get_BackLinkContainsExpected()
    {
        // Arrange
        var startDate = new DateOnly(2024, 01, 01);
        var endDate = new DateOnly(2025, 01, 01);
        var holdsFrom = endDate;
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.HoldsFromRequired == FieldRequirement.Mandatory)
            .SingleRandom();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.HoldsFromRequired == FieldRequirement.Mandatory)
            .SingleRandom()
            .Value;
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(status)
                .WithHoldsFrom(holdsFrom)));
        var qualificationId = person.ProfessionalStatuses.First().QualificationId;
        var editRouteState = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(status)
            .WithTrainingStartDate(startDate)
            .WithTrainingEndDate(endDate)
            .WithHoldsFrom(holdsFrom)
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            editRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Get, $"/routes/{qualificationId}/edit/holds-from?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var document = await response.GetDocumentAsync();
        var backlink = document.QuerySelector(".govuk-back-link") as IHtmlAnchorElement;
        Assert.Contains($"/routes/{qualificationId}/edit/detail?{journeyInstance.GetUniqueIdQueryParameter()}", backlink!.Href);
    }

    [Fact]
    public async Task Get_MovingToAHoldsStatusJourney_BackLinkContainsExpected()
    {
        // Arrange
        var startDate = new DateOnly(2024, 01, 01);
        var endDate = new DateOnly(2025, 01, 01);
        var holdsFrom = endDate;
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.HoldsFromRequired == FieldRequirement.Mandatory)
            .SingleRandom();
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(RouteToProfessionalStatusStatus.InTraining)));
        var qualificationId = person.ProfessionalStatuses.First().QualificationId;
        var editRouteState = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(RouteToProfessionalStatusStatus.InTraining)
            .WithTrainingStartDate(startDate)
            .WithTrainingEndDate(endDate)
            .WithEditRouteStatusState(builder => builder
                .WithStatus(RouteToProfessionalStatusStatus.Holds)
                .WithEndDate(endDate))
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            editRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Get, $"/routes/{qualificationId}/edit/holds-from?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var document = await response.GetDocumentAsync();
        var backlink = document.QuerySelector(".govuk-back-link") as IHtmlAnchorElement;
        Assert.Contains($"/routes/{qualificationId}/edit/status?{journeyInstance.GetUniqueIdQueryParameter()}", backlink!.Href);
    }

    [Fact]
    public async Task Post_StatusIsAlreadyHoldsJourney_HoldsFromIsEntered_SavesDateAndRedirectsToDetail()
    {
        // Arrange
        var startDate = Clock.Today.AddYears(-1);
        var endDate = startDate.AddMonths(1);
        var holdsFrom = endDate;
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.HoldsFromRequired == FieldRequirement.Mandatory)
            .SingleRandom();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.HoldsFromRequired == FieldRequirement.Mandatory)
            .SingleRandom()
            .Value;
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(status)
                .WithHoldsFrom(holdsFrom.AddDays(-1))));
        var qualificationId = person.ProfessionalStatuses.First().QualificationId;
        var editRouteState = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(status)
            .WithTrainingStartDate(startDate)
            .WithTrainingEndDate(endDate)
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            editRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Post, $"/routes/{qualificationId}/edit/holds-from?{journeyInstance.GetUniqueIdQueryParameter()}")
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
        Assert.Equal($"/routes/{qualificationId}/edit/detail?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(holdsFrom, journeyInstance.State.HoldsFrom);
    }

    [Fact]
    public async Task Post_StatusChangedToHoldsJourney_RouteAllowsExemption_HoldsFromIsEntered_SavesDateAndRedirectsToInductionExemptionPage()
    {
        // Arrange
        var status = RouteToProfessionalStatusStatus.Holds;
        var startDate = Clock.Today.AddYears(-1);
        var endDate = startDate.AddMonths(1);
        var holdsFrom = endDate;
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.Name == "Northern Irish Recognition") // an induction exemption route that requires the exemption question
            .SingleRandom();
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(RouteToProfessionalStatusStatus.InTraining)));
        var qualificationId = person.ProfessionalStatuses.First().QualificationId;
        var editRouteState = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(status)
            .WithCurrentStatus(person.ProfessionalStatuses.First().Status)
            .WithTrainingStartDate(startDate)
            .WithTrainingEndDate(endDate)
            .WithEditRouteStatusState(builder => builder
                .WithStatus(status)
                .WithEndDate(endDate))
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            editRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Post, $"/routes/{qualificationId}/edit/holds-from?{journeyInstance.GetUniqueIdQueryParameter()}")
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
        Assert.Equal($"/routes/{qualificationId}/edit/induction-exemption?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(holdsFrom, journeyInstance.State.EditStatusState!.HoldsFrom);
    }

    [Fact]
    public async Task Post_StatusHoldsJourney_RouteHasImplicitExemption_HoldsFromIsEntered_SavesDateAndExemptionAndRedirectsToDetailPage()
    {
        // Arrange
        var status = RouteToProfessionalStatusStatus.Holds;
        var startDate = Clock.Today.AddYears(-1);
        var endDate = startDate.AddMonths(1);
        var holdsFrom = endDate;
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()) // an induction exemption route who's exemption is implicit and therefore doesn't require the exemption question
            .Where(r => r.InductionExemptionReasonId.HasValue)
            .Join(
                (await ReferenceDataCache.GetInductionExemptionReasonsAsync()).Where(e => e.RouteImplicitExemption),
                r => r.InductionExemptionReasonId,
                e => e.InductionExemptionReasonId,
                (r, e) => r
            )
            .SingleRandom();
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(RouteToProfessionalStatusStatus.InTraining)));
        var qualificationId = person.ProfessionalStatuses.First().QualificationId;
        var editRouteState = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(status)
            .WithCurrentStatus(person.ProfessionalStatuses.First().Status)
            .WithTrainingStartDate(startDate)
            .WithTrainingEndDate(endDate)
            .WithEditRouteStatusState(builder => builder
                .WithStatus(status)
                .WithEndDate(endDate)
                .WithHasInductionExemption(true)
                .WithRouteImplicitExemption(true))
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            editRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Post, $"/routes/{qualificationId}/edit/holds-from?{journeyInstance.GetUniqueIdQueryParameter()}")
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
        Assert.Equal($"/routes/{qualificationId}/edit/detail?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(holdsFrom, journeyInstance.State.HoldsFrom);
        Assert.Equal(true, journeyInstance.State.IsExemptFromInduction);
    }

    [Fact]
    public async Task Post_WhenNoHoldsFromIsEntered_ReturnsError()
    {
        // Arrange
        var startDate = new DateOnly(2024, 01, 01);
        var endDate = new DateOnly(2025, 01, 01);
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.HoldsFromRequired == FieldRequirement.Mandatory)
            .SingleRandom();
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(RouteToProfessionalStatusStatus.InTraining)));
        var qualificationId = person.ProfessionalStatuses.First().QualificationId;
        var editRouteState = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(RouteToProfessionalStatusStatus.Holds)
            .WithTrainingStartDate(startDate)
            .WithTrainingEndDate(endDate)
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            editRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Post, $"/routes/{qualificationId}/edit/holds-from?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>())
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "HoldsFrom", "Enter the date they first held this professional status");
    }

    [Fact]
    public async Task Post_WhenFutureHoldsFromDateIsEntered_ReturnsError()
    {
        // Arrange
        var startDate = Clock.Today.AddYears(-1);
        var endDate = startDate.AddMonths(1);
        var holdsFrom = Clock.Today.AddDays(1);
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.HoldsFromRequired == FieldRequirement.Mandatory)
            .SingleRandom();
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(RouteToProfessionalStatusStatus.InTraining)));
        var qualificationId = person.ProfessionalStatuses.First().QualificationId;
        var editRouteState = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(RouteToProfessionalStatusStatus.Holds)
            .WithTrainingStartDate(startDate)
            .WithTrainingEndDate(endDate)
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            editRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Post, $"/routes/{qualificationId}/edit/holds-from?{journeyInstance.GetUniqueIdQueryParameter()}")
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
    public async Task Cancel_DeleteJourneyAndRedirectsToExpectedPage()
    {
        // Arrange
        var startDate = new DateOnly(2024, 01, 01);
        var endDate = new DateOnly(2025, 01, 01);
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.HoldsFromRequired == FieldRequirement.Mandatory)
            .SingleRandom();
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(RouteToProfessionalStatusStatus.InTraining)));
        var qualificationId = person.ProfessionalStatuses.First().QualificationId;
        var editRouteState = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(RouteToProfessionalStatusStatus.Holds)
            .WithTrainingStartDate(startDate)
            .WithTrainingEndDate(endDate)
            .WithValidChangeReasonOption()
            .WithDefaultChangeReasonNoUploadFileDetail()
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId, editRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Get, $"/routes/{qualificationId}/edit/holds-from?{journeyInstance.GetUniqueIdQueryParameter()}");

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

    [Theory]
    [HttpMethods(TestHttpMethods.GetAndPost)]
    public async Task PersonIsDeactivated_ReturnsBadRequest(HttpMethod httpMethod)
    {
        // Arrange
        var startDate = new DateOnly(2024, 01, 01);
        var endDate = new DateOnly(2025, 01, 01);
        var holdsFrom = endDate;
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.HoldsFromRequired == FieldRequirement.Mandatory)
            .SingleRandom();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.HoldsFromRequired == FieldRequirement.Mandatory)
            .SingleRandom()
            .Value;
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(status)
                .WithHoldsFrom(holdsFrom)));
        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(person.Person);
            person.Person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });
        var qualificationId = person.ProfessionalStatuses.First().QualificationId;
        var editRouteState = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(status)
            .WithTrainingStartDate(startDate)
            .WithTrainingEndDate(endDate)
            .WithHoldsFrom(holdsFrom)
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationId,
            editRouteState
            );

        var request = new HttpRequestMessage(httpMethod, $"/routes/{qualificationId}/edit/holds-from?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }

    private Task<JourneyInstance<EditRouteState>> CreateJourneyInstanceAsync(Guid qualificationId, EditRouteState? state = null) =>
        CreateJourneyInstance(
           JourneyNames.EditRouteToProfessionalStatus,
           state ?? new EditRouteState(),
           new KeyValuePair<string, object>("qualificationId", qualificationId));

}
