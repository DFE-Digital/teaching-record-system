using AngleSharp.Html.Dom;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.Legacy;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.TrnRequests;
using TeachingRecordSystem.SupportUi.Pages.SupportTasks.TrnRequests.Resolve;
using static TeachingRecordSystem.SupportUi.Pages.SupportTasks.TrnRequests.Resolve.ResolveTrnRequestState;
using PersonCreatedEvent = TeachingRecordSystem.Core.Events.PersonCreatedEvent;
using PersonDetailsUpdatedEvent = TeachingRecordSystem.Core.Events.PersonDetailsUpdatedEvent;
using SupportTaskUpdatedEvent = TeachingRecordSystem.Core.Events.SupportTaskUpdatedEvent;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.TrnRequests.Resolve;

public class CheckAnswersTests(HostFixture hostFixture) : ResolveApiTrnRequestTestBase(hostFixture)
{
    [Fact]
    public async Task Get_NoPersonIdSelected_RedirectsToMatches()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var (supportTask, _, matchedPersonIds) = await TestData.CreateTrnRequestSupportTaskAsync(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveTrnRequestState
            {
                MatchedPersons = matchedPersonIds.Select(
                    p => new MatchPersonsResultPerson(
                        p,
                        [
                            PersonMatchedAttribute.FirstName,
                            PersonMatchedAttribute.MiddleName,
                            PersonMatchedAttribute.LastName,
                            PersonMatchedAttribute.DateOfBirth,
                            PersonMatchedAttribute.EmailAddress,
                            PersonMatchedAttribute.Gender
                        ]
                    )).ToArray(),
                PersonId = null
            });

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/trn-requests/{supportTask.SupportTaskReference}/resolve/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/trn-requests/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Get_NoAttributesSourcesSet_RedirectsToMerge()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var (supportTask, _, matchedPersonIds) = await TestData.CreateTrnRequestSupportTaskAsync(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveTrnRequestState
            {
                MatchedPersons = matchedPersonIds.Select(
                    p => new MatchPersonsResultPerson(
                        p,
                        [
                            PersonMatchedAttribute.FirstName,
                            PersonMatchedAttribute.MiddleName,
                            PersonMatchedAttribute.LastName,
                            PersonMatchedAttribute.DateOfBirth,
                            PersonMatchedAttribute.EmailAddress,
                            PersonMatchedAttribute.Gender
                        ]
                    )).ToArray(),
                PersonId = matchedPersonIds[0],
                PersonAttributeSourcesSet = false
            });

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/trn-requests/{supportTask.SupportTaskReference}/resolve/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/trn-requests/{supportTask.SupportTaskReference}/resolve/merge?{journeyInstance.GetUniqueIdQueryParameter()}",
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

        var state = new ResolveTrnRequestState
        {
            MatchedPersons = [new MatchPersonsResultPerson(
                matchedPerson.PersonId,
                    new[]
                    {
                        sourcedFromRequestDataAttribute.Attribute == PersonMatchedAttribute.FirstName ? PersonMatchedAttribute.FirstName : (PersonMatchedAttribute?)null,
                        sourcedFromRequestDataAttribute.Attribute == PersonMatchedAttribute.MiddleName ? PersonMatchedAttribute.MiddleName : null,
                        sourcedFromRequestDataAttribute.Attribute == PersonMatchedAttribute.LastName ? PersonMatchedAttribute.LastName : null,
                        sourcedFromRequestDataAttribute.Attribute == PersonMatchedAttribute.DateOfBirth ? PersonMatchedAttribute.DateOfBirth : null,
                        sourcedFromRequestDataAttribute.Attribute == PersonMatchedAttribute.EmailAddress ? PersonMatchedAttribute.EmailAddress : null,
                        sourcedFromRequestDataAttribute.Attribute == PersonMatchedAttribute.NationalInsuranceNumber ? PersonMatchedAttribute.NationalInsuranceNumber : null,
                        sourcedFromRequestDataAttribute.Attribute == PersonMatchedAttribute.Gender ? PersonMatchedAttribute.Gender : null
                    }
                    .Where(a => a is not null)
                    .Select(a => a!.Value)
                    .ToArray())],
            PersonId = matchedPerson.PersonId,
            PersonAttributeSourcesSet = true
        };
        SetPersonAttributeSourceToTrnRequest(state, sourcedFromRequestDataAttribute.Attribute);

