using AngleSharp.Html.Dom;
using TeachingRecordSystem.Core.DataStore.Postgres.Models;
using TeachingRecordSystem.Core.Events.Legacy;
using TeachingRecordSystem.Core.Models.SupportTasks;
using TeachingRecordSystem.Core.Services.GetAnIdentity.Api.Models;
using TeachingRecordSystem.SupportUi.Pages.SupportTasks.NpqTrnRequests.Resolve;
using Xunit.Sdk;
using static TeachingRecordSystem.SupportUi.Pages.SupportTasks.NpqTrnRequests.Resolve.ResolveNpqTrnRequestState;

namespace TeachingRecordSystem.SupportUi.Tests.PageTests.SupportTasks.NpqTrnRequests.Resolve;

public class CheckAnswersTests(HostFixture hostFixture) : NpqTrnRequestTestBase(hostFixture)
{
    [Before(Test)]
    public void ConfigureMocks() =>
        GetAnIdentityApiClientMock
            .Setup(mock => mock.CreateTrnTokenAsync(It.IsAny<CreateTrnTokenRequest>()))
            .ReturnsAsync((CreateTrnTokenRequest req) => new CreateTrnTokenResponse
            {
                Email = req.Email,
                ExpiresUtc = Clock.UtcNow.AddDays(1),
                Trn = req.Trn,
                TrnToken = Guid.NewGuid().ToString()
            });

