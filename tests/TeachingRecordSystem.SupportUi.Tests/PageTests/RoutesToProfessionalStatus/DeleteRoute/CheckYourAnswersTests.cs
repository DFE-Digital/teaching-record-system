using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.ChangeReasons;
using TeachingRecordSystem.SupportUi.Pages.RoutesToProfessionalStatus.DeleteRoute;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.RoutesToProfessionalStatus.DeleteRoute;

public class CheckYourAnswersTests(HostFixture hostFixture) : DeleteRouteTestBase(hostFixture)
{
    [Fact]
    public async Task Post_Cancel_DeletesJourneyAndRedirectsToExpectedPage()
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).Where(r => r.Name == "Northern Irish Recognition").Single();
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(RouteToProfessionalStatusStatus.Deferred)));
        var qualificationId = person.Qualifications!.OfType<RouteToProfessionalStatus>().First().QualificationId;
        var deleteRouteState = new DeleteRouteState
        {
            ChangeReason = ChangeReasonOption.RemovedQtlsStatus,
            ChangeReasonDetail = new ChangeReasonStateBuilder()
                .WithValidChangeReasonDetail()
                .Build()
        };

        var journeyInstance = await CreateJourneyInstanceAsync(qualificationId, deleteRouteState);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/routes/{qualificationId}/delete/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}")
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
            Assert.NotNull(await dbContext.RouteToProfessionalStatuses.FirstOrDefaultAsync(p => p.QualificationId == qualificationId)));
    }

    [Fact]
    public async Task Get_BackLinkReturnsToReason()
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).Where(r => r.Name == "Northern Irish Recognition").Single();
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(RouteToProfessionalStatusStatus.Deferred)));
        var qualificationId = person.Qualifications!.OfType<RouteToProfessionalStatus>().First().QualificationId;
        var deleteRouteState = new DeleteRouteState
        {
            ChangeReason = ChangeReasonOption.RemovedQtlsStatus,
            ChangeReasonDetail = new ChangeReasonStateBuilder()
                .WithValidChangeReasonDetail()
                .Build()
        };

        var journeyInstance = await CreateJourneyInstanceAsync(qualificationId, deleteRouteState);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/routes/{qualificationId}/delete/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(
            $"/routes/{qualificationId}/delete/reason?{journeyInstance.GetUniqueIdQueryParameter()}",
            doc.GetElementByTestId("back-link")!.GetAttribute("href"));
    }

    [Fact]
    public async Task Post_RedirectsToExpectedPage()
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).Where(r => r.Name == "Northern Irish Recognition").Single();
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(RouteToProfessionalStatusStatus.Deferred)));
        var qualificationId = person.Qualifications!.OfType<RouteToProfessionalStatus>().First().QualificationId;
        var deleteRouteState = new DeleteRouteState
        {
            ChangeReason = ChangeReasonOption.RemovedQtlsStatus,
            ChangeReasonDetail = new ChangeReasonStateBuilder()
                .WithValidChangeReasonDetail()
                .Build()
        };

        var journeyInstance = await CreateJourneyInstanceAsync(qualificationId, deleteRouteState);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/routes/{qualificationId}/delete/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        var location = response.Headers.Location?.OriginalString;
        Assert.Equal($"/persons/{person.PersonId}/qualifications", location);
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
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(status)
                .WithTrainingStartDate(startDate)
                .WithTrainingEndDate(endDate)
                .WithHoldsFrom(holdsFrom)
                .WithTrainingProviderId(trainingProvider.TrainingProviderId)
                .WithTrainingCountryId(country.CountryId)
                .WithTrainingSubjectIds(subjects.Select(s => s.TrainingSubjectId).ToArray())
                .WithTrainingAgeSpecialismType(TrainingAgeSpecialismType.FoundationStage)
                .WithDegreeTypeId(degreeType.DegreeTypeId)
                .WithInductionExemption(true)));

        var qualificationId = person.Qualifications!.OfType<RouteToProfessionalStatus>().First().QualificationId;
        var deleteRouteState = new DeleteRouteState
        {
            ChangeReason = ChangeReasonOption.RemovedQtlsStatus,
            ChangeReasonDetail = new ChangeReasonStateBuilder()
                .WithValidChangeReasonDetail()
                .Build()
        };

        var journeyInstance = await CreateJourneyInstanceAsync(qualificationId, deleteRouteState);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/routes/{qualificationId}/delete/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

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
    public async Task Get_ShowsChangeReasonAnswers_AsExpected()
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).Where(r => r.Name == "Northern Irish Recognition").Single();
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(RouteToProfessionalStatusStatus.Deferred)));
        var qualificationId = person.Qualifications!.OfType<RouteToProfessionalStatus>().First().QualificationId;
        var deleteRouteState = new DeleteRouteState
        {
            ChangeReason = ChangeReasonOption.RemovedQtlsStatus,
            ChangeReasonDetail = new ChangeReasonStateBuilder()
                .WithValidChangeReasonDetail()
                .Build()
        };

        var journeyInstance = await CreateJourneyInstanceAsync(qualificationId, deleteRouteState);

        var request = new HttpRequestMessage(HttpMethod.Get, $"/routes/{qualificationId}/delete/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        doc.AssertSummaryListRowValueContentMatches("Reason", deleteRouteState.ChangeReason!.GetDisplayName()!);
        doc.AssertSummaryListRowValueContentMatches("Reason details", deleteRouteState.ChangeReasonDetail!.ChangeReasonDetail!);
        doc.AssertSummaryListRowValueContentMatches("Additional information", deleteRouteState.ChangeReasonDetail!.AdditionalInformation!);
        doc.AssertSummaryListRowValueContentMatches("Evidence", "Not provided");
    }

    [Fact]
    public async Task Post_Confirm_DeletesRecordCreatesEventDeletesJourneyAndRedirectsWithFlashMessage()
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).SingleRandom();

        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(RouteToProfessionalStatusStatus.InTraining)));
        EventObserver.Clear();

        var qualificationId = person.Qualifications!.OfType<RouteToProfessionalStatus>().First().QualificationId;
        var deleteRouteState = new DeleteRouteState
        {
            ChangeReason = ChangeReasonOption.RemovedQtlsStatus,
            ChangeReasonDetail = new ChangeReasonStateBuilder()
                .WithValidChangeReasonDetail()
                .Build()
        };

        var journeyInstance = await CreateJourneyInstanceAsync(qualificationId, deleteRouteState);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/routes/{qualificationId}/delete/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var redirectResponse = await response.FollowRedirectAsync(HttpClient);
        var redirectDoc = await redirectResponse.GetDocumentAsync();
        AssertEx.HtmlDocumentHasFlashNotificationBanner(redirectDoc, "Route to professional status deleted");

        await WithDbContextAsync(async dbContext => Assert.Null(await dbContext.RouteToProfessionalStatuses.FirstOrDefaultAsync(p => p.QualificationId == qualificationId)));

        Events.AssertProcessesCreated(p =>
        {
            Assert.Equal(ProcessType.RouteToProfessionalStatusDeleting, p.ProcessContext.ProcessType);
            var changeReason = p.ProcessContext.Process.ChangeReason as ChangeReasonWithDetailsAndEvidence;

            p.AssertProcessHasEvents<RouteToProfessionalStatusDeletedEvent>(deletedEvent =>
            {
                Assert.Equal(person.PersonId, deletedEvent.PersonId);
                Assert.Equal(deleteRouteState.ChangeReason!.GetDisplayName(), changeReason?.Reason);
                Assert.Equal(deleteRouteState.ChangeReasonDetail.ChangeReasonDetail, changeReason?.Details);
                Assert.Null(changeReason?.EvidenceFile);
            });

            // Nothing about the person moved, so the route event is the only one on the process.
            p.AssertProcessHasEvents<RouteToProfessionalStatusDeletedEvent>();
        });

        Assert.Null(GetJourneyInstanceState(journeyInstance));
    }

    [Fact]
    public async Task Post_Confirm_WithHoldsQtsRouteTypeUpdatesPersonQtsDateAndHasChangesInEvent()
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.ProfessionalStatusType == ProfessionalStatusType.QualifiedTeacherStatus)
            .SingleRandom();
        var status = RouteToProfessionalStatusStatus.Holds;
        var qtsDate = TimeProvider.Today.AddYears(-1);
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(status)
                .WithHoldsFrom(qtsDate)));
        EventObserver.Clear();

        var qualificationId = person.Qualifications!.OfType<RouteToProfessionalStatus>().First().QualificationId;
        var deleteRouteState = new DeleteRouteState
        {
            ChangeReason = ChangeReasonOption.RemovedQtlsStatus,
            ChangeReasonDetail = new ChangeReasonStateBuilder()
                .WithValidChangeReasonDetail()
                .Build()
        };

        var journeyInstance = await CreateJourneyInstanceAsync(qualificationId, deleteRouteState);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/routes/{qualificationId}/delete/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var updatedPerson = await WithDbContextAsync(dbContext => dbContext.Persons.SingleAsync(p => p.PersonId == person.PersonId));
        Assert.Null(updatedPerson.QtsDate);

        var raisedByUserId = GetCurrentUserId();

        Events.AssertProcessesCreated(p =>
        {
            Assert.Equal(ProcessType.RouteToProfessionalStatusDeleting, p.ProcessContext.ProcessType);
            p.AssertProcessHasEvent<RouteToProfessionalStatusDeletedEvent>(deletedEvent =>
            {
                Assert.Equal(raisedByUserId, p.ProcessContext.UserId);
                Assert.Equal(person.PersonId, deletedEvent.PersonId);
            });

            p.AssertProcessHasEvent<PersonProfessionalStatusAttributesUpdatedEvent>(attributesEvent =>
            {
                Assert.Equal(qtsDate, attributesEvent.OldPersonAttributes.QtsDate);
                Assert.Null(attributesEvent.PersonAttributes.QtsDate);
                Assert.True(attributesEvent.Changes.HasFlag(PersonProfessionalStatusAttributesUpdatedEventChanges.QtsDate));
            });
        });

        Assert.Null(GetJourneyInstanceState(journeyInstance));
    }

    [Fact]
    public async Task Post_Confirm_WithHoldsQtsRouteType_UpdatesPersonQtsDateWithOlderRouteDateAndHasChangesInEvent()
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync())
            .Where(r => r.ProfessionalStatusType == ProfessionalStatusType.QualifiedTeacherStatus)
            .SingleRandom();
        var holdsFromEarliest = TimeProvider.Today.AddYears(-1);
        var holdsFromLatest = holdsFromEarliest.AddMonths(1);
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(RouteToProfessionalStatusStatus.Holds)
                .WithHoldsFrom(holdsFromEarliest))
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(RouteToProfessionalStatusStatus.Holds)
                .WithHoldsFrom(holdsFromLatest)));
        EventObserver.Clear();

        var qualificationIdEarliestDate = person.Qualifications!.OfType<RouteToProfessionalStatus>().Single(p => p.HoldsFrom == holdsFromEarliest).QualificationId;
        var deleteRouteState = new DeleteRouteState
        {
            ChangeReason = ChangeReasonOption.RemovedQtlsStatus,
            ChangeReasonDetail = new ChangeReasonStateBuilder()
                .WithValidChangeReasonDetail()
                .Build()
        };

        var journeyInstance = await CreateJourneyInstanceAsync(qualificationIdEarliestDate, deleteRouteState);

        var request = new HttpRequestMessage(HttpMethod.Post, $"/routes/{qualificationIdEarliestDate}/delete/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var updatedPerson = await WithDbContextAsync(dbContext => dbContext.Persons.SingleAsync(p => p.PersonId == person.PersonId));
        Assert.Equal(holdsFromLatest, updatedPerson.QtsDate);

        Events.AssertProcessesCreated(p =>
        {
            Assert.Equal(ProcessType.RouteToProfessionalStatusDeleting, p.ProcessContext.ProcessType);
            p.AssertProcessHasEvent<RouteToProfessionalStatusDeletedEvent>(deletedEvent =>
            {
                Assert.Equal(TimeProvider.UtcNow, p.ProcessContext.Now);
                Assert.Equal(person.PersonId, deletedEvent.PersonId);
            });

            p.AssertProcessHasEvent<PersonProfessionalStatusAttributesUpdatedEvent>(attributesEvent =>
            {
                Assert.Equal(holdsFromEarliest, attributesEvent.OldPersonAttributes.QtsDate);
                Assert.Equal(holdsFromLatest, attributesEvent.PersonAttributes.QtsDate);
                Assert.Equal(PersonProfessionalStatusAttributesUpdatedEventChanges.QtsDate, attributesEvent.Changes);
            });
        });

        Assert.Null(GetJourneyInstanceState(journeyInstance));
    }

    [Theory]
    [HttpMethods(TestHttpMethods.GetAndPost)]
    public async Task PersonIsDeactivated_ReturnsBadRequest(HttpMethod httpMethod)
    {
        // Arrange
        var route = (await ReferenceDataCache.GetRouteToProfessionalStatusTypesAsync()).Where(r => r.Name == "Northern Irish Recognition").Single();
        var person = await TestData.CreatePersonAsync(p => p
            .WithRouteToProfessionalStatus(r => r
                .WithRouteType(route.RouteToProfessionalStatusTypeId)
                .WithStatus(RouteToProfessionalStatusStatus.Deferred)));
        await WithDbContextAsync(async dbContext =>
        {
            dbContext.Attach(person);
            person.Status = PersonStatus.Deactivated;
            await dbContext.SaveChangesAsync();
        });
        var qualificationId = person.Qualifications!.OfType<RouteToProfessionalStatus>().First().QualificationId;
        var deleteRouteState = new DeleteRouteState
        {
            ChangeReason = ChangeReasonOption.RemovedQtlsStatus,
            ChangeReasonDetail = new ChangeReasonStateBuilder()
                .WithValidChangeReasonDetail()
                .Build()
        };

        var journeyInstance = await CreateJourneyInstanceAsync(qualificationId, deleteRouteState);

        var request = new HttpRequestMessage(httpMethod, $"/routes/{qualificationId}/delete/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, (int)response.StatusCode);
    }
}