        var journeyInstance = await CreateJourneyInstance(supportTask.SupportTaskReference, state);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/trn-requests/{supportTask.SupportTaskReference}/resolve/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

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
                null => WebConstants.EmptyFallbackContent,
                DateOnly dateOnly => dateOnly.ToString(WebConstants.DateDisplayFormat),
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

        var (supportTask, _, matchedPersonIds) = await TestData.CreateTrnRequestSupportTaskAsync(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveTrnRequestState
            {
                MatchedPersons = matchedPersonIds.Select(
                    p => new MatchPersonsResultPerson(
                        p,
                        [
                            PersonMatchedAttribute.FirstName,
                            PersonMatchedAttribute.MiddleName,
                            PersonMatchedAttribute.LastName,
                            PersonMatchedAttribute.DateOfBirth,
                            PersonMatchedAttribute.EmailAddress,
                            PersonMatchedAttribute.Gender
                        ]
                    )).ToArray(),
                PersonId = CreateNewRecordPersonIdSentinel
            });

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/trn-requests/{supportTask.SupportTaskReference}/resolve/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

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

        var (supportTask, _, matchedPersonIds) = await TestData.CreateTrnRequestSupportTaskAsync(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveTrnRequestState
            {
                MatchedPersons = matchedPersonIds.Select(
                    p => new MatchPersonsResultPerson(
                        p,
                        [
                            PersonMatchedAttribute.FirstName,
                            PersonMatchedAttribute.MiddleName,
                            PersonMatchedAttribute.LastName,
                            PersonMatchedAttribute.DateOfBirth,
                            PersonMatchedAttribute.EmailAddress,
                            PersonMatchedAttribute.Gender
                        ]
                    )).ToArray(),
                PersonId = CreateNewRecordPersonIdSentinel
            });

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/trn-requests/{supportTask.SupportTaskReference}/resolve/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

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

        var (supportTask, _, matchedPersonIds) = await TestData.CreateTrnRequestSupportTaskAsync(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveTrnRequestState
            {
                MatchedPersons = matchedPersonIds.Select(
                    p => new MatchPersonsResultPerson(
                        p,
                        [
                            PersonMatchedAttribute.FirstName,
                            PersonMatchedAttribute.MiddleName,
                            PersonMatchedAttribute.LastName,
                            PersonMatchedAttribute.DateOfBirth,
                            PersonMatchedAttribute.EmailAddress,
                            PersonMatchedAttribute.Gender
                        ]
                    )).ToArray(),
                PersonId = matchedPersonIds[0],
                PersonAttributeSourcesSet = true
            });

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/trn-requests/{supportTask.SupportTaskReference}/resolve/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

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

        var (supportTask, _, matchedPersonIds) = await TestData.CreateTrnRequestSupportTaskAsync(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveTrnRequestState
            {
                MatchedPersons = matchedPersonIds.Select(
                    p => new MatchPersonsResultPerson(
                        p,
                        [
                            PersonMatchedAttribute.FirstName,
                            PersonMatchedAttribute.MiddleName,
                            PersonMatchedAttribute.LastName,
                            PersonMatchedAttribute.DateOfBirth,
                            PersonMatchedAttribute.EmailAddress,
                            PersonMatchedAttribute.Gender
                        ]
                    )).ToArray(),
                PersonId = matchedPersonIds[0],
                PersonAttributeSourcesSet = true
            });

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/trn-requests/{supportTask.SupportTaskReference}/resolve/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

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

        var (supportTask, _, matchedPersonIds) = await TestData.CreateTrnRequestSupportTaskAsync(applicationUser.UserId);