    [Test]
    public async Task Get_NoPersonIdSelected_RedirectsToMatches()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveNpqTrnRequestState
            {
                MatchedPersonIds = supportTask.TrnRequestMetadata!.Matches!.MatchedPersons.Select(p => p.PersonId).AsReadOnly(),
                PersonId = null
            });

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/resolve/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);
    }

    [Test]
    public async Task Get_NoAttributesSourcesSet_RedirectsToMerge()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveNpqTrnRequestState
            {
                MatchedPersonIds = supportTask.TrnRequestMetadata!.Matches!.MatchedPersons.Select(p => p.PersonId).AsReadOnly(),
                PersonId = supportTask.TrnRequestMetadata!.Matches!.MatchedPersons.First().PersonId,
                PersonAttributeSourcesSet = false
            });

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/resolve/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/resolve/merge?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);
    }

    [Test]
    [MethodDataSource(nameof(GetPersonAttributeInfos))]
    public async Task Get_AttributeSourceIsTrnRequest_RendersChosenAttributeValues(PersonAttributeInfo sourcedFromRequestDataAttribute)
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var (supportTask, matchedPerson) = await CreateSupportTaskWithSingleDifferenceToMatch(
            applicationUser.UserId,
            sourcedFromRequestDataAttribute.Attribute);
        var requestData = supportTask.TrnRequestMetadata!;

        var state = new ResolveNpqTrnRequestState
        {
            MatchedPersonIds = supportTask.TrnRequestMetadata!.Matches!.MatchedPersons.Select(p => p.PersonId).AsReadOnly(),
            PersonId = matchedPerson.PersonId,
            PersonAttributeSourcesSet = true
        };
        SetPersonAttributeSourceStateToTrnRequest(state, sourcedFromRequestDataAttribute.Attribute);

        var journeyInstance = await CreateJourneyInstance(supportTask.SupportTaskReference, state);

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/resolve/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

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

    [Test]
    public async Task Get_CreatingNewRecord_DoesNotShowTrnRow()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveNpqTrnRequestState
            {
                MatchedPersonIds = supportTask.TrnRequestMetadata!.Matches!.MatchedPersons.Select(p => p.PersonId).AsReadOnly(),
                PersonId = CreateNewRecordPersonIdSentinel
            });

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/resolve/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Null(doc.GetSummaryListValueElementByKey("TRN"));
    }

    [Test]
    public async Task Get_UpdatingExistingRecord_DoesShowTrnRow()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveNpqTrnRequestState
            {
                MatchedPersonIds = supportTask.TrnRequestMetadata!.Matches!.MatchedPersons.Select(p => p.PersonId).AsReadOnly(),
                PersonId = supportTask.TrnRequestMetadata!.Matches!.MatchedPersons.First().PersonId,
                PersonAttributeSourcesSet = true
            });

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/resolve/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.NotNull(doc.GetSummaryListValueElementByKey("TRN"));
    }

    [Test]
    public async Task Get_ShowsComments()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId);

        var comments = Faker.Lorem.Paragraph();

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveNpqTrnRequestState
            {
                MatchedPersonIds = supportTask.TrnRequestMetadata!.Matches!.MatchedPersons.Select(p => p.PersonId).AsReadOnly(),
                PersonId = supportTask.TrnRequestMetadata!.Matches!.MatchedPersons.First().PersonId,
                PersonAttributeSourcesSet = true,
                Comments = comments
            });

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/resolve/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(comments, doc.GetSummaryListValueElementByKey("Comments")?.TrimmedText());
    }

    [Test]
    public async Task Get_UpdatingExistingRecord_HasBackAndChangeLinksToMergePage()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveNpqTrnRequestState
            {
                MatchedPersonIds = supportTask.TrnRequestMetadata!.Matches!.MatchedPersons.Select(p => p.PersonId).AsReadOnly(),
                PersonId = supportTask.TrnRequestMetadata!.Matches!.MatchedPersons.First().PersonId,
                PersonAttributeSourcesSet = true
            });

        var expectedBackLink = $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/resolve/merge?{journeyInstance.GetUniqueIdQueryParameter()}";
        var expectedChangeLink = $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/resolve/merge?fromCheckAnswers=True&{journeyInstance.GetUniqueIdQueryParameter()}";

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/resolve/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(expectedBackLink, doc.GetElementsByClassName("govuk-back-link").Single().GetAttribute("href"));
        Assert.Equal(expectedChangeLink, doc.GetElementByTestId("change-link")?.GetAttribute("href"));
    }

    [Test]
    public async Task Get_CreatingNewRecord_RequestHasMatches_HasBackLinkAndChangeLinkToMatchPage()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveNpqTrnRequestState
            {
                MatchedPersonIds = supportTask.TrnRequestMetadata!.Matches!.MatchedPersons.Select(p => p.PersonId).AsReadOnly(),
                PersonId = CreateNewRecordPersonIdSentinel,
                PersonAttributeSourcesSet = true
            });

        var expectedBackLink = $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}";
        var expectedChangeLink = $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/resolve/matches?fromCheckAnswers=True&{journeyInstance.GetUniqueIdQueryParameter()}";

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/resolve/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        var doc = await AssertEx.HtmlResponseAsync(response);
        Assert.Equal(expectedBackLink, doc.GetElementsByClassName("govuk-back-link").Single().GetAttribute("href"));
        Assert.Equal(expectedChangeLink, doc.GetElementByTestId("change-link")?.GetAttribute("href"));
    }

    [Test]
    public async Task Post_NoPersonIdSelected_RedirectsToMatches()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveNpqTrnRequestState
            {
                MatchedPersonIds = supportTask.TrnRequestMetadata!.Matches!.MatchedPersons.Select(p => p.PersonId).AsReadOnly(),
                PersonId = null
            });

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/resolve/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/resolve/matches?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);
    }

    [Test]
    public async Task Post_NoAttributesSourcesSet_RedirectsToMerge()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId);

        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveNpqTrnRequestState
            {
                MatchedPersonIds = supportTask.TrnRequestMetadata!.Matches!.MatchedPersons.Select(p => p.PersonId).AsReadOnly(),
                PersonId = supportTask.TrnRequestMetadata!.Matches!.MatchedPersons.First().PersonId,
                PersonAttributeSourcesSet = false
            });

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/resolve/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);
        Assert.Equal(
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/resolve/merge?{journeyInstance.GetUniqueIdQueryParameter()}",
            response.Headers.Location?.OriginalString);
    }

    [Test]
    [Arguments(PersonMatchedAttribute.EmailAddress)]
    [Arguments(PersonMatchedAttribute.DateOfBirth)]
    [Arguments(PersonMatchedAttribute.NationalInsuranceNumber)]
    [Arguments(PersonMatchedAttribute.Gender)]
    public async Task Post_UpdatingExistingRecord_UpdatesRecordUpdatesSupportTaskPublishesEventCompletesJourneyAndRedirects(PersonMatchedAttribute attribute)
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var (supportTask, matchedPerson) = await CreateSupportTaskWithSingleDifferenceToMatch(
            applicationUser.UserId,
            attribute);
        var requestData = supportTask.TrnRequestMetadata!;

        Clock.Advance();

        var comments = Faker.Lorem.Paragraph();
        var state = new ResolveNpqTrnRequestState
        {
            MatchedPersonIds = supportTask.TrnRequestMetadata!.Matches!.MatchedPersons.Select(p => p.PersonId).AsReadOnly(),
            PersonId = matchedPerson.PersonId,
            PersonAttributeSourcesSet = true,
            Comments = comments
        };

        if (attribute == PersonMatchedAttribute.DateOfBirth)
        {
            state.DateOfBirthSource = PersonAttributeSource.TrnRequest;
        }
        if (attribute == PersonMatchedAttribute.EmailAddress)
        {
            state.EmailAddressSource = PersonAttributeSource.TrnRequest;
        }
        if (attribute == PersonMatchedAttribute.NationalInsuranceNumber)
        {
            state.NationalInsuranceNumberSource = PersonAttributeSource.TrnRequest;
        }
        if (attribute == PersonMatchedAttribute.Gender)
        {
            state.GenderSource = PersonAttributeSource.TrnRequest;
        }

        var journeyInstance = await CreateJourneyInstance(supportTask.SupportTaskReference, state);

        EventObserver.Clear();

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/resolve/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        // redirect
        Assert.Equal("/support-tasks/npq-trn-requests", response.Headers.Location?.OriginalString);

        // person record is updated
        await WithDbContextAsync(async dbContext =>
        {
            var updatedPersonRecord = await dbContext.Persons
                .SingleAsync(p => p.PersonId == matchedPerson.PersonId);

            if (attribute == PersonMatchedAttribute.DateOfBirth)
            {
                Assert.Equal(requestData.DateOfBirth, updatedPersonRecord.DateOfBirth);
            }
            if (attribute == PersonMatchedAttribute.EmailAddress)
            {
                Assert.Equal(requestData.EmailAddress, updatedPersonRecord.EmailAddress);
            }
            if (attribute == PersonMatchedAttribute.NationalInsuranceNumber)
            {
                Assert.Equal(requestData.NationalInsuranceNumber, updatedPersonRecord.NationalInsuranceNumber);
            }
            if (attribute == PersonMatchedAttribute.Gender)
            {
                Assert.Equal(requestData.Gender, updatedPersonRecord.Gender);
            }
        });

        // support task is updated
        await WithDbContextAsync(async dbContext =>
        {
            var updatedSupportTask = await dbContext
                .SupportTasks
                .Include(st => st.TrnRequestMetadata)
                .SingleAsync(t => t.SupportTaskReference == supportTask.SupportTaskReference);
            Assert.Equal(SupportTaskStatus.Closed, updatedSupportTask.Status);
            Assert.Equal(Clock.UtcNow, updatedSupportTask.UpdatedOn);
            Assert.Equal(matchedPerson.PersonId, updatedSupportTask.TrnRequestMetadata!.ResolvedPersonId);
            var supportTaskData = updatedSupportTask.GetData<NpqTrnRequestData>();
            Assert.Equal(SupportRequestOutcome.Approved, supportTaskData.SupportRequestOutcome);
            AssertPersonAttributesMatch(supportTaskData.SelectedPersonAttributes, matchedPerson.Person);

            var dateOfBirth = attribute == PersonMatchedAttribute.DateOfBirth ? requestData.DateOfBirth : matchedPerson.DateOfBirth;
            var emailAddress = attribute == PersonMatchedAttribute.EmailAddress ? requestData.EmailAddress : matchedPerson.EmailAddress;
            var nationalInsuranceNumber = attribute == PersonMatchedAttribute.NationalInsuranceNumber ? requestData.NationalInsuranceNumber : matchedPerson.NationalInsuranceNumber;
            var gender = attribute == PersonMatchedAttribute.Gender ? requestData.Gender : matchedPerson.Gender;

            AssertPersonAttributesMatch(supportTaskData.ResolvedAttributes, new NpqTrnRequestDataPersonAttributes
            {
                FirstName = matchedPerson.FirstName,
                MiddleName = matchedPerson.MiddleName,
                LastName = matchedPerson.LastName,
                DateOfBirth = dateOfBirth,
                EmailAddress = emailAddress,
                NationalInsuranceNumber = nationalInsuranceNumber,
                Gender = gender
            });
        });

        // event is published
        var expectedMetadata = EventModels.TrnRequestMetadata.FromModel(supportTask.TrnRequestMetadata!) with
        {
            ResolvedPersonId = matchedPerson.PersonId
        };
        EventObserver.AssertEventsSaved(e =>
        {
            var actualEvent = Assert.IsType<NpqTrnRequestSupportTaskResolvedEvent>(e);
            AssertSupportTaskUpdatedEventIsExpected(actualEvent, expectOldPersonAttributes: true, expectedPersonId: matchedPerson.PersonId, comments);
            Assert.Equal("Records merged - identified as same person during task resolution", actualEvent.ChangeReason.GetDisplayName());
            AssertTrnRequestMetadataMatches(expectedMetadata, actualEvent.RequestData);
            Assert.Equal(supportTask.TrnRequestMetadata!.NpqEvidenceFileId, actualEvent.RequestData?.NpqEvidenceFileId);
            Assert.Equal(supportTask.TrnRequestMetadata!.NpqEvidenceFileName, actualEvent.RequestData?.NpqEvidenceFileName);
        });

        var nextPage = await response.FollowRedirectAsync(HttpClient);
        var nextPageDoc = await nextPage.GetDocumentAsync();
        AssertEx.HtmlDocumentHasFlashSuccess(
            nextPageDoc,
            $"TRN request for {StringHelper.JoinNonEmpty(' ', matchedPerson.FirstName, matchedPerson.MiddleName, matchedPerson.LastName)} completed");

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.True(journeyInstance.Completed);
    }

    [Test]
    [MethodDataSource(nameof(GetPersonAttributeInfos))]
    public async Task Post_UpdatingExistingRecord_OnlyUpdatesAttributesSourcedFromRequestData(PersonAttributeInfo attributeSourcedFromRequestData)
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var (supportTask, matchedPerson) = await CreateSupportTaskWithSingleDifferenceToMatch(
            applicationUser.UserId,
            attributeSourcedFromRequestData.Attribute);
        var requestData = supportTask.TrnRequestMetadata!;

        var state = new ResolveNpqTrnRequestState
        {
            MatchedPersonIds = supportTask.TrnRequestMetadata!.Matches!.MatchedPersons.Select(p => p.PersonId).AsReadOnly(),
            PersonId = matchedPerson.PersonId,
            PersonAttributeSourcesSet = true
        };
        SetPersonAttributeSourceStateToTrnRequest(state, attributeSourcedFromRequestData.Attribute);

        var journeyInstance = await CreateJourneyInstance(supportTask.SupportTaskReference, state);

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/resolve/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        await WithDbContextAsync(async dbContext =>
        {
            var updatedPersonRecord = await dbContext.Persons
                .SingleAsync(p => p.PersonId == matchedPerson.PersonId);
            Assert.Equal(attributeSourcedFromRequestData.Attribute is PersonMatchedAttribute.DateOfBirth ? supportTask.TrnRequestMetadata!.DateOfBirth : matchedPerson.DateOfBirth, updatedPersonRecord.DateOfBirth);
            Assert.Equal(attributeSourcedFromRequestData.Attribute is PersonMatchedAttribute.EmailAddress ? supportTask.TrnRequestMetadata!.EmailAddress : matchedPerson.EmailAddress, updatedPersonRecord.EmailAddress);
            Assert.Equal(attributeSourcedFromRequestData.Attribute is PersonMatchedAttribute.NationalInsuranceNumber ? supportTask.TrnRequestMetadata!.NationalInsuranceNumber : matchedPerson.NationalInsuranceNumber, updatedPersonRecord.NationalInsuranceNumber);
            Assert.Equal(attributeSourcedFromRequestData.Attribute is PersonMatchedAttribute.Gender ? supportTask.TrnRequestMetadata!.Gender : matchedPerson.Gender, updatedPersonRecord.Gender);
        });
    }

    [Test]
    [MethodDataSource(nameof(GetPersonAttributeInfos))]
    public async Task Post_UpdatingExistingRecord_UpdatesSupportTask(PersonAttributeInfo attributeSourcedFromRequestData)
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var (supportTask, matchedPerson) = await CreateSupportTaskWithSingleDifferenceToMatch(
            applicationUser.UserId,
            attributeSourcedFromRequestData.Attribute);
        var requestData = supportTask.TrnRequestMetadata!;

        var state = new ResolveNpqTrnRequestState
        {
            MatchedPersonIds = supportTask.TrnRequestMetadata!.Matches!.MatchedPersons.Select(p => p.PersonId).AsReadOnly(),
            PersonId = matchedPerson.PersonId,
            PersonAttributeSourcesSet = true
        };
        SetPersonAttributeSourceStateToTrnRequest(state, attributeSourcedFromRequestData.Attribute);

        var journeyInstance = await CreateJourneyInstance(supportTask.SupportTaskReference, state);

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/resolve/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert
        Assert.Equal(StatusCodes.Status302Found, (int)response.StatusCode);

        // support task is updated
        await WithDbContextAsync(async dbContext =>
        {
            var updatedSupportTask = await dbContext
                .SupportTasks
                .Include(st => st.TrnRequestMetadata)
                .SingleAsync(t => t.SupportTaskReference == supportTask.SupportTaskReference);
            Assert.Equal(SupportTaskStatus.Closed, updatedSupportTask.Status);
            Assert.Equal(Clock.UtcNow, updatedSupportTask.UpdatedOn);
            var supportTaskData = updatedSupportTask.GetData<NpqTrnRequestData>();
            Assert.Equal(SupportRequestOutcome.Approved, supportTaskData.SupportRequestOutcome);
            AssertPersonAttributesMatch(supportTaskData.SelectedPersonAttributes, matchedPerson.Person);
            AssertPersonAttributesMatch(supportTaskData.ResolvedAttributes, new NpqTrnRequestDataPersonAttributes
            {
                FirstName = matchedPerson.FirstName,
                MiddleName = matchedPerson.MiddleName,
                LastName = matchedPerson.LastName,
                DateOfBirth = attributeSourcedFromRequestData.Attribute is PersonMatchedAttribute.DateOfBirth ? supportTask.TrnRequestMetadata!.DateOfBirth! : matchedPerson.DateOfBirth,
                EmailAddress = attributeSourcedFromRequestData.Attribute is PersonMatchedAttribute.EmailAddress ? supportTask.TrnRequestMetadata!.EmailAddress! : matchedPerson.EmailAddress,
                NationalInsuranceNumber = attributeSourcedFromRequestData.Attribute is PersonMatchedAttribute.NationalInsuranceNumber ? supportTask.TrnRequestMetadata!.NationalInsuranceNumber! : matchedPerson.NationalInsuranceNumber,
                Gender = attributeSourcedFromRequestData.Attribute is PersonMatchedAttribute.Gender ? supportTask.TrnRequestMetadata!.Gender : matchedPerson.Gender
            });
        });
    }

    [Test]
    public async Task Post_CreatingNewRecord_CreatesRecordUpdatesSupportTaskPublishesEventCompletesJourneyAndRedirects()
    {
        // Arrange
        var applicationUser = await TestData.CreateApplicationUserAsync();

        var supportTask = await TestData.CreateNpqTrnRequestSupportTaskAsync(applicationUser.UserId, s =>
        {
            s.WithMiddleName(TestData.GenerateMiddleName());
            s.WithEmailAddress(TestData.GenerateUniqueEmail());
            s.WithNationalInsuranceNumber(TestData.GenerateNationalInsuranceNumber());
            s.WithGender(TestData.GenerateGender());
        });
        var requestMetadata = supportTask.TrnRequestMetadata;
        Assert.NotNull(requestMetadata);
        var comments = Faker.Lorem.Paragraph();
        var journeyInstance = await CreateJourneyInstance(
            supportTask.SupportTaskReference,
            new ResolveNpqTrnRequestState
            {
                MatchedPersonIds = supportTask.TrnRequestMetadata!.Matches!.MatchedPersons.Select(p => p.PersonId).AsReadOnly(),
                PersonId = CreateNewRecordPersonIdSentinel,
                PersonAttributeSourcesSet = true,
                Comments = comments
            });

        EventObserver.Clear();

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/support-tasks/npq-trn-requests/{supportTask.SupportTaskReference}/resolve/check-answers?{journeyInstance.GetUniqueIdQueryParameter()}");

        // Act
        var response = await HttpClient.SendAsync(request);

        // Assert

        // redirect
        Assert.Equal("/support-tasks/npq-trn-requests", response.Headers.Location?.OriginalString);

        var nextPage = await response.FollowRedirectAsync(HttpClient);
        var nextPageDoc = await nextPage.GetDocumentAsync();
        var linkToPersonRecord = GetLinkToPersonFromBanner(nextPageDoc);
        Assert.NotNull(linkToPersonRecord);
        var personId = Guid.Parse(linkToPersonRecord.AsSpan("/persons/".Length));

        // person record is updated
        await WithDbContextAsync(async dbContext =>
        {
            var person = await dbContext.Persons
                .SingleAsync(p => p.PersonId == personId);
            Assert.Equal(person.FirstName, requestMetadata.FirstName);
            Assert.Equal(person.MiddleName, requestMetadata.MiddleName);
            Assert.Equal(person.LastName, requestMetadata.LastName);
            Assert.Equal(person.DateOfBirth, requestMetadata.DateOfBirth);
            Assert.Equal(person.EmailAddress, requestMetadata.EmailAddress);
            Assert.Equal(person.NationalInsuranceNumber, requestMetadata.NationalInsuranceNumber);
            Assert.Equal(person.Gender, requestMetadata.Gender);
        });

        // support task is updated
        await WithDbContextAsync(async dbContext =>
        {
            var updatedSupportTask = await dbContext
                .SupportTasks
                .Include(st => st.TrnRequestMetadata)
                .SingleAsync(t => t.SupportTaskReference == supportTask.SupportTaskReference);
            Assert.Equal(SupportTaskStatus.Closed, updatedSupportTask.Status);
            Assert.Equal(Clock.UtcNow, updatedSupportTask.UpdatedOn);
            Assert.Equal(personId, updatedSupportTask.TrnRequestMetadata!.ResolvedPersonId);
            var supportTaskData = updatedSupportTask.GetData<NpqTrnRequestData>();
            Assert.Equal(SupportRequestOutcome.Approved, supportTaskData.SupportRequestOutcome);
            AssertPersonAttributesMatch(supportTaskData.ResolvedAttributes, new NpqTrnRequestDataPersonAttributes
            {
                FirstName = requestMetadata.FirstName!,
                MiddleName = requestMetadata.MiddleName ?? string.Empty,
                LastName = requestMetadata.LastName!,
                DateOfBirth = requestMetadata.DateOfBirth,
                EmailAddress = requestMetadata.EmailAddress,
                NationalInsuranceNumber = requestMetadata.NationalInsuranceNumber,
                Gender = requestMetadata.Gender
            });
        });

        // event is published
        var expectedMetadata = EventModels.TrnRequestMetadata.FromModel(requestMetadata) with
        {
            ResolvedPersonId = personId
        };
        EventObserver.AssertEventsSaved(e =>
        {
            var actualEvent = Assert.IsType<NpqTrnRequestSupportTaskResolvedEvent>(e);
            AssertSupportTaskUpdatedEventIsExpected(actualEvent, expectedPersonId: personId, expectOldPersonAttributes: false, comments: comments);
            Assert.Equal("Record created - no existing person identified during task resolution", actualEvent.ChangeReason.GetDisplayName());
            AssertTrnRequestMetadataMatches(expectedMetadata, actualEvent.RequestData);
            Assert.Equal(requestMetadata.NpqEvidenceFileId, actualEvent.RequestData.NpqEvidenceFileId);
            Assert.Equal(requestMetadata.NpqEvidenceFileName, actualEvent.RequestData.NpqEvidenceFileName);
        });

        journeyInstance = await ReloadJourneyInstance(journeyInstance);
        Assert.True(journeyInstance.Completed);
    }

    private string? GetLinkToPersonFromBanner(IHtmlDocument doc, string? expectedHeading = null, string? expectedMessage = null)
    {
        var banner = doc.GetElementsByClassName("govuk-notification-banner--success").SingleOrDefault();

        if (banner is null)
        {
            throw new XunitException("No notification banner found.");
        }
        var link = banner.QuerySelector(".govuk-link");

        var href = link?.GetAttribute("href");
        return href;
    }

    private Task<JourneyInstance<ResolveNpqTrnRequestState>> CreateJourneyInstance(
        string supportTaskReference,
        ResolveNpqTrnRequestState state) =>
        CreateJourneyInstance(
            JourneyNames.ResolveNpqTrnRequest,
            state,
            new KeyValuePair<string, object>("supportTaskReference", supportTaskReference));

    private static void SetPersonAttributeSourceStateToTrnRequest(ResolveNpqTrnRequestState state, PersonMatchedAttribute attribute)
    {
        state.DateOfBirthSource = attribute is PersonMatchedAttribute.DateOfBirth ? PersonAttributeSource.TrnRequest : PersonAttributeSource.ExistingRecord;
        state.EmailAddressSource = attribute is PersonMatchedAttribute.EmailAddress ? PersonAttributeSource.TrnRequest : PersonAttributeSource.ExistingRecord;
        state.NationalInsuranceNumberSource = attribute is PersonMatchedAttribute.NationalInsuranceNumber ? PersonAttributeSource.TrnRequest : PersonAttributeSource.ExistingRecord;
        state.GenderSource = attribute is PersonMatchedAttribute.Gender ? PersonAttributeSource.TrnRequest : PersonAttributeSource.ExistingRecord;
    }

    private void AssertPersonAttributesMatch(
        NpqTrnRequestDataPersonAttributes? personAttributes,
        Person person)
    {
        Assert.NotNull(personAttributes);
        Assert.Equal(personAttributes.DateOfBirth, person.DateOfBirth);
        Assert.Equal(personAttributes.EmailAddress, person.EmailAddress);
        Assert.Equal(personAttributes.NationalInsuranceNumber, person.NationalInsuranceNumber);
        Assert.Equal(personAttributes.Gender, person.Gender);
    }

    private void AssertSupportTaskUpdatedEventIsExpected(
        NpqTrnRequestSupportTaskResolvedEvent @event,
        bool expectOldPersonAttributes,
        Guid expectedPersonId,
        string? comments)
    {
        Assert.Equal(expectedPersonId, @event.PersonId);
        Assert.Equal(Clock.UtcNow, @event.CreatedUtc);
        Assert.True(@event.Changes.HasFlag(NpqTrnRequestSupportTaskResolvedEventChanges.Status));

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

    private void AssertPersonAttributesMatch(
        NpqTrnRequestDataPersonAttributes? personAttributes,
        NpqTrnRequestDataPersonAttributes expectedPersonAttributes)
    {
        Assert.NotNull(personAttributes);
        Assert.Equal(personAttributes.DateOfBirth, expectedPersonAttributes.DateOfBirth);
        Assert.Equal(personAttributes.EmailAddress, expectedPersonAttributes.EmailAddress);
        Assert.Equal(personAttributes.NationalInsuranceNumber, expectedPersonAttributes.NationalInsuranceNumber);
        Assert.Equal(personAttributes.Gender, expectedPersonAttributes.Gender);
    }

    private void AssertTrnRequestMetadataMatches(EventModels.TrnRequestMetadata expected, EventModels.TrnRequestMetadata actual)
    {
        Assert.Equal(expected.FirstName, actual.FirstName);
        Assert.Equal(expected.MiddleName, actual.MiddleName);
        Assert.Equal(expected.LastName, actual.LastName);
        Assert.Equal(expected.DateOfBirth, actual.DateOfBirth);
        Assert.Equal(expected.EmailAddress, actual.EmailAddress);
        Assert.Equal(expected.NationalInsuranceNumber, actual.NationalInsuranceNumber);
        Assert.Equal(expected.Gender, actual.Gender);
        Assert.Equal(expected.ResolvedPersonId, actual.ResolvedPersonId);
        Assert.Equivalent(expected.Matches, actual.Matches);
    }

    public static PersonAttributeInfo[] GetPersonAttributeInfos() =>
    [
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
