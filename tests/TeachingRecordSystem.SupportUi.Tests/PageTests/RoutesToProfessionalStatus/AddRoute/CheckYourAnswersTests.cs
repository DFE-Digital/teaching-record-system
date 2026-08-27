using TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.AddRoute;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.RoutesToProfessionalStatus.AddRoute;

public class CheckYourAnswersTests(HostFixture hostFixture) : AddRouteTestBase(hostFixture)
{
    private const string CountryCode = "AG";

    [Fact]
    public async Task Post_Cancel_DeletesJourneyAndRedirectsToQualifications()
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).Single(r => r.Name == "Northern Irish Recognition");
        var person = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new AddRouteState
            {
                RouteToProfessionalStatusId = route.RouteToProfessionalStatusTypeId,
                Status = RouteToProfessionalStatusStatus.Deferred,
                TrainingCountryId = CountryCode,
                ChangeReason = ChangeReasonOption.AnotherReason,
                ChangeReasonDetail = CreateChangeReasonDetail()
            });

        var request = new HttpRequestMessage(HttpMethod.Post, $"/routes/add/check-answers?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}")
        {
            Content = new FormUrlEncodedContentBuilder().Add("Cancel", bool.TrueString)
        };

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal($"/persons/{person.PersonId}/qualifications", response.Headers.Location?.OriginalString);
        Assert.Null(GetJourneyInstanceState(journeyInstance));

        await WithDbContextAsync(async dbContext =>
        {
            var routes = await dbContext.RouteToProfessionalStatuses.Where(r => r.PersonId == person.PersonId).ToArrayAsync();
            Assert.Empty(routes);
        });
    }

    [Fact]
    public async Task Get_ShowsAnswers_AsExpected()
    {
        // Arrange
        var startDate = TimeProvider.Today.AddYears(-1);
        var endDate = TimeProvider.Today.AddDays(-1);
        var holdsFrom = endDate.AddDays(1);
        var route = await ReferenceDataCache.GetRouteWhereAllFieldsApplyAsync();
        var status = TestDataHelper.GetRouteStatusWhereAllFieldsApply();
        var subjects = (await ReferenceDataCache.GetTrainingSubjectsAsync()).Where(s => !s.Name.Contains('\'')).Take(1);
        var trainingProvider = (await ReferenceDataCache.GetTrainingProvidersAsync()).Where(s => !s.Name.Contains('\'')).SingleRandom();
        var degreeType = (await ReferenceDataCache.GetDegreeTypesAsync()).SingleRandom();
        var country = (await ReferenceDataCache.GetTrainingCountriesAsync()).SingleRandom();
        var person = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new AddRouteState
            {
                RouteToProfessionalStatusId = route.RouteToProfessionalStatusTypeId,
                Status = status,
                TrainingStartDate = startDate,
                TrainingEndDate = endDate,
                HoldsFrom = holdsFrom,
                TrainingProviderId = trainingProvider.TrainingProviderId,
                TrainingCountryId = country.CountryId,
                TrainingSubjectIds = subjects.Select(s => s.TrainingSubjectId).ToArray(),
                TrainingAgeSpecialismType = TrainingAgeSpecialismType.FoundationStage,
                DegreeTypeId = degreeType.DegreeTypeId,
                IsExemptFromInduction = true,
                ChangeReason = ChangeReasonOption.AnotherReason,
                ChangeReasonDetail = CreateChangeReasonDetail()
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/routes/add/check-answers?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertSummaryListRowValueContentMatches("Route", route.Name);
        doc.AssertSummaryListRowValueContentMatches("Status", status.GetTitle());
        doc.AssertSummaryListRowValueContentMatches("Start date", startDate.ToString(WebConstants.DateDisplayFormat));
        doc.AssertSummaryListRowValueContentMatches("End date", endDate.ToString(WebConstants.DateDisplayFormat));
        doc.AssertSummaryListRowValueContentMatches("Held since", holdsFrom.ToString(WebConstants.DateDisplayFormat));
        doc.AssertSummaryListRowValueContentMatches("Training provider", trainingProvider.Name);
        doc.AssertSummaryListRowValueContentMatches("Degree type", degreeType.Name);
        doc.AssertSummaryListRowValueContentMatches("Country of training", country.Name);
        doc.AssertSummaryListRowValueContentMatches("Age range", "Foundation stage");
        doc.AssertSummaryListRowValueContentMatches("Subjects", subjects.Select(s => $"{s.Reference} - {s.Name}"));
    }

    [Fact]
    public async Task Get_ShowsExemptionAnswer_AsExpected()
    {
        // Arrange
        var startDate = TimeProvider.Today.AddYears(-1);
        var endDate = TimeProvider.Today;
        var country = (await ReferenceDataCache.GetTrainingCountriesAsync())
            .SingleRandom();
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.InductionExemptionRequired == FieldRequirement.Mandatory && r.TrainingProviderRequired == FieldRequirement.NotApplicable)
            .SingleRandom();
        var status = ProfessionalStatusStatusRegistry.All
            .Where(s => s.Value.GetInductionExemptionRequirement() == FieldRequirement.Mandatory)
            .SingleRandom();
        var person = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new AddRouteState
            {
                RouteToProfessionalStatusId = route.RouteToProfessionalStatusTypeId,
                Status = status.Value,
                TrainingStartDate = startDate,
                TrainingEndDate = endDate,
                HoldsFrom = endDate,
                TrainingCountryId = country.CountryId,
                IsExemptFromInduction = true,
                ChangeReason = ChangeReasonOption.AnotherReason,
                ChangeReasonDetail = CreateChangeReasonDetail()
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/routes/add/check-answers?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertSummaryListRowValueContentMatches("Held since", endDate.ToString(WebConstants.DateDisplayFormat));
        doc.AssertSummaryListRowValueContentMatches("Induction exemption", "Yes");
    }

    [Fact]
    public async Task Get_ShowsOptionalAnswersNotPopulated_AsExpected()
    {
        // Arrange
        var startDate = TimeProvider.Today.AddYears(-1);
        var endDate = TimeProvider.Today;
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).Single(r => r.Name == "Postgraduate Teaching Apprenticeship");
        var trainingProvider = (await ReferenceDataCache.GetTrainingProvidersAsync()).Where(s => !s.Name.Contains('\''))
            .SingleRandom();
        var degreeType = (await ReferenceDataCache.GetDegreeTypesAsync())
            .SingleRandom();
        var person = await TestData.CreatePersonAsync();

        var journeyInstance = await CreateJourneyInstanceAsync(
            person.PersonId,
            new AddRouteState
            {
                RouteToProfessionalStatusId = route.RouteToProfessionalStatusTypeId,
                Status = RouteToProfessionalStatusStatus.InTraining,
                TrainingStartDate = startDate,
                TrainingEndDate = endDate,
                TrainingCountryId = CountryCode,
                TrainingProviderId = trainingProvider.TrainingProviderId,
                DegreeTypeId = degreeType.DegreeTypeId,
                ChangeReason = ChangeReasonOption.AnotherReason,
                ChangeReasonDetail = CreateChangeReasonDetail()
            });

        var request = new HttpRequestMessage(HttpMethod.Get, $"/routes/add/check-answers?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertSummaryListRowValueContentMatches("Route", route.Name);
        doc.AssertSummaryListRowValueContentMatches("Status", "In training");
        doc.AssertSummaryListRowValueContentMatches("Start date", startDate.ToString(WebConstants.DateDisplayFormat));
        doc.AssertSummaryListRowValueContentMatches("End date", endDate.ToString(WebConstants.DateDisplayFormat));
        doc.AssertSummaryListRowValueContentMatches("Training provider", trainingProvider.Name);
        doc.AssertSummaryListRowValueContentMatches("Age range", "Not provided");
        doc.AssertSummaryListRowValueContentMatches("Subjects", "Not provided");
    }

    [Fact]
    public async Task Post_Confirm_AddsProfessionalStatusCreatesEventDeletesJourneyAndRedirectsWithFlashMessage()
    {
        // Arrange
        var route = await ReferenceDataCache.GetRouteWhereAllFieldsApplyAsync();
        var status = TestDataHelper.GetRouteStatusWhereAllFieldsApply();
        var trainingProvider = (await ReferenceDataCache.GetTrainingProvidersAsync()).Where(s => !s.Name.Contains('\'')).First();
        var subjects = (await ReferenceDataCache.GetTrainingSubjectsAsync()).Where(s => !s.Name.Contains('\'')).Take(1);
        var country = (await ReferenceDataCache.GetTrainingCountriesAsync()).SingleRandom();
        var degreeType = (await ReferenceDataCache.GetDegreeTypesAsync()).SingleRandom();
        var ageRange = TrainingAgeSpecialismType.KeyStage3;

        var person = await TestData.CreatePersonAsync();
        var state = new AddRouteState
        {
            RouteToProfessionalStatusId = route.RouteToProfessionalStatusTypeId,
            Status = status,
            TrainingStartDate = TimeProvider.Today.AddYears(-1),
            TrainingEndDate = TimeProvider.Today.AddDays(-1),
            HoldsFrom = TimeProvider.Today,
            TrainingProviderId = trainingProvider.TrainingProviderId,
            TrainingSubjectIds = subjects.Select(s => s.TrainingSubjectId).ToArray(),
            TrainingCountryId = country.CountryId,
            TrainingAgeSpecialismType = ageRange,
            IsExemptFromInduction = true,
            DegreeTypeId = degreeType.DegreeTypeId,
            ChangeReason = ChangeReasonOption.AnotherReason,
            ChangeReasonDetail = CreateChangeReasonDetail()
        };

        var journeyInstance = await CreateJourneyInstanceAsync(person.PersonId, state);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/routes/add/check-answers?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var redirectResponse = await response.FollowRedirectAsync(HttpClient);
        var redirectDoc = await redirectResponse.GetDocumentAsync();
        AssertEx.HtmlDocumentHasFlashNotificationBanner(redirectDoc, "Route to professional status added");

        await WithDbContextAsync(async dbContext =>
        {
            var addedProfessionalStatusRecord = await dbContext.RouteToProfessionalStatuses.FirstOrDefaultAsync(p => p.PersonId == person.PersonId);
            Assert.NotNull(addedProfessionalStatusRecord);
            Assert.Equal(state.IsExemptFromInduction, addedProfessionalStatusRecord.ExemptFromInduction);
            Assert.Equal(state.Status, addedProfessionalStatusRecord.Status);
            Assert.Equal(state.RouteToProfessionalStatusId, addedProfessionalStatusRecord.RouteToProfessionalStatusTypeId);
            Assert.Equal(state.TrainingStartDate, addedProfessionalStatusRecord.TrainingStartDate);
            Assert.Equal(state.TrainingEndDate, addedProfessionalStatusRecord.TrainingEndDate);
            Assert.Equal(state.HoldsFrom, addedProfessionalStatusRecord.HoldsFrom);
            Assert.Equal(state.TrainingProviderId, addedProfessionalStatusRecord.TrainingProviderId);
            Assert.Equal(state.TrainingCountryId, addedProfessionalStatusRecord.TrainingCountryId);
            Assert.Equal(state.TrainingAgeSpecialismType, addedProfessionalStatusRecord.TrainingAgeSpecialismType);
            Assert.Equal(state.TrainingAgeSpecialismRangeFrom, addedProfessionalStatusRecord.TrainingAgeSpecialismRangeFrom);
            Assert.Equal(state.TrainingAgeSpecialismRangeTo, addedProfessionalStatusRecord.TrainingAgeSpecialismRangeTo);
            Assert.Equal(state.TrainingSubjectIds, addedProfessionalStatusRecord.TrainingSubjectIds);
            Assert.Equal(state.DegreeTypeId, addedProfessionalStatusRecord.DegreeTypeId);
        });

        Events.AssertProcessesCreated(p =>
        {
            Assert.Equal(ProcessType.RouteToProfessionalStatusCreating, p.ProcessContext.ProcessType);
            p.AssertProcessHasEvent<RouteToProfessionalStatusCreatedEvent>(actualCreatedEvent =>
            {
                Assert.Equal(person.PersonId, actualCreatedEvent.PersonId);
                Assert.Equal(state.Status, actualCreatedEvent.RouteToProfessionalStatus.Status);
                Assert.Equal(state.RouteToProfessionalStatusId, actualCreatedEvent.RouteToProfessionalStatus.RouteToProfessionalStatusTypeId);
                Assert.Equal(state.TrainingStartDate, actualCreatedEvent.RouteToProfessionalStatus.TrainingStartDate);
                Assert.Equal(state.TrainingEndDate, actualCreatedEvent.RouteToProfessionalStatus.TrainingEndDate);
                Assert.Equal(state.HoldsFrom, actualCreatedEvent.RouteToProfessionalStatus.HoldsFrom);
                Assert.Equal(state.TrainingAgeSpecialismType, actualCreatedEvent.RouteToProfessionalStatus.TrainingAgeSpecialismType);
                Assert.Equal(state.TrainingAgeSpecialismRangeFrom, actualCreatedEvent.RouteToProfessionalStatus.TrainingAgeSpecialismRangeFrom);
                Assert.Equal(state.TrainingAgeSpecialismRangeTo, actualCreatedEvent.RouteToProfessionalStatus.TrainingAgeSpecialismRangeTo);
                Assert.Equal(state.TrainingSubjectIds, actualCreatedEvent.RouteToProfessionalStatus.TrainingSubjectIds);
                Assert.Equal(state.IsExemptFromInduction, actualCreatedEvent.RouteToProfessionalStatus.ExemptFromInduction);
            });
        });

        Assert.Null(GetJourneyInstanceState(journeyInstance));
    }

    [Fact]
    public async Task Post_Confirm_WithHeldQtsRouteTypeUpdatesPersonQtsDateAndHasChangesInEvent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var state = new AddRouteState
        {
            RouteToProfessionalStatusId = new("6F27BDEB-D00A-4EF9-B0EA-26498CE64713"),  // Apply for QTS
            Status = RouteToProfessionalStatusStatus.Holds,
            HoldsFrom = new(2024, 10, 10),
            TrainingCountryId = "GB-SCT",  // Scotland
            IsExemptFromInduction = false,
            ChangeReason = ChangeReasonOption.AnotherReason,
            ChangeReasonDetail = CreateChangeReasonDetail()
        };

        var journeyInstance = await CreateJourneyInstanceAsync(person.PersonId, state);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/routes/add/check-answers?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var updatedPerson = await WithDbContextAsync(dbContext => dbContext.Persons.SingleAsync(p => p.PersonId == person.PersonId));
        Assert.Equal(state.HoldsFrom, updatedPerson.QtsDate);

        Events.AssertProcessesCreated(p =>
        {
            Assert.Equal(ProcessType.RouteToProfessionalStatusCreating, p.ProcessContext.ProcessType);
            p.AssertProcessHasEvent<PersonProfessionalStatusAttributesUpdatedEvent>(attributesEvent =>
            {
                Assert.Equal(state.HoldsFrom, attributesEvent.PersonAttributes.QtsDate);
                Assert.Null(attributesEvent.OldPersonAttributes.QtsDate);
            });
        });
    }

    [Fact]
    public async Task Post_Confirm_WithHeldEytsRouteTypeUpdatesPersonEytsDateAndHasChangesInEvent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).First(r => r.Name == "Early Years ITT Assessment Only");
        var country = (await ReferenceDataCache.GetTrainingCountriesAsync()).SingleRandom();
        var trainingProvider = (await ReferenceDataCache.GetTrainingProvidersAsync()).Where(s => !s.Name.Contains('\'')).First();
        var degreeType = (await ReferenceDataCache.GetDegreeTypesAsync()).SingleRandom();

        var state = new AddRouteState
        {
            RouteToProfessionalStatusId = route.RouteToProfessionalStatusTypeId,
            Status = RouteToProfessionalStatusStatus.Holds,
            TrainingStartDate = TimeProvider.Today.AddYears(-1),
            TrainingEndDate = TimeProvider.Today.AddDays(-1),
            HoldsFrom = TimeProvider.Today,
            TrainingCountryId = country.CountryId,
            IsExemptFromInduction = true,
            TrainingProviderId = trainingProvider.TrainingProviderId,
            DegreeTypeId = degreeType.DegreeTypeId,
            ChangeReason = ChangeReasonOption.AnotherReason,
            ChangeReasonDetail = CreateChangeReasonDetail()
        };

        var journeyInstance = await CreateJourneyInstanceAsync(person.PersonId, state);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/routes/add/check-answers?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var updatedPerson = await WithDbContextAsync(dbContext => dbContext.Persons.SingleAsync(p => p.PersonId == person.PersonId));
        Assert.Equal(state.HoldsFrom, updatedPerson.EytsDate);

        Events.AssertProcessesCreated(p =>
        {
            Assert.Equal(ProcessType.RouteToProfessionalStatusCreating, p.ProcessContext.ProcessType);
            p.AssertProcessHasEvent<PersonProfessionalStatusAttributesUpdatedEvent>(attributesEvent =>
            {
                Assert.Equal(state.HoldsFrom, attributesEvent.PersonAttributes.EytsDate);
                Assert.Null(attributesEvent.OldPersonAttributes.EytsDate);
            });
        });
    }

    [Fact]
    public async Task Post_Confirm_WithHeldEypsRouteTypeUpdatesPersonHasEypsAndHasChangesInEvent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).Single(r => r.Name == "EYPS");
        var country = (await ReferenceDataCache.GetTrainingCountriesAsync()).SingleRandom();
        var degreeType = (await ReferenceDataCache.GetDegreeTypesAsync()).SingleRandom();

        var state = new AddRouteState
        {
            RouteToProfessionalStatusId = route.RouteToProfessionalStatusTypeId,
            Status = RouteToProfessionalStatusStatus.Holds,
            TrainingStartDate = TimeProvider.Today.AddYears(-1),
            TrainingEndDate = TimeProvider.Today.AddDays(-1),
            HoldsFrom = TimeProvider.Today,
            TrainingCountryId = country.CountryId,
            IsExemptFromInduction = true,
            DegreeTypeId = degreeType.DegreeTypeId,
            ChangeReason = ChangeReasonOption.AnotherReason,
            ChangeReasonDetail = CreateChangeReasonDetail()
        };

        var journeyInstance = await CreateJourneyInstanceAsync(person.PersonId, state);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/routes/add/check-answers?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var updatedPerson = await WithDbContextAsync(dbContext => dbContext.Persons.SingleAsync(p => p.PersonId == person.PersonId));
        Assert.True(updatedPerson.HasEyps);

        Events.AssertProcessesCreated(p =>
        {
            Assert.Equal(ProcessType.RouteToProfessionalStatusCreating, p.ProcessContext.ProcessType);
            p.AssertProcessHasEvent<PersonProfessionalStatusAttributesUpdatedEvent>(attributesEvent =>
            {
                Assert.True(attributesEvent.PersonAttributes.HasEyps);
                Assert.False(attributesEvent.OldPersonAttributes.HasEyps);
            });
        });
    }

    [Fact]
    public async Task Post_Confirm_WithHeldPqtsRouteTypeUpdatesPersonPqtsDateAndHasChangesInEvent()
    {
        // Arrange
        var person = await TestData.CreatePersonAsync();

        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).First(r => r.Name == "European Recognition - PQTS");
        var country = (await ReferenceDataCache.GetTrainingCountriesAsync()).SingleRandom();

        var state = new AddRouteState
        {
            RouteToProfessionalStatusId = route.RouteToProfessionalStatusTypeId,
            Status = RouteToProfessionalStatusStatus.Holds,
            TrainingStartDate = TimeProvider.Today.AddYears(-1),
            TrainingEndDate = TimeProvider.Today.AddDays(-1),
            HoldsFrom = TimeProvider.Today,
            TrainingCountryId = country.CountryId,
            ChangeReason = ChangeReasonOption.AnotherReason,
            ChangeReasonDetail = CreateChangeReasonDetail()
        };

        var journeyInstance = await CreateJourneyInstanceAsync(person.PersonId, state);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/routes/add/check-answers?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var updatedPerson = await WithDbContextAsync(dbContext => dbContext.Persons.SingleAsync(p => p.PersonId == person.PersonId));
        Assert.Equal(state.HoldsFrom, updatedPerson.PqtsDate);

        Events.AssertProcessesCreated(p =>
        {
            Assert.Equal(ProcessType.RouteToProfessionalStatusCreating, p.ProcessContext.ProcessType);
            p.AssertProcessHasEvent<PersonProfessionalStatusAttributesUpdatedEvent>(attributesEvent =>
            {
                Assert.Equal(state.HoldsFrom, attributesEvent.PersonAttributes.PqtsDate);
                Assert.Null(attributesEvent.OldPersonAttributes.PqtsDate);
            });
        });
    }

    [Theory]
    [HttpMethods(TestHttpMethods.GetAndPost)]
    public async Task PersonIsDeactivated_ReturnsBadRequest(HttpMethod httpMethod)
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).Single(r => r.Name == "Northern Irish Recognition");
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
                Status = RouteToProfessionalStatusStatus.Deferred,
                TrainingCountryId = CountryCode,
                ChangeReason = ChangeReasonOption.AnotherReason,
                ChangeReasonDetail = CreateChangeReasonDetail()
            });

        var request = new HttpRequestMessage(httpMethod, $"/routes/add/check-answers?personId={person.PersonId}&{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }
}