        var comments = Faker.Lorem.Paragraph();

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveTrnRequestState
            {
                MatchedPersons = matchedPersonIds.Select(
                    p => new MatchPersonsResultPerson(
                        p,
                        [
                            PersonMatchedAttribute.FirstName,
                            PersonMatchedAttribute.MiddleName,
                            PersonMatchedAttribute.LastName,
                            PersonMatchedAttribute.DateOfBirth,
                            PersonMatchedAttribute.EmailAddress,
                            PersonMatchedAttribute.Gender
                        ]
                    )).ToArray(),
                PersonId = matchedPersonIds[0],
                PersonAttributeSourcesSet = true,
                Comments = comments
            });

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/trn-requests/{supportTask.SupportTaskReference}/resolve/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

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

        var (supportTask, _, matchedPersonIds) = await TestData.CreateTrnRequestSupportTaskAsync(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveTrnRequestState
            {
                MatchedPersons = matchedPersonIds.Select(
                    p => new MatchPersonsResultPerson(
                        p,
                        [
                            PersonMatchedAttribute.FirstName,
                            PersonMatchedAttribute.MiddleName,
                            PersonMatchedAttribute.LastName,
                            PersonMatchedAttribute.DateOfBirth,
                            PersonMatchedAttribute.EmailAddress,
                            PersonMatchedAttribute.Gender
                        ]
                    )).ToArray(),
                PersonId = CreateNewRecordPersonIdSentinel
            });

        var expectedBackLink = $"/support-tasks/trn-requests/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}";
        var expectedChangeLink = $"/support-tasks/trn-requests/{supportTask.SupportTaskReference}/resolve/matches?fromCheckAnswers=True&{journeyInstance.GetUniqueIdQueryParameter()}";

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/trn-requests/{supportTask.SupportTaskReference}/resolve/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

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

        var (supportTask, _, matchedPersonIds) = await TestData.CreateTrnRequestSupportTaskAsync(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveTrnRequestState
            {
                MatchedPersons = matchedPersonIds.Select(
                    p => new MatchPersonsResultPerson(
                        p,
                        [
                            PersonMatchedAttribute.FirstName,
                            PersonMatchedAttribute.MiddleName,
                            PersonMatchedAttribute.LastName,
                            PersonMatchedAttribute.DateOfBirth,
                            PersonMatchedAttribute.EmailAddress,
                            PersonMatchedAttribute.Gender
                        ]
                    )).ToArray(),
                PersonId = matchedPersonIds[0],
                PersonAttributeSourcesSet = true,
                DateOfBirthSource = PersonAttributeSource.TrnRequest,
                NationalInsuranceNumberSource = PersonAttributeSource.TrnRequest,
                EmailAddressSource = PersonAttributeSource.ExistingRecord,
                GenderSource = PersonAttributeSource.TrnRequest
            });

        var expectedBackLink = $"/support-tasks/trn-requests/{supportTask.SupportTaskReference}/resolve/merge?{journeyInstance.GetUniqueIdQueryParameter()}";
        var expectedChangeLink = $"/support-tasks/trn-requests/{supportTask.SupportTaskReference}/resolve/merge?fromCheckAnswers=True&{journeyInstance.GetUniqueIdQueryParameter()}";

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/trn-requests/{supportTask.SupportTaskReference}/resolve/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

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

        var (supportTask, _, matchedPersonIds) = await TestData.CreateTrnRequestSupportTaskAsync(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveTrnRequestState
            {
                MatchedPersons = matchedPersonIds.Select(
                    p => new MatchPersonsResultPerson(
                        p,
                        [
                            PersonMatchedAttribute.FirstName,
                            PersonMatchedAttribute.MiddleName,
                            PersonMatchedAttribute.LastName,
                            PersonMatchedAttribute.DateOfBirth,
                            PersonMatchedAttribute.EmailAddress,
                            PersonMatchedAttribute.Gender
                        ]
                    )).ToArray(),
                PersonId = null
            });

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/trn-requests/{supportTask.SupportTaskReference}/resolve/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/trn-requests/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Post_NoAttributesSourcesSet_RedirectsToMerge()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var (supportTask, _, matchedPersonIds) = await TestData.CreateTrnRequestSupportTaskAsync(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveTrnRequestState
            {
                MatchedPersons = matchedPersonIds.Select(
                    p => new MatchPersonsResultPerson(
                        p,
                        [
                            PersonMatchedAttribute.FirstName,
                            PersonMatchedAttribute.MiddleName,
                            PersonMatchedAttribute.LastName,
                            PersonMatchedAttribute.DateOfBirth,
                            PersonMatchedAttribute.EmailAddress,
                            PersonMatchedAttribute.Gender
                        ]
                    )).ToArray(),
                PersonId = matchedPersonIds[0],
                PersonAttributeSourcesSet = false
            });

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/trn-requests/{supportTask.SupportTaskReference}/resolve/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/trn-requests/{supportTask.SupportTaskReference}/resolve/merge?{journeyInstance.GetUniqueIdQueryParameter()}",
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

        TimeProvider.Advance(TimeSpan.FromDays(1));

        var comments = Faker.Lorem.Paragraph();
        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveTrnRequestState
            {
                MatchedPersons = [new MatchPersonsResultPerson(
                    matchedPerson.PersonId,
                    [
                        PersonMatchedAttribute.FirstName,
                        PersonMatchedAttribute.LastName,
                        PersonMatchedAttribute.DateOfBirth,
                        PersonMatchedAttribute.EmailAddress,
                        PersonMatchedAttribute.NationalInsuranceNumber,
                        PersonMatchedAttribute.Gender
                    ])],
                PersonId = CreateNewRecordPersonIdSentinel,
                Comments = comments
            });

        EventObserver.Clear();

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/trn-requests/{supportTask.SupportTaskReference}/resolve/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith("/support-tasks/trn-requests", response.Headers.Location?.OriginalString);

        var person = await WithDbContextAsync(dbContext => dbContext.Persons.SingleOrDefaultAsync(p => p.SourceTrnRequestId == requestData.RequestId));
        Assert.NotNull(person);
        Assert.Equal(requestData.FirstName, person.FirstName);
        Assert.Equal(requestData.MiddleName, person.MiddleName);
        Assert.Equal(requestData.LastName, person.LastName);
        Assert.Equal(requestData.DateOfBirth, person.DateOfBirth);
        Assert.Equal(requestData.EmailAddress, person.EmailAddress);
        Assert.Equal(requestData.NationalInsuranceNumber, person.NationalInsuranceNumber);
        Assert.Equal(requestData.Gender, person.Gender);

        var updatedSupportTask = await WithDbContextAsync(dbContext => dbContext
            .SupportTasks.Include(st => st.TrnRequestMetadata).SingleAsync(t => t.SupportTaskReference == supportTask.SupportTaskReference));
        Assert.Equal(SupportTaskStatus.Closed, updatedSupportTask.Status);
        Assert.Equal(SupportTaskOutcome.TrnRequest_ResolvedWithNewPerson, updatedSupportTask.Outcome);
        Assert.Equal(TimeProvider.UtcNow, updatedSupportTask.UpdatedOn);
        Assert.Equal(person.PersonId, updatedSupportTask.TrnRequestMetadata!.ResolvedPersonId);
        Assert.NotNull(updatedSupportTask.TrnRequestMetadata.TrnToken);
        var supportTaskData = updatedSupportTask.GetData<TrnRequestData>();
        AssertPersonAttributesMatchPerson(supportTaskData.ResolvedAttributes, person);
        Assert.Null(supportTaskData.SelectedPersonAttributes);

        EventObserver.AssertEventsSaved(@event =>
        {
            var apiTrnRequestSupportTaskUpdatedEvent = Assert.IsType<ApiTrnRequestSupportTaskUpdatedEvent>(@event);
            AssertEventIsExpected(apiTrnRequestSupportTaskUpdatedEvent, expectOldPersonAttributes: false, expectedPersonId: person.PersonId, comments);
        });

        Events.AssertProcessesCreated(p =>
        {
            Assert.Equal(ProcessType.TrnRequestResolving, p.ProcessContext.ProcessType);
            p.AssertProcessHasEvents<PersonCreatedEvent, TrnRequestUpdatedEvent, SupportTaskUpdatedEvent>();
        });

        var nextPage = await response.FollowRedirectAsync(HttpClient);
        var nextPageDoc = await nextPage.GetDocumentAsync();
        AssertEx.HtmlDocumentHasFlashNotificationBanner(
            nextPageDoc,
            $"Record created for {requestData.FirstName} {requestData.MiddleName} {requestData.LastName}");
        AssertBannerLinksToRecord(nextPageDoc, person.PersonId);
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

        TimeProvider.Advance(TimeSpan.FromDays(1));

        var comments = Faker.Lorem.Paragraph();
        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveTrnRequestState
            {
                MatchedPersons = [new MatchPersonsResultPerson(
                    matchedPerson.PersonId,
                    [
                        PersonMatchedAttribute.FirstName,
                        PersonMatchedAttribute.LastName,
                        PersonMatchedAttribute.DateOfBirth,
                        PersonMatchedAttribute.EmailAddress,
                        PersonMatchedAttribute.NationalInsuranceNumber,
                        PersonMatchedAttribute.Gender
                    ])],
                PersonId = matchedPerson.PersonId,
                PersonAttributeSourcesSet = true,
                MiddleNameSource = PersonAttributeSource.TrnRequest,
                Comments = comments
            });

        EventObserver.Clear();

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/trn-requests/{supportTask.SupportTaskReference}/resolve/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.StartsWith("/support-tasks/trn-requests", response.Headers.Location?.OriginalString);

        var person = await WithDbContextAsync(dbContext => dbContext.Persons.SingleAsync(p => p.PersonId == matchedPerson.PersonId));
        Assert.Equal(requestData.MiddleName, person.MiddleName);

        var updatedSupportTask = await WithDbContextAsync(dbContext => dbContext
            .SupportTasks.Include(st => st.TrnRequestMetadata).SingleAsync(t => t.SupportTaskReference == supportTask.SupportTaskReference));
        Assert.Equal(SupportTaskStatus.Closed, updatedSupportTask.Status);
        Assert.Equal(SupportTaskOutcome.TrnRequest_ResolvedWithExistingPerson, updatedSupportTask.Outcome);
        Assert.Equal(TimeProvider.UtcNow, updatedSupportTask.UpdatedOn);
        Assert.Equal(person.PersonId, updatedSupportTask.TrnRequestMetadata!.ResolvedPersonId);
        Assert.NotNull(updatedSupportTask.TrnRequestMetadata.TrnToken);
        var supportTaskData = updatedSupportTask.GetData<TrnRequestData>();
        AssertPersonAttributesMatchPerson(supportTaskData.ResolvedAttributes, person);
        AssertPersonAttributesMatchPerson(supportTaskData.SelectedPersonAttributes, matchedPerson.Person);

        EventObserver.AssertEventsSaved(@event =>
        {
            var apiTrnRequestSupportTaskUpdatedEvent = Assert.IsType<ApiTrnRequestSupportTaskUpdatedEvent>(@event);
            AssertEventIsExpected(apiTrnRequestSupportTaskUpdatedEvent, expectOldPersonAttributes: true, expectedPersonId: matchedPerson.PersonId, comments);
        });

        Events.AssertProcessesCreated(p =>
        {
            Assert.Equal(ProcessType.TrnRequestResolving, p.ProcessContext.ProcessType);
            p.AssertProcessHasEvents<PersonDetailsUpdatedEvent, TrnRequestUpdatedEvent, SupportTaskUpdatedEvent>();
        });

        var nextPage = await response.FollowRedirectAsync(HttpClient);
        var nextPageDoc = await nextPage.GetDocumentAsync();
        AssertEx.HtmlDocumentHasFlashNotificationBanner(
            nextPageDoc,
            $"Records merged for {requestData.FirstName} {requestData.MiddleName} {requestData.LastName}");
        AssertBannerLinksToRecord(nextPageDoc, matchedPerson.PersonId);
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

        TimeProvider.Advance(TimeSpan.FromDays(1));

        var comments = Faker.Lorem.Paragraph();
        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveTrnRequestState
            {
                MatchedPersons = [new MatchPersonsResultPerson(
                    matchedPerson.PersonId,
                    [
                        PersonMatchedAttribute.FirstName,
                        PersonMatchedAttribute.LastName,
                        PersonMatchedAttribute.DateOfBirth,
                        PersonMatchedAttribute.EmailAddress,
                        PersonMatchedAttribute.NationalInsuranceNumber,
                        PersonMatchedAttribute.Gender
                    ])],
                PersonId = matchedPerson.PersonId,
                PersonAttributeSourcesSet = true,
                MiddleNameSource = PersonAttributeSource.TrnRequest,
                Comments = comments
            });

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/trn-requests/{supportTask.SupportTaskReference}/resolve/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

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

        TimeProvider.Advance(TimeSpan.FromDays(1));

        var comments = Faker.Lorem.Paragraph();
        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveTrnRequestState
            {
                MatchedPersons = [new MatchPersonsResultPerson(
                    matchedPerson.PersonId,
                    [
                        PersonMatchedAttribute.FirstName,
                        PersonMatchedAttribute.LastName,
                        PersonMatchedAttribute.DateOfBirth,
                        PersonMatchedAttribute.EmailAddress,
                        PersonMatchedAttribute.NationalInsuranceNumber,
                        PersonMatchedAttribute.Gender
                    ])],
                PersonId = matchedPerson.PersonId,
                PersonAttributeSourcesSet = true,
                MiddleNameSource = PersonAttributeSource.TrnRequest,
                Comments = comments
            });

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/trn-requests/{supportTask.SupportTaskReference}/resolve/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

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

    private Task<JourneyInstance<ResolveTrnRequestState>> CreateJourneyInstance(
        string supportTaskReference,
        ResolveTrnRequestState state) =>
        CreateJourneyInstance(
            JourneyNames.ResolveTrnRequest,
            state,
            new KeyValuePair<string, object>("supportTaskReference", supportTaskReference));

    private static void SetPersonAttributeSourceToTrnRequest(ResolveTrnRequestState state, PersonMatchedAttribute attribute)
    {
        state.FirstNameSource = attribute is PersonMatchedAttribute.FirstName ? PersonAttributeSource.TrnRequest : PersonAttributeSource.ExistingRecord;
        state.MiddleNameSource = attribute is PersonMatchedAttribute.MiddleName ? PersonAttributeSource.TrnRequest : PersonAttributeSource.ExistingRecord;
        state.LastNameSource = attribute is PersonMatchedAttribute.LastName ? PersonAttributeSource.TrnRequest : PersonAttributeSource.ExistingRecord;
        state.DateOfBirthSource = attribute is PersonMatchedAttribute.DateOfBirth ? PersonAttributeSource.TrnRequest : PersonAttributeSource.ExistingRecord;
        state.EmailAddressSource = attribute is PersonMatchedAttribute.EmailAddress ? PersonAttributeSource.TrnRequest : PersonAttributeSource.ExistingRecord;
        state.NationalInsuranceNumberSource = attribute is PersonMatchedAttribute.NationalInsuranceNumber ? PersonAttributeSource.TrnRequest : PersonAttributeSource.ExistingRecord;
        state.GenderSource = attribute is PersonMatchedAttribute.Gender ? PersonAttributeSource.TrnRequest : PersonAttributeSource.ExistingRecord;
    }

    private static void AssertBannerLinksToRecord(IHtmlDocument doc, Guid expectedPersonId)
    {
        var banner = doc.GetElementsByClassName("govuk-notification-banner").Single();
        var viewRecordLink = banner.QuerySelector("a");
        Assert.NotNull(viewRecordLink);
        Assert.Contains(expectedPersonId.ToString(), viewRecordLink.GetAttribute("href"));
    }

    // An attribute with no source selected isn't written to the person, so the resolved attributes must
    // record the existing record's value rather than the request's.
    [Fact]
    public async Task Post_UpdatingExistingRecordWithNoSourceSelected_ResolvesAttributeToExistingRecord()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var (supportTask, matchedPerson) = await CreateSupportTaskWithSingleDifferenceToMatch(
            applicationUser.UserId,
            PersonMatchedAttribute.MiddleName);

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveTrnRequestState
            {
                MatchedPersons = [new MatchPersonsResultPerson(
                    matchedPerson.PersonId,
                    [
                        PersonMatchedAttribute.FirstName,
                        PersonMatchedAttribute.LastName,
                        PersonMatchedAttribute.DateOfBirth,
                        PersonMatchedAttribute.EmailAddress,
                        PersonMatchedAttribute.NationalInsuranceNumber,
                        PersonMatchedAttribute.Gender
                    ])],
                PersonId = matchedPerson.PersonId,
                PersonAttributeSourcesSet = true,
                MiddleNameSource = null
            });

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/trn-requests/{supportTask.SupportTaskReference}/resolve/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        var person = await WithDbContextAsync(dbContext => dbContext.Persons.SingleAsync(p => p.PersonId == matchedPerson.PersonId));
        Assert.Equal(matchedPerson.Person.MiddleName, person.MiddleName);

        var updatedSupportTask = await WithDbContextAsync(dbContext => dbContext
            .SupportTasks.SingleAsync(t => t.SupportTaskReference == supportTask.SupportTaskReference));
        var supportTaskData = updatedSupportTask.GetData<TrnRequestData>();
        AssertPersonAttributesMatchPerson(supportTaskData.ResolvedAttributes, person);
    }

    private void AssertPersonAttributesMatchPerson(
        TrnRequestDataPersonAttributes? personAttributes,
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
        Assert.Equal(TimeProvider.UtcNow, @event.CreatedUtc);
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
            value => ((DateOnly?)value)?.ToString(WebConstants.DateDisplayFormat)
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
