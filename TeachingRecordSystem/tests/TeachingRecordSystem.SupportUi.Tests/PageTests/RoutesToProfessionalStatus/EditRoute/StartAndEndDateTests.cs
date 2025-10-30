using AngleSharp.Html.Dom;
using TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.EditRoute;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.RoutesToProfessionalStatus.EditRoute;

public class StartAndEndDateTests(HostFixture hostFixture) : TestBase(hostFixture)
{
    [Theory]
    [InlineData("Apply for Qualified Teacher Status in England", RouteToProfessionalStatusStatus.Holds, false, true)]
    [InlineData("Apply for Qualified Teacher Status in England", RouteToProfessionalStatusStatus.Holds, true, true)]
    [InlineData("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.InTraining, false, true)]
    [InlineData("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.InTraining, true, true)]
    [InlineData("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.Holds, false, true)]
    [InlineData("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.Holds, true, true)]
    [InlineData("Early Years Teacher Degree Apprenticeship", RouteToProfessionalStatusStatus.Holds, false, true)]
    [InlineData("Early Years Teacher Degree Apprenticeship", RouteToProfessionalStatusStatus.Holds, true, true)]
    [InlineData("Test Route With Mandatory Start/End Dates", RouteToProfessionalStatusStatus.Holds, false, false)]
    [InlineData("Test Route With Mandatory Start/End Dates", RouteToProfessionalStatusStatus.Holds, true, false)]
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

        var request = new HttpRequestMessage(HttpMethod.Get, $"/routes/{qualificationid}/edit/start-and-end-date?{journeyInstance.GetUniqueIdQueryParameter()}");

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
    [InlineData("Apply for Qualified Teacher Status in England", RouteToProfessionalStatusStatus.Holds, false, true)]
    [InlineData("Apply for Qualified Teacher Status in England", RouteToProfessionalStatusStatus.Holds, true, true)]
    [InlineData("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.InTraining, false, true)]
    [InlineData("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.InTraining, true, true)]
    [InlineData("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.Holds, false, true)]
    [InlineData("Postgraduate Teaching Apprenticeship", RouteToProfessionalStatusStatus.Holds, true, true)]
    [InlineData("Early Years Teacher Degree Apprenticeship", RouteToProfessionalStatusStatus.Holds, false, true)]
    [InlineData("Early Years Teacher Degree Apprenticeship", RouteToProfessionalStatusStatus.Holds, true, true)]
    [InlineData("Test Route With Mandatory Start/End Dates", RouteToProfessionalStatusStatus.Holds, false, false)]
    [InlineData("Test Route With Mandatory Start/End Dates", RouteToProfessionalStatusStatus.Holds, true, false)]
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

        var request = new HttpRequestMessage(HttpMethod.Post, $"/routes/{qualificationid}/edit/start-and-end-date?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        if (expectFieldsToBeOptional)
        {
            Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        }
        else
        {
            await AssertEx.HtmlResponseHasErrorAsync(response, "TrainingStartDate", "Enter a start date");
            await AssertEx.HtmlResponseHasErrorAsync(response, "TrainingEndDate", "Enter an end date");
        }
    }

    [Fact]
    public async Task Get_ShowsPreviouslyStoredEntry()
    {
        // Arrange
        var startDate = new DateOnly(2024, 01, 01);
        var endDate = startDate.AddMonths(1);
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.TrainingStartDateRequired == FieldRequirement.Optional && r.TrainingEndDateRequired == FieldRequirement.Optional)
            .SingleRandom();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.TrainingStartDateRequired == FieldRequirement.Optional
                && s.TrainingEndDateRequired == FieldRequirement.Optional
                && s.HoldsFromRequired == FieldRequirement.NotApplicable)
            .SingleRandom()
            .Value;
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(status)));
        var qualificationid = person.ProfessionalStatuses.First().QualificationId;
        var editRouteState = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(status)
            .WithTrainingStartDate(startDate)
            .WithTrainingEndDate(endDate)
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(qualificationid, editRouteState);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/routes/{qualificationid}/edit/start-and-end-date?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        Assert.Equal(startDate.Day.ToString(), doc.QuerySelector("#TrainingStartDate\\.Day")?.GetAttribute("value"));
        Assert.Equal(startDate.Month.ToString(), doc.QuerySelector("#TrainingStartDate\\.Month")?.GetAttribute("value"));
        Assert.Equal(startDate.Year.ToString(), doc.QuerySelector("#TrainingStartDate\\.Year")?.GetAttribute("value"));
        Assert.Equal(endDate.Day.ToString(), doc.QuerySelector("#TrainingEndDate\\.Day")?.GetAttribute("value"));
        Assert.Equal(endDate.Month.ToString(), doc.QuerySelector("#TrainingEndDate\\.Month")?.GetAttribute("value"));
        Assert.Equal(endDate.Year.ToString(), doc.QuerySelector("#TrainingEndDate\\.Year")?.GetAttribute("value"));
    }

    [Fact]
    public async Task Post_WhenTrainingStartAndEndDateIsEntered_SavesDateAndRedirectsToDetail()
    {
        // Arrange
        var startDate = new DateOnly(2024, 01, 01);
        var endDate = new DateOnly(2025, 01, 01);
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.TrainingStartDateRequired == FieldRequirement.Optional && r.TrainingEndDateRequired == FieldRequirement.Optional)
            .SingleRandom();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.TrainingStartDateRequired == FieldRequirement.Optional && s.TrainingEndDateRequired == FieldRequirement.Optional && s.HoldsFromRequired == FieldRequirement.NotApplicable)
            .SingleRandom()
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

        var request = new HttpRequestMessage(HttpMethod.Post, $"/routes/{qualificationid}/edit/start-and-end-date?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "TrainingStartDate.Day", $"{startDate:%d}" },
                { "TrainingStartDate.Month", $"{startDate:%M}" },
                { "TrainingStartDate.Year", $"{startDate:yyyy}" },
                { "TrainingEndDate.Day", $"{endDate:%d}" },
                { "TrainingEndDate.Month", $"{endDate:%M}" },
                { "TrainingEndDate.Year", $"{endDate:yyyy}" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/routes/{qualificationid}/edit/detail?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(startDate, journeyInstance.State.TrainingStartDate);
        Assert.Equal(endDate, journeyInstance.State.TrainingEndDate);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Post_EndDateIsEqualOrBeforeStartDate_ReturnsError(int daysAfter)
    {
        // Arrange
        var startDate = new DateOnly(2024, 01, 01);
        var endDate = startDate.AddDays(daysAfter);
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.TrainingStartDateRequired == FieldRequirement.Optional && r.TrainingEndDateRequired == FieldRequirement.Optional)
            .SingleRandom();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.TrainingStartDateRequired == FieldRequirement.Optional && s.TrainingEndDateRequired == FieldRequirement.Optional && s.HoldsFromRequired == FieldRequirement.NotApplicable)
            .SingleRandom()
            .Value;
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(status)));
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

        var request = new HttpRequestMessage(HttpMethod.Post, $"/routes/{qualificationId}/edit/start-and-end-date?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "TrainingStartDate.Day", $"{startDate:%d}" },
                { "TrainingStartDate.Month", $"{startDate:%M}" },
                { "TrainingStartDate.Year", $"{startDate:yyyy}" },
                { "TrainingEndDate.Day", $"{endDate:%d}" },
                { "TrainingEndDate.Month", $"{endDate:%M}" },
                { "TrainingEndDate.Year", $"{endDate:yyyy}" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertEx.HtmlResponseHasErrorAsync(response, "TrainingEndDate", "End date must be after start date");
    }

    [Fact]
    public async Task Post_NotStatusHoldsJourney_StartAndEndDateIsEntered_SavesDatesAndRedirectsToDetail()
    {
        // Arrange
        var startDate = new DateOnly(2024, 01, 01);
        var endDate = new DateOnly(2025, 01, 01);
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.TrainingEndDateRequired == FieldRequirement.Optional)
            .SingleRandom();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.TrainingEndDateRequired == FieldRequirement.Optional
                && s.Value != RouteToProfessionalStatusStatus.Holds)
            .SingleRandom()
            .Value;
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(status)));
        var qualificationid = person.ProfessionalStatuses.First().QualificationId;
        var editRouteState = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(status)
            .WithTrainingStartDate(startDate)
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationid,
            editRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Post, $"/routes/{qualificationid}/edit/start-and-end-date?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "TrainingStartDate.Day", $"{startDate:%d}" },
                { "TrainingStartDate.Month", $"{startDate:%M}" },
                { "TrainingStartDate.Year", $"{startDate:yyyy}" },
                { "TrainingEndDate.Day", $"{endDate:%d}" },
                { "TrainingEndDate.Month", $"{endDate:%M}" },
                { "TrainingEndDate.Year", $"{endDate:yyyy}" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/routes/{qualificationid}/edit/detail?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(startDate, journeyInstance.State.TrainingStartDate);
        Assert.Equal(endDate, journeyInstance.State.TrainingEndDate);
    }

    [Fact]
    public async Task Post_StatusHoldsJourney_StartAndEndDateIsEntered_SavesDatesAndRedirectsToHoldsDate()
    {
        // Arrange
        var status = RouteToProfessionalStatusStatus.Holds;
        var startDate = new DateOnly(2024, 01, 01);
        var endDate = new DateOnly(2025, 01, 01);
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.TrainingEndDateRequired == FieldRequirement.Optional)
            .SingleRandom();
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(RouteToProfessionalStatusStatus.InTraining)));
        var qualificationid = person.ProfessionalStatuses.First().QualificationId;
        var editRouteState = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(status)
            .WithCurrentStatus(person.ProfessionalStatuses.First().Status)
            .WithTrainingStartDate(startDate)
                .WithEditRouteStatusState(builder => builder
                .WithStatus(status))
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationid,
            editRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Post, $"/routes/{qualificationid}/edit/start-and-end-date?{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder
            {
                { "TrainingStartDate.Day", $"{startDate:%d}" },
                { "TrainingStartDate.Month", $"{startDate:%M}" },
                { "TrainingStartDate.Year", $"{startDate:yyyy}" },
                { "TrainingEndDate.Day", $"{endDate:%d}" },
                { "TrainingEndDate.Month", $"{endDate:%M}" },
                { "TrainingEndDate.Year", $"{endDate:yyyy}" }
            }
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/routes/{qualificationid}/edit/holds-from?{journeyInstance.GetUniqueIdQueryParameter()}", response.Headers.Location?.OriginalString);
        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.Equal(startDate, journeyInstance.State.TrainingStartDate);
        Assert.Equal(endDate, journeyInstance.State.TrainingEndDate);
    }

    [Fact]
    public async Task Cancel_DeletesJourneyAndRedirectsToExpectedPage()
    {
        // Arrange
        var startDate = new DateOnly(2024, 01, 01);
        var endDate = new DateOnly(2025, 01, 01);
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.TrainingStartDateRequired == FieldRequirement.Optional && r.TrainingEndDateRequired == FieldRequirement.Optional)
            .SingleRandom();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.TrainingStartDateRequired == FieldRequirement.Optional && s.TrainingEndDateRequired == FieldRequirement.Optional && s.HoldsFromRequired == FieldRequirement.NotApplicable)
            .SingleRandom()
            .Value;
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(status)));
        var qualificationid = person.ProfessionalStatuses.First().QualificationId;
        var editRouteState = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(status)
            .WithTrainingStartDate(startDate)
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationid,
            editRouteState
            );

        var request = new HttpRequestMessage(HttpMethod.Get, $"/routes/{qualificationid}/edit/start-and-end-date?{journeyInstance.GetUniqueIdQueryParameter()}");

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
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.TrainingStartDateRequired == FieldRequirement.Optional && r.TrainingEndDateRequired == FieldRequirement.Optional)
            .SingleRandom();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.TrainingStartDateRequired == FieldRequirement.Optional && s.TrainingEndDateRequired == FieldRequirement.Optional && s.HoldsFromRequired == FieldRequirement.NotApplicable)
            .SingleRandom()
            .Value;
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(status)));
        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(person.Person);
            person.Person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });
        var qualificationid = person.ProfessionalStatuses.First().QualificationId;
        var editRouteState = new EditRouteStateBuilder()
            .WithRouteToProfessionalStatusId(route.RouteToProfessionalStatusTypeId)
            .WithStatus(status)
            .WithTrainingStartDate(startDate)
            .Build();

        var journeyInstance = await CreateJourneyInstanceAsync(
            qualificationid,
            editRouteState
            );

        var request = new HttpRequestMessage(httpMethod, $"/routes/{qualificationid}/edit/start-and-end-date?{journeyInstance.GetUniqueIdQueryParameter()}");

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

