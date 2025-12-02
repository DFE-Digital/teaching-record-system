using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.Legacy;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.GetAnIdentity.Api.Models;
using TeachingRecordSystem.SupportUi.Pages.SupportTasks.ApiTrnRequests.Resolve;
using TeachingRecordSystem.SupportUi.Services;
using static TeachingRecordSystem.SupportUi.Pages.SupportTasks.ApiTrnRequests.Resolve.ResolveApiTrnRequestState;
using PersonCreatedEvent = TeachingRecordSystem.Core.Events.PersonCreatedEvent;
using PersonDetailsUpdatedEvent = TeachingRecordSystem.Core.Events.PersonDetailsUpdatedEvent;
using SupportTaskUpdatedEvent = TeachingRecordSystem.Core.Events.SupportTaskUpdatedEvent;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.ApiTrnRequests.Resolve;

public class CheckAnswersTests : ResolveApiTrnRequestTestBase
{
    public CheckAnswersTests(HostFixture hostFixture) : base(hostFixture)
    {
        GetAnIdentityApiClientMock
                 .Setup(mock => mock.CreateTrnTokenAsync(It.IsAny<CreateTrnTokenRequest>()))
                 .ReturnsAsync((CreateTrnTokenRequest req) => new CreateTrnTokenResponse
                 {
                     Email = req.Email,
                     ExpiresUtc = Clock.UtcNow.AddDays(1),
                     Trn = req.Trn,
                     TrnToken = Guid.NewGuid().ToString()
                 });
    }

    [Fact]
    public async Task Get_NoPersonIdSelected_RedirectsToMatches()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var (supportTask, _, matchedPersonIds) = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveApiTrnRequestState
            {
                MatchedPersonIds = matchedPersonIds,
                PersonId = null
            });

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/resolve/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_NoAttributesSourcesSet_RedirectsToMerge()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var (supportTask, _, matchedPersonIds) = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveApiTrnRequestState
            {
                MatchedPersonIds = matchedPersonIds,
                PersonId = matchedPersonIds[0],
                PersonAttributeSourcesSet = false
            });

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/resolve/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/resolve/merge?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);
    }

    [Theory]
    [MemberData(nameof(GetPersonAttributeInfosData))]
    public async Task Get_AttributeSourceIsTrnRequest_RendersChosenAttributeValues(PersonAttributeInfo sourcedFromRequestDataAttribute)
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var (supportTask, matchedPerson) = await CreateSupportTaskWithSingleDifferenceToMatch(
            applicationUser.UserId,
            sourcedFromRequestDataAttribute.Attribute);
        var requestData = supportTask.TrnRequestMetadata!;

        var state = new ResolveApiTrnRequestState
        {
            MatchedPersonIds = [matchedPerson.PersonId],
            PersonId = matchedPerson.PersonId,
            PersonAttributeSourcesSet = true
        };
        SetPersonAttributeSourceToTrnRequest(state, sourcedFromRequestDataAttribute.Attribute);

        var journeyInstance = await CreateJourneyInstance(supportTask.SupportTaskReference, state);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/resolve/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);

        var allSummaryListRowValues = doc.GetElementsByClassName("govuk-summary-list__row")
            .ToDictionary(
                row => row.GetElementsByClassName("govuk-summary-list__key").Single().TrimmedText(),
                row => row.GetElementsByClassName("govuk-summary-list__value").Single().TrimmedText());

        foreach (var kvp in allSummaryListRowValues)
        {
            var attributeInfo = GetPersonAttributeInfos().SingleOrDefault(i => i.SummaryListRowKey == kvp.Key);
            if (attributeInfo is null)
            {
                continue;
            }

            static object? FormatValue(object? value) => value switch
            {
                null => UiDefaults.EmptyDisplayContent,
                DateOnly dateOnly => dateOnly.ToString(UiDefaults.DateOnlyDisplayFormat),
                Gender gender => gender.GetDisplayName(),
                _ => value
            };

            if (sourcedFromRequestDataAttribute.SummaryListRowKey == kvp.Key)
            {
                var requestDataValue = FormatValue(sourcedFromRequestDataAttribute.GetValueFromRequestData(requestData));
                Assert.Equal(requestDataValue, kvp.Value);
            }
            else
            {
                var existingRecordValue = FormatValue(
                    GetPersonAttributeInfos().Single(i => i.SummaryListRowKey == kvp.Key, kvp.Key).GetValueFromPerson(matchedPerson));
                Assert.Equal(existingRecordValue, kvp.Value);
            }
        }
    }

    [Fact]
    public async Task Get_CreatingNewRecord_DoesNotShowTrnRow()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var (supportTask, _, matchedPersonIds) = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveApiTrnRequestState
            {
                MatchedPersonIds = matchedPersonIds,
                PersonId = CreateNewRecordPersonIdSentinel
            });

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/resolve/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Null(doc.GetSummaryListValueElementByKey("TRN"));
    }

    [Fact]
    public async Task Get_CreatingNewRecord_DoesNotShowCommentsRow()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var (supportTask, _, matchedPersonIds) = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveApiTrnRequestState
            {
                MatchedPersonIds = matchedPersonIds,
                PersonId = CreateNewRecordPersonIdSentinel
            });

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/resolve/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Null(doc.GetSummaryListValueElementByKey("Comments"));
    }

    [Fact]
    public async Task Get_UpdatingExistingRecord_DoesShowCommentsRow()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var (supportTask, _, matchedPersonIds) = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveApiTrnRequestState
            {
                MatchedPersonIds = matchedPersonIds,
                PersonId = matchedPersonIds[0],
                PersonAttributeSourcesSet = true
            });

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/resolve/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.NotNull(doc.GetSummaryListValueElementByKey("Comments"));
    }

    [Fact]
    public async Task Get_UpdatingExistingRecord_DoesShowTrnRow()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var (supportTask, _, matchedPersonIds) = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveApiTrnRequestState
            {
                MatchedPersonIds = matchedPersonIds,
                PersonId = matchedPersonIds[0],
                PersonAttributeSourcesSet = true
            });

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/resolve/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.NotNull(doc.GetSummaryListValueElementByKey("TRN"));
    }

    [Fact]
    public async Task Get_ShowsComments()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var (supportTask, _, matchedPersonIds) = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId);

        var comments = Faker.Lorem.Paragraph();

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveApiTrnRequestState
            {
                MatchedPersonIds = matchedPersonIds,
                PersonId = matchedPersonIds[0],
                PersonAttributeSourcesSet = true,
                Comments = comments
            });

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/resolve/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(comments, doc.GetSummaryListValueElementByKey("Comments")?.TrimmedText());
    }

    [Fact]
    public async Task Get_CreatingNewRecord_HasBackChangeLinksToMatchesPage()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var (supportTask, _, matchedPersonIds) = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveApiTrnRequestState
            {
                MatchedPersonIds = matchedPersonIds,
                PersonId = CreateNewRecordPersonIdSentinel
            });

        var expectedBackLink = $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}";
        var expectedChangeLink = $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/resolve/matches?fromCheckAnswers=True&{journeyInstance.GetUniqueIdQueryParameter()}";

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/resolve/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(expectedBackLink, doc.GetElementsByClassName("govuk-back-link").Single().GetAttribute("href"));
        Assert.Null(doc.GetElementByTestId("dob-change-link"));
        Assert.Null(doc.GetElementByTestId("ni-change-link"));
        Assert.Null(doc.GetElementByTestId("email-change-link"));
        Assert.Null(doc.GetElementByTestId("gender-change-link"));
    }

    [Fact]
    public async Task Get_UpdatingExistingRecord_HasBackAndChangeLinksToMergePage()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var (supportTask, _, matchedPersonIds) = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveApiTrnRequestState
            {
                MatchedPersonIds = matchedPersonIds,
                PersonId = matchedPersonIds[0],
                PersonAttributeSourcesSet = true,
                DateOfBirthSource = PersonAttributeSource.TrnRequest,
                NationalInsuranceNumberSource = PersonAttributeSource.TrnRequest,
                EmailAddressSource = PersonAttributeSource.ExistingRecord,
                GenderSource = PersonAttributeSource.TrnRequest
            });

        var expectedBackLink = $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/resolve/merge?{journeyInstance.GetUniqueIdQueryParameter()}";
        var expectedChangeLink = $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/resolve/merge?fromCheckAnswers=True&{journeyInstance.GetUniqueIdQueryParameter()}";

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/resolve/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(expectedBackLink, doc.GetElementsByClassName("govuk-back-link").Single().GetAttribute("href"));
        Assert.Equal(expectedChangeLink, doc.GetElementByTestId("email-change-link")?.GetAttribute("href"));
        Assert.Equal(expectedChangeLink, doc.GetElementByTestId("ni-change-link")?.GetAttribute("href"));
        Assert.Equal(expectedChangeLink, doc.GetElementByTestId("dob-change-link")?.GetAttribute("href"));
        Assert.Equal(expectedChangeLink, doc.GetElementByTestId("gender-change-link")?.GetAttribute("href"));
    }

    [Fact]
    public async Task Post_NoPersonIdSelected_RedirectsToMatches()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var (supportTask, _, matchedPersonIds) = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveApiTrnRequestState
            {
                MatchedPersonIds = matchedPersonIds,
                PersonId = null
            });

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/resolve/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_NoAttributesSourcesSet_RedirectsToMerge()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var (supportTask, _, matchedPersonIds) = await TestData.CreateApiTrnRequestSupportTaskAsync(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveApiTrnRequestState
            {
                MatchedPersonIds = matchedPersonIds,
                PersonId = matchedPersonIds[0],
                PersonAttributeSourcesSet = false
            });

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/resolve/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/resolve/merge?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_CreatingNewRecord_CreatesNewRecordUpdatesSupportTaskStatusAndRedirects()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var (supportTask, matchedPerson) = await CreateSupportTaskWithSingleDifferenceToMatch(
            applicationUser.UserId,
            PersonMatchedAttribute.MiddleName);
        var requestData = supportTask.TrnRequestMetadata!;

        Clock.Advance();

        var comments = Faker.Lorem.Paragraph();
        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveApiTrnRequestState
            {
                MatchedPersonIds = [matchedPerson.PersonId],
                PersonId = CreateNewRecordPersonIdSentinel,
                Comments = comments
            });

        EventObserver.Clear();

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/resolve/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith("/support-tasks/api-trn-requests", response.Headers.Location?.OriginalString);

        var person = await WithDbContextAsync(dbContext => dbContext.Persons.SingleOrDefaultAsync(p => p.SourceTrnRequestId == requestData.RequestId));
        Assert.NotNull(person);
        Assert.Equal(requestData.FirstName, person.FirstName);
        Assert.Equal(requestData.MiddleName, person.MiddleName);
        Assert.Equal(requestData.LastName, person.LastName);
        Assert.Equal(requestData.DateOfBirth, person.DateOfBirth);
        Assert.Equal(requestData.EmailAddress, person.EmailAddress);
        Assert.Equal(requestData.NationalInsuranceNumber, person.NationalInsuranceNumber);
        Assert.Equal(requestData.Gender, person.Gender);
        Assert.NotNull(person.Trn);

        var updatedSupportTask = await WithDbContextAsync(dbContext => dbContext
            .SupportTasks.Include(st => st.TrnRequestMetadata).SingleAsync(t => t.SupportTaskReference == supportTask.SupportTaskReference));
        Assert.Equal(SupportTaskStatus.Closed, updatedSupportTask.Status);
        Assert.Equal(Clock.UtcNow, updatedSupportTask.UpdatedOn);
        Assert.Equal(person.PersonId, updatedSupportTask.TrnRequestMetadata!.ResolvedPersonId);
        Assert.NotNull(updatedSupportTask.TrnRequestMetadata.TrnToken);
        var supportTaskData = updatedSupportTask.GetData<ApiTrnRequestData>();
        AssertPersonAttributesMatchPerson(supportTaskData.ResolvedAttributes, person);
        Assert.Null(supportTaskData.SelectedPersonAttributes);

        EventObserver.AssertEventsSaved(@event =>
        {
            var apiTrnRequestSupportTaskUpdatedEvent = Assert.IsType<ApiTrnRequestSupportTaskUpdatedEvent>(@event);
            AssertEventIsExpected(apiTrnRequestSupportTaskUpdatedEvent, expectOldPersonAttributes: false, expectedPersonId: person.PersonId, comments);
        });

        Events.AssertProcessesCreated(p =>
        {
            Assert.Equal(ProcessType.ApiTrnRequestResolving, p.ProcessContext.ProcessType);
            p.AssertProcessHasEvents<PersonCreatedEvent, TrnRequestUpdatedEvent, SupportTaskUpdatedEvent>();
        });

        var nextPage = await response.FollowRedirectAsync(HttpClient);
        var nextPageDoc = await nextPage.GetDocumentAsync();
        AssertEx.HtmlDocumentHasFlashSuccess(
            nextPageDoc,
            $"Record created for {requestData.FirstName} {requestData.MiddleName} {requestData.LastName}");
    }

    [Fact]
    public async Task Post_UpdatingExistingRecord_UpdatesRecordUpdatesSupportTaskAndRedirects()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var (supportTask, matchedPerson) = await CreateSupportTaskWithSingleDifferenceToMatch(
            applicationUser.UserId,
            PersonMatchedAttribute.MiddleName);
        var requestData = supportTask.TrnRequestMetadata!;

        Clock.Advance();

        var comments = Faker.Lorem.Paragraph();
        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveApiTrnRequestState
            {
                MatchedPersonIds = [matchedPerson.PersonId],
                PersonId = matchedPerson.PersonId,
                PersonAttributeSourcesSet = true,
                MiddleNameSource = PersonAttributeSource.TrnRequest,
                Comments = comments
            });

        EventObserver.Clear();

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/resolve/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith("/support-tasks/api-trn-requests", response.Headers.Location?.OriginalString);

        var person = await WithDbContextAsync(dbContext => dbContext.Persons.SingleAsync(p => p.PersonId == matchedPerson.PersonId));
        Assert.Equal(requestData.MiddleName, person.MiddleName);

        var updatedSupportTask = await WithDbContextAsync(dbContext => dbContext
            .SupportTasks.Include(st => st.TrnRequestMetadata).SingleAsync(t => t.SupportTaskReference == supportTask.SupportTaskReference));
        Assert.Equal(SupportTaskStatus.Closed, updatedSupportTask.Status);
        Assert.Equal(Clock.UtcNow, updatedSupportTask.UpdatedOn);
        Assert.Equal(person.PersonId, updatedSupportTask.TrnRequestMetadata!.ResolvedPersonId);
        Assert.NotNull(updatedSupportTask.TrnRequestMetadata.TrnToken);
        var supportTaskData = updatedSupportTask.GetData<ApiTrnRequestData>();
        AssertPersonAttributesMatchPerson(supportTaskData.ResolvedAttributes, person);
        AssertPersonAttributesMatchPerson(supportTaskData.SelectedPersonAttributes, matchedPerson.Person);

        EventObserver.AssertEventsSaved(@event =>
        {
            var apiTrnRequestSupportTaskUpdatedEvent = Assert.IsType<ApiTrnRequestSupportTaskUpdatedEvent>(@event);
            AssertEventIsExpected(apiTrnRequestSupportTaskUpdatedEvent, expectOldPersonAttributes: true, expectedPersonId: matchedPerson.PersonId, comments);
        });

        Events.AssertProcessesCreated(p =>
        {
            Assert.Equal(ProcessType.ApiTrnRequestResolving, p.ProcessContext.ProcessType);
            p.AssertProcessHasEvents<PersonDetailsUpdatedEvent, TrnRequestUpdatedEvent, SupportTaskUpdatedEvent>();
        });

        var nextPage = await response.FollowRedirectAsync(HttpClient);
        var nextPageDoc = await nextPage.GetDocumentAsync();
        AssertEx.HtmlDocumentHasFlashSuccess(
            nextPageDoc,
            $"Records merged for {requestData.FirstName} {requestData.MiddleName} {requestData.LastName}");
    }

    [Fact]
    public async Task Post_UpdatingExistingRecordAndMatchedRecordDoesNotRequireFurtherChecks_SetsTrnRequestToCompleted()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        TrnRequestOptions.FlagFurtherChecksRequiredFromUserIds = [applicationUser.UserId];

        var (supportTask, matchedPerson) = await CreateSupportTaskWithSingleDifferenceToMatch(
            applicationUser.UserId,
            PersonMatchedAttribute.MiddleName,
            matchedPersonHasFlags: false);
        var requestData = supportTask.TrnRequestMetadata!;

        Clock.Advance();

        var comments = Faker.Lorem.Paragraph();
        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveApiTrnRequestState
            {
                MatchedPersonIds = [matchedPerson.PersonId],
                PersonId = matchedPerson.PersonId,
                PersonAttributeSourcesSet = true,
                MiddleNameSource = PersonAttributeSource.TrnRequest,
                Comments = comments
            });

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/resolve/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var updatedTrnRequestMetadata = await WithDbContextAsync(dbContext =>
            dbContext.TrnRequestMetadata.SingleAsync(t => t.RequestId == requestData.RequestId));
        Assert.Equal(TrnRequestStatus.Completed, updatedTrnRequestMetadata.Status);

        var furtherChecksRequiredTasks = await WithDbContextAsync(dbContext =>
            dbContext.SupportTasks.Where(t =>
                t.SupportTaskType == SupportTaskType.TrnRequestManualChecksNeeded && t.TrnRequestId == requestData.RequestId).ToArrayAsync());
        Assert.Empty(furtherChecksRequiredTasks);
    }

    [Fact]
    public async Task Post_UpdatingExistingRecordAndMatchedRecordDoesRequireFurtherChecks_CreatesSupportTaskAndKeepsTrnRequestPending()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();
        TrnRequestOptions.FlagFurtherChecksRequiredFromUserIds = [applicationUser.UserId];

        var (supportTask, matchedPerson) = await CreateSupportTaskWithSingleDifferenceToMatch(
            applicationUser.UserId,
            PersonMatchedAttribute.MiddleName,
            matchedPersonHasFlags: true);
        var requestData = supportTask.TrnRequestMetadata!;

        Clock.Advance();

        var comments = Faker.Lorem.Paragraph();
        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveApiTrnRequestState
            {
                MatchedPersonIds = [matchedPerson.PersonId],
                PersonId = matchedPerson.PersonId,
                PersonAttributeSourcesSet = true,
                MiddleNameSource = PersonAttributeSource.TrnRequest,
                Comments = comments
            });

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/api-trn-requests/{supportTask.SupportTaskReference}/resolve/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var updatedTrnRequestMetadata = await WithDbContextAsync(dbContext =>
            dbContext.TrnRequestMetadata.SingleAsync(t => t.RequestId == requestData.RequestId));
        Assert.Equal(TrnRequestStatus.Pending, updatedTrnRequestMetadata.Status);

        var furtherChecksRequiredTasks = await WithDbContextAsync(dbContext =>
            dbContext.SupportTasks.Where(t =>
                t.SupportTaskType == SupportTaskType.TrnRequestManualChecksNeeded && t.TrnRequestId == requestData.RequestId).ToArrayAsync());
        Assert.NotEmpty(furtherChecksRequiredTasks);
    }

    private Task<JourneyInstance<ResolveApiTrnRequestState>> CreateJourneyInstance(
        string supportTaskReference,
        ResolveApiTrnRequestState state) =>
        CreateJourneyInstance(
            JourneyNames.ResolveApiTrnRequest,
            state,
            new KeyValuePair<string, object>("supportTaskReference", supportTaskReference));

    private static void SetPersonAttributeSourceToTrnRequest(ResolveApiTrnRequestState state, PersonMatchedAttribute attribute)
    {
        state.FirstNameSource = attribute is PersonMatchedAttribute.FirstName ? PersonAttributeSource.TrnRequest : PersonAttributeSource.ExistingRecord;
        state.MiddleNameSource = attribute is PersonMatchedAttribute.MiddleName ? PersonAttributeSource.TrnRequest : PersonAttributeSource.ExistingRecord;
        state.LastNameSource = attribute is PersonMatchedAttribute.LastName ? PersonAttributeSource.TrnRequest : PersonAttributeSource.ExistingRecord;
        state.DateOfBirthSource = attribute is PersonMatchedAttribute.DateOfBirth ? PersonAttributeSource.TrnRequest : PersonAttributeSource.ExistingRecord;
        state.EmailAddressSource = attribute is PersonMatchedAttribute.EmailAddress ? PersonAttributeSource.TrnRequest : PersonAttributeSource.ExistingRecord;
        state.NationalInsuranceNumberSource = attribute is PersonMatchedAttribute.NationalInsuranceNumber ? PersonAttributeSource.TrnRequest : PersonAttributeSource.ExistingRecord;
        state.GenderSource = attribute is PersonMatchedAttribute.Gender ? PersonAttributeSource.TrnRequest : PersonAttributeSource.ExistingRecord;
    }

    private void AssertPersonAttributesMatchPerson(
        ApiTrnRequestDataPersonAttributes? personAttributes,
        Person person)
    {
        Assert.NotNull(personAttributes);
        Assert.Equal(personAttributes.FirstName, person.FirstName);
        Assert.Equal(personAttributes.MiddleName, person.MiddleName);
        Assert.Equal(personAttributes.LastName, person.LastName);
        Assert.Equal(personAttributes.DateOfBirth, person.DateOfBirth);
        Assert.Equal(personAttributes.EmailAddress, person.EmailAddress);
        Assert.Equal(personAttributes.NationalInsuranceNumber, person.NationalInsuranceNumber);
        Assert.Equal(personAttributes.Gender, person.Gender);
    }

    private void AssertEventIsExpected(
        ApiTrnRequestSupportTaskUpdatedEvent @event,
        bool expectOldPersonAttributes,
        Guid expectedPersonId,
        string? comments)
    {
        Assert.Equal(expectedPersonId, @event.PersonId);
        Assert.Equal(Clock.UtcNow, @event.CreatedUtc);
        Assert.True(@event.Changes.HasFlag(ApiTrnRequestSupportTaskUpdatedEventChanges.Status));

        Assert.Equal(SupportTaskStatus.Open, @event.OldSupportTask.Status);
        Assert.Equal(SupportTaskStatus.Closed, @event.SupportTask.Status);

        if (expectOldPersonAttributes)
        {
            Assert.NotNull(@event.OldPersonAttributes);
        }
        else
        {
            Assert.Null(@event.OldPersonAttributes);
        }

        Assert.Equal(comments, @event.Comments);
    }

    public static TheoryData<PersonAttributeInfo> GetPersonAttributeInfosData => new(GetPersonAttributeInfos());

    public static PersonAttributeInfo[] GetPersonAttributeInfos() =>
    [
        new(
            PersonMatchedAttribute.FirstName,
            "FirstName",
            "First name",
            d => d.FirstName,
            p => p.FirstName
        ),
        new(
            PersonMatchedAttribute.MiddleName,
            "MiddleName",
            "Middle name",
            d => d.MiddleName,
            p => p.MiddleName
        ),
        new(
            PersonMatchedAttribute.LastName,
            "LastName",
            "Last name",
            d => d.LastName,
            p => p.LastName
        ),
        new(
            PersonMatchedAttribute.DateOfBirth,
            "DateOfBirth",
            "Date of birth",
            d => d.DateOfBirth,
            p => p.DateOfBirth,
            value => ((DateOnly?)value)?.ToString(UiDefaults.DateOnlyDisplayFormat)
        ),
        new(
            PersonMatchedAttribute.EmailAddress,
            "EmailAddress",
            "Email address",
            d => d.EmailAddress,
            p => p.EmailAddress
        ),
        new(
            PersonMatchedAttribute.NationalInsuranceNumber,
            "NationalInsuranceNumber",
            "National Insurance number",
            d => d.NationalInsuranceNumber,
            p => p.NationalInsuranceNumber
        ),
        new(
            PersonMatchedAttribute.Gender,
            "Gender",
            "Gender",
            d => d.Gender,
            p => p.Gender,
            value => ((Gender?)value)?.GetDisplayName()
        )
    ];

    public record PersonAttributeInfo(
        PersonMatchedAttribute Attribute,
        string FieldName,
        string SummaryListRowKey,
        Func<TrnRequestMetadata, object?> GetValueFromRequestData,
        Func<TestData.CreatePersonResult, object?> GetValueFromPerson,
        Func<object?, object?>? MapValueToSummaryListRowValue = null);
}
